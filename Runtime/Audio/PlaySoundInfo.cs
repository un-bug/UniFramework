using UnityEngine;

namespace UniFramework.Runtime
{
    public class PlaySoundInfo
    {
        public Transform BindingTransform { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public object UserData { get; private set; }

        public static PlaySoundInfo Create(Transform bindingTransform, Vector3 worldPosition, object userData)
        {
            PlaySoundInfo playSoundInfo = new PlaySoundInfo();
            playSoundInfo.BindingTransform = bindingTransform;
            playSoundInfo.WorldPosition = worldPosition;
            playSoundInfo.UserData = userData;
            return playSoundInfo;
        }
    }
}