using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// alertCheck 통해 경고를 감지했거나,
/// 특정거리에서 시야가 막혀 있어도 특정 위치에서 타겟의 위치가 여러번 인지 되었을 경우
/// '인지했다(들었다)'라고 판단한다.
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Hear")]
public class HearDecision : Decision
{
    Vector3 lastPos, currentPos;

    public override void OnEnableDecision(StateController controller)
    {
        //초기화
        lastPos = currentPos = Vector3.positiveInfinity;
    }

    private bool MyHandleTargets(StateController controller, bool hasTarget, Collider[] targetsInHearRadius)
    {
        if(hasTarget)
        {
            currentPos = targetsInHearRadius[0].transform.position;
            if(!Equals(lastPos, Vector3.positiveInfinity)) // lastPos가 초기값이 아니고
            {
                if(!Equals(lastPos, currentPos))
                {
                    controller.personalTarget = currentPos;
                    return true;
                }
            }
            lastPos = currentPos;
        }
        return false;
    }

    public override bool Decide(StateController controller)
    {
        if(controller.variables.hearAlert)
        {
            controller.variables.hearAlert = false;
            return true;
        }
        else
        {
            return CheckTargetInRadius(controller, controller.perceptionRadius, MyHandleTargets);
        }
    }
}
