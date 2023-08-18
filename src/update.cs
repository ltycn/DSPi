using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPi
{
    public class update
    {
        private string cmdFilePath;

        public void RunUpdate(string filePath)
        {
            cmdFilePath = filePath;
        }

        public void CreateAndExecuteCmdFile()
        {
            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(cmdFilePath);
            Directory.CreateDirectory(directoryPath);

            CreateCmdFile();
        }

        private void CreateCmdFile()
        {
            using (StreamWriter writer = new StreamWriter(cmdFilePath))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine("setlocal");
                writer.WriteLine("set ProcessName=DSPi.exe");
                writer.WriteLine("set DownloadURL=http://dl.lnvpe.com/logtool/DSPi.exe");
                writer.WriteLine("set DesktopPath=%USERPROFILE%\\Desktop");
                writer.WriteLine("set DownloadedFilePath=%DesktopPath%\\DSPi.exe");
                writer.WriteLine("echo This script will perform the following tasks:");
                writer.WriteLine("echo 1. Download the file from \"%DownloadURL%\"");
                writer.WriteLine("echo 2. Replace the existing file in DESKTOP");
                writer.WriteLine("echo 3. Run the downloaded file");
                writer.WriteLine("echo.");
                writer.WriteLine("pause");
                writer.WriteLine("REM Step 1: Download the file");
                writer.WriteLine("echo Downloading file from \"%DownloadURL%\"...");
                writer.WriteLine("bitsadmin.exe /transfer \"Download DSPi\" %DownloadURL% %DownloadedFilePath%");
                writer.WriteLine("timeout 5");
                writer.WriteLine("REM Step 2: Run the downloaded file");
                writer.WriteLine("echo Running the downloaded file...");
                writer.WriteLine("start \"\" %DownloadedFilePath%");
                writer.WriteLine("echo Update Success!");
                writer.WriteLine("endlocal");
            }
        }
    }
}
