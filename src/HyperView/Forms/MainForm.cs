using System.Data;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using HyperView.Class;

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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if there's an active Hyper-V connection
            if (SessionContext.IsSessionActive())
            {
                FileLogger.Message("User is closing the application with active connection",
                    FileLogger.EventType.Information, 2012);

                // Show confirmation dialog similar to disconnect button
                var confirmResult = MessageBox.Show(
                    $"Are you sure you want to disconnect from Hyper-V (server: '{SessionContext.ServerName}') and close the application?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult == DialogResult.Yes)
                {
                    FileLogger.Message($"User confirmed exit - disconnecting from Hyper-V server '{SessionContext.ServerName}'...",
                        FileLogger.EventType.Information, 2013);

                    // Cleanup will be handled by OnFormClosing after this event handler completes
                    // Allow the form to close
                }
                else
                {
                    // User cancelled - prevent form from closing
                    FileLogger.Message("User cancelled exit - keeping application open",
                        FileLogger.EventType.Information, 2015);
                    e.Cancel = true;
                }
            }
            else
            {
                // No active connection - just log and close
                FileLogger.Message("Form closing event triggered - no active connection",
                    FileLogger.EventType.Information, 2016);
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
                        var createResult = CreateHyperVVMGroup(groupName, groupType);

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

        private class VMGroupCreationResult
        {
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        private VMGroupCreationResult CreateHyperVVMGroup(string groupName, string groupType)
        {
            try
            {
                FileLogger.Message($"Executing New-VMGroup command for '{groupName}'...",
                    FileLogger.EventType.Information, 2035);

                // Build PowerShell command
                string command = $"New-VMGroup -Name '{groupName}' -GroupType {groupType}";

                var results = ExecutePowerShellCommand(command);

                if (results != null && results.Count > 0)
                {
                    FileLogger.Message($"VM Group '{groupName}' created successfully via PowerShell",
                        FileLogger.EventType.Information, 2036);

                    return new VMGroupCreationResult
                    {
                        Success = true
                    };
                }
                else
                {
                    string error = "No results returned from New-VMGroup command";
                    FileLogger.Message($"VM Group creation returned no results: {error}",
                        FileLogger.EventType.Warning, 2037);

                    return new VMGroupCreationResult
                    {
                        Success = false,
                        Error = error
                    };
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception creating VM Group: {ex.Message}",
                    FileLogger.EventType.Error, 2038);

                return new VMGroupCreationResult
                {
                    Success = false,
                    Error = ex.Message
                };
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
    }
}
