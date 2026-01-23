namespace UniFramework.Runtime
{
    public interface IUniFrameworkModule
    {
        int Priority { get; }
        void Initialize();
        void Shutdown();
        void OnUpdate(float deltaTime);
    }

    public abstract class UniFrameworkModule<T> : MonoSingleton<T>, IUniFrameworkModule where T : MonoSingleton<T>
    {
        public virtual int Priority => 0;
        public virtual void Initialize() { }
        public virtual void Shutdown() { }
        public virtual void OnUpdate(float deltaTime) { }
    }
}