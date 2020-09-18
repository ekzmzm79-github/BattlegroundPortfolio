using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사격이 시작되고 재장전 전까지 한번에 사격 가능한 총알의 수를 판단
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/EndBurst")]
public class EndBurstDecision : Decision
{

    public override bool Decide(StateController controller)
    {
        // 한 번에 사격하는 횟수보다 currentShots이 더 크다면 True - 재장전(Wait)
        return controller.variables.currentShots >= controller.variables.shotInRounds;
    }
}
