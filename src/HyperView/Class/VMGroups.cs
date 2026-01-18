using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

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
    }
}
