using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSPi.HttpGet;

namespace DSPi
{
    public  class ShowSelect
    {
        static string logo = @"
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

Dispatcher Installer v1.2                     github.com/ltycn/DSPi
***********************************************************************
";

        public static int ShowReleases(List<Release> releases)
        {
            int selectedReleaseIndex = 0;
            int itemsPerPage = 5;
            int currentPage = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine(logo);
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select to choose a version of Dispatcher:");
                Console.WriteLine($"Page {currentPage + 1}/{(int)Math.Ceiling((double)releases.Count / itemsPerPage)}  (Left/Right to navigate pages...)");
                Console.ResetColor();

                int startIndex = currentPage * itemsPerPage;
                int endIndex = Math.Min(startIndex + itemsPerPage, releases.Count);

                for (int i = startIndex; i < endIndex; i++)
                {
                    if (i == selectedReleaseIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    var release = releases[i];
                    var highlight = (i == selectedReleaseIndex) ? " >> " : "    ";
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
                Console.WriteLine($"Size: {((double)selectedRelease.assets[0].size / 1024):F2} KB");
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
                else if (keyInfo.Key == ConsoleKey.LeftArrow && currentPage > 0)
                {
                    currentPage--;
                    selectedReleaseIndex = currentPage * itemsPerPage;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow && startIndex + itemsPerPage < releases.Count)
                {
                    currentPage++;
                    selectedReleaseIndex = currentPage * itemsPerPage;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    return selectedReleaseIndex;
                }
            }
        }


        public static int ShowRepositories(List<Repository> repositories)
        {

            int selectedIndex = 0;

            do
            {
                Console.Clear();
                Console.WriteLine(logo);
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Select a repository:");
                Console.ResetColor();
                for (int i = 0; i < repositories.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    var highlight = (i == selectedIndex) ? " >> " : "    ";
                    Console.WriteLine($"{highlight}{i + 1}. {repositories[i].Name} ({repositories[i].description})");

                    Console.ResetColor();
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true); // Read a key without displaying it on the screen.
                if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex = Math.Min(repositories.Count - 1, selectedIndex + 1);
                }
                else if (keyInfo.Key == ConsoleKey.Enter) // Use the space bar for selection.
                {
                    return selectedIndex;
                }

            } while (true);
        }

    }
}
