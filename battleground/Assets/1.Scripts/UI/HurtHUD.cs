using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 어느 방향에서 피격되었는지 방향을 알려주는 UI
/// </summary>
public class HurtHUD : MonoBehaviour
{
    #region Variable

    struct HurtData
    {
        public Transform shotOrigin;
        public Image hurtImg;
    }

    private Transform canvas;
    private GameObject hurtPrefab;
    private float decayFactor = 0.8f;
    private Dictionary<int, HurtData> hurtUIData;
    private Transform player, cam;

    #endregion Variable

    #region Method

    /// <summary>
    /// HurtHUD 활성화 함수
    /// </summary>
    public void Setup(Transform canvas, GameObject hurtPrefab, float decayFactor, Transform player)
    {
        hurtUIData = new Dictionary<int, HurtData>();
        this.canvas = canvas;
        this.hurtPrefab = hurtPrefab;
        this.decayFactor = decayFactor;
        this.player = player;
        cam = Camera.main.transform;
    }

    /// <summary>
    /// HurtUI의 회전방향을 세팅하는 함수 (z값만 세팅)
    /// </summary>
    private void SetRotation(Image hurtUI, Vector3 orientation, Vector3 shotDirection)
    {
        orientation.y = 0;
        shotDirection.y = 0;
        //SignedAngle : from - to 사이의 부호있는 각도를 반환
        float rotation = Vector3.SignedAngle(shotDirection, orientation, Vector3.up);

        Vector3 newRotation = hurtUI.rectTransform.rotation.eulerAngles;
        newRotation.z = rotation;
        Image hurtImg = hurtUI.GetComponent<Image>();
        hurtImg.rectTransform.rotation = Quaternion.Euler(newRotation);
    }

    private Color GetUpdatedAlpha(Color currentColor, bool reset = false)
    {
        if(reset)
        {
            currentColor.a = 1f;
        }
        else
        {
            currentColor.a -= decayFactor * Time.deltaTime;
        }
        return currentColor;
    }

    public void DrawHurtUI(Transform shotOrigin, int hashID)
    {
        if (hurtUIData.ContainsKey(hashID))
        {
            //이미 기존에 있던 HurtData 이므로 리셋
            hurtUIData[hashID].hurtImg.color = GetUpdatedAlpha(hurtUIData[hashID].hurtImg.color, true);
        }
        else
        {
            //새로운 HurtData의 추가
            GameObject hurtUI = Instantiate(hurtPrefab, canvas); // ui이므로 canvas 하위 자식으로
            // 회전방향 세팅
            SetRotation(hurtUI.GetComponent<Image>(), cam.forward, shotOrigin.position - player.position); 
            
            // 새로운 데이터로 HurtData 생성이후에 hurtUIData에 추가
            HurtData data;
            data.shotOrigin = shotOrigin;
            data.hurtImg = hurtUI.GetComponent<Image>();
            hurtUIData.Add(hashID, data);
        }
    }

    private void Update()
    {
        List<int> toRemoveKeys = new List<int>(); // 제거된 (알파값이 0이하가 된 hurtData들
        foreach (int key in hurtUIData.Keys) // 현재 hurtUIData에 있는 모든 값들을 세팅
        {
            // 방향 세팅
            SetRotation(hurtUIData[key].hurtImg, cam.forward, hurtUIData[key].shotOrigin.position - player.position);
            // 컬러값(알파) 세팅
            hurtUIData[key].hurtImg.color = GetUpdatedAlpha(hurtUIData[key].hurtImg.color);
            if (hurtUIData[key].hurtImg.color.a <= 0f) // 만약 해당 컬러 알파값이 0이하라면
            {
                toRemoveKeys.Add(key); // toRemoveKeys 에 추가
            }
        }

        for (int i = 0; i < toRemoveKeys.Count; i++) // 제거된 hurtData 숫자만큼 제거 및 파괴
        {
            Destroy(hurtUIData[toRemoveKeys[i]].hurtImg.transform.gameObject);
            hurtUIData.Remove(toRemoveKeys[i]);
        }
    }

    #endregion Method
}
