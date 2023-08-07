using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DSPi
{
    public class TestModeCheck
    {
        public static bool IsTestModeEnabled()
        {
            // 执行 bcdedit /enum 命令，并获取输出结果
            Process process = new Process();
            process.StartInfo.FileName = "bcdedit";
            process.StartInfo.Arguments = "/enum";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // 在输出结果中查找 nointegritychecks 和 testsigning 字符串
            bool isNointegritychecksFound = output.Contains("nointegritychecks       Yes");
            bool isTestSigningFound = output.Contains("testsigning             Yes");

            // 判断是否找到 nointegritychecks 和 testsigning 字符串
            if (isNointegritychecksFound && isTestSigningFound)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void EnableTestMode()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // Run as administrator
                };

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.Start();

                    using (var sw = process.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            // Combine both commands into a single line using "&" to run them simultaneously
                            sw.WriteLine("bcdedit /set nointegritychecks on & bcdedit /set testsigning on");
                            sw.WriteLine("exit");
                        }
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Check if the output contains the word "success"
                    if (output.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Console.WriteLine("Test mode has been enabled successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to enable test mode. Have you Disabled Security Boot in BIOS ?.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while enabling test mode: " + ex.Message);
            }
        }

    }
}