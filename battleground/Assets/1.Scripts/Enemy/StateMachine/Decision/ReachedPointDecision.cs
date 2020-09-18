using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// navMeshAgent에서 남은 거리가 멈추는 거리에 포함되거나,
/// 경로를 검색 중이라면 True
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/ReachedPoint")]
public class ReachedPointDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        if(Application.isPlaying == false) // 예외처리
        {
            return false;
        }

        if(controller.nav.remainingDistance <= controller.nav.stoppingDistance &&
            !controller.nav.pathPending)
        {
            //navMeshAgent에서 남은 거리가 멈추는 거리에 포함되거나, 경로를 검색 중이라면 True
            return true;
        }
        else
        {
            return false;
        }
    }
}
