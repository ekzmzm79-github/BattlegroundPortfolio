using UnityEngine;
using UnityEditor;
using System.Text;
using UnityObject = UnityEngine.Object;

/// <summary>
/// 각 이펙트를 이펙트에서 선택하고 수정, 복사 등을 하는 에디터툴 클래스
/// </summary>
public class EffectTool : EditorWindow
{
    #region Variable
    public int uiWidthLarge = 300;
    public int uiWidthMiddle = 200;
    private int selection = 0;
    private Vector2 SP1 = Vector2.zero;
    private Vector2 SP2 = Vector2.zero;

    private GameObject effectSource = null; //이펙트 클립
    private static EffectData effectData; //이펙트 데이터
    #endregion Variable

    #region Method
    [MenuItem("Tools/Effect Tool")] // 유니티 상단에 Tools 버튼과 클릭시 Effect Tool버튼이 생김
    static void init()
    {
        effectData = ScriptableObject.CreateInstance<EffectData>();
        effectData.LoadData();

        //EffectTool 클래스를 생성하여 보여줌
        EffectTool window = GetWindow<EffectTool>(false, "Effect Tool");
        window.Show();
    }

    private void OnGUI()
    {
        if(effectData == null)
        {
            Debug.LogError("EffectTool/OnGUI Error! effectData is null");
            return;
        }

        EditorGUILayout.BeginVertical();// EffectTool 레이아웃 전체를 수직적으로 구성
        {
            UnityObject source = effectSource; //GameObject -> UnityObject 박싱
            /* 
             * 가장 상단에 EditorToolTopLayer
             * source는 게임 오브젝트, 오디오 클립, 애니메이션 클립 등이 될 수 있으므로
             * UnityObject 형식으로 받아서 작업한 뒤,
             * 각 메소드마다 자신이 사용할 형식으로 다시 언박싱한다.(여기서는 GameObject)
            */
            EditorHelper.EditorToolTopLayer(effectData, ref selection, ref source, this.uiWidthMiddle);
            effectSource = source as GameObject; // UnityObject-> GameObject 언박싱

            
            EditorGUILayout.BeginHorizontal();
            {
                // 중간 데이터 리스트 레이아웃
                EditorHelper.EditorToolListLayer(ref SP1, effectData, ref selection, ref source, this.uiWidthLarge);
                effectSource = source as GameObject;

                //설정 부분
                EditorGUILayout.BeginVertical();
                {
                    SP2 = EditorGUILayout.BeginScrollView(this.SP2);
                    {
                        if(effectData.GetDataCount() > 0 )
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                EditorGUILayout.LabelField("ID", selection.ToString(), GUILayout.Width(uiWidthLarge));
                                effectData.names[selection] = EditorGUILayout.TextField("이름.", effectData.names[selection], GUILayout.Width(uiWidthLarge * 1.5f));
                                effectData.effectClips[selection].effectType = (EffectType)EditorGUILayout.EnumPopup("이펙트 타입.",
                                    effectData.effectClips[selection].effectType,
                                    GUILayout.Width(uiWidthLarge));

                                //이펙트 리로스 실제로 보여주기
                                EditorGUILayout.Separator();
                                if(effectSource == null && effectData.effectClips[selection].effectName != string.Empty)
                                {
                                    effectData.effectClips[selection].PreLoad();
                                    //full path 사용해볼것
                                    effectSource = Resources.Load(
                                        effectData.effectClips[selection].effectPath +
                                        effectData.effectClips[selection].effectName) as GameObject;
                                }
                                effectSource = (GameObject)EditorGUILayout.ObjectField("이펙트.", this.effectSource, typeof(GameObject), false, GUILayout.Width(uiWidthLarge * 1.5f));

                                if (effectSource != null) //불러오기 성공 -> 경로와 이름 세팅
                                {
                                    effectData.effectClips[selection].effectPath = EditorHelper.GetPath(this.effectSource);
                                    effectData.effectClips[selection].effectName = effectSource.name;
                                }
                                else //불러오기 실패
                                {
                                    effectData.effectClips[selection].effectPath = string.Empty;
                                    effectData.effectClips[selection].effectName = string.Empty;
                                    effectSource = null;
                                }
                                EditorGUILayout.Separator();
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

        //하단
        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Reload Settings"))
            {
                effectData = CreateInstance<EffectData>();
                effectData.LoadData();
                selection = 0;
                this.effectSource = null;
            }
            if(GUILayout.Button("Save"))
            {
                EffectTool.effectData.SaveData();
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
        string enumName = "EffectList";
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        for (int i = 0; i < effectData.names.Length; i++)
        {
            if (effectData.names[i] != string.Empty)
            {
                builder.AppendLine($"     {effectData.names[i]} = {i},");
            }
        }
        EditorHelper.CreateEnumStructure(enumName, builder);
    }

    #endregion Method
}
