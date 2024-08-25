using System;
using System.Collections.Generic;
using System.Linq;

namespace MyMSIAnalyzer
{
    static class Options
    {
        public static string path = @"C:\Windows\Installer";
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (var entry in args.Select((value, index) => new { index, value }))
            {
                var argument = entry.value.ToLower();

                switch (argument)
                {
                    case "-path":
                        Options.path = args[entry.index + 1];
                        break;
                }
            }

            var msiFiles = new List<string>();
            Console.WriteLine("[+] Start searching MSI files on system");
            Finder.FindMsiFiles(Options.path, msiFiles);

            Console.WriteLine($"[+] Found {msiFiles.Count} files. Analyzing...");

            foreach (var msiFile in msiFiles)
            {
                Console.WriteLine($"[+] File {msiFile}");
                Console.WriteLine($"\t[?] Signature: {(Signature.VerifySignature(msiFile) ? "valid" : "invalid")}");
                ActionAnalyzer.AnalyzeCustomActions(msiFile);
                CredFinder.FindCredentials(msiFile);
                Console.WriteLine($"\t[?] Can write custom actions: {(Writer.TryToWriteCustomAction(msiFile) ? "TRUE" : "FALSE")}");
            }
        }
    }
}
