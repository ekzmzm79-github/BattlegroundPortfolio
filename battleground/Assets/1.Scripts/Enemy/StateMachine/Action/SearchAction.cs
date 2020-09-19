using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타겟이 있다면 타겟까지 이동하지만, 타겟을 잃는다면 가만히 서있는다.(Patrol?)
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/Search")]
public class SearchAction : Action
{
    //초기화
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.enemyAnimation.AbortPendingAim(); // 타겟이 없으니 조준 abort
        controller.enemyAnimation.anim.SetBool(FC.AnimatorKey.Crouch, false);
        controller.CoverSpot = Vector3.positiveInfinity; // 커버는 더미값 세팅
    }

    public override void Act(StateController controller)
    {
        if(Equals(controller.personalTarget, Vector3.positiveInfinity)) // 타겟이 없다
        {
            controller.nav.destination = controller.transform.position; // navMesh 멈춤
        }
        else
        {
            // 타겟이 있다면 navMesh 스피드 및 목적지 세팅
            controller.nav.speed = controller.generalStats.chaseSpeed;
            controller.nav.destination = controller.personalTarget;
        }
    }

}
