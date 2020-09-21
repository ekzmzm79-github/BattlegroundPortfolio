using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격과 동시에 이동하는 액션이지만 회전 중에는 회전이 끝나고 난 뒤에 
/// strafing 활성화
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/FocusMove")]
public class FocusMoveAction : Action
{
    // 사격과 동시에 일어나는 액션이기 때문에 현재 clearShot이 가능한지 여부에 따라 분기
    public ClearShotDecition clearShotDecition;

    private Vector3 currentDest;
    private bool aligned; // 타겟과 나란히 정렬된 상태인가 (타겟을 바라보는 회전이 완료되었나)


    public override void OnReadyAction(StateController controller)
    {
        controller.hadClearShot = controller.haveClearShot = false;
        currentDest = controller.nav.destination;
        controller.focusSight = true;
        aligned = false;
    }

    public override void Act(StateController controller)
    {
        if(!aligned)
        {
            controller.nav.destination = controller.personalTarget;
            controller.nav.speed = 0f; // 목적지만 세팅하고 실제 이동은 안함 -> 회전만함
            if (controller.enemyAnimation.angularSpeed == 0f) // 회전이 완료되었나
            {
                controller.Strafing = true;
                aligned = true;
                controller.nav.destination = currentDest;
                controller.nav.speed = controller.generalStats.evadeSpeed;
            }
        }
        else
        {
            controller.haveClearShot = clearShotDecition.Decide(controller); // 현재 clearShot 가능한지 체크
            if (controller.hadClearShot != controller.haveClearShot)
            {
                // 이전까지의 clearShot 여부와 현재 다르다면 
                controller.Aiming = controller.haveClearShot;
                if (controller.haveClearShot && !Equals(currentDest, controller.CoverSpot))
                {
                    // 사격 가능하고 현재 목표지와 현재 엄폐물이 다르다면(엄폐물 교체 예정) 
                    controller.nav.destination = controller.transform.position; // 일단 이동 금지
                }
            }
            controller.hadClearShot = controller.haveClearShot;
        }
    }
}
