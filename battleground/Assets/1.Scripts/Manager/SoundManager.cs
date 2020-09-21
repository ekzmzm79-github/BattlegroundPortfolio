using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// BGM, Effect, UI 사운드 설정값 초기화 하거나 PlayerPrefabs를 이용하여 세팅한다.
/// </summary>
public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    #region Variable
    public const string MasterGroupName = "Master";
    public const string EffectGropName = "Effect";
    public const string BGMGropName = "BGM";
    public const string UIGropName = "UI";
    public const string MixerName = "AudioMixer";
    public const string ContainerName = "SoundContainer";
    public const string FadeA = "FadeA";
    public const string FadeB = "FadeB";
    public const string UI = "UI";
    public const string EffectVolumeParam = "Volume_Effect";
    public const string BGMVolumeParam = "Volume_BGM";
    public const string UIVolumeParam = "Volume_UI";

    public enum MusicPlayingType //Fade에 사용 A->B, B->A
    {
        None = 0,
        SourceA = 1,
        SourceB = 2,
        AtoB = 3,
        BtoA = 4,
    }

    public AudioMixer mixer = null;
    public Transform audioRoot = null;
    public AudioSource fadeA_audio = null;
    public AudioSource fadeB_audio = null;
    public AudioSource[] effect_audios = null;
    public AudioSource UI_audio = null;

    public float[] effect_PlayStartTime = null;
    private int EffectChannelCount = 5;
    private MusicPlayingType currentPlayingType = MusicPlayingType.None;
    private bool isTicking = false;
    private SoundClip currentSound = null;
    private SoundClip lastSound = null;
    private float minVolume = -80.0f;
    private float maxVolume = 0.0f;


    #endregion Variable

    #region Method
    // Start is called before the first frame update
    void Start()
    {
        //여러 필드의 초기 세팅

        if (this.mixer == null)
        {
            //this.mixer = Resources.Load(MixerName) as AudioMixer;
            this.mixer = ResourceManager.Load(MixerName) as AudioMixer;
        }
        if(this.audioRoot == null)
        {
            audioRoot = new GameObject(ContainerName).transform;
            audioRoot.SetParent(transform);
            audioRoot.localPosition = Vector3.zero;
        }
        if (fadeA_audio == null)
        {
            GameObject fadeA = new GameObject(FadeA, typeof(AudioSource));
            fadeA.transform.SetParent(audioRoot);
            this.fadeA_audio = fadeA.GetComponent<AudioSource>();
            this.fadeA_audio.playOnAwake = false;
        }
        if(fadeB_audio == null)
        {
            GameObject fadeB = new GameObject(FadeB, typeof(AudioSource));
            fadeB.transform.SetParent(audioRoot);
            this.fadeB_audio = fadeB.GetComponent<AudioSource>();
            this.fadeB_audio.playOnAwake = false;
        }
        if(UI_audio == null)
        {
            GameObject ui = new GameObject(UI, typeof(AudioSource));
            ui.transform.SetParent(audioRoot);
            this.UI_audio = ui.GetComponent<AudioSource>();
            this.UI_audio.playOnAwake = false;
        }
        if(this.effect_audios == null || this.effect_audios.Length == 0)
        {
            this.effect_PlayStartTime = new float[EffectChannelCount];
            this.effect_audios = new AudioSource[EffectChannelCount];
            for(int i = 0; i<EffectChannelCount; i++)
            {
                effect_PlayStartTime[i] = 0.0f;
                GameObject effect = new GameObject("Effect" + i.ToString(), typeof(AudioSource));
                this.effect_audios[i] = effect.GetComponent<AudioSource>();
                this.effect_audios[i].playOnAwake = false;
            }
        }

        if (this.mixer != null)
        {
            this.fadeA_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(BGMGropName)[0];
            this.fadeB_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(BGMGropName)[0];
            this.UI_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(UIGropName)[0];
            for(int i = 0; i< this.effect_audios.Length; i++)
            {
                this.effect_audios[i].outputAudioMixerGroup = mixer.FindMatchingGroups(EffectGropName)[0];
            }
        }

        VolumeInit(); //모든 사운드 초기화

    }

    public void SetBGMVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio); // Mathf.Clamp01 0~1사이에서 값을 지정
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio); // minVolume <-> maxVolume 사이 currentRatio 비율
        this.mixer.SetFloat(BGMVolumeParam, volume);
        //PlayerPrefs : 실행 기기의 레지스트값으로 설정값 저장
        PlayerPrefs.SetFloat(BGMVolumeParam, volume);
    }

    public float GetBGMVolume()
    {
        if(PlayerPrefs.HasKey(BGMVolumeParam))
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(BGMVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }
    public void SetEffectVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(EffectVolumeParam, volume);
        PlayerPrefs.SetFloat(EffectVolumeParam, volume);
    }
    public float GetEffectVolume()
    {
        if (PlayerPrefs.HasKey(EffectVolumeParam))
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(EffectVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }

    public void SetUIVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(UIVolumeParam, volume);
        PlayerPrefs.SetFloat(UIVolumeParam, volume);
    }
    public float GetUIVolume()
    {
        if (PlayerPrefs.HasKey(UIVolumeParam))
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(UIVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }

    void VolumeInit()
    {
        if(this.mixer != null)
        {
            this.mixer.SetFloat(BGMVolumeParam, GetBGMVolume());
            this.mixer.SetFloat(EffectVolumeParam, GetEffectVolume());
            this.mixer.SetFloat(UIVolumeParam, GetUIVolume());

        }
    }

    /// <summary>
    /// source를 clip, volume으로 세팅
    /// </summary>
    void PlayAudioSource(AudioSource source, SoundClip clip, float volume)
    {
        if (source == null || clip == null)
        {
            Debug.LogError("SoundManager/PlayAudioSource source or clip is null");
            return;
        }

        source.Stop();
        source.clip = clip.GetClip();
        source.volume = volume;
        source.pitch = clip.pitch;
        source.dopplerLevel = clip.dopplerLevel;
        source.rolloffMode = clip.rolloffMode;
        source.minDistance = clip.minDistance;
        source.maxDistance = clip.maxDistance;
        source.spatialBlend = clip.spatialBlend;
        source.Play();
    }

    void PlayAudioSourceAtPoint(SoundClip clip, Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(clip.GetClip(), position, volume);
    }

    public bool IsPlaying()
    {
        return (int)this.currentPlayingType > 0;
    }

    public bool IsDifferentSound(SoundClip clip)
    {
        if (clip == null)
        {
            return false;
        }

        if(currentSound != null && currentSound.realId == clip.realId && 
            IsPlaying() && currentSound.isFadeOut == false)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private IEnumerator CheckProcess()
    {
        while (this.isTicking == true && IsPlaying())
        {
            yield return new WaitForSeconds(0.05f);

            if (this.currentSound.HasLoop())
            {
                if (currentPlayingType == MusicPlayingType.SourceA)
                {
                    currentSound.CheckLoop(fadeA_audio);
                }
                else if (currentPlayingType == MusicPlayingType.SourceB)
                {
                    currentSound.CheckLoop(fadeB_audio);
                }
                else if (currentPlayingType == MusicPlayingType.AtoB)
                {
                    this.lastSound.CheckLoop(fadeA_audio);
                    this.currentSound.CheckLoop(fadeB_audio);
                }
                else if (currentPlayingType == MusicPlayingType.BtoA)
                {
                    this.lastSound.CheckLoop(fadeB_audio);
                    this.currentSound.CheckLoop(fadeA_audio);
                }
            }
        }
    }
         
    public void DoCheck()
    {
        StartCoroutine(CheckProcess());
    }

    //Interpolate.EaseType easeType 보간 곡선
    /// <summary>
    /// clip을 주어지는 time과 ease로 FadeIn
    /// </summary>
    public void FadeIn(SoundClip clip, float time, Interpolate.EaseType ease)
    {
        if(this.IsDifferentSound(clip)) //다른 사운드가 맞다면
        {
            fadeA_audio.Stop();
            fadeB_audio.Stop();
            this.lastSound = this.currentSound;
            this.currentSound = clip;
            PlayAudioSource(fadeA_audio, currentSound, 0.0f);
            this.currentSound.FadeIn(time, ease);
            this.currentPlayingType = MusicPlayingType.SourceA;
            if (this.currentSound.HasLoop())
            {
                this.isTicking = true;
                DoCheck();
            }
        }
    }

    //사운드 클립을 모르고 index만 알때 사용하는 FadeIn
    public void FadeIn(int index, float time, Interpolate.EaseType ease)
    {
        this.FadeIn(DataManager.SoundData().GetCopy(index), time, ease);
    }

    public void FadeOut(float time, Interpolate.EaseType ease)
    {
        if (currentSound != null)
        {
            currentSound.FadeOut(time, ease);
        }
    }

    /// <summary>
    /// 매 프레임마다 현재 사운드 플레이 상태를 체크해서 currentPlayingType 결정
    /// </summary>
    void Update()
    {
        if(currentSound == null)
        {
            return;
        }

        switch(currentPlayingType)
        {
            case MusicPlayingType.SourceA:
                currentSound.DoFade(Time.deltaTime, fadeA_audio);
                break;
            case MusicPlayingType.SourceB:
                currentSound.DoFade(Time.deltaTime, fadeB_audio);
                break;
            case MusicPlayingType.AtoB:
                lastSound.DoFade(Time.deltaTime, fadeA_audio);
                currentSound.DoFade(Time.deltaTime, fadeB_audio);
                break;
            case MusicPlayingType.BtoA:
                lastSound.DoFade(Time.deltaTime, fadeB_audio);
                currentSound.DoFade(Time.deltaTime, fadeA_audio);
                break;
        }

        if(fadeA_audio.isPlaying == true && fadeB_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingType.SourceA;
        }
        else if(fadeA_audio.isPlaying == false && fadeB_audio.isPlaying == true)
        {
            this.currentPlayingType = MusicPlayingType.SourceB;
        }
        else if (fadeA_audio.isPlaying == false && fadeB_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingType.None;
        }
    }

    public void FadeTo(SoundClip clip, float time, Interpolate.EaseType ease)
    {
        if(currentPlayingType == MusicPlayingType.None)
        {
            FadeIn(clip, time, ease);
        }
        else if(this.IsDifferentSound(clip))
        {
            if(currentPlayingType == MusicPlayingType.AtoB)
            {
                fadeA_audio.Stop();
                currentPlayingType = MusicPlayingType.SourceB;
            }
            else if(currentPlayingType == MusicPlayingType.BtoA)
            {
                fadeB_audio.Stop();
                currentPlayingType = MusicPlayingType.SourceA;
            }

            lastSound = currentSound;
            currentSound = clip;
            lastSound.FadeOut(time, ease);
            currentSound.FadeIn(time, ease);


            //FadeTo A->B, B->A 사운드 교체
            if (currentPlayingType == MusicPlayingType.SourceA)
            {
                PlayAudioSource(fadeB_audio, currentSound, 0.0f);
                currentPlayingType = MusicPlayingType.AtoB;
            }
            else if(currentPlayingType == MusicPlayingType.SourceB)
            {
                PlayAudioSource(fadeA_audio, currentSound, 0.0f);
                currentPlayingType = MusicPlayingType.BtoA;
            }

            if(currentSound.HasLoop())
            {
                isTicking = true;
                DoCheck();
            }

        }
    }

    public void FadeTo(int index, float time, Interpolate.EaseType ease)
    {
        FadeTo(DataManager.SoundData().GetCopy(index), time, ease);
    }


    /// <summary>
    /// clip이 현재 진행중인 사운드가 아니라면 배경음악 교체
    /// </summary>
    public void PlayBGM(SoundClip clip)
    {
        if(IsDifferentSound(clip))
        {
            fadeB_audio.Stop();
            lastSound = currentSound;
            currentSound = clip;
            PlayAudioSource(fadeA_audio, clip, clip.maxVolume);
            if(currentSound.HasLoop())
            {
                isTicking = true;
                DoCheck();
            }
        }
    }

    public void PlayBGM(int index)
    {
        SoundClip clip = DataManager.SoundData().GetCopy(index);
        PlayBGM(clip);
    }

    public void PlayUISound(SoundClip clip)
    {
        PlayAudioSource(UI_audio, clip, clip.maxVolume);
    }

    public void PlayEffectSound(SoundClip clip)
    {
        //찢어지는 사운드 방지(특정 채널 갯수 이하로만 재생)
        bool isPlaySuccess = false;

        for (int i = 0; i < this.EffectChannelCount; i++)
        {
            if(this.effect_audios[i].isPlaying == false)
            {
                PlayAudioSource(this.effect_audios[i], clip, clip.maxVolume);
                effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
            else if(this.effect_audios[i].clip == clip.GetClip())
            {
                this.effect_audios[i].Stop();
                PlayAudioSource(this.effect_audios[i], clip, clip.maxVolume);
                effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
        }


        // 이미 EffectChannelCount만큼 이펙트 사운드가 진행중이라면,
        // 현잰 진행중인 EffectSound 중에서 가장 오래 진행된 것을 꺼버리고 새로운 clip 재생
        if (isPlaySuccess == false)
        {
            float maxTime = 0.0f;
            int selectIndex = 0;
            for (int i = 0; i < EffectChannelCount; i++) //최대값 찾기
            {
                if(effect_PlayStartTime[i] > maxTime)
                {
                    maxTime = effect_PlayStartTime[i];
                    selectIndex = i;
                }
            }

            PlayAudioSource(effect_audios[selectIndex], clip, clip.maxVolume);

        }

    }

    /// <summary>
    /// 3d 사운드 재생(PlayAudioSourceAtPoint)
    /// </summary>
    public void PlayEffectSound(SoundClip clip, Vector3 position, float volume)
    {
        //찢어지는 사운드 방지(특정 채널 갯수 이하로만 재생)
        bool isPlaySuccess = false;

        for (int i = 0; i < this.EffectChannelCount; i++)
        {
            if (this.effect_audios[i].isPlaying == false)
            {
                PlayAudioSourceAtPoint(clip, position, volume);
                effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
            else if (this.effect_audios[i].clip == clip.GetClip())
            {
                this.effect_audios[i].Stop();
                PlayAudioSourceAtPoint(clip, position, volume);
                effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
        }

        if (isPlaySuccess == false)
        {
            PlayAudioSourceAtPoint(clip, position, volume);

        }
    }

    public void PlayOneShotEffect(int index, Vector3 position, float volume)
    {
        if(index == (int)SoundList.None)
        {
            return;
        }

        SoundClip clip = DataManager.SoundData().GetCopy(index);
        if(clip == null)
        {
            return;
        }

        PlayEffectSound(clip, position, volume);
    }

    public void PlayOneShot(SoundClip clip)
    {
        if (clip == null)
        {
            return;
        }
        switch(clip.playType)
        {
            case SoundPlayType.EFFECT:
                PlayEffectSound(clip);
                break;
            case SoundPlayType.BGM:
                PlayBGM(clip);
                break;
            case SoundPlayType.UI:
                PlayUISound(clip);
                break;
        }
    }

    public void Stop(bool allStop = false)
    {
        if(allStop)
        {
            fadeA_audio.Stop();
            fadeB_audio.Stop();
        }

        FadeOut(0.5f, Interpolate.EaseType.Linear);
        currentPlayingType = MusicPlayingType.None;
        StopAllCoroutines();

    }

    /// <summary>
    /// Enemy의 클래스에 따라 사격 사운드를 교체
    /// </summary>
    public void PlayShotSound(string ClassID, Vector3 position, float volume)
    {
        //classID를 SoundList로 캐스팅하고(string to enum)
        //해당 sound를 PlayOneShotEffect 함수에 인자로 전달해서(enum to int) 재생
        SoundList sound = (SoundList)Enum.Parse(typeof(SoundList), ClassID.ToLower());
        PlayOneShotEffect((int)sound, position, volume);
    }


    #endregion Method
}
