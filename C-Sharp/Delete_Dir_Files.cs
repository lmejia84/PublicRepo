              static void DeleteAllFiles(string pcName, string userName)
              {
                     if (pcName == "JCFNATNL01")
                     {
                           DirectoryInfo di = new DirectoryInfo(@"D:\OneDrive\ADT LLC\Natl Acct Reporting - General\CT Suite\Email DT\");
                           foreach (FileInfo file in di.GetFiles())
                           {
                                  file.Delete();
                           }
                           foreach (DirectoryInfo dir in di.GetDirectories())
                           {
                                  dir.Delete(true);
                           }
                     }
                     else
                     {
                           DirectoryInfo di = new DirectoryInfo(@"C:\Users\" + userName + @"\ADT LLC\Natl Acct Reporting - General\CT Suite\Email DT\");
                           foreach (FileInfo file in di.GetFiles())
                           {
                                  file.Delete();
                           }
                           foreach (DirectoryInfo dir in di.GetDirectories())
                           {
                                  dir.Delete(true);
                           }
                     }
              }