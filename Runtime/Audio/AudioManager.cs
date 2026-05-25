namespace UniFramework
{
    public static class AudioManager 
    {
        internal static AudioModule m_AudioModuleInstance;
        
        internal static AudioModule m_AudioModule
        {
            get
            {
                return ModuleProvider.GetModule(ref m_AudioModuleInstance, "AudioManager");
            }
        }

        public static void InjectSettings(AudioManagerSettings settings, bool recreatePool = false)
        {
            m_AudioModule.InjectSettings(settings, recreatePool);
        }

        public static int PlaySound(string soundAssetName, string soundGroup, PlaySoundParams playSoundParams, object userData)
        {
            return m_AudioModule.PlaySound(soundAssetName, soundGroup, playSoundParams, userData);
        }

        public static void StopSound(int serialId, float fadeOutSeconds = 0)
        {
            m_AudioModule.StopSound(serialId, fadeOutSeconds);
        }

        public static void PauseSound(int serialId, float fadeOutSeconds = 0)
        {
            m_AudioModule.PauseSound(serialId, fadeOutSeconds);
        }

        public static void ResumeSound(int serialId, float fadeInSeconds = 0)
        {
            m_AudioModule.ResumeSound(serialId, fadeInSeconds);
        }

        public static void StopAllSound()
        {
            m_AudioModule.StopAllSound();
        }
    }
}