using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;
using static DSPi.HttpGet;

namespace DSPi
{
    class Program
    {
        private const string BaseUrl = "https://git.lnvpe.com/api/v1";
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "update")
            {
                RunUpdate();
            }
            else
            {
                await Precheck();
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

            Process.Start("explorer.exe", "/select," + cmdFilePath);

            Thread.Sleep(6000);
            Environment.Exit(0);
        }
        public static async Task Precheck()
        {
            Application.Init();

            var top = Application.Top;

            var win = new Window("DISPATCHER INSTALLER v1.0")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            Console.Clear();
            await PerformPrecheck(win);

            Application.RequestStop();

        }

        public static async Task PerformPrecheck(Window parent)
        {

            if (!TestModeCheck.IsTestModeEnabled())
            {
                await Views.ShowTestModeDialogAsync(parent);
            }

            string driverName = DriverInstaller.CheckDriver("Lenovo Process Management");

            if (!string.IsNullOrEmpty(driverName))
            {
                await Views.ShowDispatcherDialogAsync(parent, driverName);
            }

            if (!IsNetworkConnected())
            {
                await Views.ShowNetworkDialogAsync(parent);
            }
        }

        static async Task Run() 
        {
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
                DriverInstaller.NeversleepCommands();
                Console.WriteLine("Driver installed successfully!");
                Console.WriteLine("System will reboot to make sure register is correctly flashed!");
                Thread.Sleep(5000);
                Process.Start("shutdown", "/r /t 0");
                Environment.Exit(0);
            }
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
