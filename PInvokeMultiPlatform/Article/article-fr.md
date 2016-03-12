# P/Invoke une DLL 32bits ou 64bits en fonction de la plateforme

Pour notre exemple nous allons simplementer créer une application Console qui va invoquer
la DLL Lua 5.3. Lua possède une fonction "lua_version" qui retourne un pointeur sur la valeur
de la version. Comme c'est un pointeur, si nous n'utilisons pas la bonne plateforme nous
provoquons une exception. Donc nos tests sont facilement vérifiables.

Les binaires de la DLL Lua pour Windows se trouvent sur SourceForge : http://luabinaries.sourceforge.net/download.html.

## Mise en oeuvre

### Création du projet

![Création du projet](create-project.png)

Si vous créez un projet .Net 4.5 il faut désactiver l'option "Préférer 32 bits" pour que notre
exemple fonctionne correctement.

### Intégration de la DLL

Pour nos essais on va intégrer la [DLL 32 bits](http://sourceforge.net/projects/luabinaries/files/5.3.2/Tools%20Executables/lua-5.3.2_Win32_bin.zip/download) directement dans notre projet.

![Intégration de la DLL](add-dll-in-root.png)

Puis on va la marquer comme contenu à copier depuis les propriétés.

![Marquer comme contenu](mark-dll-as-content.png)

### Création de la classe Wrapper

Ajouter un fichier de classe `Lua.cs`, on la rend statique puis importe la
fonction "lua_version". Comme cette fonction retourne un pointeur sur une valeur nous devons
écrire le code qui va extraire la valeur qui se trouve dans ce pointeur.

```csharp
    /// <summary>
    /// Lua DLL Wrapper
    /// </summary>
    public static class Lua
    {

        /// <summary>
        /// DLL Name
        /// </summary>
        const String LuaDllName = "Lua53.dll";

        /// <summary>
        /// Get Lua version
        /// </summary>
        /// <param name="L">Lua state. Can be null.</param>
        /// <returns>Number represents version</returns>
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_version")]
        private extern static IntPtr _lua_version(IntPtr L);
        public static double lua_version(IntPtr L)
        {
            var ptr = _lua_version(L);
            return (double)Marshal.PtrToStructure(ptr, typeof(double));
        }

    }
```

- Nous définissons une constante "LuaDllName" qui contient le nom de la DLL pour des raisons pratiques
- On défini la méthode "_lua_version" avec un attribut DllImport et on la déclare privée. "extern" permet de ne pas avoir à déclarer un corps de méthode
- On créé une méthode "lua_version" qui va récupérer le pointeur (IntPtr), puis on utilise les méthodes de marshaling pour convertir les données dans la mémoire pointée en donnée managée (double dans notre cas)

### Ecriture du code de test

Dans le fichier "program.cs" on écrit le code de test:

```csharp
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Version {0}", Lua.lua_version(IntPtr.Zero));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err ({0}): {1}", ex.GetType().Name, ex.GetBaseException().Message);
            }
            Console.Read();
        }
```

Ce code est simple, on affiche la version de la DLL en invoquant `Lua.lua-version()` avec un pointeur null ("lua_version" ne nécessite pas d'état Lua pour retourner une version).

On intercepte les erreurs dans un try/catch. On attend que l'on appui sur une touche pour quitter.

Dans l'état actuel des choses, si on exécute notre code sur un Windows 64 bits, une erreur "BadImageException" est provoquée car en étant en "Any CPU", .Net va s'exécuter selon la plateforme en cours, c'est-à-dire 64 bits. Comme notre DLL est en 32 bits, nous avons une erreur car le système à détecter un mauvais format entre l'exécutable et la DLL.

![Création du projet](bad-image-exception.png)


### Création des configurations de plateforme x86 et x64

Nous allons créer deux configurations de compilation pour nos tests:
- x86 pour compiler notre application en 32 bits
- x64 pour compiler noter application en 64 bits

Ouvrir le gestionnaire de configuration :

![Ouverture du gestionnaire de configuration](open-configuration-manager.png)

![Gestionnaire de configuration](configuration-manager.png)

Pour créer une nouvelle plateforme cliquer sur le sélecteur "Plateforme de la solution active" et sélectionner "&lt;Nouveau...&gt;":

![Créer une configuration](create-configuration.png)
 
Saisir **x86** dans la nouvelle plateforme

![Créer une configuration](new-configuration.png)

et valider la création.

Vérifier que la nouvelle plateforme créée est sélectionnée comme plateforme active, et s'assurer que la "Plateforme" du projet est également en **x86**. 

![Vérification de la plateforme](check-plateform-configuration.png)

Répéter l'opération pour créer la plateforme **x64**.

Fermer le gestionnaire de configuration.

Maintenant sélectionner la configuration **x86** pour tester le programme en 32 bits

![Sélection de la plateforme x86](select-x86.png)

et exécutons notre programme. Cette fois comme nous sommes en 32 bits, nous n'avons plus d'erreur de plateforme incompatible.
 
![Sélection de la plateforme x86](display-version.png)

### Préchargement de la DLL en fonction de la plateforme 

Nous allons mettre en place le préchargement de la DLL pour que chaque appel de DllImport s'effectue sur la DLL qui est déjà en mémoire.

Pour celà nous devons charger la DLL **AVANT** que le premier DllImport ai lieu. Pour celà on va tout simplement utiliser le constructeur statique de notre classe Lua. Ce constructeur étant appelé dés que la classe Lua est référencée, nous serons sûrs que le chargement s'effectuera avant l'appel à notre fonction.

Nous devons avant tout organiser nos DLL dans des dossiers séparés en fonction de la plateforme mais la DLL doit avoir le même nom.

Dans le projet créons le dossier "x86" et déplacons le fichier "lua53.dll" dedans.


![Déplacement de la DLL dans x86](move-dll-in-x86.png)

Créons ensuite le dossier "x64", et ajoutons la [DLL en version 64 bits](http://sourceforge.net/projects/luabinaries/files/5.3.2/Tools%20Executables/lua-5.3.2_Win64_bin.zip/download).

Comme pour la DLL 32 bits, marquer la DLL comme contenu à copier si plus récent.

Dans la classe `Lua` du fichier `Lua.cs` on va définir la méthode pour invoquer l'API Window 'LoadLibrary'.

```csharp
[DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = false)] 
private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
```

Ensuite on va définir le constructeur statique qui va charger la librairie en fonction de la plateforme.

```csharp
/// <summary>
/// Preload the Lua DLL
/// </summary>
static Lua()
{
    // Check the size of the pointer
    String folder = IntPtr.Size == 8 ? "x64" : "x86";
    // Build the full library file name
    String libraryFile = Path.Combine(Path.GetDirectoryName(typeof(Lua).Assembly.Location), folder, LuaDllName);
    // Load the library
    var res = LoadLibrary(libraryFile);
    if (res == IntPtr.Zero)
        throw new InvalidOperationException("Failed to load the library.");
    System.Diagnostics.Debug.WriteLine(libraryFile);
}
```

Pour détermine la plateforme il existe plusieurs méthodes. Dans notre cas j'utilise la technique de la taille du pointeur: sur une plateforme 32 bits un pointeur fait 4 octets (4 * 8 = 32 bits), alors que sur une plateforme 64 bits un pointeur fait 8 octets (8 * 8 = 64 bits).

En fonction de cette taille on détermine le dossier dans lequel on va trouver la DLL, et on la charge en mémoire avec l'API "LoadLibrary".

Maintenant on peut exécuter notre programme avec la configuration **x86**, **x64** et bien sûr **Any CPU**. Dans la console de sortie en mode débuggage de Visual Studio, le nom complet du fichier chargé est affiché permettant de confirmer le chargement de la bonne librarie.


