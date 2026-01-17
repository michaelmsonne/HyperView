using System.Management.Automation;

namespace HyperView.Class
{
    /// <summary>
    /// Global session context to store connection credentials and information
    /// that can be reused across the application
    /// </summary>
    public static class SessionContext
    {
        public static string ServerName { get; set; }
        public static bool UseWindowsAuth { get; set; }
        public static PSCredential Credentials { get; set; }
        public static string ConnectedUser { get; set; }
        public static string ConnectionType { get; set; }
        public static int VMCount { get; set; }
        public static bool IsLocal { get; set; }
        
        // Enhanced properties
        public static string HostName { get; set; }
        public static string HyperVVersion { get; set; }
        public static int LogicalProcessorCount { get; set; }
        public static double TotalMemoryGB { get; set; }
        public static bool IsCluster { get; set; }
        public static string ClusterName { get; set; }
        public static string FullyQualifiedDomainName { get; set; }
        public static DateTime ConnectedAt { get; set; }

        /// <summary>
        /// Initialize the session context with login information
        /// </summary>
        public static void Initialize(string serverName, bool useWindowsAuth, PSCredential credentials, 
            string connectedUser, string connectionType, int vmCount, bool isLocal,
            string hostName = null, string hyperVVersion = null, int logicalProcessorCount = 0,
            double totalMemoryGB = 0, bool isCluster = false, string clusterName = null,
            string fullyQualifiedDomainName = null)
        {
            ServerName = serverName;
            UseWindowsAuth = useWindowsAuth;
            Credentials = credentials;
            ConnectedUser = connectedUser;
            ConnectionType = connectionType;
            VMCount = vmCount;
            IsLocal = isLocal;
            HostName = hostName ?? serverName;
            HyperVVersion = hyperVVersion;
            LogicalProcessorCount = logicalProcessorCount;
            TotalMemoryGB = totalMemoryGB;
            IsCluster = isCluster;
            ClusterName = clusterName;
            FullyQualifiedDomainName = fullyQualifiedDomainName ?? serverName;
            ConnectedAt = DateTime.Now;

            string clusterInfo = isCluster ? $", Cluster: '{clusterName}'" : "";
            FileLogger.Message($"Session initialized for '{serverName}' as '{connectedUser}' ({connectionType}){clusterInfo}", 
                FileLogger.EventType.Information, 2000);
                
            if (!string.IsNullOrEmpty(hyperVVersion))
            {
                FileLogger.Message($"Hyper-V Version: {hyperVVersion}, Processors: {logicalProcessorCount}, Memory: {totalMemoryGB:F2} GB", 
                    FileLogger.EventType.Information, 2002);
            }
        }

        /// <summary>
        /// Clear the session context
        /// </summary>
        public static void Clear()
        {
            FileLogger.Message("Session context cleared", FileLogger.EventType.Information, 2001);
            
            ServerName = null;
            UseWindowsAuth = false;
            Credentials = null;
            ConnectedUser = null;
            ConnectionType = null;
            VMCount = 0;
            IsLocal = false;
            HostName = null;
            HyperVVersion = null;
            LogicalProcessorCount = 0;
            TotalMemoryGB = 0;
            IsCluster = false;
            ClusterName = null;
            FullyQualifiedDomainName = null;
            ConnectedAt = DateTime.MinValue;
        }

        /// <summary>
        /// Check if a valid session exists
        /// </summary>
        public static bool IsSessionActive()
        {
            return !string.IsNullOrEmpty(ServerName) && !string.IsNullOrEmpty(ConnectedUser);
        }
    }
}
