using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.IO;

/// <summary>
/// 사운드 클립을 배열로 소지
/// 사운드 데이터를 저장하고 로드, 프리로딩 기능 포함
/// </summary>
public class SoundData : BaseData
{
    #region Variable
    //최대 개수가 명확해야하는 경우에 리스트보다 배열이 유용
    public SoundClip[] soundClips = new SoundClip[0];

    public string clipPath = "Sound/";
    private string xmlFilePath = "";
    private string xmlFileName = "soundData.xml";
    private string dataPath = "Data/soundData";
    //xml 구분자
    private const string SOUND = "sound"; //저장 키
    private const string CLIP = "clip"; //저장 키
    #endregion Variable

    #region Constructor
    private SoundData() { }

    #endregion Constructor

    #region Method

    public void SaveData()
    {
        using (XmlTextWriter xml = new XmlTextWriter(xmlFilePath + xmlFileName, System.Text.Encoding.Unicode))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement(SOUND);
            xml.WriteElementString("length", GetDataCount().ToString());
            xml.WriteWhitespace("\n");
            for (int i = 0; i < this.names.Length; i++)
            {
                SoundClip clip = this.soundClips[i];
                xml.WriteStartElement(CLIP);
                xml.WriteElementString("id", i.ToString());
                xml.WriteElementString("name", this.names[i]);
                xml.WriteElementString("loops", clip.checkTime.Length.ToString());
                xml.WriteElementString("maxvol", clip.maxVolume.ToString());
                xml.WriteElementString("pitch", clip.pitch.ToString());
                xml.WriteElementString("dopperlevel", clip.dopplerLevel.ToString());
                xml.WriteElementString("rolloffmode", clip.rolloffMode.ToString());
                xml.WriteElementString("mindistance", clip.minDistance.ToString());
                xml.WriteElementString("maxdistance", clip.maxDistance.ToString());
                xml.WriteElementString("sparialblend", clip.spartialBlend.ToString());
                if (clip.isLoop == true)
                {
                    xml.WriteElementString("loop", "true");
                }
                xml.WriteElementString("clippath", clip.clipPath);
                xml.WriteElementString("clipname", clip.clipName);
                xml.WriteElementString("checktimecount", clip.checkTime.Length.ToString());

                string str = "";
                foreach (float t in clip.checkTime)
                {
                    //시간 별로 '/' 문자로 구분되어서 저장
                    str += t.ToString() + "/";
                }
                xml.WriteElementString("checktime", str);

                str = "";
                foreach (float t in clip.setTime)
                {
                    //시간 별로 '/' 문자로 구분되어서 저장
                    str += t.ToString() + "/";
                }
                xml.WriteElementString("settime", str);
                xml.WriteElementString("type", clip.playType.ToString());

                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
    }

    public void LoadData()
    {
        this.xmlFilePath = Application.dataPath + dataDirectory;
        TextAsset asset = ResourceManager.Load(dataPath) as TextAsset;
        if (asset == null || asset.text == null)
        {
            this.AddData("NewSound");
            return;
        }
        using (XmlTextReader reader = new XmlTextReader(new StringReader(asset.text)))
        {

            int currentID = 0;
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        //xml 속성 값에 따라서 effectClips 세팅하는 작업
                        case "length":
                            int length = int.Parse(reader.ReadString());
                            this.names = new string[length];
                            this.soundClips = new SoundClip[length];
                            break;
                        case "clip":

                            break;
                        case "id":
                            currentID = int.Parse(reader.ReadString());
                            this.soundClips[currentID] = new SoundClip();
                            this.soundClips[currentID].realId = currentID;
                            break;
                        case "name":
                            this.names[currentID] = reader.ReadString();
                            break;
                        case "loops":
                            int count = int.Parse(reader.ReadString());
                            soundClips[currentID].checkTime = new float[count];
                            soundClips[currentID].setTime = new float[count];
                            break;
                        case "maxvol":
                            soundClips[currentID].maxVolume = float.Parse(reader.ReadString());
                            break;
                        case "pitch":
                            soundClips[currentID].pitch = float.Parse(reader.ReadString());
                            break;
                        case "dopperlevel":
                            soundClips[currentID].dopplerLevel = float.Parse(reader.ReadString());
                            break;
                        case "rolloffmode":
                            soundClips[currentID].rolloffMode = (AudioRolloffMode)Enum.Parse(typeof(AudioRolloffMode), reader.ReadString());
                            break;
                        case "mindistance":
                            soundClips[currentID].minDistance = float.Parse(reader.ReadString());
                            break;
                        case "maxdistance":
                            soundClips[currentID].maxDistance = float.Parse(reader.ReadString());
                            break;
                        case "sparialblend":
                            soundClips[currentID].spartialBlend = float.Parse(reader.ReadString());
                            break;
                        case "loop":
                            soundClips[currentID].isLoop = true;
                            break;
                        case "clippath":
                            soundClips[currentID].clipPath = reader.ReadString();
                            break;
                        case "checktimecount":
                            break;
                        case "checktime":
                            SetLoopTime(true, soundClips[currentID], reader.ReadString());
                            break;
                        case "settime":
                            SetLoopTime(false, soundClips[currentID], reader.ReadString());
                            break;
                        case "type":
                            soundClips[currentID].playType = (SoundPlayType)Enum.Parse(typeof(SoundPlayType), reader.ReadString());
                            break;
                    }
                }
            }
        }

        //프리로딩 테스트
        foreach(SoundClip clip in soundClips)
        {
            clip.PreLoad();
        }

    }

    void SetLoopTime(bool isCheck, SoundClip clip, string timeString)
    {
        string[] time = timeString.Split('/');
        for (int i = 0; i < time.Length; i++)
        {
            if(time[i] != string.Empty)
            {
                if (isCheck == true)
                {
                    clip.checkTime[i] = float.Parse(time[i]);
                }
                else
                {
                    clip.setTime[i] = float.Parse(time[i]);
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
            this.soundClips = new SoundClip[] { new SoundClip() };
        }
        else
        {
            this.names = ArrayHelper.Add(name, this.names);
            this.soundClips = ArrayHelper.Add(new SoundClip(), this.soundClips);
        }

        return GetDataCount();
    }

    /// <summary>
    /// 해당 index의 데이터를 지우는 메소드
    /// </summary>
    public override void RemoveData(int index)
    {
        this.names = ArrayHelper.Remove(index, this.names);
        if (this.names.Length == 0)
        {
            this.names = null;
        }
        this.soundClips = ArrayHelper.Remove(index, this.soundClips);
    }

    /// <summary>
    /// 해당 index의 EffectClip을 복사해서 반환
    /// </summary>
    public SoundClip GetCopy(int index)
    {
        if (index < 0 || index >= this.soundClips.Length)
        {
            Debug.LogError("SoundData/GetCopy Out of index! index = " + index);
            return null;
        }

        SoundClip original = this.soundClips[index];

        //deep copy
        SoundClip clip = new SoundClip();
        clip.realId = index;
        clip.clipPath = original.clipPath;
        clip.clipName = original.clipName;
        clip.maxVolume = original.maxVolume;
        clip.pitch = original.pitch;
        clip.dopplerLevel = original.dopplerLevel;
        clip.rolloffMode = original.rolloffMode;
        clip.minDistance = original.minDistance;
        clip.maxDistance = original.maxDistance;
        clip.spartialBlend = original.spartialBlend;
        clip.isLoop = original.isLoop;
        clip.checkTime = new float[original.checkTime.Length];
        clip.setTime = new float[original.setTime.Length];
        for (int i = 0; i < clip.checkTime.Length; i++)
        {
            clip.checkTime[i] = original.checkTime[i];
            clip.setTime[i] = clip.setTime[i];
        }
        clip.PreLoad();
        return clip;
    }


    /// <summary>
    /// 해당 index의 names, effectClips 요소를 복사해서 추가함
    /// (즉, 같은 배열에서 하나의 요소를 복사해서 추가시키는 메소드)
    /// </summary>
    public override void Copy(int index)
    {
        this.names = ArrayHelper.Add(this.names[index], this.names);
        this.soundClips = ArrayHelper.Add(GetCopy(index), this.soundClips);
    }

    #endregion Method



}
