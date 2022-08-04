using UnityEngine;
namespace Pixelbyte
{
    /// <summary>
    /// This class contains Debug stuff with Conditional attributes so it can
    /// be turned on/off
    /// </summary>
    public static class Dbg
    {
        [System.Diagnostics.Conditional("DEBUG_INFO")]
        public static void Log(object msg, Object context = null)
        {
            Debug.Log(msg, context);
        }

        [System.Diagnostics.Conditional("DEBUG_WARN")]
        public static void Warn(object msg, Object context = null)
        {
            Debug.LogWarning(msg, context);
        }

        [System.Diagnostics.Conditional("DEBUG_ERR")]
        public static void Err(object msg, Object context = null)
        {
            Debug.LogError(msg, context);
        }

#if !UNITY_WINRT
        /// <summary>
        /// Prints the name of the method you are currently in
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG_INFO")]
        public static void MethodName()
        {
            //we need the previous stack frame since we want to print the name of that method not this one
            //Note this doe NOT currently work inside of an IEnumerator coroutine method!
            System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1);

            Debug.Log("Method: " + sf.GetMethod());
        }
#endif
    }
}