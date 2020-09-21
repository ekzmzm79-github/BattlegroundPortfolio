using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정찰 행동을 담당
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/Patrol")]
public class PatrolAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.enemyAnimation.AbortPendingAim(); // 에임 막기
        controller.enemyAnimation.anim.SetBool(FC.AnimatorKey.Crouch, false); // 포복 막기
        controller.personalTarget = Vector3.positiveInfinity; // 타겟 더미값
        controller.CoverSpot = Vector3.positiveInfinity; // 엄폐물 더미값
    }

    private void Patrol(StateController controller)
    {
        if(controller.patrolWaypoints.Count == 0)
        {
            return;
        }

        controller.focusSight = false;
        controller.nav.speed = controller.generalStats.patrolSpeed;
        // navMesh.pathPending : 현재 경로를 계산 중이다
        if (controller.nav.remainingDistance <= controller.nav.stoppingDistance && !controller.nav.pathPending) 
        {
            // 남은 거리가 멈춰야하는 거리이하이고 경로 계산 중이 아니라면 patrolTimer 증가
            controller.variables.patrolTimer += Time.deltaTime;
            if (controller.variables.patrolTimer >= controller.generalStats.PatrolWaitTime)
            {
                // 현재 patrolTimer가 PatrolWaitTime 이상이라면 wayPointeIndex 증가시키고 patrolTimer 리셋
                controller.wayPointeIndex = (controller.wayPointeIndex + 1) % controller.patrolWaypoints.Count;
                controller.variables.patrolTimer = 0;
            }
        }
        try
        {
            controller.nav.destination = controller.patrolWaypoints[controller.wayPointeIndex].position;
        }
        catch(UnassignedReferenceException)
        {
            Debug.LogWarning("웨이 포인트 세팅이 되지 않았습니다.", controller.gameObject);
            controller.patrolWaypoints = new List<Transform> // 임의로 새 리스트 추가(제자리 대기)
            {
                controller.transform
            };
            controller.nav.destination = controller.transform.position;
        }
    }

    public override void Act(StateController controller)
    {
        Patrol(controller);
    }
}
