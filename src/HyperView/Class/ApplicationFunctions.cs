using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace HyperView.Class
{
    internal class ApplicationFunctions
    {
        public static bool IsFileCodeSignedByMicrosoft(string filePath)
        {
            try
            {
                // Load the file's certificate  
                var cert = new X509Certificate2(filePath);

                // Display certificate details in a MessageBox
#if DEBUG
                MessageBox.Show(
                    $@"Certificate Details:  
        Subject: {cert.Subject}  
        Issuer: {cert.Issuer}  
        Thumbprint: {cert.Thumbprint}  
        Effective Date: {cert.NotBefore}  
        Expiration Date: {cert.NotAfter}",
                    @"Certificate Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
#endif
                // Check the issuer name to verify it's signed by Microsoft  
                var issuer = cert.Issuer;
                if (issuer.Contains("CN=Microsoft Corporation") || issuer.Contains("CN=Microsoft Code Signing PCA"))
                {
                    // Check if the certificate is valid
                    if (cert.NotAfter > DateTime.Now && cert.NotBefore < DateTime.Now)
                    {
#if DEBUG
                        MessageBox.Show($@"The file '{filePath}' is code signed by Microsoft.", @"Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
                        return true;
                    }
                    else
                    {
#if DEBUG
                        MessageBox.Show($@"The certificate is expired or not a Microsoft valid one for the file '{filePath}'.", @"Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
                        return false;
                    }
                }
            }
            catch (CryptographicException)
            {
                // Return false if the file is not signed or any cryptographic error occurs  
                return false;
            }
            catch
            {
                // Return false for any other exceptions  
                return false;
            }

            // Default return value if no conditions are met
            return false;
        }

        /// <summary>
        /// Checks if the current machine is running in Azure.
        /// </summary>
        /// <returns>True if the machine is in Azure, otherwise false.</returns>
        public static bool IsRunningInAzureOrHardware()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("Metadata", "true");
                    string metadataUrl = "http://169.254.169.254/metadata/instance/compute?api-version=2019-06-01";
                    string response = client.DownloadString(metadataUrl);

#if DEBUG
                    MessageBox.Show(response, @"Azure Metadata", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif

                    if (!string.IsNullOrEmpty(response) && response.Contains("resourceId"))
                    {
                        // Check for MSFT_ARC_TEST environment variable
                        string overrideTest = Environment.GetEnvironmentVariable("MSFT_ARC_TEST", EnvironmentVariableTarget.Machine);
                        if ("true".Equals(overrideTest, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show(
                                @"Running on an Azure Virtual Machine with MSFT_ARC_TEST set.
        Azure Connected Machine Agent is designed for use outside Azure.
        This virtual machine should only be used for testing purposes.
        See https://aka.ms/azcmagent-testwarning for more details.",
                                @"Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                @"Cannot install Azure Connected Machine agent on an Azure Virtual Machine.
        Azure Connected Machine Agent is designed for use outside Azure.
        To connect an Azure VM for TESTING PURPOSES ONLY, see https://aka.ms/azcmagent-testwarning for more details."
                            );
                        }

                        return true;
                    }
                }
            }
            catch (WebException ex) when (ex.Message.Contains("Unable to connect to the remote server"))
            {
#if DEBUG
                MessageBox.Show(@"Unable to connect to the remote server. This may indicate the machine is not running in Azure due to network restrictions.", @"Network Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
#endif
                // Check if the machine is physical hardware
                if (IsPhysicalHardware())
                {
#if DEBUG
                    MessageBox.Show(@"The machine is identified as physical hardware.", @"Hardware Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, @"Error checking if running in Azure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Ignore other exceptions and assume not running in Azure
            }

            return false;
        }

        public static bool IsRunningAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Function to restart the application is running as administrator
        public static void RestartAsAdmin()
        {
            // Get the current process
            var process = Process.GetCurrentProcess();
            // Create a new process start info
            if (process.MainModule != null)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = process.MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas" // This will run the process as administrator
                };
                // Start the new process
                Process.Start(startInfo);
            }

            // Exit the current process
            Environment.Exit(0);
        }

        /// <summary>
        /// Gets the physical hardware status of the machine.
        /// </summary>
        /// <returns>True if the machine is physical hardware, otherwise false.</returns>
        private static bool IsPhysicalHardware()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem")) // No changes here
                {
                    foreach (var obj in searcher.Get())
                    {
                        var manufacturer = obj["Manufacturer"]?.ToString() ?? string.Empty;
                        var model = obj["Model"]?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(model))
                        {
                            if (manufacturer.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                model.IndexOf("Virtual", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                return false; // Virtual machine
                            }

                        }
                    }
                }
            }
            catch
            {
                // Handle or log exceptions if necessary
            }

            return true; // Assume physical hardware if no virtual indicators are found
        }
    }
}
