using System;
using System.Collections;

namespace UniFramework
{
    public interface IAssetOperation<out T> : IDisposable
    {
        bool IsDone { get; }
        float Progress { get; }
        T Result { get; }
        IEnumerator WaitForCompletion();
    }
}