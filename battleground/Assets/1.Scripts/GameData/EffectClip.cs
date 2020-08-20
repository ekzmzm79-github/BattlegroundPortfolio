using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이펙트 프리팹 경로와 타입등의 속성 데이터를 가지는 이펙트클립
/// 프리팹 사전로딩(풀링), 이펙트 인스턴스 기능을 가짐
/// </summary>

// 풀링 기능은 임의로 구현할 예정
public class EffectClip
{
    #region Variable
    public int realId = 0; // 각 클립을 구분하기 위한 id

    public EffectType effectType = EffectType.NORMAL;
    public GameObject effectPrefab = null;

    /* 리소스 경로명을 나타내는 변수는 반드시
     * 1. 해당 파일까지의 경로 string,
     * 2. 해당 파일의 이름 string 으로 나눠서 저장하는 것이 좋다.
     */
    public string effectName = "";
    public string effectPath = "";
    public string effectFullPath = "";
    #endregion Variable

    #region Constructor
    public EffectClip() { }

    #endregion Constructor

    #region Method
    public void PreLoad()
    {
        this.effectFullPath = effectPath + effectName;
        if(this.effectFullPath == "")
        {
            Debug.LogError("EffectClip/PreLoad effectFullPath is Empty!");
            return;
        }
        if(this.effectPrefab != null)
        {
            Debug.LogWarning("EffectClip/PreLoad effectPrefab is already set! effectPrefab = " + effectPrefab.name);
            return;
        }

        this.effectPrefab = ResourceManager.Load(effectFullPath) as GameObject;

    }
    public void ReleaseEffect()
    {
        if(this.effectPrefab != null)
        {
            this.effectPrefab = null; //가비지 컬렉터가 알아서 수거함
        }
    }
    /// <summary>
    /// Pos로 전달된 지점에서 이펙트를 인스턴스.
    /// </summary>
    public GameObject Instantiate(Vector3 Pos)
    {
        if(this.effectPrefab == null)
        {
            this.PreLoad();
            if (this.effectPrefab == null)
                return null;
        }

        GameObject effetct = GameObject.Instantiate(effectPrefab, Pos, Quaternion.identity);
        return effetct;
    }

    #endregion Method

}
