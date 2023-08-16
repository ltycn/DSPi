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
        static readonly string downloadPath = @"C:\DSPi\";
        public static async Task<string> DownloadAndExtractReleaseAsync(Release release)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    Console.WriteLine($"Downloading {release.name}...");

                    // Create the download directory if it doesn't exist
                    Directory.CreateDirectory(downloadPath);

                    string fileNameWithExtension = release.assets[0].name;
                    string filePath = Path.Combine(downloadPath, fileNameWithExtension);

                    using (var response = await httpClient.GetAsync(release.assets[0].browser_download_url))
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"Download complete. File saved to: {filePath}");

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);
                    // Extract the downloaded release file
                    string extractPath = Path.Combine(downloadPath, "Extracted");
                    await ExtractZipArchiveAsync(filePath, extractPath); // Await the async extraction
                    

                    
                    Console.WriteLine($"Release extracted to: {extractPath}\\{fileNameWithoutExtension}");
                    return fileNameWithoutExtension; // Return the downloaded file's name without extension
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading or extracting file: {ex.Message}");
                return null; // Return null in case of an error
            }
        }

        public static async Task ExtractZipArchiveAsync(string sourceArchiveFileName, string destinationDirectoryName)
        {
            // Get the name of the archive without extension
            string archiveName = Path.GetFileNameWithoutExtension(sourceArchiveFileName);

            // Create a folder with the archive's name inside the destination directory
            string archiveFolder = Path.Combine(destinationDirectoryName, archiveName);
            Directory.CreateDirectory(archiveFolder);

            using (var archive = new ZipArchive(File.OpenRead(sourceArchiveFileName), ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    string targetPath = Path.Combine(archiveFolder, entry.FullName);

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    if (!entry.FullName.EndsWith("/"))
                    {
                        using (var fileStream = new FileStream(targetPath, FileMode.Create))
                        {
                            await entry.Open().CopyToAsync(fileStream); // Await the async file copy
                        }
                    }
                }
            }
        }



        public static string SearchForFile(string FilePath)
        {
            string searchPath = Path.Combine(downloadPath, "Extracted\\", FilePath);
            Console.WriteLine($"Searching for inf in: {searchPath}");
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
