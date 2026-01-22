namespace UniFramework.Runtime
{
    public interface IUniFrameworkModule
    {
        int Priority { get; }
        void OnUpdate(float deltaTime);
        void Shutdown();
    }

    public abstract class UniFrameworkModule<T> : MonoSingleton<T>, IUniFrameworkModule where T : MonoSingleton<T>
    {
        public virtual int Priority => 100;
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void Shutdown() { }
    }
}