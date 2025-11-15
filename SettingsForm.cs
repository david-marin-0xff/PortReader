using System;
using System.Windows.Forms;

namespace PortReader
{
    public class SettingsForm : Form
    {
        private Form1 mainForm;

        private Label lblFilter = new Label();
        private ComboBox cmbFilter = new ComboBox();

        private Label lblColor = new Label();
        private ComboBox cmbColorScheme = new ComboBox();

        private Button btnExport = new Button();
        private Button btnClose = new Button();

        public SettingsForm(Form1 form)
        {
            mainForm = form;

            Text = "Settings";
            Width = 300;
            Height = 220;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            BuildLayout();
        }

        private void BuildLayout()
        {
            // Filter Label
            lblFilter.Text = "Filter:";
            lblFilter.Left = 10;
            lblFilter.Top = 20;
            lblFilter.AutoSize = true;
            Controls.Add(lblFilter);

            // Filter ComboBox
            cmbFilter.Left = 100;
            cmbFilter.Top = 15;
            cmbFilter.Width = 150;
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Items.AddRange(new object[] { "All", "TCP", "UDP", "Listening", "Established" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
            Controls.Add(cmbFilter);

            // Color Label
            lblColor.Text = "Color Scheme:";
            lblColor.Left = 10;
            lblColor.Top = 60;
            lblColor.AutoSize = true;
            Controls.Add(lblColor);

            // Color ComboBox
            cmbColorScheme.Left = 100;
            cmbColorScheme.Top = 55;
            cmbColorScheme.Width = 150;
            cmbColorScheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbColorScheme.Items.AddRange(new object[] { "Light", "Dark", "Blue", "Green", "High Contrast" });
            cmbColorScheme.SelectedIndex = 0;
            cmbColorScheme.SelectedIndexChanged += CmbColorScheme_SelectedIndexChanged;
            Controls.Add(cmbColorScheme);

            // Export Button
            btnExport.Text = "Export CSV";
            btnExport.Left = 30;
            btnExport.Top = 110;
            btnExport.Width = 100;
            btnExport.Click += BtnExport_Click;
            Controls.Add(btnExport);

            // Close Button
            btnClose.Text = "Close";
            btnClose.Left = 150;
            btnClose.Top = 110;
            btnClose.Width = 100;
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }

        private void CmbFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (mainForm != null)
            {
                mainForm.Invoke(async () =>
                {
                    mainForm.ViewFilter = cmbFilter.SelectedItem?.ToString() ?? "All";
                    await mainForm.RefreshPortsAsync();
                });
            }
        }

        private void CmbColorScheme_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (mainForm != null)
            {
                mainForm.ApplyColorScheme(cmbColorScheme.SelectedItem?.ToString() ?? "Light");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (mainForm != null)
            {
                mainForm.ExportGridToCsv();
            }
        }
    }
}
