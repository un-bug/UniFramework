namespace UniFramework
{
    public interface IPoolable
    {
        /// <summary>
        /// 当对象从池中获取时调用。
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 当对象被释放回池时调用。
        /// </summary>
        void OnDespawn();
    }
}