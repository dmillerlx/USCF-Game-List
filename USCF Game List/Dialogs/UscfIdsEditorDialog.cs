using USCF_Game_List.Services;

namespace USCF_Game_List.Dialogs;

public partial class UscfIdsEditorDialog : Form
{
    private TextBox txtEditor = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Label lblStatus = null!;
    private readonly S3Service _s3Service;
    private string _originalContent = "";

    public UscfIdsEditorDialog(S3Service s3Service)
    {
        _s3Service = s3Service;
        InitializeComponent();
        LoadUscfIds();
    }

    private void InitializeComponent()
    {
        this.Text = "Edit USCF IDs";
        this.Size = new System.Drawing.Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;

        // Create label
        var lblInstructions = new Label
        {
            Text = "Enter one USCF ID per line:",
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(560, 20),
            Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
        };

        // Create text editor
        txtEditor = new TextBox
        {
            Location = new System.Drawing.Point(10, 35),
            Size = new System.Drawing.Size(560, 360),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new System.Drawing.Font("Consolas", 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        // Create status label
        lblStatus = new Label
        {
            Location = new System.Drawing.Point(10, 405),
            Size = new System.Drawing.Size(560, 20),
            Text = "Loading...",
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        // Create save button
        btnSave = new Button
        {
            Text = "Save to S3",
            Location = new System.Drawing.Point(380, 430),
            Size = new System.Drawing.Size(90, 25),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnSave.Click += BtnSave_Click;

        // Create cancel button
        btnCancel = new Button
        {
            Text = "Cancel",
            Location = new System.Drawing.Point(480, 430),
            Size = new System.Drawing.Size(90, 25),
            DialogResult = DialogResult.Cancel,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnCancel.Click += (s, e) => this.Close();

        this.Controls.Add(lblInstructions);
        this.Controls.Add(txtEditor);
        this.Controls.Add(lblStatus);
        this.Controls.Add(btnSave);
        this.Controls.Add(btnCancel);
        this.CancelButton = btnCancel;
    }

    private async void LoadUscfIds()
    {
        try
        {
            lblStatus.Text = "Loading from S3...";
            btnSave.Enabled = false;

            var ids = await _s3Service.DownloadUscfIdsAsync();
            _originalContent = string.Join(Environment.NewLine, ids);
            txtEditor.Text = _originalContent;

            lblStatus.Text = $"Loaded {ids.Count} USCF IDs from S3";
            btnSave.Enabled = true;
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error loading: {ex.Message}";
            MessageBox.Show($"Failed to load USCF IDs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            lblStatus.Text = "Saving to S3...";
            btnSave.Enabled = false;
            btnCancel.Enabled = false;

            // Parse IDs from text (one per line, remove duplicates and empty lines)
            var ids = txtEditor.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            await _s3Service.UploadUscfIdsAsync(ids);

            lblStatus.Text = $"Saved {ids.Count} USCF IDs to S3";
            _originalContent = string.Join(Environment.NewLine, ids);
            txtEditor.Text = _originalContent; // Update with sorted/deduplicated version

            MessageBox.Show($"Successfully saved {ids.Count} USCF IDs to S3!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error saving: {ex.Message}";
            MessageBox.Show($"Failed to save USCF IDs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
        }
    }
}
