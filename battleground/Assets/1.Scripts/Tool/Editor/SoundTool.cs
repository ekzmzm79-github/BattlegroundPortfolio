using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using UnityObject = UnityEngine.Object;

/// <summary>
/// 각 사운드를 선택하고 수정, 복사 등을 하는 에디터툴 클래스
/// </summary>
public class SoundTool : EditorWindow
{
    #region Variable
    public int uiWidthLarge = 450;
    public int uiWidthMiddle = 300;
    public int uiWidthSmall = 200;

    private int selection = 0;
    private Vector2 SP1 = Vector2.zero;
    private Vector2 SP2 = Vector2.zero;

    private AudioClip soundSource = null; //사운드 클립
    private static SoundData soundData; //이펙트 데이터
    #endregion Variable

    #region Method
    [MenuItem("Tools/Sound Tool")]
    static void init()
    {
        soundData = ScriptableObject.CreateInstance<SoundData>();
        soundData.LoadData();

        //EffectTool 클래스를 생성하여 보여줌
        SoundTool window = GetWindow<SoundTool>(false, "Sound Tool");
        window.Show();
    }
    

    private void OnGUI()
    {
        if (soundData == null)
        {
            //Debug.LogError("SoundTool/OnGUI Error! soundData is null");
            return;
        }

        EditorGUILayout.BeginVertical();
        {
            UnityObject source = soundSource; //AudioClip -> UnityObject 박싱
            EditorHelper.EditorToolTopLayer(soundData, ref selection, ref source, this.uiWidthMiddle);
            soundSource = source as AudioClip; // UnityObject-> AudioClip 언박싱
            

            EditorGUILayout.BeginHorizontal();
            {
                // 중간 데이터 리스트 레이아웃
                EditorHelper.EditorToolListLayer(ref SP1, soundData, ref selection, ref source, this.uiWidthMiddle);
                SoundClip sound = soundData.soundClips[selection];
                soundSource = source as AudioClip;
                

                EditorGUILayout.BeginVertical();
                {
                    this.SP2 = EditorGUILayout.BeginScrollView(this.SP2);
                    {
                        if (soundData.GetDataCount() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                
                                EditorGUILayout.LabelField("ID", selection.ToString(), GUILayout.Width(uiWidthLarge));
                                soundData.names[selection] = EditorGUILayout.TextField("Name", soundData.names[selection], GUILayout.Width(uiWidthLarge));
                                sound.playType = (SoundPlayType)EditorGUILayout.EnumPopup("PlayType", sound.playType, GUILayout.Width(uiWidthLarge));
                                sound.maxVolume = EditorGUILayout.FloatField("Max Volume", sound.maxVolume, GUILayout.Width(uiWidthLarge));
                                sound.isLoop = EditorGUILayout.Toggle("LoopClip", sound.isLoop, GUILayout.Width(uiWidthLarge));
                                EditorGUILayout.Separator();

                                //리소스 보여주기
                                if (soundSource == null && sound.clipName != string.Empty)
                                {
                                    //full path 사용해볼것
                                    soundSource = Resources.Load(sound.clipPath + sound.clipName) as AudioClip;

                                   //sound.PreLoad();
                                }

                                this.soundSource = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", this.soundSource, typeof(AudioClip), false, GUILayout.Width(uiWidthLarge));
                                if (soundSource != null) //불러오기 성공 -> 경로와 이름 세팅
                                {
                                    sound.clipPath = EditorHelper.GetPath(soundSource);
                                    sound.clipName = soundSource.name;
                                    sound.pitch = EditorGUILayout.Slider("Pitch", sound.pitch, -3.0f, 3.0f, GUILayout.Width(uiWidthLarge));
                                    sound.dopplerLevel = EditorGUILayout.Slider("Doppler", sound.dopplerLevel, 0.0f, 5.0f, GUILayout.Width(uiWidthLarge));
                                    sound.rolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup("volume Rolloff", sound.rolloffMode, GUILayout.Width(uiWidthLarge));
                                    sound.minDistance = EditorGUILayout.FloatField("min Distance", sound.minDistance, GUILayout.Width(uiWidthLarge));
                                    sound.maxDistance = EditorGUILayout.FloatField("max Distance", sound.maxDistance, GUILayout.Width(uiWidthLarge));
                                    sound.spatialBlend = EditorGUILayout.Slider("PanLevel", sound.spatialBlend, 0.0f, 1.0f, GUILayout.Width(uiWidthLarge));

                                }
                                else //불러오기 실패
                                {
                                    sound.clipPath = string.Empty;
                                    sound.clipName = string.Empty;
                                    soundSource = null;
                                }
                                EditorGUILayout.Separator();

                                if(GUILayout.Button("Add Loop", GUILayout.Width(uiWidthMiddle)))
                                {
                                    sound.AddLoop();
                                    //soundData.soundClips[selection].AddLoop();
                                }
                                for (int i = 0; i < soundData.soundClips[selection].checkTime.Length; i++)
                                {
                                    EditorGUILayout.BeginVertical("box");
                                    {
                                        GUILayout.Label("Loop Step" + i, EditorStyles.boldLabel);
                                        if(GUILayout.Button("Remove", GUILayout.Width(uiWidthMiddle)))
                                        {
                                            sound.RemoveLoop(i);
                                            return;
                                        }
                                        sound = soundData.soundClips[selection];
                                        sound.checkTime[i] = EditorGUILayout.FloatField("check Time", sound.checkTime[i], GUILayout.Width(uiWidthMiddle));
                                        sound.setTime[i] = EditorGUILayout.FloatField("set Time", sound.setTime[i], GUILayout.Width(uiWidthMiddle));
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();


        EditorGUILayout.Separator();
        // 하단부
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Reload"))
            {
                soundData = CreateInstance<SoundData>();
                soundData.LoadData();
                selection = 0;
                this.soundSource = null;
            }
            if (GUILayout.Button("Save"))
            {
                soundData.SaveData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

        }
        EditorGUILayout.EndHorizontal();

    }

    /// <summary>
    /// data의 정보를 string으로 한줄씩 enumStructure로 만들고 각 리스트.cs로 생성
    /// </summary>
    public void CreateEnumStructure()
    {
        string enumName = "SoundList";
        StringBuilder builder = new StringBuilder();
        //builder.AppendLine();
        for (int i = 0; i < soundData.names.Length; i++)
        {
            if (!soundData.names[i].ToLower().Contains("none"))
            {
                builder.AppendLine($"     {soundData.names[i]} = {i},");
            }
        }
        EditorHelper.CreateEnumStructure(enumName, builder);
    }
    #endregion Method
}
