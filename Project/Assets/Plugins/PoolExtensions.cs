using UnityEngine;

namespace Pixelbyte.Extensions
{
    public static class ObjectPoolExtensions
    {
        #region Spawn Extensions

        public static T Spawn<T>(this T prefab) where T : Component
        {
            return Pool.Spawn(prefab, Vector3.zero, null, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab, Vector3 position, Transform parent = null) where T : Component
        {
            return Pool.Spawn(prefab, position, parent, Quaternion.identity);
        }
        public static T Spawn<T>(this T prefab, Vector3 position, Transform parent, Quaternion rotation) where T : Component
        {
            return Pool.Spawn(prefab, position, parent, rotation);
        }
        public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Pool.Spawn(prefab, position, null, rotation);
        }
        public static GameObject Spawn(this GameObject prefab)
        {
            return Pool.Spawn(prefab, Vector3.zero, null, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Transform parent = null)
        {
            return Pool.Spawn(prefab, position, parent, Quaternion.identity);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Pool.Spawn(prefab, position, null, rotation);
        }
        public static GameObject Spawn(this GameObject prefab, Vector3 position, Transform parent, Quaternion rotation)
        {
            return Pool.Spawn(prefab, position, parent, rotation);
        }

        #endregion

        #region Recycle Extensions

        public static void Recycle<T>(this T obj) where T : Component
        {
            Pool.Recycle(obj.gameObject);
        }

        public static void Recycle(this GameObject obj)
        {
            Pool.Recycle(obj);
        }

        public static void RecycleChildren(this GameObject parent)
        {
            if (parent == null) return;
            RecycleChildren(parent.transform);
        }

        public static void RecycleChildren(this Transform parent)
        {
            if (parent.childCount < 1) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                parent.GetChild(i).gameObject.Recycle();
            }
        }

        #endregion

        #region Pool Extensions

        public static void CreatePool<T>(this T prefab, int initialPoolSize = 0, int maxSize = 0, bool allowUnpooled = true) where T : Component
        {
            Pool.CreatePool(prefab.gameObject, initialPoolSize, maxSize, allowUnpooled);
        }

        public static void CreatePool(this GameObject prefab, int initialSize = 0, int maxSize = 0, bool allowUnpooled = true)
        {
            Pool.CreatePool(prefab, initialSize, maxSize, allowUnpooled);
        }

        public static int CountPooled<T>(this T prefab) where T : Component
        {
            return Pool.CountPooled(prefab);
        }

        public static int CountPooled(this GameObject prefab)
        {
            return Pool.CountPooled(prefab);
        }

        public static int CountSpawned<T>(this T prefab) where T : Component
        {
            return Pool.CountSpawned(prefab);
        }

        public static int CountSpawned(this GameObject prefab)
        {
            return Pool.CountSpawned(prefab);
        }

        public static void DestroyPooled(this GameObject prefab)
        {
            Pool.DestroyPooled(prefab);
        }
        public static void DestroyPooled<T>(this T prefab) where T : Component
        {
            Pool.DestroyPooled(prefab.gameObject);
        }

        #endregion
    }
}
