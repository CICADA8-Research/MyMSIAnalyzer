using System;
using WixToolset.Dtf.WindowsInstaller;
using WixToolset.Dtf.WindowsInstaller.Linq;
using System.Linq;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller.Linq.Entities;

namespace MyMSIAnalyzer
{

    // Analyze existing Custom Actions inside the MSI file
    internal class ActionAnalyzer
    {
        static public string[] keywords = new string[]
        {
            "AdminToolsFolder",
            "AppDataFolder",
            "DesktopFolder",
            "FavoritesFolder",
            "LocalAppDataFolder",
            "MyPicturesFolder",
            "NetHoodFolder",
            "PersonalFolder",
            "PrintHoodFolder",
            "ProgramMenuFolder",
            "RecentFolder",
            "SendToFolder",
            "StartMenuFolder",
            "StartupFolder",
            "TempFolder",
            "APPDATA",
            "HomePath",
            "LOCALAPPDATA",
            "TMP",
            "USERPROFILE",
            "conhost.exe",
            "cmd.exe",
            "powershell.exe",
            "-noprofile",
            "HKCU",
            "HKEY_CURRENT_USER",
            "C:\\Windows\\Tasks",
            "C:\\windows\\tracing",
            "C:\\Windows\\Temp",
            "C:\\Windows\\tracing",
            "C:\\Windows\\Registration\\CRMLog",
            "C:\\Windows\\System32\\FxsTmp",
            "C:\\Windows\\System32\\Tasks",
            "C:\\Windows\\System32\\AppLocker\\AppCache.dat",
            "C:\\Windows\\System32\\AppLocker\\AppCache.dat.LOG1",
            "C:\\Windows\\System32\\AppLocker\\AppCache.dat.LOG2",
            "C:\\Windows\\System32\\Com\\dmp",
            "C:\\Windows\\System32\\Microsoft\\Crypto\\RSA\\MachineKeys",
            "C:\\Windows\\System32\\spool\\PRINTERS",
            "C:\\Windows\\System32\\spool\\SERVERS",
            "C:\\Windows\\System32\\spool\\drivers\\color",
            "C:\\Windows\\System32\\Tasks\\OneDrive",
            ""
        };

        public static void AnalyzeCustomActions(string msiFilePath)
        {
            using (var database = new QDatabase(msiFilePath, DatabaseOpenMode.ReadOnly))
            {
                var installExecuteSequenceList = database.InstallExecuteSequences.ToList();

                if (!installExecuteSequenceList.Any())
                {
                    return;
                }

                var installExecuteSequenceOrder = installExecuteSequenceList.OrderBy(row => row.Sequence).ToList();

                var installExecuteIndex = installExecuteSequenceOrder.FindIndex(row => row.Action == "InstallValidate");

                if (installExecuteIndex != -1)
                {
                    installExecuteIndex = installExecuteSequenceOrder.FindIndex(row => row.Action == "InstallInitialize");
                }

                var installFinalizeIndex = installExecuteSequenceOrder.FindIndex(row => row.Action == "InstallFinalize");

                var customActionsList = new List<CustomAction_>();
                try
                {
                    customActionsList = database.CustomActions.ToList();
                } catch (Exception ex)
                {
                    return;
                }

                if (!customActionsList.Any())
                {
                    return;
                }

                foreach (var customAction in customActionsList)
                {
                    if ((customAction.Type & WixToolset.Dtf.WindowsInstaller.CustomActionTypes.NoImpersonate) != 0 &&
                        ((customAction.Type & WixToolset.Dtf.WindowsInstaller.CustomActionTypes.Commit) != 0 ||
                        (customAction.Type & WixToolset.Dtf.WindowsInstaller.CustomActionTypes.Rollback) != 0))
                    {

                        var actionInSequence = installExecuteSequenceList.FirstOrDefault(seq => seq.Action == customAction.Action);

                        if (actionInSequence != null)
                        {
                            var actionIndex = installExecuteSequenceOrder.IndexOf(actionInSequence);
                            if (actionIndex > installExecuteIndex && actionIndex < installFinalizeIndex)
                            {
                                Console.WriteLine($"\t[+] Interesting Custom Action found");
                                Console.WriteLine($"\tIndex: {actionIndex}");
                                Console.WriteLine($"\tAction: {customAction.Action}");
                                Console.WriteLine($"\tType: {customAction.Type}");
                                Console.WriteLine($"\tSource: {customAction.Source}");


                                var target = customAction.Target;
                                Console.Write("\tTarget: ");

                                bool containsSensitiveKeywords = false;
                                foreach (var keyword in keywords)
                                {
                                    if (target.ToLower().Contains(keyword.ToLower()))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.Write(keyword + " ");
                                        Console.ResetColor();
                                        containsSensitiveKeywords = true;
                                    }
                                }

                                if (!containsSensitiveKeywords)
                                {
                                    Console.Write(target);
                                }

                                Console.WriteLine();
                                Console.WriteLine();
                            }
                        }
                    }

                }
            }
        }
    }
}
