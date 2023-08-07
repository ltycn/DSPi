using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using static DSPi.HttpGet;

namespace DSPi
{
    public class FileOperator
    {
        static readonly string downloadPath = @"C:\DSP_Files\";

        public static async Task DownloadAndExtractReleaseAsync(Release release)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    Console.WriteLine($"Downloading {release.name}...");

                    // Create the download directory if it doesn't exist
                    Directory.CreateDirectory(downloadPath);

                    string filePath = Path.Combine(downloadPath, release.assets[0].name);

                    using (var response = await httpClient.GetAsync(release.assets[0].browser_download_url))
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"Download complete. File saved to: {filePath}");

                    // Extract the downloaded release file
                    string extractPath = Path.Combine(downloadPath, "Extracted");
                    ExtractZipArchive(filePath, extractPath);
                    Console.WriteLine($"Release extracted to: {extractPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading or extracting file: {ex.Message}");
            }
        }

        public static void ExtractZipArchive(string sourceArchiveFileName, string destinationDirectoryName)
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

        public static string SearchForFile()
        {
            string searchPath = Path.Combine(downloadPath, "Extracted");

            string INFFilePath = FindFileRecursively(searchPath, "LNVProcessManagement.inf");
            if (INFFilePath != null)
            {
                return INFFilePath; // Return the value
            }
            else
            {
                Console.WriteLine("\nYou may have selected the wrong file...");
                return null; // Return null if the file is not found
            }
        }

        public static string FindFileRecursively(string directory, string fileName)
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
            catch (UnauthorizedAccessException)
            {
                // Handle any unauthorized access errors while accessing files/directories
            }
            catch (PathTooLongException)
            {
                // Handle path too long errors
            }
            catch (Exception)
            {
                // Handle other errors
            }

            return null;
        }
    }
}
