using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoSingleton<SoundManager>
{
    public List<SoundClipInfo> soundList = new List<SoundClipInfo>();
    public List<SoundClipInfo> bgmPlayList = new List<SoundClipInfo>();

    [Header("Drive Information")]
    [ReadOnly] [Range(0, 100f)] public float player_speed = 0f;
    [ReadOnly] [Range(-3f, 3f)] public float drive_pitch = 0f;

    [Header("Drive Pitch Settings ")]
    [Range(-3f, 3f)] public float drive_MAX_pitch = 1.7f;
    [Range(-3f, 3f)] public float drive_MIN_pitch = 0.45f;


    [Serializable]
    public class SoundClipInfo
    {
        public SoundClip clipType;
        public AudioClip clip;
        public bool loopSound = false;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        [ReadOnly] public AudioSource audioSource = null;
    }

    public enum SoundClip
    { 
        None = 0,
        Booster_03,
        Drive,
        Hit,
        ChargePad,
        CountDown_Beep,
        Water,
        Ingame_BGM_01,
        Outgame_BGM,
        ShieldOn,
        ShieldOff,
        CountDown_Go,
        UI_Click,
        PassFinishLine,
        Victory,
        Ingame_BGM_02,
        Stun,
        MatchSuccess,
        Applause,
        Booster_02,
        LapCheckPoint,
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
                    go.name = CommonDefine.GetGoNameWithHeader("AudioClip") + "  " + _audioSource.clip.name;
                }
            }
        }
    }
    public enum PlaySoundType { FadeIn, Immediate , Delay}
    public void PlaySound(SoundClip clip, PlaySoundType type = PlaySoundType.Immediate, float delayTime = 0f)
    {
        var sc = soundList.Find(x => x.clipType.Equals(clip));

        if (sc != null && sc.audioSource != null)
        {
            if (type == PlaySoundType.Immediate)
            {
                sc.audioSource.Stop();
                sc.audioSource.Play();
            }
            else if (type == PlaySoundType.Delay)
            {
                this.Invoke(() =>
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
                this.Invoke(() =>
                {
                    sc.audioSource.Stop();
                    sc.audioSource.Play();
                }, delayTime);

                if (bgmPlayList.Contains(sc) == false)
                    bgmPlayList.Add(sc);
            }
        }
    }

    public enum StopSoundType { FadeOut, Immediate }
    public void StopSound(SoundClip clip, StopSoundType type = StopSoundType.Immediate)
    {
        var sc = soundList.Find(x => x.clipType.Equals(clip));

        if (sc != null && sc.audioSource != null)
        {
            if (type == StopSoundType.Immediate)
                sc.audioSource.Stop();
            else if (type == StopSoundType.FadeOut)
            {
                StartCoroutine(FadeOutSound(sc));
            }
        }
    }

    private IEnumerator FadeOutSound(SoundClipInfo info, float fadeOutSpeed = 0.02f)
    {
        AudioSource audio = info.audioSource;

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

        //¿ø»ó º¹±Í
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

        //¿ø»ó º¹±Í
        audio.volume = info.volume;
        audio.pitch = info.pitch;
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
