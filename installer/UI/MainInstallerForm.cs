using NoobcraftInstaller.Services;
using NoobcraftInstaller.Utils;

namespace NoobcraftInstaller.UI;

/// <summary>
/// Main installer form for Noobcraft with a modern UI.
/// </summary>
public partial class MainInstallerForm : Form
{
    private readonly InstallerService _installerService;
    private ProgressBar? _progressBar;
    private Label? _statusLabel;
    private Button? _installButton;
    private Button? _cancelButton;
    private RichTextBox? _logTextBox;
    private PictureBox? _logoBox;
    private Label? _titleLabel;
    private Label? _descriptionLabel;
    private CheckBox? _devModeCheckBox;
    private Panel? _headerPanel;
    private Panel? _mainPanel;
    private Panel? _footerPanel;

    public MainInstallerForm()
    {
        _installerService = new InstallerService();
        InitializeComponent();
        SetupEventHandlers();
    }

    private void InitializeComponent()
    {
        // Form properties
        Text = "Noobcraft Installer";
        Size = new Size(600, 500);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(240, 240, 240);

        // Header Panel
        _headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(20)
        };

        // Logo (placeholder)
        _logoBox = new PictureBox
        {
            Size = new Size(64, 64),
            Location = new Point(20, 28),
            BackColor = Color.FromArgb(0, 122, 204),
            SizeMode = PictureBoxSizeMode.CenterImage
        };

        // Title
        _titleLabel = new Label
        {
            Text = "NOOBCRAFT INSTALLER",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(100, 25),
            AutoSize = true
        };

        // Description
        _descriptionLabel = new Label
        {
            Text = "Install mods, configurations, and launcher integration",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.LightGray,
            Location = new Point(100, 55),
            AutoSize = true
        };

        _headerPanel.Controls.AddRange(new Control[] { _logoBox, _titleLabel, _descriptionLabel });

