using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 사격 기능 : 사격이 가능한지의 여부를 체크해야함
/// 발사 키 입력을 받아서 애니메이션 재생, 이펙트 생성, 충돌 체크 기능
/// UI 관련 십자선(크로스 헤어) 표시 기능
/// 발사 속도 조정 기능(interver)
/// 캐릭터 상체를 IK(Inverse Kinematics)를 이용하여서 조준 시점에 맞춰 회전
/// 벽이나 충돌체 총알이 피격되었을 경우 피탄 이펙트를 생성(pooling)
/// 인벤토리 역활(어쩔 수 없이), 무기 소지 확인하는 기능
/// 재장전과 무기 교체 기능까지 포함
/// </summary>
public class ShootBehaviour : GenericBehaviour
{
    #region Variable
    public Texture2D aimCrossHair, shootCrossHair;
    public GameObject muzzleFlash, shot, spark; // 이펙트
    public Material bulletHole; // 피탄 텍스쳐 매터리얼
    public int maxBulletHoles = 50;
    public float shootErrorRate = 0.01f; // 오발률
    public float shootRateFactor = 1f; // 발사 속도

    public float armsRotation = 8f; // 팔 회전

    //shot이 가능한 마스크들
    public LayerMask shotMask = ~(FC.TagAndLayer.LayerMasking.IgnoreRayCast | FC.TagAndLayer.LayerMasking.IgnoreShot |
        FC.TagAndLayer.LayerMasking.CoverInvisible | FC.TagAndLayer.LayerMasking.Player);
    //생명체 체크용 마스크
    public LayerMask organicMask = FC.TagAndLayer.LayerMasking.Player | FC.TagAndLayer.LayerMasking.Enemy;
    // 짧은 총(피스톨)을 들었을 때 조준시 왼팔의 위치 보정.
    public Vector3 leftArmShortAim = new Vector3(-4.0f, 0.0f, 2.0f);

    private int activeWeapon = 0;

    //animator value
    private int weaponType;
    private int changeWeaponTrigger;
    private int shootingTrigger;
    private int aimBool, blockedAimBool, reloadBool;

    private List<InteractiveWeapon> weapons; // 소지 무기들 -> inventory
    private bool isAiming, isAimBlocked;
    private Transform gunMuzzle;
    private float distToHand; // 목부터 손까지의 거리

    private Vector3 castRelativeOrigin;
    private Dictionary<InteractiveWeapon.WeaponType, int> slotMap; // 각 슬롯에 장착된 장비

    // IK를 위한 변수들
    private Transform hips, spine, chest, rightHand, leftArm; 
    private Vector3 initialRootRoatation;
    private Vector3 initialHipsRoatation;
    private Vector3 initialSpineRoatation;
    private Vector3 initialChestRoatation;

    private float shotInterval, originalShotInterval = 0.5f;
    private List<GameObject> bulletHoles; // 피탄 이펙트가 표시될 오브젝트들
    private int bulletHoleSlot = 0; // 생성 총알이 maxBulletHoles 개수를 넘어가면 재순환할 인덱스 변수 
    private int burstShotCount = 0; // 피탄 효과를 내는 총알 개수
    private AimBehaviour aimBehaviour;
    private Texture2D originalCorssHair;
    private bool isShooting = false;
    private bool isChangingWeapon = false;
    private bool isShotAlive = false; // 아직 남아있는 총알이 있는가
    #endregion Variable


