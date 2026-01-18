using System.Management.Automation;

namespace HyperView.Class
{
    public class VMGroupInfo
    {
        public string Name { get; set; }
        public string GroupType { get; set; }
        public string GroupTypeDisplay { get; set; }
        public int VMCount { get; set; }
        public string VMList { get; set; }
        public List<string> VMMembers { get; set; } = new List<string>();
        public int GroupCount { get; set; }
        public string GroupList { get; set; }
        public List<string> GroupMembers { get; set; } = new List<string>();
        public int TotalMembers { get; set; }
        public string ComputerName { get; set; }
        public string Id { get; set; }
    }

    public class VMGroupDeletionResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool CanForce { get; set; }
        public int VMCount { get; set; }
        public List<string> VMNames { get; set; } = new List<string>();
    }

    public class VMGroupCreationResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class VMGroupRenameResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class VMGroupMemberResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public static class VMGroups
    {
        public static List<VMGroupInfo> GetHyperVVMGroups(
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Retrieving VM Groups from '{SessionContext.ServerName}'...",
                    FileLogger.EventType.Information, 2061);

                // Build PowerShell script to get VM Groups
                string script = @"
                    $groups = Get-VMGroup -ErrorAction SilentlyContinue
                    $result = @()
                    
                    foreach ($group in $groups) {
                        $vmMembers = @()
                        $groupMembers = @()
                        
                        # Process VM members
                        if ($group.VMMembers -and $group.VMMembers.Count -gt 0) {
                            foreach ($vmMember in $group.VMMembers) {
                                if ($vmMember -and $vmMember.Name) {
                                    $vmMembers += $vmMember.Name
                                }
                            }
                        }
                        
                        # Process group members (nested groups)
                        if ($group.VMGroupMembers -and $group.VMGroupMembers.Count -gt 0) {
                            foreach ($groupMember in $group.VMGroupMembers) {
                                if ($groupMember -and $groupMember.Name) {
                                    $groupMembers += $groupMember.Name
                                }
                            }
                        }
                        
                        $groupObject = [PSCustomObject]@{
                            Name = $group.Name
                            GroupType = $group.GroupType.ToString()
                            VMMembers = $vmMembers
                            VMCount = $vmMembers.Count
                            VMList = if ($vmMembers.Count -gt 0) { $vmMembers -join ', ' } else { 'No VMs' }
                            GroupMembers = $groupMembers
                            GroupCount = $groupMembers.Count
                            GroupList = if ($groupMembers.Count -gt 0) { $groupMembers -join ', ' } else { 'No Nested Groups' }
                            TotalMembers = $vmMembers.Count + $groupMembers.Count
                            ComputerName = $group.ComputerName
                            Id = $group.Id
                        }
                        
                        $result += $groupObject
                    }
                    
                    return $result
                ";

                var results = executePowerShellCommand(script);

                if (results == null || results.Count == 0)
                {
                    FileLogger.Message("No VM Groups found",
                        FileLogger.EventType.Information, 2062);
                    return new List<VMGroupInfo>();
                }

                var vmGroups = new List<VMGroupInfo>();

                foreach (var result in results)
                {
                    try
                    {
                        var groupInfo = new VMGroupInfo
                        {
                            Name = result.Properties["Name"]?.Value?.ToString() ?? "Unknown",
                            GroupType = result.Properties["GroupType"]?.Value?.ToString() ?? "Unknown",
                            VMCount = Convert.ToInt32(result.Properties["VMCount"]?.Value ?? 0),
                            VMList = result.Properties["VMList"]?.Value?.ToString() ?? "",
                            GroupCount = Convert.ToInt32(result.Properties["GroupCount"]?.Value ?? 0),
                            GroupList = result.Properties["GroupList"]?.Value?.ToString() ?? "",
                            TotalMembers = Convert.ToInt32(result.Properties["TotalMembers"]?.Value ?? 0),
                            ComputerName = result.Properties["ComputerName"]?.Value?.ToString() ?? SessionContext.ServerName,
                            Id = result.Properties["Id"]?.Value?.ToString() ?? ""
                        };

                        // Process VM Members array
                        var vmMembersProperty = result.Properties["VMMembers"]?.Value;
                        if (vmMembersProperty != null && vmMembersProperty is System.Collections.IEnumerable enumerable)
                        {
                            foreach (var item in enumerable)
                            {
                                if (item != null)
                                    groupInfo.VMMembers.Add(item.ToString());
                            }
                        }

                        // Process Group Members array
                        var groupMembersProperty = result.Properties["GroupMembers"]?.Value;
                        if (groupMembersProperty != null && groupMembersProperty is System.Collections.IEnumerable groupEnum)
                        {
                            foreach (var item in groupEnum)
                            {
                                if (item != null)
                                    groupInfo.GroupMembers.Add(item.ToString());
                            }
                        }

                        // Set display name for group type
                        groupInfo.GroupTypeDisplay = groupInfo.GroupType switch
                        {
                            "VMCollectionType" => "Collection",
                            "ManagementCollectionType" => "Management",
                            _ => groupInfo.GroupType
                        };

                        vmGroups.Add(groupInfo);

                        FileLogger.Message($"Processed VM Group: '{groupInfo.Name}' ({groupInfo.GroupTypeDisplay}) - {groupInfo.VMCount} VMs",
                            FileLogger.EventType.Information, 2063);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Message($"Error processing VM Group result: {ex.Message}",
                            FileLogger.EventType.Warning, 2064);
                    }
                }

                FileLogger.Message($"Successfully retrieved {vmGroups.Count} VM Groups",
                    FileLogger.EventType.Information, 2065);

                return vmGroups;
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Error retrieving VM Groups: {ex.Message}",
                    FileLogger.EventType.Error, 2066);
                return new List<VMGroupInfo>();
            }
        }

        public static VMGroupDeletionResult RemoveHyperVVMGroup(
            string groupName,
            bool force,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Checking VM Group '{groupName}' before removal...",
                    FileLogger.EventType.Information, 2050);

                // Get the VM group and check if it contains VMs
                var checkScript = $@"
                    $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                    $vmMembers = $group.VMMembers
                    $vmNames = @()
                    foreach ($vm in $vmMembers) {{
                        if ($vm.Name) {{
                            $vmNames += $vm.Name
                        }}
                    }}
                    @{{ 
                        VMCount = $vmMembers.Count
                        VMNames = $vmNames
                        GroupExists = $true
                    }}
                ";

                var checkResults = executePowerShellCommand(checkScript);

                if (checkResults == null || checkResults.Count == 0)
                {
                    return new VMGroupDeletionResult
                    {
                        Success = false,
                        Error = $"VM Group '{groupName}' does not exist"
                    };
                }

                int vmCount = 0;
                List<string> vmNames = new List<string>();

                var result = checkResults[0];
                if (result.BaseObject is System.Collections.Hashtable hashtable)
                {
                    vmCount = Convert.ToInt32(hashtable["VMCount"] ?? 0);

                    if (hashtable["VMNames"] != null && hashtable["VMNames"] is System.Collections.IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (item != null && !string.IsNullOrWhiteSpace(item.ToString()))
                                vmNames.Add(item.ToString());
                        }
                    }
                }

                FileLogger.Message($"VM Group '{groupName}' contains {vmCount} VM(s): {string.Join(", ", vmNames)}",
                    FileLogger.EventType.Information, 2051);

                // If group contains VMs and not forcing, return error
                if (vmCount > 0 && !force)
                {
                    FileLogger.Message($"VM Group '{groupName}' cannot be deleted without force - contains {vmCount} VM(s)",
                        FileLogger.EventType.Warning, 2052);

                    return new VMGroupDeletionResult
                    {
                        Success = false,
                        Error = $"Cannot delete VM Group '{groupName}' because it contains {vmCount} VM(s). Use Force to delete anyway.",
                        CanForce = true,
                        VMCount = vmCount,
                        VMNames = vmNames
                    };
                }

                // If force deletion and group has VMs, remove VMs from group first
                if (force && vmCount > 0)
                {
                    FileLogger.Message($"Force deletion - removing {vmCount} VM(s) from group '{groupName}' first...",
                        FileLogger.EventType.Information, 2072);

                    foreach (var vmName in vmNames)
                    {
                        try
                        {
                            FileLogger.Message($"Removing VM '{vmName}' from group '{groupName}'...",
                                FileLogger.EventType.Information, 2073);

                            // Use correct Remove-VMGroupMember format
                            var removeVMScript = $@"
                                $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                                $vm = Get-VM -Name '{vmName}' -ErrorAction Stop
                                Remove-VMGroupMember -VMGroup $group -VM $vm -ErrorAction Stop
                            ";

                            var removeVMResult = executePowerShellCommand(removeVMScript);

                            FileLogger.Message($"Successfully removed VM '{vmName}' from group '{groupName}'",
                                FileLogger.EventType.Information, 2074);
                        }
                        catch (Exception ex)
                        {
                            FileLogger.Message($"Error removing VM '{vmName}' from group '{groupName}': {ex.Message}",
                                FileLogger.EventType.Error, 2076);
                        }
                    }

                    FileLogger.Message($"Finished removing VMs from group '{groupName}'",
                        FileLogger.EventType.Information, 2077);
                }

                // Remove the VM group (it should now be empty)
                FileLogger.Message($"Deleting VM Group '{groupName}'...",
                    FileLogger.EventType.Information, 2078);

                var removeGroupScript = $"Remove-VMGroup -Name '{groupName}' -Force -ErrorAction Stop";
                var removeGroupResult = executePowerShellCommand(removeGroupScript);

                if (removeGroupResult == null)
                {
                    return new VMGroupDeletionResult
                    {
                        Success = false,
                        Error = "Failed to remove VM Group. Check logs for details."
                    };
                }

                FileLogger.Message($"VM Group '{groupName}' removed successfully",
                    FileLogger.EventType.Information, 2054);

                return new VMGroupDeletionResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception removing VM Group '{groupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2055);

                return new VMGroupDeletionResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static void RefreshVMGroupsView(
            string reason,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand,
            Action<List<VMGroupInfo>> updateDataGridView)
        {
            try
            {
                FileLogger.Message($"Refreshing VM Groups view - Reason: {reason}",
                    FileLogger.EventType.Information, 2079);

                // Get fresh VM Groups data
                var vmGroups = GetHyperVVMGroups(executePowerShellCommand);

                if (vmGroups != null && updateDataGridView != null)
                {
                    FileLogger.Message($"VM Groups view refreshed with {vmGroups.Count} groups",
                        FileLogger.EventType.Information, 2080);
                    
                    // Update the DataGridView
                    updateDataGridView(vmGroups);
                }
                else
                {
                    FileLogger.Message("DataGridView update callback not available",
                        FileLogger.EventType.Information, 2081);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Failed to refresh VM Groups view: {ex.Message}",
                    FileLogger.EventType.Warning, 2082);
            }
        }

        public static VMGroupCreationResult CreateHyperVVMGroup(
            string groupName,
            string groupType,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Executing New-VMGroup command for '{groupName}'...",
                    FileLogger.EventType.Information, 2035);

                // Build PowerShell command
                string command = $"New-VMGroup -Name '{groupName}' -GroupType {groupType}";

                var results = executePowerShellCommand(command);

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

        public static VMGroupRenameResult RenameHyperVVMGroup(
            string oldGroupName,
            string newGroupName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Renaming VM Group from '{oldGroupName}' to '{newGroupName}'...",
                    FileLogger.EventType.Information, 2090);

                // Build PowerShell command
                string command = $"Rename-VMGroup -Name '{oldGroupName}' -NewName '{newGroupName}' -ErrorAction Stop";

                var results = executePowerShellCommand(command);

                if (results == null)
                {
                    string error = "Failed to rename VM Group. Check logs for details.";
                    FileLogger.Message($"VM Group rename failed: {error}",
                        FileLogger.EventType.Error, 2091);

                    return new VMGroupRenameResult
                    {
                        Success = false,
                        Error = error
                    };
                }

                FileLogger.Message($"VM Group renamed successfully from '{oldGroupName}' to '{newGroupName}'",
                    FileLogger.EventType.Information, 2092);

                return new VMGroupRenameResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception renaming VM Group '{oldGroupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2093);

                return new VMGroupRenameResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static VMGroupMemberResult AddVMToGroup(
            string vmName,
            string groupName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Adding VM '{vmName}' to group '{groupName}'...",
                    FileLogger.EventType.Information, 2117);

                // Build PowerShell command
                string command = $@"
                    $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                    $vm = Get-VM -Name '{vmName}' -ErrorAction Stop
                    Add-VMGroupMember -VMGroup $group -VM $vm -ErrorAction Stop
                ";

                var results = executePowerShellCommand(command);

                if (results == null)
                {
                    string error = "Failed to add VM to group. Check logs for details.";
                    FileLogger.Message($"Failed to add VM '{vmName}' to group '{groupName}': {error}",
                        FileLogger.EventType.Error, 2118);

                    return new VMGroupMemberResult
                    {
                        Success = false,
                        Error = error
                    };
                }

                FileLogger.Message($"Successfully added VM '{vmName}' to group '{groupName}'",
                    FileLogger.EventType.Information, 2119);

                return new VMGroupMemberResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception adding VM '{vmName}' to group '{groupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2120);

                return new VMGroupMemberResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public static VMGroupMemberResult RemoveVMFromGroup(
            string vmName,
            string groupName,
            Func<string, System.Collections.ObjectModel.Collection<PSObject>> executePowerShellCommand)
        {
            try
            {
                FileLogger.Message($"Removing VM '{vmName}' from group '{groupName}'...",
                    FileLogger.EventType.Information, 2121);

                // Build PowerShell command
                string command = $@"
                    $group = Get-VMGroup -Name '{groupName}' -ErrorAction Stop
                    $vm = Get-VM -Name '{vmName}' -ErrorAction Stop
                    Remove-VMGroupMember -VMGroup $group -VM $vm -ErrorAction Stop
                ";

                var results = executePowerShellCommand(command);

                if (results == null)
                {
                    string error = "Failed to remove VM from group. Check logs for details.";
                    FileLogger.Message($"Failed to remove VM '{vmName}' from group '{groupName}': {error}",
                        FileLogger.EventType.Error, 2122);

                    return new VMGroupMemberResult
                    {
                        Success = false,
                        Error = error
                    };
                }

                FileLogger.Message($"Successfully removed VM '{vmName}' from group '{groupName}'",
                    FileLogger.EventType.Information, 2123);

                return new VMGroupMemberResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                FileLogger.Message($"Exception removing VM '{vmName}' from group '{groupName}': {ex.Message}",
                    FileLogger.EventType.Error, 2124);

                return new VMGroupMemberResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}
