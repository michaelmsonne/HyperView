using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using HyperView.Class;

namespace HyperView.Forms
{
    public partial class LoginForm : Form
    {
        public class LoginResult
        {
            public bool Success { get; set; }
            public bool Cancelled { get; set; }
            public string ServerName { get; set; }
            public bool UseWindowsAuth { get; set; }
            public PSCredential Credentials { get; set; }
            public string ConnectedUser { get; set; }
            public string ConnectionType { get; set; }
            public int VMCount { get; set; }
        }

        public LoginResult Result { get; private set; }
        private string _lastServerChecked = string.Empty;
        private bool _isInitializing = true;
        private bool _isConnecting = false; // Prevent double login attempts

        public LoginForm()
        {
            InitializeComponent();
            InitializeFormEvents();
            InitializeHyperVDefaults();
            LoadSavedCredentials();
            SetToolName();

            // Mark initialization as complete
            _isInitializing = false;
        }

        private void InitializeHyperVDefaults()
        {
            try
            {
                FileLogger.Message("Determining default Hyper-V server name...", 
                    FileLogger.EventType.Information, 1047);

                // Check if local machine has Hyper-V role installed
                bool hyperVInstalled = TestLocalHyperVInstallation();

                // Set default server name based on Hyper-V availability
                if (hyperVInstalled)
                {
                    textboxServer.Text = Environment.MachineName;
                    UpdateStatusLabel("Ready - Local Hyper-V detected", isSuccess: true);
                    FileLogger.Message($"Local Hyper-V installation detected - default server set to: {Environment.MachineName}",
                        FileLogger.EventType.Information, 1048);
                }
                else
                {
                    textboxServer.Text = Environment.MachineName;
                    UpdateStatusLabel("Ready - No local Hyper-V detected", isSuccess: false);
                    FileLogger.Message("No local Hyper-V detected - default server set to current machine name",
                        FileLogger.EventType.Information, 1049);
                }
            }
            catch (Exception ex)
            {
                // Fallback to machine name if anything goes wrong
                textboxServer.Text = Environment.MachineName;
                UpdateStatusLabel("Ready", isSuccess: null);
                FileLogger.Message($"Error determining Hyper-V status, defaulting to machine name: {ex.Message}",
                    FileLogger.EventType.Warning, 1050);
            }
        }

        private bool TestLocalHyperVInstallation()
        {
            FileLogger.Message("Testing for local Hyper-V installation...",
                FileLogger.EventType.Information, 1051);

            // Method 1: Check for Hyper-V Windows Feature (Windows 10/11)
            if (TestHyperVWindowsFeature())
                return true;

            // Method 2: Check for Hyper-V service
            if (TestHyperVService())
                return true;

            // Method 3: Check for Hyper-V Windows Feature using Management Objects (Server)
            if (TestHyperVServerRole())
                return true;

            FileLogger.Message("No local Hyper-V installation detected",
                FileLogger.EventType.Information, 1052);
            return false;
        }

