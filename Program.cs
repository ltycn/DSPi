using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static DSPi.HttpGet;

namespace DSPi
{
    class Program
    {
        private const string BaseUrl = "https://git.lnvpe.com/api/v1";
        private const string TestModePrompt = "Test mode is not enabled. Would you like to enable it? (Y/N)";
        private const string UninstallPrompt = "Do you want to uninstall Dispatcher? (y/n)";
        private const string RestartPrompt = "A restart required, now restart? (y/n)";
        private const string NetworkUnavailable = "Network connection is not available.";
        private const string NoRepositoriesFound = "No repositories found.";
        private const string NoReleasesFound = "No releases found.";
        private const string DriverInstalled = "Driver installed successfully. Press any key to exit...";

        static void Main(string[] args)
        {
            Console.Title = "Dispatcher Installer 1.0";

            int width = 100; // Set the desired width
            int height = 30; // Set the desired height
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            Run().Wait();
        }

        static async Task Run()
        {
            await CheckAndEnableTestMode();

            string driverName = DriverInstaller.CheckDriver("Lenovo Process Management");
            if (!string.IsNullOrEmpty(driverName))
            {
                Console.WriteLine("You've already installed Dispatcher, continue using this tool should uninstall Dispatcher first!");
                if (PromptYesNo(UninstallPrompt))
                {
                    DriverInstaller.UninstallDriver(driverName);
                    Console.WriteLine("Driver uninstalled successfully.");
                    if (PromptYesNo(RestartPrompt))
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
            }
            else
            {
                Console.WriteLine("No Dispatcher Detected...");
            }

            bool isNetworkConnected = await IsNetworkConnected();
            if (!isNetworkConnected)
            {
                Console.WriteLine(NetworkUnavailable);
                Thread.Sleep(3000);
                return;
            }

            var repositories = await GetRepositoriesAsync($"{BaseUrl}/users/tianyi/repos");
            if (repositories.Count == 0)
            {
                Console.WriteLine(NoRepositoriesFound);
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
                Console.WriteLine(NoReleasesFound);
                Thread.Sleep(3000);
                return;
            }

            int selectedReleaseIndex = ShowSelect.ShowReleases(releases);
            if (selectedReleaseIndex != -1)
            {
                await DownloadAndInstallDriver(releases[selectedReleaseIndex]);
                Console.WriteLine(DriverInstalled);
                Console.ReadLine();
            }
        }

        static async Task CheckAndEnableTestMode()
        {
            bool isTestModeEnabled = TestModeCheck.IsTestModeEnabled();
            if (!isTestModeEnabled)
            {
                Console.WriteLine(TestModePrompt);
                ConsoleKeyInfo response = Console.ReadKey();
                if (response.KeyChar == 'y' || response.KeyChar == 'Y' || response.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("");
                    TestModeCheck.EnableTestMode();
                    Console.WriteLine("Restart to apply changes, Confirm? ");
                    ConsoleKeyInfo needrestartrespose = Console.ReadKey();
                    if (needrestartrespose.KeyChar == 'y' || needrestartrespose.KeyChar == 'Y' || needrestartrespose.Key == ConsoleKey.Enter)
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
        }

        static bool PromptYesNo(string prompt)
        {
            Console.WriteLine(prompt);
            ConsoleKeyInfo response = Console.ReadKey();
            return response.KeyChar == 'y' || response.KeyChar == 'Y' || response.Key == ConsoleKey.Enter;
        }

        static async Task DownloadAndInstallDriver(Release release)
        {
            await FileOperator.DownloadAndExtractReleaseAsync(release);
            string infFilePath = FileOperator.SearchForFile();

            if (!string.IsNullOrEmpty(infFilePath))
            {
                DriverInstaller.InstallDriver(infFilePath);
            }
        }
    }
}
