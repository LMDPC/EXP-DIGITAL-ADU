using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Drawing;

namespace EXPEDIENTE
{
    class Program
    {
        static void Main(string[] args)
        {
            //El objetivo del proyecto es analizar multiples fuentes de informacion de el sistema de pedimentos, sistema de trafico y archivos complementarios para crear un expediente digital mas completo.
            #region Rutas,Variables,Directory
            //rutas
            string rutoriginal = ConfigurationManager.AppSettings["rutaoriginal"].ToString();
            string rutcomplementario = ConfigurationManager.AppSettings["rutacomplementario"].ToString();
            string rutaduanet = ConfigurationManager.AppSettings["rutaaduanet"].ToString();

            string r240 = rutaduanet + @"\240\";
            string r270 = rutaduanet + @"\270\";
            string r271 = rutaduanet + @"\271\";
            string r430 = rutaduanet + @"\430\";
            string r800 = rutaduanet + @"\800\";
            string numped = "";
            string file = "";
            string aduana = "";
            DateTime fecha = DateTime.Now;
            //directorios
            DirectoryInfo Rutoriginal = new DirectoryInfo(rutoriginal);
            DirectoryInfo Rutcomplementario = new DirectoryInfo(rutcomplementario);
            //Instancia
            digital objt = new digital();
            string Aduanas = ConfigurationManager.AppSettings["aduanas"].ToString();
            string[] AduanasArray;
            //Nueva declaracion
            
            //
            #endregion

            Console.WriteLine("START: " + fecha);
            try
            {
                #region Nuevo Proceso de FTP Aduanet
                AduanasArray = Aduanas.Split(@",".ToCharArray());
                for (int i = 0; i < AduanasArray.Length; i++)
                {
                    Console.WriteLine("SE INICIO PROCESO DEL FTP");
                    objt.aduanet(rutaduanet + @"\" + AduanasArray[i]);
                    Console.WriteLine("SE FINALIZO PROCESO DE ADUANA " + AduanasArray[i]);
                    objt.aduanetstatus(rutaduanet + @"\" + AduanasArray[i]);
                }
                #endregion

                #region Proceso de archivos complementarios
                Console.WriteLine("SE INICIA PROCESO DE ARCHIVOS COMPLEMENTARIOS");
                foreach (var files in Rutcomplementario.GetFiles("*"))
                {
                    file = Convert.ToString(files);

                    if (file.Length > 15)
                    {
                        if (file.Contains("am3.hoja.calculo"))
                        {
                            string rz = "";
                            numped = file.Substring(22, 7);
                            rz = file.Substring(30, 12);
                            if (rz == "BMH120709897")
                            {
                                objt.process("270", numped, file);
                            }
                            else
                            {
                                objt.process("240", numped, file);
                            }
                        }
                        else if (file.Substring(0, 3) == "MV_")
                        {
                            aduana = file.Substring(8, 3);
                            numped = file.Substring(12, 7);
                            objt.process(aduana, numped, file);
                        }
                        else
                        {
                            aduana = file.Substring(0, 3);
                            numped = file.Substring(9, 7);
                            objt.process(aduana, numped, file);
                        }
                    }
                    else
                    {
                        objt.Log(file,"ERR","FUE MAL ESCANEADO","GENERAL");       
                    }
                }
                Console.WriteLine("SE FINALIZA PROCESO DE ARCHIVOS COMPLEMENTARIOS");
                #endregion

                #region Proceso de archivos originales
                Console.WriteLine("SE INICIA PROCESO DE ARCHIVOS COMPLEMENTARIOS");
                foreach (var files in Rutoriginal.GetFiles("*"))
                {
                    string sfile = files.ToString();
                    if (sfile.Contains("-"))
                    {
                        numped = sfile.Substring(0, 9);
                        objt.process(aduana, numped, sfile);
                    }
                    else
                    {
                        aduana = sfile.Substring(11, 3);
                        numped = sfile.Substring(4, 7);
                        objt.process(aduana, numped, sfile);
                    }
                }
                Console.WriteLine("SE FINALIZA PROCESO DE ARCHIVOS COMPLEMENTARIOS");
                #endregion

                #region Proceso de movimientos especiales
                //EN PROCESO
                //objt.specialmov();
                #endregion

                //Console.WriteLine("PROCESO DE ELIMINACION");
                //objt.delete();
 
            }
            catch (Exception err)
            {      
                Console.WriteLine("EXCEPTION: " + err);
                objt.Log(err.ToString(), "ERR", "ERROR EN EL MAIN", "GENERAL");
            }
            Console.WriteLine("END: " + fecha);     
        }
    }
}
