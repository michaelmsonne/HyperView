using System;
using System.Collections.Generic;
using System.Text;

namespace HyperView.Class
{
    internal class FileManager
    {
        public static string ConfigIniPath
        {
            get
            {
                // Path to the configuration file
                var configIniPathvar = ProgramDataFilePath + @"\Config.ini";
                return configIniPathvar;
            }
        }
        public static string ProgramDataFilePath
        {
            get
            {
                // Path to the program data folder
                var programDataFilePathvar = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{Globals.ToolName.HyperView}";
                return programDataFilePathvar;
            }
        }

        public static string LogFilePath
        {
            get
            {
                // Root folder for log files
                var logfilePathvar = ProgramDataFilePath + @"\Log";
                return logfilePathvar;
            }
        }
    }
}
