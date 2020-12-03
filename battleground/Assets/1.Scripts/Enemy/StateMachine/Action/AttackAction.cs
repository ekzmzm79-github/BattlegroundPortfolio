using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사격까지 총 4단계의 과정을 거침
/// 1. 조준 중이고 조준 유효 각도 안에 타겟이 있는가와 충분히 가까운지 체크
/// 2. 발사 간격 딜레이가 충분히 지났다면 애니메이션을 재생
/// 3. 충돌 검출 및 사격 시 발생하는 반동(충격파)으로 인한 오발 구현
/// 4. 총구 이펙트 및 총알 이펙트를 생성해 줍니다.
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/Attack")]
public class AttackAction : Action
{
    private readonly float startShootDelay = 0.2f;
    private readonly float aimAngleGap = 30f;

    // 액션 실행전에 초기화 세팅
    public override void OnReadyAction(StateController controller)
    {
        controller.variables.shotInRounds = Random.Range(controller.maximumBurst / 2, controller.maximumBurst);
        controller.variables.currentShots = 0;
        controller.variables.startShootTimer = 0f;
        controller.enemyAnimation.anim.ResetTrigger(FC.AnimatorKey.Shooting);
        controller.enemyAnimation.anim.SetBool(FC.AnimatorKey.Crouch, false);
        controller.variables.waitInCoverTime = 0f;
        controller.enemyAnimation.ActivatePendingAim(); // 조준 대기 : 시야에만 확인되면 조준가능
    }

    /// <summary>
    /// 실제 사격을 위한 함수
    /// </summary>
    /// <param name="stateController">Enemy</param>
    /// <param name="direction">사격 방향</param>
    /// <param name="hitPoint">총알이 맞는 위치(타겟의 위치와는 다름)</param>
    /// <param name="hitNormal"></param>
    /// <param name="organic">Bullet Hole을 표시할지 말지를 결정</param>
    /// <param name="target">타겟</param>
    private void DoShot(StateController controller, Vector3 direction, Vector3 hitPoint,
        Vector3 hitNormal = default, bool organic = false, Transform target = null)
    {
        // 총알 발사 이펙트 생성 및 세팅
        GameObject muzzleFlash = EffectManager.Instance.EffectOneShot((int)EffectList.flash, Vector3.zero);
        muzzleFlash.transform.SetParent(controller.enemyAnimation.gunMuzzle);
        muzzleFlash.transform.localPosition = Vector3.zero;
        muzzleFlash.transform.localEulerAngles = Vector3.left * 90f;
        DestroyDelayed destroyDelayed = muzzleFlash.AddComponent<DestroyDelayed>();
        destroyDelayed.DelayTime = 0.5f; // 0.5초 뒤에 자동으로 삭제

        // 총아 궤적 이펙트 생성 및 세팅
        GameObject shotTracer = EffectManager.Instance.EffectOneShot((int)EffectList.tracer, Vector3.zero);
        shotTracer.transform.SetParent(controller.enemyAnimation.gunMuzzle);
        Vector3 origin = controller.enemyAnimation.gunMuzzle.position;
        shotTracer.transform.position = origin;
        shotTracer.transform.rotation = Quaternion.LookRotation(direction);

        // 피탄 구멍 생성 여부 체크
        if(target && !organic)
        {
            GameObject bulletHole = EffectManager.Instance.EffectOneShot((int)EffectList.bulletHole,
                hitPoint + 0.01f * hitNormal);
            bulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal);

            GameObject instantSpark = EffectManager.Instance.EffectOneShot((int)EffectList.shotEffect, hitPoint);
        }
        else if(target && organic) // == player
        {
            HealthBase targetHealth = target.GetComponent<HealthBase>(); // playerHealth
            if(targetHealth)
            {
                targetHealth.TakeDamage(hitPoint, direction, controller.classStats.BulletDamage,
                    target.GetComponent<Collider>(), controller.gameObject); // maybe floating text
            }
        }

