using USCF_Game_List.Models;

namespace USCF_Game_List;

public class AddGameLinkDialog : Form
{
    private readonly List<GameDisplayModel> _games;
    private Label lblTournament;
    private Label lblDate;
    private Label lblUrl;
    private TextBox txtUrl;
    private Button btnCancel;
    private Button btnSave;

    public string GameUrl { get; private set; } = "";

    public AddGameLinkDialog(List<GameDisplayModel> games)
    {
        _games = games;
        InitializeComponent();
        LoadGameInfo();
    }

    private void InitializeComponent()
    {
        this.lblTournament = new Label();
        this.lblDate = new Label();
        this.lblUrl = new Label();
        this.txtUrl = new TextBox();
        this.btnCancel = new Button();
        this.btnSave = new Button();

        this.SuspendLayout();

        // lblTournament
        this.lblTournament.AutoSize = false;
        this.lblTournament.Location = new System.Drawing.Point(12, 12);
        this.lblTournament.Name = "lblTournament";
        this.lblTournament.Size = new System.Drawing.Size(460, 40);
        this.lblTournament.TabIndex = 0;
        this.lblTournament.Text = "Tournament: ";

        // lblDate
        this.lblDate.AutoSize = true;
        this.lblDate.Location = new System.Drawing.Point(12, 60);
        this.lblDate.Name = "lblDate";
        this.lblDate.Size = new System.Drawing.Size(100, 15);
        this.lblDate.TabIndex = 1;
        this.lblDate.Text = "Date: ";

        // lblUrl
        this.lblUrl.AutoSize = true;
        this.lblUrl.Location = new System.Drawing.Point(12, 92);
        this.lblUrl.Name = "lblUrl";
        this.lblUrl.Size = new System.Drawing.Size(58, 15);
        this.lblUrl.TabIndex = 2;
        this.lblUrl.Text = "Game URL:";

        // txtUrl
        this.txtUrl.Location = new System.Drawing.Point(12, 110);
        this.txtUrl.Name = "txtUrl";
        this.txtUrl.Size = new System.Drawing.Size(460, 23);
        this.txtUrl.TabIndex = 3;

        // btnCancel
        this.btnCancel.DialogResult = DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(316, 150);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 30);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;

        // btnSave
        this.btnSave.Location = new System.Drawing.Point(397, 150);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(75, 30);
        this.btnSave.TabIndex = 5;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += BtnSave_Click;

        // AddGameLinkDialog
        this.AcceptButton = this.btnSave;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(484, 192);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.txtUrl);
        this.Controls.Add(this.lblUrl);
        this.Controls.Add(this.lblDate);
        this.Controls.Add(this.lblTournament);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "AddGameLinkDialog";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Add Game Link";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void LoadGameInfo()
    {
        if (_games.Count == 0) return;

        var firstGame = _games[0];
        lblTournament.Text = $"Tournament: {firstGame.EventName}";
        lblDate.Text = $"Date: {firstGame.EndDate}";

        if (_games.Count > 1)
        {
            this.Text = $"Add Game Link ({_games.Count} tournaments selected)";
        }

        // Try to get URL from clipboard if it looks like a URL
        if (Clipboard.ContainsText())
        {
            var clipText = Clipboard.GetText();
            if (Uri.IsWellFormedUriString(clipText, UriKind.Absolute))
            {
                txtUrl.Text = clipText;
                txtUrl.SelectAll();
            }
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var url = txtUrl.Text.Trim();

        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("Please enter a game URL", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            var result = MessageBox.Show(
                "The URL doesn't appear to be valid. Save anyway?",
                "Invalid URL",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }
        }

        GameUrl = url;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
