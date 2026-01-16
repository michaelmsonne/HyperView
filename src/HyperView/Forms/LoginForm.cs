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

        public LoginForm()
        {
            InitializeComponent();
            InitializeFormEvents();
            SetDefaultServer();
            LoadSavedCredentials();
            SetToolName();
        }

        private void SetDefaultServer()
        {
            // Set default server to current hostname if textbox is empty
            if (string.IsNullOrWhiteSpace(textboxServer.Text))
            {
                try
                {
                    textboxServer.Text = Environment.MachineName;
                }
                catch
                {
                    // If unable to get machine name, leave empty
                    textboxServer.Text = string.Empty;
                }
            }
        }

        private void SetToolName()
        {
            labelLoginFormToolName.Text = Globals.ToolName.HyperViewGui;
            Text = $"{Globals.ToolName.HyperView} - Login";
        }

        private void InitializeFormEvents()
        {
            // Wire up event handlers
            ButtonLogin.Click += ButtonLogin_Click;
            buttonCancel.Click += ButtonCancel_Click;
            radioWindows.CheckedChanged += RadioAuth_CheckedChanged;
            radioCustom.CheckedChanged += RadioAuth_CheckedChanged;
            textboxUsername.KeyDown += TextboxUsername_KeyDown;
            textboxPassword.KeyDown += TextboxPassword_KeyDown;
            textboxServer.KeyDown += TextboxServer_KeyDown;

            // Set password char
            textboxPassword.UseSystemPasswordChar = true;

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
                ButtonLogin.PerformClick();
            }
        }

        private async void ButtonLogin_Click(object sender, EventArgs e)
        {
            string serverName = textboxServer.Text.Trim();

            // Validate input
            if (string.IsNullOrWhiteSpace(serverName))
            {
                MessageBox.Show("Please enter a server name or IP address.", Globals.MsgBox.Warning,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textboxServer.Focus();
                return;
            }

            if (radioCustom.Checked)
            {
                if (string.IsNullOrWhiteSpace(textboxUsername.Text))
                {
                    MessageBox.Show("Please enter a username.", Globals.MsgBox.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textboxUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(textboxPassword.Text))
                {
                    MessageBox.Show("Please enter a password.", Globals.MsgBox.Warning,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textboxPassword.Focus();
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

            // Disable UI and show progress
            string originalText = ButtonLogin.Text;
            ButtonLogin.Text = "Connecting...";
            ButtonLogin.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

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
                    this.Hide();

                    using (MainForm mainForm = new MainForm())
                    {
                        var mainResult = mainForm.ShowDialog();

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
                this.Cursor = Cursors.Default;
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

        private bool IsLocalComputer(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                return true;

            computerName = computerName.Trim().ToLower();

            // Check common local names
            if (computerName == "." || computerName == "localhost" ||
                computerName == "127.0.0.1" || computerName == "::1")
                return true;

            // Check against actual computer name
            string localName = Environment.MachineName.ToLower();
            if (computerName == localName)
                return true;

            // Check against hostname
            try
            {
                string hostname = System.Net.Dns.GetHostName().ToLower();
                if (computerName == hostname)
                    return true;

                // Check against FQDN
                var hostEntry = System.Net.Dns.GetHostEntry(hostname);
                if (computerName == hostEntry.HostName.ToLower())
                    return true;

                // Check against all aliases
                foreach (var alias in hostEntry.Aliases)
                {
                    if (computerName == alias.ToLower())
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
                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = runspace;
                        ps.AddScript("Get-Module -ListAvailable -Name Hyper-V");

                        var moduleResult = ps.Invoke();

                        if (moduleResult != null && moduleResult.Count > 0)
                        {
                            ps.Commands.Clear();
                            ps.AddScript("Get-VM -ErrorAction Stop");

                            try
                            {
                                var vmResult = ps.Invoke();

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

                                FileLogger.Message($"Local Hyper-V access successful.",
                                    FileLogger.EventType.Information, 1005);
                                // FileLogger.Message($"Local Hyper-V access successful. Found {vmCount} VMs.", 
                                //    FileLogger.EventType.Information, 1005);

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

                                FileLogger.Message($"Local Hyper-V test exception: {errorMessage}",
                                    FileLogger.EventType.Error, 1018);

                                // Check if it's an elevation/permission issue
                                if (errorMessage.Contains("required permission") ||
                                    errorMessage.Contains("Access is denied") ||
                                    errorMessage.Contains("Administrator") ||
                                    errorMessage.Contains("authorization policy") ||
                                    errorMessage.Contains("elevation"))
                                {
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
                FileLogger.Message($"Local Hyper-V test error: {ex.Message}",
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
                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();

                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = runspace;

                        // Build New-PSSession command
                        ps.AddCommand("New-PSSession")
                          .AddParameter("ComputerName", serverName)
                          .AddParameter("ErrorAction", "Stop");

                        if (credential != null)
                        {
                            ps.AddParameter("Credential", credential);
                        }

                        var sessionResult = ps.Invoke();

                        if (ps.HadErrors)
                        {
                            var error = ps.Streams.Error[0];
                            return new ConnectionTestResult
                            {
                                Success = false,
                                Error = $"Failed to create PowerShell session: {error.Exception.Message}"
                            };
                        }

                        if (sessionResult != null && sessionResult.Count > 0)
                        {
                            var session = sessionResult[0];

                            // Test Hyper-V availability
                            ps.Commands.Clear();
                            ps.AddCommand("Invoke-Command")
                              .AddParameter("Session", session)
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

                            var hyperVResult = ps.Invoke();

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
                                    return new ConnectionTestResult
                                    {
                                        Success = false,
                                        Error = $"Hyper-V module not available or accessible on '{serverName}'"
                                    };
                                }
                            }
                        }

                        return new ConnectionTestResult
                        {
                            Success = false,
                            Error = $"Failed to create PowerShell session to '{serverName}'"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Remote connection test error: {ex.Message}",
                    FileLogger.EventType.Error, 1009);
                return new ConnectionTestResult
                {
                    Success = false,
                    Error = ex.Message
                };
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

        private void SaveCredentials(string server, string username, string password)
        {
            try
            {
                if (!Directory.Exists(FileManager.ProgramDataFilePath))
                {
                    Directory.CreateDirectory(FileManager.ProgramDataFilePath);
                }

                string credFile = Path.Combine(FileManager.ProgramDataFilePath, "credentials.dat");

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

                FileLogger.Message("Credentials saved (encrypted)", FileLogger.EventType.Information, 1010);
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to save credentials: {ex.Message}",
                    FileLogger.EventType.Error, 1011);
            }
        }

        private void LoadSavedCredentials()
        {
            try
            {
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

                    FileLogger.Message("Credentials loaded", FileLogger.EventType.Information, 1012);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to load credentials: {ex.Message}",
                    FileLogger.EventType.Warning, 1013);
                // Silently fail - credentials might be corrupted or from different user
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                string credFile = Path.Combine(FileManager.ProgramDataFilePath, "credentials.dat");

                if (File.Exists(credFile))
                {
                    File.Delete(credFile);
                    FileLogger.Message("Saved credentials cleared", FileLogger.EventType.Information, 1014);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to clear credentials: {ex.Message}",
                    FileLogger.EventType.Warning, 1015);
            }
        }

        #endregion
    }
}

