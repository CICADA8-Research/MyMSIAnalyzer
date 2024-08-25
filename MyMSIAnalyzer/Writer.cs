using System;
using WixToolset.Dtf.WindowsInstaller;
using WixToolset.Dtf.WindowsInstaller.Linq;
using System.Linq;
using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller.Linq.Entities;

namespace MyMSIAnalyzer
{
    // Checking whether you can record your own Custom Action into MSI file
    internal class Writer
    {
        public static string customActionName = "GooglePing";
        public static bool TryToWriteCustomAction(string msiPath)
        {
            try
            {
                using (var database = new QDatabase(msiPath, DatabaseOpenMode.Direct))
                {
                    var insertQuery = $"INSERT INTO CustomAction (Action, Type, Source, Target) VALUES ('{customActionName}', 34, 'cmd.exe', '/c ping google.com')";
                    database.Execute(insertQuery);

                    var deleteQuery = $"DELETE FROM CustomAction WHERE Action='{customActionName}'";
                    database.Execute(deleteQuery);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
