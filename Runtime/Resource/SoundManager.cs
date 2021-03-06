using System;
using UnityEngine;
using System.Collections.Generic;
using Debug = JJFramework.Runtime.Extension.Debug;

namespace JJFramework.Runtime.Resource
{
    public class SoundManager
    {
        private GameObject _soundManagerObject;
        
        private Func<string, string, AudioClip> _resourceLoader;

        private readonly List<AudioSource> _effectSource = new List<AudioSource>();
        private AudioSource _musicSource;

        private readonly Dictionary<string, AudioClip> _clipsDic = new Dictionary<string, AudioClip>();
        private int _currentIndex;
        private int _maxIndex;
        private string _assetBundleName;

        private float _bgmVolume = 1f;
        public float BGMVolume => _bgmVolume;
        
        private float _effectVolume = 1f;
        public float EffectVolume => _effectVolume;

        public void Init(Func<string, string, AudioClip> resourceLoader, int effectBuffer, string assetBundleName)
        {
            _resourceLoader = resourceLoader;

            _maxIndex = effectBuffer;

            _assetBundleName = assetBundleName;

            if (null == _soundManagerObject)
            {
                _soundManagerObject = new GameObject($"[{nameof(SoundManager)}]");
                GameObject.DontDestroyOnLoad(_soundManagerObject);
            }

            for (int listLoop = 0; listLoop < effectBuffer; ++listLoop)
            {
                var audioSource = _soundManagerObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                _effectSource.Add(audioSource);
            }

            _musicSource = _soundManagerObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
        }

        public void Cleanup()
        {
            StopAllEffect();
            StopBGM();
            
            _effectSource.Clear();

            _musicSource = null;
            
            _clipsDic.Clear();

            _resourceLoader = null;

            if (null != _soundManagerObject)
            {
                GameObject.Destroy(_soundManagerObject);
                _soundManagerObject = null;
            }
        }

        public void PreloadEffects(params string[] clipList)
        {
            if (clipList != null)
            {
                foreach (var clip in clipList)
                {
                    LoadClip(clip);
                }
            }
        }

        private bool LoadClip(string clip)
        {
            if (_clipsDic.ContainsKey(clip) == false)
            {
                var audio = _resourceLoader?.Invoke(_assetBundleName, clip);
                if (audio != null)
                {
                    _clipsDic.Add(clip, audio);
                }
                else
                {
                    Debug.LogError($"AudioClip이 없어요! - {clip}");
                    return false;
                }
            }

            return true;
        }

        public int PlaySingle(string clip, bool isLoop = false, float volume = 1f)
        {
            // NOTE(JJO): Load에 실패하면 재생하지 않는다.
            if (false == LoadClip(clip))
            {
                return -1;
            }
            
            return PlaySingle(_clipsDic[clip], isLoop, volume);
        }

        public int PlaySingle(AudioClip clip, bool isLoop = false, float volume = 1f)
        {
            _effectSource[_currentIndex].clip = clip;
            _effectSource[_currentIndex].loop = isLoop;
            _effectSource[_currentIndex].volume = _effectVolume * volume;
            _effectSource[_currentIndex].Play();

            var current = _currentIndex;
            ++_currentIndex;
            if (_currentIndex >= _maxIndex)
            {
                _currentIndex = 0;
            }

            return current;
        }

        public void PlayBGM(string clip, bool isLoop = true, float volume = 1f, float pitch = 1f)
        {
            // NOTE(JJO): Load에 실패하면 재생하지 않는다.
            if (false == LoadClip(clip))
            {
                return;
            }
            
            PlayBGM(_clipsDic[clip], isLoop, volume, pitch);
        }

        public void PlayBGM(AudioClip clip, bool isLoop = true, float volume = 1f, float pitch = 1f)
        {
            _musicSource.clip = clip;
            _musicSource.loop = isLoop;
            _musicSource.volume = _bgmVolume * volume;
            SetBGMPitch(pitch);
            _musicSource.Play();
        }

        public void SetBGMPitch(float pitch = 1f)
        {
            if (null != _musicSource)
            {
                _musicSource.pitch = pitch;
            }
        }

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = volume;
            _musicSource.volume = volume;
        }

        public void StopEffect(int index)
        {
            if (index < 0
                || index >= _effectSource.Count)
            {
                Debug.LogError($"Invalid index - {index}");
                return;
            }
            
            _effectSource[index].Stop();
        }

        public void StopAllEffect()
        {
            _effectSource.ForEach(x =>
            {
                if (ReferenceEquals(x, null) == false)
                {
                    x.Stop();
                }
            });
        }

        public void StopBGM()
        {
            if (ReferenceEquals(_musicSource, null) == false)
            {
                _musicSource.Stop();
            }
        }

        public void Stop(string clip)
        {
            _effectSource.FindAll(x => x.clip == _clipsDic[clip]).ForEach(x => x.Stop());
        }

        public void SetEffectVolume(float volume)
        {
            _effectVolume = volume;
            _effectSource.ForEach(d => d.volume = volume);
        }
    }
}
