using ATS.Application.Abstract.Services;
using ATS.Application.Reporting;
using ATS.Application.Results;
using ATS.Application.Security;
using ATS.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATS.FromUI
{
    public partial class Form1 : Form
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<Form1> _logger;
        private string? _uploadedFilePath;
        private string? _uploadedFileName;
        private string? _uploadedFileType;
        private string? _lastReportPath;
        private CvScan? _selectedScan;
        private CancellationTokenSource? _analysisCts;
        public Form1(IServiceScopeFactory scopeFactory, IReportService reportService, ILogger<Form1> logger)
        {
            InitializeComponent();
            _scopeFactory = scopeFactory;
            _logger = logger;

            // Form kapanırken süren analiz iptal edilir.
            //FormClosing += Form1_FormClosing;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            ConfigureDataGridView();
            SetInitialControlStates();
            await LoadAnalyzeHistoryAsync();
        }

        private void ConfigureDataGridView()
        {
            dtgAllAnalyze.AutoGenerateColumns = false;
            dtgAllAnalyze.ReadOnly = true;
            dtgAllAnalyze.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgAllAnalyze.MultiSelect = false;
            dtgAllAnalyze.AllowUserToAddRows = false;
            dtgAllAnalyze.RowHeadersVisible = false;

            dtgAllAnalyze.Columns.Clear();

            dtgAllAnalyze.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Width = 40
            });
            dtgAllAnalyze.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Name",
                DataPropertyName = "CandidateName",
                Width = 130
            });
            dtgAllAnalyze.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colScore",
                HeaderText = "Score",
                DataPropertyName = "OverallScore",
                Width = 50
            });
            dtgAllAnalyze.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "Type",
                DataPropertyName = "AnalysisType",
                Width = 85
            });
            dtgAllAnalyze.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "Date",
                DataPropertyName = "CreatedAt",
                Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle
                { Format = "dd.MM.yyyy HH:mm" }
            });
        }

        private async Task LoadAnalyzeHistoryAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cvScanService = scope.ServiceProvider.GetRequiredService<ICvScanService>();

                var scans = (await cvScanService.GetAllScansAsync()).OrderByDescending(s => s.CreatedAt).ToList();
                var rows = scans.Select(s => new
                {
                    s.Id,
                    CandidateName = !string.IsNullOrEmpty(s.FilePath)
                                    ? Path.GetFileName(s.FilePath)
                                    : "No File Name",
                    s.OverallScore,
                    AnalysisType = s.IsJobMatched ? "Job Match" : " Gen. ATS Score",
                    CreatedAt = s.CreatedAt.ToLocalTime()
                }).ToList();
                dtgAllAnalyze.DataSource = rows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "History load failed");
                SetStatusMessage("⚠️ Failed to load history: " + ex.Message, Color.OrangeRed);
            }
        }

        private void btnUploadCv_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select CV Folder",
                Filter = "Supported File Types: (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt",
                Multiselect = false
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            var fi = new FileInfo(dlg.FileName);
            if (fi.Length > 10 * 1024 * 1024)
            {
                MessageBox.Show("File size exceeds 10 MB. Please select a smaller file.", "Fil Size Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _uploadedFilePath = dlg.FileName;
            _uploadedFileName = fi.Name;
            _uploadedFileType = fi.Extension.TrimStart('.').ToLower();
            _lastReportPath = null;

            SetStatusMessage($"✅ File uploaded: {fi.Name}", Color.ForestGreen);
            btnAnalyze.Enabled = true;

            _logger.LogInformation("CV uploaded: {Path}", _uploadedFilePath);
        }

        private async void btnAnalyze_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_uploadedFilePath) || string.IsNullOrWhiteSpace(_uploadedFileType))
            {
                MessageBox.Show("Please upload a CV file first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _analysisCts?.Dispose();
            _analysisCts = new CancellationTokenSource();
            var cancellationToken = _analysisCts.Token;

            SetBusyState(true);
            ResetResultArea();

            try
            {
                await SimulateProgressAsync(0, 30, 300);

                string jobRequirementsText = txtJobRequitments.Text.Trim();
                bool isJobMatch = !string.IsNullOrWhiteSpace(jobRequirementsText);

                await SimulateProgressAsync(30, 60, 200);

                AnalysisResult analysisResult;
                CvScan scan;

                // Her işlem kendi DI kapsamında çalışır; DbContext işlemler arasında paylaşılmaz.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var cvScanService = scope.ServiceProvider.GetRequiredService<ICvScanService>();

                    if (isJobMatch)
                    {
                        string finalJobTitle = !string.IsNullOrWhiteSpace(txtJobTitle.Text)
                               ? txtJobTitle.Text.Trim()
                               : ExtractJobTitle(jobRequirementsText);

                        analysisResult = await cvScanService.AnalyzeWithJobPostingAsync(
                            _uploadedFilePath, _uploadedFileType, jobRequirementsText, finalJobTitle, cancellationToken);
                    }
                    else
                    {
                        analysisResult = await cvScanService.AnalyzeAsync(_uploadedFilePath, _uploadedFileType, cancellationToken);
                    }

                    // Kapsam kapanınca ilişkiler yüklenemez; raporun okuyacağı her şey burada önceden yüklenir.
                    scan = await cvScanService.GetScanWithDetailsAsync(analysisResult.Scan.Id) ?? analysisResult.Scan;
                }

                await SimulateProgressAsync(60, 90, 200);

                _selectedScan = scan;
                _lastReportPath = scan.ScoreReports?.FirstOrDefault()?.ReportPath;

                lblMaxScore.Text = "100";
                lblScore.Text = scan.OverallScore.ToString();
                lblName.Text = !string.IsNullOrWhiteSpace(scan.CandidateName) ? scan.CandidateName : "Unknown Candidate";
                txtFeedback.Text = BuildFeedbackText(scan);

                await SimulateProgressAsync(90, 100, 150);

                bool hasWarnings = analysisResult.Warnings.Count > 0;
                if (hasWarnings)
                    SetStatusMessage($"⚠️ Analysis complete with warnings — Score: {scan.OverallScore}/100", Color.DarkOrange);
                else
                    SetStatusMessage($"✅ Analysis complete — Score: {scan.OverallScore}/100", Color.ForestGreen);

                btnPdfReport.Enabled = true;
                await LoadAnalyzeHistoryAsync();

                _logger.LogInformation("Analysis done. Mode={Mode}, CvScanId={Id}, Score={Score}", isJobMatch ? "JobMatch" : "General", scan.Id, scan.OverallScore);

                var securityMessage = SecurityFindingMessageFormatter.Format(analysisResult.SecurityFindings);
                if (securityMessage != null)
                    MessageBox.Show(securityMessage, "Security findings", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (hasWarnings)
                {
                    var warningText = string.Join(Environment.NewLine, analysisResult.Warnings.Select(warning => "• " + warning));
                    MessageBox.Show(warningText, "Analysis warnings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Analysis cancelled");
                SetStatusMessage("Analysis cancelled.", Color.Gray);
                progressAnalyzeTime.Value = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed");
                MessageBox.Show(ex.Message, "Analysis failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                SetStatusMessage("❌ Analysis failed: " + ex.Message, Color.Crimson);
                progressAnalyzeTime.Value = 0;
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private void btnNewAnalyze_Click(object sender, EventArgs e)
        {
            ResetForm();
            btnUploadCv_Click(sender, e);
        }

        private void btnPdfReport_Click(object sender, EventArgs e)
        {
            CvScan? target = _selectedScan;
            if (target == null)
            {
                MessageBox.Show("No analysis found to generate PDF.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                string reportPath = target.IsJobMatched
                    ? reportService.GenerateJobMatchReport(target)
                    : reportService.GenerateGeneralReport(target);

                _lastReportPath = reportPath;
                SetStatusMessage($"📄 PDF successfully generated: {Path.GetFileName(reportPath)}", Color.SteelBlue);

                if (File.Exists(reportPath))
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(reportPath)
                        { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF generation failed");
                MessageBox.Show("Failed to generate PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnClean_Click(object sender, EventArgs e)
        {
            ResetForm();
            await LoadAnalyzeHistoryAsync();
            SetStatusMessage("🔄 Form Cleaned.", Color.Gray);
        }

        private async void dtgAllAnalyze_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                var row = dtgAllAnalyze.Rows[e.RowIndex];
                int scanId = Convert.ToInt32(row.Cells["colId"].Value);

                CvScan? scan;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var cvScanService = scope.ServiceProvider.GetRequiredService<ICvScanService>();
                    scan = await cvScanService.GetScanWithDetailsAsync(scanId);
                }

                if (scan == null) return;

                _selectedScan = scan;
                _lastReportPath = scan.ScoreReports?.FirstOrDefault()?.ReportPath;

                lblMaxScore.Text = "100";
                lblScore.Text = scan.OverallScore.ToString();
                lblName.Text = scan.CandidateName;
                txtFeedback.Text = BuildFeedbackText(scan);

                txtJobRequitments.Text = scan.JobPosting?.RawText ?? string.Empty;
                txtJobTitle.Text = scan.JobPosting?.Title ?? string.Empty;
                progressAnalyzeTime.Value = Math.Min(scan.OverallScore, 100);
                btnPdfReport.Enabled = true;

                SetStatusMessage($"📋 History analysis loaded — {scan.CandidateName} | " + $"Score: {scan.OverallScore}/100", Color.SteelBlue);

                _logger.LogInformation("History row loaded. CvScanId={Id}", scanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Row load failed");
                SetStatusMessage("⚠️ Failed to load record: " + ex.Message, Color.OrangeRed);
            }
        }

        private static string BuildFeedbackText(CvScan scan)
        {
            if (scan.SectionScores == null || !scan.SectionScores.Any())
                return "Detailed feedback not available.";

            var sb = new System.Text.StringBuilder();

            sb.AppendLine("══════════════════════════════════");
            sb.AppendLine($"  TOTAL SCORE: {scan.OverallScore}/100");
            sb.AppendLine($"  Analysis Type: {(scan.IsJobMatched ? "Job Match" : "Gen ATS Analysis")}");
            if (scan.IsJobMatched)
                sb.AppendLine($"  {OverallScoreExplanation.Build()}");
            sb.AppendLine("══════════════════════════════════");
            sb.AppendLine();

            foreach (var section in scan.SectionScores.OrderBy(s => s.SectionName))
            {
                string status = section.IsPassed ? "✅" : "❌";
                sb.AppendLine($"{status} {section.SectionName,-25} {section.Score,3}/{section.MaxScore}");

                if (!string.IsNullOrWhiteSpace(section.Feedback))
                {
                    foreach (var line in section.Feedback.Split('|', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("MISSING:", StringComparison.OrdinalIgnoreCase))
                            sb.AppendLine($"   ⚠️  Missing: {trimmed[8..].Trim()}");
                        else
                            sb.AppendLine($"   • {trimmed}");
                    }
                }
                sb.AppendLine();
            }

            // Job Match puanı doğrudan bölüm skorundan okunur.
            var jobMatchSection = scan.SectionScores.FirstOrDefault(s => s.SectionName == OverallScoreCalculator.JobMatchSectionName);
            if (scan.IsJobMatched && jobMatchSection != null)
            {
                sb.AppendLine($"🎯 Job Match Score: {jobMatchSection.Score}/{jobMatchSection.MaxScore}");
                if (!string.IsNullOrWhiteSpace(scan.JobPosting?.Title))
                    sb.AppendLine($"   Position: {scan.JobPosting!.Title}");
                sb.AppendLine();
            }

            var requirementReportLines = RequirementReportFormatter.FormatLines(RequirementReportBuilder.Build(scan));
            if (requirementReportLines.Count > 0)
            {
                foreach (var requirementReportLine in requirementReportLines)
                    sb.AppendLine(requirementReportLine);
                sb.AppendLine();
            }

            var hiddenTextLines = SecurityReportLineBuilder.BuildHiddenTextLines(scan.SecurityFindings);
            if (hiddenTextLines.Count > 0)
            {
                sb.AppendLine($" {SecurityReportTexts.HiddenTextSectionTitle}");
                sb.AppendLine($"   {SecurityReportTexts.HiddenTextSectionNote}");
                foreach (var hiddenTextLine in hiddenTextLines)
                    sb.AppendLine($"   • [{hiddenTextLine.Severity}] {hiddenTextLine.Text}");
                sb.AppendLine();
            }

            var securityLines = SecurityReportLineBuilder.BuildFindingLines(scan.SecurityFindings);
            if (securityLines.Count > 0)
            {
                sb.AppendLine($"️ {SecurityReportTexts.SecuritySectionTitle}");
                sb.AppendLine($"   {SecurityReportTexts.SecuritySectionNote}");
                foreach (var securityLine in securityLines)
                    sb.AppendLine($"   • [{securityLine.Severity}] {securityLine.Text}");
                sb.AppendLine();
            }

            var warningLines = SecurityReportLineBuilder.BuildWarningLines(scan.AnalysisWarnings).ToList();
            if (warningLines.Count > 0)
            {
                sb.AppendLine("⚠️ Analysis warnings");
                foreach (var warningLine in warningLines)
                    sb.AppendLine($"   • {warningLine}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string ExtractJobTitle(string jobText)
        {
            var firstLine = jobText.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
            return firstLine?.Trim().TruncateSafe(100) ?? "Unnamed Position";
        }

        private async Task SimulateProgressAsync(int from, int to, int durationMs)
        {
            int steps = to - from;
            int delayEach = steps > 0 ? durationMs / steps : durationMs;

            for (int i = from; i <= to; i++)
            {
                progressAnalyzeTime.Value = i;
                if (delayEach > 0)
                    await Task.Delay(delayEach);
            }
        }

        private void SetBusyState(bool busy)
        {
            btnUploadCv.Enabled = !busy;
            btnAnalyze.Enabled = !busy;
            btnNewAnalyze.Enabled = !busy;
            btnClean.Enabled = !busy;
            // Analiz hata verirse ya da iptal edilirse eski analizin raporu üretilmesin.
            btnPdfReport.Enabled = !busy && _selectedScan != null;
            txtJobRequitments.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void SetInitialControlStates()
        {
            btnAnalyze.Enabled = false;
            btnPdfReport.Enabled = false;
            progressAnalyzeTime.Value = 0;
            progressAnalyzeTime.Maximum = 100;
            progressAnalyzeTime.Minimum = 0;
        }

        private void ResetResultArea()
        {
            lblMaxScore.Text = "—";
            lblScore.Text = "—";
            lblName.Text = "—";
            txtFeedback.Text = string.Empty;
            progressAnalyzeTime.Value = 0;
            btnPdfReport.Enabled = false;
            _selectedScan = null;
            _lastReportPath = null;
        }

        private void ResetForm()
        {
            ResetResultArea();
            txtJobRequitments.Text = string.Empty;
            txtJobTitle.Text = string.Empty;
            _uploadedFilePath = null;
            _uploadedFileName = null;
            _uploadedFileType = null;
            btnAnalyze.Enabled = false;
            SetStatusMessage(string.Empty, Color.Gray);
        }

        private void SetStatusMessage(string message, Color color)
        {
            lblStatus.ForeColor = color;
            lblStatus.Text = message;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _analysisCts?.Cancel();
        }
    }

    internal static class StringSafeExtension
    {
        public static string TruncateSafe(this string s, int max) =>
            s.Length <= max ? s : s[..max];
    }
}
