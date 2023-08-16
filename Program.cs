using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static DSPi.HttpGet;

namespace DSPi
{
    class Program
    {
        private const string BaseUrl = "https://git.lnvpe.com/api/v1";

        static async Task Main(string[] args)
        {
            Console.Title = "Dispatcher Installer 1.0";

            int width = 100; // Set the desired width
            int height = 40; // Set the desired height
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            if (args.Length > 0 && args[0] == "update")
            {
                RunUpdate();
            }
            else
            {
                await Run();
            }
        }

        static void RunUpdate()
        {
            string cmdFilePath = @"C:\DSPi\UpdateDSPi.cmd"; // 指定要创建的CMD文件路径
            update updateExecutor = new update();

            updateExecutor.RunUpdate(cmdFilePath); // 设置CMD文件路径
            updateExecutor.CreateAndExecuteCmdFile(); // 创建并执行CMD文件

            Console.WriteLine("Update script has generated, plese <double click> <UpdataDSPi.cmd> to update this program!");

            string ScriptPath = @"C:\DSP_Files\UpdateDSPi.cmd";
            Process.Start("explorer.exe", "/select," + ScriptPath);

            Thread.Sleep(6000);
            Environment.Exit(0);
        }
        static async Task Run()
        {
            bool isTestModeEnabled = TestModeCheck.IsTestModeEnabled();
            if (!isTestModeEnabled)
            {
                if (PromptYesNo("Test mode is not enabled. Would you like to enable it? (Y/N)"))
                {
                    Console.WriteLine("");
                    TestModeCheck.EnableTestMode();
                    if (PromptYesNo("Restart to apply changes, Confirm? "))
                    {
                        Console.WriteLine("Restarting...");
                        Process.Start("shutdown", "/r /t 0");
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("TestMode Only become effective after reboot!");
                        Thread.Sleep(3000);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("In non-test mode usage, it may lead to issues which the driver cannot be installed.");
                    Thread.Sleep(3000);
                    //Environment.Exit(0);
                }
            }

            string driverName = DriverInstaller.CheckDriver("Lenovo Process Management");
            if (!string.IsNullOrEmpty(driverName))
            {
                Console.WriteLine("You've already installed Dispatcher, continue using this tool should uninstall Dispatcher first!");
                if (PromptYesNo("Do you want to uninstall Dispatcher? (y/n)"))
                {
                    DriverInstaller.UninstallDriver(driverName);
                    Console.WriteLine("Driver uninstalled successfully.");
                    if (PromptYesNo("A restart required, now restart? (y/n)"))
                    {
                        Console.WriteLine("Restarting...");
                        Process.Start("shutdown", "/r /t 0");
                    }
                    else
                    {
                        Console.WriteLine("You can restart the system later to apply the changes.");
                    }
                    return;
                }
                else
                {
                    Console.WriteLine("Driver uninstallation canceled by the user.");
                    Environment.Exit(0);
                }
                return;
            }
            else
            {
                Console.WriteLine("No Dispatcher Detected...");
            }

            bool isNetworkConnected = await IsNetworkConnected();
            if (!isNetworkConnected)
            {
                Console.WriteLine("Network connection is not available.");
                Thread.Sleep(3000);
                return;
            }

            var repositories = await GetRepositoriesAsync($"{BaseUrl}/users/tianyi/repos");
            if (repositories.Count == 0)
            {
                Console.WriteLine("No repositories found.");
                return;
            }

            int selectedIndex = ShowSelect.ShowRepositories(repositories);
            string full_name = null;

            if (selectedIndex >= 0 && selectedIndex < repositories.Count)
            {
                full_name = repositories[selectedIndex].full_name;
                Console.WriteLine("Selected repository: " + full_name);
            }
            else
            {
                Console.WriteLine("No repository selected.");
                return;
            }

            

            string releaseUrl = $"{BaseUrl}/repos/{full_name}/releases";
            var releases = await HttpGet.GetReleasesAsync(releaseUrl);

            if (releases.Count == 0)
            {
                Console.WriteLine("No releases found.");
                Thread.Sleep(3000);
                return;
            }

            int selectedReleaseIndex = ShowSelect.ShowReleases(releases);
            if (selectedReleaseIndex != -1)
            {
                await DownloadAndInstallDriver(releases[selectedReleaseIndex]);
                Console.WriteLine("Driver installed successfully. Press any key to exit...");
                Console.ReadLine();
            }
        }


        static bool PromptYesNo(string prompt)
        {
            Console.WriteLine(prompt);
            ConsoleKeyInfo response = Console.ReadKey();
            return response.KeyChar == 'y' || response.KeyChar == 'Y' || response.Key == ConsoleKey.Enter;
        }

        static async Task DownloadAndInstallDriver(Release release)
        {
            string downloadedFileName = await FileOperator.DownloadAndExtractReleaseAsync(release);
            string infFilePath = FileOperator.SearchForFile(downloadedFileName);

            if (!string.IsNullOrEmpty(infFilePath))
            {
                DriverInstaller.InstallDriver(infFilePath);
            }
        }
    }
}
