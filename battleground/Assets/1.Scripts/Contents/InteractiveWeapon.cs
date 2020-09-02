using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 충돌체를 생성해서(Interactive용 구체형) 무기를 루팅할 수 있도록 함
/// 루팅했다면 충돌체는 제거되며 무기를 다시 버릴 수 있다.
/// (무기를 버리면 충돌체는 다시 복구됨.)
/// 관련 UI 역시 컨트롤 가능해야하며, ShootBehaviour에 루팅한 무기를 추가함
/// </summary>
public class InteractiveWeapon : MonoBehaviour
{
    #region Variable
    public string label_weaponName;
    public SoundList shotSound, reloadSound, pickSound, dropSound, noBulletSound;
    public Sprite weaponSprite;

    //각 무기마다의 플레이어 보정값 변수
    public Vector3 rightHandPosition; // 플레이어 오른손에 보정 위치
    public Vector3 relativeRotation; // 플레이어에 맞춘 보정 회전값
    public float bulletDamage = 10f;
    public float recoilAngle; // 반동
    public Transform muzzleTransform; // 총구 위치

    public enum WeaponType
    {
        NONE,
        SHORT,
        LONG,
    }
    public enum WeaponMode
    {
        SEMI,
        BURST,
        AUTO,
    }

    public WeaponType weaponType = WeaponType.NONE;
    public WeaponMode weaponMode = WeaponMode.SEMI;
    public int burstSize = 1;

    public int currentMagCapacity, totalBullets; // 현재 탄창량, 소지한 해당 전체 총알량
    private Transform myTransform;
    private int fullMag, maxBullets; // 재장전시 꽉 채우는 탄량, 한번에 채울 수 있는 최대 총알량
    private GameObject player, gameController;
    private ShootBehaviour playerInventory; // bad naming
    private BoxCollider weaponCollider;
    private SphereCollider interactiveRadius;
    private Rigidbody weaponRigidbody;
    private bool pickable;

    //UI
    public GameObject screenHUD;
    public WeaponUIManager weaponHUD;
    private Transform pickHUD;
    public Text pickipHUD_Label;

    #endregion Variable

    #region Method

    private void Awake()
    {
        myTransform = transform;
        gameObject.name = this.label_weaponName;

        // 레이캐스트 무시 레이어 설정(하위 자식 모두)
        gameObject.layer = LayerMask.NameToLayer(FC.TagAndLayer.LayerName.IgnoreRayCast);
        foreach(Transform tr in this.myTransform)
        {
            tr.gameObject.layer = LayerMask.NameToLayer(FC.TagAndLayer.LayerName.IgnoreRayCast);
        }

        player = GameObject.FindGameObjectWithTag(FC.TagAndLayer.TagName.Player);
        playerInventory = player.GetComponent<ShootBehaviour>();
        gameController = GameObject.FindGameObjectWithTag(FC.TagAndLayer.TagName.GameController);

        if(weaponHUD == null)
        {
            if(screenHUD == null)
            {
                screenHUD = GameObject.Find("ScreenHUD");
            }
            weaponHUD = screenHUD.GetComponent<WeaponUIManager>();
        }

        if(pickHUD == null)
        {
            pickHUD = gameController.transform.Find("PickupHUD");
        }

        //Interactive 하기 위한 충돌체 설정
        weaponCollider = myTransform.GetChild(0).gameObject.AddComponent<BoxCollider>();
        CreateInteractiveRadius(weaponCollider.center);
        weaponRigidbody = gameObject.AddComponent<Rigidbody>();

        if (this.weaponType == WeaponType.NONE)
        {
            this.weaponType = WeaponType.SHORT;
        }
        fullMag = currentMagCapacity;
        maxBullets = totalBullets;
        pickHUD.gameObject.SetActive(false);
        if (muzzleTransform == null)
        {
            muzzleTransform = transform.Find("muzzle");
        }
    }

    /// <summary>
    /// 플레이어가 바닥에 떨어져있는 상태의 아이템과 상호작용하기 위한 구체형 충돌체 생성
    /// </summary>
    /// <param name="centor">구체 중심 벡터</param>
    private void CreateInteractiveRadius(Vector3 centor)
    {
        interactiveRadius = gameObject.AddComponent<SphereCollider>();
        interactiveRadius.center = centor;
        interactiveRadius.radius = 1f;
        interactiveRadius.isTrigger = true;
    }


