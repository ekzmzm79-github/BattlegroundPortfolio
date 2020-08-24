using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BGM, UI, Effect
//루프구간, 페이트 관련 속성, 오디오 클립 속성

public class SoundClip
{
    #region Variable
    public SoundPlayType playType = SoundPlayType.None;
    public string clipName = string.Empty;
    public string clipPath = string.Empty;
    public float maxVolume = 1.0f;
    public bool isLoop = false;
    public float[] checkTime = new float[0];
    public float[] setTime = new float[0];
    public int realId = 0;

    private AudioClip clip = null;
    public int currentLoop = 0;
    public float pitch = 1.0f; //오디오 소스(audio source)의 음높이
    public float dopplerLevel = 1.0f; //도플러 효과(멀리서~가까이~멀리 사운드 효과) 수준
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 10000.0f;
    public float maxDistance = 50000.0f;
    public float spartialBlend = 1.0f;

    public float fadeTime1 = 0.0f;
    public float fadeTime2 = 0.0f;
    public Interpolate.Function interpolate_Func;
    public bool isFadeIn = false;
    public bool isFadeOut = false;

    #endregion Variable

    #region Constructor
    public SoundClip() { }
    public SoundClip(string clipPath, string clipName)
    {
        this.clipPath = clipPath;
        this.clipName = clipName;

    }

    #endregion Constructor

    #region Method
    public void PreLoad()
    {
        if (this.clip == null)
        {
            string fullPath = this.clipPath + this.clipName;
            this.clip = ResourceManager.Load(fullPath) as AudioClip;
        }
    }

    public void AddLoop()
    {
        //루프 배열의 길이가 0이 아니라면 루프가 존재한다는 의미
        this.checkTime = ArrayHelper.Add(0.0f, this.checkTime);
        this.setTime = ArrayHelper.Add(0.0f, this.setTime);
    }

    public void RemoveLoop(int index)
    {
        this.checkTime = ArrayHelper.Remove(index, this.checkTime);
        this.setTime = ArrayHelper.Remove(index, this.setTime);
    }

    public AudioClip GetClip()
    {
        if(this.clip == null)
        {
            if(this.clipName != string.Empty)
            {
                Debug.LogWarning($"Can't load audio clip Resource {this.clipName}");
                return null;
            }

            PreLoad();
        }

        return this.clip;
    }

    public void ReleaseClip()
    {
        if(this.clip != null)
        {
            this.clip = null;
        }
    }

    public bool HasLoop()
    {
        return this.checkTime.Length > 0;
    }
    public void NextLoop()
    {
        this.currentLoop++;
        if (this.currentLoop >= this.checkTime.Length)
        {
            this.currentLoop = 0;
        }
    }

    public void CheckLoop(AudioSource source)
    {
        /*
         * 현재 source의 재생 시간이 사전에 설정한 반복 구간 시간을 넘어가 같다면,
         * source의 재생 시간을 전에 설정한 setTime(시작부분)으로 설정해서 루프를 세팅
         * 
        */
        if (HasLoop() && source.time >= this.checkTime[this.currentLoop])
        {
            source.time = this.setTime[this.currentLoop];
            this.NextLoop();
        }
    }

    //Interpolate.EaseType easeType 보간 곡선

    public void FadeIn(float time, Interpolate.EaseType easeType)
    {
        this.isFadeOut = false;
        this.fadeTime1 = 0.0f;
        this.fadeTime2 = time;
        this.interpolate_Func = Interpolate.Ease(easeType);
        this.isFadeIn = true;
    }

    public void FadeOut(float time, Interpolate.EaseType easeType)
    {
        this.isFadeIn = false;
        this.fadeTime1 = 0.0f;
        this.fadeTime2 = time;
        this.interpolate_Func = Interpolate.Ease(easeType);
        this.isFadeOut = true;
    }

    /// <summary>
    /// 페이드 인, 아웃 효과 프로세스
    /// </summary>
    public void DoFade(float time, AudioSource audio)
    {
        //fadeTime1 : 축적되는 현재 재생시간
        //fadeTime2 : 끝나는 시간을 체크하는 끝 시간

        if (this.isFadeIn == true)
        {
            this.fadeTime1 += time;
            audio.volume = Interpolate.Ease(this.interpolate_Func, 0, maxVolume, fadeTime1, fadeTime2);
            if(this.fadeTime1 > this.fadeTime2)
            {
                this.isFadeIn = false;
            }
            
        }
        else if (this.isFadeOut == true)
        {
            this.fadeTime1 += time;
            audio.volume = Interpolate.Ease(this.interpolate_Func, maxVolume, 0 - maxVolume, fadeTime1, fadeTime2);
            if(this.fadeTime1 >= this.fadeTime2)
            {
                this.isFadeOut = false;
                audio.Stop();
            }
        }
    }

    #endregion Method

}
