using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.IO;

/// <summary>
/// 이펙트 클립 리스트와 이펙트 파일 이름, 경로를 가진다.
/// 파일을 읽고 쓰는 기능(xml)을 가짐.
/// </summary>
public class EffectData : BaseData
{
    #region Variable
    //최대 개수가 명확해야하는 경우에 리스트보다 배열이 유용
    public EffectClip[] effectClips = new EffectClip[0];

    public string clipPath = "Effects/";
    private string xmlFilePath = "";
    private string xmlFileName = "effectData.xml";
    private string dataPath = "Data/EffectData";
    //xml 구분자
    private const string EFFECT = "effect"; //저장 키
    private const string CLIP = "clip"; //저장 키

    #endregion Variable

    #region Constructor
    private EffectData() { }

    #endregion Constructor

    #region Method
    //읽기, 저장, 삭제, 클립을 얻고 복사하는 메소드 구현 예정


    /// <summary>
    /// this의 각 필드를 xml 외부 파일로 저장
    /// </summary>
    public void SaveData()
    {
        using (XmlTextWriter xml = new XmlTextWriter(xmlFilePath + xmlFileName, System.Text.Encoding.Unicode))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement(EFFECT);
            xml.WriteElementString("length", GetDataCount().ToString());
            for (int i = 0; i < this.names.Length; i++)
            {
                EffectClip clip = this.effectClips[i];
                xml.WriteStartElement(CLIP);
                xml.WriteElementString("id", i.ToString());
                xml.WriteElementString("name", this.names[i]);
                xml.WriteElementString("effectType", clip.effectType.ToString());
                xml.WriteElementString("effectName", clip.effectName);
                xml.WriteElementString("effectPath", clip.effectPath);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
    }


    /// <summary>
    /// xml 형식의 외부 데이터를 Load해서 this의 각 필드에 매핑
    /// </summary>
    public void LoadData()
    {
        Debug.Log($"xmlFilePath = {Application.dataPath} + {dataDirectory}");
        //폰에서 실행시와 데스크탑 실행시, Application.dataPath 서로 다르다
        this.xmlFilePath = Application.dataPath + dataDirectory;
        TextAsset asset = ResourceManager.Load(dataPath) as TextAsset;
        if (asset == null || asset.text == null)
        {
            this.AddData("NewEffect");
            return;
        }

        /*
        using문의 Scope에서 벗어나면 Dispose(c++에서 소멸자 개념)호출
        프로그래머가 따로 메모리 해제를 하지 않아도 된다.
        */
        using (XmlTextReader reader = new XmlTextReader(new StringReader(asset.text)))
        {
            int currentID = 0;
            while(reader.Read())
            {
                if(reader.IsStartElement())
                {
                    switch(reader.Name)
                    {
                        //xml 속성 값에 따라서 effectClips 세팅하는 작업
                        case "length":
                            int length = int.Parse(reader.ReadString());
                            this.names = new string[length];
                            this.effectClips = new EffectClip[length];
                            break;
                        case "id":
                            currentID = int.Parse(reader.ReadString());
                            this.effectClips[currentID] = new EffectClip();
                            this.effectClips[currentID].realId = currentID;
                            break;
                        case "name":
                            this.names[currentID] = reader.ReadString();
                            break;
                        case "effectType":
                            this.effectClips[currentID].effectType = (EffectType)
                                Enum.Parse(typeof(EffectType), reader.ReadString()); //reader.ReadString -> EffectType(enum)                               
                            break;
                        case "effectName":
                            this.effectClips[currentID].effectName = reader.ReadString();
                            break;
                        case "effectPath":
                            this.effectClips[currentID].effectPath = reader.ReadString();
                            break;
                    }
                }
            }
        }
    }
    

    /// <summary>
    /// newName 이름의 새로운 데이터를 추가하는 메소드
    /// names는 배열이기 때문에 현재 add될 데이터가 들어갈 끝 인덱스 찾기가 어렵다.
    /// 그렇기 때문에 ArrayHelper의 add를 통해 names를 foreach로 순회하고
    /// list tmp로 받아서 각각 추가한뒤, 가장 마지막에 새로 추가될 데이터를 추가하고 array로 바꿔서 반화
    /// </summary>
    /// <returns>데이터가 새로 추가된 뒤의 데이터 개수를 반환</returns>
    public override int AddData(string newName)
    {
        if (this.names == null)
        {
            this.names = new string[] { name }; //아무 이름
            this.effectClips = new EffectClip[] { new EffectClip() };
        }
        else
        {
            this.names = ArrayHelper.Add(name, this.names);
            this.effectClips = ArrayHelper.Add(new EffectClip(), this.effectClips);
        }

        return GetDataCount();
    }

    /// <summary>
    /// 해당 index의 데이터를 지우는 메소드
    /// </summary>
    public override void RemoveData(int index)
    {
        this.names = ArrayHelper.Remove(index, this.names);
        if(this.names.Length == 0)
        {
            this.names = null;
        }
        this.effectClips = ArrayHelper.Remove(index, this.effectClips);
    }

    /// <summary>
    /// effectClips, names를 비우는 메소드
    /// </summary>
    public void ClearData()
    {
        foreach(EffectClip clip in this.effectClips)
        {
            clip.ReleaseEffect();
        }
        this.effectClips = null;
        this.names = null;
    }

    /// <summary>
    /// 해당 index의 EffectClip을 복사해서 반환
    /// </summary>
    public EffectClip GetCopy(int index)
    {
        if(index <0 || index >=this.effectClips.Length)
        {
            Debug.LogError("EffectData/GetCopy Out of index! index = " + index);
            return null;
        }

        EffectClip original = this.effectClips[index];

        //deep copy
        EffectClip clip = new EffectClip();
        clip.effectFullPath = original.effectFullPath;
        clip.effectName = original.effectName;
        clip.effectType = original.effectType;
        clip.effectPath = original.effectPath;
        clip.realId = effectClips.Length;
        //clip.effectPrefab = original.effectPrefab;
        return clip;
    }

    /// <summary>
    /// 해당 index의 EffectClip을 PreLoad하여서 반환
    /// </summary>
    public EffectClip GetClip(int index)
    {
        if (index < 0 || index >= this.effectClips.Length)
        {
            Debug.LogError("EffectData/GetClip Out of index! index = " + index);
            return null;
        }

        effectClips[index].PreLoad();
        return effectClips[index];
    }

    /// <summary>
    /// 해당 index의 names, effectClips 요소를 복사해서 추가함
    /// (즉, 같은 배열에서 하나의 요소를 복사해서 추가시키는 메소드)
    /// </summary>
    public override void Copy(int index)
    {
        this.names = ArrayHelper.Add(this.names[index], this.names);
        this.effectClips = ArrayHelper.Add(GetCopy(index), this.effectClips);
    }

    #endregion Method
}
