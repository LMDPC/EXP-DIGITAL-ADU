using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Configuration;
using System.IO.Compression;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Data.SqlTypes;
using System.Security.Policy;
using System.Dynamic;

namespace EXPEDIENTE
{
    class digital
    {
        //Enjoy the silence
        

        #region RUTAS Y DIRECTORIOS DE ARCHIVO CONFIG
        //rutas
        string rutoriginal         = ConfigurationManager.AppSettings["rutaoriginal"].ToString();
        string rutcomplementario   = ConfigurationManager.AppSettings["rutacomplementario"].ToString();
        string rutdestino          = ConfigurationManager.AppSettings["rutadestino"].ToString();
        string rutdescompresion    = ConfigurationManager.AppSettings["rutadescompresion"].ToString();
        string rutdesdescompresion = ConfigurationManager.AppSettings["rutadesdescompresion"].ToString();
        string rutprocesados       = ConfigurationManager.AppSettings["rutaprocesados"].ToString();
        string adufechinicio        = ConfigurationManager.AppSettings["adufechinicio"].ToString();
        string adufechfin          = ConfigurationManager.AppSettings["adufechfin"].ToString();
        string rutsacmah6expo      = ConfigurationManager.AppSettings["rutsacmah6expo"].ToString();
        string rutsacmah6impo      = ConfigurationManager.AppSettings["rutsacmah6impo"].ToString();
        string rutsacmah3expo      = ConfigurationManager.AppSettings["rutsacmah3expo"].ToString();
        string rutsacmah3impo      = ConfigurationManager.AppSettings["rutsacmah3impo"].ToString();
        string rutimar             = ConfigurationManager.AppSettings["rutimar"].ToString();
        string rutaduanet = ConfigurationManager.AppSettings["rutaaduanet"].ToString();
        string TipAduana = "";
        #endregion

        #region VARIABLES VIEJAS
        SqlDataReader reader { get; set; }
        SqlConnection con { get; set; }
        string carpetaped { get; set; }
        string RutaDestino { get; set; }
        //variables de informacion del Query
        string ped { get; set; }
        string pat { get; set; }
        int tipope { get; set; }
        string ope { get; set; }
        string adu { get; set; }
        string razonsocial { get; set; }

        string globalfile { get; set; }
        DateTime fech { get; set; }
        int month { get; set; }
        string mes { get; set; }
        string Clavecliente { get; set; }
        SqlString nomcliente { get; set; }
        SqlString clavecliente { get; set; }
        //variables para Log de errores
        string statusok = "OK";
        string statuserr = "ERR";
        string mensaje = "";
        //Aduanet
        string rutaduanetdirectory { get; set; }
        int Aduanetfecha { get; set; }
        int year { get; set; }
        string sfile { get; set; }
        string spat { get; set; }
        string sped { get; set; }
        string sadu { get; set; }

        #endregion

        #region NUEVAS VARIABLES

        //Se esta buscando depurar aquellas variables repetitivas y solo usar las necesarias en dicho proceso.

        //Metodo archivos originales

        //Metodo archivos complementarios

        //Metodo IMAR

        //Metodo Aduanet
        int Adufechfin { get; set; }
        int Adufechinicio { get; set; }
        
        #endregion

