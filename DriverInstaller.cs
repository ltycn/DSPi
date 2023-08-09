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
            return;
        }

        public static void UninstallDriver(string infFileName)
        {
            ExecutePnputilCommand($"/delete-driver {infFileName} /uninstall /force");
            RestorePowerCfgCommands();
            return;
        }

        public static string CheckDriver(string deviceDescription)
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

        public static void RestorePowerCfgCommands()
        {
            try
            {
                // 第一个命令：powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
                RunCommand("powercfg", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

                // 第二个命令：powercfg -restoredefaultschemes
                RunCommand("powercfg", "-restoredefaultschemes");

                // 第三个命令：powercfg /setactive 381b4222-f694-41f0-9685-ff5bb260df2e
                RunCommand("powercfg", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e");

                Console.WriteLine("Restored PowerConfig.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("error：" + ex.Message);
            }
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

        private static void RunCommand(string command, string arguments)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + command + " " + arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(output))
                Console.WriteLine(output);

            if (!string.IsNullOrWhiteSpace(error))
                Console.WriteLine(error);
        }
    }

}