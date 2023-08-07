using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace DSPi
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static readonly string downloadPath = @"C:\DSP_Files\";

        static void Main(string[] args)
        {

            // Set the console window width and height
            int width = 120; // Set the desired width
            int height = 40; // Set the desired height
            Console.SetWindowSize(width, height);

            // Set the console buffer width and height
            Console.SetBufferSize(width, height);


            bool isTestModeEnabled = TestModeCheck.IsTestModeEnabled();
            if (!isTestModeEnabled)
            {
                Console.WriteLine("Test mode is not enabled. Would you like to enable it? (Y/N)");
                var ModeCheckRespose = Console.ReadLine()?.ToLower();

                if (ModeCheckRespose == "y" || string.IsNullOrEmpty(ModeCheckRespose))
                {
                    TestModeCheck.EnableTestMode();
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            string drivername = DriverInstaller.GetDrivernameForDevice("Lenovo Process Management");

            if (!string.IsNullOrEmpty(drivername))
            {
                Console.WriteLine("You've already installed Dispatcher, continue using this tool should uninstall Dispatcher first!");

                Console.WriteLine("Do you want to uninstall Dispatcher? (y/n)");
                var userResponse = Console.ReadLine()?.ToLower();

                if (userResponse == "y" || string.IsNullOrEmpty(userResponse))
                {
                    DriverInstaller.UninstallDriver(drivername);
                    Console.WriteLine("Driver uninstalled successfully.");
                    Console.WriteLine("A restart required，now restart?");
                    var restartResponse = Console.ReadLine()?.ToLower();

                    if (restartResponse == "y" || string.IsNullOrEmpty(restartResponse))
                    {
                        Console.WriteLine("Restarting...");
                        Process.Start("shutdown", "/r /t 0");
                    }
                    else
                    {
                        Console.WriteLine("You can restart the system later to apply the changes.");
                    }
                }
                else if (userResponse == "n")
                {
                    Console.WriteLine("Driver uninstallation canceled by the user.");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Invalid response. Driver uninstallation canceled.");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("No Dispatcher Detected...");
            }
            Run().Wait();
        }

        static async Task Run()
        {

            bool isNetworkConnected = await IsNetworkConnected();
            if (!isNetworkConnected)
            {
                Console.WriteLine("Network connection is not available.");
                Thread.Sleep(5000);
                return;
            }

            var releases = await GetReleasesAsync("https://git.lnvpe.com/api/v1/repos/tianyi/LNVProcessManagement/releases");

            if (releases.Count == 0)
            {
                Console.WriteLine("No releases found.");
                Thread.Sleep(5000);
                return;
            }

            int selectedReleaseIndex = ShowReleases(releases);

            if (selectedReleaseIndex != -1)
            {
                await DownloadAndExtractReleaseAsync(releases[selectedReleaseIndex]);
                string infFilePath = SearchForFile();

                if (!string.IsNullOrEmpty(infFilePath))
                {
                    DriverInstaller.InstallDriver(infFilePath);
                    Console.WriteLine("Driver installed successfully.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadLine();
                }
            }
        }
        public static async Task<bool> IsNetworkConnected()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, "https://git.lnvpe.com"))
                    {
                        var response = await client.SendAsync(request);
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        static async Task<List<Release>> GetReleasesAsync(string url)
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var releasesJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Release>>(releasesJson);
        }

        static int ShowReleases(List<Release> releases)
        {

            int selectedReleaseIndex = 0;
            while (true)
            {
                Console.Clear();
                string logo = @"
**********************************************************************

__/\\\\\\\\\\\\________/\\\\\\\\\\\____/\\\\\\\\\\\\\_________        
 _\/\\\////////\\\____/\\\/////////\\\_\/\\\/////////\\\_______       
  _\/\\\______\//\\\__\//\\\______\///__\/\\\_______\/\\\__/\\\_      
   _\/\\\_______\/\\\___\////\\\_________\/\\\\\\\\\\\\\/__\///__     
    _\/\\\_______\/\\\______\////\\\______\/\\\/////////_____/\\\_    
     _\/\\\_______\/\\\_________\////\\\___\/\\\_____________\/\\\_   
      _\/\\\_______/\\\___/\\\______\//\\\__\/\\\_____________\/\\\_  
       _\/\\\\\\\\\\\\/___\///\\\\\\\\\\\/___\/\\\_____________\/\\\_ 
        _\////////////_______\///////////_____\///______________\///__

Dispatcher Installer 1.0.0                     github.com/ltycn/DSPi
***********************************************************************
";
                Console.WriteLine(logo);
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select to choose a version of Dispatcher:");
                Console.ResetColor();
                for (int i = 0; i < releases.Count; i++)
                {
                    var release = releases[i];
                    var highlight = (i == selectedReleaseIndex) ? " >> " : "    ";
                    Console.ForegroundColor = (i == selectedReleaseIndex) ? ConsoleColor.Green : ConsoleColor.White;
                    Console.WriteLine($"{highlight}{release.name} (Tag: {release.tag_name})");
                    Console.ResetColor();
                }

                Console.WriteLine("==========================================================");
                var selectedRelease = releases[selectedReleaseIndex];
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Release Note:");
                Console.ResetColor();
                Console.WriteLine($"{selectedRelease.body}");
                Console.WriteLine("==========================================================");
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Asset Information:");
                Console.ResetColor();
                Console.WriteLine($"Uploaded By: {selectedRelease.author.username}");
                Console.WriteLine($"Email: {selectedRelease.author.email}");
                Console.WriteLine($"Asset: {selectedRelease.assets[0].name}");
                Console.WriteLine($"URL: {selectedRelease.assets[0].browser_download_url}");
                Console.WriteLine($"Size: {selectedRelease.assets[0].size} bytes");
                Console.WriteLine("==========================================================");
                Console.WriteLine("Press Enter to install...");

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.UpArrow && selectedReleaseIndex > 0)
                {
                    selectedReleaseIndex--;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow && selectedReleaseIndex < releases.Count - 1)
                {
                    selectedReleaseIndex++;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedReleaseIndex;
                }
            }
        }

        static async Task DownloadAndExtractReleaseAsync(Release release)
        {
            using (var webClient = new System.Net.WebClient())
            {
                Console.WriteLine($"Downloading {release.name}...");

                // Create the download directory if it doesn't exist
                if (!Directory.Exists(downloadPath))
                {
                    Directory.CreateDirectory(downloadPath);
                }

                string filePath = Path.Combine(downloadPath, release.assets[0].name);

                try
                {
                    await webClient.DownloadFileTaskAsync(release.assets[0].browser_download_url, filePath);
                    Console.WriteLine($"Download complete. File saved to: {filePath}");

                    // Extract the downloaded release file
                    string extractPath = Path.Combine(downloadPath, "Extracted");
                    ExtractZipArchive(filePath, extractPath);
                    Console.WriteLine($"Release extracted to: {extractPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading or extracting file: {ex.Message}");
                }
            }
        }

        static void ExtractZipArchive(string sourceArchiveFileName, string destinationDirectoryName)
        {
            using (var archive = new ZipArchive(File.OpenRead(sourceArchiveFileName), ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    string targetPath = Path.Combine(destinationDirectoryName, entry.FullName);

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    if (!entry.FullName.EndsWith("/"))
                    {
                        using (var fileStream = new FileStream(targetPath, FileMode.Create))
                        {
                            entry.Open().CopyTo(fileStream);
                        }
                    }
                }
            }
        }



        static string SearchForFile()
        {
            string searchPath = Path.Combine(downloadPath, "Extracted");

            Console.WriteLine("\nSearching for 'LNVProcessManagement.inf' file...");

            string INFFilePath = FindFileRecursively(searchPath, "LNVProcessManagement.inf");
            if (INFFilePath != null)
            {
                return INFFilePath; // Return the value
            }
            else
            {
                Console.WriteLine("\nYou may selected a wrong file...");
                return null; // Return null if the file is not found
            }
        }

        static string FindFileRecursively(string directory, string fileName)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (Path.GetFileName(file).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }

                foreach (var subDirectory in Directory.GetDirectories(directory))
                {
                    string result = FindFileRecursively(subDirectory, fileName);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                // Handle any errors while accessing files/directories
            }

            return null;
        }
    }

    class Release
    {
        public string name { get; set; }
        public string tag_name { get; set; }
        public string body { get; set; }
        public List<Asset> assets { get; set; }
        public Author author { get; set; }
    }

    class Asset
    {
        public string name { get; set; }
        public int size { get; set; }
        public string browser_download_url { get; set; }
    }

    class Author
    {
        public string username { get; set; }
        public string email { get; set; }
    }
}
