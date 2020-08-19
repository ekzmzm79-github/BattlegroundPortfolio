using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//데이터 편집 유니티 툴 만들기

/// <summary>
/// 사용되는 모든 data의 기본 클래스
/// ㅇ데이터의 개수와 ㅇ목록 리스트를 얻는 메소드 제공
/// </summary>

//ScriptableObject 
//대량의 데이터를 저장하는 데 사용할 수 있는 데이터 컨테이너
//값의 사본이 생성되는 것을 방지
public class BaseData : ScriptableObject 
{
    #region Variable
    public const string dataDirectory = "/9.ResourcesData/Resources/Data/";
    public string[] names = null;

    #endregion Variable

    #region Constructor
    public BaseData() { }

    #endregion Constructor


    #region Method
    /// <summary>
    /// 데이터의 개수를 리턴
    /// </summary>
    public int GetDataCount()
    {
        int retValue = 0;

        /*
         * Length : array object의 속성이며 가장 효과적으로 배열 요소 개수를 알려준다.
         * Count : 모든 enumerable objects에서 사용가능한 LINQ 확장 일반화 메소드(템플릿)아기 때문에 비교적 느리다.
         */
        if (this.names == null)
        {
            Debug.LogError("BaseData/GetDataCount Error! Data has null.");
            return retValue;
        }

        if (this.names.Length == 0)
        {
            Debug.LogWarning("BaseData/GetDataCount Warning! Data has zero count.");

        }

        retValue = this.names.Length;
        return retValue;
    }

    /// <summary>
    /// 툴에 출력하기 위한 이름 목록을 리턴.
    /// </summary>
    public string[] GetNameList(bool showID, string filterWord = "")
    {
        string[] retList = new string[0];

        if (this.names == null)
        {
            Debug.LogError("BaseData/GetNameList Error! Data has null.");
            return retList;
        }

        if (this.names.Length == 0)
        {
            Debug.LogWarning("BaseData/GetNameList Warning! Data has zero count.");
        }

        retList = new string[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            if (filterWord != "")
            {
                // 이름을 소문자로 바꾼뒤에 필터 워드와 일치하는 단어를 포함하지 않는다면
                // retList에 포함시키지 않는다.
                if (names[i].ToLower().Contains(filterWord.ToLower()) == false)
                {

                    continue;
                }
            }

            if (showID)
            {
                //showID가 트루라면 해당 데이터의 인덱스를 포함시킨다.
                retList[i] = i.ToString() + " : " + this.names[i];
            }
            else
            {
                retList[i] = this.names[i];
            }

        }

        return retList;
    }

    /// <summary>
    /// newName이름의 데이터를 추가하는 메소드
    /// </summary>
    /// <returns>데이터가 추가된 이후 데이터의 개수</returns>
    public virtual int AddData(string newName)
    {
        return GetDataCount();
    }

    /// <summary>
    /// index에 해당하는 데이터를 삭제
    /// </summary>
    public virtual void RemoveData(int index)
    {

    }

    /// <summary>
    /// index에 해당하는 데이터를 복제
    /// </summary>
    public virtual void Copy(int index)
    {

    }
    #endregion Method
}
