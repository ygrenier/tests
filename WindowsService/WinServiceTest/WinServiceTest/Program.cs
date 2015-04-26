using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WinServiceTest
{
    static class Program
    {

        /// <summary>
        /// Exécute les services en mode interactif
        /// </summary>
        static void RunInteractiveServices(ServiceBase[] servicesToRun)
        {
            Console.WriteLine();
            Console.WriteLine("Démarrage des services en mode intéractif.");
            Console.WriteLine();

            // Récupération de la méthode a exécuter sur chaque service pour le démarrer 
            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);

            // Boucle de démarrage des services
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Démarrage de {0} ... ", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.WriteLine("Démarré");
            }

            // Attente de la fin
            Console.WriteLine();
            Console.WriteLine("Appuyer sur une touche pour arrêter les services et terminer le processus...");
            Console.ReadKey();
            Console.WriteLine();

            // Récupération de la méthode à exécuter sur chaque service pour l'arrêter
            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Boucle d'arrêt
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Arrêt de {0} ... ", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Arrêté");
            }

            Console.WriteLine();
            Console.WriteLine("Tous les services sont arrêtés.");

            // Attend l'appui d'une touche pour ne pas retourner directement à VS
            Console.WriteLine();
            Console.Write("=== Appuyer sur une touche pour quitter ===");
            Console.ReadKey();
        }

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        static void Main()
        {
            // Initialisation du service à démarrer
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new svcMyService() 
            };

            // On est en mode intéractif et débogage ?
            if (Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached)
            {
                // Simule l'exécution des services
                RunInteractiveServices(ServicesToRun);
            }
            else
            {
                // Exécute les services normalement
                ServiceBase.Run(ServicesToRun);
            }
        }

    }
}
