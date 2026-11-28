using System.Drawing.Drawing2D;
using ReboostTV.Core.Adb;
using ReboostTV.Core.Models;
using ReboostTV.Core.Security;
using ReboostTV.WinForms.Localization;

namespace ReboostTV.WinForms;

public sealed class MainForm : Form
{
    private readonly AdbService _adb;
    private readonly string _baseDir;
    private ReboostConfig _activeConfig = ReboostConfig.SafeDefault();
    private string _connectedDevice = string.Empty;
    private bool _isDarkMode = true;
    private int _activeTabIndex = 0;
    private string _lang = "Čeština";
    private readonly ToolTip _toolTip = new();

    // Barevné palety (Dark / Light)
    private Color BgMain => _isDarkMode ? Color.FromArgb(18, 20, 26) : Color.FromArgb(243, 244, 246);
    private Color BgCard => _isDarkMode ? Color.FromArgb(28, 31, 40) : Color.FromArgb(255, 255, 255);
    private Color BgRemote => _isDarkMode ? Color.FromArgb(24, 27, 35) : Color.FromArgb(229, 231, 235);
    private Color BgRing => _isDarkMode ? Color.FromArgb(36, 40, 52) : Color.FromArgb(209, 213, 219);
    private Color BgBtn => _isDarkMode ? Color.FromArgb(45, 50, 64) : Color.FromArgb(209, 213, 219);
    private Color BgInput => _isDarkMode ? Color.FromArgb(35, 39, 50) : Color.White;
    private Color BgLog => _isDarkMode ? Color.FromArgb(12, 14, 18) : Color.FromArgb(249, 250, 251);
    private Color TextMain => _isDarkMode ? Color.FromArgb(240, 244, 248) : Color.FromArgb(17, 24, 39);
    private Color TextMuted => _isDarkMode ? Color.FromArgb(156, 163, 175) : Color.FromArgb(75, 85, 99);
    private Color TextLog => _isDarkMode ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 101, 52);

    private static readonly Color AccentBlue = Color.FromArgb(0, 132, 255);
    private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129);
    private static readonly Color AccentRestore = Color.FromArgb(22, 163, 74);
    private static readonly Color AccentRed = Color.FromArgb(220, 38, 38);

    // Hlavička a navigace
    private readonly Panel _topPanel = new() { Dock = DockStyle.Top, Height = 58, Padding = new Padding(12, 10, 12, 8) };
    private readonly Panel _navPanel = new() { Dock = DockStyle.Top, Height = 44, Padding = new Padding(12, 4, 12, 0) };
    private readonly Panel _contentHost = new() { Dock = DockStyle.Fill };
    private readonly Panel _boostPanel = new() { Dock = DockStyle.Fill, AutoScroll = true };
    private readonly Panel _toolsPanel = new() { Dock = DockStyle.Fill, AutoScroll = true, Visible = false };

    private readonly Label _lblIp = new() { AutoSize = true, Padding = new Padding(0, 7, 4, 0) };
    private readonly ComboBox _cmbDevices = new() { Width = 148, FlatStyle = FlatStyle.Flat };
    private readonly Button _btnScan = new() { Width = 112, Height = 34, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly Button _btnConnect = new() { Width = 112, Height = 34, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly ComboBox _cmbLang = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 105, FlatStyle = FlatStyle.Flat };
    private readonly Button _btnTheme = new() { Text = "☀️", Width = 40, Height = 34, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly Label _lblStatus = new() { AutoSize = true, ForeColor = AccentRed, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Padding = new Padding(6, 7, 0, 0) };

    private readonly Button _tabBtnBoost = new() { Width = 175, Height = 38, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly Button _tabBtnTools = new() { Width = 235, Height = 38, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

    private readonly TextBox _txtLog = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Bottom,
        Height = 120,
        BorderStyle = BorderStyle.None,
        Font = new Font("Consolas", 9.5f)
    };

    // 1-Click Boost prvky
    private readonly CheckBox _chkDebloat = new() { Checked = true, AutoSize = true };
    private readonly CheckBox _chkCache = new() { Checked = true, AutoSize = true };
    private readonly CheckBox _chkAnim = new() { Checked = true, AutoSize = true };
    private readonly CheckBox _chkSilent = new() { AutoSize = true };
    private readonly CheckBox _chkSleep = new() { AutoSize = true };
    private readonly CheckBox _chkTurbo = new() { Checked = true, AutoSize = true };
    private readonly CheckBox _chkAutoHide = new() { Checked = true, AutoSize = true };
    private readonly Label _lblDns = new() { AutoSize = true, Padding = new Padding(0, 6, 8, 0) };
    private readonly ComboBox _cmbDns = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 155, FlatStyle = FlatStyle.Flat };
    private readonly Label _lblDnsDesc = new() { AutoSize = true, Padding = new Padding(0, 2, 0, 6), Font = new Font("Segoe UI", 9.5f, FontStyle.Italic) };

    private readonly Button _btnBoost = new() { Width = 245, Height = 46, BackColor = AccentGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
    private readonly Button _btnReboot = new() { Width = 155, Height = 46, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly Button _btnLoadConfig = new() { Width = 175, Height = 46, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

    // Pravý sloupec na 1-Click Boost: Skóre zdraví + Kulaté zelené tlačítko Obnovit
    private readonly Label _lblHealthTitle = new() { AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
    private readonly Label _lblHealthScore = new() { AutoSize = true, Font = new Font("Segoe UI", 22f, FontStyle.Bold), ForeColor = AccentGreen };
    private readonly Label _lblHealthDetail = new() { AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
    private Button _btnRestoreRound = null!;

    // Rentgen & Ovladač prvky
    private readonly Label _lblDiag = new() { AutoSize = true, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold) };
    private readonly Button _btnKillRam = new() { Width = 240, Height = 40, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(0, 6, 0, 6) };
    private readonly Button _btnPing = new() { Width = 240, Height = 40, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(0, 4, 0, 4) };
    private readonly Button _btnRamInfo = new() { Width = 240, Height = 40, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(0, 4, 0, 4) };
    private readonly Label _lblKey = new() { AutoSize = true, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), Padding = new Padding(0, 18, 0, 6) };
    private readonly TextBox _txtSend = new() { Width = 265, BorderStyle = BorderStyle.FixedSingle };
    private readonly Button _btnSendText = new() { Width = 90, Height = 30, BackColor = AccentBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
    private readonly Label _lblRemoteTitle = new() { AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Location = new Point(82, 24) };

    private Panel _dpadRing = null!;
    private readonly List<Button> _neutralButtons = [];
    private readonly List<Label> _mutedLabels = [];
    private readonly List<Panel> _cardPanels = [];

    public MainForm()
    {
        _baseDir = AppContext.BaseDirectory;
        _adb = new AdbService(Path.Combine(_baseDir, "adb.exe"));

        Text = "ReboostTV (NakopniTV) v1.11";
        Width = 940;
        Height = 710;
        MinimumSize = new Size(880, 660);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10.5f);

        BuildUi();
        ApplyLanguage();
        ApplyTheme();
        TryAutoLoadConfig();

        Shown += async (_, _) => await ScanTvsAsync(autoConnectIfSingle: true);
    }

    private void BuildUi()
    {
        var topFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _mutedLabels.Add(_lblIp);
        _cmbDevices.Text = "192.168.1.50";

        _cmbLang.Items.AddRange(Strings.Languages);
        _cmbLang.SelectedIndex = 0;
        _cmbLang.SelectedIndexChanged += (_, _) =>
        {
            _lang = _cmbLang.SelectedItem?.ToString() ?? "Čeština";
            ApplyLanguage();
        };

        _btnScan.FlatAppearance.BorderSize = 0;
        _btnConnect.FlatAppearance.BorderSize = 0;
        _btnConnect.BackColor = AccentBlue;
        _btnConnect.ForeColor = Color.White;
        _btnTheme.FlatAppearance.BorderSize = 0;

        _neutralButtons.AddRange([_btnScan, _btnTheme]);

        _btnScan.Click += async (_, _) => await ScanTvsAsync(false);
        _btnConnect.Click += async (_, _) => await ConnectTvAsync();
        _btnTheme.Click += (_, _) => { _isDarkMode = !_isDarkMode; ApplyTheme(); };

        topFlow.Controls.AddRange([_lblIp, _cmbDevices, _btnScan, _btnConnect, _cmbLang, _btnTheme, _lblStatus]);
        _topPanel.Controls.Add(topFlow);

        var navFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _tabBtnBoost.FlatAppearance.BorderSize = 0;
        _tabBtnTools.FlatAppearance.BorderSize = 0;
        _tabBtnBoost.Click += (_, _) => SwitchTab(0);
        _tabBtnTools.Click += (_, _) => SwitchTab(1);
        navFlow.Controls.AddRange([_tabBtnBoost, _tabBtnTools]);
        _navPanel.Controls.Add(navFlow);

        BuildBoostContent();
        BuildToolsContent();
        _contentHost.Controls.Add(_boostPanel);
        _contentHost.Controls.Add(_toolsPanel);

        Controls.Add(_contentHost);
        Controls.Add(_navPanel);
        Controls.Add(_topPanel);
        Controls.Add(_txtLog);
    }

    private void BuildBoostContent()
    {
        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(22, 14, 22, 14)
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));

        // --- LEVÁ STRANA: Přepínače + DNS + Akce ---
        var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };

        _cmbDns.Items.AddRange([DnsMode.None, DnsMode.Adblock, DnsMode.Family]);
        _cmbDns.SelectedItem = DnsMode.Adblock;
        _cmbDns.SelectedIndexChanged += (_, _) => UpdateDnsDescription();

        var dnsRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        _mutedLabels.Add(_lblDns);
        _mutedLabels.Add(_lblDnsDesc);
        dnsRow.Controls.AddRange([_lblDns, _cmbDns]);

        _btnBoost.FlatAppearance.BorderSize = 0;
        _btnReboot.FlatAppearance.BorderSize = 0;
        _btnLoadConfig.FlatAppearance.BorderSize = 0;
        _neutralButtons.AddRange([_btnReboot, _btnLoadConfig]);

        _btnBoost.Click += async (_, _) => await RunBoostAsync();
        _btnReboot.Click += async (_, _) => await RunRebootAsync();
        _btnLoadConfig.Click += (_, _) => LoadConfigDialog();

        var actionRow = new FlowLayoutPanel { AutoSize = true, Padding = new Padding(0, 12, 0, 0) };
        actionRow.Controls.AddRange([_btnBoost, _btnReboot, _btnLoadConfig]);

        foreach (var chk in new[] { _chkDebloat, _chkCache, _chkAnim, _chkSilent, _chkSleep, _chkTurbo, _chkAutoHide })
        {
            chk.Margin = new Padding(0, 4, 0, 4);
            left.Controls.Add(chk);
        }

        left.Controls.Add(dnsRow);
        left.Controls.Add(_lblDnsDesc);
        left.Controls.Add(actionRow);

        // --- PRAVÁ STRANA: Skóre zdraví TV + Kulaté zelené tlačítko Obnovit ---
        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10, 4, 0, 0)
        };

        var healthCard = new Panel { Width = 215, Height = 125, Padding = new Padding(14) };
        ApplyRoundedRegion(healthCard, 16);
        _cardPanels.Add(healthCard);
        _mutedLabels.Add(_lblHealthTitle);
        _mutedLabels.Add(_lblHealthDetail);

        _lblHealthTitle.Location = new Point(14, 12);
        _lblHealthScore.Location = new Point(14, 36);
        _lblHealthDetail.Location = new Point(14, 86);
        healthCard.Controls.AddRange([_lblHealthTitle, _lblHealthScore, _lblHealthDetail]);

        // Kulaté zelené tlačítko pro obnovení do původního stavu
        _btnRestoreRound = MakeRoundButton("", AccentRestore, Color.White, 126, 126);
        _btnRestoreRound.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _btnRestoreRound.Margin = new Padding(44, 24, 0, 0);
        _btnRestoreRound.Click += async (_, _) => await RunRestoreAsync();

        right.Controls.Add(healthCard);
        right.Controls.Add(_btnRestoreRound);

        split.Controls.Add(left, 0, 0);
        split.Controls.Add(right, 1, 0);
        _boostPanel.Controls.Add(split);
    }

    private void BuildToolsContent()
    {
        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(22, 14, 22, 14)
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));

        var leftFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        _mutedLabels.Add(_lblDiag);
        _mutedLabels.Add(_lblKey);

        _btnKillRam.FlatAppearance.BorderSize = 0;
        _btnPing.FlatAppearance.BorderSize = 0;
        _btnRamInfo.FlatAppearance.BorderSize = 0;
        _btnSendText.FlatAppearance.BorderSize = 0;
        _neutralButtons.AddRange([_btnPing, _btnRamInfo]);

        _btnKillRam.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.KillAllBackgroundAsync(d);
            Log(r.Success ? "Kill Switch: procesy na pozadí ukončeny." : $"Chyba: {r.Error}");
        });
        _btnPing.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            Log("Spouštím Wi-Fi Ping test...");
            var r = await _adb.RunWifiPingTestAsync(d);
            Log(r.Success ? r.Output : $"Chyba ping testu: {r.Error}");
        });
        _btnRamInfo.Click += async (_, _) => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.GetMemoryInfoAsync(d);
            Log(r.Success ? string.Join(Environment.NewLine, r.Output.Split('\n').Take(3)) : $"Chyba: {r.Error}");
        });

        var textRow = new FlowLayoutPanel { AutoSize = true };
        async Task SendTextAction() => await ExecConnectedAsync(async d =>
        {
            var r = await _adb.SendTextAsync(d, _txtSend.Text);
            Log(r.Success ? "Text odeslán do TV." : $"Chyba: {r.Error}");
            if (r.Success) _txtSend.Clear();
        });
        _btnSendText.Click += async (_, _) => await SendTextAction();
        _txtSend.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await SendTextAction(); }
        };
        textRow.Controls.AddRange([_txtSend, _btnSendText]);

        leftFlow.Controls.AddRange([_lblDiag, _btnKillRam, _btnPing, _btnRamInfo, _lblKey, textRow]);

        var remoteBody = new Panel { Width = 250, Height = 395, Anchor = AnchorStyles.Top };
        ApplyRoundedRegion(remoteBody, 24);
        _cardPanels.Add(remoteBody);

        var btnPower = MakeRoundButton("⏻", AccentRed, Color.White, 42, 42);
        btnPower.Location = new Point(18, 14);
        btnPower.Click += async (_, _) => await ExecConnectedAsync(d => _adb.SendKeyEventAsync(d, 26));
        _mutedLabels.Add(_lblRemoteTitle);

        _dpadRing = new Panel { Width = 186, Height = 186, Location = new Point(32, 68) };
        using (var ringPath = new GraphicsPath())
        {
            ringPath.AddEllipse(0, 0, 186, 186);
            _dpadRing.Region = new Region(ringPath);
        }

        var btnUp = MakeRoundRemoteKey("▲", 19, 71, 10, 44, 44);
        var btnDown = MakeRoundRemoteKey("▼", 20, 71, 132, 44, 44);
        var btnLeft = MakeRoundRemoteKey("◀", 21, 10, 71, 44, 44);
        var btnRight = MakeRoundRemoteKey("▶", 22, 132, 71, 44, 44);

        var btnOk = MakeRoundButton("OK", AccentBlue, Color.White, 60, 60);
        btnOk.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        btnOk.Location = new Point(63, 63);
        btnOk.Click += async (_, _) => await ExecConnectedAsync(d => _adb.SendKeyEventAsync(d, 23));

        _dpadRing.Controls.AddRange([btnUp, btnDown, btnLeft, btnRight, btnOk]);

        var btnBack = MakePillRemoteKey("↩", 4, 24, 274, 62, 40);
        var btnHome = MakePillRemoteKey("🏠", 3, 94, 274, 62, 40);
        var btnMenu = MakePillRemoteKey("☰", 82, 164, 274, 62, 40);

        var btnVolDown = MakePillRemoteKey("🔉-", 25, 24, 326, 62, 40);
        var btnMute = MakePillRemoteKey("🔇", 164, 94, 326, 62, 40);
        var btnVolUp = MakePillRemoteKey("🔊+", 24, 164, 326, 62, 40);

        remoteBody.Controls.AddRange([btnPower, _lblRemoteTitle, _dpadRing, btnBack, btnHome, btnMenu, btnVolDown, btnMute, btnVolUp]);

        split.Controls.Add(leftFlow, 0, 0);
        split.Controls.Add(remoteBody, 1, 0);
        _toolsPanel.Controls.Add(split);
    }

    private Button MakeRoundRemoteKey(string label, int keyCode, int x, int y, int w, int h)
    {
        var btn = MakeRoundButton(label, BgBtn, TextMain, w, h);
        btn.Location = new Point(x, y);
        btn.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        btn.Click += async (_, _) => await ExecConnectedAsync(d => _adb.SendKeyEventAsync(d, keyCode));
        _neutralButtons.Add(btn);
        return btn;
    }

    private Button MakePillRemoteKey(string label, int keyCode, int x, int y, int w, int h)
    {
        var btn = new Button
        {
            Text = label,
            Location = new Point(x, y),
            Width = w,
            Height = h,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };
        btn.FlatAppearance.BorderSize = 0;
        ApplyRoundedRegion(btn, 12);
        btn.Click += async (_, _) => await ExecConnectedAsync(d => _adb.SendKeyEventAsync(d, keyCode));
        _neutralButtons.Add(btn);
        return btn;
    }

    private static Button MakeRoundButton(string text, Color bg, Color fg, int w, int h)
    {
        var btn = new Button
        {
            Text = text,
            Width = w,
            Height = h,
            BackColor = bg,
            ForeColor = fg,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        using var path = new GraphicsPath();
        path.AddEllipse(0, 0, w, h);
        btn.Region = new Region(path);
        return btn;
    }

    private static void ApplyRoundedRegion(Control c, int radius)
    {
        using var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(0, 0, d, d, 180, 90);
        path.AddArc(c.Width - d, 0, d, d, 270, 90);
        path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
        path.AddArc(0, c.Height - d, d, d, 90, 90);
        path.CloseFigure();
        c.Region = new Region(path);
    }

    private void ApplyLanguage()
    {
        string L(string key) => Strings.T(_lang, key);

        _lblIp.Text = L("IpLabel");
        _btnScan.Text = L("Scan");
        _btnConnect.Text = L("Connect");
        _lblStatus.Text = string.IsNullOrEmpty(_connectedDevice)
            ? L("Disconnected")
            : $"{L("Connected")} ({_connectedDevice.Split(':')[0]})";

        _tabBtnBoost.Text = L("TabBoost");
        _tabBtnTools.Text = L("TabTools");

        _chkDebloat.Text = L("Debloat");
        _chkCache.Text = L("Cache");
        _chkAnim.Text = L("Anim");
        _chkSilent.Text = L("Silent");
        _chkSleep.Text = L("Sleep");
        _chkTurbo.Text = L("Turbo");
        _chkAutoHide.Text = L("AutoHide");

        _lblDns.Text = L("DnsLabel");
        _btnBoost.Text = L("RunBoost");
        _btnReboot.Text = L("Reboot");
        _btnLoadConfig.Text = L("LoadConfig");

        _lblHealthTitle.Text = L("HealthTitle");
        if (string.IsNullOrEmpty(_connectedDevice)) _lblHealthScore.Text = L("HealthUnknown");
        _btnRestoreRound.Text = L("RestoreBtn");
        _toolTip.SetToolTip(_btnRestoreRound, L("RestoreTooltip"));

        _lblDiag.Text = L("DiagTitle");
        _btnKillRam.Text = L("KillRam");
        _btnPing.Text = L("PingTest");
        _btnRamInfo.Text = L("RamStatus");
        _lblKey.Text = L("KeyTitle");
        _txtSend.PlaceholderText = L("KeyPlaceholder");
        _btnSendText.Text = L("Send");
        _lblRemoteTitle.Text = L("RemoteTitle");

        UpdateDnsDescription();
    }

    private void UpdateDnsDescription()
    {
        var mode = _cmbDns.SelectedItem is DnsMode m ? m : DnsMode.None;
        _lblDnsDesc.Text = mode switch
        {
            DnsMode.Adblock => Strings.T(_lang, "DnsAdblock"),
            DnsMode.Family => Strings.T(_lang, "DnsFamily"),
            _ => Strings.T(_lang, "DnsNone")
        };
    }

    private void SwitchTab(int index)
    {
        _activeTabIndex = index;
        _boostPanel.Visible = index == 0;
        _toolsPanel.Visible = index == 1;
        UpdateTabColors();
    }

    private void ApplyTheme()
    {
        BackColor = BgMain;
        ForeColor = TextMain;

        _topPanel.BackColor = BgCard;
        _navPanel.BackColor = BgCard;
        _contentHost.BackColor = BgMain;
        _boostPanel.BackColor = BgMain;
        _toolsPanel.BackColor = BgMain;
        _dpadRing.BackColor = BgRing;

        _cmbDevices.BackColor = BgInput;
        _cmbDevices.ForeColor = TextMain;
        _cmbDns.BackColor = BgInput;
        _cmbDns.ForeColor = TextMain;
        _cmbLang.BackColor = BgInput;
        _cmbLang.ForeColor = TextMain;
        _txtSend.BackColor = BgInput;
        _txtSend.ForeColor = TextMain;

        _txtLog.BackColor = BgLog;
        _txtLog.ForeColor = TextLog;
        _btnTheme.Text = _isDarkMode ? "☀️" : "🌙";

        foreach (var btn in _neutralButtons)
        {
            btn.BackColor = BgBtn;
            btn.ForeColor = TextMain;
        }
        foreach (var lbl in _mutedLabels) lbl.ForeColor = TextMuted;
        foreach (var p in _cardPanels) p.BackColor = BgRemote;
        foreach (var chk in new[] { _chkDebloat, _chkCache, _chkAnim, _chkSilent, _chkSleep, _chkTurbo, _chkAutoHide })
            chk.ForeColor = TextMain;

        UpdateTabColors();
    }

    private void UpdateTabColors()
    {
        _tabBtnBoost.BackColor = _activeTabIndex == 0 ? AccentBlue : BgBtn;
        _tabBtnBoost.ForeColor = _activeTabIndex == 0 ? Color.White : TextMain;
        _tabBtnTools.BackColor = _activeTabIndex == 1 ? AccentBlue : BgBtn;
        _tabBtnTools.ForeColor = _activeTabIndex == 1 ? Color.White : TextMain;
    }

    private async Task RefreshHealthScoreAsync(string device)
    {
        var (score, summary) = await _adb.CalculateHealthScoreAsync(device);
        _lblHealthScore.Text = $"{score} / 100";
        _lblHealthScore.ForeColor = score >= 80 ? AccentGreen : (score >= 55 ? Color.Orange : AccentRed);
        _lblHealthDetail.Text = summary;
    }

    private async Task ScanTvsAsync(bool autoConnectIfSingle)
    {
        _btnScan.Enabled = false;
        Log("Skenuji domácí Wi-Fi pro Android/Google/Fire TV (port 5555)...");
        var (ips, subnets) = await _adb.ScanNetworkForTvsAsync();
        _cmbDevices.Items.Clear();

        if (subnets.Count > 0)
        {
            Log($"Proskenované sítě: {string.Join(", ", subnets.Select(s => s + ".x"))}");
        }

        if (ips.Count == 0)
        {
            Log("Žádná TV s povoleným ADB (5555) nebyla nalezena. Zadej IP ručně nebo zapni Ladění přes síť v TV.");
        }
        else
        {
            foreach (var ip in ips) _cmbDevices.Items.Add(ip);
            _cmbDevices.SelectedIndex = 0;
            Log($"Nalezeno TV: {ips.Count} ({string.Join(", ", ips)}). Vyber TV ze seznamu.");
            if (autoConnectIfSingle && ips.Count == 1) await ConnectTvAsync();
        }
        _btnScan.Enabled = true;
    }
    
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
        _chkAutoHide.Checked = _activeConfig.EnableHudAutoHide;
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
            _lblStatus.ForeColor = AccentGreen;
            ApplyLanguage();
            Log($"Připojeno: {res.Output}");
            await RefreshHealthScoreAsync(_connectedDevice);
        }
        else
        {
            _connectedDevice = string.Empty;
            _lblStatus.ForeColor = AccentRed;
            ApplyLanguage();
            Log($"Připojení selhalo: {res.Output} {res.Error}");
        }
    }

    private async Task RunBoostAsync() => await ExecConnectedAsync(async d =>
    {
        var backupPath = await _adb.SaveBackupAsync(d);
        Log($"Záloha uložena do: {backupPath}");

        var currentConfig = new ReboostConfig
        {
            DebloatBloatware = _chkDebloat.Checked,
            DeepCacheClean = _chkCache.Checked,
            FastAnimations = _chkAnim.Checked,
            SilentMenu = _chkSilent.Checked,
            DisableSleep = _chkSleep.Checked,
            InstallTurboHud = _chkTurbo.Checked,
            EnableHudAutoHide = _chkAutoHide.Checked,
            DnsMode = _cmbDns.SelectedItem is DnsMode mode ? mode : DnsMode.None
        };

        Log("Spouštím 1-Click Boost...");
        var logs = await _adb.ApplyBoostAsync(d, currentConfig, _baseDir);
        foreach (var line in logs) Log("✓ " + line);
        await RefreshHealthScoreAsync(d);
        Log("Hotovo! Skóre zdraví TV bylo aktualizováno.");
    });

    private async Task RunRestoreAsync() => await ExecConnectedAsync(async d =>
    {
        Log("Obnovuji televizi do původního stavu...");
        var logs = await _adb.RestoreDefaultsAsync(d);
        foreach (var line in logs) Log("↩ " + line);
        await RefreshHealthScoreAsync(d);
        Log("Obnova dokončena.");
    });

    private async Task RunRebootAsync() => await ExecConnectedAsync(async d =>
    {
        Log("Posílám příkaz k restartu TV...");
        await _adb.RebootAsync(d);
    });

    private async Task ExecConnectedAsync(Func<string, Task> action)
 {
     if (string.IsNullOrEmpty(_connectedDevice) || !await _adb.IsDeviceOnlineAsync(_connectedDevice))
     {
         _connectedDevice = string.Empty;
         _lblStatus.ForeColor = AccentRed;
         ApplyLanguage();
         Log("TV není připojena! Nejdřív vyber televizi a klikni na Připojit TV.");
         MessageBox.Show(
             "Televize není připojena (nebo se odpojila).\nNejprve vyberte TV a klikněte na Připojit TV.",
             "ReboostTV – TV není připojena",
             MessageBoxButtons.OK,
             MessageBoxIcon.Warning);
         return;
     }
     await action(_connectedDevice);
    }

    private void Log(string message) =>
        _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");


} 