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
    /// 사격을 위한 함수
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

        //사운드 재생 : SoundList.pistol 추후에 수정 요망 
        // (현재 Enemy가 가진 무기 종류에 따라 사운드 재생)
        SoundManager.Instance.PlayOneShotEffect((int)SoundList.pistol, 
            controller.enemyAnimation.gunMuzzle.position, 2f);

    }


    public override void Act(StateController controller)
    {
        
    }
}

