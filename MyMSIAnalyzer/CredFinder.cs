using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixToolset.Dtf.WindowsInstaller;

namespace MyMSIAnalyzer
{
    // Search for abandoned credentials inside an MSI file
    internal class CredFinder
    {
        static public string[] keywords = new string[] 
        { 
            "USERNAME", 
            "PASSWORD", 
            "USER", 
            "PASS",
            "PFX",
            "CERTIFICATE",
            "PRIVATE",
            "KEY",
            "API",
            "PATH",
            "FOLDER"
        };

        static public string[] blacklistedKeywords = new string[]
        {
            "ALLUSERS",
            "nousername"
        };

        public static void FindCredentials(string msiPath)
        {
            try
            {
                using (Database db = new Database(msiPath, DatabaseOpenMode.ReadOnly))
                {
                    var sql = "SELECT `Property`, `Value` FROM `Property`";
                    using (var view = db.OpenView(sql))
                    {
                        view.Execute();

                        foreach (var record in view)
                        {
                            var property = record.GetString("Property").ToLower();
                            var value = record.GetString("Value");
                            var containsBlackListedKeyword = false;


                            foreach (var blKw in blacklistedKeywords)
                            {
                                if (property.Contains(blKw.ToLower()))
                                {
                                    containsBlackListedKeyword = true;
                                    break;
                                }
                            }

                            if (containsBlackListedKeyword)
                                break;

                            foreach (var keyword in keywords)
                            {
                                if (property.Contains(keyword.ToLower()))
                                {
                                    Console.WriteLine($"\t[?] Interesting property: {property}, Value: {value}");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return;
        }
    }
}
