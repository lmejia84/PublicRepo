using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Outlook_Save
{
    class Program
    {
        static void Main(string[] args)
        {
            string userName = Environment.UserName;
            string pcName = Environment.MachineName;
            DeleteAllFiles(pcName, userName);
            OutlookSave(pcName, userName);
            //Console.ReadLine();
        }

        static void OutlookSave(string pcName, string userName)
        {
            Outlook.Application ol = new Outlook.Application();
            Outlook.NameSpace ns = ol.GetNamespace("MAPI");
            Outlook.MailItem mi = null;
            Outlook.MAPIFolder defaultFolder = ns.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.MAPIFolder fol = defaultFolder.Parent.Folders("Name of your outlook folder here");

            int x = 1;
            int y = fol.Items.Count;

            while (!(x > y))
            {
                foreach (object collectionItem in fol.Items)
                {
                    mi = collectionItem as Outlook.MailItem;
                    if (mi.Class.ToString() == "olMail")
                    {
                        foreach (Outlook.Attachment item in mi.Attachments)
                        {
                            if (item.FileName == "name of the attachment I want to download")
                            {
                                item.SaveAsFile(@"C:\Users\" + item.FileName.ToString());
                            }
                        }
                        mi.UnRead = false;
                        mi.Delete();
                    }
                }
                x++;
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ol);
        }
    }
}