    /// <summary>
    /// 플레이어가 떨어진 아이템 인근으로 이동하면 나타나는 Billboard 3D UI 끄고 켜기
    /// </summary>
    private void TogglePickHUD(bool toggle)
    {
        pickHUD.gameObject.SetActive(toggle);
        if(toggle)
        {
            pickHUD.position = myTransform.position + Vector3.up * 0.5f;
            Vector3 direction = player.GetComponent<BehaviourController>().playerCamera.forward;
            direction.y = 0;
            pickHUD.rotation = Quaternion.LookRotation(direction); // -direction ?
            pickipHUD_Label.text = "Pick " + this.gameObject.name;
        }
    }

    private void UpdateHUD()
    {
        //총알량은 지속적인 업데이트가 필요하다
        weaponHUD.UpdateWeaponHUD(weaponSprite, currentMagCapacity, fullMag, totalBullets);
    }

    public void Toggle(bool active)
    {
        if(active)
        {
            SoundManager.Instance.PlayOneShotEffect((int)pickSound, myTransform.position, 0.5f);
        }
        weaponHUD.Toggle(active);
        UpdateHUD();
    }

    private void Update()
    {
        if(this.pickable && Input.GetButtonDown(ButtonName.Pick))
        {
            // 주울 수 있는 상태에서 pick 버튼을 눌렀다

            //disable physics weapon
            weaponRigidbody.isKinematic = true;
            weaponCollider.enabled = false;
            playerInventory.AddWeapon(this);
            Destroy(interactiveRadius);
            this.Toggle(true);
            this.pickable = false;
            TogglePickHUD(false);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        /*
         * 아이템이 바닥에 떨어졌을 때, but 너무 먼 거리에서는 소리 재생할 필요없음
         */ 
        if(collision.collider.gameObject != player &&
            Vector3.Distance(myTransform.position, player.transform.position) <= 5f)
        {
            SoundManager.Instance.PlayOneShotEffect((int)dropSound, myTransform.position, 0.5f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == player)
        {
            pickable = false;
            TogglePickHUD(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject == player && playerInventory && playerInventory.isActiveAndEnabled)
        {
            pickable = true;
            TogglePickHUD(true);
        }
    }

    public void Drop()
    {
        gameObject.SetActive(true);
        myTransform.position += Vector3.up;
        weaponRigidbody.isKinematic = false;
        myTransform.parent = null; // 주웠을 때는 플레이어의 본 하위 자식으로 붙어있기 때문에
        CreateInteractiveRadius(weaponCollider.center);
        weaponCollider.enabled = true;
        weaponHUD.Toggle(false);
    }

    /// <summary>
    /// 전체 총알량과 현재 탄창을 비교하여 리로드 분기 실행
    /// </summary>
    /// <returns>리로드 성공, 실패 여부</returns>
    public bool StartReload()
    {
        if(currentMagCapacity == fullMag || totalBullets == 0)
        {
            //현재 탄창이 풀이거나 총알이 더 이상 없다면 리로드 불가
            return false;
        }
        else if(totalBullets < fullMag - currentMagCapacity) 
        {
            // 총알이 있지만 다 채우진 못함 -> 남은량만큼만 리로드
            currentMagCapacity += totalBullets;
            totalBullets = 0;
        }
        else
        {
            totalBullets -= fullMag - currentMagCapacity;
            currentMagCapacity = fullMag;
        }

        return true;
    }

    public void EndReload()
    {
        UpdateHUD();
    }
    public bool Shoot(bool firstShot = true)
    {
        if(currentMagCapacity > 0)
        {
            currentMagCapacity--;
            UpdateHUD();
            return true;
        }

        if(firstShot && noBulletSound != SoundList.None)
        {
            SoundManager.Instance.PlayOneShotEffect((int)noBulletSound, muzzleTransform.position, 5f);
        }

        return false;
    }

    public void ResetBullet()
    {
        currentMagCapacity = fullMag;
        totalBullets = maxBullets;
    }

    #endregion Method

}
