using ATS.Application.Abstract.Services;
using ATS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ATS.FromUI
{
    public partial class Form1 : Form
    {
        private readonly ICvScanService _cvScanService;
        private readonly IReportService _reportService;
        private readonly ILogger<Form1> _logger;
        private string? _uploadedFilePath;
        private string? _uploadedFileName;
        private string? _uploadedFileType;
        private string? _lastReportPath;
        private CvScan? _selectedScan;
        public Form1(ICvScanService cvScanService, IReportService reportService, ILogger<Form1> logger)
        {
            InitializeComponent();
            _cvScanService = cvScanService;
            _reportService = reportService;
            _logger = logger;
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
                var scans = (await _cvScanService.GetAllScansAsync()).OrderByDescending(s => s.CreatedAt).ToList();
                var rows = scans.Select(s => new
                {
                    s.Id,
                    CandidateName = !string.IsNullOrEmpty(s.FilePath)
                                    ? Path.GetFileName(s.FilePath)
                                    : "No File Name",
                    s.OverallScore,
                    AnalysisType = s.IsJobMatched ? "Job Match" : " Gen. ATS Score",
                    s.CreatedAt
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
            SetBusyState(true);
            ResetResultArea();

            try
            {
                await SimulateProgressAsync(0, 30, 300);

                CvScan result;
                string title = txtJobTitle.Text.Trim();
                string jobRequirementsText = txtJobRequitments.Text.Trim();
                bool isJobMatch = !string.IsNullOrWhiteSpace(jobRequirementsText);

                await SimulateProgressAsync(30, 60, 200);

                if (isJobMatch)
                {
                    string finalJobTitle = !string.IsNullOrWhiteSpace(txtJobTitle.Text)
                           ? txtJobTitle.Text.Trim()
                           : ExtractJobTitle(jobRequirementsText);

                    result = await _cvScanService.AnalyzeWithJobPostingAsync(_uploadedFilePath, _uploadedFileType, jobRequirementsText, finalJobTitle);
                }
                else
                {
                    result = await _cvScanService.AnalyzeAsync(_uploadedFilePath, _uploadedFileType);
                }
                await SimulateProgressAsync(60, 90, 200);

                _selectedScan = result;
                _lastReportPath = result.ScoreReports?.FirstOrDefault()?.ReportPath;

                lblMaxScore.Text = "100";
                lblScore.Text = result.OverallScore.ToString();
                lblName.Text = !string.IsNullOrWhiteSpace(result.CandidateName) ? result.CandidateName : "Unknown Candidate";
                txtFeedback.Text = BuildFeedbackText(result);

                await SimulateProgressAsync(90, 100, 150);

                SetStatusMessage($"✅ Analysis complete — Score: {result.OverallScore}/100", Color.ForestGreen);

                btnPdfReport.Enabled = true;
                await LoadAnalyzeHistoryAsync();

                _logger.LogInformation("Analysis done. Mode={Mode}, CvScanId={Id}, Score={Score}", isJobMatch ? "JobMatch" : "General", result.Id, result.OverallScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed");
                string fullError = ex.ToString();
                if (ex.InnerException != null)
                    fullError += "\n\nINNER: " + ex.InnerException.ToString();

                MessageBox.Show(fullError, "Analysis failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
                string reportPath = target.IsJobMatched
                    ? _reportService.GenerateJobMatchReport(target)
                    : _reportService.GenerateGeneralReport(target);

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

                CvScan? scan = await _cvScanService.GetScanWithDetailsAsync(scanId);
                if (scan == null) return;

                _selectedScan = scan;
                _lastReportPath = scan.ScoreReports?.FirstOrDefault()?.ReportPath;

                lblMaxScore.Text = "100";
                lblScore.Text = scan.OverallScore.ToString();
                lblName.Text = scan.CandidateName;
                txtFeedback.Text = BuildFeedbackText(scan);

                txtJobRequitments.Text = scan.JobPosting?.RawText ?? string.Empty;
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

            if (scan.IsJobMatched && scan.JobPosting != null)
            {
                sb.AppendLine($"🎯 Job Match Score: {scan.JobPosting.MatchScore}/20");
                if (!string.IsNullOrWhiteSpace(scan.JobPosting.Title))
                    sb.AppendLine($"   Position: {scan.JobPosting.Title}");
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
            btnPdfReport.Enabled = !busy;
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
            txtJobTitle.Text = string.Empty;
            progressAnalyzeTime.Value = 0;
            btnPdfReport.Enabled = false;
        }

        private void ResetForm()
        {
            ResetResultArea();
            txtJobRequitments.Text = string.Empty;
            txtJobTitle.Text = string.Empty;
            _uploadedFilePath = null;
            _uploadedFileType = null;
            _lastReportPath = null;
            _selectedScan = null;
            btnAnalyze.Enabled = false;
            SetStatusMessage(string.Empty, Color.Gray);
        }

        private void SetStatusMessage(string message, Color color)
        {

            lblStatus.ForeColor = color;
            lblStatus.Text = message;
        }
    }

    internal static class StringSafeExtension
    {
        public static string TruncateSafe(this string s, int max) =>
            s.Length <= max ? s : s[..max];
    }
}
