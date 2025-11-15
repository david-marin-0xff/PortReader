using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortReader
{
    public partial class Form1 : Form
    {
        // UI
        private Label lblCpu = new Label();
        private Label lblRam = new Label();
        private DataGridView portGrid = new DataGridView();
        private TextBox explanationBox = new TextBox();
        private Button btnSettings = new Button();

        // Performance counters
        private PerformanceCounter cpuCounter;
        private PerformanceCounter memCounter;

        // Timers
        private System.Windows.Forms.Timer uiTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer();

        // Refresh guard
        private bool isRefreshing = false;

        // Regex for netstat parsing
        private static readonly Regex netstatLineRegex = new Regex(
            @"^(TCP|UDP)\s+([^\s:]+):(\d+)\s+([^\s:]+):(\d+|\*)\s+([A-Za-z]+)?\s*([0-9]+)?",
            RegexOptions.Compiled);

        // Filter property
        public string ViewFilter { get; set; } = "All";

        public Form1()
        {
            InitializeComponent();

            Text = "Port Reader 1.0";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            // Perf counters
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memCounter = new PerformanceCounter("Memory", "Available MBytes");

            BuildLayout();
            SetupGridColumns();

            // UI timer
            uiTimer.Interval = 1000;
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();

            // Refresh timer
            refreshTimer.Interval = 5000;
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();

            // Initial load
            _ = RefreshPortsAsync();
        }

        private void BuildLayout()
        {
            // Labels
            lblCpu.AutoSize = true;
            lblCpu.Left = 10;
            lblCpu.Top = 10;
            lblCpu.Font = new System.Drawing.Font("Segoe UI", 9);

            lblRam.AutoSize = true;
            lblRam.Left = 10;
            lblRam.Top = 30;
            lblRam.Font = new System.Drawing.Font("Segoe UI", 9);

            Controls.Add(lblCpu);
            Controls.Add(lblRam);

            // Settings button
            btnSettings.Text = "Settings";
            btnSettings.Left = 820;
            btnSettings.Top = 10;
            btnSettings.Width = 150;
            btnSettings.Click += BtnSettings_Click;
            Controls.Add(btnSettings);

            // DataGridView
            portGrid.Left = 10;
            portGrid.Top = 60;
            portGrid.Width = 960;
            portGrid.Height = 420;
            portGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            portGrid.ReadOnly = true;
            portGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            portGrid.MultiSelect = false;
            portGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            portGrid.CellDoubleClick += PortGrid_CellDoubleClick;
            Controls.Add(portGrid);

            // Explanation box
            explanationBox.Left = 10;
            explanationBox.Top = 500;
            explanationBox.Width = 960;
            explanationBox.Height = 100;
            explanationBox.Multiline = true;
            explanationBox.ReadOnly = true;
            explanationBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(explanationBox);
        }

        private void SetupGridColumns()
        {
            portGrid.Columns.Clear();
            portGrid.Columns.Add("Port", "Port");
            portGrid.Columns.Add("Protocol", "Protocol");
            portGrid.Columns.Add("State", "State");
            portGrid.Columns.Add("LocalIP", "Local IP");
            portGrid.Columns.Add("LocalPort", "Local Port");
            portGrid.Columns.Add("RemoteIP", "Remote IP");
            portGrid.Columns.Add("RemotePort", "Remote Port");
            portGrid.Columns.Add("PID", "PID");
            portGrid.Columns.Add("Process", "Process");
            portGrid.Columns.Add("ProcessPath", "Process Path");
            portGrid.Columns.Add("Service", "Service");
        }

        private void UiTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                float cpu = cpuCounter.NextValue();
                float memAvailable = memCounter.NextValue();
                lblCpu.Text = $"CPU Usage: {cpu:0.0}%";
                lblRam.Text = $"Available RAM: {memAvailable:0.0} MB";

                if (portGrid.SelectedRows.Count > 0)
                {
                    var r = portGrid.SelectedRows[0];
                    explanationBox.Text =
                        $"Protocol: {r.Cells["Protocol"].Value}\r\n" +
                        $"Local: {r.Cells["LocalIP"].Value}:{r.Cells["LocalPort"].Value}\r\n" +
                        $"Remote: {r.Cells["RemoteIP"].Value}:{r.Cells["RemotePort"].Value}\r\n" +
                        $"State: {r.Cells["State"].Value}\r\n" +
                        $"PID: {r.Cells["PID"].Value}\r\n" +
                        $"Process: {r.Cells["Process"].Value}\r\n" +
                        $"Service: {r.Cells["Service"].Value}\r\n" +
                        $"Path: {r.Cells["ProcessPath"].Value}";
                }
            }
            catch { }
        }

        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            if (isRefreshing) return;
            isRefreshing = true;
            try { await RefreshPortsAsync(); }
            finally { isRefreshing = false; }
        }

        public async Task RefreshPortsAsync()
        {
            try
            {
                var entries = await Task.Run(() => GetNetstatEntries());

                // Apply filter
                string filter = ViewFilter;
                if (filter != "All")
                {
                    if (filter == "TCP" || filter == "UDP")
                        entries = entries.Where(x => x.Protocol.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                    else if (filter == "Listening")
                        entries = entries.Where(x => x.State?.Equals("LISTENING", StringComparison.OrdinalIgnoreCase) == true).ToList();
                    else if (filter == "Established")
                        entries = entries.Where(x => x.State?.Equals("ESTABLISHED", StringComparison.OrdinalIgnoreCase) == true).ToList();
                }

                if (InvokeRequired)
                    Invoke(() => PopulateGrid(entries));
                else
                    PopulateGrid(entries);
            }
            catch (Exception ex)
            {
                if (InvokeRequired)
                    Invoke(new Action(() => explanationBox.Text = "Refresh failed: " + ex.Message));
                else
                    explanationBox.Text = "Refresh failed: " + ex.Message;
            }
        }

        private void PopulateGrid(List<NetstatEntry> entries)
        {
            portGrid.Rows.Clear();
            foreach (var e in entries)
            {
                portGrid.Rows.Add(
                    e.LocalPort,
                    e.Protocol,
                    e.State ?? "",
                    e.LocalIP,
                    e.LocalPort,
                    e.RemoteIP,
                    e.RemotePort,
                    e.PID?.ToString() ?? "",
                    e.ProcessName ?? "Unknown",
                    e.ProcessPath ?? "",
                    e.ServiceName ?? ""
                );
            }
        }

        private List<NetstatEntry> GetNetstatEntries()
        {
            var results = new List<NetstatEntry>();
            var psi = new ProcessStartInfo("netstat", "-ano")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi)!)
            {
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(l => l.Trim())
                                  .Where(l => !string.IsNullOrWhiteSpace(l))
                                  .ToList();

                foreach (var line in lines)
                {
                    var m = netstatLineRegex.Match(line);
                    if (!m.Success) continue;

                    string protocol = m.Groups[1].Value;
                    string localIP = m.Groups[2].Value;
                    string localPort = m.Groups[3].Value;
                    string remoteIP = m.Groups[4].Value;
                    string remotePort = m.Groups[5].Value;
                    string state = m.Groups[6].Success ? m.Groups[6].Value : (protocol.Equals("UDP", StringComparison.OrdinalIgnoreCase) ? "NONE" : "");
                    int? pid = m.Groups[7].Success ? int.Parse(m.Groups[7].Value) : (int?)null;

                    var entry = new NetstatEntry
                    {
                        Protocol = protocol,
                        LocalIP = localIP,
                        LocalPort = localPort,
                        RemoteIP = remoteIP,
                        RemotePort = remotePort,
                        State = state,
                        PID = pid
                    };

                    if (pid.HasValue)
                    {
                        try
                        {
                            var proc = Process.GetProcessById(pid.Value);
                            entry.ProcessName = proc.ProcessName;
                            try { entry.ProcessPath = proc.MainModule?.FileName ?? ""; }
                            catch { entry.ProcessPath = ""; }
                            try { entry.ServiceName = QueryServiceNameByPid(pid.Value); }
                            catch { }
                        }
                        catch
                        {
                            entry.ProcessName = "Unknown";
                            entry.ProcessPath = "";
                        }
                    }

                    results.Add(entry);
                }
            }

            return results.OrderBy(r =>
            {
                if (int.TryParse(r.LocalPort, out int p)) return p;
                return int.MaxValue;
            }).ToList();
        }

        private string QueryServiceNameByPid(int pid)
        {
            try
            {
                string query = $"SELECT Name, DisplayName FROM Win32_Service WHERE ProcessId = {pid}";
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject svc in searcher.Get())
                        return svc["DisplayName"]?.ToString() ?? svc["Name"]?.ToString() ?? "";
                }
            }
            catch { }
            return "";
        }

        private void PortGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = portGrid.Rows[e.RowIndex];
                string pidStr = row.Cells["PID"].Value?.ToString() ?? "";
                if (int.TryParse(pidStr, out int pid))
                {
                    try
                    {
                        var proc = Process.GetProcessById(pid);
                        MessageBox.Show(
                            $"PID: {pid}\r\nProcess: {proc.ProcessName}\r\nPath: {proc.MainModule?.FileName ?? "N/A"}",
                            "Process Info",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot obtain process info: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(this))
            {
                settingsForm.ShowDialog();
            }
        }

        public void ApplyColorScheme(string scheme)
        {
            switch (scheme)
            {
                case "Light":
                    BackColor = System.Drawing.Color.White;
                    lblCpu.ForeColor = lblRam.ForeColor = System.Drawing.Color.Black;
                    portGrid.BackgroundColor = System.Drawing.Color.White;
                    explanationBox.BackColor = System.Drawing.Color.White;
                    explanationBox.ForeColor = System.Drawing.Color.Black;
                    break;
                case "Dark":
                    BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
                    lblCpu.ForeColor = lblRam.ForeColor = System.Drawing.Color.White;
                    portGrid.BackgroundColor = System.Drawing.Color.FromArgb(50, 50, 50);
                    explanationBox.BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
                    explanationBox.ForeColor = System.Drawing.Color.White;
                    break;
                case "Blue":
                    BackColor = System.Drawing.Color.LightBlue;
                    lblCpu.ForeColor = lblRam.ForeColor = System.Drawing.Color.DarkBlue;
                    portGrid.BackgroundColor = System.Drawing.Color.AliceBlue;
                    explanationBox.BackColor = System.Drawing.Color.AliceBlue;
                    explanationBox.ForeColor = System.Drawing.Color.DarkBlue;
                    break;
                case "Green":
                    BackColor = System.Drawing.Color.LightGreen;
                    lblCpu.ForeColor = lblRam.ForeColor = System.Drawing.Color.DarkGreen;
                    portGrid.BackgroundColor = System.Drawing.Color.Honeydew;
                    explanationBox.BackColor = System.Drawing.Color.Honeydew;
                    explanationBox.ForeColor = System.Drawing.Color.DarkGreen;
                    break;
                case "High Contrast":
                    BackColor = System.Drawing.Color.Black;
                    lblCpu.ForeColor = lblRam.ForeColor = System.Drawing.Color.Yellow;
                    portGrid.BackgroundColor = System.Drawing.Color.Black;
                    explanationBox.BackColor = System.Drawing.Color.Black;
                    explanationBox.ForeColor = System.Drawing.Color.Yellow;
                    break;
            }
        }

        public void ExportGridToCsv()
        {
            if (portGrid.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv";
                sfd.FileName = "PortReaderLog.csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var writer = new System.IO.StreamWriter(sfd.FileName))
                        {
                            var headers = portGrid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText);
                            writer.WriteLine(string.Join(",", headers));

                            foreach (DataGridViewRow row in portGrid.Rows)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>().Select(c => $"\"{c.Value}\"");
                                writer.WriteLine(string.Join(",", cells));
                            }
                        }
                        MessageBox.Show("Export complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to export CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private class NetstatEntry
        {
            public string Protocol { get; set; } = "";
            public string LocalIP { get; set; } = "";
            public string LocalPort { get; set; } = "";
            public string RemoteIP { get; set; } = "";
            public string RemotePort { get; set; } = "";
            public string? State { get; set; }
            public int? PID { get; set; }
            public string? ProcessName { get; set; }
            public string? ProcessPath { get; set; }
            public string? ServiceName { get; set; }
        }
    }
}
