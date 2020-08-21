using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 싱글톤, 원하는 위치에 이펙트 GameObject들을 생성하고 EffectManager 하위 자식으로서 관리
/// </summary>
public class EffectManager : SingletonMonobehaviour<EffectManager>
{
    #region Variable
    private Transform effectRoot = null;

    #endregion Variable

    #region Method
    // Start is called before the first frame update
    void Start()
    {
        if(effectRoot == null)
        {
            effectRoot = new GameObject("EffectRoot").transform;
            effectRoot.SetParent(transform); //EffectManager 밑으로 Effect들이 생성
        }
    }

    public GameObject EffectOneShot(int index, Vector3 position)
    {
        EffectClip clip = DataManager.EffectData().GetClip(index);
        GameObject effectInstance = clip.Instantiate(position);
        effectInstance.SetActive(true);
        return effectInstance;
    }

    #endregion Method
}
