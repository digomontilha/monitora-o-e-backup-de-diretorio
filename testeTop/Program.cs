using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Topshelf;
using Tamir.SharpSsh;
using Renci.SshNet;

namespace MoBaFtp

{
    class Program
    {
        static void Main(string[] args)
        {
           

            HostFactory.Run(x =>                                 
            {
                x.Service<TownCrier>(s =>                        
                {
                    s.ConstructUsing(name => new TownCrier());   
                    s.WhenStarted(tc => tc.Start());             
                    s.WhenStopped(tc => tc.Stop());              
                });
                x.RunAsLocalSystem();                            

                x.SetDescription("Monitoração de pasta e envio via ftp");        
                x.SetDisplayName("MoBaFtp");                       
                x.SetServiceName("MoBaFtp"); 
                                      
            });                                                       

        }
    }

    
    public class TownCrier
    {

        
        public TownCrier()
        {
           
        }
       

        public void NetMonitorar()
        {
            //pasta com os arquivos a serem verificados
            //string pathFiles = @"\\ppspdb01\FileChange\Abbott";
             
             string pathFiles = @"C:\LOG";
            //Console.WriteLine(pathFiles);

            //Objeto para minutoração da pasta
            FileSystemWatcher Monitorar = new FileSystemWatcher();

            //passado o caminho para objeto
            Monitorar.Path = pathFiles;

            //tipos de monitoração
            Monitorar.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            //Filtro para monitorar arquivos
            Monitorar.Filter = "*";

            //Eventos a ser monitorados
            Monitorar.Changed += new FileSystemEventHandler(OnChanged);
            Monitorar.Created += new FileSystemEventHandler(OnChanged);
            Monitorar.Deleted += new FileSystemEventHandler(OnChanged);
            Monitorar.Renamed += new RenamedEventHandler(OnRenamed);
            
            //Ativando eventos
            Monitorar.EnableRaisingEvents = true;


            /*Verificar arquivo e enviar para ftp
            Upload de arquivo as 23:00
            Verificar data do servidor*/
            string horarioDeterminado = "10:14";
            

            while (true)
            {
              //  Console.WriteLine(DateTime.Now.ToString("H:mm"));

                //verificar se pastas backup existe
                //if (false == File.Exists(pathFiles + @"\backup"))
                //{
                //    // cria a pasta backup
                //    Directory.CreateDirectory(pathFiles + @"\backup");
                //}

                while (horarioDeterminado == DateTime.Now.ToString("H:mm"))
                 {
                    string[] nomeArquivo = Directory.GetFiles(pathFiles);
                    foreach (var item in nomeArquivo)
                     {
                        
                        
                        string file = Path.GetFileName(item);
                        string arquivoDestino = @"C:\LOG\backup\" + DateTime.Now.ToString("ddMM-Hmm-s-") + item;

                        status = EnviarArquivoFtp("172.16.101.13", "abtt.sftp", "ThePassword", "", item);
                        if (status == true)
                        {
                            File.Move(item, arquivoDestino);
                        }
                        //Console.WriteLine(item);
                        //Console.WriteLine(file);
                        //Console.WriteLine(arquivoDestino);
                        
                        Thread.Sleep(10000);
                        
                        

                        StreamWriter escrever = new StreamWriter("backup.log", true);
                        escrever.WriteLine(@"arquivo movido para backup: {0} renomeado para: {1}", item ,arquivoDestino);
                        escrever.Close();

                    }

                }
                Thread.Sleep(30000);
            }
        }


        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            StreamWriter escrever = new StreamWriter("log.log", true);
            //Console.WriteLine(@"Arquivo: {0}, Evento: {1}, Data: {2}", e.FullPath, e.ChangeType, DateTime.Now.ToString("dd-MM/H:mm"));
            escrever.WriteLine(@"Arquivo: {0}, Evento: {1}, Data: {2}", e.FullPath, e.ChangeType, DateTime.Now.ToString("dd-MM/H:mm"));
            escrever.Close();

        }
        public static void OnRenamed(object source, RenamedEventArgs e)
        {
        
           StreamWriter escrever = new StreamWriter("log.log", true);
           

           //Console.WriteLine(@"Arquivo: {0} alterou para {1}, Data:{2}", e.OldFullPath, e.FullPath, DateTime.Now.ToString("dd-MM/H:mm"));
           escrever.WriteLine(@"Arquivo: {0} alterou para {1}, Data:{2}", e.OldFullPath, e.FullPath, DateTime.Now.ToString("dd-MM/H:mm"));
           escrever.Close();
        }

        public bool status;
        public bool EnviarArquivoFtp(string ftpServer, string userName, string password, string filename, string path)
        {
         

            try
            {
                using (SftpClient client = new SftpClient(ftpServer, 115, userName, password))
                {
                    string workingdirectory = "/highway/hell";
                    FileInfo f = new FileInfo(path);
                    string uploadfile = f.FullName;
                    
                    client.Connect();
                    if (client.IsConnected)
                    {
                        Console.WriteLine("Conectado");
                    }

                    var fileStream = new FileStream(uploadfile, FileMode.Open);
                    if (fileStream != null)
                    {
                        Console.WriteLine("Não é null");
                    }
                    client.BufferSize = 4 * 1024;
                    client.ChangeDirectory(workingdirectory);
                    client.UploadFile(fileStream, f.Name, null);
                    client.Disconnect();
                    client.Dispose();


                    StreamWriter writer = new StreamWriter("arquivos.log", true);


                    Console.WriteLine(@"Upload de Arquivo:{0} as {1}", filename, DateTime.Now.ToString("dd-MM-H:mm"));
                    writer.WriteLine(@"Upload de Arquivo:{0} as {1}", filename, DateTime.Now.ToString("dd-MM-H:mm"));



                    status = true;
                    
                }
            }
            catch (Exception e)
            {
                if (e.Message != null)
                {
                    StreamWriter writer = new StreamWriter("arquivos.log", true);
                    Console.WriteLine(@"Erro: {0} , data: {1}", e.Message, DateTime.Now.ToString("dd-MM-H:mm"));
                    writer.WriteLine(@"Erro: {0} , data: {1}", e.Message, DateTime.Now.ToString("dd-MM-H:mm"));
                    status = false;
                    
                }
                    
            }

            return (status);

        }

        public void Start()
        {

            NetMonitorar();

        }

       

        public void Stop()
        {

            var processes = Process.GetProcessesByName("MoBaFtp.vshost");
            foreach (var p in processes)
            p.Kill();


            
        }

       
    }
}
