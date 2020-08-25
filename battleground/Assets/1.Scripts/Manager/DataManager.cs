using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 데이터를 담아두고 관리하는 데이터 홀더 기능을 담당
/// </summary>
public class DataManager : MonoBehaviour
{
    #region Variable
    private static EffectData effectData = null;
    private static SoundData soundData = null;
    #endregion Variable

    #region Method
    // Start is called before the first frame update
    void Start()
    {
        if (effectData == null)
        {
            effectData = ScriptableObject.CreateInstance<EffectData>();
            effectData.LoadData();
        }

        if (soundData == null)
        {
            soundData = ScriptableObject.CreateInstance<SoundData>();
            soundData.LoadData();
        }
    }

    public static EffectData EffectData()
    {
        if (effectData == null)
        {
            effectData = ScriptableObject.CreateInstance<EffectData>();
            effectData.LoadData();
        }

        return effectData;
    }

    public static SoundData SoundData()
    {
        if (soundData == null)
        {
            soundData = ScriptableObject.CreateInstance<SoundData>();
            soundData.LoadData();
        }

        return soundData;
    }

    #endregion Method


}
