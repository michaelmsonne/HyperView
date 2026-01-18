using System.Management.Automation;

namespace HyperView.Class
{
    /// <summary>
    /// Represents detailed information about a Hyper-V host
    /// </summary>
    public class HostDetailsInfo
    {
        public string HostName { get; set; } = "";
        public string ClusterName { get; set; } = "N/A";
        public string NodeState { get; set; } = "Standalone";
        public string Domain { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public string OSVersion { get; set; } = "";
        public string BuildNumber { get; set; } = "";
        public string BootTime { get; set; } = "";
        public string Uptime { get; set; } = "";
        public string TimeZone { get; set; } = "";
        public string NtpServers { get; set; } = "";
        public string NtpStatus { get; set; } = "";
        public string LicenseStatus { get; set; } = "Unknown";
        public string LicenseType { get; set; } = "Unknown";
        public string ProductKey { get; set; } = "Unknown";
        public string GracePeriod { get; set; } = "N/A";
        public string LicenseDescription { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Model { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string Processor { get; set; } = "";
        public int Sockets { get; set; } = 1;
        public int Cores { get; set; } = 1;
        public int LogicalCPUs { get; set; } = 1;
        public string HyperThreading { get; set; } = "No";
        public string SLATSupport { get; set; } = "No";
        public double TotalMemoryGB { get; set; }
        public double UsedMemoryGB { get; set; }
        public double FreeMemoryGB { get; set; }
        public double MemoryUsagePercent { get; set; }
        public int TotalVMs { get; set; }
        public int RunningVMs { get; set; }
        public int StoppedVMs { get; set; }
        public int VirtualSwitches { get; set; }
        public int ExternalSwitches { get; set; }
        public string IPAddresses { get; set; } = "";
        public string LiveMigration { get; set; } = "Disabled";
        public string EnhancedSession { get; set; } = "No";
        public string NUMASpanning { get; set; } = "No";
        public string VHDPath { get; set; } = "";
        public string VMConfigPath { get; set; } = "";
    }

    /// <summary>
    /// Provides functionality to retrieve Hyper-V host details
    /// </summary>
    public static class HostDetails
    {
        /// <summary>
        /// Gets detailed information about Hyper-V host(s)
        /// </summary>
        public static List<HostDetailsInfo> GetHyperVHostDetails(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommandOnNode = null)
        {
            var allHosts = new List<HostDetailsInfo>();

            try
            {
                FileLogger.Message("Getting Hyper-V host details...",
                    FileLogger.EventType.Information, 4001);

                // Check if connected to a cluster
                bool isCluster = SessionContext.IsCluster;
                var clusterNodes = new List<string>();

                if (isCluster && !SessionContext.IsLocal)
                {
                    // Get cluster nodes
                    FileLogger.Message("Detected cluster environment, getting cluster nodes...",
                        FileLogger.EventType.Information, 4002);

                    string getNodesScript = @"Get-ClusterNode -ErrorAction Stop | Select-Object -ExpandProperty Name";
                    var nodesResult = executePowerShellCommand(getNodesScript);

                    if (nodesResult != null && nodesResult.Count > 0)
                    {
                        foreach (var nodeObj in nodesResult)
                        {
                            string nodeName = nodeObj.BaseObject?.ToString();
                            if (!string.IsNullOrEmpty(nodeName))
                            {
                                // If the original connection used FQDN, construct FQDNs for cluster nodes
                                if (SessionContext.ServerName.Contains('.') && !nodeName.Contains('.'))
                                {
                                    string domain = SessionContext.ServerName.Substring(SessionContext.ServerName.IndexOf('.'));
                                    nodeName = nodeName + domain;
                                }
                                clusterNodes.Add(nodeName);
                            }
                        }

                        FileLogger.Message($"Found {clusterNodes.Count} cluster nodes: {string.Join(", ", clusterNodes)}",
                            FileLogger.EventType.Information, 4003);
                    }
                }

                if (clusterNodes.Count > 0 && executePowerShellCommandOnNode != null)
                {
                    // Get details for all cluster nodes
                    int nodeIndex = 0;
                    foreach (var node in clusterNodes)
                    {
                        nodeIndex++;
                        try
                        {
                            FileLogger.Message($"Getting host details for cluster node {nodeIndex} of {clusterNodes.Count}: {node}",
                                FileLogger.EventType.Information, 4004);

                            var hostInfo = GetHostDetailsFromNode(node, executePowerShellCommandOnNode);
                            if (hostInfo != null)
                            {
                                allHosts.Add(hostInfo);
                                FileLogger.Message($"Successfully retrieved details for node: {node}",
                                    FileLogger.EventType.Information, 4005);
                            }
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Failed to get details for cluster node {node}: {ex.Message}",
                                FileLogger.EventType.Warning, 4006);
                        }
                    }
                }
                else
                {
                    // Single host
                    FileLogger.Message("Getting host details for single host...",
                        FileLogger.EventType.Information, 4007);

                    var hostInfo = GetHostDetailsFromSession(executePowerShellCommand);
                    if (hostInfo != null)
                    {
                        allHosts.Add(hostInfo);
                    }
                }

                FileLogger.Message($"Successfully retrieved details for {allHosts.Count} host(s)",
                    FileLogger.EventType.Information, 4008);

                return allHosts;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting Hyper-V host details: {ex.Message}",
                    FileLogger.EventType.Error, 4009);
                return allHosts;
            }
        }

        /// <summary>
        /// Gets host details from a specific cluster node
        /// </summary>
        private static HostDetailsInfo GetHostDetailsFromNode(string nodeName,
            Func<string, string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommandOnNode)
        {
            string script = GetHostDetailsScript();
            var result = executePowerShellCommandOnNode(nodeName, script);

            if (result != null && result.Count > 0)
            {
                return ParseHostDetails(result[0]);
            }

            return null;
        }

        /// <summary>
        /// Gets host details from the current session
        /// </summary>
        private static HostDetailsInfo GetHostDetailsFromSession(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            string script = GetHostDetailsScript();
            var result = executePowerShellCommand(script);

            if (result != null && result.Count > 0)
            {
                return ParseHostDetails(result[0]);
            }

            return null;
        }

        /// <summary>
        /// Gets the PowerShell script to retrieve host details
        /// </summary>
        private static string GetHostDetailsScript()
        {
            return @"
                try {
                    $vmHost = Get-VMHost -ErrorAction SilentlyContinue
                    $computerSystem = Get-WmiObject Win32_ComputerSystem -ErrorAction SilentlyContinue
                    $operatingSystem = Get-WmiObject Win32_OperatingSystem -ErrorAction SilentlyContinue
                    $bios = Get-WmiObject Win32_BIOS -ErrorAction SilentlyContinue
                    $processors = @(Get-WmiObject Win32_Processor -ErrorAction SilentlyContinue)

                    # Calculate processor info
                    $processorSockets = if ($processors) { $processors.Count } else { 1 }
                    $totalCores = ($processors | Measure-Object NumberOfCores -Sum).Sum
                    $totalLogicalProcessors = ($processors | Measure-Object NumberOfLogicalProcessors -Sum).Sum
                    if ($totalCores -eq 0) { $totalCores = 1 }
                    if ($totalLogicalProcessors -eq 0) { $totalLogicalProcessors = $totalCores }
                    if ($processorSockets -eq 0) { $processorSockets = 1 }

                    # Get memory info
                    $totalMemoryGB = [Math]::Round($computerSystem.TotalPhysicalMemory / 1GB, 2)
                    $availableMemoryGB = [Math]::Round(($operatingSystem.FreePhysicalMemory * 1KB) / 1GB, 2)
                    $usedMemoryGB = $totalMemoryGB - $availableMemoryGB
                    $memoryUsagePercent = if ($totalMemoryGB -gt 0) { [Math]::Round(($usedMemoryGB / $totalMemoryGB) * 100, 1) } else { 0 }

                    # Get VM counts
                    $allVMs = @(Get-VM -ErrorAction SilentlyContinue)
                    $runningVMs = @($allVMs | Where-Object { $_.State -eq 'Running' })
                    $stoppedVMs = @($allVMs | Where-Object { $_.State -eq 'Off' })

                    # Get virtual switches
                    $virtualSwitches = @(Get-VMSwitch -ErrorAction SilentlyContinue)
                    $externalSwitches = @($virtualSwitches | Where-Object { $_.SwitchType -eq 'External' })

                    # Calculate uptime
                    $uptime = (Get-Date) - $operatingSystem.ConvertToDateTime($operatingSystem.LastBootUpTime)
                    $uptimeString = ""$($uptime.Days)d $($uptime.Hours)h $($uptime.Minutes)m""

                    # Boot time
                    $bootTime = $operatingSystem.ConvertToDateTime($operatingSystem.LastBootUpTime).ToString('yyyy-MM-dd HH:mm:ss')

                    # Get cluster info
                    $clusterName = 'N/A'
                    $nodeState = 'Standalone'
                    try {
                        $clusterNode = Get-ClusterNode -Name $env:COMPUTERNAME -ErrorAction SilentlyContinue
                        if ($clusterNode) {
                            $cluster = Get-Cluster -ErrorAction SilentlyContinue
                            $clusterName = if ($cluster) { $cluster.Name } else { 'Cluster Detected' }
                            $nodeState = if ($clusterNode.State) { $clusterNode.State.ToString() } else { 'Online' }
                        }
                    } catch { }

                    # Get IP addresses
                    $ipList = @()
                    try {
                        $networkAdapters = Get-WmiObject Win32_NetworkAdapterConfiguration -ErrorAction SilentlyContinue |
                            Where-Object { $_.IPEnabled -eq $true -and $_.IPAddress -ne $null }
                        foreach ($adapter in $networkAdapters) {
                            foreach ($ip in $adapter.IPAddress) {
                                if ($ip -notlike '169.254.*' -and $ip -notlike 'fe80:*' -and $ip -ne '::1' -and $ip -ne '127.0.0.1') {
                                    $ipList += $ip
                                }
                            }
                        }
                    } catch { }
                    $ipAddresses = if ($ipList.Count -gt 0) { $ipList -join ', ' } else { 'N/A' }

                    # Get time zone
                    $timeZone = try { (Get-WmiObject Win32_TimeZone -ErrorAction SilentlyContinue).Description } catch { 'Unknown' }

                    # Get license info
                    $licenseStatus = 'Unknown'
                    $licenseType = 'Unknown'
                    $productKey = 'Unknown'
                    try {
                        $windowsLicense = Get-WmiObject -Class SoftwareLicensingProduct -ErrorAction SilentlyContinue |
                            Where-Object { $_.Name -like '*Windows*' -and $_.LicenseStatus -eq 1 } | Select-Object -First 1
                        if ($windowsLicense) {
                            $licenseStatus = switch ($windowsLicense.LicenseStatus) {
                                0 { 'Unlicensed' }
                                1 { 'Licensed' }
                                2 { 'OOB Grace Period' }
                                3 { 'OOT Grace Period' }
                                4 { 'Non-Genuine Grace' }
                                5 { 'Notification' }
                                6 { 'Extended Grace' }
                                default { 'Unknown' }
                            }
                            if ($windowsLicense.Description -like '*VOLUME*') { $licenseType = 'Volume License' }
                            elseif ($windowsLicense.Description -like '*OEM*') { $licenseType = 'OEM' }
                            elseif ($windowsLicense.Description -like '*RETAIL*') { $licenseType = 'Retail' }
                            else { $licenseType = 'Standard' }
                            if ($windowsLicense.PartialProductKey) {
                                $productKey = '*****-*****-*****-*****-' + $windowsLicense.PartialProductKey
                            }
                        }
                    } catch { }

                    # Serial number
                    $serialNumber = try { $bios.SerialNumber } catch { 'Unknown' }
                    if ([string]::IsNullOrWhiteSpace($serialNumber)) { $serialNumber = 'Unknown' }

                    # Return object
                    [PSCustomObject]@{
                        HostName = if ($vmHost) { $vmHost.Name } else { $env:COMPUTERNAME }
                        ClusterName = $clusterName
                        NodeState = $nodeState
                        Domain = $computerSystem.Domain
                        OperatingSystem = $operatingSystem.Caption -replace 'Microsoft ', ''
                        OSVersion = $operatingSystem.Version
                        BuildNumber = $operatingSystem.BuildNumber
                        BootTime = $bootTime
                        Uptime = $uptimeString
                        TimeZone = $timeZone
                        LicenseStatus = $licenseStatus
                        LicenseType = $licenseType
                        ProductKey = $productKey
                        Manufacturer = $computerSystem.Manufacturer
                        Model = $computerSystem.Model
                        SerialNumber = $serialNumber
                        Processor = ($processors | Select-Object -First 1).Name -replace '\s+', ' '
                        Sockets = $processorSockets
                        Cores = $totalCores
                        LogicalCPUs = $totalLogicalProcessors
                        HyperThreading = if ($totalLogicalProcessors -gt $totalCores) { 'Yes' } else { 'No' }
                        TotalMemoryGB = $totalMemoryGB
                        UsedMemoryGB = [Math]::Round($usedMemoryGB, 2)
                        FreeMemoryGB = [Math]::Round($availableMemoryGB, 2)
                        MemoryUsagePercent = $memoryUsagePercent
                        TotalVMs = $allVMs.Count
                        RunningVMs = $runningVMs.Count
                        StoppedVMs = $stoppedVMs.Count
                        VirtualSwitches = $virtualSwitches.Count
                        ExternalSwitches = $externalSwitches.Count
                        IPAddresses = $ipAddresses
                        LiveMigration = if ($vmHost -and $vmHost.VirtualMachineMigrationEnabled) { 'Enabled' } else { 'Disabled' }
                        EnhancedSession = if ($vmHost -and $vmHost.EnableEnhancedSessionMode) { 'Yes' } else { 'No' }
                        NUMASpanning = if ($vmHost -and $vmHost.NumaSpanningEnabled) { 'Yes' } else { 'No' }
                        VHDPath = if ($vmHost) { $vmHost.VirtualHardDiskPath } else { '' }
                        VMConfigPath = if ($vmHost) { $vmHost.VirtualMachinePath } else { '' }
                    }
                } catch {
                    # Return minimal info on error
                    [PSCustomObject]@{
                        HostName = $env:COMPUTERNAME
                        ClusterName = 'N/A'
                        NodeState = 'Unknown'
                        Domain = ''
                        OperatingSystem = 'Error retrieving details'
                        OSVersion = ''
                        BuildNumber = ''
                        BootTime = ''
                        Uptime = ''
                        TimeZone = ''
                        LicenseStatus = 'Unknown'
                        LicenseType = 'Unknown'
                        ProductKey = 'Unknown'
                        Manufacturer = ''
                        Model = ''
                        SerialNumber = ''
                        Processor = ''
                        Sockets = 0
                        Cores = 0
                        LogicalCPUs = 0
                        HyperThreading = 'No'
                        TotalMemoryGB = 0
                        UsedMemoryGB = 0
                        FreeMemoryGB = 0
                        MemoryUsagePercent = 0
                        TotalVMs = 0
                        RunningVMs = 0
                        StoppedVMs = 0
                        VirtualSwitches = 0
                        ExternalSwitches = 0
                        IPAddresses = ''
                        LiveMigration = 'Unknown'
                        EnhancedSession = 'Unknown'
                        NUMASpanning = 'Unknown'
                        VHDPath = ''
                        VMConfigPath = ''
                    }
                }
            ";
        }

        /// <summary>
        /// Parses a PSObject into a HostDetailsInfo object
        /// </summary>
        private static HostDetailsInfo ParseHostDetails(PSObject psObject)
        {
            return new HostDetailsInfo
            {
                HostName = GetStringProperty(psObject, "HostName"),
                ClusterName = GetStringProperty(psObject, "ClusterName"),
                NodeState = GetStringProperty(psObject, "NodeState"),
                Domain = GetStringProperty(psObject, "Domain"),
                OperatingSystem = GetStringProperty(psObject, "OperatingSystem"),
                OSVersion = GetStringProperty(psObject, "OSVersion"),
                BuildNumber = GetStringProperty(psObject, "BuildNumber"),
                BootTime = GetStringProperty(psObject, "BootTime"),
                Uptime = GetStringProperty(psObject, "Uptime"),
                TimeZone = GetStringProperty(psObject, "TimeZone"),
                LicenseStatus = GetStringProperty(psObject, "LicenseStatus"),
                LicenseType = GetStringProperty(psObject, "LicenseType"),
                ProductKey = GetStringProperty(psObject, "ProductKey"),
                Manufacturer = GetStringProperty(psObject, "Manufacturer"),
                Model = GetStringProperty(psObject, "Model"),
                SerialNumber = GetStringProperty(psObject, "SerialNumber"),
                Processor = GetStringProperty(psObject, "Processor"),
                Sockets = GetIntProperty(psObject, "Sockets"),
                Cores = GetIntProperty(psObject, "Cores"),
                LogicalCPUs = GetIntProperty(psObject, "LogicalCPUs"),
                HyperThreading = GetStringProperty(psObject, "HyperThreading"),
                TotalMemoryGB = GetDoubleProperty(psObject, "TotalMemoryGB"),
                UsedMemoryGB = GetDoubleProperty(psObject, "UsedMemoryGB"),
                FreeMemoryGB = GetDoubleProperty(psObject, "FreeMemoryGB"),
                MemoryUsagePercent = GetDoubleProperty(psObject, "MemoryUsagePercent"),
                TotalVMs = GetIntProperty(psObject, "TotalVMs"),
                RunningVMs = GetIntProperty(psObject, "RunningVMs"),
                StoppedVMs = GetIntProperty(psObject, "StoppedVMs"),
                VirtualSwitches = GetIntProperty(psObject, "VirtualSwitches"),
                ExternalSwitches = GetIntProperty(psObject, "ExternalSwitches"),
                IPAddresses = GetStringProperty(psObject, "IPAddresses"),
                LiveMigration = GetStringProperty(psObject, "LiveMigration"),
                EnhancedSession = GetStringProperty(psObject, "EnhancedSession"),
                NUMASpanning = GetStringProperty(psObject, "NUMASpanning"),
                VHDPath = GetStringProperty(psObject, "VHDPath"),
                VMConfigPath = GetStringProperty(psObject, "VMConfigPath")
            };
        }

        private static string GetStringProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                return prop?.Value?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static int GetIntProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToInt32(prop.Value);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetDoubleProperty(PSObject psObject, string propertyName)
        {
            try
            {
                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return Convert.ToDouble(prop.Value);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
