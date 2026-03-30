namespace ATS.FromUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dtgAllAnalyze = new DataGridView();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            txtJobTitle = new TextBox();
            txtJobRequitments = new TextBox();
            label7 = new Label();
            label6 = new Label();
            groupBox3 = new GroupBox();
            lblMaxScore = new Label();
            lblScore = new Label();
            lblName = new Label();
            label2 = new Label();
            label3 = new Label();
            label1 = new Label();
            txtFeedback = new TextBox();
            lblStatus = new Label();
            label5 = new Label();
            groupBox4 = new GroupBox();
            btnClean = new Button();
            btnPdfReport = new Button();
            btnNewAnalyze = new Button();
            btnUploadCv = new Button();
            btnAnalyze = new Button();
            progressAnalyzeTime = new ProgressBar();
            groupBox5 = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dtgAllAnalyze).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // dtgAllAnalyze
            // 
            dtgAllAnalyze.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtgAllAnalyze.Location = new Point(6, 26);
            dtgAllAnalyze.Name = "dtgAllAnalyze";
            dtgAllAnalyze.RowHeadersWidth = 51;
            dtgAllAnalyze.Size = new Size(482, 394);
            dtgAllAnalyze.TabIndex = 0;
            dtgAllAnalyze.CellClick += dtgAllAnalyze_CellClick;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dtgAllAnalyze);
            groupBox1.FlatStyle = FlatStyle.Popup;
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(497, 426);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "All Analyze Report List";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(txtJobTitle);
            groupBox2.Controls.Add(txtJobRequitments);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label6);
            groupBox2.FlatStyle = FlatStyle.Popup;
            groupBox2.Location = new Point(515, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(917, 426);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Job Requirements";
            // 
            // txtJobTitle
            // 
            txtJobTitle.BackColor = Color.Gainsboro;
            txtJobTitle.Location = new Point(82, 23);
            txtJobTitle.Name = "txtJobTitle";
            txtJobTitle.Size = new Size(396, 27);
            txtJobTitle.TabIndex = 1;
            // 
            // txtJobRequitments
            // 
            txtJobRequitments.BackColor = Color.Gainsboro;
            txtJobRequitments.Location = new Point(8, 84);
            txtJobRequitments.Multiline = true;
            txtJobRequitments.Name = "txtJobRequitments";
            txtJobRequitments.ScrollBars = ScrollBars.Vertical;
            txtJobRequitments.Size = new Size(903, 335);
            txtJobRequitments.TabIndex = 0;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.FlatStyle = FlatStyle.Popup;
            label7.Location = new Point(8, 61);
            label7.Name = "label7";
            label7.Size = new Size(130, 20);
            label7.TabIndex = 0;
            label7.Text = "Job Requirements:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.FlatStyle = FlatStyle.Popup;
            label6.Location = new Point(8, 26);
            label6.Name = "label6";
            label6.Size = new Size(68, 20);
            label6.TabIndex = 0;
            label6.Text = "Job Title:";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(lblMaxScore);
            groupBox3.Controls.Add(lblScore);
            groupBox3.Controls.Add(lblName);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(label1);
            groupBox3.FlatStyle = FlatStyle.Popup;
            groupBox3.Location = new Point(999, 444);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(433, 107);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Analyze Results";
            // 
            // lblMaxScore
            // 
            lblMaxScore.AutoSize = true;
            lblMaxScore.FlatStyle = FlatStyle.Popup;
            lblMaxScore.Location = new Point(113, 31);
            lblMaxScore.Name = "lblMaxScore";
            lblMaxScore.Size = new Size(16, 20);
            lblMaxScore.TabIndex = 0;
            lblMaxScore.Text = "x";
            // 
            // lblScore
            // 
            lblScore.AutoSize = true;
            lblScore.FlatStyle = FlatStyle.Popup;
            lblScore.Location = new Point(320, 30);
            lblScore.Name = "lblScore";
            lblScore.Size = new Size(16, 20);
            lblScore.TabIndex = 0;
            lblScore.Text = "x";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.FlatStyle = FlatStyle.Popup;
            lblName.Location = new Point(64, 70);
            lblName.Name = "lblName";
            lblName.Size = new Size(16, 20);
            lblName.TabIndex = 0;
            lblName.Text = "x";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.FlatStyle = FlatStyle.Popup;
            label2.Location = new Point(6, 70);
            label2.Name = "label2";
            label2.Size = new Size(52, 20);
            label2.TabIndex = 0;
            label2.Text = "Name:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.FlatStyle = FlatStyle.Popup;
            label3.Location = new Point(6, 31);
            label3.Name = "label3";
            label3.Size = new Size(110, 20);
            label3.TabIndex = 0;
            label3.Text = "Max ATS Score:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.FlatStyle = FlatStyle.Popup;
            label1.Location = new Point(236, 30);
            label1.Name = "label1";
            label1.Size = new Size(78, 20);
            label1.TabIndex = 0;
            label1.Text = "ATS Score:";
            // 
            // txtFeedback
            // 
            txtFeedback.BackColor = Color.Gainsboro;
            txtFeedback.Location = new Point(6, 26);
            txtFeedback.Multiline = true;
            txtFeedback.Name = "txtFeedback";
            txtFeedback.ReadOnly = true;
            txtFeedback.ScrollBars = ScrollBars.Vertical;
            txtFeedback.Size = new Size(1408, 267);
            txtFeedback.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.FlatStyle = FlatStyle.Popup;
            lblStatus.Location = new Point(506, 444);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(16, 20);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "x";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.FlatStyle = FlatStyle.Popup;
            label5.Location = new Point(448, 444);
            label5.Name = "label5";
            label5.Size = new Size(52, 20);
            label5.TabIndex = 0;
            label5.Text = "Status:";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(btnClean);
            groupBox4.Controls.Add(btnPdfReport);
            groupBox4.Controls.Add(btnNewAnalyze);
            groupBox4.Controls.Add(btnUploadCv);
            groupBox4.Controls.Add(btnAnalyze);
            groupBox4.FlatStyle = FlatStyle.Popup;
            groupBox4.Location = new Point(12, 444);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(427, 107);
            groupBox4.TabIndex = 4;
            groupBox4.TabStop = false;
            groupBox4.Text = "Operations";
            // 
            // btnClean
            // 
            btnClean.FlatStyle = FlatStyle.Popup;
            btnClean.Location = new Point(178, 61);
            btnClean.Name = "btnClean";
            btnClean.Size = new Size(234, 29);
            btnClean.TabIndex = 5;
            btnClean.Text = "Clean Form";
            btnClean.UseVisualStyleBackColor = true;
            btnClean.Click += btnClean_Click;
            // 
            // btnPdfReport
            // 
            btnPdfReport.FlatStyle = FlatStyle.Popup;
            btnPdfReport.Location = new Point(6, 61);
            btnPdfReport.Name = "btnPdfReport";
            btnPdfReport.Size = new Size(166, 29);
            btnPdfReport.TabIndex = 5;
            btnPdfReport.Text = "Report Pdf Export";
            btnPdfReport.UseVisualStyleBackColor = true;
            btnPdfReport.Click += btnPdfReport_Click;
            // 
            // btnNewAnalyze
            // 
            btnNewAnalyze.FlatStyle = FlatStyle.Popup;
            btnNewAnalyze.Location = new Point(298, 26);
            btnNewAnalyze.Name = "btnNewAnalyze";
            btnNewAnalyze.Size = new Size(114, 29);
            btnNewAnalyze.TabIndex = 5;
            btnNewAnalyze.Text = "New Analyze";
            btnNewAnalyze.UseVisualStyleBackColor = true;
            btnNewAnalyze.Click += btnNewAnalyze_Click;
            // 
            // btnUploadCv
            // 
            btnUploadCv.FlatStyle = FlatStyle.Popup;
            btnUploadCv.Location = new Point(6, 26);
            btnUploadCv.Name = "btnUploadCv";
            btnUploadCv.Size = new Size(114, 29);
            btnUploadCv.TabIndex = 5;
            btnUploadCv.Text = "Upload CV";
            btnUploadCv.UseVisualStyleBackColor = true;
            btnUploadCv.Click += btnUploadCv_Click;
            // 
            // btnAnalyze
            // 
            btnAnalyze.FlatStyle = FlatStyle.Popup;
            btnAnalyze.Location = new Point(126, 26);
            btnAnalyze.Name = "btnAnalyze";
            btnAnalyze.Size = new Size(166, 29);
            btnAnalyze.TabIndex = 5;
            btnAnalyze.Text = "Analyze";
            btnAnalyze.UseVisualStyleBackColor = true;
            btnAnalyze.Click += btnAnalyze_Click;
            // 
            // progressAnalyzeTime
            // 
            progressAnalyzeTime.Location = new Point(445, 490);
            progressAnalyzeTime.Name = "progressAnalyzeTime";
            progressAnalyzeTime.Size = new Size(545, 29);
            progressAnalyzeTime.TabIndex = 5;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(txtFeedback);
            groupBox5.FlatStyle = FlatStyle.Popup;
            groupBox5.Location = new Point(12, 560);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(1420, 299);
            groupBox5.TabIndex = 6;
            groupBox5.TabStop = false;
            groupBox5.Text = "Feedbacks";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1444, 865);
            Controls.Add(groupBox5);
            Controls.Add(progressAnalyzeTime);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(lblStatus);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(label5);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ATS Score Analyzer";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dtgAllAnalyze).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dtgAllAnalyze;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private TextBox txtJobRequitments;
        private GroupBox groupBox3;
        private Label label1;
        private Label label2;
        private Label lblMaxScore;
        private Label lblScore;
        private Label lblName;
        private Label label3;
        private TextBox txtFeedback;
        private GroupBox groupBox4;
        private Button btnPdfReport;
        private Button btnNewAnalyze;
        private Button btnAnalyze;
        private Button btnClean;
        private ProgressBar progressAnalyzeTime;
        private Button btnUploadCv;
        private Label lblStatus;
        private Label label5;
        private GroupBox groupBox5;
        private Label label6;
        private TextBox txtJobTitle;
        private Label label7;
    }
}
