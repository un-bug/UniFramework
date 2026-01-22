using UnityEngine;

namespace UniFramework.Runtime
{
    public sealed class BaseManager : UniFrameworkModule<BaseManager>
    {
        public override int Priority => int.MaxValue;

        private void Update()
        {
            UniFrameworkEntry.Update(Time.deltaTime);
        }

        protected override void OnDispose()
        {
            UniFrameworkEntry.Shutdown();
            base.OnDispose();
        }
    }
}