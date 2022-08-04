using System.Collections.Generic;
using UnityEngine;

namespace Pixelbyte
{
    /// <summary>
    /// Describes each pool contained by the Pool class
    /// 2017 Pixelbyte Studios
    /// </summary>
    public sealed class PoolDescriptor
    {
        //The name of this pool
        string poolName;

        //The parent where these gameObjects are pooled
        Transform poolParent;

        //Max number of pooled objects this pool will 
        int maxPooledObjects;

        /// <summary>
        /// If true then when the pool is full and there are no slots availabel, the pool manager
        /// will create unpooled objects which will be destroyed as normal
        /// If false, the pool manager will not create the object and return null
        /// </summary>
        bool allowUnpooledObjects = true;

        /// <summary>
        /// If true, then the objects that go into this pool implement IPoolable
        /// </summary>
        bool implementsIPoolable;

        /// <summary>
        /// The prefab for which this pool exists
        /// </summary>
        GameObject prefab;

        /// <summary>
        /// Objects currently in this pool
        /// </summary>
        public List<GameObject> pool;

        int numBorrowed;

        /// <summary>
        /// Gets # of items that have been spawned from this PoolDescriptor
        /// and are expected to be returned
        /// </summary>
        public int Borrowed { get { return numBorrowed; } }

        /// <summary>
        /// Gets # of items currently in the pool
        /// </summary>
        public int Available { get { return pool.Count; } }

        public bool AllowUnpooled { get { return allowUnpooledObjects; } }

        /// <summary>
        /// Returns the maximum size of the pool (including borrowed items)
        /// if 0, then there is no maximum
        /// </summary>
        public int MaxPoolSize { get { return maxPooledObjects; } }

        /// <summary>
        /// True if the pool is full, false otherwise. Note if maxSize = 0 this will
        /// always be false
        /// </summary>
        public bool Full { get { return (maxPooledObjects - (Available + numBorrowed)) == 0; } }

        public string Name { get { return poolName; } }

        public PoolDescriptor(GameObject prefab, string name, int initialSize = 0, int max = 0, bool unpooledAllowed = true)
        {
            this.prefab = prefab;
            poolName = name;
            numBorrowed = 0;

            //Check if any of the objects implement IPoolable
            //If not, then we dont bother to check each time 
            //objects are pooled or borrowed
            var poolInterface = prefab.GetComponentsInChildren<IPoolable>(true);
            implementsIPoolable = (poolInterface != null && poolInterface.Length > 0);

            //If there is a desired initial size, then create these in the pool
            if (initialSize > 0)
            {
                if (max != 0 && max < initialSize)
                    max = initialSize;
                pool = new List<GameObject>(initialSize);
                CreatePoolParent();
                for (int i = 0; i < initialSize; i++)
                {
                    var obj = Object.Instantiate(prefab);
                    obj.SetActive(false);
                    obj.transform.parent = poolParent;
                    pool.Add(obj);
                }
            }
            else
                pool = new List<GameObject>();

            maxPooledObjects = max;
            allowUnpooledObjects = unpooledAllowed;
        }

        void CreatePoolParent()
        {
            if (poolParent != null) return;
            poolParent = new GameObject(poolName).transform;
            poolParent.SetParent(Pool.I.transform);
            poolParent.position = Vector3.zero;
        }

        public GameObject Borrow()
        {
            GameObject gob = null;

            while (pool.Count > 0 && gob == null)
            {
                gob = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                if (gob == null)
                {
                    Dbg.Warn($"A Pooled object from the {Name} pool was destroyed. That is a no-no");
                }
                else
                {
                    numBorrowed++;
                    gob.transform.SetParent(null, false);
                    gob.SetActive(true);
                    if (implementsIPoolable)
                        SendRespawnedMessage(gob);
                    return gob;
                }
            }

            if (numBorrowed + pool.Count < maxPooledObjects || maxPooledObjects == 0)
            {
                gob = Object.Instantiate(prefab);
                numBorrowed++;
                return gob;
            }
            else if (allowUnpooledObjects)
            {
                gob = Object.Instantiate(prefab);
                return gob;
            }
            else
                return null;
        }

        /// <summary>
        /// Returns a gameobject to the pool
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Return(GameObject obj)
        {
            if (obj == null) return false;

            CreatePoolParent();

            if (numBorrowed > 0)
            {
                obj.SetActive(false);
                obj.transform.SetParent(poolParent, false);

                //First, make sure it is not already in the pool
                if (!pool.Contains(obj))
                {
                    numBorrowed--;
                    //Put it back into the pool
                    pool.Add(obj);

                    if (implementsIPoolable)
                        SendPooledMessage(obj);
                    return true;
                }
                else
                {
                    Dbg.Warn($"The object: {obj.name} was already been pooled but was re-enabled. This is bad!");
                    return false;
                }
            }
            else
            {
                //Just dispose of it as normal
                GameObject.Destroy(obj);
                return false;
            }
        }

        public bool IsPooled(GameObject gob) { return pool.Contains(gob); }

        /// <summary>
        /// Destroys ALL pooled objects
        /// </summary>
        public void DestroyPool(bool includeSpawned = true)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                GameObject.Destroy(pool[i]);

            }
            pool.Clear();
        }

        //This is used to avoid a GC alloc when we look for components that implement IPoolable
        private static List<IPoolable> iPoolableCache = new List<IPoolable>(64);

        private static void SendPooledMessage(GameObject obj)
        {
            //Get all components on this object that have an IPoolable interface
            iPoolableCache.Clear();
            obj.GetComponentsInChildren<IPoolable>(true, iPoolableCache);
            if (iPoolableCache.Count > 0)
                for (int i = 0; i < iPoolableCache.Count; i++)
                    iPoolableCache[i].OnPooled();
        }

        static void SendRespawnedMessage(GameObject obj)
        {
            //Get all components on this object and its children that have an IPoolable interface
            iPoolableCache.Clear();
            obj.GetComponentsInChildren<IPoolable>(true, iPoolableCache);
            if (iPoolableCache.Count > 0)
                for (int i = 0; i < iPoolableCache.Count; i++)
                    iPoolableCache[i].OnReSpawned();
        }
    }
}
