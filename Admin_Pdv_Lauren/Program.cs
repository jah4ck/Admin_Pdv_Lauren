using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Admin_Pdv_Lauren.Tools;

namespace Admin_Pdv_Lauren
{
    class Program
    {
        public static WS_Pool1.WS_Rem_File_PDV ws = new WS_Pool1.WS_Rem_File_PDV();
        public static string codehex = Dns.GetHostName().Substring(2,4);
        public static string IdScenario = "";
        public static string Rem = "";
        public static string ProgramExec = "";
        public static string repDest = @"C:\TEMP\PRG_P3\";
        public static string repResult = @"C:\TEMP\LOG_P3\";
        public static int CodeErreur = 0;
        public static string FileResult = "";
        public static string typeRem = "";
        public static string FileSortieRem = "";
        public static string pathSortieRem = "";
        public static string FormatSortie = "";
        /* code erreur :
         * 1 = suppression en erreur des répertoire
         * 2 = erreur de téléchargement d'un fichier
         * 3 = erreur d'exécution du program
         * 4 = WS en erreur
         * 5 = Pas de traitement à faire
         * 6 = maj ws etape download
         * 7 = maj ws etape execution
         * 8 = maj ws etape terminée
         * 9 = maj ws code erreur
         * 10 = erreur remonté fichier vie ws
         * 11 = fichier de résultat attendu est introuvable
         * 12 = erreur sur la remontée du fichier de résultat
         * 13 = erreur d'importation sql
         * 14 = erreur de déplacement des fichier sur le serveur prod
         * 15 = création impossible du directory sur le serveur prod
         * 16 = maj WS etape rem BDD
         * 17 = maj WS etape rem File
         * 18 = Ecriture du fichier
         * 19 = La remontée est vide
         * */

