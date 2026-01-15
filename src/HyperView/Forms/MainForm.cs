using System.Data;
using System.Management.Automation;

namespace HyperView
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LoadVMOverview();
        }

        private void LoadVMOverview()
        {
            try
            {
                // Clear existing data
                datagridviewVMOverView.DataSource = null;
                datagridviewVMOverView.Rows.Clear();
                datagridviewVMOverView.Columns.Clear();

                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddCommand("Get-VM");

                    var results = powerShell.Invoke();

                    if (powerShell.HadErrors)
                    {
                        var errors = string.Join(Environment.NewLine,
                            powerShell.Streams.Error.Select(e => e.ToString()));
                        MessageBox.Show($"PowerShell errors occurred:\n{errors}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (results.Count == 0)
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
                            var totalDiskGB = GetVMTotalDiskSize(powerShell, vmName);
                            row["Total Disk (GB)"] = totalDiskGB > 0 ? totalDiskGB.ToString("F2") : "";
                            
                            var networkAdapterCount = GetVMNetworkAdapterCount(powerShell, vmName);
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
                            var groups = GetVMGroups(powerShell, vmName);
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
                            var checkpointCount = GetVMCheckpointCount(powerShell, vmName);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading VM overview: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private double GetVMTotalDiskSize(PowerShell powerShell, string vmName)
        {
            try
            {
                powerShell.Commands.Clear();
                powerShell.AddCommand("Get-VMHardDiskDrive")
                    .AddParameter("VMName", vmName);
                
                var hdds = powerShell.Invoke();
                powerShell.Streams.Error.Clear();
                
                double totalGB = 0;
                foreach (var hdd in hdds)
                {
                    var path = hdd.Properties["Path"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    {
                        var fileInfo = new System.IO.FileInfo(path);
                        totalGB += fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                    }
                }
                
                return totalGB;
            }
            catch
            {
                return 0;
            }
        }

        private int GetVMNetworkAdapterCount(PowerShell powerShell, string vmName)
        {
            try
            {
                powerShell.Commands.Clear();
                powerShell.AddCommand("Get-VMNetworkAdapter")
                    .AddParameter("VMName", vmName);
                
                var adapters = powerShell.Invoke();
                powerShell.Streams.Error.Clear();
                
                return adapters.Count;
            }
            catch
            {
                return 0;
            }
        }

        private string GetVMGroups(PowerShell powerShell, string vmName)
        {
            try
            {
                powerShell.Commands.Clear();
                powerShell.AddScript($"Get-VMGroup | Where-Object {{ $_.VMMembers.Name -contains '{vmName}' }} | Select-Object -ExpandProperty Name");
                
                var groups = powerShell.Invoke();
                powerShell.Streams.Error.Clear();
                
                if (groups.Count > 0)
                {
                    return string.Join(", ", groups.Select(g => g.ToString()));
                }
                
                return "";
            }
            catch
            {
                return "";
            }
        }

        private int GetVMCheckpointCount(PowerShell powerShell, string vmName)
        {
            try
            {
                powerShell.Commands.Clear();
                powerShell.AddCommand("Get-VMSnapshot")
                    .AddParameter("VMName", vmName);
                
                var checkpoints = powerShell.Invoke();
                powerShell.Streams.Error.Clear();
                
                return checkpoints.Count;
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
    }
}