    #region Method
    private void Start()
    {
        weaponType = Animator.StringToHash(FC.AnimatorKey.Weapon);
        aimBool = Animator.StringToHash(FC.AnimatorKey.Aim);
        blockedAimBool = Animator.StringToHash(FC.AnimatorKey.BlockedAim);
        changeWeaponTrigger = Animator.StringToHash(FC.AnimatorKey.ChangeWeapon);
        shootingTrigger = Animator.StringToHash(FC.AnimatorKey.Shooting);
        reloadBool = Animator.StringToHash(FC.AnimatorKey.Reload);
        weapons = new List<InteractiveWeapon>(new InteractiveWeapon[3]);
        aimBehaviour = GetComponent<AimBehaviour>();
        bulletHoles = new List<GameObject>();

        muzzleFlash.SetActive(false);
        shot.SetActive(false);
        spark.SetActive(false);

        slotMap = new Dictionary<InteractiveWeapon.WeaponType, int>
        {
            {InteractiveWeapon.WeaponType.SHORT, 1 },
            {InteractiveWeapon.WeaponType.LONG, 2 }
        };

        Transform neck = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Neck);
        if(!neck) // neck 은 가끔 없는 경우가 있다
        {
            neck = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Head).parent;
        }

        hips = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        spine = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
        chest = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        leftArm = this.behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        initialRootRoatation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRoatation = hips.localEulerAngles;
        initialSpineRoatation = spine.localEulerAngles;
        initialChestRoatation = chest.localEulerAngles;
        originalCorssHair = aimBehaviour.crossHair;
        shotInterval = originalShotInterval;
        castRelativeOrigin = neck.position - transform.position; //
        distToHand = (rightHand.position - neck.position).magnitude * 1.5f;//
        
    }

    /// <summary>
    /// 사격시 이펙트 그리는 메소드
    /// targetNormal : 타겟 표현에 수직인 벡터 (법선 벡터)
    /// 타겟의 위치 destination과 타겟이 피격당한 위치에서의 수직인 벡터 targetNormal는 다를 수 있다
    /// </summary>
    private void DrawShoot(GameObject weapon, Vector3 destination, Vector3 targetNormal, Transform parent,
        bool placeSparks = true, bool placeBulletHole = true)
    {
        Vector3 origin = gunMuzzle.position - gunMuzzle.right * 0.5f;
        muzzleFlash.SetActive(true);
        muzzleFlash.transform.SetParent(gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.back * 90f;

        GameObject instantShot = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, origin);
        instantShot.SetActive(true);
        instantShot.transform.rotation = Quaternion.LookRotation(destination - origin);
        instantShot.transform.parent = shot.transform.parent;

        if(placeSparks)
        {
            // 생명체가 아닌 곳에 사격했을 때, 스파크 이펙트가 나타나야함
            GameObject instantSparks = EffectManager.Instance.EffectOneShot((int)EffectList.sparks, destination);
            instantSparks.SetActive(true);
            instantSparks.transform.parent = spark.transform.parent;
        }

        if(placeBulletHole)
        {
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.back, targetNormal);
            GameObject hole = null;
            if(bulletHoles.Count < maxBulletHoles)
            {
                hole = GameObject.CreatePrimitive(PrimitiveType.Quad);
                hole.GetComponent<MeshRenderer>().material = bulletHole;
                hole.GetComponent<Collider>().enabled = false;
                hole.transform.localScale = Vector3.one * 0.07f;
                hole.name = "BulletHole";
                bulletHoles.Add(hole);
            }
            else
            {
                hole = bulletHoles[bulletHoleSlot];
                bulletHoleSlot++;
                bulletHoleSlot %= maxBulletHoles;
            }

            hole.transform.position = destination + 0.01f * targetNormal;
            hole.transform.rotation = hitRotation;
            hole.transform.SetParent(parent);
        }
        
    }

    private void ShootWeapon(int weaponIdx, bool firstShot = true)
    {
        if(!isAiming || isAimBlocked || behaviourController.GetAnimator.GetBool(reloadBool) ||
            !weapons[weaponIdx].Shoot(firstShot))
        {
            return;
        }
        else
        {
            burstShotCount++;
            behaviourController.GetAnimator.SetTrigger(shootingTrigger);
            aimBehaviour.crossHair = shootCrossHair;
            behaviourController.GetCamScript.BounceVertical(weapons[weaponIdx].recoilAngle);

            // imprecision : 총알이 무조건 일직선 정방향이 아니라 약간의 errorRate를 가지고 흔들리게만듦.
            Vector3 imprecision = Random.Range(-shootErrorRate, shootErrorRate) 
                * behaviourController.playerCamera.forward;
            Ray ray = new Ray(behaviourController.playerCamera.position,
                behaviourController.playerCamera.forward + imprecision);
            RaycastHit hit = default(RaycastHit);
            if(Physics.Raycast(ray, out hit, 500f, shotMask))
            {
                if(hit.collider.transform != transform) // 나 자신이 맞은게 아니라면
                {
                    bool isOrganic = (organicMask == (organicMask | (1 << hit.transform.root.gameObject.layer)));
                    DrawShoot(weapons[weaponIdx].gameObject, hit.point, hit.normal, hit.collider.transform,
                        !isOrganic, !isOrganic);
                    if(hit.collider)
                    {
                        hit.collider.SendMessageUpwards("HitCallback", new HealthBase.DamageInfo(
                            hit.point, ray.direction, weapons[weaponIdx].bulletDamage, hit.collider),
                            SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
            else // 레이캐스트 히트가 없을때
            {
                //허공에 발사
                Vector3 destination = (ray.direction * 500f) - ray.origin;
                DrawShoot(weapons[weaponIdx].gameObject, destination, Vector3.up, null, false ,false);
            }

            SoundManager.Instance.PlayOneShotEffect((int)weapons[weaponIdx].shotSound, gunMuzzle.position, 5f);
            GameObject gameController = GameObject.FindGameObjectWithTag(FC.TagAndLayer.TagName.GameController);
            gameController.SendMessage("RootAlertNearBy", ray.origin, SendMessageOptions.DontRequireReceiver);
            shotInterval = originalShotInterval;
            isShotAlive = true;
        }
    }

    public void EndReloadWeapon()
    {
        behaviourController.GetAnimator.SetBool(reloadBool, false);
        weapons[activeWeapon].EndReload();
    }

    private void SetWeaponCrossHair(bool aimed)
    {
        if(aimed)
        {
            aimBehaviour.crossHair = aimCrossHair;
        }
        else
        {
            aimBehaviour.crossHair = originalCorssHair;
        }
    }

    /// <summary>
    /// firstShot 이후의 사격 과정을 구현하는 메소드
    /// </summary>
    private void ShotProgress()
    {
        if(shotInterval > 0.2f)
        {
            shotInterval -= shootRateFactor * Time.deltaTime;
            if(shotInterval <= 0.4f) // 
            {
                SetWeaponCrossHair(activeWeapon > 0);
                muzzleFlash.SetActive(false);
                if(activeWeapon > 0)
                {
                    behaviourController.GetCamScript.BounceVertical(-weapons[activeWeapon].recoilAngle * 0.1f);

                    if(shotInterval <= (0.4f - 2f * Time.deltaTime))
                    {
                        // shotMode에 따라서 다음 shotInterval을 결정
                        if (weapons[activeWeapon].weaponMode == InteractiveWeapon.WeaponMode.AUTO &&
                            Input.GetAxisRaw(ButtonName.Shoot) != 0)
                        {
                            ShootWeapon(activeWeapon, false);
                        }
                        else if(weapons[activeWeapon].weaponMode == InteractiveWeapon.WeaponMode.BURST &&
                            burstShotCount < weapons[activeWeapon].burstSize)
                        {
                            ShootWeapon(activeWeapon, false);
                        }
                        else if(weapons[activeWeapon].weaponMode != InteractiveWeapon.WeaponMode.BURST)
                        {
                            burstShotCount = 0;
                        }
                    }
                }
            }
        }
        else
        {
            isShotAlive = false;
            behaviourController.GetCamScript.BounceVertical(0);
            burstShotCount = 0;
        }
    }

    private void ChangeWeapon(int oldWeapon, int newWeapon)
    {
        if(oldWeapon > 0)
        {
            weapons[oldWeapon].gameObject.SetActive(false);
            gunMuzzle = null;
            weapons[oldWeapon].Toggle(false);
        }

        while(weapons[newWeapon] == null && newWeapon > 0) // newWeapon 들어갈 슬롯 찾기
        {
            newWeapon = (newWeapon + 1) % weapons.Count;
        }

        if(newWeapon > 0)
        {
            weapons[newWeapon].gameObject.SetActive(true);
            gunMuzzle = weapons[newWeapon].transform.Find("muzzle");
            weapons[newWeapon].Toggle(true);
        }

        activeWeapon = newWeapon;
        if(oldWeapon != newWeapon)
        {
            behaviourController.GetAnimator.SetTrigger(changeWeaponTrigger);
            behaviourController.GetAnimator.SetInteger(weaponType, weapons[newWeapon] ?
                (int)weapons[newWeapon].weaponType : 0);
        }

        SetWeaponCrossHair(newWeapon > 0);
    }

    private void Update()
    {
        // 각 버튼 누른 상태와 현재 사격 상태등을 고려해서 진행

        float shootTrigger = Mathf.Abs(Input.GetAxisRaw(ButtonName.Shoot));
        //float의 0 비교는 반드시 Mathf.Epsilon으로 하자
        if(shootTrigger > Mathf.Epsilon && !isShooting && activeWeapon > 0 && burstShotCount == 0)
        {
            isShooting = true;
            ShootWeapon(activeWeapon);
        }
        else if(isShooting && shootTrigger < Mathf.Epsilon) // 사격
        {
            isShooting = false;
        }
        else if(Input.GetButtonUp(ButtonName.Reload) && activeWeapon > 0) // 리로드
        {
            if(weapons[activeWeapon].StartReload())
            {
                SoundManager.Instance.PlayOneShotEffect((int)weapons[activeWeapon].reloadSound,
                    gunMuzzle.position, 0.5f);
                behaviourController.GetAnimator.SetBool(reloadBool, true);
            }
        }
        else if(Input.GetButtonDown(ButtonName.Drop) && activeWeapon > 0) // 버리기
        {
            EndReloadWeapon();
            int weaponToDrop = activeWeapon;
            ChangeWeapon(activeWeapon, 0);
            weapons[weaponToDrop].Drop();
            weapons[weaponToDrop] = null;
        }
        else
        {
            if (Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) > Mathf.Epsilon && !isChangingWeapon)
            {
                isChangingWeapon = true;
                int nextWeapon = activeWeapon + 1;
                ChangeWeapon(activeWeapon, nextWeapon % weapons.Count);
            }
            else if(Mathf.Abs(Input.GetAxisRaw(ButtonName.Change)) < Mathf.Epsilon)
            {
                isChangingWeapon = false;
            }
        }

        if(isShotAlive)
        {
            ShotProgress();
        }

        isAiming = behaviourController.GetAnimator.GetBool(aimBool);
    }

    /// <summary>
    /// 인벤토리 역활을 하게될 함수
    /// </summary>
    public void AddWeapon(InteractiveWeapon newWeapon)
    {
        // newWeapon 위치 조정
        newWeapon.gameObject.transform.SetParent(rightHand);
        newWeapon.transform.localPosition = newWeapon.rightHandPosition;
        newWeapon.transform.localRotation = Quaternion.Euler(newWeapon.relativeRotation);

        if(weapons[slotMap[newWeapon.weaponType]]) // 해당 타입의 무기가 이미 있다
        {
            if (weapons[slotMap[newWeapon.weaponType]].label_weaponName == newWeapon.label_weaponName)
            {
                // 이름이 같은 무기 -> 이미 해당 슬롯의 같은 무기 착용중인걸 주웠다
                // 단순히 총알 리셋, 바꾸는 모션 실행 및 중복 무기 제거
                weapons[slotMap[newWeapon.weaponType]].ResetBullet();
                ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
                Destroy(newWeapon.gameObject);
                return;
            }
            else
            {
                // 같은 타입의 다른 무기라면 현재 가지고 있는 무기는 떨굼
                weapons[slotMap[newWeapon.weaponType]].Drop();
            }
        }

        weapons[slotMap[newWeapon.weaponType]] = newWeapon;
        ChangeWeapon(activeWeapon, slotMap[newWeapon.weaponType]);
    }

    private bool CheckforBlockAim()
    {
        //목(대략적)에서 핸드까지 SphereCast 생성해서 자신이 아닌 다른 물체가 있다면 isAimBlocked
        isAimBlocked = Physics.SphereCast(transform.position + castRelativeOrigin, 0.1f,
            behaviourController.GetCamScript.transform.forward, out RaycastHit hit, distToHand - 0.1f);

        isAimBlocked = isAimBlocked && hit.collider.transform != transform;
        behaviourController.GetAnimator.SetBool(blockedAimBool, isAimBlocked);

        Debug.DrawRay(transform.position + castRelativeOrigin,
            behaviourController.GetCamScript.transform.forward * distToHand, isAimBlocked ? Color.red : Color.cyan);

        return isAimBlocked;
    }

    // 조준 중일때 상체의 기울임 구현
    public void OnAnimatorIK(int layerIndex)
    {
        if(isAiming && activeWeapon > 0)
        {
            if(CheckforBlockAim())
            {
                return;
            }

            //캐릭터 자체의 기울임 -> spine 조절
            Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            targetRot *= Quaternion.Euler(initialRootRoatation);
            targetRot *= Quaternion.Euler(initialHipsRoatation);
            targetRot *= Quaternion.Euler(initialSpineRoatation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Spine,
                Quaternion.Inverse(hips.rotation) * targetRot);

            // 카메라를 고려한 추가적 기울김 -> chekc 조절
            float xcamRot = Quaternion.LookRotation(behaviourController.playerCamera.forward).eulerAngles.x;
            targetRot = Quaternion.AngleAxis(xcamRot + armsRotation, this.transform.right);
            if(weapons[activeWeapon] && weapons[activeWeapon].weaponType == InteractiveWeapon.WeaponType.LONG)
            {
                targetRot *= Quaternion.AngleAxis(9f, transform.right);
                targetRot *= Quaternion.AngleAxis(20f, transform.up);
            }

            targetRot *= spine.rotation;
            targetRot *= Quaternion.Euler(initialChestRoatation);
            behaviourController.GetAnimator.SetBoneLocalRotation(HumanBodyBones.Chest, 
                Quaternion.Inverse(spine.rotation) * targetRot);

        }
    }

    private void LateUpdate()
    {
        //매 프레임마다 짧은 무기로 조준시 leftArm 조절
        if(isAiming && weapons[activeWeapon] && 
            weapons[activeWeapon].weaponType ==InteractiveWeapon.WeaponType.SHORT)
        {
            leftArm.localEulerAngles = leftArm.localEulerAngles + leftArmShortAim;
        }
    }

    #endregion Method
}
