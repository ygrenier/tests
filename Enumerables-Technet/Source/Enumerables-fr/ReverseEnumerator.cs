using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{

    /// <summary>
    /// Enumérateur parcourant une liste dans le sens inverse
    /// </summary>
    public class ReverseEnumerator<T> : IEnumerator<T>
    {
        IList<T> _Source;
        int _Position;
        bool _Completed;

        /// <summary>
        /// Création d'un nouvel énumérateur
        /// </summary>
        public ReverseEnumerator(IList<T> source)
        {
            this._Source = source;
            // On met -1 pour indiquer qu'on a pas commencé l'itération
            this._Position = -1;
            // L'itération n'est pas terminée
            this._Completed = false;
            // On défini Current avec la valeur par défaut
            this.Current = default(T);
        }

        /// <summary>
        /// Libération des ressources
        /// </summary>
        public void Dispose()
        {
            // On a rien à libérer , mais on marque notre itérateur comme terminé
            this._Completed = true;
            Console.WriteLine("Enumerateur disposé");
        }

        /// <summary>
        /// Cette méthode est appelée lorsque l'on veut réinitialiser l'énumérateur
        /// </summary>
        public void Reset()
        {
            // On met -1 pour indiquer qu'on a pas commencer l'itération
            this._Position = -1;
            // L'itération n'est pas terminée
            this._Completed = false;
            // On défini Current avec la valeur par défaut
            this.Current = default(T);
        }

        /// <summary>
        /// On se déplace vers le prochain élément
        /// </summary>
        /// <returns>False lorsque l'itération est terminée</returns>
        public bool MoveNext()
        {
            // Si la source est Null alors on a rien à parcourir, donc l'itération s'arrête
            if (this._Source == null) return false;

            // Si l'itération est terminée alors on ne va pas plus loin
            if (this._Completed) return false;

            // Si la position est à -1 on récupère le nombre d'éléments à parcourir pour démarrer l'itération
            if (this._Position == -1)
            {
                Console.WriteLine("Itération commencée");
                this._Position = _Source.Count;
            }

            // On se déplace dans la liste
            this._Position--;

            // Si on a atteind -1 alors on a terminé l'itération
            if (this._Position < 0)
            {
                this._Completed = true;
                Console.WriteLine("Itération terminée");
                return false;
            }

            // On défini Current et on continue
            Current = this._Source[this._Position];

            return true;
        }

        /// <summary>
        /// Elément en cours de l'itération
        /// </summary>
        public T Current { get; private set; }

        /// <summary>
        /// Elément en cours pour la version non générique
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

    }

}
