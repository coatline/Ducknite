namespace Pixelbyte
{
    /// BNC:
    /// Implement this interface on any MonoBehaviours you want to be notified 
    /// when the attached GameObject is recycled or spawned
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is respawned from a pool
        /// </summary>
        void OnReSpawned();

        /// <summary>
        /// called when the object is Pooled
        /// </summary>
        void OnPooled();
    } 
}