        public void process(string tipaduana, string numped, string nomarchv)
        {
            try
            {
                TipAduana = tipaduana;
                conDB(numped);
                carpetaped = adu + @"-" + pat + @"-" + ped;
                RutaDestino = rutdestino + @"\" + Clavecliente + @"\" + year + @"\" + ope + @"\" + mes + @"\" + carpetaped;
                Console.WriteLine("PEDIMENTO: "+ numped);
                if (Clavecliente == null)
                {
                    
                    Console.WriteLine("NO EXISTE INFORMACION EN LA REPLICA DE TCI");
                    Log(numped, "ERR", "NO EXISTE EN LA REPLICA", "FALTANTES");
                }
                else
                {
                    if (!Directory.Exists(RutaDestino))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(RutaDestino);//se crea carpeta
                        movement(nomarchv);
                    }
                    else
                    {
                        movement(nomarchv);
                    }
                }

            }
            catch (Exception err)
            {
                Console.WriteLine("EXCEPTION: " + err);
            }
        }
        public void movement(string nomarchv)
        {

            if (nomarchv.Contains(".zip"))//validacion de zio
            {
                //RutaDestinoMetodo(nomarchv);
                File.Copy(rutoriginal + @"\" + nomarchv, rutprocesados + @"\ExpDgtOrg\" + nomarchv,true);
                zipmov(rutoriginal + @"\" + nomarchv,RutaDestino);              
            }
            else if (nomarchv.Contains(".rar"))
            {
                File.Copy(rutoriginal + @"\" + nomarchv, rutprocesados + @"\ExpDgtOrg\" + nomarchv,true);
                rarmov(rutoriginal);      
            }
            else
            {
                filemov(nomarchv);
            }
        }
        public void specialmov()
        {
            //EN PROCESO
        }
        public void aduanet(string r)
        {
            Adufechinicio = Convert.ToInt32(adufechinicio);
            Adufechfin = Convert.ToInt32(adufechfin);
            try
            {
                for (int i = Adufechinicio; i <= Adufechfin; i++)//AÑOS
                {
                    for (int j = 1; j <= 12; j++)//MESES
                    {
                        if (j <= 9)//CONTADOR ARRIBA DE 1O MESES
                        {
                            string R = r + @"\" + i.ToString() + @"0" + j.ToString() + @"\";
                            aduanetmov(R);
                        }
                        else
                        {
                            string Y = r + @"\" + i.ToString() + j.ToString() + @"\";
                            aduanetmov(Y);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Log(r,"ERR","ERROR EN METODO aduanet","GENERAL");
            }
        }
        public void aduanetmov(string ruta)
        {
            DirectoryInfo Rutaduanet = new DirectoryInfo(ruta);
            try
            {
                if (Directory.Exists(ruta))
                {
                    foreach (var file in Rutaduanet.GetDirectories("*"))
                    {
                        #region proceso 
                        //subtraer info del archivo
                        string sfile = file.ToString();
                        string spat = sfile.Substring(5, 4);
                        string sped = sfile.Substring(9, 7);
                        string sadu = sfile.Substring(2, 3);
                        TipAduana = sadu; //variable que se utiliza en consulta del query
                                          //ped = sped;
                        rutaduanetdirectory = Rutaduanet + sfile;
                        #endregion
                        conDB(sped);
                        string RutDestino = rutdestino + @"\" + Clavecliente + @"\" + year + @"\" + ope + @"\" + mes + @"\" + sadu + "-" + spat + "-" + sped;
                        Console.WriteLine("EXISTE RUTA " + ruta);
                        Console.WriteLine("PEDIMENTO: " + sped);

                        if (Clavecliente == null)
                        {
                            Console.WriteLine("NO EXISTE INFORMACION EN LA REPLICA DE TCI");
                            Log(sped,"ERR","NO EXISTE EN LA REPLICA","FALTANTES");
                        }
                        else
                        {
                            if (!Directory.Exists(RutDestino))
                            {
                                DirectoryInfo di = Directory.CreateDirectory(RutDestino);
                                filemovaduanet(rutaduanetdirectory, RutDestino);
                            }
                            else
                            {
                                filemovaduanet(rutaduanetdirectory, RutDestino);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("NO EXISTE RUTA " + ruta);
                    Log(ruta,"ERR", "NO EXISTE LA RUTA","GENERAL");
                }
            }
            catch(Exception e)
            {
                Log(e.ToString(),"ERR","ERROR EN METODO ADUANETMOV","GENERAL");
            }
        }
        public void filemovaduanet(string rutaorigen,string rutadestino)
        {
            DirectoryInfo Rutaorigen = new DirectoryInfo(rutaorigen);
            try
            {
                foreach (var file in Rutaorigen.GetFiles("*"))
                {
                    string sfile = file.ToString();
                    Console.WriteLine("ARCHIVO:" + sfile);

                    if (Clavecliente == "MAHLE COMPONENTES")
                    {
                        if (ope == "IMPO")
                        {
                            //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO

                            File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                            Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                            Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");

                            File.Copy(rutaorigen + @"\" + sfile, rutsacmah6impo + @"\" + sfile, true);
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("SAC: " + rutsacmah6impo + @"\" + sfile);
                            Console.ForegroundColor = ConsoleColor.White;
                            Log(sfile, "OK", rutsacmah6impo + @"\" + sfile, "SAC");

                            archvprocesados(rutaorigen, sfile);

                        }
                        else if (ope == "EXPO")
                        {
                            //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO

                            File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                            Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                            Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");

                            File.Copy(rutaorigen + @"\" + sfile, rutsacmah6expo + @"\" + sfile, true);
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("SAC: " + rutsacmah6expo + @"\" + sfile);
                            Log(sfile, "OK", rutsacmah6impo + @"\" + sfile, "SAC");
                            Console.ForegroundColor = ConsoleColor.White;
                            archvprocesados(rutaorigen, sfile);
                        }
                    }
                    else if (Clavecliente == "MAHLE DE MEXICO")
                    {
                        if (ope == "IMPO")
                        {
                            //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO

                            File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                            Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                            Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");

                            File.Copy(rutaorigen + @"\" + sfile, rutsacmah3impo + @"\" + sfile, true);
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("SAC: " + rutsacmah3impo + @"\" + sfile);
                            Log(sfile, "OK", rutsacmah3impo + @"\" + sfile, "SAC");
                            Console.ForegroundColor = ConsoleColor.White;
                            archvprocesados(rutaorigen, sfile);
                        }
                        else if (ope == "EXPO")
                        {
                            //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO

                            File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                            Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                            Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");

                            File.Copy(rutaorigen + @"\" + sfile, rutsacmah3expo + @"\" + sfile, true);
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("SAC: " + rutsacmah3expo + @"\" + sfile);
                            Log(sfile, "OK", rutsacmah3expo + @"\" + sfile, "SAC");
                            Console.ForegroundColor = ConsoleColor.White;
                            archvprocesados(rutaorigen, sfile);
                        }
                    }
                    else if (Clavecliente == "BREMBO" || Clavecliente == "CONSORCIO" || Clavecliente == "REDNUM" || Clavecliente == "TRIARA" || Clavecliente == "TELMEX" || Clavecliente == "UNINET")
                    {
                        if (sfile.Contains("a3649") || sfile.Contains("e3649") || sfile.Contains("m3649"))
                        {

                            File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                            Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                            Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");

                            File.Copy(rutaorigen + @"\" + file, rutimar + @"\" + file, true);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("IMAR: " + rutimar + @"\" + file);
                            Log(sfile, "OK", rutimar + @"\" + file, "IMAR");
                            Console.ForegroundColor = ConsoleColor.White;
                            archvprocesados(rutaorigen, sfile);
                        }
                        else
                        {

                            File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                            Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                            Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");

                            archvprocesados(rutaorigen, sfile);
                        }
                    }
                    else
                    {
                        

                        File.Copy(rutaorigen + @"\" + sfile, rutadestino + @"\" + sfile, true);
                        Console.WriteLine("EXPEDIENTE: " + rutadestino + @"\" + sfile);
                        Log(sfile, "OK", rutadestino + @"\" + sfile, "GENERAL");
                        archvprocesados(rutaorigen, sfile);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e.ToString(),"ERR","ERRROR EN METODO FILEMOVADUANET","GENERAL");
            }
        }
        public void aduanetstatus(string rutaorigen)
        {    
            for (int i = Adufechinicio; i >= Adufechfin; i--)//AÑOS
            {
                for (int j = 1; j <= 12; j++)//MESES
                {
                    if (j <= 9)//CONTADOR ARRIBA DE 1O MESES
                    {
                        string r = rutaorigen +@"\"+ i.ToString() + @"0" + j.ToString();
                        DirectoryInfo R = new DirectoryInfo(r);
                        if (Directory.Exists(r))
                        {
                            foreach (var directory in R.GetDirectories("*"))
                            {
                                string direct = R + @"\"+directory.ToString();
                                if(Directory.GetFiles(direct).Length == 0)
                                {
                                    Directory.Delete(direct);
                                    Log(directory.ToString(),"ERR",direct,"ELIMINADOS");
                                }
                                else
                                {
                                    Console.WriteLine("AUN TIENE ARCHIVOS");                              
                                }
                            }

                            if(Directory.GetDirectories(r).Length != 0)
                            {
                                Console.WriteLine("AUN TIENE CARPETAS CON ARCHIVOS");
                            }
                            else
                            {
                                Directory.Delete(r);
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine("NO EXISTE LA CARPETA" + r + " EN EL FTP");                      
                        }
                    }
                    else
                    {
                        string r = rutaorigen + @"\" + i.ToString() + j.ToString();
                        DirectoryInfo R = new DirectoryInfo(r);
                        if (Directory.Exists(r))
                        {
                            foreach (var directory in R.GetDirectories("*"))
                            {
                                string direct = R + @"\" + directory.ToString();
                                if (Directory.GetFiles(direct).Length == 0)
                                {
                                    Directory.Delete(direct);
                                    Log(directory.ToString(), "ERR", direct, "ELIMINADOS");
                                }
                                else
                                {
                                    Console.WriteLine("AUN TIENE ARCHIVOS");
                                }
                            }

                            if (Directory.GetDirectories(r).Length != 0)
                            {
                                Console.WriteLine("AUN TIENE CARPETAS CON ARCHIVOS");
                            }
                            else
                            {
                                Directory.Delete(r);
                            }

                        }
                        else
                        {
                            Console.WriteLine("NO EXISTE LA CARPETA" + r + " EN EL FTP");
                        }
                    }
                }
            }
        }
        public void conDB(string dato)
        {   //modificar metodo para recibir query
            try
            {
                #region CONEXION SQL
                string constring = @"Data Source=10.10.10.23\S22SQLEXPRESS;Database=saet_prod;User id = sa;password=Passw0rd;Max Pool Size =200;";
                SqlConnection con = new SqlConnection(constring);
                con.Open();
                SqlCommand cmdped = new SqlCommand("SELECT TOP 1 PedPedimento,PedPatente, PedTipoOperacion,PedAduana,PedFechaPago,PedNombreCliente,CliClaveTCI FROM Reportes.pedimento right join Catalogo.Clientes ON Pedimento.PedNombreCliente = Clientes.CliRazonSocial WHERE PedAduana =" + TipAduana + "AND PedPedimento = '" + dato + "'", con);
                reader = cmdped.ExecuteReader();
                #endregion
                if (reader.HasRows)
                {
                    while (reader.Read())//se lee query
                    {
                        #region INFORMACION OBTENIDA DEL QUERY
                        //Se obtiene pedimento
                        ped = reader.GetString(0);
                        //Se obtiene patente
                        pat = reader.GetString(1);
                        #region tipoperacion
                        tipope = reader.GetInt32(2);
                        ope = "";
                        switch (tipope)
                        {
                            case 1:
                                ope = "IMPO";
                                break;
                            case 2:
                                ope = "EXPO";
                                break;
                        }
                        #endregion
                        //ADUANA
                        adu = reader.GetString(3);
                        //Se obtiene el año y mes
                        #region mes_año
                        fech = reader.GetDateTime(4);
                        month = fech.Month;
                        mes = "";
                        switch (month)
                        {
                            case 1:
                                mes = "01-ENERO";
                                break;
                            case 2:
                                mes = "02-FEBRERO";
                                break;
                            case 3:
                                mes =
                                "03-MARZO";
                                break;
                            case 4:
                                mes =
                                "04-ABRIL";
                                break;
                            case 5:
                                mes =
                                "05-MAYO";
                                break;
                            case 6:
                                mes =
                                "06-JUNIO";
                                break;
                            case 7:
                                mes =
                                "07-JULIO";
                                break;
                            case 8:
                                mes =
                                "08-AGOSTO";
                                break;
                            case 9:
                                mes =
                                "09-SEPTIEMBRE";
                                break;
                            case 10:
                                mes =
                                "10-OCTUBRE";
                                break;
                            case 11:
                                mes =
                                "11-NOVIEMBRE";
                                break;
                            case 12:
                                mes =
                                "12-DICIEMBRE";
                                break;
                        };
                        year = fech.Year;
                        #endregion
                        //Se obtiene nombre de cliente
                        nomcliente = reader.GetSqlString(5);
                        //Se obtiene clave del cliente
                        Clavecliente = reader.GetSqlString(6).ToString();
                        #endregion

                    }
                }
                con.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine("EXCEPTION: " + err);
            }
        }
        public void filerename(string file)
        {

            if (file.Contains("_P1"))// PEDIMENTO TRADICIONAL
            {

                globalfile = file;
            }
            else if (file.Contains("_PS"))//PEDIMENTO SIMPLIFICADO
            {
                globalfile = file;
            }
            else if (file.Contains("MV_"))//MANIFESTACIONES
            {
                globalfile = file;
            }
            else if (file.Contains("am3.hoja.calculo"))//HOJA CALCULO
            {
                globalfile = file;
            }
            else if (file.Contains("EDOCXML"))//EDOCXML
            {
                globalfile = file;
            }
            else if (file.Contains("a3649"))
            {
                globalfile = file;

            }
            else if (file.Contains("e3649"))
            {
                globalfile = file;
            }
            else if (file.Contains("k3649"))
            {
                globalfile = file;
            }
            else if (file.Contains("m3649"))
            {
                globalfile = file;
            }
            else if(file.Contains("NOM") || file.Contains("nom") || file.Contains("Nom"))
            {

            }
            else
            {
                if(file.Length == 20)//simplificado
                {
                    globalfile = file;
                }
                else
                {
                    globalfile = file;
                }
            }

        }
        public void filemov(string file)
        {
            Console.WriteLine("ARCHIVO: " + file);
            #region EXPEDIENTE
            File.Copy(rutcomplementario + @"\" + file, RutaDestino + @"\" + file, true);
            Console.WriteLine("EXPEDIENTE: " + RutaDestino + @"\" + file);
            Log(ped + @"-" + file, "OK", RutaDestino + @"\" + file,"GENERAL");
            #endregion
            #region PROCESO SAC
            Console.ForegroundColor = ConsoleColor.Blue;
            if (Clavecliente == "MAHLE COMPONENTES")
            {
                if (ope == "IMPO")
                {
                    //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO
                    File.Copy(rutcomplementario + @"\" + file, rutsacmah6impo + @"\" + file, true);
                    Console.WriteLine("SAC: " + rutsacmah6impo + @"\" + file);
                    Log(ped+@"-"+file, "OK", rutsacmah6impo + @"\" + file, "SAC");             
                }
                else if (ope == "EXPO")
                {
                    //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO
                    File.Copy(rutcomplementario + @"\" + file, rutsacmah6expo + @"\" + file, true);
                    Console.WriteLine("SAC: " + rutsacmah6expo + @"\" + file);
                    Log(ped + @"-" + file, "OK", rutsacmah6expo + @"\" + file, "SAC");
                }
            }
            else if (Clavecliente == "MAHLE DE MEXICO")
            {
                if (ope == "IMPO")
                {
                    //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO
                    File.Copy(rutcomplementario + @"\" + file, rutsacmah3impo + @"\" +file, true);
                    Console.WriteLine("SAC: " + rutsacmah3impo + @"\" + file);
                    Log(ped + @"-" + file, "OK", rutsacmah3impo + @"\" + file, "SAC");
                }
                else if (ope == "EXPO")
                {
                    //filerename(sfile); //METODO PARA RENOMBRAR SI ES NECESARIO
                    File.Copy(rutcomplementario + @"\" + file, rutsacmah3expo + @"\" + file, true);
                    Console.WriteLine("SAC: " + rutsacmah3expo + @"\" + file);
                    Log(ped + @"-" + file, "OK", rutsacmah3expo + @"\" + file, "SAC");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            #endregion
            #region PROCESO PROCESADOS
            String rutprocesado = Path.GetFullPath(rutcomplementario + @"\" + file);
            string[] rutprocesadosarray = rutprocesado.Split(@"\".ToCharArray());
            File.Copy(rutcomplementario+@"\"+file,rutprocesados+@"\"+rutprocesadosarray[6]+@"\"+file,true);
            Console.WriteLine("PROCESADOS"+ rutprocesados + @"\" + rutprocesadosarray[6] + @"\" + file);
            Log(ped + @"-" + file, "OK", rutprocesados + @"\" + rutprocesadosarray[6] + @"\" + file, "PROCESADOS");
            #endregion
            #region PROCESO ELIMINACION
            File.Delete(rutcomplementario + @"\" + file);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("SE HA ELIMINADO EL ARCHIVO: " + file);
            Log(ped + @"-" + file, "ERR", "SE ELIMINO DE PROCESADOS", "GENERAL");
            Console.ForegroundColor = ConsoleColor.White;
            #endregion
        }
        public void rarmov(string RutaOrigen)
        {
            DirectoryInfo Ruta = new DirectoryInfo(RutaOrigen);
            foreach (var rar in Ruta.GetFiles("*")) //ciclo para leer archivos de la ruta 
            {
                using (var archive = RarArchive.Open(rar))//creacion de objeto con los archivos del rar
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))//ciclo para leer lo extraido en el objeto
                    {
                     
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("RAR");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("SE HA MOVIDO EL ARCHIVO: " + entry + " A LA RUTA DESTINO: " + RutaDestino);
                        //Log(entry.ToString(),statusok, "SE HA MOVIDO A LA RUTA DESTINO: " + RutaDestino);
                  
                        entry.WriteToDirectory(rutdescompresion, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }

                }
            }
            DirectoryInfo Rutdescompresion = new DirectoryInfo(rutdescompresion);
            foreach (var file in Rutdescompresion.GetFiles("*", SearchOption.AllDirectories))
            {
                string Directory = Convert.ToString(file.Directory).Split(@"\".ToCharArray()).Last();
                //RutaDestinoMetodo(Directory);
                string Directoryname = Convert.ToString(file.DirectoryName);
                File.Copy(Directoryname + @"\" + file.ToString(), RutaDestino + @"\" + file.ToString(), true);
            }
        }
        public void zipmov(string origen, string destino)
        {
            using (FileStream zipToOpen = new FileStream(origen, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {    
                    foreach (ZipArchiveEntry files in archive.Entries)
                    {
                        string completeFileName;
                        if (files.Name == "")
                        {

                        }
                        else
                        {
                            if (files.Name.Contains(".zip"))//validacion de zio
                            {
                                string archivozip = files.ToString().Substring(0, 14);
                                ZipFile.ExtractToDirectory(rutoriginal + @"\"+archivozip+ @"\" + archivozip + @"\" +files.Name,rutdescompresion);
                            }
                            else if (files.Name.Contains(".rar"))
                            {
;
                            }
                            else
                            {
                                completeFileName = Path.Combine(destino, files.Name);
                                string directory = Path.GetDirectoryName(completeFileName);

                                if (!Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);

                                if (files.Name != "")
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Archivo: "+ files.Name);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("SE HA MOVIDO EL ARCHIVO: " + files.Name + " A LA RUTA DESTINO: " + destino);
                                //Log(files.Name, statusok, "SE HA MOVIDO A LA RUTA DESTINO: " + RutaDestino);
                                files.ExtractToFile(completeFileName, true);
                            }
                        }
                    }
                }
            }
        }     
        public void Log(string info,string status,string mensaje,string tiplog)
        {
            string NFecha = DateTime.Today.Day.ToString() + @"-" + DateTime.Today.Month.ToString() + @"-" + DateTime.Today.Year.ToString();
            string Nminutos = DateTime.Now.Hour.ToString() + @":" + DateTime.Now.Minute.ToString() + @":" + DateTime.Now.Second.ToString();
            string Log = ConfigurationManager.AppSettings["log"].ToString();
            string LogErr = ConfigurationManager.AppSettings["logerr"].ToString();
            if (status == "OK")
            {
                if(tiplog == "GENERAL")
                {
                    using (StreamWriter notepad = new StreamWriter(Log + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info + " - " + mensaje + @" - " + Nminutos);
                    }
                }
                else if(tiplog == "PROGESPEC")
                {
                    using (StreamWriter notepad = new StreamWriter(Log + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info  + " - " + mensaje + @" - " + Nminutos);
                    }
                }
                else if (tiplog == "SAC")
                {
                    using (StreamWriter notepad = new StreamWriter(Log + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info  + " - " + mensaje + @" - " + Nminutos);
                    }
                }
                else if (tiplog == "IMAR")
                {
                    using (StreamWriter notepad = new StreamWriter(Log + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info  + " - " + mensaje + @" - " + Nminutos);
                    }
                }
                else if(tiplog == "PROCESADOS")
                {
                    using (StreamWriter notepad = new StreamWriter(Log + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info  + " - " + mensaje + @" - " + Nminutos);
                    }
                }

            }
            else if(status == "ERR")
            {
                if (tiplog == "GENERAL")
                {
                    using (StreamWriter notepad = new StreamWriter(LogErr+@"\"+tiplog+@"\"+NFecha +@".txt", true))
                    {
                        notepad.WriteLine(info + @" - " +mensaje +@" - " +Nminutos);
                    }
                }
                else if (tiplog == "ELIMINADOS")
                {
                    using (StreamWriter notepad = new StreamWriter(LogErr + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info + @" - " + mensaje + @" - " + Nminutos);
                    }

                }
                else if (tiplog == "FALTANTES")
                {
                    using (StreamWriter notepad = new StreamWriter(LogErr + @"\" + tiplog + @"\" + NFecha + @".txt", true))
                    {
                        notepad.WriteLine(info + @" - " + mensaje + @" - " + Nminutos);
                    }

                }
            }      
        }
        public void delete()
        {
            try
            {
                DirectoryInfo Rutoriginal = new DirectoryInfo(rutoriginal);
                DirectoryInfo Rutaduanet = new DirectoryInfo(rutaduanet);
                foreach (var rar in Rutoriginal.GetFiles("*"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ELIMINADO: " + rar.ToString());
                    rar.Delete();          
                }
                foreach(var directory in Rutaduanet.GetDirectories("."))
                {
                  
                }

                
            }
            catch (Exception e)
            {
                string[] lines = { e.ToString() };
                System.IO.File.WriteAllLines(@"C:\Users\luisd\OneDrive\Escritorio\COGITO ED\log\error\error-" + DateTime.Now + @"-.txt", lines);
                Console.Write("El error es el siguiente: {0}", e);
            }
        }
        public void archvprocesados(string rutaorigen,string sfile)
        {
           
            String rutprocesadospath = Path.GetFullPath(rutaorigen + @"\" + sfile);
            //int rutprocesadoslenght = rutprocesados.Length;
            string[] rutprocesadossplit = rutprocesadospath.Split(@"\".ToCharArray());
            string rutprocesadosdestino = rutprocesados + @"\" + rutprocesadossplit[4] + @"\" + rutprocesadossplit[5] + @"\" + rutprocesadossplit[6] + @"\" + rutprocesadossplit[7] + @"\";
            DirectoryInfo di = Directory.CreateDirectory(rutprocesadosdestino);

            File.Copy(rutaorigen + @"\" + sfile, rutprocesadosdestino + sfile,true);
            Console.WriteLine("PROCESADOS:" + rutprocesadosdestino + sfile);
            Log(sfile,"OK", rutprocesadosdestino + sfile, "PROCESADOS");
            File.Delete(rutaorigen + @"\" + sfile);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ELIMINADO:"+ rutaorigen + sfile);
            Log(sfile, "ERR", rutaorigen + @"\" + sfile, "ELIMINADOS");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public void metimar(string file)
        {
            //EN PROCESO
            if (file.Contains("a3649") || file.Contains("e3649") || file.Contains("m3649"))// PEDIMENTO TRADICIONAL
            {
                File.Copy(rutoriginal + @"\" + file, rutimar);

            }
            else
            {

            }

        }
    }
}
