using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

// 싱글톤으로 구성되는 리소스관리 매니저 스크립트
// 굳이 싱글톤으로 구성하지 않아도 될 정도로 간단한 기능만 포함한다면,
// 해당 메소드를 static으로 정의시켜서 사용하는 방법이 유용하다.

/// <summary>
/// Resources.Load를 매핑하는 클래스.
/// 이후에 어셋번들로 변경될 예정.
/// </summary>
public class ResourceManager
{
    public static UnityObject Load(string path)
    {
        //Resources.Load -> 추후에 Asset.Load로 변경 예정

        UnityObject retObject = Resources.Load(path);
        if(retObject == null)
        {
            //Debug.LogError("ResourceManager/Load Error! path = " + path);
            return null;
        }
        return retObject;
    }

    public static GameObject LoadAndInstantiate(string path)
    {
        UnityObject source = Load(path);
        if(source == null)
        {
            return null;
        }
        return GameObject.Instantiate(source) as GameObject;
    }
}
