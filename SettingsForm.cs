using System;
using System.Windows.Forms;

namespace OnDisplayOff
{
    /// <summary>
    /// Settings dialog form that allows users to configure application behavior.
    /// Provides controls for grace period, power action, startup settings, and pause state.
    /// </summary>
    public sealed class SettingsForm : Form
    {
        /// <summary>Numeric input for grace period value (no maximum limit)</summary>
        private NumericUpDown nudGrace = new() { Minimum = 0, Maximum = int.MaxValue, Value = 60, Width = 100 };

        /// <summary>Dropdown for selecting time unit (milliseconds, seconds, minutes, hours, days)</summary>
        private ComboBox cbTimeUnit = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        
        /// <summary>Dropdown for selecting power action (Sleep/Hibernate/Shutdown/Restart)</summary>
        private ComboBox cbAction = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        
        /// <summary>Checkbox to enable/disable starting application at Windows logon</summary>
        private CheckBox chkStartup = new() { Text = "Start at logon" };
        
        /// <summary>Checkbox to pause/resume application functionality</summary>
        private CheckBox chkPaused  = new() { Text = "Paused" };
        
        /// <summary>OK button to save changes</summary>
        private Button btnOK = new() { Text = "OK", DialogResult = DialogResult.OK };
        
        /// <summary>Cancel button to discard changes</summary>
        private Button btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

        /// <summary>
        /// Gets the configured settings if the user clicked OK, null if cancelled.
        /// </summary>
        public AppSettings? Result { get; private set; }

        /// <summary>
        /// Initializes the settings form with current application settings.
        /// </summary>
        /// <param name="current">Current settings to populate the form controls</param>
        public SettingsForm(AppSettings current)
        {
            // Configure form appearance
            Text = "OnDisplayOff — Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = MinimizeBox = false;

            // Populate power action dropdown with enum values
            cbAction.Items.AddRange(Enum.GetNames(typeof(SleepAction)));
            cbAction.SelectedItem = current.Action.ToString();
            
            // Populate time unit dropdown with enum values
            cbTimeUnit.Items.AddRange(Enum.GetNames(typeof(TimeUnit)));
            cbTimeUnit.SelectedItem = current.GraceTimeUnit.ToString();

            // Set grace period value
            nudGrace.Value = Math.Max(current.GraceValue, 0);
            
            // Set checkbox states from current settings
            chkStartup.Checked = current.StartAtLogon;
            chkPaused.Checked = current.Paused;

            // Create table layout for form controls
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 5 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Add controls to table layout
            table.Controls.Add(new Label(){Text="Grace period:", AutoSize=true}, 0, 0);

            // Create a panel for grace period controls (value + unit)
            var gracePanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = Padding.Empty };
            gracePanel.Controls.Add(nudGrace);
            gracePanel.Controls.Add(cbTimeUnit);
            table.Controls.Add(gracePanel, 1, 0);
            table.Controls.Add(new Label(){Text="Action:", AutoSize=true}, 0, 1);
            table.Controls.Add(cbAction, 1, 1);
            table.Controls.Add(chkStartup, 1, 2);
            table.Controls.Add(chkPaused, 1, 3);

            // Create button panel (right-aligned)
            var flow = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            flow.Controls.Add(btnOK);
            flow.Controls.Add(btnCancel);
            table.Controls.Add(flow, 0, 4);
            table.SetColumnSpan(flow, 2);
            Controls.Add(table);

            // Set default buttons for Enter/Escape keys
            AcceptButton = btnOK; 
            CancelButton = btnCancel;

            // Handle OK button click - create new settings object from form values
            btnOK.Click += (_,__) =>
            {
                Result = new AppSettings
                {
                    GraceValue = (int)nudGrace.Value,
                    GraceTimeUnit = Enum.Parse<TimeUnit>(cbTimeUnit.SelectedItem!.ToString()!),
                    Action = Enum.Parse<SleepAction>(cbAction.SelectedItem!.ToString()!),
                    StartAtLogon = chkStartup.Checked,
                    Paused = chkPaused.Checked
                };
            };
        }
    }
}
