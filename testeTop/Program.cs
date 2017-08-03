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
              

        public TownCrier()
        {
            
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

            //Verificar arquivo e enviar para ftp
            //Upload de arquivo as 23:00
            
            
           
            
            
            //Verificar data do servidor
            

            while(true)
            {
                Console.WriteLine(DateTime.Now);
                string horarioDeterminado = "12:23";
                string horarioAtual = DateTime.Now.ToString("h:mm");

                while (horarioDeterminado == horarioAtual)
                 {
                    string[] nomeArquivo = Directory.GetFiles(pathFiles);
                    foreach (var item in nomeArquivo)
                     {
                        
                        FileInfo arquivo = new FileInfo(item);
                        string file = Path.GetFileName(item);
                        string arquivoDestino = pathFiles + @"\backup\" + file;
                        Console.WriteLine(item);
                        Console.WriteLine(file);
                        Console.WriteLine(arquivoDestino);
                        //EnviarArquivoFtp("ftp://172.", "TheUserName", "ThePassword", item);
                        arquivo.MoveTo(arquivoDestino);


                    }

                }
                Thread.Sleep(20000);
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

                 

        }

       
    }
}
