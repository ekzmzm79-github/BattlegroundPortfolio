using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무기 획득하면 해당 무기를 ui를 통해 보여줌
/// 또한 현재 잔탄량과 전체 소지가능한 총알량을 출력
/// </summary>
public class WeaponUIManager : MonoBehaviour
{
    #region Variable
    

    public Color bulletColor = Color.white;
    public Color emptyBulletColor = Color.black;
    private Color noBulletColor; // 투명하게 색 표시

    [SerializeField] private Image weaponHUD;
    [SerializeField] private GameObject bulletMag; // 탄창 게임 오브젝트
    [SerializeField] private Text totalBulletsHUD;


    #endregion Variable

    #region Method
    
    // Start is called before the first frame update
    void Start()
    {
        noBulletColor = new Color(0f, 0f, 0f, 0f);
        if(weaponHUD == null)
        {
            weaponHUD = transform.Find("WeaponHUD/Weapon").GetComponent<Image>();
        }
        if(bulletMag == null)
        {
            bulletMag = transform.Find("WeaponHUD/Data/Mag").gameObject;
        }
        if (totalBulletsHUD == null)
        {
            totalBulletsHUD = transform.Find("WeaponHUD/Data/bulletAmount").GetComponent<Text>();
        }

        Toggle(false);
    }

    public void Toggle(bool active)
    {
        weaponHUD.transform.parent.gameObject.SetActive(active); // 무기를 들어야만 weaponHUD 켜짐
    }

    public void UpdateWeaponHUD(Sprite weaponSprite, int bulletLeft, int fullMag, int ExtraBullets)
    {
        if(weaponSprite != null && weaponHUD.sprite != weaponSprite)
        {
            weaponHUD.sprite = weaponSprite;
            weaponHUD.type = Image.Type.Filled;
            weaponHUD.fillMethod = Image.FillMethod.Horizontal;
        }

        int bulletCount = 0;
        foreach(Transform bullet in bulletMag.transform)
        {
            if(bulletCount < bulletLeft)
            {
                //잔탄
                bullet.GetComponent<Image>().color = bulletColor;
            }
            else if(bulletCount >= fullMag)
            {
                //넘치는 탄
                bullet.GetComponent<Image>().color = noBulletColor;
            }
            else
            {
                //사용한 탄
                bullet.GetComponent<Image>().color = emptyBulletColor;
            }

            bulletCount++;

        }
        totalBulletsHUD.text = bulletLeft + "/" + ExtraBullets;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion Method
}
