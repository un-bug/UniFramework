using UnityEngine;
using UnityEngine.Audio;

namespace UniFramework.Runtime
{
    [CreateAssetMenu(fileName = "AudioManagerSettings", menuName = "UniFramework/Modules/AudioManagerSettings")]
    public class AudioManagerSettings : ScriptableObject
    {
        public AudioMixer AudioMixer = null;
        public int DefaultCapacity = 10;
        public int MaxSize = 100;
    }
}