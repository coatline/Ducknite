using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pixelbyte
{
    /// <summary>
    /// This class provides an object pool manager
    /// 2017 Pixelbyte Studios
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Pool : Singleton<Pool>
    {
        const string GOB_NAME = "_MgrPool";

        /// <summary>
        /// When Spawn is called on an object and it doesn't have a pool already created,
        /// This is the default max value set for that pool (set to 0 for unlimited)
        /// </summary>
        const int MAX_AUTO_POOLED = 10;

        static Pool() { SetParameters(GOB_NAME); }

        public enum StartupPoolMode { Awake, Start, CallManually };

        [System.Serializable]
        public class StartupPool
        {
            public int initialElements = 5;
            public int maxElements = 10;
            public bool allowUnpooled = true;
            public GameObject prefab = null;
        }

        Dictionary<int, PoolDescriptor> poolDescriptors = new Dictionary<int, PoolDescriptor>();
        Dictionary<int, PoolDescriptor> spawnedObjects = new Dictionary<int, PoolDescriptor>();

        //Stick the gameobject delayed recycle Coroutines in here
        Dictionary<int, Coroutine> recycleRoutines = new Dictionary<int, Coroutine>();

        bool startupPoolsCreated;

        public StartupPoolMode startupPoolMode = StartupPoolMode.CallManually;
        public StartupPool[] startupPools = null;

        protected override void Awake()
        {
            base.Awake();

            if (startupPoolMode == StartupPoolMode.Awake)
                CreateStartupPools();
        }

        void Start()
        {
            if (startupPoolMode == StartupPoolMode.Start)
                CreateStartupPools();
        }

        #region Pool Creation

        /// <summary>
        /// Call this to create any starup pools that were previously-specified in
        /// the startupPools array
        /// </summary>
        public static void CreateStartupPools()
        {
            if (!I.startupPoolsCreated)
            {
                I.startupPoolsCreated = true;
                var pools = I.startupPools;
                if (pools != null && pools.Length > 0)
                    for (int i = 0; i < pools.Length; ++i)
                        CreatePool(pools[i].prefab, pools[i].initialElements, pools[i].maxElements, pools[i].allowUnpooled);
            }
        }

        public static void CreatePool<T>(T prefab, int initialPoolSize = 0, int maxPoolSize = 0, bool allowUnpooled = true) where T : Component
        {
            CreatePool(prefab.gameObject, initialPoolSize, maxPoolSize, allowUnpooled);
        }

        public static void CreatePool(GameObject prefab, int initialPoolSize = 0, int maxPoolSize = 0, bool allowUnpooled = true)
        {
            if (maxPoolSize > 0 && initialPoolSize > maxPoolSize)
            {
                Dbg.Err("maxPoolSize must be > initialPoolSize or maxPoolsize must be 0!");
                return;
            }
            else if (prefab == null)
            {
                Dbg.Err("Prefab cannot be null!");
                return;
            }

            //See if a pool for this prefab type already exists
            if (!I.poolDescriptors.ContainsKey(prefab.GetInstanceID()))
            {
                //Create a new pool
                PoolDescriptor pd = new PoolDescriptor(prefab, prefab.name, initialPoolSize, maxPoolSize, allowUnpooled);

                //Add this new prefab pool to the pool list
                I.poolDescriptors.Add(prefab.GetInstanceID(), pd);
            }
        }

        #endregion

        #region Spawn Methods

        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab.gameObject, Vector3.zero, null, prefab.transform.rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Transform parent = null) where T : Component
        {
            return Spawn(prefab.gameObject, position, parent, prefab.transform.rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, position, null, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(T prefab, Vector3 position, Transform parent, Quaternion rotation) where T : Component
        {
            return Spawn(prefab.gameObject, position, parent, rotation).GetComponent<T>();
        }

        public static T Spawn<T>(GameObject prefab) where T : Component
        {
            return Spawn<T>(prefab, Vector3.zero, null, prefab.transform.rotation);
        }
        public static T Spawn<T>(GameObject prefab, Vector3 position, Transform parent = null) where T : Component
        {
            return Spawn<T>(prefab, position, parent, prefab.transform.rotation);
        }
        public static T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Spawn<T>(prefab, position, null, rotation).GetComponent<T>();
        }
        public static T Spawn<T>(GameObject prefab, Vector3 position, Transform parent, Quaternion rotation) where T : Component
        {
            if (prefab == null)
            {
                Dbg.Err("prefab cannot be null!");
                return null;
            }

            //Does the prefab have a component of type T attached to it??
            if (prefab.GetComponent<T>() == null)
            {
                Dbg.Err($"Could not find component of type: {typeof(T)} in the prefab: {prefab.name}");
                return null;
            }

            var gob = Spawn(prefab, position, parent, rotation);
            return gob.GetComponent<T>();
        }

        public static GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, Vector3.zero, null, prefab.transform.rotation);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Transform parent = null)
        {
            return Spawn(prefab, position, parent, prefab.transform.rotation);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, null, rotation);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Transform parent, Quaternion rotation)
        {
            PoolDescriptor pd;
            Transform trans;
            GameObject obj = null;

            if (prefab == null)
            {
                Dbg.Err("Can't spawn with a null prefab!");
                return null;
            }

            //BNC: this chunk of code will create a pool with the default
            //MAX_AUTO_POOLED size if a pool has not already been created
            //comment this out to disable it.
            if (!I.poolDescriptors.TryGetValue(prefab.GetInstanceID(), out pd))
            {
                pd = new PoolDescriptor(prefab, prefab.name, 0, MAX_AUTO_POOLED, true);
                I.poolDescriptors[prefab.GetInstanceID()] = pd;
            }

            obj = pd.Borrow();

            //Were we able to borrow a pooled object?
            if (obj != null)
            {
                trans = obj.transform;
                OrientObject(trans, parent, position, rotation);
                I.spawnedObjects.Add(obj.GetInstanceID(), pd);
            }
            return obj;
        }
        #endregion

        static public bool IsSpawned(GameObject gob) { return I.spawnedObjects.ContainsKey(gob.GetInstanceID()); }

        static bool IsInAPool(GameObject gob)
        {
            foreach (var kv in I.poolDescriptors)
            {
                if (kv.Value.IsPooled(gob)) { return true; }
            }
            return false;
        }

        static void OrientObject(Transform trans, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (trans as RectTransform == null)
            {
                trans.SetParent(parent, true);
                trans.position = position;
                trans.rotation = rotation;
            }
            else
            {
                //If it is a RectTransform, then the world Position should NOT stay
                trans.SetParent(parent, false);
                trans.localPosition = position;
                trans.localRotation = rotation;
            }
        }

        #region Recycle Methods
        public static void Recycle<T>(T obj) where T : Component
        {
            Recycle(obj.gameObject);
        }

        public static void Recycle<T>(params T[] objects) where T : Component
        {
            if (objects != null)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    Recycle(objects[i].gameObject);
                }
            }
        }

        public static void Recycle<T>(IEnumerable<T> objects) where T : Component
        {
            foreach (var item in objects)
            {
                Recycle(item.gameObject);
            }
        }

        /// <summary>
        /// Recycle the given object after the specified time has elapsed
        /// </summary>
        /// <param name="obj">The component attatched to the game object to recycle</param>
        /// <param name="time">Time in seconds to wait before recycling the gameObject</param>
        public static void Recycle<T>(T obj, float time) where T : Component
        {
            Recycle(obj.gameObject, time);
        }

        /// <summary>
        /// Recycle the given object after the specified time has elapsed
        /// </summary>
        /// <param name="gob">The game object to recycle</param>
        /// <param name="time">Time in seconds to wait before recycling the gameObject</param>
        public static void Recycle(GameObject gob, float time)
        {
            if (time <= 0)
                Recycle(gob);
            else
                I.recycleRoutines.Add(gob.GetInstanceID(), I.StartCoroutine(I.DoRecycleAfter(gob, time)));
        }

        public static void Recycle(GameObject gob)
        {
            PoolDescriptor descriptor;

            if (gob == null) return;

            Coroutine routine;
            if (I.recycleRoutines.TryGetValue(gob.GetInstanceID(), out routine))
            {
                I.StopCoroutine(routine);
                I.recycleRoutines.Remove(gob.GetInstanceID());
            }

            //See if this object is currently spawned and in a pool
            descriptor = GetDescriptorFromSpawned(gob);
            if (descriptor != null)
            {
                I.spawnedObjects.Remove(gob.GetInstanceID());
                descriptor.Return(gob);
            }
            //Otherwise, it may already have been recyceld
            //IF the object has a component that implements IPoolable, then we KNOW Recycle has been called on it twice if we get here!
            //OR if the object is already in one of our pools
            else if (!gob.activeSelf && (gob.GetComponentInChildren<IPoolable>() != null || IsInAPool(gob)))
            {
                Dbg.Err($"The object {gob.name} [{gob.GetInstanceID():X}] has already been returned to its pool!");
            }
            else
            {
                //The object was not part of a pool at all
                Dbg.Warn($"Destroyed object: {gob.name} [{gob.GetInstanceID():X}]");
                Destroy(gob);
            }
        }

        //static void Recycle(GameObject obj, GameObject prefab)
        //{
        //    PoolDescriptor pd;
        //    if (I.poolDescriptors.TryGetValue(prefab.GetInstanceID(), out pd))
        //    {
        //        pd.Return(obj);
        //    }
        //}

        IEnumerator DoRecycleAfter(GameObject gob, float timer)
        {
            while (gob != null && timer > 0 && gob.activeSelf)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            Recycle(gob);
        }

        /// <summary>
        /// Recycles all the children of the gameobject.
        /// Note: References to the recyled objects will still be valid even
        /// when they are in the pool so make sure to account for this
        /// </summary>
        /// <param name="gob"></param>
        public static void RecycleChildren(GameObject gob)
        {
            if (gob == null) return;
            for (int i = gob.transform.childCount - 1; i >= 0; i--)
            {
                Recycle(gob.transform.GetChild(i));
            }
        }

        #endregion

        public static PoolDescriptor GetDescriptorFromSpawned(GameObject gob)
        {
            PoolDescriptor pd;
            I.spawnedObjects.TryGetValue(gob.GetInstanceID(), out pd);
            return pd;
        }

        public static int CountPooled<T>(T prefab) where T : Component
        {
            return CountPooled(prefab.gameObject);
        }

        public static int CountPooled(GameObject prefab)
        {
            PoolDescriptor pd;
            if (I.poolDescriptors.TryGetValue(prefab.GetInstanceID(), out pd))
                return pd.Available;
            return 0;
        }

        public static int CountSpawned<T>(T prefab) where T : Component
        {
            return CountSpawned(prefab.gameObject);
        }

        public static int CountSpawned(GameObject prefab)
        {
            PoolDescriptor pd;
            if (I.poolDescriptors.TryGetValue(prefab.GetInstanceID(), out pd))
            {
                return pd.Borrowed;
            }
            else return 0;
        }

        public static void DestroyPooled<T>(T prefab) where T : Component
        {
            DestroyPooled(prefab.gameObject);
        }

        public static void DestroyPooled(GameObject prefab)
        {
            PoolDescriptor pd;
            if (I.poolDescriptors.TryGetValue(prefab.GetInstanceID(), out pd))
            {
                pd.DestroyPool();
            }
        }
    }
}