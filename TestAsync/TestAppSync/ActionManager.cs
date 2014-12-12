using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestAppSync
{
    public class ActionManager
    {
        readonly Object LockObject = new object();
        Task _Worker;
        CancellationTokenSource _Canceler;
        ManualResetEvent _ActionAvailable;
        Queue<Action> _Actions;

        /// <summary>
        /// Traitement de la tâche
        /// </summary>
        void WorkProcess()
        {
            // Initialisation
            var cToken = _Canceler.Token;

            // On démarre le manager dans le thread
            Log("Initialisation du manager");
            Thread.Sleep(100);

            // Boucle de traitement tant qu'on ne demande pas l'arrêt du processus
            while (!cToken.IsCancellationRequested)
            {
                // On attend qu'une action soit disponible
                if (_ActionAvailable.WaitOne(TimeSpan.FromMilliseconds(100)))
                {
                    // On dépile une action
                    Action act = null;
                    lock (LockObject)
                        act = _Actions.Count > 0 ? _Actions.Dequeue() : null;   // Mode parano
                    // Si on a une action on l'exécute
                    if (act != null)
                        act();
                    // Reset le waiter d'action si on a plus d'action
                    lock (LockObject)
                    {
                        if (_Actions.Count == 0)
                            _ActionAvailable.Reset();
                    }
                }
            }

            // Arrêt du manager
            Log("Finalisation du manager");

        }

        /// <summary>
        /// Démarrage du manager
        /// </summary>
        public void Start()
        {
            Task tskWorker;
            lock (LockObject) tskWorker = _Worker;
            if (tskWorker != null) throw new InvalidOperationException("Le manager est déjà en cours d'exécution");
            // Création de la tâche
            _Canceler = new CancellationTokenSource();
            _ActionAvailable = new ManualResetEvent(false);
            _Actions = new Queue<Action>();
            _Worker = Task.Factory.StartNew(WorkProcess, _Canceler.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Arrêt du manager
        /// </summary>
        public void Stop()
        {
            // On récupère la tâche pour l'arrêter
            Task tskWorker;
            lock (LockObject) tskWorker = _Worker;
            if (tskWorker == null) return;
            lock (LockObject) _Worker = null;

            // Demande l'arrêt de la tâche
            _Canceler.Cancel();
            // On attends que la tâche s'arrête
            tskWorker.Wait();
            // On fait le ménage
            _Canceler = null;
        }

        /// <summary>
        /// Provoque l'exécution d'une action dans le manager
        /// </summary>
        public void SendAction(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");
            Task tskWorker;
            lock (LockObject) tskWorker = _Worker;
            if (tskWorker == null) throw new InvalidOperationException("Le manager n'est pas en cours d'exécution");
            lock (LockObject)
            {
                _Actions.Enqueue(action);
                _ActionAvailable.Set();
            }
        }

        /// <summary>
        /// Provoque l'exécution d'une action dans le manager et retourne une tâche qui va attendre la fin de l'exécution
        /// </summary>
        public Task SendActionAsync<T>(Func<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            Task tskWorker;
            lock (LockObject) tskWorker = _Worker;
            if (tskWorker == null) throw new InvalidOperationException("Le manager n'est pas en cours d'exécution");
            TaskCompletionSource<T> result = new TaskCompletionSource<T>();
            lock (LockObject)
            {
                _Actions.Enqueue(() => {
                    try
                    {
                        result.SetResult(action());
                    }
                    catch (AggregateException aex)
                    {
                        result.SetException(aex.InnerExceptions);
                    }
                    catch (Exception ex)
                    {
                        result.SetException(ex);
                    }
                });
                _ActionAvailable.Set();
            }
            return result.Task;
        }

        public void Log(String message, params object[] args)
        {
            String msg = args != null ? String.Format(message, args) : message;
            var h = OnLog;
            if (h != null)
                h(this, new LogEventArgs(msg));
        }

        public int ActionCount { get { return _Actions.Count; } }

        public event EventHandler<LogEventArgs> OnLog;
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(String message)
        {
            this.Message = message;
        }
        public String Message { get; private set; }
    }

}
