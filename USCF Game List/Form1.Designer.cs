namespace USCF_Game_List
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
            this.lblUSCFId = new System.Windows.Forms.Label();
            this.txtUSCFId = new System.Windows.Forms.TextBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnClearCache = new System.Windows.Forms.Button();
            this.btnYearlyStats = new System.Windows.Forms.Button();
            this.btnEditUscfIds = new System.Windows.Forms.Button();
            this.btnRemoveRandom = new System.Windows.Forms.Button();
            this.btnRemoveRecent = new System.Windows.Forms.Button();
            this.lblStats = new System.Windows.Forms.Label();
            this.dataGridGames = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblS3Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelStats = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridGames)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.panelStats.SuspendLayout();
            this.SuspendLayout();
            //
            // lblUSCFId
            //
            this.lblUSCFId.AutoSize = true;
            this.lblUSCFId.Location = new System.Drawing.Point(12, 15);
            this.lblUSCFId.Name = "lblUSCFId";
            this.lblUSCFId.Size = new System.Drawing.Size(58, 15);
            this.lblUSCFId.TabIndex = 0;
            this.lblUSCFId.Text = "USCF ID:";
            //
            // txtUSCFId
            //
            this.txtUSCFId.Location = new System.Drawing.Point(76, 12);
            this.txtUSCFId.Name = "txtUSCFId";
            this.txtUSCFId.Size = new System.Drawing.Size(120, 23);
            this.txtUSCFId.TabIndex = 1;
            this.txtUSCFId.Text = "30635618";
            //
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(212, 11);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(120, 25);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Refresh Events";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // btnSettings
            //
            this.btnSettings.Location = new System.Drawing.Point(348, 11);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(80, 25);
            this.btnSettings.TabIndex = 3;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            //
            // btnUpload
            //
            this.btnUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpload.Location = new System.Drawing.Point(1068, 11);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(120, 25);
            this.btnUpload.TabIndex = 4;
            this.btnUpload.Text = "Upload to S3";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            //
            // btnClearCache
            //
            this.btnClearCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCache.Location = new System.Drawing.Point(938, 11);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(120, 25);
            this.btnClearCache.TabIndex = 5;
            this.btnClearCache.Text = "Clear Cache";
            this.btnClearCache.UseVisualStyleBackColor = true;
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            //
            // btnYearlyStats
            //
            this.btnYearlyStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnYearlyStats.Location = new System.Drawing.Point(808, 11);
            this.btnYearlyStats.Name = "btnYearlyStats";
            this.btnYearlyStats.Size = new System.Drawing.Size(120, 25);
            this.btnYearlyStats.TabIndex = 8;
            this.btnYearlyStats.Text = "Yearly Stats";
            this.btnYearlyStats.UseVisualStyleBackColor = true;
            this.btnYearlyStats.Click += new System.EventHandler(this.btnYearlyStats_Click);
            //
            // btnEditUscfIds
            //
            this.btnEditUscfIds.Location = new System.Drawing.Point(510, 11);
            this.btnEditUscfIds.Name = "btnEditUscfIds";
            this.btnEditUscfIds.Size = new System.Drawing.Size(120, 25);
            this.btnEditUscfIds.TabIndex = 9;
            this.btnEditUscfIds.Text = "Edit USCF IDs";
            this.btnEditUscfIds.UseVisualStyleBackColor = true;
            this.btnEditUscfIds.Click += new System.EventHandler(this.btnEditUscfIds_Click);
            //
            // btnRemoveRandom
            //
            this.btnRemoveRandom.Location = new System.Drawing.Point(640, 11);
            this.btnRemoveRandom.Name = "btnRemoveRandom";
            this.btnRemoveRandom.Size = new System.Drawing.Size(120, 25);
            this.btnRemoveRandom.TabIndex = 6;
            this.btnRemoveRandom.Text = "Remove 10 Random";
            this.btnRemoveRandom.UseVisualStyleBackColor = true;
            this.btnRemoveRandom.Visible = false;
            this.btnRemoveRandom.Click += new System.EventHandler(this.btnRemoveRandom_Click);
            //
            // btnRemoveRecent
            //
            this.btnRemoveRecent.Location = new System.Drawing.Point(770, 11);
            this.btnRemoveRecent.Name = "btnRemoveRecent";
            this.btnRemoveRecent.Size = new System.Drawing.Size(120, 25);
            this.btnRemoveRecent.TabIndex = 7;
            this.btnRemoveRecent.Text = "Remove 5 Recent";
            this.btnRemoveRecent.UseVisualStyleBackColor = true;
            this.btnRemoveRecent.Visible = false;
            this.btnRemoveRecent.Click += new System.EventHandler(this.btnRemoveRecent_Click);
            //
            // lblStats
            //
            this.lblStats.AutoSize = true;
            this.lblStats.Location = new System.Drawing.Point(10, 8);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(200, 15);
            this.lblStats.TabIndex = 0;
            this.lblStats.Text = "Tournaments: 0 | Games: 0 | W-L-D: 0-0-0";
            //
            // dataGridGames
            //
            this.dataGridGames.AllowUserToAddRows = false;
            this.dataGridGames.AllowUserToDeleteRows = false;
            this.dataGridGames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridGames.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridGames.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridGames.Location = new System.Drawing.Point(12, 125);
            this.dataGridGames.Name = "dataGridGames";
            this.dataGridGames.ReadOnly = true;
            this.dataGridGames.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridGames.Size = new System.Drawing.Size(1176, 476);
            this.dataGridGames.TabIndex = 5;
            this.dataGridGames.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridGames_CellDoubleClick);
            this.dataGridGames.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridGames_CellMouseClick);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblS3Status,
            this.progressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 611);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1200, 22);
            this.statusStrip.TabIndex = 6;
            //
            // lblStatus
            //
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 17);
            this.lblStatus.Text = "Ready";
            //
            // lblS3Status
            //
            this.lblS3Status.Name = "lblS3Status";
            this.lblS3Status.Size = new System.Drawing.Size(53, 17);
            this.lblS3Status.Text = "S3: Not Connected";
            //
            // progressBar
            //
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 16);
            this.progressBar.Visible = false;
            //
            // panelTop
            //
            this.panelTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTop.BackColor = System.Drawing.SystemColors.Control;
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.lblUSCFId);
            this.panelTop.Controls.Add(this.txtUSCFId);
            this.panelTop.Controls.Add(this.btnRefresh);
            this.panelTop.Controls.Add(this.btnSettings);
            this.panelTop.Controls.Add(this.btnUpload);
            this.panelTop.Controls.Add(this.btnClearCache);
            this.panelTop.Controls.Add(this.btnYearlyStats);
            this.panelTop.Controls.Add(this.btnEditUscfIds);
            this.panelTop.Controls.Add(this.btnRemoveRandom);
            this.panelTop.Controls.Add(this.btnRemoveRecent);
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1200, 48);
            this.panelTop.TabIndex = 7;
            //
            // panelStats
            //
            this.panelStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelStats.BackColor = System.Drawing.Color.LightYellow;
            this.panelStats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelStats.Controls.Add(this.lblStats);
            this.panelStats.Location = new System.Drawing.Point(12, 60);
            this.panelStats.Name = "panelStats";
            this.panelStats.Size = new System.Drawing.Size(1176, 48);
            this.panelStats.TabIndex = 8;
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 633);
            this.Controls.Add(this.panelStats);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.dataGridGames);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "USCF Tournament & Game Manager";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridGames)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelStats.ResumeLayout(false);
            this.panelStats.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblUSCFId;
        private System.Windows.Forms.TextBox txtUSCFId;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.Button btnYearlyStats;
        private System.Windows.Forms.Button btnEditUscfIds;
        private System.Windows.Forms.Button btnRemoveRandom;
        private System.Windows.Forms.Button btnRemoveRecent;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.DataGridView dataGridGames;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblS3Status;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelStats;
    }
}
