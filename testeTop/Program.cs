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

            HostFactory.Run(x =>                                 //1
            {
                x.Service<TownCrier>(s =>                        //2
                {
                    s.ConstructUsing(name => new TownCrier());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("Sample Topshelf Host");        //7
                x.SetDisplayName("Stuff");                       //8
                x.SetServiceName("Stuff");                       //9
            });

        }
    }

    public class TownCrier
    {
        readonly System.Timers.Timer _timer;
        public TownCrier()
        {
            //pasta com os arquivos a serem verificados
            string pathFiles = @"C:\LOG";

            //Objeto para minutoração da pasta
            FileSystemWatcher Monitorar = new FileSystemWatcher();

            //passado o caminho para objeto
            Monitorar.Path = pathFiles;

            //tipos de monitoração
            Monitorar.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            //Filtro para monitorar arquivos
            Monitorar.Filter = "*.*";

            //Eventos a ser monitorados
            Monitorar.Changed += new FileSystemEventHandler(OnChanged);
            Monitorar.Created += new FileSystemEventHandler(OnChanged);
            Monitorar.Deleted += new FileSystemEventHandler(OnChanged);
            Monitorar.Renamed += new RenamedEventHandler(OnRenamed);
            
            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => Console.WriteLine("It is {0} and all is well", DateTime.Now);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            StreamWriter escrever = new StreamWriter("log.log", true);
            DateTime data = new DateTime();

            Console.WriteLine(@"Arquivo: {0}, Evento: {1}, Data: {2}", e.FullPath, e.ChangeType, data);
            escrever.WriteLine(@"Arquivo: {0}, Evento: {1}, Data: {2}", e.FullPath, e.ChangeType, data);
            escrever.Close();

        }
        public static void OnRenamed(object source, RenamedEventArgs e)
        {
        
           StreamWriter escrever = new StreamWriter("log.log", true);
           DateTime data = DateTime.Now;

           Console.WriteLine(@"Arquivo: {0} alterou para {1}, Data:{2}", e.OldFullPath, e.FullPath, data);
           escrever.WriteLine(@"Arquivo: {0} alterou para {1}, Data:{2}", e.OldFullPath, e.FullPath, data);
           escrever.Close();
        }




      


        public void Start() { _timer.Start(); }
        public void Stop() { _timer.Stop(); }
    }
}
