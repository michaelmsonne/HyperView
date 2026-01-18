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

                // Get detailed cluster information using a comprehensive PowerShell script
                string detailedInfoScript = @"
                    $result = @{
                        Nodes = @()
                        Networks = @()
                        SharedStorage = @()
                        VirtualMachines = @()
                    }

                    try {
                        # Get cluster nodes
                        $nodes = Get-ClusterNode -ErrorAction Stop
                        foreach ($node in $nodes) {
                            $result.Nodes += @{
                                Name = $node.Name
                                State = $node.State.ToString()
                                Id = $node.Id.ToString()
                                NodeWeight = $node.NodeWeight
                                DynamicWeight = $node.DynamicWeight
                                FaultDomain = $node.FaultDomain
                                DrainStatus = $node.DrainStatus.ToString()
                            }
                        }
                    }
                    catch {
                        # Nodes retrieval failed
                    }

                    try {
                        # Get cluster networks
                        $networks = Get-ClusterNetwork -ErrorAction Stop
                        foreach ($network in $networks) {
                            $result.Networks += @{
                                Name = $network.Name
                                Address = $network.Address
                                AddressMask = $network.AddressMask
                                Role = $network.Role.ToString()
                                State = $network.State.ToString()
                            }
                        }
                    }
                    catch {
                        # Networks retrieval failed
                    }

                    try {
                        # Get cluster shared volumes
                        $csvs = Get-ClusterSharedVolume -ErrorAction SilentlyContinue
                        if ($csvs) {
                            foreach ($csv in $csvs) {
                                $result.SharedStorage += @{
                                    Name = $csv.Name
                                    OwnerNode = $csv.OwnerNode.ToString()
                                    State = $csv.State.ToString()
                                    SharedVolumeInfo = if ($csv.SharedVolumeInfo) { $csv.SharedVolumeInfo.ToString() } else { '' }
                                }
                            }
                        }
                    }
                    catch {
                        # CSV retrieval failed
                    }

                    try {
                        # Get clustered VMs
                        $groups = Get-ClusterGroup | Where-Object { $_.GroupType -eq 'VirtualMachine' }
                        foreach ($group in $groups) {
                            $preferredOwners = ''
                            if ($group.PreferredOwners) {
                                $preferredOwners = ($group.PreferredOwners -join ', ')
                            }

                            $result.VirtualMachines += @{
                                Name = $group.Name
                                OwnerNode = $group.OwnerNode.ToString()
                                State = $group.State.ToString()
                                Priority = $group.Priority
                                PreferredOwners = $preferredOwners
                            }
                        }
                    }
                    catch {
                        # VM groups retrieval failed
                    }

                    return $result
                ";

                var detailedResult = executePowerShellCommand(detailedInfoScript);

                if (detailedResult != null && detailedResult.Count > 0)
                {
                    var result = (PSObject)detailedResult[0];
                    var hashtable = (System.Collections.Hashtable)result.BaseObject;

                    // Parse nodes
                    var nodesArray = hashtable["Nodes"];
                    if (nodesArray is System.Collections.ArrayList nodesList)
                    {
                        foreach (System.Collections.Hashtable nodeHash in nodesList)
                        {
                            clusterDetails.Nodes.Add(new ClusterNodeInfo
                            {
                                Name = nodeHash["Name"]?.ToString(),
                                State = nodeHash["State"]?.ToString(),
                                Id = nodeHash["Id"]?.ToString(),
                                NodeWeight = Convert.ToInt32(nodeHash["NodeWeight"] ?? 0),
                                DynamicWeight = Convert.ToInt32(nodeHash["DynamicWeight"] ?? 0),
                                FaultDomain = nodeHash["FaultDomain"]?.ToString(),
                                DrainStatus = nodeHash["DrainStatus"]?.ToString()
                            });
                        }

                        FileLogger.Message($"Retrieved information for {clusterDetails.Nodes.Count} cluster node(s)",
                            FileLogger.EventType.Information, 3023);
                    }

                    // Parse networks
                    var networksArray = hashtable["Networks"];
                    if (networksArray is System.Collections.ArrayList networksList)
                    {
                        foreach (System.Collections.Hashtable networkHash in networksList)
                        {
                            clusterDetails.Networks.Add(new ClusterNetworkInfo
                            {
                                Name = networkHash["Name"]?.ToString(),
                                Address = networkHash["Address"]?.ToString(),
                                AddressMask = networkHash["AddressMask"]?.ToString(),
                                Role = networkHash["Role"]?.ToString(),
                                State = networkHash["State"]?.ToString()
                            });
                        }

                        FileLogger.Message($"Retrieved information for {clusterDetails.Networks.Count} cluster network(s)",
                            FileLogger.EventType.Information, 3024);
                    }

                    // Parse shared storage
                    var storageArray = hashtable["SharedStorage"];
                    if (storageArray is System.Collections.ArrayList storageList)
                    {
                        foreach (System.Collections.Hashtable storageHash in storageList)
                        {
                            clusterDetails.SharedStorage.Add(new ClusterStorageInfo
                            {
                                Name = storageHash["Name"]?.ToString(),
                                OwnerNode = storageHash["OwnerNode"]?.ToString(),
                                State = storageHash["State"]?.ToString(),
                                SharedVolumeInfo = storageHash["SharedVolumeInfo"]?.ToString()
                            });
                        }

                        FileLogger.Message($"Retrieved information for {clusterDetails.SharedStorage.Count} cluster shared volume(s)",
                            FileLogger.EventType.Information, 3025);
                    }

                    // Parse VMs
                    var vmsArray = hashtable["VirtualMachines"];
                    if (vmsArray is System.Collections.ArrayList vmsList)
                    {
                        foreach (System.Collections.Hashtable vmHash in vmsList)
                        {
                            clusterDetails.VirtualMachines.Add(new ClusterGroupInfo
                            {
                                Name = vmHash["Name"]?.ToString(),
                                OwnerNode = vmHash["OwnerNode"]?.ToString(),
                                State = vmHash["State"]?.ToString(),
                                Priority = Convert.ToInt32(vmHash["Priority"] ?? 0),
                                PreferredOwners = vmHash["PreferredOwners"]?.ToString()
                            });
                        }

                        FileLogger.Message($"Retrieved information for {clusterDetails.VirtualMachines.Count} clustered VM(s)",
                            FileLogger.EventType.Information, 3026);
                    }
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
    }
}

