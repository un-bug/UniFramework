using System.Collections;

namespace UniFramework
{
    public interface IAssetSceneHandle
    {
        string Key { get; }
        bool IsValid { get; }
        IEnumerator ActivateAsync();
    }
}