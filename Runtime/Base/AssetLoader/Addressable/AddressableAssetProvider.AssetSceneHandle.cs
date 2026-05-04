using System.Collections;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UniFramework
{
    public partial class AddressableAssetProvider
    {
        private sealed class AssetSceneHandle : IAssetSceneHandle
        {
            public string Key { get; }

            public SceneInstance SceneInstance { get; }

            public bool IsValid => SceneInstance.Scene.IsValid();

            public AssetSceneHandle(string key, SceneInstance sceneInstance)
            {
                Key = key;
                SceneInstance = sceneInstance;
            }

            public IEnumerator ActivateAsync()
            {
                if (!IsValid)
                {
                    yield break;
                }

                yield return SceneInstance.ActivateAsync();
            }
        }
    }
}