        private bool TestHyperVWindowsFeature()
        {
            try
            {
                FileLogger.Message("Checking Windows Optional Feature for Hyper-V...",
                    FileLogger.EventType.Information, 1053);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript("Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -ErrorAction SilentlyContinue");
                    var results = ps.Invoke();

                    if (results != null && results.Count > 0)
                    {
                        var feature = results[0];
                        var state = feature.Properties["State"]?.Value?.ToString();

                        if (state == "Enabled")
                        {
                            FileLogger.Message("Hyper-V feature detected as enabled",
                                FileLogger.EventType.Information, 1054);

                            // Verify service is running
                            if (TestHyperVServiceStatus())
                            {
                                return true;
                            }
                            else
                            {
                                FileLogger.Message("Hyper-V feature is enabled but service is not running",
                                    FileLogger.EventType.Warning, 1055);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Windows Optional Feature check failed: {ex.Message}",
                    FileLogger.EventType.Information, 1056);
            }

            return false;
        }

        private bool TestHyperVService()
        {
            try
            {
                FileLogger.Message("Checking Hyper-V service...",
                    FileLogger.EventType.Information, 1057);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript("Get-Service -Name vmms -ErrorAction SilentlyContinue");
                    var results = ps.Invoke();

                    if (results != null && results.Count > 0)
                    {
                        var service = results[0];
                        var status = service.Properties["Status"]?.Value?.ToString();

                        FileLogger.Message("Hyper-V service detected",
                            FileLogger.EventType.Information, 1058);

                        if (status == "Running")
                        {
                            FileLogger.Message("Hyper-V service is running",
                                FileLogger.EventType.Information, 1059);
                            return true;
                        }
                        else
                        {
                            FileLogger.Message($"Hyper-V service exists but is not running (Status: {status})",
                                FileLogger.EventType.Warning, 1060);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Hyper-V service check failed: {ex.Message}",
                    FileLogger.EventType.Information, 1061);
            }

            return false;
        }

        private bool TestHyperVServerRole()
        {
            try
            {
                FileLogger.Message("Checking Windows Server role for Hyper-V...",
                    FileLogger.EventType.Information, 1062);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript("Get-WindowsFeature -Name Hyper-V -ErrorAction SilentlyContinue");
                    var results = ps.Invoke();

                    if (results != null && results.Count > 0)
                    {
                        var feature = results[0];
                        var installState = feature.Properties["InstallState"]?.Value?.ToString();

                        if (installState == "Installed")
                        {
                            FileLogger.Message("Hyper-V role detected on Windows Server",
                                FileLogger.EventType.Information, 1063);

                            // Verify service is running
                            if (TestHyperVServiceStatus())
                            {
                                return true;
                            }
                            else
                            {
                                FileLogger.Message("Hyper-V role is installed but service is not running",
                                    FileLogger.EventType.Warning, 1064);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Windows Server role check failed: {ex.Message}",
                    FileLogger.EventType.Information, 1065);
            }

            return false;
        }

        private bool TestHyperVServiceStatus()
        {
            try
            {
                FileLogger.Message("Checking Hyper-V service status...",
                    FileLogger.EventType.Information, 1066);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript("Get-Service -Name vmms -ErrorAction SilentlyContinue");
                    var results = ps.Invoke();

                    if (results != null && results.Count > 0)
                    {
                        var service = results[0];
                        var status = service.Properties["Status"]?.Value?.ToString();

                        if (status == "Running")
                        {
                            FileLogger.Message("Hyper-V Virtual Machine Management service is running",
                                FileLogger.EventType.Information, 1067);
                            return true;
                        }
                        else
                        {
                            FileLogger.Message($"Hyper-V Virtual Machine Management service is not running (Status: {status})",
                                FileLogger.EventType.Warning, 1068);
                            return false;
                        }
                    }
                    else
                    {
                        FileLogger.Message("Hyper-V Virtual Machine Management service not found",
                            FileLogger.EventType.Warning, 1069);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error checking Hyper-V service status: {ex.Message}",
                    FileLogger.EventType.Error, 1070);
                return false;
            }
        }

        private void UpdateStatusLabel(string message, bool? isSuccess = null)
        {
            try
            {
                if (toolStripStatusLabelTextLoginForm != null)
                {
                    toolStripStatusLabelTextLoginForm.Text = message;

                    // Update color based on status
                    if (isSuccess.HasValue)
                    {
                        toolStripStatusLabelTextLoginForm.ForeColor = isSuccess.Value 
                            ? Color.Green 
                            : Color.Orange;
                    }
                    else
                    {
                        toolStripStatusLabelTextLoginForm.ForeColor = SystemColors.ControlText;
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error updating status label: {ex.Message}",
                    FileLogger.EventType.Warning, 1071);
            }
        }

        private void SetDefaultServer()
        {
            // This method is now replaced by InitializeHyperVDefaults
            // Keeping for backward compatibility but not doing anything
        }

        private void SetToolName()
        {
            labelLoginFormToolName.Text = Globals.ToolName.HyperView + " v." + Globals.ToolProperties.ToolVersion;
            Text = $"{Globals.ToolName.HyperView} - Login";
        }

        private void InitializeFormEvents()
        {
            // Set password char
            textboxPassword.UseSystemPasswordChar = true;

            // Setup tooltip for Windows authentication radio button to show current user
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(radioWindows, 
                $"Connect using your current Windows credentials: {WindowsIdentity.GetCurrent().Name}");

            // Update UI based on initial selection
            RadioAuth_CheckedChanged(null, null);
        }

        private void RadioAuth_CheckedChanged(object sender, EventArgs e)
        {
            bool useCustomAuth = radioCustom.Checked;
            labelUsername.Enabled = useCustomAuth;
            textboxUsername.Enabled = useCustomAuth;
            labelPassword.Enabled = useCustomAuth;
            textboxPassword.Enabled = useCustomAuth;
            checkboxRemember.Enabled = useCustomAuth;

            if (!useCustomAuth)
            {
                checkboxRemember.Checked = false;
            }
        }

        private void TextboxServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                // Don't trigger login if already in progress
                if (_isConnecting)
                    return;

                if (radioWindows.Checked)
                {
                    ButtonLogin.PerformClick();
                }
                else
                {
                    textboxUsername.Focus();
                }
            }
        }

        private void TextboxUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                // Don't trigger login if already in progress
                if (_isConnecting)
                    return;

                if (string.IsNullOrWhiteSpace(textboxPassword.Text))
                {
                    textboxPassword.Focus();
                }
                else
                {
                    ButtonLogin.PerformClick();
                }
            }
        }

        private void TextboxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                // Don't trigger login if already in progress
                if (_isConnecting)
                    return;

                ButtonLogin.PerformClick();
            }
        }

