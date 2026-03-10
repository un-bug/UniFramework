using System;

namespace UniFramework.Runtime
{
    internal interface IUniFrameworkModule
    {
        int Priority { get; }
        Type Type { get; }
        void Initialize();
        void Shutdown();
        void OnUpdate(float deltaTime);
    }

    public abstract class UniFrameworkModule<T> : MonoSingleton<T>, IUniFrameworkModule where T : MonoSingleton<T>
    {
        public virtual int Priority
        {
            get
            {
                return 0;
            }
        }

        public virtual Type Type
        {
            get
            {
                return GetType();
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            UniFrameworkEntry.RegisterModule(this);
        }

        protected override void OnDispose()
        {
            UniFrameworkEntry.UnregisterModule(this);
            base.OnDispose();
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void Shutdown()
        {
        }

        public virtual void OnUpdate(float deltaTime)
        {
        }

        void IUniFrameworkModule.Initialize() => Initialize();
        void IUniFrameworkModule.Shutdown() => Shutdown();
        void IUniFrameworkModule.OnUpdate(float deltaTime) => OnUpdate(deltaTime);
    }
}