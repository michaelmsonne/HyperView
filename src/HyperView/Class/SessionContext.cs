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

        /// <summary>
        /// Initialize the session context with login information
        /// </summary>
        public static void Initialize(string serverName, bool useWindowsAuth, PSCredential credentials, 
            string connectedUser, string connectionType, int vmCount, bool isLocal)
        {
            ServerName = serverName;
            UseWindowsAuth = useWindowsAuth;
            Credentials = credentials;
            ConnectedUser = connectedUser;
            ConnectionType = connectionType;
            VMCount = vmCount;
            IsLocal = isLocal;

            FileLogger.Message($"Session initialized for '{serverName}' as '{connectedUser}' ({connectionType})", 
                FileLogger.EventType.Information, 2000);
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
