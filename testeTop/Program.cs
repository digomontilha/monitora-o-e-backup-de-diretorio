using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

using Topshelf;

namespace testeTop
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
                x.SetDisplayName("Montirar");                       
                x.SetServiceName("Montirar");                       
            });                                                       

        }
    }

    
    public class TownCrier
    {

        private readonly CancellationTokenSource _cancellationTokenSource;
        public TownCrier()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

    public void MetMonitorar()
        {
            
            //pasta com os arquivos a serem verificados
            string pathFiles = @"C:\LOG";
            Console.WriteLine(pathFiles);

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
            string horarioDeterminado = "14:35";
            string horarioAtual = DateTime.Now.ToString("H:mm");

            while (true)
            {
                Console.WriteLine(horarioAtual);

                if (false == File.Exists(pathFiles + @"\backup"))
                {
                    // This path is a file
                    Directory.CreateDirectory(pathFiles + @"\backup");
                }

                while (horarioDeterminado == horarioAtual)
                 {
                    string[] nomeArquivo = Directory.GetFiles(pathFiles);
                    foreach (var item in nomeArquivo)
                     {
                        
                        
                        string file = Path.GetFileName(item);
                        string arquivoDestino = pathFiles + @"\backup\" +DateTime.Now.ToString("ddMM-Hmmss") + file;
                        Console.WriteLine(item);
                        Console.WriteLine(file);
                        Console.WriteLine(arquivoDestino);
                        //EnviarArquivoFtp("ftp://172.", "TheUserName", "ThePassword", item);
                        File.Move(item, arquivoDestino);

                        StreamWriter escrever = new StreamWriter("backup.log", true);
                        escrever.WriteLine(@"arquivo movido para backup: {0} renomeado para: {1}", item ,arquivoDestino);
                        escrever.Close();

                    }

                }
                Thread.Sleep(2000);
            }
        }


        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            StreamWriter escrever = new StreamWriter("log.log", true);
            Console.WriteLine(@"Arquivo: {0}, Evento: {1}, Data: {2}", e.FullPath, e.ChangeType, DateTime.Now);
            escrever.WriteLine(@"Arquivo: {0}, Evento: {1}, Data: {2}", e.FullPath, e.ChangeType, DateTime.Now);
            escrever.Close();

        }
        public static void OnRenamed(object source, RenamedEventArgs e)
        {
        
           StreamWriter escrever = new StreamWriter("log.log", true);
           

           Console.WriteLine(@"Arquivo: {0} alterou para {1}, Data:{2}", e.OldFullPath, e.FullPath, DateTime.Now);
           escrever.WriteLine(@"Arquivo: {0} alterou para {1}, Data:{2}", e.OldFullPath, e.FullPath, DateTime.Now);
           escrever.Close();
        }


        public void EnviarArquivoFtp(string ftpServer, string userName, string password, string filename)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.UploadFile(ftpServer + "/" + new FileInfo(filename).Name, "STOR", filename);
                StreamWriter writer = new StreamWriter("arquivos.log", true);
                DateTime data = DateTime.Now;

                Console.WriteLine(@"Upload de Arquivo:{0} as {1}", filename, data);
                writer.WriteLine(@"Upload de Arquivo:{0} as {1}", filename, data);
            }

        }

        public void Start() {

            MetMonitorar();
            


        }

       

        public void Stop() {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            

        }

       
    }
}
