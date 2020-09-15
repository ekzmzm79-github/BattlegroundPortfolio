using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 Enemy가 시야가 막히지 않은 상태에서
/// 타겟이 시야각 1/2 사이에 있는지 판단
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Look")]
public class LookDecision : Decision
{
    private bool MyHandleTargets(StateController controller, bool hasTarget, Collider[] targetsInRadius)
    {
        if(hasTarget)
        {
            //플레이어의 위치
            Vector3 target = targetsInRadius[0].transform.position; // 플레이어는 현재 무조건 하나이므로
            Vector3 dirToTarget = target - controller.transform.position;
            bool inFOVCondition = (Vector3.Angle(controller.transform.forward, dirToTarget) <
                controller.viewAngle / 2); // forward ~ dirToTarget 각도가 viewAngle/2 보다 작은가? ㅜ(인지가능한가?)

            if(inFOVCondition && !controller.BlockedSight())
            {
                controller.targetInSight = true;
                controller.personalTarget = controller.aimTarget.position;
                return true;
            }
        }
        return false;
    }

    public override bool Decide(StateController controller)
    {
        controller.targetInSight = false;
        return CheckTargetInRadius(controller, controller.viewRadius, MyHandleTargets);
    }

}
