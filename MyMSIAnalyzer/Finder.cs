using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMSIAnalyzer
{
    // Search for MSI files
    public class Finder
    {
        public static void FindMsiFiles(string directoryPath, List<string> msiFiles)
        {
            try
            {
                var files = Directory.GetFiles(directoryPath, "*.msi");
                msiFiles.AddRange(files);

                var directories = Directory.GetDirectories(directoryPath);

                foreach (var directory in directories)
                {
                    FindMsiFiles(directory, msiFiles);
                }
            }
            catch (UnauthorizedAccessException uex)
            {
                //Console.WriteLine($"[-] Access denied to {directoryPath}: {uex.Message}");
            }
            catch (DirectoryNotFoundException dnfex)
            {
                //Console.WriteLine($"[-] Directory not found: {dnfex.Message}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[-] Error while accessing {directoryPath}: {ex.Message}");
            }
        }
    }
}