        // (현재 Enemy가 가진 무기 종류에 따라 사운드 재생)
        SoundManager.Instance.PlayShotSound(controller.classID,
            controller.enemyAnimation.gunMuzzle.position, 2f);

    }

    /// <summary>
    /// 오발률을 포함해서 발사 위치와 방향을 세팅하고 DoShot 실행
    /// </summary>
    private void CastShot(StateController controller)
    {
        /* imprecision : 부정확,
         * Enemy가 사격시마다 랜덤적으로 오발률을 만들어서 발사 위치를 랜덤하게 결정
         * (약간 오른쪽 + 위쪽)
         */ 
        Vector3 imprecision = Random.Range(-controller.classStats.ShotErrorRate, controller.classStats.ShotErrorRate) *
            controller.transform.right;
        imprecision += Random.Range(-controller.classStats.ShotErrorRate, controller.classStats.ShotErrorRate) *
            controller.transform.up;

        //발사 방향 세팅
        Vector3 shotDirection = controller.personalTarget - controller.enemyAnimation.gunMuzzle.position;
        shotDirection = shotDirection.normalized + imprecision; //오발률 더하기
        Ray ray = new Ray(controller.enemyAnimation.gunMuzzle.position, shotDirection);
        if(Physics.Raycast(ray, out RaycastHit hit, controller.viewRadius, controller.generalStats.shotMask.value))
        {
            // 0이 아니라면 orgaic에 해당함
            bool isOrganic = ((1 << hit.transform.root.gameObject.layer) & controller.generalStats.targetMask) != 0;
            DoShot(controller, ray.direction, hit.point, hit.normal, isOrganic, hit.transform);
        }
        else
        {
            //orgaic에 맞지 않았다면 -> 허공을 맞추는것과 마찬가지이므로 3번째 인자 임읠 지정
            DoShot(controller, ray.direction, ray.origin + (ray.direction * 500f));
        }
    }

    /// <summary>
    /// 현재 발사 가능한 상태인가를 체크
    /// </summary>
    private bool CanShoot(StateController controller)
    {
        float distance = (controller.personalTarget -
            controller.enemyAnimation.gunMuzzle.position).sqrMagnitude;
        if(controller.Aiming &&
            (controller.enemyAnimation.currentAimingAngleGap < aimAngleGap ||
            distance <= 200.0f)) // 상수값 제거하고 현재 가지고있는 무기 종류에따라 사정거리 변화주기
        {
            /* 현재 조준 중이고,
             * (현재 각도 차이가 aimAngleGap보다 작거나 거리가 5.0 이하라면)
             */
            if(controller.variables.startShootTimer >=startShootDelay)
            {
                // 딜레이 시간이상 대기했음
                return true;
            }
            else
            {
                controller.variables.startShootTimer += Time.deltaTime; // 한 프레임 시간만큼 더해주기
            }
        }

        return false;
    }


    /// <summary>
    /// 발사 딜레이 타임을 체크해서 CastShot을 실행
    /// </summary>
    private void Shoot(StateController controller)
    {
        if(Time.timeScale > 0 && controller.variables.shotTimer == 0f)
        {
            //발사 처리
            controller.enemyAnimation.anim.SetTrigger(FC.AnimatorKey.Shooting);
            CastShot(controller);
        }
        else if(controller.variables.shotTimer >= (0.1f + 3f * Time.deltaTime))
        {
            // (0.1f * 2f * Time.deltaTime) : 임의로 정한 딜레이 타임
            // 애니메이션 재생을 위한 시간이 따로 필요해서 약간의 텀을 두고 castshot을 실행함
            controller.bullets = Mathf.Max(--controller.bullets, 0);
            controller.variables.currentShots++;
            controller.variables.shotTimer = 0;
            return;
        }
        // 아직 딜레이 타임이므로 shot 불가 및 shotTimer에 시간 더해주기
        controller.variables.shotTimer += controller.classStats.ShotRateFactor * Time.deltaTime;
    }
    
    // Act: (CanShoot 체크) -> Shoot(딜레이타임) -> CastShot(위치 및 방향 확정) -> DoShot(사격 이펙트 생성 및 피격)
    public override void Act(StateController controller)
    {
        controller.focusSight = true;

        if(CanShoot(controller))
        {
            Shoot(controller);
        }

        controller.variables.blindEngageTimer += Time.deltaTime;
    }
}

