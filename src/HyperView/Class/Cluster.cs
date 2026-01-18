using System.Management.Automation;

namespace HyperView.Class
{
    /// <summary>
    /// Information about a cluster node
    /// </summary>
    public class ClusterNodeInfo
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string Id { get; set; }
        public int NodeWeight { get; set; }
        public int DynamicWeight { get; set; }
        public string FaultDomain { get; set; }
        public string DrainStatus { get; set; }
    }

    /// <summary>
    /// Information about a cluster group (VM)
    /// </summary>
    public class ClusterGroupInfo
    {
        public string Name { get; set; }
        public string OwnerNode { get; set; }
        public string State { get; set; }
        public int Priority { get; set; }
        public string PreferredOwners { get; set; }
    }

    /// <summary>
    /// Information about a cluster network
    /// </summary>
    public class ClusterNetworkInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string AddressMask { get; set; }
        public string Role { get; set; }
        public string State { get; set; }
    }

    /// <summary>
    /// Information about cluster shared storage
    /// </summary>
    public class ClusterStorageInfo
    {
        public string Name { get; set; }
        public string OwnerNode { get; set; }
        public string State { get; set; }
        public string SharedVolumeInfo { get; set; }
    }

    /// <summary>
    /// Result of cluster detection test
    /// </summary>
    public class ClusterTestResult
    {
        public bool IsCluster { get; set; }
        public string ClusterName { get; set; }
        public string NodeName { get; set; }
        public string ClusterService { get; set; }
        public bool FailoverClusteringInstalled { get; set; }
        public List<ClusterNodeInfo> ClusterNodes { get; set; } = new List<ClusterNodeInfo>();
        public List<ClusterGroupInfo> ClusterGroups { get; set; } = new List<ClusterGroupInfo>();
        public string Method { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// Detailed cluster information
    /// </summary>
    public class ClusterInformation
    {
        public string ClusterName { get; set; }
        public string CurrentNode { get; set; }
        public List<ClusterNodeInfo> Nodes { get; set; } = new List<ClusterNodeInfo>();
        public List<ClusterNetworkInfo> Networks { get; set; } = new List<ClusterNetworkInfo>();
        public List<ClusterStorageInfo> SharedStorage { get; set; } = new List<ClusterStorageInfo>();
        public List<ClusterGroupInfo> VirtualMachines { get; set; } = new List<ClusterGroupInfo>();
    }

    /// <summary>
    /// Provides methods for managing and retrieving information about Hyper-V clusters
    /// </summary>
    public static class Cluster
    {
        /// <summary>
        /// Tests if the Hyper-V host is part of a cluster using multiple detection methods
        /// </summary>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <returns>ClusterTestResult with cluster information</returns>
        public static ClusterTestResult TestHyperVCluster(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Testing cluster status for '{SessionContext.ServerName}'",
                    FileLogger.EventType.Information, 3001);

                var clusterInfo = new ClusterTestResult
                {
                    IsCluster = false,
                    ClusterName = null,
                    NodeName = SessionContext.ServerName,
                    ClusterService = null,
                    FailoverClusteringInstalled = false,
                    Method = "Unknown"
                };

                // Method 1: Check Failover Clustering Windows Feature (Server OS only)
                if (!SessionContext.IsLocal)
                {
                    // For remote connections, try to check Windows Feature
                    try
                    {
                        FileLogger.Message("Checking Failover Clustering feature via PowerShell...",
                            FileLogger.EventType.Information, 3002);

                        string featureCheckScript = @"
                            if (Get-Command -Name Get-WindowsFeature -ErrorAction SilentlyContinue) {
                                $feature = Get-WindowsFeature -Name Failover-Clustering -ErrorAction SilentlyContinue
                                if ($feature) {
                                    return @{
                                        Installed = ($feature.InstallState -eq 'Installed')
                                        Available = $true
                                    }
                                }
                            }
                            return @{ Available = $false }
                        ";

                        var featureResult = executePowerShellCommand(featureCheckScript);

                        if (featureResult != null && featureResult.Count > 0)
                        {
                            var result = (PSObject)featureResult[0];
                            var hashtable = (System.Collections.Hashtable)result.BaseObject;
                            bool available = (bool)hashtable["Available"];

                            if (available && (bool)hashtable["Installed"])
                            {
                                clusterInfo.FailoverClusteringInstalled = true;
                                FileLogger.Message("Failover Clustering feature is installed",
                                    FileLogger.EventType.Information, 3003);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Message($"Windows Feature check skipped or failed: {ex.Message}",
                            FileLogger.EventType.Information, 3004);
                    }
                }

                // Method 2: Check Cluster Service Status (works for both local and remote)
                try
                {
                    FileLogger.Message("Checking Cluster service status...",
                        FileLogger.EventType.Information, 3005);

                    string serviceCheckScript = @"
                        $service = Get-Service -Name ClusSvc -ErrorAction SilentlyContinue
                        if ($service) {
                            return @{
                                Exists = $true
                                Status = $service.Status.ToString()
                            }
                        }
                        return @{ Exists = $false }
                    ";

                    var serviceResult = executePowerShellCommand(serviceCheckScript);

                    if (serviceResult != null && serviceResult.Count > 0)
                    {
                        var result = (PSObject)serviceResult[0];
                        var hashtable = (System.Collections.Hashtable)result.BaseObject;
                        bool exists = (bool)hashtable["Exists"];

                        if (exists)
                        {
                            string status = hashtable["Status"]?.ToString();
                            clusterInfo.ClusterService = status;

                            FileLogger.Message($"Cluster service found with status: {status}",
                                FileLogger.EventType.Information, 3006);

                            if (status == "Running")
                            {
                                clusterInfo.IsCluster = true;
                                clusterInfo.Method = "ClusterService";
                            }
                        }
                        else
                        {
                            FileLogger.Message("Cluster service not found - likely a standalone host",
                                FileLogger.EventType.Information, 3007);
                        }
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Failed to check Cluster service: {ex.Message}",
                        FileLogger.EventType.Information, 3008);
                }

                // Method 3: Try to get cluster information using Get-Cluster cmdlet
                if (clusterInfo.FailoverClusteringInstalled || clusterInfo.ClusterService == "Running")
                {
                    try
                    {
                        FileLogger.Message("Attempting to get cluster information via Get-Cluster...",
                            FileLogger.EventType.Information, 3009);

                        string clusterInfoScript = @"
                            $cluster = Get-Cluster -ErrorAction SilentlyContinue
                            if ($cluster) {
                                $result = @{
                                    Found = $true
                                    Name = $cluster.Name
                                    Nodes = @()
                                    Groups = @()
                                }

                                # Get cluster nodes
                                $nodes = Get-ClusterNode -ErrorAction SilentlyContinue
                                if ($nodes) {
                                    foreach ($node in $nodes) {
                                        $result.Nodes += @{
                                            Name = $node.Name
                                            State = $node.State.ToString()
                                            Id = $node.Id.ToString()
                                        }
                                    }
                                }

                                # Get cluster VM groups
                                $groups = Get-ClusterGroup -ErrorAction SilentlyContinue | Where-Object { $_.GroupType -eq 'VirtualMachine' }
                                if ($groups) {
                                    foreach ($group in $groups) {
                                        $result.Groups += @{
                                            Name = $group.Name
                                            OwnerNode = $group.OwnerNode.ToString()
                                            State = $group.State.ToString()
                                        }
                                    }
                                }

                                return $result
                            }
                            return @{ Found = $false }
                        ";

                        var clusterResult = executePowerShellCommand(clusterInfoScript);

                        if (clusterResult != null && clusterResult.Count > 0)
                        {
                            var result = (PSObject)clusterResult[0];
                            var hashtable = (System.Collections.Hashtable)result.BaseObject;
                            bool found = (bool)hashtable["Found"];

                            if (found)
                            {
                                clusterInfo.IsCluster = true;
                                clusterInfo.ClusterName = hashtable["Name"]?.ToString();
                                clusterInfo.Method = "Get-Cluster";

                                FileLogger.Message($"Cluster detected: '{clusterInfo.ClusterName}'",
                                    FileLogger.EventType.Information, 3010);

                                // Parse cluster nodes
                                var nodesArray = hashtable["Nodes"];
                                if (nodesArray is System.Collections.ArrayList nodesList)
                                {
                                    foreach (System.Collections.Hashtable nodeHash in nodesList)
                                    {
                                        clusterInfo.ClusterNodes.Add(new ClusterNodeInfo
                                        {
                                            Name = nodeHash["Name"]?.ToString(),
                                            State = nodeHash["State"]?.ToString(),
                                            Id = nodeHash["Id"]?.ToString()
                                        });
                                    }

                                    FileLogger.Message($"Found {clusterInfo.ClusterNodes.Count} cluster node(s)",
                                        FileLogger.EventType.Information, 3011);
                                }

                                // Parse cluster groups
                                var groupsArray = hashtable["Groups"];
                                if (groupsArray is System.Collections.ArrayList groupsList)
                                {
                                    foreach (System.Collections.Hashtable groupHash in groupsList)
                                    {
                                        clusterInfo.ClusterGroups.Add(new ClusterGroupInfo
                                        {
                                            Name = groupHash["Name"]?.ToString(),
                                            OwnerNode = groupHash["OwnerNode"]?.ToString(),
                                            State = groupHash["State"]?.ToString()
                                        });
                                    }

                                    FileLogger.Message($"Found {clusterInfo.ClusterGroups.Count} cluster VM group(s)",
                                        FileLogger.EventType.Information, 3012);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Message($"Failed to get cluster information: {ex.Message}",
                            FileLogger.EventType.Information, 3013);
                    }
                }

                // Method 4: Check for highly available VMs (alternative detection)
                if (!clusterInfo.IsCluster)
                {
                    try
                    {
                        FileLogger.Message("Checking for highly available VMs...",
                            FileLogger.EventType.Information, 3014);

                        string haVMCheckScript = @"
                            $haVMs = Get-VM | Where-Object { $_.IsClustered -eq $true } -ErrorAction SilentlyContinue
                            if ($haVMs) {
                                return @{
                                    Found = $true
                                    Count = ($haVMs | Measure-Object).Count
                                }
                            }
                            return @{ Found = $false }
                        ";

                        var haVMResult = executePowerShellCommand(haVMCheckScript);

                        if (haVMResult != null && haVMResult.Count > 0)
                        {
                            var result = (PSObject)haVMResult[0];
                            var hashtable = (System.Collections.Hashtable)result.BaseObject;
                            bool found = (bool)hashtable["Found"];

                            if (found)
                            {
                                int count = Convert.ToInt32(hashtable["Count"]);
                                clusterInfo.IsCluster = true;
                                clusterInfo.Method = "ClusteredVMs";

                                FileLogger.Message($"Cluster detected via {count} highly available VM(s)",
                                    FileLogger.EventType.Information, 3015);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Message($"Failed to check for clustered VMs: {ex.Message}",
                            FileLogger.EventType.Information, 3016);
                    }
                }

                // Log final result
                if (clusterInfo.IsCluster)
                {
                    FileLogger.Message($"Connected to cluster node: '{clusterInfo.NodeName}' (Cluster: '{clusterInfo.ClusterName}', Method: {clusterInfo.Method})",
                        FileLogger.EventType.Information, 3017);
                }
                else
                {
                    FileLogger.Message($"Connected to standalone Hyper-V host: '{clusterInfo.NodeName}'",
                        FileLogger.EventType.Information, 3018);
                }

                return clusterInfo;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error testing cluster status: {ex.Message}",
                    FileLogger.EventType.Error, 3019);

                return new ClusterTestResult
                {
                    IsCluster = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets detailed information about the Hyper-V cluster
        /// </summary>
        /// <param name="executePowerShellCommand">Function to execute PowerShell commands</param>
        /// <returns>ClusterInformation with detailed cluster data, or null if not a cluster</returns>
        public static ClusterInformation GetClusterInformation(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                if (!SessionContext.IsSessionActive())
                {
                    FileLogger.Message("No active Hyper-V connection found",
                        FileLogger.EventType.Warning, 3020);
                    return null;
                }

                // First test if this is a cluster
                var clusterTest = TestHyperVCluster(executePowerShellCommand);

                if (!clusterTest.IsCluster)
                {
                    FileLogger.Message("Current connection is not to a cluster",
                        FileLogger.EventType.Information, 3021);
                    return null;
                }

                FileLogger.Message("Gathering detailed cluster information...",
                    FileLogger.EventType.Information, 3022);

                var clusterDetails = new ClusterInformation
                {
                    ClusterName = clusterTest.ClusterName,
                    CurrentNode = SessionContext.ServerName
                };

                // Get cluster nodes
                try
                {
                    string nodesScript = @"
                        Get-ClusterNode -ErrorAction Stop | ForEach-Object {
                            [PSCustomObject]@{
                                Name = $_.Name
                                State = $_.State.ToString()
                                Id = $_.Id.ToString()
                                NodeWeight = $_.NodeWeight
                                DynamicWeight = $_.DynamicWeight
                                FaultDomain = $_.FaultDomain
                                DrainStatus = $_.DrainStatus.ToString()
                            }
                        }
                    ";

                    var nodesResult = executePowerShellCommand(nodesScript);
                    if (nodesResult != null && nodesResult.Count > 0)
                    {
                        foreach (var node in nodesResult)
                        {
                            clusterDetails.Nodes.Add(new ClusterNodeInfo
                            {
                                Name = GetPSObjectProperty(node, "Name"),
                                State = GetPSObjectProperty(node, "State"),
                                Id = GetPSObjectProperty(node, "Id"),
                                NodeWeight = GetPSObjectPropertyInt(node, "NodeWeight"),
                                DynamicWeight = GetPSObjectPropertyInt(node, "DynamicWeight"),
                                FaultDomain = GetPSObjectProperty(node, "FaultDomain"),
                                DrainStatus = GetPSObjectProperty(node, "DrainStatus")
                            });
                        }
                        FileLogger.Message($"Retrieved information for {clusterDetails.Nodes.Count} cluster node(s)",
                            FileLogger.EventType.Information, 3023);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Failed to get cluster nodes: {ex.Message}",
                        FileLogger.EventType.Warning, 3040);
                }

                // Get cluster networks
                try
                {
                    string networksScript = @"
                        Get-ClusterNetwork -ErrorAction Stop | ForEach-Object {
                            [PSCustomObject]@{
                                Name = $_.Name
                                Address = $_.Address
                                AddressMask = $_.AddressMask
                                Role = $_.Role.ToString()
                                State = $_.State.ToString()
                            }
                        }
                    ";

                    var networksResult = executePowerShellCommand(networksScript);
                    if (networksResult != null && networksResult.Count > 0)
                    {
                        foreach (var network in networksResult)
                        {
                            clusterDetails.Networks.Add(new ClusterNetworkInfo
                            {
                                Name = GetPSObjectProperty(network, "Name"),
                                Address = GetPSObjectProperty(network, "Address"),
                                AddressMask = GetPSObjectProperty(network, "AddressMask"),
                                Role = GetPSObjectProperty(network, "Role"),
                                State = GetPSObjectProperty(network, "State")
                            });
                        }
                        FileLogger.Message($"Retrieved information for {clusterDetails.Networks.Count} cluster network(s)",
                            FileLogger.EventType.Information, 3024);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Failed to get cluster networks: {ex.Message}",
                        FileLogger.EventType.Warning, 3041);
                }

                // Get cluster shared volumes
                try
                {
                    string storageScript = @"
                        $csvs = Get-ClusterSharedVolume -ErrorAction SilentlyContinue
                        if ($csvs) {
                            $csvs | ForEach-Object {
                                [PSCustomObject]@{
                                    Name = $_.Name
                                    OwnerNode = $_.OwnerNode.ToString()
                                    State = $_.State.ToString()
                                    SharedVolumeInfo = if ($_.SharedVolumeInfo) { $_.SharedVolumeInfo.ToString() } else { '' }
                                }
                            }
                        }
                    ";

                    var storageResult = executePowerShellCommand(storageScript);
                    if (storageResult != null && storageResult.Count > 0)
                    {
                        foreach (var storage in storageResult)
                        {
                            // Skip if it's not a valid result (could be null or empty from the if statement)
                            var name = GetPSObjectProperty(storage, "Name");
                            if (!string.IsNullOrEmpty(name))
                            {
                                clusterDetails.SharedStorage.Add(new ClusterStorageInfo
                                {
                                    Name = name,
                                    OwnerNode = GetPSObjectProperty(storage, "OwnerNode"),
                                    State = GetPSObjectProperty(storage, "State"),
                                    SharedVolumeInfo = GetPSObjectProperty(storage, "SharedVolumeInfo")
                                });
                            }
                        }
                        FileLogger.Message($"Retrieved information for {clusterDetails.SharedStorage.Count} cluster shared volume(s)",
                            FileLogger.EventType.Information, 3025);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Failed to get cluster shared volumes: {ex.Message}",
                        FileLogger.EventType.Warning, 3042);
                }

                // Get clustered VMs
                try
                {
                    string vmsScript = @"
                        Get-ClusterGroup -ErrorAction Stop | Where-Object { $_.GroupType -eq 'VirtualMachine' } | ForEach-Object {
                            $preferredOwners = ''
                            if ($_.PreferredOwners) {
                                $preferredOwners = ($_.PreferredOwners -join ', ')
                            }
                            [PSCustomObject]@{
                                Name = $_.Name
                                OwnerNode = $_.OwnerNode.ToString()
                                State = $_.State.ToString()
                                Priority = $_.Priority
                                PreferredOwners = $preferredOwners
                            }
                        }
                    ";

                    var vmsResult = executePowerShellCommand(vmsScript);
                    if (vmsResult != null && vmsResult.Count > 0)
                    {
                        foreach (var vm in vmsResult)
                        {
                            clusterDetails.VirtualMachines.Add(new ClusterGroupInfo
                            {
                                Name = GetPSObjectProperty(vm, "Name"),
                                OwnerNode = GetPSObjectProperty(vm, "OwnerNode"),
                                State = GetPSObjectProperty(vm, "State"),
                                Priority = GetPSObjectPropertyInt(vm, "Priority"),
                                PreferredOwners = GetPSObjectProperty(vm, "PreferredOwners")
                            });
                        }
                        FileLogger.Message($"Retrieved information for {clusterDetails.VirtualMachines.Count} clustered VM(s)",
                            FileLogger.EventType.Information, 3026);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Failed to get clustered VMs: {ex.Message}",
                        FileLogger.EventType.Warning, 3043);
                }

                FileLogger.Message("Cluster information retrieval completed",
                    FileLogger.EventType.Information, 3027);

                return clusterDetails;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error getting cluster information: {ex.Message}",
                    FileLogger.EventType.Error, 3028);
                return null;
            }
        }

        /// <summary>
        /// Helper method to safely get a string property from a PSObject (handles remote deserialization)
        /// </summary>
        private static string GetPSObjectProperty(PSObject psObject, string propertyName)
        {
            try
            {
                if (psObject == null) return string.Empty;

                var prop = psObject.Properties[propertyName];
                if (prop?.Value != null)
                {
                    return prop.Value.ToString();
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Helper method to safely get an int property from a PSObject (handles remote deserialization)
        /// </summary>
        private static int GetPSObjectPropertyInt(PSObject psObject, string propertyName)
        {
            try
            {
                if (psObject == null) return 0;

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
    }
}