        private async void ButtonLogin_Click(object sender, EventArgs e)
        {
            // Prevent double-click or multiple simultaneous login attempts
            if (_isConnecting || !ButtonLogin.Enabled)
            {
                FileLogger.Message($"Login attempt blocked - already in progress or button disabled",
                    FileLogger.EventType.Warning, 1042);
                return;
            }

            _isConnecting = true;

            // Immediately disable the button to prevent any possibility of re-entry
            ButtonLogin.Enabled = false;
            buttonCancel.Enabled = false;

            string serverName = textboxServer.Text.Trim();

            // Validate input
            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show("Please enter a server name or IP address.", Globals.MsgBox.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textboxServer.Focus();
                ButtonLogin.Enabled = true;
                buttonCancel.Enabled = true;
                _isConnecting = false;
                return;
            }

            if (radioCustom.Checked)
            {
                if (string.IsNullOrWhiteSpace(textboxUsername.Text))
                {
                    MessageBox.Show("Please enter a username.", Globals.MsgBox.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textboxUsername.Focus();
                    ButtonLogin.Enabled = true;
                    buttonCancel.Enabled = true;
                    _isConnecting = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(textboxPassword.Text))
                {
                    MessageBox.Show("Please enter a password.", Globals.MsgBox.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textboxPassword.Focus();
                    ButtonLogin.Enabled = true;
                    buttonCancel.Enabled = true;
                    _isConnecting = false;
                    return;
                }
            }

            // Save credentials if requested
            if (checkboxRemember.Checked && radioCustom.Checked)
            {
                SaveCredentials(serverName, textboxUsername.Text, textboxPassword.Text);
            }
            else if (!checkboxRemember.Checked)
            {
                ClearSavedCredentials();
            }

            // Disable UI and show progress (button already disabled above)
            string originalText = ButtonLogin.Text;
            ButtonLogin.Text = "Connecting...";
            this.Cursor = Cursors.WaitCursor;

            FileLogger.Message($"Starting connection test to '{serverName}'",
                FileLogger.EventType.Information, 1044);

            try
            {
                bool useWindowsAuth = radioWindows.Checked;
                PSCredential credentials = null;

                if (!useWindowsAuth)
                {
                    SecureString securePassword = new SecureString();
                    foreach (char c in textboxPassword.Text)
                    {
                        securePassword.AppendChar(c);
                    }
                    securePassword.MakeReadOnly();
                    credentials = new PSCredential(textboxUsername.Text.Trim(), securePassword);
                }

                // Test connection
                var connectionResult = await TestHyperVConnection(serverName, credentials);

                if (connectionResult.Success)
                {
                    string connectedUser = useWindowsAuth
                        ? WindowsIdentity.GetCurrent().Name
                        : textboxUsername.Text.Trim();

                    string connectionType = useWindowsAuth
                        ? "Windows Authentication"
                        : "Custom Credentials";

                    // Store result for legacy compatibility
                    Result = new LoginResult
                    {
                        Success = true,
                        ServerName = serverName,
                        UseWindowsAuth = useWindowsAuth,
                        Credentials = credentials,
                        ConnectedUser = connectedUser,
                        ConnectionType = connectionType,
                        VMCount = connectionResult.VMCount
                    };

                    // Initialize global session context for reuse across the application
                    SessionContext.Initialize(
                        serverName,
                        useWindowsAuth,
                        credentials,
                        connectedUser,
                        connectionType,
                        connectionResult.VMCount,
                        connectionResult.IsLocal
                    );

                    FileLogger.Message($"Login successful for '{serverName}' as '{connectedUser}'",
                        FileLogger.EventType.Information, 1016);

                    // Hide login form and show main form
                    FileLogger.Message($"Hiding login form and showing MainForm...",
                        FileLogger.EventType.Information, 1039);

                    this.Hide();

                    using (MainForm mainForm = new MainForm())
                    {
                        FileLogger.Message($"MainForm created, showing dialog...",
                            FileLogger.EventType.Information, 1040);

                        var mainResult = mainForm.ShowDialog();

                        FileLogger.Message($"MainForm closed with result: {mainResult}",
                            FileLogger.EventType.Information, 1041);

                        // If main form closes, clear session and close application
                        if (mainResult == DialogResult.OK || mainResult == DialogResult.Cancel)
                        {
                            SessionContext.Clear();
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                    }
                }
                else
                {
                    // Handle connection failures
                    if (connectionResult.RequiresElevation && connectionResult.CanAutoElevate)
                    {
                        var elevationPrompt = "Administrator privileges or member of the group 'Hyper-V Administrators' " +
                            "are required for local Hyper-V management.\n\n" +
                            "Would you like to restart this application as administrator to continue?";

                        var result = MessageBox.Show(elevationPrompt, "Elevation Required",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                FileLogger.Message("User requested application restart with elevation",
                                    FileLogger.EventType.Information, 1000);
                                ApplicationFunctions.RestartAsAdmin();
                                Application.Exit();
                            }
                            catch (Exception ex)
                            {
                                FileLogger.Message($"Failed to start as admin: {ex.Message}",
                                    FileLogger.EventType.Error, 1001);
                                MessageBox.Show($"Failed to restart as administrator: {ex.Message}",
                                    "Elevation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show(connectionResult.Error, "Connection Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Failed to connect to {serverName}\n\nError: {connectionResult.Error}",
                            "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Connection error: {ex.Message}", FileLogger.EventType.Error, 1002);
                MessageBox.Show($"Connection error: {ex.Message}", Globals.MsgBox.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ButtonLogin.Text = originalText;
                ButtonLogin.Enabled = true;
                buttonCancel.Enabled = true;
                this.Cursor = Cursors.Default;
                _isConnecting = false; // Reset the flag

                FileLogger.Message($"Login attempt completed, resetting UI",
                    FileLogger.EventType.Information, 1045);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            Result = new LoginResult { Success = false, Cancelled = true };
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private class ConnectionTestResult
        {
            public bool Success { get; set; }
            public string Error { get; set; }
            public int VMCount { get; set; }
            public bool RequiresElevation { get; set; }
            public bool CanAutoElevate { get; set; }
            public bool IsLocal { get; set; }
        }

        private async Task<ConnectionTestResult> TestHyperVConnection(string serverName, PSCredential credential)
        {
            return await Task.Run(() =>
            {
                try
                {
                    bool isLocal = IsLocalComputer(serverName);

                    FileLogger.Message($"Testing connection to '{serverName}' (Local: {isLocal})...",
                        FileLogger.EventType.Information, 1003);

                    if (isLocal)
                    {
                        return TestLocalHyperV();
                    }
                    else
                    {
                        return TestRemoteHyperV(serverName, credential);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Connection test exception: {ex.Message}",
                        FileLogger.EventType.Error, 1004);
                    return new ConnectionTestResult
                    {
                        Success = false,
                        Error = ex.Message
                    };
                }
            });
        }

        private static bool IsLocalComputer(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                return true;

            computerName = computerName.Trim();

            // Check common local names
            if (computerName == "." || 
                string.Equals(computerName, "localhost", StringComparison.OrdinalIgnoreCase) ||
                computerName == "127.0.0.1" || 
                computerName == "::1")
                return true;

            // Check against actual computer name
            if (string.Equals(computerName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Check if it's a local IP address (IPv4)
            if (System.Text.RegularExpressions.Regex.IsMatch(computerName, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
            {
                try
                {
                    // Get all local IPv4 addresses
                    var localIPs = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                        .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                        .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                        .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(addr => addr.Address.ToString())
                        .Where(ip => !ip.StartsWith("169.254.")) // Exclude APIPA addresses
                        .Distinct()
                        .ToList();

                    if (localIPs.Contains(computerName))
                    {
                        FileLogger.Message($"'{computerName}' found in local IP addresses",
                            FileLogger.EventType.Information, 1043);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.Message($"Error checking local IP addresses: {ex.Message}",
                        FileLogger.EventType.Warning, 1046);
                }
            }

            // Check against hostname
            try
            {
                string hostname = System.Net.Dns.GetHostName();
                if (string.Equals(computerName, hostname, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Check against FQDN
                var hostEntry = System.Net.Dns.GetHostEntry(hostname);
                if (string.Equals(computerName, hostEntry.HostName, StringComparison.OrdinalIgnoreCase))
                    return true;

                // Check against all aliases
                foreach (var alias in hostEntry.Aliases)
                {
                    if (string.Equals(computerName, alias, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
                // Ignore DNS lookup failures
            }

            return false;
        }

        private ConnectionTestResult TestLocalHyperV()
        {
            try
            {
                FileLogger.Message($"Starting local Hyper-V module check...",
                    FileLogger.EventType.Information, 1030);

                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();
                    FileLogger.Message($"Local runspace opened successfully",
                        FileLogger.EventType.Information, 1031);

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = runspace;
                        ps.AddScript("Get-Module -ListAvailable -Name Hyper-V");

                        FileLogger.Message($"Checking for Hyper-V module...",
                            FileLogger.EventType.Information, 1032);

                        var moduleResult = ps.Invoke();

                        if (moduleResult != null && moduleResult.Count > 0)
                        {
                            FileLogger.Message($"Hyper-V module found, testing VM access...",
                                FileLogger.EventType.Information, 1033);

                            ps.Commands.Clear();
                            ps.AddScript("Get-VM -ErrorAction Stop");

                            try
                            {
                                FileLogger.Message($"Executing Get-VM command...",
                                    FileLogger.EventType.Information, 1034);

                                var vmResult = ps.Invoke();

                                FileLogger.Message($"Get-VM command completed, checking for errors...",
                                    FileLogger.EventType.Information, 1035);

                                // Check for errors in the error stream
                                if (ps.HadErrors)
                                {
                                    var error = ps.Streams.Error[0];
                                    string errorMessage = error.Exception?.Message ?? error.ToString();

                                    FileLogger.Message($"Local Hyper-V test error: {errorMessage}",
                                        FileLogger.EventType.Error, 1017);

                                    // Check if it's an elevation/permission issue
                                    if (errorMessage.Contains("required permission") ||
                                        errorMessage.Contains("Access is denied") ||
                                        errorMessage.Contains("Administrator") ||
                                        errorMessage.Contains("authorization policy") ||
                                        errorMessage.Contains("elevation"))
                                    {
                                        FileLogger.Message($"Access denied detected - elevation required",
                                            FileLogger.EventType.Warning, 1036);

                                        return new ConnectionTestResult
                                        {
                                            Success = false,
                                            Error = "Access denied. Administrator privileges or membership in the 'Hyper-V Administrators' group is required for local Hyper-V management.",
                                            RequiresElevation = true,
                                            CanAutoElevate = true
                                        };
                                    }

                                    return new ConnectionTestResult
                                    {
                                        Success = false,
                                        Error = $"Local Hyper-V test failed: {errorMessage}"
                                    };
                                }

                                int vmCount = vmResult?.Count ?? 0;

                                FileLogger.Message($"Local Hyper-V access successful. Found {vmCount} VMs.",
                                    FileLogger.EventType.Information, 1005);

                                return new ConnectionTestResult
                                {
                                    Success = true,
                                    VMCount = vmCount,
                                    IsLocal = true
                                };
                            }
                            catch (Exception ex)
                            {
                                string errorMessage = ex.Message;

                                FileLogger.Message($"Local Hyper-V test exception: {ex.GetType().Name} - {errorMessage}",
                                    FileLogger.EventType.Error, 1018);

                                // Check if it's an elevation/permission issue
                                if (errorMessage.Contains("required permission") ||
                                    errorMessage.Contains("Access is denied") ||
                                    errorMessage.Contains("Administrator") ||
                                    errorMessage.Contains("authorization policy") ||
                                    errorMessage.Contains("elevation"))
                                {
                                    FileLogger.Message($"Access denied detected in exception - elevation required",
                                        FileLogger.EventType.Warning, 1037);

                                    return new ConnectionTestResult
                                    {
                                        Success = false,
                                        Error = "Access denied. Administrator privileges or membership in the 'Hyper-V Administrators' group is required for local Hyper-V management.",
                                        RequiresElevation = true,
                                        CanAutoElevate = true
                                    };
                                }

                                return new ConnectionTestResult
                                {
                                    Success = false,
                                    Error = $"Local Hyper-V test failed: {errorMessage}"
                                };
                            }
                        }
                        else
                        {
                            FileLogger.Message($"Hyper-V module not found on local system",
                                FileLogger.EventType.Error, 1038);

                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = "Hyper-V module not found. Ensure Hyper-V is installed on this machine."
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Local Hyper-V test fatal error: {ex.GetType().Name} - {ex.Message}",
                    FileLogger.EventType.Error, 1006);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 1006);

                return new ConnectionTestResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private ConnectionTestResult TestRemoteHyperV(string serverName, PSCredential credential)
        {
            Runspace tempRunspace = null;
            PSObject tempSession = null;

            try
            {
                FileLogger.Message($"Testing remote connection to '{serverName}' (Remote) with credentials of {credential?.UserName ?? "Windows Authentication"}",
                    FileLogger.EventType.Information, 1007);

                // Test basic connectivity
                if (!TestNetworkConnection(serverName, 5985) && !TestNetworkConnection(serverName, 5986))
                {
                    return new ConnectionTestResult
                    {
                        Success = false,
                        Error = $"Cannot connect to '{serverName}' on WinRM ports (5985/5986). " +
                                "Ensure WinRM is enabled and accessible."
                    };
                }

                // Test PowerShell remoting
                tempRunspace = RunspaceFactory.CreateRunspace();
                tempRunspace.Open();

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = tempRunspace;

                    // Build New-PSSession command
                    ps.AddCommand("New-PSSession")
                      .AddParameter("ComputerName", serverName)
                      .AddParameter("ErrorAction", "Stop");

                    if (credential != null)
                    {
                        ps.AddParameter("Credential", credential);
                    }

                    FileLogger.Message($"Creating PowerShell session to '{serverName}'...",
                        FileLogger.EventType.Information, 1022);

                    var sessionResult = ps.Invoke();

                    if (ps.HadErrors)
                    {
                        var error = ps.Streams.Error[0];
                        string errorMsg = error.Exception?.Message ?? error.ToString();

                        FileLogger.Message($"PowerShell session creation failed: {errorMsg}",
                            FileLogger.EventType.Error, 1023);

                        return new ConnectionTestResult
                        {
                            Success = false,
                            Error = $"Failed to create PowerShell session: {errorMsg}"
                        };
                    }

                    if (sessionResult == null || sessionResult.Count == 0)
                    {
                        FileLogger.Message($"PowerShell session creation returned no results",
                            FileLogger.EventType.Error, 1024);

                        return new ConnectionTestResult
                        {
                            Success = false,
                            Error = $"Failed to create PowerShell session to '{serverName}'"
                        };
                    }

                    tempSession = sessionResult[0];
                    FileLogger.Message($"PowerShell session created successfully",
                        FileLogger.EventType.Information, 1025);

                    // Test Hyper-V availability
                    ps.Commands.Clear();
                    ps.AddCommand("Invoke-Command")
                      .AddParameter("Session", tempSession)
                      .AddParameter("ScriptBlock", ScriptBlock.Create(@"
                        try {
                            $module = Get-Module -ListAvailable -Name Hyper-V -ErrorAction SilentlyContinue
                            if ($module) {
                                $vms = Get-VM -ErrorAction SilentlyContinue
                                return @{ Available = $true; VMCount = ($vms | Measure-Object).Count }
                            }
                            return @{ Available = $false; VMCount = 0 }
                        }
                        catch {
                            return @{ Available = $false; Error = $_.Exception.Message }
                        }
                    "));

                    FileLogger.Message($"Testing Hyper-V availability on '{serverName}'...",
                        FileLogger.EventType.Information, 1026);

                    var hyperVResult = ps.Invoke();

                    if (ps.HadErrors)
                    {
                        var error = ps.Streams.Error[0];
                        string errorMsg = error.Exception?.Message ?? error.ToString();

                        FileLogger.Message($"Hyper-V test command failed: {errorMsg}",
                            FileLogger.EventType.Error, 1027);

                        return new ConnectionTestResult
                        {
                            Success = false,
                            Error = $"Failed to test Hyper-V on '{serverName}': {errorMsg}"
                        };
                    }

                    if (hyperVResult != null && hyperVResult.Count > 0)
                    {
                        var result = (PSObject)hyperVResult[0];
                        var hashtable = (System.Collections.Hashtable)result.BaseObject;
                        bool available = (bool)hashtable["Available"];
                        int vmCount = (int)hashtable["VMCount"];

                        if (available)
                        {
                            FileLogger.Message($"Remote Hyper-V access successful. Found {vmCount} VMs.",
                                FileLogger.EventType.Information, 1008);

                            return new ConnectionTestResult
                            {
                                Success = true,
                                VMCount = vmCount,
                                IsLocal = false
                            };
                        }
                        else
                        {
                            string error = hashtable.ContainsKey("Error") ? hashtable["Error"]?.ToString() : "Unknown error";

                            FileLogger.Message($"Hyper-V not available on '{serverName}': {error}",
                                FileLogger.EventType.Warning, 1028);

                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = $"Hyper-V module not available or accessible on '{serverName}'. {error}"
                            };
                        }
                    }

                    FileLogger.Message($"No results returned from Hyper-V test on '{serverName}'",
                        FileLogger.EventType.Error, 1029);

                    return new ConnectionTestResult
                    {
                        Success = false,
                        Error = $"No response from Hyper-V test on '{serverName}'"
                    };
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Remote connection test exception: {ex.GetType().Name} - {ex.Message}",
                    FileLogger.EventType.Error, 1009);
                FileLogger.Message($"Stack trace: {ex.StackTrace}",
                    FileLogger.EventType.Error, 1009);

                return new ConnectionTestResult
                {
                    Success = false,
                    Error = $"Connection test failed: {ex.Message}"
                };
            }
            finally
            {
                // Clean up the temporary session
                if (tempSession != null && tempRunspace != null)
                {
                    try
                    {
                        using (PowerShell ps = PowerShell.Create())
                        {
                            ps.Runspace = tempRunspace;
                            ps.AddCommand("Remove-PSSession")
                              .AddParameter("Session", tempSession);
                            ps.Invoke();
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                // Clean up the temporary runspace
                if (tempRunspace != null)
                {
                    try
                    {
                        tempRunspace.Close();
                        tempRunspace.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private bool TestNetworkConnection(string hostname, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(hostname, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                    if (success)
                    {
                        client.EndConnect(result);
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        #region Credential Storage

        private class SavedCredential
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private void SaveCredentials(string server, string username, string password)
        {
            try
            {
                if (!Directory.Exists(FileManager.ProgramDataFilePath))
                {
                    Directory.CreateDirectory(FileManager.ProgramDataFilePath);
                }

                // Create a safe filename from the server name
                string safeServerName = GetSafeFileName(server);
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, $"cred_{safeServerName}.dat");

                // Encrypt credentials using DPAPI
                byte[] serverBytes = Encoding.UTF8.GetBytes(server);
                byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                byte[] encryptedServer = ProtectedData.Protect(serverBytes, null, DataProtectionScope.CurrentUser);
                byte[] encryptedUsername = ProtectedData.Protect(usernameBytes, null, DataProtectionScope.CurrentUser);
                byte[] encryptedPassword = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);

                using (FileStream fs = new FileStream(credFile, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(encryptedServer.Length);
                    writer.Write(encryptedServer);
                    writer.Write(encryptedUsername.Length);
                    writer.Write(encryptedUsername);
                    writer.Write(encryptedPassword.Length);
                    writer.Write(encryptedPassword);
                }

                FileLogger.Message($"Credentials saved (encrypted) for server '{server}'",
                    FileLogger.EventType.Information, 1010);
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to save credentials: {ex.Message}",
                    FileLogger.EventType.Error, 1011);
            }
        }

        private SavedCredential LoadServerCredentials(string serverName)
        {
            try
            {
                // Create a safe filename from the server name
                string safeServerName = GetSafeFileName(serverName);
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, $"cred_{safeServerName}.dat");

                if (!File.Exists(credFile))
                    return null;

                using (FileStream fs = new FileStream(credFile, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int serverLength = reader.ReadInt32();
                    byte[] encryptedServer = reader.ReadBytes(serverLength);

                    int usernameLength = reader.ReadInt32();
                    byte[] encryptedUsername = reader.ReadBytes(usernameLength);

                    int passwordLength = reader.ReadInt32();
                    byte[] encryptedPassword = reader.ReadBytes(passwordLength);

                    byte[] serverBytes = ProtectedData.Unprotect(encryptedServer, null, DataProtectionScope.CurrentUser);
                    byte[] usernameBytes = ProtectedData.Unprotect(encryptedUsername, null, DataProtectionScope.CurrentUser);
                    byte[] passwordBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);

                    string storedServer = Encoding.UTF8.GetString(serverBytes);

                    // Verify the server name matches (security check)
                    if (!storedServer.Equals(serverName, StringComparison.OrdinalIgnoreCase))
                    {
                        FileLogger.Message($"Server name mismatch in credential file for '{serverName}'",
                            FileLogger.EventType.Warning, 1020);
                        return null;
                    }

                    return new SavedCredential
                    {
                        Username = Encoding.UTF8.GetString(usernameBytes),
                        Password = Encoding.UTF8.GetString(passwordBytes)
                    };
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to load credentials for '{serverName}': {ex.Message}",
                    FileLogger.EventType.Warning, 1013);
                return null;
            }
        }

        private void LoadSavedCredentials()
        {
            try
            {
                // Try to load the default/last used credentials file (legacy support)
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, "credentials.dat");

                if (!File.Exists(credFile))
                    return;

                using (FileStream fs = new FileStream(credFile, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    int serverLength = reader.ReadInt32();
                    byte[] encryptedServer = reader.ReadBytes(serverLength);

                    int usernameLength = reader.ReadInt32();
                    byte[] encryptedUsername = reader.ReadBytes(usernameLength);

                    int passwordLength = reader.ReadInt32();
                    byte[] encryptedPassword = reader.ReadBytes(passwordLength);

                    byte[] serverBytes = ProtectedData.Unprotect(encryptedServer, null, DataProtectionScope.CurrentUser);
                    byte[] usernameBytes = ProtectedData.Unprotect(encryptedUsername, null, DataProtectionScope.CurrentUser);
                    byte[] passwordBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);

                    textboxServer.Text = Encoding.UTF8.GetString(serverBytes);
                    textboxUsername.Text = Encoding.UTF8.GetString(usernameBytes);
                    textboxPassword.Text = Encoding.UTF8.GetString(passwordBytes);

                    radioCustom.Checked = true;
                    checkboxRemember.Checked = true;

                    FileLogger.Message("Legacy credentials loaded", FileLogger.EventType.Information, 1012);

                    // Migrate to new format
                    string server = textboxServer.Text;
                    string username = textboxUsername.Text;
                    string password = textboxPassword.Text;
                    SaveCredentials(server, username, password);

                    // Delete old file
                    try
                    {
                        File.Delete(credFile);
                        FileLogger.Message("Migrated credentials to new server-specific format",
                            FileLogger.EventType.Information, 1021);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to load legacy credentials: {ex.Message}",
                    FileLogger.EventType.Warning, 1013);
                // Silently fail - credentials might be corrupted or from different user
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                string serverName = textboxServer.Text.Trim();

                if (string.IsNullOrWhiteSpace(serverName))
                    return;

                // Delete server-specific credential file
                string safeServerName = GetSafeFileName(serverName);
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, $"cred_{safeServerName}.dat");

                if (File.Exists(credFile))
                {
                    File.Delete(credFile);
                    FileLogger.Message($"Saved credentials cleared for server '{serverName}'",
                        FileLogger.EventType.Information, 1014);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to clear credentials: {ex.Message}",
                    FileLogger.EventType.Warning, 1015);
            }
        }

        private string GetSafeFileName(string serverName)
        {
            // Remove invalid filename characters and convert to safe format
            string safe = serverName.ToLower();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(c, '_');
            }
            // Also replace some additional characters that might cause issues
            safe = safe.Replace('.', '_').Replace(':', '_').Replace('\\', '_').Replace('/', '_');
            return safe;
        }

        #endregion

        private void textboxServer_TextChanged(object sender, EventArgs e)
        {
            // Skip during form initialization
            if (_isInitializing)
                return;

            // Trim server name for spaces
            string serverName = textboxServer.Text.Trim();

            // Try to load saved credentials when server changes
            if (radioCustom.Checked && serverName.Length > 2 && serverName != _lastServerChecked)
            {
                // Update last server checked
                _lastServerChecked = serverName;

                // Try to load server-specific credentials
                var savedCreds = LoadServerCredentials(serverName);

                if (savedCreds != null)
                {
                    // Add username to UI
                    textboxUsername.Text = savedCreds.Username;
                    textboxPassword.Text = savedCreds.Password;

                    // Update UI
                    checkboxRemember.Checked = true;

                    // Log
                    FileLogger.Message($"Loading saved credentials for server '{serverName}' into application",
                        FileLogger.EventType.Information, 1019);
                }
                else
                {
                    // Clear credentials if no saved credentials found
                    if (!string.IsNullOrEmpty(textboxUsername.Text) || !string.IsNullOrEmpty(textboxPassword.Text))
                    {
                        textboxUsername.Text = string.Empty;
                        textboxPassword.Text = string.Empty;
                    }

                    // Update UI
                    checkboxRemember.Checked = false;
                }
            }
            else if (serverName.Length <= 2)
            {
                // Reset check when server name is too short
                _lastServerChecked = string.Empty;
            }
        }

        private void buttonHelpConnectGuide_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "🔍 To directly manage a single host, enter the IP address or host name. To manage multiple hosts, enter the IP address or name of a Cluster.\n\n" +
                "To connect to a Hyper-V server or Cluster, ensure the following prerequisites are met:\n\n" +
                "1. The Hyper-V/Cluster role(s) is installed on the target server.\n" +
                "2. PowerShell Remoting (WinRM) is enabled and accessible on the target server.\n" +
                "3. You have the necessary permissions to manage Hyper-V/Cluster on the target server.\n" +
                "4. If using custom credentials, ensure they are valid and have Hyper-V management rights.\n\n" +
                "For local connections, ensure you run this application with Administrator privileges or " +
                "that your user account is a member of the 'Hyper-V Administrators' group.",
                "Connection Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

