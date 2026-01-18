using HyperView.Class;
using HyperView.Forms;
using System.Data;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using static HyperView.Class.FileLogger;

namespace HyperView
{
    public partial class MainForm : Form
    {
        private PSObject _psSession = null;
        private Runspace _persistentRunspace = null;

        public MainForm()
        {
            InitializeComponent();
            InitializeSession();
            LoadVMOverview();
        }

        private void InitializeSession()
        {
            try
            {
                // Check if we have an active session
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active session found. Please login again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                // For remote connections, create a persistent runspace and PS session
                if (!SessionContext.IsLocal)
                {
                    // Create persistent runspace for remote operations
                    _persistentRunspace = RunspaceFactory.CreateRunspace();
                    _persistentRunspace.Open();

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _persistentRunspace;

                        // Build New-PSSession command
                        ps.AddCommand("New-PSSession")
                          .AddParameter("ComputerName", SessionContext.ServerName)
                          .AddParameter("ErrorAction", "Stop");

                        if (SessionContext.Credentials != null)
                        {
                            ps.AddParameter("Credential", SessionContext.Credentials);
                        }

                        var sessionResult = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var error = ps.Streams.Error[0];
                            MessageBox.Show($"Failed to create PowerShell session: {error.Exception.Message}",
                                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.DialogResult = DialogResult.Cancel;
                            this.Close();
                            return;
                        }

                        if (sessionResult != null && sessionResult.Count > 0)
                        {
                            _psSession = sessionResult[0];
                            FileLogger.Message($"Remote PowerShell session created for '{SessionContext.ServerName}'",
                                FileLogger.EventType.Information, 2002);
                        }
                    }
                }

                // Update form title with connection info
                this.Text = $"{Globals.ToolName.HyperView} - Connected to {SessionContext.ServerName} ({SessionContext.ConnectionType})";
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to initialize session: {ex.Message}",
                    FileLogger.EventType.Error, 2003);
                MessageBox.Show($"Failed to initialize session: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Only perform cleanup if the form is actually closing (not cancelled by user)
            if (e.Cancel)
            {
                FileLogger.Message("Form closing cancelled - skipping cleanup",
                    FileLogger.EventType.Information, 2014);
                return;
            }

            // Clean up remote session if exists
            if (_psSession != null && _persistentRunspace != null)
            {
                try
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _persistentRunspace;
                        ps.AddCommand("Remove-PSSession")
                          .AddParameter("Session", _psSession);
                        ps.Invoke();
                    }
                    FileLogger.Message("Remote PowerShell session closed",
                        FileLogger.EventType.Information, 2004);
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Error closing PS session: {ex.Message}",
                        FileLogger.EventType.Warning, 2005);
                }
            }

            // Dispose persistent runspace
            if (_persistentRunspace != null)
            {
                try
                {
                    _persistentRunspace.Close();
                    _persistentRunspace.Dispose();
                    FileLogger.Message("Persistent runspace closed",
                        FileLogger.EventType.Information, 2009);
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Error closing persistent runspace: {ex.Message}",
                        FileLogger.EventType.Warning, 2010);
                }
            }
        }

        private void LoadVMOverview()
        {
            try
            {
                // Clear existing data
                datagridviewVMOverView.DataSource = null;
                datagridviewVMOverView.Rows.Clear();
                datagridviewVMOverView.Columns.Clear();

                var results = ExecutePowerShellCommand("Get-VM");

                if (results == null || results.Count == 0)
                {
                    MessageBox.Show("No VMs found.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Create DataTable with enhanced columns
                var dataTable = new DataTable();
                dataTable.Columns.Add("VM Name");
                dataTable.Columns.Add("State");
                dataTable.Columns.Add("CPU Count");
                dataTable.Columns.Add("CPU Usage %");
                dataTable.Columns.Add("Memory Assigned (MB)");
                dataTable.Columns.Add("Memory Demand (MB)");
                dataTable.Columns.Add("Memory Startup (MB)");
                dataTable.Columns.Add("Dynamic Memory");
                dataTable.Columns.Add("Total Disk (GB)");
                dataTable.Columns.Add("Network Adapters");
                dataTable.Columns.Add("Generation");
                dataTable.Columns.Add("Uptime");
                dataTable.Columns.Add("Heartbeat");
                dataTable.Columns.Add("Integration Services");
                dataTable.Columns.Add("Auto Start");
                dataTable.Columns.Add("Auto Stop");
                dataTable.Columns.Add("VM Groups");
                dataTable.Columns.Add("Checkpoint Type");
                dataTable.Columns.Add("Checkpoints");
                dataTable.Columns.Add("Replication");
                dataTable.Columns.Add("Created");
                dataTable.Columns.Add("Is Clustered");
                dataTable.Columns.Add("Categories");

                foreach (var vm in results)
                {
                    var row = dataTable.NewRow();
                    row["VM Name"] = vm.Properties["Name"]?.Value?.ToString() ?? "";
                    row["State"] = vm.Properties["State"]?.Value?.ToString() ?? "";
                    row["CPU Count"] = vm.Properties["ProcessorCount"]?.Value?.ToString() ?? "";
                    row["CPU Usage %"] = vm.Properties["CPUUsage"]?.Value?.ToString() ?? "";

                    // Memory values - convert from bytes to MB
                    var memAssigned = vm.Properties["MemoryAssigned"]?.Value;
                    row["Memory Assigned (MB)"] = memAssigned != null ? ((long)memAssigned / 1048576).ToString() : "";

                    var memDemand = vm.Properties["MemoryDemand"]?.Value;
                    row["Memory Demand (MB)"] = memDemand != null ? ((long)memDemand / 1048576).ToString() : "";

                    var memStartup = vm.Properties["MemoryStartup"]?.Value;
                    row["Memory Startup (MB)"] = memStartup != null ? ((long)memStartup / 1048576).ToString() : "";

                    var dynamicMem = vm.Properties["DynamicMemoryEnabled"]?.Value;
                    row["Dynamic Memory"] = dynamicMem != null && (bool)dynamicMem ? "Yes" : "No";

                    // Get hard drive info
                    var vmName = vm.Properties["Name"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(vmName))
                    {
                        var totalDiskGB = GetVMTotalDiskSize(vmName);
                        row["Total Disk (GB)"] = totalDiskGB > 0 ? totalDiskGB.ToString("F2") : "";

                        var networkAdapterCount = GetVMNetworkAdapterCount(vmName);
                        row["Network Adapters"] = networkAdapterCount.ToString();
                    }
                    else
                    {
                        row["Total Disk (GB)"] = "";
                        row["Network Adapters"] = "";
                    }

                    row["Generation"] = vm.Properties["Generation"]?.Value?.ToString() ?? "";

                    // Format uptime
                    var uptime = vm.Properties["Uptime"]?.Value;
                    row["Uptime"] = uptime != null ? FormatTimeSpan((TimeSpan)uptime) : "";

                    row["Heartbeat"] = vm.Properties["Heartbeat"]?.Value?.ToString() ?? "";

                    // Integration services version
                    var integrationServicesVersion = vm.Properties["IntegrationServicesVersion"]?.Value;
                    row["Integration Services"] = integrationServicesVersion?.ToString() ?? "";

                    row["Auto Start"] = vm.Properties["AutomaticStartAction"]?.Value?.ToString() ?? "";
                    row["Auto Stop"] = vm.Properties["AutomaticStopAction"]?.Value?.ToString() ?? "";

                    // Get VM groups
                    if (!string.IsNullOrEmpty(vmName))
                    {
                        var groups = GetVMGroups(vmName);
                        row["VM Groups"] = groups;
                    }
                    else
                    {
                        row["VM Groups"] = "";
                    }

                    row["Checkpoint Type"] = vm.Properties["CheckpointType"]?.Value?.ToString() ?? "";

                    // Get checkpoint count
                    if (!string.IsNullOrEmpty(vmName))
                    {
                        var checkpointCount = GetVMCheckpointCount(vmName);
                        row["Checkpoints"] = checkpointCount.ToString();
                    }
                    else
                    {
                        row["Checkpoints"] = "";
                    }

                    var replicationState = vm.Properties["ReplicationState"]?.Value?.ToString();
                    row["Replication"] = !string.IsNullOrEmpty(replicationState) ? replicationState : "Not Configured";

                    var creationTime = vm.Properties["CreationTime"]?.Value;
                    row["Created"] = creationTime != null ? ((DateTime)creationTime).ToString("yyyy-MM-dd HH:mm") : "";

                    var isClustered = vm.Properties["IsClustered"]?.Value;
                    row["Is Clustered"] = isClustered != null && (bool)isClustered ? "Yes" : "No";

                    row["Categories"] = "";

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewVMOverView.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewVMOverView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewVMOverView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewVMOverView.MultiSelect = false;
                datagridviewVMOverView.ReadOnly = true;
                datagridviewVMOverView.AllowUserToAddRows = false;
                datagridviewVMOverView.AllowUserToDeleteRows = false;
                datagridviewVMOverView.RowHeadersVisible = false;

                // Apply color coding
                ApplyColorCoding();
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error loading VM overview: {ex.Message}",
                    FileLogger.EventType.Error, 2006);
                MessageBox.Show($"Error loading VM overview: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private System.Collections.ObjectModel.Collection<PSObject> ExecutePowerShellCommand(string command, Dictionary<string, object> parameters = null)
        {
            try
            {
                if (SessionContext.IsLocal)
                {
                    // Local execution - create new runspace for each command
                    using (Runspace runspace = RunspaceFactory.CreateRunspace())
                    {
                        runspace.Open();

                        using (PowerShell ps = PowerShell.Create())
                        {
                            ps.Runspace = runspace;
                            ps.AddScript(command);

                            var results = ps.Invoke();

                            if (ps.HadErrors)
                            {
                                var errors = string.Join(Environment.NewLine,
                                    ps.Streams.Error.Select(e => e.ToString()));
                                FileLogger.Message($"PowerShell command '{command}' errors: {errors}",
                                    FileLogger.EventType.Error, 2007);
                                return null;
                            }

                            return results;
                        }
                    }
                }
                else
                {
                    // Remote execution - use persistent runspace with the session
                    if (_persistentRunspace == null || _persistentRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
                    {
                        FileLogger.Message("Persistent runspace is not available or not opened",
                            FileLogger.EventType.Error, 2011);
                        return null;
                    }

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _persistentRunspace;

                        // Remote execution via Invoke-Command
                        ps.AddCommand("Invoke-Command")
                          .AddParameter("Session", _psSession)
                          .AddParameter("ScriptBlock", ScriptBlock.Create(command));

                        var results = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var errors = string.Join(Environment.NewLine,
                                ps.Streams.Error.Select(e => e.ToString()));
                            FileLogger.Message($"PowerShell command '{command}' errors: {errors}",
                                FileLogger.EventType.Error, 2007);
                            return null;
                        }

                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error executing PowerShell command '{command}': {ex.Message}",
                    FileLogger.EventType.Error, 2008);
                return null;
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            else
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        private double GetVMTotalDiskSize(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMHardDiskDrive -VMName '{vmName}'");

                if (results == null || results.Count == 0)
                    return 0;

                double totalGB = 0;
                foreach (var hdd in results)
                {
                    var path = hdd.Properties["Path"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        // For remote, we need to get file size through PS
                        if (!SessionContext.IsLocal)
                        {
                            var sizeResult = ExecutePowerShellCommand($"(Get-Item '{path}').Length");
                            if (sizeResult != null && sizeResult.Count > 0)
                            {
                                var size = Convert.ToInt64(sizeResult[0].BaseObject);
                                totalGB += size / (1024.0 * 1024.0 * 1024.0);
                            }
                        }
                        else if (System.IO.File.Exists(path))
                        {
                            var fileInfo = new System.IO.FileInfo(path);
                            totalGB += fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                        }
                    }
                }

                return totalGB;
            }
            catch
            {
                return 0;
            }
        }

        private int GetVMNetworkAdapterCount(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMNetworkAdapter -VMName '{vmName}'");
                return results?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetVMGroups(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMGroup | Where-Object {{ $_.VMMembers.Name -contains '{vmName}' }} | Select-Object -ExpandProperty Name");

                if (results != null && results.Count > 0)
                {
                    return string.Join(", ", results.Select(g => g.ToString()));
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        private int GetVMCheckpointCount(string vmName)
        {
            try
            {
                var results = ExecutePowerShellCommand($"Get-VMSnapshot -VMName '{vmName}'");
                return results?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private void ApplyColorCoding()
        {
            foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
            {
                // Color code VM State
                if (row.Cells["State"] != null && row.Cells["State"].Value != null)
                {
                    var state = row.Cells["State"].Value.ToString();
                    switch (state)
                    {
                        case "Running":
                            row.Cells["State"].Style.BackColor = Color.LightGreen;
                            row.Cells["State"].Style.ForeColor = Color.DarkGreen;
                            break;
                        case "Off":
                            row.Cells["State"].Style.BackColor = Color.LightCoral;
                            row.Cells["State"].Style.ForeColor = Color.DarkRed;
                            break;
                        case "Paused":
                            row.Cells["State"].Style.BackColor = Color.LightYellow;
                            row.Cells["State"].Style.ForeColor = Color.DarkOrange;
                            break;
                        case "Saved":
                            row.Cells["State"].Style.BackColor = Color.LightBlue;
                            row.Cells["State"].Style.ForeColor = Color.DarkBlue;
                            break;
                    }
                }

                // Color code Heartbeat status
                if (row.Cells["Heartbeat"] != null && row.Cells["Heartbeat"].Value != null)
                {
                    var heartbeat = row.Cells["Heartbeat"].Value.ToString();
                    if (heartbeat.Contains("Ok"))
                    {
                        row.Cells["Heartbeat"].Style.BackColor = Color.LightGreen;
                    }
                    else if (heartbeat == "Unknown")
                    {
                        row.Cells["Heartbeat"].Style.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        row.Cells["Heartbeat"].Style.BackColor = Color.LightCoral;
                    }
                }

                // Color code Dynamic Memory
                if (row.Cells["Dynamic Memory"] != null && row.Cells["Dynamic Memory"].Value != null)
                {
                    var dynMem = row.Cells["Dynamic Memory"].Value.ToString();
                    if (dynMem == "Yes")
                    {
                        row.Cells["Dynamic Memory"].Style.BackColor = Color.LightBlue;
                    }
                }

                // Color code Replication status
                if (row.Cells["Replication"] != null && row.Cells["Replication"].Value != null)
                {
                    var replication = row.Cells["Replication"].Value.ToString();
                    if (replication != "Not Configured")
                    {
                        row.Cells["Replication"].Style.BackColor = Color.LightGreen;
                    }
                }
            }
        }

        private bool ConfirmDisconnectAndExit()
        {
            // Check if there's an active Hyper-V connection
            if (SessionContext.IsSessionActive())
            {
                FileLogger.Message("User is attempting to close the application with active connection",
                    FileLogger.EventType.Information, 2012);

                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}') and close the application?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    FileLogger.Message($"User confirmed exit - disconnecting from Hyper-V server '{SessionContext.ServerName}'...",
                        FileLogger.EventType.Information, 2013);

                    // Cleanup will be handled by OnFormClosing
                    return true;
                }
                else
                {
                    // User cancelled
                    FileLogger.Message("User cancelled exit - keeping application open",
                        FileLogger.EventType.Information, 2015);
                    return false;
                }
            }
            else
            {
                // No active connection - allow close
                FileLogger.Message("No active connection - proceeding with exit",
                    FileLogger.EventType.Information, 2016);
                return true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Use the shared confirmation function
            if (!ConfirmDisconnectAndExit())
            {
                e.Cancel = true;
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message($"User initiated disconnect from '{SessionContext.ServerName}'",
                    FileLogger.EventType.Information, 2017);

                // Check if there's an active connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("No active connection to disconnect.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show confirmation dialog
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}')?",
                    "Confirm Disconnect",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    FileLogger.Message($"User confirmed disconnect from Hyper-V server '{SessionContext.ServerName}'",
                        FileLogger.EventType.Information, 2018);

                    // Disable disconnect menu item during operation to prevent double-clicks
                    disconnectToolStripMenuItem.Enabled = false;

                    // Store server name before clearing
                    string disconnectedServer = SessionContext.ServerName;

                    // Clean up remote session if exists
                    if (_psSession != null && _persistentRunspace != null)
                    {
                        try
                        {
                            using (PowerShell ps = PowerShell.Create())
                            {
                                ps.Runspace = _persistentRunspace;
                                ps.AddCommand("Remove-PSSession")
                                  .AddParameter("Session", _psSession);
                                ps.Invoke();
                            }
                            FileLogger.Message("Remote PowerShell session closed during disconnect",
                                FileLogger.EventType.Information, 2019);
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Error closing PS session during disconnect: {ex.Message}",
                                FileLogger.EventType.Warning, 2020);
                        }
                    }

                    // Dispose persistent runspace
                    if (_persistentRunspace != null)
                    {
                        try
                        {
                            _persistentRunspace.Close();
                            _persistentRunspace.Dispose();
                            _persistentRunspace = null;
                            FileLogger.Message("Persistent runspace closed during disconnect",
                                FileLogger.EventType.Information, 2021);
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Error closing persistent runspace during disconnect: {ex.Message}",
                                FileLogger.EventType.Warning, 2022);
                        }
                    }

                    // Clear PS session reference
                    _psSession = null;

                    // Clear global connection data (SessionContext)
                    SessionContext.Clear();

                    FileLogger.Message($"Successfully disconnected from Hyper-V server '{disconnectedServer}'",
                        FileLogger.EventType.Information, 2023);

                    // Hide main form temporarily
                    this.Hide();

                    FileLogger.Message("Showing login form for reconnection...",
                        FileLogger.EventType.Information, 2024);

                    // Show login form for reconnection
                    using (Forms.LoginForm loginForm = new Forms.LoginForm())
                    {
                        var loginResult = loginForm.ShowDialog();

                        if (loginResult == DialogResult.OK && loginForm.Result != null && loginForm.Result.Success)
                        {
                            // Authentication successful - close current form and open new MainForm
                            FileLogger.Message($"Reconnected successfully to '{loginForm.Result.ServerName}'",
                                FileLogger.EventType.Information, 2025);

                            // Close current MainForm (will trigger cleanup)
                            this.DialogResult = DialogResult.OK;
                            this.Close();

                            // The LoginForm will handle showing the new MainForm
                        }
                        else
                        {
                            // Authentication failed or cancelled - close application
                            FileLogger.Message("Authentication cancelled after disconnect - closing application",
                                FileLogger.EventType.Information, 2026);

                            this.DialogResult = DialogResult.Cancel;
                            this.Close();
                        }
                    }
                }
                else
                {
                    // User cancelled disconnect
                    FileLogger.Message("User cancelled disconnect operation",
                        FileLogger.EventType.Information, 2027);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error during disconnect: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2028);

                // Re-enable disconnect menu item so user can try again
                disconnectToolStripMenuItem.Enabled = true;

                // Show error to user
                MessageBox.Show($"Failed to disconnect from Hyper-V:\n\n{ex.Message}",
                    "Disconnect Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // Show the form again if it was hidden
                this.Show();
            }
        }

        private void buttonCreateANewVMGroup_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User initiated VM Group creation",
                    FileLogger.EventType.Information, 2029);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Show the CreateVMGroupForm
                using (Forms.CreateVMGroupForm createGroupForm = new Forms.CreateVMGroupForm())
                {
                    var result = createGroupForm.ShowDialog();

                    if (result == DialogResult.OK && createGroupForm.Result != null)
                    {
                        string groupName = createGroupForm.Result.GroupName;
                        string groupType = createGroupForm.Result.GroupType;

                        FileLogger.Message($"Creating VM Group '{groupName}' of type '{groupType}'...",
                            FileLogger.EventType.Information, 2030);

                        // Create the VM Group using PowerShell
                        var createResult = VMGroups.CreateHyperVVMGroup(groupName, groupType, cmd => ExecutePowerShellCommand(cmd));

                        if (createResult.Success)
                        {
                            FileLogger.Message($"VM Group '{groupName}' created successfully",
                                FileLogger.EventType.Information, 2031);

                            MessageBox.Show($"VM Group '{groupName}' created successfully.",
                                "Group Created",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh VM Groups view
                            VMGroups.RefreshVMGroupsView(
                                $"New group created: {groupName}",
                                cmd => ExecutePowerShellCommand(cmd),
                                groups => UpdateVMGroupsDataGridView(groups));
                        }
                        else
                        {
                            FileLogger.Message($"Failed to create VM Group '{groupName}': {createResult.Error}",
                                FileLogger.EventType.Error, 2032);

                            MessageBox.Show($"Failed to create VM Group:\n\n{createResult.Error}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        FileLogger.Message("VM Group creation cancelled",
                            FileLogger.EventType.Information, 2033);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error creating VM Group: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2034);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonDeleteSelectedVMGrou_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User initiated VM Group deletion",
                    FileLogger.EventType.Information, 2039);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group (assuming you have a datagridviewVMGroups control)
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a VM Group to delete.",
                        "No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string selectedGroupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(selectedGroupName))
                {
                    MessageBox.Show("Invalid VM Group selection.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                FileLogger.Message($"User selected VM Group '{selectedGroupName}' for deletion",
                    FileLogger.EventType.Information, 2040);

                // First confirmation
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete VM Group '{selectedGroupName}'?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes)
                {
                    FileLogger.Message("VM Group deletion cancelled by user",
                        FileLogger.EventType.Information, 2041);
                    return;
                }

                // Try to remove without force first
                var result = VMGroups.RemoveHyperVVMGroup(selectedGroupName, false, cmd => ExecutePowerShellCommand(cmd));

                if (result.Success)
                {
                    FileLogger.Message($"VM Group '{selectedGroupName}' deleted successfully",
                        FileLogger.EventType.Information, 2042);

                    MessageBox.Show($"VM Group '{selectedGroupName}' deleted successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Refresh the VM Groups view
                    VMGroups.RefreshVMGroupsView(
                        $"Group deleted: {selectedGroupName}",
                        cmd => ExecutePowerShellCommand(cmd),
                        groups => UpdateVMGroupsDataGridView(groups));
                }
                else
                {
                    // Check if it's because the group contains VMs and can be forced
                    if (result.CanForce)
                    {
                        FileLogger.Message($"VM Group '{selectedGroupName}' contains {result.VMCount} VM(s), asking for force deletion",
                            FileLogger.EventType.Information, 2043);

                        string vmList = string.Join("\n• ", result.VMNames);
                        string forceMessage = $"VM Group '{selectedGroupName}' contains {result.VMCount} VM(s):\n\n• {vmList}\n\n" +
                                            "The VMs will remain but will be removed from this group.\n\n" +
                                            "Do you want to force delete the VM Group anyway?";

                        var forceResult = MessageBox.Show(forceMessage,
                            "Force Delete VM Group?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation);

                        if (forceResult == DialogResult.Yes)
                        {
                            FileLogger.Message($"User confirmed force deletion of VM Group '{selectedGroupName}'",
                                FileLogger.EventType.Information, 2044);

                            // Try again with force
                            var forceDeleteResult = VMGroups.RemoveHyperVVMGroup(selectedGroupName, true, cmd => ExecutePowerShellCommand(cmd));

                            if (forceDeleteResult.Success)
                            {
                                FileLogger.Message($"VM Group '{selectedGroupName}' force deleted successfully",
                                    FileLogger.EventType.Information, 2045);

                                MessageBox.Show($"VM Group '{selectedGroupName}' force deleted successfully. " +
                                              "The VMs remain but are no longer part of this group.",
                                    "Success",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                                // Refresh the VM Groups view
                                VMGroups.RefreshVMGroupsView(
                                    $"Group force deleted: {selectedGroupName}",
                                    cmd => ExecutePowerShellCommand(cmd),
                                    groups => UpdateVMGroupsDataGridView(groups));
                            }
                            else
                            {
                                FileLogger.Message($"Failed to force delete VM Group '{selectedGroupName}': {forceDeleteResult.Error}",
                                    FileLogger.EventType.Error, 2046);

                                MessageBox.Show($"Failed to force delete VM Group:\n\n{forceDeleteResult.Error}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            FileLogger.Message("User cancelled force deletion",
                                FileLogger.EventType.Information, 2047);
                        }
                    }
                    else
                    {
                        // Other error
                        FileLogger.Message($"Failed to delete VM Group '{selectedGroupName}': {result.Error}",
                            FileLogger.EventType.Error, 2048);

                        MessageBox.Show($"Failed to delete VM Group:\n\n{result.Error}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error deleting VM Group: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2049);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonRenameSelectedVMGrou_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User initiated VM Group rename",
                    FileLogger.EventType.Information, 2094);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a VM Group to rename.",
                        "No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string currentGroupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(currentGroupName))
                {
                    MessageBox.Show("Invalid VM Group selection.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                FileLogger.Message($"User selected VM Group '{currentGroupName}' for rename",
                    FileLogger.EventType.Information, 2095);

                // Show rename form
                using (Forms.RenameVMGroupForm renameForm = new Forms.RenameVMGroupForm())
                {
                    renameForm.CurrentGroupName = currentGroupName;

                    var result = renameForm.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrEmpty(renameForm.NewGroupName))
                    {
                        string newGroupName = renameForm.NewGroupName;

                        FileLogger.Message($"Renaming VM Group from '{currentGroupName}' to '{newGroupName}'...",
                            FileLogger.EventType.Information, 2096);

                        // Rename the VM Group using PowerShell
                        var renameResult = VMGroups.RenameHyperVVMGroup(
                            currentGroupName,
                            newGroupName,
                            cmd => ExecutePowerShellCommand(cmd));

                        if (renameResult.Success)
                        {
                            FileLogger.Message($"VM Group renamed successfully from '{currentGroupName}' to '{newGroupName}'",
                                FileLogger.EventType.Information, 2097);

                            MessageBox.Show($"VM Group renamed successfully from '{currentGroupName}' to '{newGroupName}'.",
                                "Group Renamed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Refresh VM Groups view
                            VMGroups.RefreshVMGroupsView(
                                $"Group renamed: {currentGroupName} -> {newGroupName}",
                                cmd => ExecutePowerShellCommand(cmd),
                                groups => UpdateVMGroupsDataGridView(groups));
                        }
                        else
                        {
                            FileLogger.Message($"Failed to rename VM Group '{currentGroupName}': {renameResult.Error}",
                                FileLogger.EventType.Error, 2098);

                            MessageBox.Show($"Failed to rename VM Group:\n\n{renameResult.Error}",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        FileLogger.Message("VM Group rename cancelled",
                            FileLogger.EventType.Information, 2099);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error renaming VM Group: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2100);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void buttonLoadGroupsrefresh_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User requested VM Groups refresh",
                    FileLogger.EventType.Information, 2056);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Show progress
                this.Cursor = Cursors.WaitCursor;

                FileLogger.Message("Retrieving VM Groups from server...",
                    FileLogger.EventType.Information, 2057);

                // Get VM Groups
                var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));

                if (vmGroups != null)
                {
                    FileLogger.Message($"Retrieved {vmGroups.Count} VM Groups, updating DataGridView",
                        FileLogger.EventType.Information, 2058);

                    // Update DataGridView
                    UpdateVMGroupsDataGridView(vmGroups);

                    MessageBox.Show($"VM Groups refreshed successfully.\n\nFound {vmGroups.Count} group(s).",
                        "Refresh Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    FileLogger.Message("No VM Groups retrieved",
                        FileLogger.EventType.Warning, 2059);

                    MessageBox.Show("No VM Groups found or error retrieving groups.",
                        "Refresh Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error refreshing VM Groups: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2060);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateVMGroupsDataGridView(List<VMGroupInfo> vmGroups)
        {
            try
            {
                if (datagridviewVMGroups == null)
                {
                    FileLogger.Message("datagridviewVMGroups control not found",
                        FileLogger.EventType.Warning, 2067);
                    return;
                }

                FileLogger.Message($"Updating VM Groups DataGridView with {vmGroups.Count} groups",
                    FileLogger.EventType.Information, 2068);

                // Clear existing data
                datagridviewVMGroups.DataSource = null;
                datagridviewVMGroups.Rows.Clear();
                datagridviewVMGroups.Columns.Clear();

                if (vmGroups == null || vmGroups.Count == 0)
                {
                    FileLogger.Message("No VM Groups to display",
                        FileLogger.EventType.Information, 2069);
                    return;
                }

                // Create DataTable
                var dataTable = new DataTable();
                dataTable.Columns.Add("Group Name", typeof(string));
                dataTable.Columns.Add("Group Type", typeof(string));
                dataTable.Columns.Add("VM Count", typeof(string));
                dataTable.Columns.Add("VM Members", typeof(string));
                dataTable.Columns.Add("Computer Name", typeof(string));

                // Add rows
                foreach (var group in vmGroups)
                {
                    var row = dataTable.NewRow();
                    row["Group Name"] = group.Name;
                    row["Group Type"] = group.GroupTypeDisplay;
                    row["VM Count"] = group.VMCount.ToString();

                    // Truncate long VM lists
                    string vmMembers = group.VMList;
                    if (!string.IsNullOrEmpty(vmMembers) && vmMembers.Length > 100)
                    {
                        vmMembers = vmMembers.Substring(0, 100) + "...";
                    }
                    row["VM Members"] = vmMembers;

                    row["Computer Name"] = group.ComputerName;

                    dataTable.Rows.Add(row);
                }

                // Bind to DataGridView
                datagridviewVMGroups.DataSource = dataTable;

                // Configure DataGridView properties
                datagridviewVMGroups.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                datagridviewVMGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                datagridviewVMGroups.MultiSelect = false;
                datagridviewVMGroups.ReadOnly = true;
                datagridviewVMGroups.AllowUserToAddRows = false;
                datagridviewVMGroups.AllowUserToDeleteRows = false;
                datagridviewVMGroups.RowHeadersVisible = false;

                FileLogger.Message($"VM Groups DataGridView updated successfully with {vmGroups.Count} groups",
                    FileLogger.EventType.Information, 2070);
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error updating VM Groups DataGridView: {ex.Message}",
                    FileLogger.EventType.Error, 2071);
            }
        }

        private void UpdateVMGroupsDataGridView()
        {
            try
            {
                FileLogger.Message("Refreshing VM Groups DataGridView (silent refresh)",
                    FileLogger.EventType.Information, 2083);

                // Get VM Groups without showing message boxes
                var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));

                if (vmGroups != null)
                {
                    UpdateVMGroupsDataGridView(vmGroups);
                }
                else
                {
                    FileLogger.Message("No VM Groups retrieved during silent refresh",
                        FileLogger.EventType.Information, 2084);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error during silent VM Groups refresh: {ex.Message}",
                    FileLogger.EventType.Error, 2085);
            }
        }

        private void allVMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User requested VM data export",
                    FileLogger.EventType.Information, 2101);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Check if we have VM data
                if (datagridviewVMOverView == null || datagridviewVMOverView.Rows.Count == 0)
                {
                    MessageBox.Show("No VM data available. Please load VMs first.",
                        "No Data",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                FileLogger.Message($"Export All VM Data requested - {datagridviewVMOverView.Rows.Count} VMs available",
                    FileLogger.EventType.Information, 2102);

                // Show SaveFileDialog with format options
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Title = "Export All VM Data";
                    saveFileDialog.FileName = $"HyperV_AllVMData_{SessionContext.ServerName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    saveFileDialog.Filter = "JSON Files (*.json)|*.json|CSV Files (*.csv)|*.csv|XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;
                        string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

                        FileLogger.Message($"Exporting VM data to: {filePath} (Format: {fileExtension})",
                            FileLogger.EventType.Information, 2103);

                        // Show progress cursor
                        this.Cursor = Cursors.WaitCursor;

                        try
                        {
                            // Get VM Groups data
                            var vmGroups = VMGroups.GetHyperVVMGroups(cmd => ExecutePowerShellCommand(cmd));

                            // Export based on file extension
                            bool success = false;
                            switch (fileExtension)
                            {
                                case ".json":
                                    success = ExportToJson(filePath, vmGroups);
                                    break;

                                case ".csv":
                                    success = ExportToCsv(filePath, vmGroups);
                                    break;

                                case ".xml":
                                    success = ExportToXml(filePath, vmGroups);
                                    break;

                                case ".txt":
                                    success = ExportToText(filePath, vmGroups);
                                    break;

                                default:
                                    success = ExportToJson(filePath, vmGroups);
                                    break;
                            }

                            if (success)
                            {
                                FileLogger.Message($"VM data export completed successfully: {filePath}",
                                    FileLogger.EventType.Information, 2104);

                                // Show success message with option to open file location
                                var result = MessageBox.Show(
                                    $"VM data exported successfully to:\n{filePath}\n\nWould you like to open the file location?",
                                    "Export Complete",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Information);

                                if (result == DialogResult.Yes)
                                {
                                    try
                                    {
                                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                                    }
                                    catch (Exception ex)
                                    {
                                        FileLogger.Message($"Could not open file location: {ex.Message}",
                                            FileLogger.EventType.Warning, 2105);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            this.Cursor = Cursors.Default;
                        }
                    }
                    else
                    {
                        FileLogger.Message("Export dialog cancelled by user",
                            FileLogger.EventType.Information, 2106);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error exporting VM data: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2107);

                MessageBox.Show(errorMsg,
                    "Export Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool ExportToJson(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                FileLogger.Message("Exporting as JSON format",
                    FileLogger.EventType.Information, 2108);

                var exportData = new
                {
                    ExportInfo = new
                    {
                        ExportDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        ExportedBy = Environment.UserName,
                        HyperVHost = SessionContext.ServerName,
                        ConnectionType = SessionContext.IsLocal ? "Local" : "Remote",
                        TotalVMs = datagridviewVMOverView.Rows.Count,
                        ApplicationVersion = "HyperView v1.0.0.0"
                    },
                    VMData = GetVMDataFromGrid(),
                    VMGroups = vmGroups
                };

                string jsonData = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                System.IO.File.WriteAllText(filePath, jsonData, System.Text.Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error exporting to JSON: {ex.Message}",
                    FileLogger.EventType.Error, 2109);
                return false;
            }
        }

        private bool ExportToCsv(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                FileLogger.Message("Exporting as CSV format",
                    FileLogger.EventType.Information, 2110);

                var vmData = GetVMDataFromGrid();
                var csv = new System.Text.StringBuilder();

                // Write header
                var headers = new List<string>();
                foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                {
                    headers.Add($"\"{column.HeaderText}\"");
                }
                csv.AppendLine(string.Join(",", headers));

                // Write data rows
                foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                {
                    var values = new List<string>();
                    foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                    {
                        var cellValue = row.Cells[column.Index].Value?.ToString() ?? "";
                        values.Add($"\"{cellValue.Replace("\"", "\"\"")}\"");
                    }
                    csv.AppendLine(string.Join(",", values));
                }

                System.IO.File.WriteAllText(filePath, csv.ToString(), System.Text.Encoding.UTF8);

                // Also create a separate CSV for VM Groups if available
                if (vmGroups != null && vmGroups.Count > 0)
                {
                    string groupsCsvPath = filePath.Replace(".csv", "_VMGroups.csv");
                    var groupsCsv = new System.Text.StringBuilder();

                    groupsCsv.AppendLine("\"Group Name\",\"Group Type\",\"VM Count\",\"VM Members\",\"Computer Name\"");

                    foreach (var group in vmGroups)
                    {
                        groupsCsv.AppendLine($"\"{group.Name}\",\"{group.GroupTypeDisplay}\",\"{group.VMCount}\",\"{group.VMList}\",\"{group.ComputerName}\"");
                    }

                    System.IO.File.WriteAllText(groupsCsvPath, groupsCsv.ToString(), System.Text.Encoding.UTF8);

                    FileLogger.Message($"VM Groups data also exported to: {groupsCsvPath}",
                        FileLogger.EventType.Information, 2111);
                }

                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error exporting to CSV: {ex.Message}",
                    FileLogger.EventType.Error, 2112);
                return false;
            }
        }

        private bool ExportToXml(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                FileLogger.Message("Exporting as XML format",
                    FileLogger.EventType.Information, 2113);

                var vmData = GetVMDataFromGrid();

                using (var writer = System.Xml.XmlWriter.Create(filePath, new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    Encoding = System.Text.Encoding.UTF8
                }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("HyperVExport");

                    // Export Info
                    writer.WriteStartElement("ExportInfo");
                    writer.WriteElementString("ExportDateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    writer.WriteElementString("ExportedBy", Environment.UserName);
                    writer.WriteElementString("HyperVHost", SessionContext.ServerName);
                    writer.WriteElementString("ConnectionType", SessionContext.IsLocal ? "Local" : "Remote");
                    writer.WriteElementString("TotalVMs", datagridviewVMOverView.Rows.Count.ToString());
                    writer.WriteEndElement();

                    // VM Data
                    writer.WriteStartElement("VMData");
                    foreach (var vm in vmData)
                    {
                        writer.WriteStartElement("VM");
                        foreach (var kvp in vm)
                        {
                            writer.WriteElementString(kvp.Key.Replace(" ", ""), kvp.Value);
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // VM Groups
                    if (vmGroups != null && vmGroups.Count > 0)
                    {
                        writer.WriteStartElement("VMGroups");
                        foreach (var group in vmGroups)
                        {
                            writer.WriteStartElement("VMGroup");
                            writer.WriteElementString("Name", group.Name);
                            writer.WriteElementString("GroupType", group.GroupTypeDisplay);
                            writer.WriteElementString("VMCount", group.VMCount.ToString());
                            writer.WriteElementString("VMMembers", group.VMList);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error exporting to XML: {ex.Message}",
                    FileLogger.EventType.Error, 2114);
                return false;
            }
        }

        private bool ExportToText(string filePath, List<VMGroupInfo> vmGroups)
        {
            try
            {
                FileLogger.Message("Exporting as formatted text",
                    FileLogger.EventType.Information, 2115);

                var vmData = GetVMDataFromGrid();
                var textOutput = new List<string>();

                textOutput.Add(new string('=', 80));
                textOutput.Add("HYPERVIEW - ALL VM DATA EXPORT");
                textOutput.Add(new string('=', 80));
                textOutput.Add($"Export Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                textOutput.Add($"Exported By: {Environment.UserName}");
                textOutput.Add($"Hyper-V Host: {SessionContext.ServerName}");
                textOutput.Add($"Connection Type: {(SessionContext.IsLocal ? "Local" : "Remote")}");
                textOutput.Add($"Total VMs: {datagridviewVMOverView.Rows.Count}");
                textOutput.Add("");

                // VM Data
                textOutput.Add("VIRTUAL MACHINES DATA");
                textOutput.Add(new string('-', 80));

                foreach (var vm in vmData)
                {
                    textOutput.Add("");
                    foreach (var kvp in vm)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value))
                            textOutput.Add($"  {kvp.Key}: {kvp.Value}");
                    }
                }

                // VM Groups Data
                if (vmGroups != null && vmGroups.Count > 0)
                {
                    textOutput.Add("");
                    textOutput.Add("");
                    textOutput.Add("VM GROUPS DATA");
                    textOutput.Add(new string('-', 80));

                    foreach (var group in vmGroups)
                    {
                        textOutput.Add("");
                        textOutput.Add($"Group Name: {group.Name}");
                        textOutput.Add($"  Type: {group.GroupTypeDisplay}");
                        textOutput.Add($"  VM Count: {group.VMCount}");
                        textOutput.Add($"  VM Members: {group.VMList}");
                    }
                }

                textOutput.Add("");
                textOutput.Add(new string('=', 80));
                textOutput.Add("END OF EXPORT");
                textOutput.Add(new string('=', 80));

                System.IO.File.WriteAllLines(filePath, textOutput, System.Text.Encoding.UTF8);

                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error exporting to text: {ex.Message}",
                    FileLogger.EventType.Error, 2116);
                return false;
            }
        }

        private List<Dictionary<string, string>> GetVMDataFromGrid()
        {
            var vmData = new List<Dictionary<string, string>>();

            foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
            {
                var vmInfo = new Dictionary<string, string>();

                foreach (DataGridViewColumn column in datagridviewVMOverView.Columns)
                {
                    var value = row.Cells[column.Index].Value?.ToString() ?? "";
                    vmInfo[column.HeaderText] = value;
                }

                vmData.Add(vmInfo);
            }

            return vmData;
        }

        private void buttonManageServerMembers_Click(object sender, EventArgs e)
        {
            try
            {
                FileLogger.Message("User initiated VM Group member management",
                    FileLogger.EventType.Information, 2139);

                // Check if there's an active Hyper-V connection
                if (!SessionContext.IsSessionActive())
                {
                    MessageBox.Show("Please connect to a Hyper-V server first.",
                        "Connection Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                // Get selected VM group
                if (datagridviewVMGroups == null || datagridviewVMGroups.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a VM Group to manage.",
                        "No Selection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string groupName = datagridviewVMGroups.SelectedRows[0].Cells["Group Name"].Value?.ToString();

                if (string.IsNullOrEmpty(groupName))
                {
                    MessageBox.Show("Invalid VM Group selection.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                FileLogger.Message($"User selected VM Group '{groupName}' for member management",
                    FileLogger.EventType.Information, 2140);

                // Get list of all available VMs from the overview grid
                var allVMs = new List<string>();
                if (datagridviewVMOverView != null)
                {
                    foreach (DataGridViewRow row in datagridviewVMOverView.Rows)
                    {
                        var vmName = row.Cells["VM Name"].Value?.ToString();
                        if (!string.IsNullOrEmpty(vmName))
                        {
                            allVMs.Add(vmName);
                        }
                    }
                }

                // Show manage members form
                using (Forms.ManageVMGroupMembers manageForm = new Forms.ManageVMGroupMembers())
                {
                    manageForm.GroupName = groupName;
                    manageForm.AllVMs = allVMs;
                    manageForm.ExecutePowerShellCommand = cmd => ExecutePowerShellCommand(cmd);

                    var result = manageForm.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        FileLogger.Message($"Member management completed for group '{groupName}', refreshing view...",
                            FileLogger.EventType.Information, 2141);

                        // Refresh the VM Groups view
                        VMGroups.RefreshVMGroupsView(
                            $"Group members updated: {groupName}",
                            cmd => ExecutePowerShellCommand(cmd),
                            groups => UpdateVMGroupsDataGridView(groups));
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error managing VM Group members: {ex.Message}";
                FileLogger.Message(errorMsg, FileLogger.EventType.Error, 2142);

                MessageBox.Show(errorMsg,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Use the shared confirmation function
            if (ConfirmDisconnectAndExit())
            {
                this.Close();
            }
        }

        private void myWebpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Globals.ToolStings.URLMyWebPage);

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.URLMyWebPage + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.URLMyWebPage + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }

        private void myBlogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Globals.ToolStings.URLMyBlog);

                // Log the opening of the URL message
                Message("User clicked the 'My webpage' link to open the URL: '" + Globals.ToolStings.URLMyBlog + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.URLMyBlog + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }

        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Log the user's action to open the Changelog form
            Message("User clicked the 'Changelog' menu item to open the Changelog form", EventType.Information, 1057);

            // Open the Changelog form
            ChangelogForm f2 = new ChangelogForm();
            f2.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Log the user's action to open the About form
            Message("User clicked the 'About' menu item to open the About form", EventType.Information, 1056);

            // Open the About form
            AboutForm f2 = new AboutForm();
            f2.ShowDialog();
        }

        private void openLogForTodayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the log file for today
            try
            {
                var logFilePath = FileManager.LogFilePath;
                logFilePath = logFilePath + "\\" + Globals.ToolName.HyperView + " Log " + DateTime.Today.ToString("dd-MM-yyyy") + "." + "log";
                Process.Start(logFilePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void openLogFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the log folder
            try
            {
                var logFolderPath = FileManager.LogFilePath;
                Process.Start("explorer.exe", logFolderPath);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void pictureboxSupportMe_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Globals.ToolStings.URLBuyMeaCoffie);

                // Log the opening of the URL message
                Message("User clicked the 'Buy me a coffie' picture in MainForm to open the URL: '" + Globals.ToolStings.URLBuyMeaCoffie + "'", EventType.Information, 1052);
            }
            catch (Exception ex)
            {
                // Show an error message if the URL could not be opened
                MessageBox.Show(@"Failed to open the URL '" + Globals.ToolStings.URLBuyMeaCoffie + "'. Error: " + ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error message
                Message("Failed to open the URL: " + ex.Message, EventType.Error, 1041);
            }
        }
    }
}
