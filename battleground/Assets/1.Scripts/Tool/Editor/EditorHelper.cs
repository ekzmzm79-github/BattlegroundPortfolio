using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityObject = UnityEngine.Object;

public class EditorHelper
{

	/// <summary>
	/// 경로 계산 함수.
	/// </summary>
	/// <param name="p_clip"></param>
	/// <returns></returns>
	public static string GetPath(UnityEngine.Object p_clip)
	{
		string retString = string.Empty;
		retString = AssetDatabase.GetAssetPath(p_clip);
		string[] path_node = retString.Split('/'); //Assets/9.ResourcesData/Resources/Sound/BGM.wav
		bool findResource = false;
		for (int i = 0; i < path_node.Length - 1; i++)
		{
			if (findResource == false)
			{
				if (path_node[i] == "Resources")
				{
					findResource = true;
					retString = string.Empty;
				}
			}
			else
			{
				retString += path_node[i] + "/";
			}

		}

		return retString;
	}

	/// <summary>
	/// Data 리스트를 enum structure로 뽑아주는 함수.
	/// </summary>
	public static void CreateEnumStructure(string enumName, StringBuilder data)
	{
		string templateFilePath = "Assets/Editor/EnumTemplate.txt";

		string entittyTemplate = File.ReadAllText(templateFilePath);

		entittyTemplate = entittyTemplate.Replace("$DATA$", data.ToString());
		entittyTemplate = entittyTemplate.Replace("$ENUM$", enumName);
		string folderPath = "Assets/1.Scripts/GameData/";
		if (Directory.Exists(folderPath) == false)
		{
			Directory.CreateDirectory(folderPath);
		}

		string FilePath = folderPath + enumName + ".cs";
		if (File.Exists(FilePath))
		{
			File.Delete(FilePath);
		}
		File.WriteAllText(FilePath, entittyTemplate);
	}

    /// <summary>
    /// 모든 툴에서 공통적으로 사용될 상단 수평 버튼들을 구현하는 메소드
    /// </summary>
    /// <param name="data">원본 데이터</param>
    /// <param name="selection">선택할 인덱스</param>
    /// <param name="source"></param>
    /// <param name="uiWidth">ui 크기</param>
    public static void EditorToolTopLayer(BaseData data, ref int selection, 
        ref UnityObject source, int uiWidth)
    {
        //opengl과 유사한 방식으로 코딩
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("ADD", GUILayout.Width(uiWidth))) //uiWidth 크기의 ADD 버튼을 클릭시
            {
                data.AddData("NewData");
                selection = data.GetDataCount() - 1; //리스트의 가장 마지막 인덱스
                source = null;
            }
            if (GUILayout.Button("Copy", GUILayout.Width(uiWidth))) //uiWidth 크기의 Copy 버튼을 클릭시
            {
                data.Copy(selection);
                source = null;
                selection = data.GetDataCount() - 1;
            }

            if (data.GetDataCount() > 1) // 내부 if와 위치 바꿔볼것
            {
                if (GUILayout.Button("Remove", GUILayout.Width(uiWidth)))
                {
                    source = null;
                    data.RemoveData(selection);
                }
            }

            if (selection > data.GetDataCount() - 1) 
            {
                selection = data.GetDataCount() - 1;
            }

        }
        EditorGUILayout.EndHorizontal();

    }

    /// <summary>
    /// 모든 툴에서 사용될 수직 리스트 레이아웃
    /// </summary>
    /// <param name="ScrollPosition"></param>
    /// <param name="data"></param>
    /// <param name="selection"></param>
    /// <param name="source"></param>
    /// <param name="uiWidth"></param>
    public static void EditorToolListLayer(ref Vector2 ScrollPosition, BaseData data, 
        ref int selection, ref UnityObject source, int uiWidth)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(uiWidth));
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical("box");
            {
                ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
                {
                    if (data.GetDataCount() > 0)
                    {
                        int lastSelection = selection; //선택을 바꿧는지를 체크하기 위해
                        selection = GUILayout.SelectionGrid(selection, data.GetNameList(true), 1);

                        if (lastSelection != selection) 
                        {
                            // 선택이 바뀌었다.
                            source = null;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }

}
