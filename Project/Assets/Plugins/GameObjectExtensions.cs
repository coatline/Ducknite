using UnityEngine;
using System.Collections;
using System;

//
// 2016 Pixelbyte Studios LLC
//
namespace Pixelbyte.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// This method gets an existing component of the given type or adds a new one
        /// if one does not already exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T val = obj.GetComponent<T>();
            if (val == null)
                val = obj.AddComponent<T>();
            return val;
        }

        public static GameObject FindChild(this GameObject gob, string name)
        {
            var trans = gob.transform.Find(name);
            if (trans == null) return null;
            else return trans.gameObject;
        }

        /// <summary>
        /// Sets the parent of the game obect to the given GameObject
        /// if the given GameObject is null, the parent is set to null
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parent"></param>
        public static void SetParent(this GameObject obj, GameObject parent, bool worldPositionStays = true)
        {
            if (parent == null)
                obj.transform.SetParent(null, worldPositionStays);
            else
                obj.transform.SetParent(parent.transform, worldPositionStays);
        }

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                obj.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Destroys all children of the GameObject
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyChildren(this GameObject obj)
        {
            if (obj == null) { Dbg.Warn("Gameobject was null! aborting"); return; }
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isEditor)
                    UnityEngine.Object.DestroyImmediate(obj.transform.GetChild(i).gameObject);
                else
                    UnityEngine.Object.Destroy(obj.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Call the given function on all the children that have a Component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gob"></param>
        /// <param name="function"></param>
        public static void MapChildren<T>(this GameObject gob, Action<T> function) where T : Component
        {
            for (int i = gob.transform.childCount - 1; i >= 0; i--)
            { 
                var gameObject = gob.transform.GetChild(i).gameObject;
                var comp = gameObject.GetComponent<T>();
                if (comp != null)
                    function(comp);
            }
        }

        public static void MapChildren(this GameObject gob, Action<GameObject> function) 
        {
            for (int i = gob.transform.childCount - 1; i >= 0; i--)
            {
                var gameObject = gob.transform.GetChild(i).gameObject;
                function(gameObject);
            }
        }
        //
        //These next couple of methods are helpers to get the top, bottom, left, and right
        //coordinates of a GameObject
        //
        public static float Left(this GameObject obj)
        {
            return obj.transform.position.x - obj.transform.localScale.x / 2;
        }

        public static float Right(this GameObject obj)
        {
            return obj.transform.position.x + obj.transform.localScale.x / 2;
        }

        public static float Top(this GameObject obj)
        {
            return obj.transform.position.y + obj.transform.localScale.y / 2;
        }

        public static float Bottom(this GameObject obj)
        {
            return obj.transform.position.y - obj.transform.localScale.y / 2;
        }
    }
}