        // Main Panel
        _mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };

        // Status Label
        _statusLabel = new Label
        {
            Text = "Ready to install Noobcraft modpack",
            Font = new Font("Segoe UI", 10),
            Location = new Point(20, 20),
            Size = new Size(540, 23),
            ForeColor = Color.FromArgb(68, 68, 68)
        };

        // Progress Bar
        _progressBar = new ProgressBar
        {
            Location = new Point(20, 50),
            Size = new Size(540, 23),
            Style = ProgressBarStyle.Continuous,
            Value = 0
        };

        // Log TextBox
        _logTextBox = new RichTextBox
        {
            Location = new Point(20, 85),
            Size = new Size(540, 200),
            ReadOnly = true,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White,
            Font = new Font("Consolas", 9),
            ScrollBars = RichTextBoxScrollBars.Vertical
        };

        // Development Mode CheckBox
        _devModeCheckBox = new CheckBox
        {
            Text = "Development Mode (skip prompts)",
            Location = new Point(20, 295),
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(68, 68, 68)
        };

        _mainPanel.Controls.AddRange(new Control[] { 
            _statusLabel, _progressBar, _logTextBox, _devModeCheckBox 
        });

        // Footer Panel
        _footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.FromArgb(250, 250, 250),
            Padding = new Padding(20, 15, 20, 15)
        };

        // Install Button
        _installButton = new Button
        {
            Text = "Install Noobcraft",
            Size = new Size(120, 30),
            Location = new Point(440, 15),
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _installButton.FlatAppearance.BorderSize = 0;

        // Cancel Button
        _cancelButton = new Button
        {
            Text = "Cancel",
            Size = new Size(80, 30),
            Location = new Point(350, 15),
            BackColor = Color.FromArgb(180, 180, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        _cancelButton.FlatAppearance.BorderSize = 0;

        _footerPanel.Controls.AddRange(new Control[] { _installButton, _cancelButton });

        // Add panels to form
        Controls.AddRange(new Control[] { _mainPanel, _headerPanel, _footerPanel });

        // Initial log message
        AppendLog("[INFO] Noobcraft Installer ready", Color.LightBlue);
        AppendLog("[INFO] Click 'Install Noobcraft' to begin installation", Color.LightGray);
    }

    private void SetupEventHandlers()
    {
        if (_installButton != null)
            _installButton.Click += async (s, e) => await StartInstallation();
        if (_cancelButton != null)
            _cancelButton.Click += (s, e) => Close();
        
        // Logger event subscription
        Logger.LogMessageReceived += OnLogMessageReceived;
    }

    private async Task StartInstallation()
    {
        try
        {
            if (_installButton != null)
                _installButton.Enabled = false;
            if (_progressBar != null)
                _progressBar.Value = 0;
            if (_statusLabel != null)
                _statusLabel.Text = "Starting installation...";

            var args = _devModeCheckBox?.Checked == true ? new[] { "--dev" } : Array.Empty<string>();
            
            // Run installation with UI updates
            var success = await RunInstallationWithProgress(args);

            if (success)
            {
                if (_statusLabel != null)
                    _statusLabel.Text = "Installation completed successfully!";
                if (_progressBar != null)
                    _progressBar.Value = 100;
                if (_installButton != null)
                {
                    _installButton.Text = "Launch Minecraft";
                    _installButton.Enabled = true;
                    _installButton.Click -= async (s, e) => await StartInstallation();
                    _installButton.Click += async (s, e) => await LaunchMinecraft();
                }
                
                AppendLog("[SUCCESS] Installation completed! You can now launch Minecraft.", Color.LightGreen);
            }
            else
            {
                if (_statusLabel != null)
                    _statusLabel.Text = "Installation failed. Check the log for details.";
                if (_installButton != null)
                {
                    _installButton.Text = "Retry Installation";
                    _installButton.Enabled = true;
                }
                AppendLog("[ERROR] Installation failed. Please check the log above for details.", Color.LightCoral);
            }
        }
        catch (Exception ex)
        {
            if (_statusLabel != null)
                _statusLabel.Text = "Installation error occurred.";
            if (_installButton != null)
                _installButton.Enabled = true;
            AppendLog($"[ERROR] {ex.Message}", Color.LightCoral);
        }
    }

    private async Task<bool> RunInstallationWithProgress(string[] args)
    {
        var progress = new Progress<(int percentage, string message)>(report =>
        {
            Invoke(() =>
            {
                if (_progressBar != null)
                    _progressBar.Value = Math.Min(report.percentage, 100);
                if (_statusLabel != null)
                    _statusLabel.Text = report.message;
            });
        });

        return await _installerService.RunInstallationWithProgressAsync(args, progress);
    }

    private async Task LaunchMinecraft()
    {
        try
        {
            if (_statusLabel != null)
                _statusLabel.Text = "Launching Minecraft...";
            var launcherService = new LauncherService();
            await launcherService.LaunchMinecraftAsync();
            
            // Close installer after launching
            await Task.Delay(2000);
            Close();
        }
        catch (Exception ex)
        {
            AppendLog($"[ERROR] Failed to launch Minecraft: {ex.Message}", Color.LightCoral);
        }
    }

    private void OnLogMessageReceived(string message, LogLevel level)
    {
        Color color = level switch
        {
            LogLevel.Info => Color.LightBlue,
            LogLevel.Success => Color.LightGreen,
            LogLevel.Warning => Color.Yellow,
            LogLevel.Error => Color.LightCoral,
            _ => Color.White
        };

        Invoke(() => AppendLog(message, color));
    }

    private void AppendLog(string message, Color color)
    {
        if (_logTextBox == null) return;
        
        _logTextBox.SelectionStart = _logTextBox.TextLength;
        _logTextBox.SelectionLength = 0;
        _logTextBox.SelectionColor = color;
        _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} {message}\n");
        _logTextBox.SelectionColor = _logTextBox.ForeColor;
        _logTextBox.ScrollToCaret();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        Logger.LogMessageReceived -= OnLogMessageReceived;
        base.OnFormClosing(e);
    }
}
