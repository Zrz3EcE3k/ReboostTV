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

    private readonly TextBox _txtIp = new() { PlaceholderText = "192.168.1.50", Width = 150 };
    private readonly Button _btnConnect = new() { Text = "Připojit TV", Width = 110 };
    private readonly Label _lblStatus = new() { Text = "Nepřipojeno", AutoSize = true, ForeColor = Color.DarkRed };
    private readonly TextBox _txtLog = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Bottom,
        Height = 140,
        BackColor = Color.FromArgb(24, 24, 28),
        ForeColor = Color.LightGreen,
        Font = new Font("Consolas", 9.5f)
    };

    // Přepínače na záložce 1-Click Boost
    private readonly CheckBox _chkDebloat = new() { Text = "Odstranit balast (Safe Debloat)", Checked = true, AutoSize = true };
    private readonly CheckBox _chkCache = new() { Text = "Deep Cache Clean", Checked = true, AutoSize = true };
    private readonly CheckBox _chkAnim = new() { Text = "Zrychlit animace (0.5x)", Checked = true, AutoSize = true };
    private readonly CheckBox _chkSilent = new() { Text = "Tiché menu (vypnout klikání)", AutoSize = true };
    private readonly CheckBox _chkSleep = new() { Text = "Vypnout automatické usínání", AutoSize = true };
    private readonly CheckBox _chkTurbo = new() { Text = "Nainstalovat ReboostTV Turbo HUD (.apk)", Checked = true, AutoSize = true };
    private readonly ComboBox _cmbDns = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };

    public MainForm()
    {
        _baseDir = AppContext.BaseDirectory;
        var adbPath = Path.Combine(_baseDir, "adb.exe");
        _adb = new AdbService(adbPath);

        Text = "ReboostTV (NakopniTV) v1.0";
        Width = 760;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        BuildUi();
        TryAutoLoadConfig();
    }

    private void BuildUi()
    {
        // Horní lišta: Připojení k TV
        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(10, 10, 10, 5)
        };
        topPanel.Controls.Add(new Label { Text = "IP adresa TV:", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
        topPanel.Controls.Add(_txtIp);
        topPanel.Controls.Add(_btnConnect);
        topPanel.Controls.Add(_lblStatus);

        _btnConnect.Click += async (_, _) => await ConnectTvAsync();

        // Záložky
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildBoostTab());
        tabs.TabPages.Add(BuildToolsTab());

        Controls.Add(tabs);
        Controls.Add(topPanel);
        Controls.Add(_txtLog);
    }

    private TabPage BuildBoostTab()
    {
        var page = new TabPage("1-Click Boost");
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16)
        };

        _cmbDns.Items.AddRange([DnsMode.None, DnsMode.Adblock, DnsMode.Family]);
        _cmbDns.SelectedItem = DnsMode.Adblock;

        var dnsRow = new FlowLayoutPanel { AutoSize = true };
        dnsRow.Controls.Add(new Label { Text = "Private DNS:", AutoSize = true, Padding = new Padding(0, 5, 6, 0) });
        dnsRow.Controls.Add(_cmbDns);

        var btnLoadConfig = new Button { Text = "Načíst config.json...", Width = 170, Height = 34 };
        btnLoadConfig.Click += (_, _) => LoadConfigDialog();

        var btnBoost = new Button
        {
            Text = "🚀 SPUSTIT 1-CLICK BOOST",
            Width = 240,
            Height = 42,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnBoost.Click += async (_, _) => await RunBoostAsync();

        var btnReboot = new Button { Text = "Restartovat TV", Width = 140, Height = 42 };
        btnReboot.Click += async (_, _) => await RunRebootAsync();

        var actionRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 12, 0, 0) };
        actionRow.Controls.Add(btnBoost);
        actionRow.Controls.Add(btnReboot);
        actionRow.Controls.Add(btnLoadConfig);

        layout.Controls.Add(_chkDebloat);
        layout.Controls.Add(_chkCache);
        layout.Controls.Add(_chkAnim);
        layout.Controls.Add(_chkSilent);
        layout.Controls.Add(_chkSleep);
        layout.Controls.Add(_chkTurbo);
        layout.Controls.Add(dnsRow);
        layout.Controls.Add(actionRow);

        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildToolsTab()
    {
        var page = new TabPage("Rentgen TV & PC Ovladač");
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(16)
        };

        // Diagnostika & Kill Switch
        var diagRow = new FlowLayoutPanel { AutoSize = true };
        var btnKillRam = new Button { Text = "⚡ Kill Switch (RAM)", Width = 150, Height = 36 };
        var btnPing = new Button { Text = "📶 Wi-Fi Ping Test", Width = 150, Height = 36 };
        var btnRamInfo = new Button { Text = "📊 Stav RAM", Width = 130, Height = 36 };

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
            var lines = r.Output.Split('\n').Take(3);
            Log(r.Success ? string.Join(Environment.NewLine, lines) : $"Chyba: {r.Error}");
        });

        diagRow.Controls.AddRange([btnKillRam, btnPing, btnRamInfo]);

        // Ovladač
        var remoteRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 12, 0, 0) };
        remoteRow.Controls.Add(MakeKeyButton("▲", 19));
        remoteRow.Controls.Add(MakeKeyButton("▼", 20));
        remoteRow.Controls.Add(MakeKeyButton("◀", 21));
        remoteRow.Controls.Add(MakeKeyButton("▶", 22));
        remoteRow.Controls.Add(MakeKeyButton("OK", 23));
        remoteRow.Controls.Add(MakeKeyButton("Zpět", 4));
        remoteRow.Controls.Add(MakeKeyButton("Domů", 3));
        remoteRow.Controls.Add(MakeKeyButton("Mute", 164));

        // Klávesnice na hesla
        var textRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 12, 0, 0) };
        var txtSend = new TextBox { PlaceholderText = "Napiš heslo nebo text do TV...", Width = 280 };
        var btnSendText = new Button { Text = "Poslat do TV", Width = 110, Height = 28 };
        btnSendText.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.SendTextAsync(d, txtSend.Text);
            Log(r.Success ? "Text odeslán do TV." : $"Chyba: {r.Error}");
            if (r.Success) txtSend.Clear();
        });
        textRow.Controls.AddRange([txtSend, btnSendText]);

        layout.Controls.Add(new Label { Text = "Rychlá diagnostika:", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        layout.Controls.Add(diagRow);
        layout.Controls.Add(new Label { Text = "PC Ovladač:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 10, 0, 0) });
        layout.Controls.Add(remoteRow);
        layout.Controls.Add(new Label { Text = "Klávesnice pro hesla:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 10, 0, 0) });
        layout.Controls.Add(textRow);

        page.Controls.Add(layout);
        return page;
    }

    private Button MakeKeyButton(string label, int keyCode)
    {
        var btn = new Button { Text = label, Width = 58, Height = 34 };
        btn.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            await _adb.SendKeyEventAsync(d, keyCode);
        });
        return btn;
    }

    private void TryAutoLoadConfig()
    {
        var defaultPath = Path.Combine(_baseDir, "config.json");
        if (File.Exists(defaultPath))
        {
            ApplyConfigFile(defaultPath);
        }
    }

    private void LoadConfigDialog()
    {
        using var dialog = new OpenFileDialog { Filter = "JSON konfigurace (*.json)|*.json" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            ApplyConfigFile(dialog.FileName);
        }
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
            ? $"Konfigurace úspěšně načtena a ověřena: {Path.GetFileName(path)}"
            : $"VAROVÁNÍ: {result.ErrorMessage} Použito bezpečné výchozí nastavení.");
    }

    private async Task ConnectTvAsync()
    {
        var ip = _txtIp.Text.Trim();
        Log($"Připojuji se k {ip}:5555...");
        var res = await _adb.ConnectAsync(ip);
        if (res.Success && res.Output.Contains("connected", StringComparison.OrdinalIgnoreCase))
        {
            _connectedDevice = $"{ip}:5555";
            _lblStatus.Text = $"Připojeno ({_connectedDevice})";
            _lblStatus.ForeColor = Color.DarkGreen;
            Log($"Připojeno: {res.Output}");
        }
        else
        {
            _connectedDevice = string.Empty;
            _lblStatus.Text = "Nepřipojeno";
            _lblStatus.ForeColor = Color.DarkRed;
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
            Log("Nejdřív se připoj k televizi (zadej IP adresu nahoře).");
            return;
        }
        await action(_connectedDevice);
    }

    private void Log(string message)
    {
        _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}