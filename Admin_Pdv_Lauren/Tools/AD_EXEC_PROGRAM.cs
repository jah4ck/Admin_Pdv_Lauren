using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Admin_Pdv_Lauren.Tools
{
    public class AD_EXEC_PROGRAM
    {
        
        public void ExecProgram(string Prog,string arg)
        {
            try
            {
                if (File.Exists(Program.repDest + Prog) )
                {
                    if (arg.ToUpper()=="TRUE")
                    {
                        arg = "1";
                    }
                    Console.WriteLine("Exécution de " + Prog+" "+arg);
                    Process Exec = new Process();
                    Exec.StartInfo.FileName = Program.repDest + Prog;
                    Exec.StartInfo.Arguments = arg;
                    Exec.StartInfo.UseShellExecute = false;
                    Exec.StartInfo.RedirectStandardOutput = true;
                    Exec.Start();
                    Console.WriteLine("En cours d'exec");
                    bool timeout = Exec.WaitForExit(30000);
                    Console.WriteLine(timeout);
                    if (!timeout)
                    {
                        Console.WriteLine("Le temps d'exécution a dépassé les 10s");
                        Program.CodeErreur = 3;
                        Console.WriteLine("Kill du process");
                        Exec.Kill();
                    }
                }
            }
            catch (Exception err)
            {
                Program.CodeErreur = 3;
                Console.WriteLine(err.Message);
            }

        }
        
    }
}
