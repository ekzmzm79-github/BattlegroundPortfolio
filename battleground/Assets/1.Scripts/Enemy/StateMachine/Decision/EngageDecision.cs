using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타겟이 보이거나 근처에 있으면 교전 대기 시간을 초기화하고
/// 반대로 보이지 않거나 멀어져 있다면 blindEngageTime만큼 기다릴지 결정
/// blindEngageTime : 타켓이 있다는것은 인지하지만 사격이 불가능한 상태에서 타켓을 찾는 시간
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Engage")]
public class EngageDecision : Decision
{
    [Header("Extra Decision")]
    public LookDecision isViewing;
    public FocusDecision targetNear;

    public override bool Decide(StateController controller)
    {
        if(isViewing.Decide(controller) || targetNear.Decide(controller))
        {
            //타겟이 보이거나 감지했다면 blindEngageTimer = 0 초기화
            controller.variables.blindEngageTimer = 0;
        }
        else if(controller.variables.blindEngageTimer >= controller.blindEngageTime)
        {
            // 현재 Enemy가 blindEngageTime이 정해둔 시간을 넘어섰다면
            // blindEngageTimer = 0 초기화 및 false state
            controller.variables.blindEngageTimer = 0;
            return false;
        }

        return true;
    }
}
