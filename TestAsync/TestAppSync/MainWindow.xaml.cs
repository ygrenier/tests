using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestAppSync
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ActionManager _Manager;

        public MainWindow()
        {
            InitializeComponent();
            CheckControlsState();
        }

        void CheckControlsState()
        {
            btnStartManager.IsEnabled = _Manager == null;
            btnStopManager.IsEnabled = _Manager != null;
            actions.Visibility = _Manager != null ? Visibility.Visible : Visibility.Hidden;
        }

        void _Manager_OnLog(object sender, LogEventArgs e)
        {
            Log(e.Message);
        }

        private void Log(String message, params object[] args)
        {
            // Création d'un message avec l'ID du thread en cours d'exécution
            String msg = String.Format("#{0} : {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, args != null ? String.Format(message, args) : message);
            Dispatcher.InvokeAsync(() => {
                tbLog.Text += msg + Environment.NewLine;
            });
        }

        private void StartManager()
        {
            Log("Création du manager");
            tbLog.Clear();
            _Manager = new ActionManager();
            Log("Démarrage du manager");
            _Manager.OnLog += _Manager_OnLog;
            _Manager.Start();
        }

        private void StopManager()
        {
            Log("Arrêt du manager");
            _Manager.Stop();
            Log("Suppression du manager");
            _Manager.OnLog -= _Manager_OnLog;
            _Manager = null;
        }

        private void btnStartManager_Click(object sender, RoutedEventArgs e)
        {
            if (_Manager == null)
            {
                StartManager();
            }
            CheckControlsState();
        }

        private void btnStopManager_Click(object sender, RoutedEventArgs e)
        {
            if (_Manager != null)
            {
                StopManager();
            }
            CheckControlsState();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Si le manager est en cours il faut l'arrêter
            if (_Manager != null)
            {
                StopManager();
            }
        }

        private void btnAction1_Click(object sender, RoutedEventArgs e)
        {
            if (_Manager != null)
            {
                // Envoi une action dans la pile
                _Manager.SendAction(() => {
                    Log("Démarrage l'action 1");
                    Task.Delay(1000).Wait();
                    Log("Arrête l'action 1");
                });
            }
        }

        private async void btnAction2_Click(object sender, RoutedEventArgs e)
        {
            // Envoi une action dans la pile
            Log("Définition de l'action 1");
            _Manager.SendAction(() => {
                Log("Démarrage l'action 1");
                for (int i = 0; i < 10; i++)
                {
                    Log("Action 1 : étape {0}", i + 1);
                    Task.Delay(100).Wait();
                }
                Log("Arrête l'action 1");
            });
            Log("Action 1 définie");

            // Envoi une action dans la pile avec exécution de code lorsqu'elle a terminée
            Log("Définition de l'action 2");
            var t = _Manager.SendActionAsync<bool>(() => {
                Log("Démarrage l'action 2");
                for (int i = 0; i < 10; i++)
                {
                    Log("Action 2 : étape {0}", i + 1);
                    Task.Delay(100).Wait();
                }
                Log("Arrête l'action 2");
                return true;
            }).ContinueWith(antecedent => {
                Log("L'action 2 est terminée");
            }, TaskScheduler.FromCurrentSynchronizationContext());
            Log("Action 2 définie");

            // Envoie une action dans la pile et attend qu'elle est terminée
            Log("Définition de l'action 3");
            await _Manager.SendActionAsync<bool>(() => {
                Log("Démarrage l'action 3");
                for (int i = 0; i < 15; i++)
                {
                    Log("Action 3 : étape {0}", i + 1);
                    Task.Delay(100).Wait();
                }
                Log("Arrête l'action 3");
                return true;
            });
            Log("Action 2 définie");
            Log("L'action 2 est terminée");

            // Envoie une dernière action, mais sa définition n'aura lieu qu'une fois que l'action 3 est terminée
            Log("Définition de l'action 4");
            _Manager.SendAction(() => {
                Log("Démarrage l'action 4");
                for (int i = 0; i < 5; i++)
                {
                    Log("Action 4 : étape {0}", i + 1);
                    Task.Delay(100).Wait();
                }
                Log("Arrête l'action 4");
            });
            Log("Action 4 définie");

        }

    }
}
