using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 숨을 수 있는 엄폐물이 있다면 가만히 서있지만
/// 거리가 더 가까운 엄폐물이 있다면 변경 이동 및 총알 장전
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/FindCover")]
public class FindCoverAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.enemyAnimation.AbortPendingAim();
        controller.enemyAnimation.anim.SetBool(FC.AnimatorKey.Crouch, false);
        ArrayList nextCoverData = controller.coverLookUp.GetBestCoverSpot(controller);
        Vector3 potentialCover = (Vector3)nextCoverData[1];
        if (Vector3.Equals(potentialCover, Vector3.positiveInfinity))
        {
            // 더 나은 엄폐물이 없다면 제자리
            controller.nav.destination = controller.transform.position;
            return;
        }
        else if ((controller.personalTarget - potentialCover).sqrMagnitude <
            (controller.personalTarget - controller.CoverSpot).sqrMagnitude &&
            !controller.IsNearOtherSpot(potentialCover, controller.nearRaius))
        {
            /*
             * potentialCover->Target 거리가 현재 Cover->Target 거리보다 작고
             * potentialCover가 현재 가까이에 있지 않다면 '엄폐물 변경'
             */

            controller.coverHash = (int)nextCoverData[0];
            controller.CoverSpot = potentialCover; // == nextCoverData[1]
        }
        
        //실제 이동
        controller.nav.destination = controller.CoverSpot;
        controller.nav.speed = controller.generalStats.evadeSpeed;

        controller.variables.currentShots = controller.variables.shotInRounds; // 재장전
    }
    public override void Act(StateController controller)
    {

    }
}