        static void Main(string[] args)
        {
            #region coucou je suis là \\fonctionne
            try
            {
                int tentative=0;
                while (Program.ws.Coucou(codehex) != true)
                {
                    tentative++;
                    Console.WriteLine(tentative);
                    Thread.Sleep(1000);
                    if (tentative == 5)
                    {
                        CodeErreur = 4;
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                CodeErreur = 4;
            }
            Console.WriteLine("Code Erreur : " + CodeErreur);
            #endregion

            #region purge répertoire
            try
            {
                if (Directory.Exists(repDest))
                {
                    string[] filenames = Directory.GetFiles(repDest, "*", SearchOption.TopDirectoryOnly);
                    foreach (string fName in filenames)
                    {
                        File.Delete(fName);
                    }
                    Directory.Delete(repDest);
                }
                if (Directory.Exists(repResult))
                {
                    string[] filenames = Directory.GetFiles(repResult, "*", SearchOption.TopDirectoryOnly);
                    foreach (string fName in filenames)
                    {
                        File.Delete(fName);
                    }
                    Directory.Delete(repResult);
                }
            }
            catch (Exception err)
            {
                CodeErreur = 1;
                Console.WriteLine(err.Message);
            }
            
            #endregion
            #region creation des répertoire 
            if (CodeErreur==0)
            {
                if (!Directory.Exists(repDest))
                {
                    Directory.CreateDirectory(repDest);
                }
                if (!Directory.Exists(repResult))
                {
                    Directory.CreateDirectory(repResult);
                }
            }
            
            #endregion

            #region demande quoi faire et télécharge les fichiers qui vont bien
            if (CodeErreur == 0)
            {
                string fileDownload = ws.QueFaire(codehex);

                if (fileDownload.Length > 5)
                {
                    string[] SplitLigneDownload = fileDownload.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (string ligneDownload in SplitLigneDownload)
                    {
                        Console.WriteLine("Code Erreur : " + CodeErreur);
                        if (CodeErreur == 0)
                        {
                            Console.WriteLine(ligneDownload);
                            string[] columnInfoParam = ligneDownload.Split(';');
                            if (columnInfoParam.Length > 2)
                            {
                                IdScenario = columnInfoParam[0];
                                Rem = columnInfoParam[1];
                                FileResult = columnInfoParam[5];
                                typeRem = columnInfoParam[6];
                                FileSortieRem = columnInfoParam[7];
                                pathSortieRem = columnInfoParam[8];
                                FormatSortie = columnInfoParam[9];
                                if (columnInfoParam[4].ToString() == "1" || columnInfoParam[4].ToString().ToUpper()=="TRUE")
                                {
                                    ProgramExec = columnInfoParam[2];
                                }
                                //téléchargement program
                                AD_DOWNLOAD myDownload = new AD_DOWNLOAD();

                                //maj etape pdv
                                int tentativews = 0;
                                while (!ws.MajEtapeScenario(codehex, IdScenario, "DOWNLOAD"))
                                {
                                    tentativews++;
                                    if (tentativews == 5)
                                    {
                                        CodeErreur = 6;
                                        break;
                                    }
                                }
                                
                                int tentative = 0;
                                while (!myDownload.DOWNLOAD(columnInfoParam[3], columnInfoParam[2]))
                                {
                                    tentative++;
                                    if (tentative == 5)
                                    {
                                        CodeErreur = 2;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    CodeErreur = 5;
                }
            }
            
            #endregion
            #region exécution du program
            if (CodeErreur==0)
            {
                Console.WriteLine("Exécution "+ProgramExec);
                try
                {
                    AD_EXEC_PROGRAM MyExecProgram = new AD_EXEC_PROGRAM();
                    int tentativews = 0;
                    while (!ws.MajEtapeScenario(codehex, IdScenario, "EXECUTION"))
                    {
                        tentativews++;
                        if (tentativews == 5)
                        {
                            CodeErreur = 7;
                            break;
                        }
                    }
                    MyExecProgram.ExecProgram(ProgramExec, Rem);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    CodeErreur = 3;
                }
                
            }
            #endregion
            #region remonté des données BDD
            if (CodeErreur==0 && Rem=="1" && typeRem=="1")
            {
                if (FileResult != "N/A")
                {
                    int tentativews = 0;
                    while (!ws.MajEtapeScenario(codehex, IdScenario, "REMBDD"))
                    {
                        tentativews++;
                        if (tentativews == 5)
                        {
                            CodeErreur = 16;
                            break;
                        }
                    }
                    Console.WriteLine("Je remonte les données");
                    try
                    {
                        if (File.Exists(repResult + FileResult))
                        {
                            string bloc = File.ReadAllText(repResult + FileResult).TrimEnd();
                            int tentative = 0;
                            int resultWS = 5;
                            while (resultWS!=0)
                            {
                                Console.WriteLine(resultWS);
                                resultWS = ws.ImportBDD(codehex, IdScenario, bloc);
                                if (resultWS==2)
                                {
                                    CodeErreur = 13;
                                    break;
                                }
                                if (resultWS==3)
                                {
                                    CodeErreur = 14;
                                    break;
                                }
                                if (resultWS == 4)
                                {
                                    CodeErreur = 15;
                                    break;
                                }
                                tentative++;
                                if (tentative==5)
                                {
                                    CodeErreur = 10;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Le fichier attendu n'est pas présent : " + repResult + FileResult);
                            CodeErreur = 11;
                        }
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                        CodeErreur = 12;
                    }
                }
                else
                {
                    Console.WriteLine("Pas de nom de fichier à remonter");
                }
            }
            #endregion

            #region remontée des données fichier plat
            if (CodeErreur==0 && Rem=="1" && typeRem=="2")
            {
                if (FileResult!="N/A" && FileSortieRem!="N/A" && pathSortieRem!="N/A" && FormatSortie!="N/A")
                {
                    int tentativews = 0;
                    while (!ws.MajEtapeScenario(codehex, IdScenario, "REMFILE"))
                    {
                        tentativews++;
                        if (tentativews == 5)
                        {
                            CodeErreur = 17;
                            break;
                        }
                    }
                    Console.WriteLine("Je remonte les données");
                    try
                    {
                        if (File.Exists(repResult + FileResult))
                        {
                            string bloc = File.ReadAllText(repResult + FileResult).TrimEnd();
                            int tentative = 0;
                            while (Program.ws.RemMdt(FileSortieRem, bloc, pathSortieRem, FormatSortie, codehex, "") != true)
                            {
                                tentative++;
                                Thread.Sleep(1000);
                                if (tentative == 5)
                                {
                                    CodeErreur = 18;
                                    break;
                                }
                            }
                            //ajouter une étape "TERMINE VIDE" voulant signifier que tout s'est bien déroulé mais que le résultat est vide
                            // faire de même pour la remontée en BDD
                            if (bloc.Length<4)
                            {
                                CodeErreur = 19;//la remontée est vide
                            }
                        }
                        else
                        {
                            Console.WriteLine("Le fichier attendu n'est pas présent : " + repResult + FileResult);
                            CodeErreur = 11;
                        }
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                        CodeErreur = 12;
                    }

                }
            }
            #endregion
            #region traitement de fin (purge)
            try
            {
                if (Directory.Exists(repDest))
                {
                    string[] filenames = Directory.GetFiles(repDest, "*", SearchOption.TopDirectoryOnly);
                    foreach (string fName in filenames)
                    {
                        File.Delete(fName);
                    }
                    Directory.Delete(repDest);
                }
                if (Directory.Exists(repResult))
                {
                    string[] filenames = Directory.GetFiles(repResult, "*", SearchOption.TopDirectoryOnly);
                    foreach (string fName in filenames)
                    {
                        File.Delete(fName);
                    }
                    Directory.Delete(repResult);
                }
            }
            catch (Exception err)
            {
                CodeErreur = 1;
                Console.WriteLine(err.Message);
            }
            #endregion

            #region remonté des status
            try
            {
                if (CodeErreur==0)
                {
                    int tentativews = 0;
                    while (!ws.MajEtapeScenario(codehex, IdScenario, "TERMINE"))
                    {
                        tentativews++;
                        if (tentativews == 5)
                        {
                            CodeErreur = 8;
                            break;
                        }
                    }
                }
                if (CodeErreur==19)
                {
                     int tentativews = 0;
                    while (!ws.MajEtapeScenario(codehex, IdScenario, "TERMINE VIDE"))
                    {
                        tentativews++;
                        CodeErreur = 0;
                        if (tentativews == 5)
                        {
                            CodeErreur = 8;
                            break;
                        }
                    }
                }
                Thread.Sleep(1000);
                int intscenar;
                bool chkScenar = Int32.TryParse(IdScenario, out intscenar);
                if (chkScenar)
                {
                    int tentativeCodeErreur = 0;
                    while (!ws.MajCodeErreurRecepteur(codehex, IdScenario, CodeErreur.ToString()))
                    {
                        tentativeCodeErreur++;
                        if (tentativeCodeErreur == 5)
                        {
                            CodeErreur = 9;
                            Console.WriteLine("MAJ WS Code Erreur impossible");
                            break;
                        }
                    }
                }
                //Thread.Sleep(1000);

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            #endregion
        }
    }
}
