using Pixelbyte.Extensions;
using UnityEngine;

namespace Pixelbyte
{
    /// <summary>
    // A Singleton class utility
    // Author: Bryan Castleberry
    // Copyright 2018 Pixelbyte Studios LLC
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        const string SINGLETONS_GOB = "Singletons";

        /// <summary>
        /// The one and (hopefully) only instance of this singleton
        /// </summary>
        protected static T _instance = null;

        #region Fields to Change in derived classes
        /// <summary>
        /// Derived classes should set this in their static constructor
        /// </summary>
        protected static string gobName = "<gobName>";

        /// <summary>
        /// If this is true then the singleton instance must be created in the scene editor
        /// or you will get an error
        /// </summary>
        protected static bool mustCreateInSceneEditor = false;

        /// <summary>
        /// If true, then the Singleton will persist between scenes
        /// </summary>
        protected static bool dontDestroy = false;

        /// <summary>
        /// If true, then if another newer instance of this singleton already exists,
        /// it will destroy itself
        /// </summary>
        protected static bool destroyNewerInstance = true;

        /// <summary>
        /// If true, then GameObject.Find(gobName) is used to look for the singleton
        /// Otherwise, it is searched for by looking for the FIRST object that contains the Monobehaviour of type T
        /// </summary>
        protected static bool findByName = false;

        #endregion

        /// <summary>
        /// If true, then an instance of the object already existed in the scene and was not created
        /// </summary>
        protected static bool existedInScene = true;

        /// <summary>
        /// True if the application is quitting
        /// </summary>
        protected static bool applicationQuitting = false;
        //private static bool wasDestroyed;

        private static bool parametersSet = false;

        #region Properties
        //public static bool WasDestroyed { get { return wasDestroyed; } }

        public static T I
        {
            get
            {
                //Dbg.Log("get:{0}", typeof(T).Name);
                if (applicationQuitting)
                {
                    Dbg.Warn("Singleton: " + typeof(T) + " Has been destroyed on application quit. Won't recreate");
                    return null;
                }
                //else if (wasDestroyed)
                //{
                //    Dbg.Warn("Singleton: " + typeof(T) + " was destroyed. Won't be recreated until a new scene is loaded");
                //    return null;
                //}
                else if (_instance == null)
                {
                    //wasDestroyed = false;
                    //If SetParameters() hasn't been called, 
                    //call the static constructor for this type and check again
                    if (!parametersSet)
                    {
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);

                        if (!parametersSet)
                        {
                            Dbg.Err(typeof(T) + " singleton must call SetParameters() in its static constructor!");
                            return null;
                        }
                    }

                    if (mustCreateInSceneEditor)
                    {
                        Dbg.Err("A " + typeof(T) + " instance must be created in the scene editor!");
                        return null;
                    }
                    else
                    {
                        //If we have to create it here then it must mean that it did not exist in the scene
                        existedInScene = false;

                        //Try to initialize the singleton,if we fail return null
                        if (!Init(dontDestroy))
                            return null;
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// True if an instance of this object exists, false otherwise
        /// If either the _instance is null OR the application is quitting, this will return FALSE
        /// </summary>
        public static bool Exists
        {
            get
            {
                return _instance != null && !applicationQuitting
#if UNITY_EDITOR
                    && Application.isPlaying
#endif
                    ;
            }
        }
        #endregion

        protected static void SetParameters(string objName, bool mustCreatInScene = false, bool persistBetweenScenes = false, bool findUsingName = false, bool destroyNewer = true)
        {
            gobName = objName;
            mustCreateInSceneEditor = mustCreatInScene;
            dontDestroy = persistBetweenScenes;
            findByName = findUsingName;
            destroyNewerInstance = destroyNewer;
            parametersSet = true;
        }

        void OnDestroy()
        {
            //Dbg.Log("{0} : [OnDestroy]", typeof(T));

            if (_instance != null && !applicationQuitting)
                Cleanup();
            //wasDestroyed = true;
            _instance = null;
        }

        void OnApplicationQuit()
        {
            //Dbg.Log("{0} : [OnApplicationQuit]", typeof(T));
            applicationQuitting = true;
        }

        /// <summary>
        /// Override to do any cleanup needed before destruction.
        /// Note: Cleanup() will NOT be called unless the static _instance != null
        /// </summary>
        protected virtual void Cleanup() { }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                //wasDestroyed = false;
                Init(dontDestroy);
            }
            else
            {
                //This allows us to persist between scenes and have the "oldest" instance
                //of the Singelton persist. Great for having an instance of a singleton in all scenes for debugging.
                if (destroyNewerInstance)
                {
                    Dbg.Warn($"An instance of {typeof(T).Name} already exists on {_instance.gameObject.name} in Scene {_instance.gameObject.scene.name}. Destroying new instance: {gameObject.name} in scene {gameObject.scene.name}.");
                    Destroy(gameObject);
                }
            }
            //gameObject.name = gobName;
        }

        /// <summary>
        /// This method encapsulates much of the logic required for a singleton object
        ///
        /// Ex:
        /// public class Mgr : Monobehaviour
        /// {
        ///     private static Mgr _instance;
        ///     public static I Mgr {get {if (_instance == null) Init(); return _instance; } }
        ///     public static Init() { 
        ///     if (_instance == null)
        ///         Singleton.Init("_mgr", ref _instance, true); }
        ///     //TODO: The rest of your code here
        /// }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gobName"></param>
        /// <param name="instance"></param>
        /// <param name="dontDestroy"></param>
        static bool Init(bool dontDestroy = true)
        {
            if (_instance == null)
            {
                //Dbg.Log("{0} was null. Looking for existing GameObject..", typeof(T).Name);

                if (findByName)
                {
                    GameObject gob = GameObject.Find(gobName);
                    if (gob == null)
                    {
                        //Dbg.Log("Unable to find {0}. Creating one...", typeof(T).Name);
                        gob = CreateSingletonGameObject();
                    }
                    else
                    {
                        //Dbg.Log("{0} found, retrieving {0} Component...", typeof(T).Name);
                        //instance = gob.GetComponent<T>();

                        //if (instance == null)
                        //    Dbg.Err("Uh oh, there is no {0} component!!!", typeof(T).Name);
                        _instance = gob.GetOrAddComponent<T>();
                    }
                }
                else
                {
                    //Find the singleton by Type instead of by game object name
                    T[] t = FindObjectsOfType<T>();

                    //Didn't find any? Then create one
                    if (t == null || t.Length == 0)
                    {
                        CreateSingletonGameObject();
                    }
                    else
                    {
                        if (t.Length > 1)
                            Dbg.Warn($"Found multiple instances of: {typeof(T).Name}! Singletons can't have multiple instances!");
                        //Assign our instance to the first one we find
                        _instance = t[0];
                    }
                }
            }
            //             else
            //             {
            // #if UNITY_EDITOR
            //             _instance.transform.SetAsFirstSibling();
            // #endif
            //             }

            if (dontDestroy)
                DontDestroyOnLoad(_instance.gameObject);

            return true;
        }

        static GameObject CreateSingletonGameObject()
        {
            var gob = new GameObject(gobName);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.localRotation = Quaternion.identity;
            gob.transform.localScale = Vector3.one;
            _instance = gob.AddComponent<T>();

            //Dbg.Log(string.Format("Created a new: {0} in scene: {1}", typeof(T).Name, gob.scene));

            //Setup all created singletons to be a child of the
            //default Singletons Gameobject, if it exists
            var parent = GameObject.Find(SINGLETONS_GOB);
            if (parent != null)
                gob.transform.SetParent(parent.transform);

            return gob;
        }
    }
}