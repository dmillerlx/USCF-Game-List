namespace USCF_Game_List;

public class SettingsDialog : Form
{
    private Label lblAccessKey;
    private TextBox txtAccessKey;
    private Label lblSecretKey;
    private TextBox txtSecretKey;
    private Label lblBucket;
    private TextBox txtBucket;
    private Label lblRegion;
    private TextBox txtRegion;
    private Button btnCancel;
    private Button btnSave;

    public AppSettings Settings { get; private set; }

    public SettingsDialog(AppSettings currentSettings)
    {
        Settings = new AppSettings
        {
            AwsAccessKey = currentSettings.AwsAccessKey,
            AwsSecretKey = currentSettings.AwsSecretKey,
            S3Bucket = currentSettings.S3Bucket,
            AwsRegion = currentSettings.AwsRegion
        };

        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        this.lblAccessKey = new Label();
        this.txtAccessKey = new TextBox();
        this.lblSecretKey = new Label();
        this.txtSecretKey = new TextBox();
        this.lblBucket = new Label();
        this.txtBucket = new TextBox();
        this.lblRegion = new Label();
        this.txtRegion = new TextBox();
        this.btnCancel = new Button();
        this.btnSave = new Button();

        this.SuspendLayout();

        // lblAccessKey
        this.lblAccessKey.AutoSize = true;
        this.lblAccessKey.Location = new System.Drawing.Point(12, 12);
        this.lblAccessKey.Name = "lblAccessKey";
        this.lblAccessKey.Size = new System.Drawing.Size(110, 15);
        this.lblAccessKey.TabIndex = 0;
        this.lblAccessKey.Text = "AWS Access Key:";

        // txtAccessKey
        this.txtAccessKey.Location = new System.Drawing.Point(12, 30);
        this.txtAccessKey.Name = "txtAccessKey";
        this.txtAccessKey.Size = new System.Drawing.Size(380, 23);
        this.txtAccessKey.TabIndex = 1;

        // lblSecretKey
        this.lblSecretKey.AutoSize = true;
        this.lblSecretKey.Location = new System.Drawing.Point(12, 62);
        this.lblSecretKey.Name = "lblSecretKey";
        this.lblSecretKey.Size = new System.Drawing.Size(105, 15);
        this.lblSecretKey.TabIndex = 2;
        this.lblSecretKey.Text = "AWS Secret Key:";

        // txtSecretKey
        this.txtSecretKey.Location = new System.Drawing.Point(12, 80);
        this.txtSecretKey.Name = "txtSecretKey";
        this.txtSecretKey.Size = new System.Drawing.Size(380, 23);
        this.txtSecretKey.TabIndex = 3;
        this.txtSecretKey.UseSystemPasswordChar = true;

        // lblBucket
        this.lblBucket.AutoSize = true;
        this.lblBucket.Location = new System.Drawing.Point(12, 112);
        this.lblBucket.Name = "lblBucket";
        this.lblBucket.Size = new System.Drawing.Size(65, 15);
        this.lblBucket.TabIndex = 4;
        this.lblBucket.Text = "S3 Bucket:";

        // txtBucket
        this.txtBucket.Location = new System.Drawing.Point(12, 130);
        this.txtBucket.Name = "txtBucket";
        this.txtBucket.Size = new System.Drawing.Size(380, 23);
        this.txtBucket.TabIndex = 5;

        // lblRegion
        this.lblRegion.AutoSize = true;
        this.lblRegion.Location = new System.Drawing.Point(12, 162);
        this.lblRegion.Name = "lblRegion";
        this.lblRegion.Size = new System.Drawing.Size(75, 15);
        this.lblRegion.TabIndex = 6;
        this.lblRegion.Text = "AWS Region:";

        // txtRegion
        this.txtRegion.Location = new System.Drawing.Point(12, 180);
        this.txtRegion.Name = "txtRegion";
        this.txtRegion.Size = new System.Drawing.Size(380, 23);
        this.txtRegion.TabIndex = 7;

        // btnCancel
        this.btnCancel.DialogResult = DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(236, 220);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 30);
        this.btnCancel.TabIndex = 8;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;

        // btnSave
        this.btnSave.Location = new System.Drawing.Point(317, 220);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(75, 30);
        this.btnSave.TabIndex = 9;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += BtnSave_Click;

        // SettingsDialog
        this.AcceptButton = this.btnSave;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(404, 262);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.txtRegion);
        this.Controls.Add(this.lblRegion);
        this.Controls.Add(this.txtBucket);
        this.Controls.Add(this.lblBucket);
        this.Controls.Add(this.txtSecretKey);
        this.Controls.Add(this.lblSecretKey);
        this.Controls.Add(this.txtAccessKey);
        this.Controls.Add(this.lblAccessKey);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SettingsDialog";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "AWS S3 Settings";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void LoadSettings()
    {
        txtAccessKey.Text = Settings.AwsAccessKey;
        txtSecretKey.Text = Settings.AwsSecretKey;
        txtBucket.Text = Settings.S3Bucket;
        txtRegion.Text = Settings.AwsRegion;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        Settings.AwsAccessKey = txtAccessKey.Text.Trim();
        Settings.AwsSecretKey = txtSecretKey.Text.Trim();
        Settings.S3Bucket = txtBucket.Text.Trim();
        Settings.AwsRegion = txtRegion.Text.Trim();

        if (string.IsNullOrEmpty(Settings.AwsAccessKey) || string.IsNullOrEmpty(Settings.AwsSecretKey))
        {
            MessageBox.Show("Please enter both AWS Access Key and Secret Key", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrEmpty(Settings.S3Bucket))
        {
            MessageBox.Show("Please enter an S3 bucket name", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
