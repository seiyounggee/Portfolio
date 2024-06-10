using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoSingleton<SoundManager>
{
    public List<SoundClipInfo> soundList = new List<SoundClipInfo>();

    private List<SoundClipInfo> bgmPlayList = new List<SoundClipInfo>();

    [Serializable]
    public class SoundClipInfo
    {
        public SoundClip clipType;
        public AudioClip clip;
        public bool loopSound = false;
        [Range(0f, 1f)] public float volume = 0.5f;
        [Range(-3f, 3f)] public float pitch = 1f;
        [ReadOnly] public AudioSource audioSource = null;
    }

    public enum SoundClip
    {
        None = 0,

        #region BGM
        BGM_OutGame_01,        
        BGM_InGame_01,
        #endregion

        #region Ingame
        Ingame_AttackSuccess,
        Ingame_Start,
        Ingame_Win,
        Ingame_Lose,

        Ingame_PlayerDeath,
        Ingame_PlayerHit,
        #endregion

        #region Outgame

        Outgame_MatchMakingLoop,
        Outgame_MatchMakingFinished,

        #endregion

        #region Common
        Common_UIClick_Open,
        Common_UIClick_Close,
        #endregion
    }

    private void Start()
    {
        SetAudioClip();
    }

    public void SetAudioClip()
    {
        for (int i = 0; i < soundList.Count; i++)
        {
            if (soundList[i].clip == null)
                continue;

            var go = new GameObject();
            go.transform.SetParent(this.transform);
            go.AddComponent<AudioSource>();
            var _audioSource = go.GetComponent<AudioSource>();
            if (_audioSource != null)
            {
                soundList[i].audioSource = _audioSource;
                _audioSource.clip = soundList[i].clip;
                _audioSource.playOnAwake = false;
                _audioSource.loop = soundList[i].loopSound;
                _audioSource.volume = Math.Clamp(soundList[i].volume, 0f, 1f);
                _audioSource.pitch = Math.Clamp(soundList[i].pitch, -3f, 3f);

                if (_audioSource.clip != null)
                {
                    go.name = "[AudioClip] " + _audioSource.clip.name;
                }
            }
        }
    }
    public enum PlaySoundType { FadeIn, Immediate, Delay }
    public void PlaySound(SoundClip clip, PlaySoundType type = PlaySoundType.Immediate, float delayTime = 0f)
    {
        if (PlayerPrefsManager.Instance.GetSettingsSFX_IsOn() == false)
            return;

        var sc = soundList.Find(x => x.clipType.Equals(clip));

        if (sc != null && sc.audioSource != null && sc.clip != null)
        {
            if (type == PlaySoundType.Immediate)
            {
                sc.audioSource.Stop();
                sc.audioSource.Play();
            }
            else if (type == PlaySoundType.Delay)
            {
                UtilityInvoker.Invoke(this, () =>
                {
                    sc.audioSource.Stop();
                    sc.audioSource.Play();
                }, delayTime);
            }
            else if (type == PlaySoundType.FadeIn)
            {
                StartCoroutine(FadeInSound(sc));
            }
        }
    }


    public void PlaySound_BGM(SoundClip clip, PlaySoundType type = PlaySoundType.Immediate, float delayTime = 0f)
    {
        if (PlayerPrefsManager.Instance.GetMusicSFX_IsOn() == false)
            return;

        foreach (var i in bgmPlayList)
        {
            if (i.audioSource != null)
                i.audioSource.Stop();
        }
        bgmPlayList.Clear();

        var sc = soundList.Find(x => x.clipType.Equals(clip));

        if (sc != null && sc.audioSource != null)
        {
            if (type == PlaySoundType.Immediate)
            {
                sc.audioSource.Stop();
                sc.audioSource.Play();

                if (bgmPlayList.Contains(sc) == false)
                    bgmPlayList.Add(sc);
            }
            else if (type == PlaySoundType.Delay)
            {
                UtilityInvoker.Invoke(this, () =>
                {
                    sc.audioSource.Stop();
                    sc.audioSource.Play();
                }, delayTime);

                if (bgmPlayList.Contains(sc) == false)
                    bgmPlayList.Add(sc);
            }
        }
        else
        {
#if UNITY_EDITOR || SERVERTYPE_DEV
            Debug.Log("<color=red>sound is null... </color>");
#endif
        }
    }

    public enum StopSoundType { FadeOut, Immediate }
    public void StopSound(SoundClip clip, StopSoundType type = StopSoundType.Immediate)
    {
        var sc = soundList.Find(x => x.clipType.Equals(clip));

        if (sc != null && sc.audioSource != null && sc.clip != null)
        {
            if (type == StopSoundType.Immediate)
                sc.audioSource.Stop();
            else if (type == StopSoundType.FadeOut)
            {
                StartCoroutine(FadeOutSound(sc));
            }
        }
    }

    //Fade Out 기능 미완성임! coroutine 여러번 호출 당할 경우 이상함
    private IEnumerator FadeOutSound(SoundClipInfo info, float fadeOutSpeed = 0.1f)
    {
        AudioSource audio = info.audioSource;

        if (audio.isPlaying == false)
            yield break;

        if (audio == null)
            yield break;

        if (fadeOutSpeed <= 0)
        {
            audio.Stop();
            yield break;
        }


        while (true)
        {
            audio.volume -= Time.deltaTime * fadeOutSpeed;

            if (audio.volume <= 0)
            {
                break;
            }

            yield return null;
        }

        audio.Stop();

        //원상 복귀
        audio.volume = info.volume;
        audio.pitch = info.pitch;
    }

    private IEnumerator FadeInSound(SoundClipInfo info, float fadeInSpeed = 0.02f)
    {
        AudioSource audio = info.audioSource;

        if (fadeInSpeed <= 0)
        {
            audio.volume = info.volume;
            audio.pitch = info.pitch;
            audio.Play();
            yield break;
        }

        audio.volume = 0f;
        audio.Play();

        while (true)
        {
            audio.volume += Time.deltaTime * fadeInSpeed;

            if (audio.volume >= info.volume)
            {
                break;
            }

            yield return null;
        }

        //원상 복귀
        audio.volume = info.volume;
        audio.pitch = info.pitch;
    }

    public void StopAllSoundPlaying(StopSoundType type = StopSoundType.Immediate)
    {
        foreach (var i in soundList)
        {
            if (bgmPlayList.Exists(x => x.clipType == i.clipType))
                continue;

            if (type == StopSoundType.Immediate)
            {
                if (i.audioSource != null && i.audioSource.isPlaying)
                    i.audioSource.Stop();
            }
            else if (type == StopSoundType.FadeOut)
            {
                StartCoroutine(FadeOutSound(i));
            }
        }
    }

    public void StopAllBgmPlaying(StopSoundType type = StopSoundType.Immediate)
    {
        foreach (var i in bgmPlayList)
        {
            if (type == StopSoundType.Immediate)
            {
                if (i.audioSource != null && i.audioSource.isPlaying)
                    i.audioSource.Stop();
            }
            else if (type == StopSoundType.FadeOut)
            {
                StartCoroutine(FadeOutSound(i));
            }
        }
    }


    public void SetSoundPitch(SoundClip clip, float pitch)
    {
        var sc = soundList.Find(x => x.clipType.Equals(clip));

        if (sc != null && sc.audioSource != null)
        {
            sc.audioSource.pitch = pitch;
        }

    }
}