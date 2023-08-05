using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DSPi
{
    public class DriverInstaller
    {
        public static void InstallDriver(string infFilePath)
        {
            ExecutePnputilCommand($"/add-driver {infFilePath} /subdirs /install");
        }

        public static void UninstallDriver(string infFileName)
        {
            ExecutePnputilCommand($"/delete-driver {infFileName} /uninstall /force");
        }

        public static string GetDrivernameForDevice(string deviceDescription)
        {
            var output = ExecutePnputilCommand($"/enum-devices /class system");
            string drivername = null;

            var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(deviceDescription))
                {
                    string driverNameLine = null;
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (Regex.IsMatch(lines[j], @"^(Driver Name|驱动程序名称):\s+", RegexOptions.IgnoreCase))
                        {
                            driverNameLine = lines[j];
                            break;
                        }
                    }

                    if (driverNameLine != null)
                    {
                        drivername = Regex.Replace(driverNameLine, @"^(Driver Name|驱动程序名称):\s+", "", RegexOptions.IgnoreCase).Trim();
                        break;
                    }
                }
            }

            return drivername;
        }

        private static string ExecutePnputilCommand(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pnputil",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Close();

            process.Close();

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"Error executing pnputil: {error}");
            }

            return output;
        }
    }
}