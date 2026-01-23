using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace UniFramework.Runtime
{
    public delegate void PlaySoundHandler(int serialId, AudioSource source, object userData);

    [DisallowMultipleComponent]
    public class AudioManager : UniFrameworkModule<AudioManager>
    {
        [SerializeField]
        private AudioManagerSettings m_Settings;

        public event PlaySoundHandler PlaySoundSuccess;
        public event PlaySoundHandler PlaySoundFailed;

        private Dictionary<int, AudioSource> m_PlayingAudioSources;
        private Dictionary<int, float> m_PauseResumeVolumes;
        private Dictionary<int, float> m_OriginalVolumes;
        private IObjectPool<AudioSource> m_AudioSourceItemPool;
        private int m_Serial;

        protected override void OnInit()
        {
            if (m_Settings == null)
            {
                m_Settings = ScriptableObject.CreateInstance<AudioManagerSettings>();
            }

            InjectSettings(m_Settings, true);
            base.OnInit();
        }

        protected override void OnDispose()
        {
            m_AudioSourceItemPool.Clear();
            m_PlayingAudioSources.Clear();
            m_PauseResumeVolumes.Clear();
            m_OriginalVolumes.Clear();
            base.OnDispose();
        }

        public void InjectSettings(AudioManagerSettings settings, bool recreatePool = false)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }

            m_Settings = settings;
            if (!recreatePool)
            {
                return;
            }

            StopAllSound();
            if (m_AudioSourceItemPool != null)
            {
                m_AudioSourceItemPool.Clear();
            }

            m_AudioSourceItemPool = new ObjectPool<AudioSource>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, m_Settings.DefaultCapacity, m_Settings.MaxSize);
            m_PlayingAudioSources = new Dictionary<int, AudioSource>(m_Settings.DefaultCapacity);
            m_OriginalVolumes = new Dictionary<int, float>(m_Settings.DefaultCapacity);
            m_PauseResumeVolumes = new Dictionary<int, float>(m_Settings.DefaultCapacity);
        }

        public int PlaySound(string soundAssetName, string soundGroup, PlaySoundParams playSoundParams, object userData)
        {
            int serialId = ++m_Serial;
            StartCoroutine(PlaySoundInternal(serialId, soundAssetName, soundGroup, playSoundParams, userData));
            return serialId;
        }

        public void StopSound(int serialId, float fadeOutSeconds = 0)
        {
            if (m_PlayingAudioSources.TryGetValue(serialId, out AudioSource audioSource))
            {
                StartCoroutine(StopCo(audioSource, fadeOutSeconds));
            }

            m_OriginalVolumes.Remove(serialId);

            return;
            IEnumerator StopCo(AudioSource audioSource, float fadeOutSeconds)
            {
                yield return FadeToVolume(audioSource, 0f, fadeOutSeconds);
                audioSource.Stop();
                Recycle(serialId);
            }
        }

        public void PauseSound(int serialId, float fadeOutSeconds = 0)
        {
            if (m_PlayingAudioSources.TryGetValue(serialId, out AudioSource audioSource))
            {
                StartCoroutine(PauseCo(serialId, audioSource, fadeOutSeconds));
            }

            return;
            IEnumerator PauseCo(int serialId, AudioSource audioSource, float fadeOutSeconds)
            {
                if (!m_PauseResumeVolumes.ContainsKey(serialId))
                {
                    if (!m_OriginalVolumes.TryGetValue(serialId, out float originalVolume))
                    {
                        originalVolume = audioSource.volume;
                    }

                    m_PauseResumeVolumes[serialId] = originalVolume;
                }

                yield return FadeToVolume(audioSource, 0f, fadeOutSeconds);
                if (m_PauseResumeVolumes.ContainsKey(serialId))
                {
                    audioSource.Pause();
                }
            }
        }

        public void ResumeSound(int serialId, float fadeInSeconds = 0)
        {
            if (m_PlayingAudioSources.TryGetValue(serialId, out AudioSource audioSource))
            {
                StartCoroutine(ResumeCo(serialId, audioSource, fadeInSeconds));
            }

            return;
            IEnumerator ResumeCo(int serialId, AudioSource audioSource, float fadeInSeconds)
            {
                if (!m_PauseResumeVolumes.TryGetValue(serialId, out float originalVolume))
                {
                    yield break;
                }

                audioSource.UnPause();
                m_PauseResumeVolumes.Remove(serialId);
                yield return FadeToVolume(audioSource, originalVolume, fadeInSeconds);
            }
        }

        public void StopAllSound()
        {
            if (m_PlayingAudioSources == null)
            {
                return;
            }

            var serialIds = new List<int>(m_PlayingAudioSources.Count);
            foreach (var key in m_PlayingAudioSources.Keys)
            {
                serialIds.Add(key);
            }

            foreach (int serialId in serialIds)
            {
                Recycle(serialId);
            }
        }

        private IEnumerator PlaySoundInternal(int serialId, string soundAssetName, string soundGroup, PlaySoundParams playSoundParams, object userData)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(soundAssetName);
            yield return handle;
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                AudioClip audioClip = handle.Result;
                AudioSource audioSource = m_AudioSourceItemPool.Get();
                audioSource.name = $"[{soundGroup} #{serialId}] - {audioClip.name}";
                audioSource.outputAudioMixerGroup = GetAudioMixerGroup(string.Format("{0}", soundGroup));
                audioSource.clip = audioClip;
                audioSource.volume = playSoundParams.Volume;
                audioSource.loop = playSoundParams.Loop;
                audioSource.spatialBlend = playSoundParams.SpatialBlend;
                audioSource.Play();

                if (!m_PlayingAudioSources.TryAdd(serialId, audioSource))
                {
                    Debug.LogError($"[{nameof(AudioManager)}] duplicate SerialId '{serialId}' detected when registering audio source.");
                }

                if (!m_OriginalVolumes.TryAdd(serialId, playSoundParams.Volume))
                {
                    Debug.LogError($"[{nameof(AudioManager)}] failed to cache original volume. SerialId already exists: {serialId}");
                }

                float fadeInSeconds = playSoundParams.FadeInSeconds;
                if (fadeInSeconds > 0f)
                {
                    float volume = audioSource.volume;
                    audioSource.volume = 0f;
                    StartCoroutine(FadeToVolume(audioSource, volume, fadeInSeconds));
                }

                if (userData is PlaySoundInfo playSoundInfo)
                {
                    PlaySoundSuccess?.Invoke(serialId, audioSource, playSoundInfo.UserData);
                    StartCoroutine(SoundFollow(audioSource, playSoundInfo));
                }
                else
                {
                    PlaySoundSuccess?.Invoke(serialId, audioSource, userData);
                }

                if (!playSoundParams.Loop)
                {
                    StartCoroutine(AutoRecycle(audioSource));
                }
            }
            else
            {
                Debug.LogError($"[SoundManager] Failed to load audioClip: {soundAssetName}");
                PlaySoundFailed?.Invoke(serialId, null, userData);
                yield break;
            }

            yield break;
            IEnumerator AutoRecycle(AudioSource source)
            {
                yield return new WaitForSeconds(source.clip.length);
                Recycle(serialId);
            }

            IEnumerator SoundFollow(AudioSource audioSource, PlaySoundInfo playSoundInfo)
            {
                audioSource.transform.position = playSoundInfo.WorldPosition;
                while (audioSource.isPlaying)
                {
                    if (playSoundInfo.BindingTransform)
                    {
                        audioSource.transform.position = playSoundInfo.BindingTransform.position;
                    }
                    else
                    {
                        break;
                    }

                    yield return null;
                }
            }
        }

        private AudioMixerGroup GetAudioMixerGroup(string groupName)
        {
            if (m_Settings.AudioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = m_Settings.AudioMixer.FindMatchingGroups(string.Format("Master/{0}", groupName));
                if (audioMixerGroups.Length > 0)
                {
                    return audioMixerGroups[0];
                }
                else
                {
                    return m_Settings.AudioMixer.FindMatchingGroups("Master")[0];
                }
            }

            return null;
        }

        private IEnumerator FadeToVolume(AudioSource audioSource, float volume, float duration)
        {
            float time = 0f;
            float originalVolume = audioSource.volume;
            while (time < duration)
            {
                time += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(originalVolume, volume, time / duration);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = volume;
        }

        private void Recycle(int serialId)
        {
            if (m_PlayingAudioSources.TryGetValue(serialId, out AudioSource source))
            {
                Addressables.Release(source.clip);
                m_PlayingAudioSources.Remove(serialId);
                m_AudioSourceItemPool.Release(source);
            }
        }

        #region ObjectPool

        private AudioSource CreatePooledItem()
        {
            AudioSource audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.transform.SetParent(transform, false);
            return audioSource;
        }

        private void OnDestroyPoolObject(AudioSource audioSource)
        {
            Destroy(audioSource.gameObject);
        }

        private void OnReturnedToPool(AudioSource audioSource)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.transform.position = Vector3.zero;
            audioSource.transform.rotation = Quaternion.identity;
            audioSource.gameObject.SetActive(false);
        }

        private void OnTakeFromPool(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(true);
        }

        #endregion ObjectPool
    }
}