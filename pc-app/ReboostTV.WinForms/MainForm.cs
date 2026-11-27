using ReboostTV.Core.Adb;
using ReboostTV.Core.Models;
using ReboostTV.Core.Security;

namespace ReboostTV.WinForms;

public sealed class MainForm : Form
{
    private readonly AdbService _adb;
    private readonly string _baseDir;
    private ReboostConfig _activeConfig = ReboostConfig.SafeDefault();
    private string _connectedDevice = string.Empty;

    // Barvy moderního Dark Mode
    private static readonly Color BgDark = Color.FromArgb(18, 20, 26);
    private static readonly Color CardBg = Color.FromArgb(28, 31, 40);
    private static readonly Color AccentBlue = Color.FromArgb(0, 132, 255);
    private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
    private static readonly Color BtnNeutral = Color.FromArgb(45, 50, 64);
    private static readonly Color TextLight = Color.FromArgb(238, 242, 248);
    private static readonly Color TextMuted = Color.FromArgb(156, 163, 175);

    private readonly ComboBox _cmbDevices = new() { Width = 175, BackColor = CardBg, ForeColor = TextLight, FlatStyle = FlatStyle.Flat };
    private readonly Button _btnScan = MakeStyledButton("🔍 Skenovat Wi-Fi", BtnNeutral, 135, 32);
    private readonly Button _btnConnect = MakeStyledButton("Připojit TV", AccentBlue, 110, 32);
    private readonly Label _lblStatus = new() { Text = "● Nepřipojeno", AutoSize = true, ForeColor = Color.IndianRed, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };

