using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Admin_Pdv_Lauren.Tools
{
    public class AD_DOWNLOAD
    {
        public bool DOWNLOAD(string link, string nameFile)
        {
            bool result = false;
            try
            {
                string uri = link;
                
                if (!Directory.Exists(Program.repDest))
                {
                    Directory.CreateDirectory(Program.repDest);
                }
                if (Directory.Exists(Program.repDest))
                {
                    string dest = Program.repDest + nameFile;
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(new Uri(uri), dest);
                    //controle présence
                    Thread.Sleep(1000);
                    if (File.Exists(dest))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                result = false;
            }
            return result;
            
        }
    }
}