    private readonly TextBox _txtLog = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Bottom,
        Height = 135,
        BackColor = Color.FromArgb(12, 14, 18),
        ForeColor = Color.FromArgb(74, 222, 128),
        BorderStyle = BorderStyle.None,
        Font = new Font("Consolas", 9.5f)
    };

    private readonly CheckBox _chkDebloat = MakeCheckBox("Odstranit balast (Safe Debloat)", true);
    private readonly CheckBox _chkCache = MakeCheckBox("Deep Cache Clean (uvolnit skrytou paměť)", true);
    private readonly CheckBox _chkAnim = MakeCheckBox("Zrychlit animace na 0.5x (okamžité reakce)", true);
    private readonly CheckBox _chkSilent = MakeCheckBox("Tiché menu (vypnout klikání ovladače)", false);
    private readonly CheckBox _chkSleep = MakeCheckBox("Vypnout automatické usínání TV", false);
    private readonly CheckBox _chkTurbo = MakeCheckBox("Nainstalovat ReboostTV Turbo HUD (.apk)", true);
    private readonly ComboBox _cmbDns = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180, BackColor = BtnNeutral, ForeColor = TextLight, FlatStyle = FlatStyle.Flat };

    public MainForm()
    {
        _baseDir = AppContext.BaseDirectory;
        _adb = new AdbService(Path.Combine(_baseDir, "adb.exe"));

        Text = "ReboostTV (NakopniTV) v1.0";
        Width = 820;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BgDark;
        ForeColor = TextLight;
        Font = new Font("Segoe UI", 9.5f);

        BuildUi();
        TryAutoLoadConfig();

        // Automatický sken Wi-Fi hned po otevření okna
        Shown += async (_, _) => await ScanTvsAsync(autoConnectIfSingle: true);
    }

    private void BuildUi()
    {
        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = CardBg,
            Padding = new Padding(14, 11, 14, 8)
        };

        _cmbDevices.Text = "192.168.1.50";
        topPanel.Controls.Add(new Label { Text = "Televize na Wi-Fi:", AutoSize = true, ForeColor = TextMuted, Padding = new Padding(0, 6, 4, 0) });
        topPanel.Controls.Add(_cmbDevices);
        topPanel.Controls.Add(_btnScan);
        topPanel.Controls.Add(_btnConnect);
        _lblStatus.Padding = new Padding(10, 6, 0, 0);
        topPanel.Controls.Add(_lblStatus);

        _btnScan.Click += async (_, _) => await ScanTvsAsync(autoConnectIfSingle: false);
        _btnConnect.Click += async (_, _) => await ConnectTvAsync();

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildBoostTab());
        tabs.TabPages.Add(BuildToolsTab());

        Controls.Add(tabs);
        Controls.Add(topPanel);
        Controls.Add(_txtLog);
    }

    private TabPage BuildBoostTab()
    {
        var page = new TabPage("🚀 1-Click Boost") { BackColor = BgDark, ForeColor = TextLight };
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20)
        };

        _cmbDns.Items.AddRange([DnsMode.None, DnsMode.Adblock, DnsMode.Family]);
        _cmbDns.SelectedItem = DnsMode.Adblock;

        var dnsRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 8, 0, 8) };
        dnsRow.Controls.Add(new Label { Text = "Private DNS (Blokování reklam):", AutoSize = true, ForeColor = TextMuted, Padding = new Padding(0, 5, 8, 0) });
        dnsRow.Controls.Add(_cmbDns);

        var btnBoost = MakeStyledButton("⚡ SPUSTIT 1-CLICK BOOST", AccentGreen, 250, 44);
        btnBoost.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        btnBoost.Click += async (_, _) => await RunBoostAsync();

        var btnReboot = MakeStyledButton("🔄 Restartovat TV", BtnNeutral, 150, 44);
        btnReboot.Click += async (_, _) => await RunRebootAsync();

        var btnLoadConfig = MakeStyledButton("📂 Načíst config.json", BtnNeutral, 175, 44);
        btnLoadConfig.Click += (_, _) => LoadConfigDialog();

        var actionRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 14, 0, 0) };
        actionRow.Controls.AddRange([btnBoost, btnReboot, btnLoadConfig]);

        layout.Controls.AddRange([_chkDebloat, _chkCache, _chkAnim, _chkSilent, _chkSleep, _chkTurbo, dnsRow, actionRow]);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildToolsTab()
    {
        var page = new TabPage("🛠️ Rentgen TV & PC Ovladač") { BackColor = BgDark, ForeColor = TextLight };
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20)
        };

        var diagRow = new FlowLayoutPanel { AutoSize = true };
        var btnKillRam = MakeStyledButton("⚡ Kill Switch (RAM)", AccentBlue, 165, 38);
        var btnPing = MakeStyledButton("📶 Wi-Fi Ping Test", BtnNeutral, 160, 38);
        var btnRamInfo = MakeStyledButton("📊 Stav RAM", BtnNeutral, 130, 38);

        btnKillRam.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.KillAllBackgroundAsync(d);
            Log(r.Success ? "Kill Switch: procesy na pozadí ukončeny." : $"Chyba: {r.Error}");
        });
        btnPing.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            Log("Spouštím Wi-Fi Ping test (1.1.1.1)...");
            var r = await _adb.RunWifiPingTestAsync(d);
            Log(r.Success ? r.Output : $"Chyba ping testu: {r.Error}");
        });
        btnRamInfo.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.GetMemoryInfoAsync(d);
            Log(r.Success ? string.Join(Environment.NewLine, r.Output.Split('\n').Take(3)) : $"Chyba: {r.Error}");
        });
        diagRow.Controls.AddRange([btnKillRam, btnPing, btnRamInfo]);

        var remoteRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        remoteRow.Controls.AddRange([
            MakeKeyButton("▲", 19), MakeKeyButton("▼", 20), MakeKeyButton("◀", 21),
            MakeKeyButton("▶", 22), MakeKeyButton("OK", 23), MakeKeyButton("Zpět", 4),
            MakeKeyButton("Domů", 3), MakeKeyButton("Mute", 164)
        ]);

        var textRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        var txtSend = new TextBox { PlaceholderText = "Napiš heslo nebo text do TV...", Width = 310, BackColor = CardBg, ForeColor = TextLight, BorderStyle = BorderStyle.FixedSingle };
        var btnSendText = MakeStyledButton("Poslat do TV", AccentBlue, 120, 30);
        btnSendText.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.SendTextAsync(d, txtSend.Text);
            Log(r.Success ? "Text odeslán do TV." : $"Chyba: {r.Error}");
            if (r.Success) txtSend.Clear();
        });
        textRow.Controls.AddRange([txtSend, btnSendText]);

        layout.Controls.Add(new Label { Text = "Rychlá diagnostika:", AutoSize = true, ForeColor = TextMuted, Font = new Font(Font, FontStyle.Bold) });
        layout.Controls.Add(diagRow);
        layout.Controls.Add(new Label { Text = "PC Ovladač:", AutoSize = true, ForeColor = TextMuted, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 14, 0, 0) });
        layout.Controls.Add(remoteRow);
        layout.Controls.Add(new Label { Text = "Klávesnice pro hesla:", AutoSize = true, ForeColor = TextMuted, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 14, 0, 0) });
        layout.Controls.Add(textRow);

        page.Controls.Add(layout);
        return page;
    }

    private async Task ScanTvsAsync(bool autoConnectIfSingle)
    {
        _btnScan.Enabled = false;
        Log("Skenuji domácí Wi-Fi pro Android/Google/Fire TV (port 5555)...");
        var ips = await _adb.ScanNetworkForTvsAsync();
        _cmbDevices.Items.Clear();

        if (ips.Count == 0)
        {
            Log("Žádná TV s povoleným ADB (5555) nebyla nalezena. Zadej IP ručně nebo zapni Ladění přes síť v TV.");
        }
        else
        {
            foreach (var ip in ips) _cmbDevices.Items.Add(ip);
            _cmbDevices.SelectedIndex = 0;
            Log($"Nalezeno TV: {ips.Count} ({string.Join(", ", ips)}). Vyber TV ze seznamu.");

            if (autoConnectIfSingle && ips.Count == 1)
            {
                await ConnectTvAsync();
            }
        }
        _btnScan.Enabled = true;
    }

    private Button MakeKeyButton(string label, int keyCode)
    {
        var btn = MakeStyledButton(label, BtnNeutral, 62, 36);
        btn.Click += async (_, _) => await ExecConnectedAsync(async d => await _adb.SendKeyEventAsync(d, keyCode));
        return btn;
    }

    private static Button MakeStyledButton(string text, Color bg, int w, int h) => new()
    {
        Text = text,
        Width = w,
        Height = h,
        BackColor = bg,
        ForeColor = TextLight,
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand,
        Margin = new Padding(0, 0, 8, 6),
        FlatAppearance = { BorderSize = 0 }
    };

    private static CheckBox MakeCheckBox(string text, bool isChecked) => new()
    {
        Text = text,
        Checked = isChecked,
        AutoSize = true,
        ForeColor = TextLight,
        Margin = new Padding(0, 4, 0, 4)
    };

    private void TryAutoLoadConfig()
    {
        var defaultPath = Path.Combine(_baseDir, "config.json");
        if (File.Exists(defaultPath)) ApplyConfigFile(defaultPath);
    }

    private void LoadConfigDialog()
    {
        using var dialog = new OpenFileDialog { Filter = "JSON konfigurace (*.json)|*.json" };
        if (dialog.ShowDialog() == DialogResult.OK) ApplyConfigFile(dialog.FileName);
    }

    private void ApplyConfigFile(string path)
    {
        var result = ConfigValidator.LoadFromFile(path);
        _activeConfig = result.Config;
        _chkDebloat.Checked = _activeConfig.DebloatBloatware;
        _chkCache.Checked = _activeConfig.DeepCacheClean;
        _chkAnim.Checked = _activeConfig.FastAnimations;
        _chkSilent.Checked = _activeConfig.SilentMenu;
        _chkSleep.Checked = _activeConfig.DisableSleep;
        _chkTurbo.Checked = _activeConfig.InstallTurboHud;
        _cmbDns.SelectedItem = _activeConfig.DnsMode;

        Log(result.IsValid
            ? $"Konfigurace ověřena: {Path.GetFileName(path)}"
            : $"VAROVÁNÍ: {result.ErrorMessage} Použito bezpečné výchozí nastavení.");
    }

    private async Task ConnectTvAsync()
    {
        var ip = _cmbDevices.Text.Trim();
        Log($"Připojuji se k {ip}:5555...");
        var res = await _adb.ConnectAsync(ip);
        if (res.Success && res.Output.Contains("connected", StringComparison.OrdinalIgnoreCase))
        {
            _connectedDevice = $"{ip}:5555";
            _lblStatus.Text = $"● Připojeno ({ip})";
            _lblStatus.ForeColor = AccentGreen;
            Log($"Připojeno: {res.Output}");
        }
        else
        {
            _connectedDevice = string.Empty;
            _lblStatus.Text = "● Nepřipojeno";
            _lblStatus.ForeColor = Color.IndianRed;
            Log($"Připojení selhalo: {res.Output} {res.Error}");
        }
    }

    private async Task RunBoostAsync() => await ExecConnectedAsync(async d =>
    {
        var currentConfig = new ReboostConfig
        {
            DebloatBloatware = _chkDebloat.Checked,
            DeepCacheClean = _chkCache.Checked,
            FastAnimations = _chkAnim.Checked,
            SilentMenu = _chkSilent.Checked,
            DisableSleep = _chkSleep.Checked,
            InstallTurboHud = _chkTurbo.Checked,
            DnsMode = _cmbDns.SelectedItem is DnsMode mode ? mode : DnsMode.None
        };

        Log("Spouštím 1-Click Boost...");
        var logs = await _adb.ApplyBoostAsync(d, currentConfig, _baseDir);
        foreach (var line in logs) Log("✓ " + line);
        Log("Hotovo! Můžeš restartovat TV nebo pokračovat.");
    });

    private async Task RunRebootAsync() => await ExecConnectedAsync(async d =>
    {
        Log("Posílám příkaz k restartu TV...");
        await _adb.RebootAsync(d);
    });

    private async Task ExecConnectedAsync(Func<string, Task> action)
    {
        if (string.IsNullOrEmpty(_connectedDevice))
        {
            Log("Nejdřív vyber TV a klikni na Připojit TV.");
            return;
        }
        await action(_connectedDevice);
    }

    private void Log(string message) =>
        _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
}