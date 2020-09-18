using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타켓이 멀리있고 엄폐물에서 최소 한 타임 정도는 공격을 기다린 후에
/// 다음 엄폐물로 이동할지를 결정
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/AdvanceCover")]
public class AdvanceCoverDecision : Decision
{
    [Tooltip("다음 엄폐물로 이동할지 고민하는 시간(라운드)")]
    public int waitRound = 1;

    [Header("Extra Decition")]
    [Tooltip("플레이어가 가까이 있는지 판단")]
    // Decision이 또 다른 Decision을 가짐
    public FocusDecision targetNear;

    public override void OnEnableDecision(StateController controller)
    {
        controller.variables.waitRounds += 1;
        controller.variables.advanceCoverDecision =
            Random.Range(0f, 1f) < controller.classStats.ChangeCoverChance / 100f;
        // AdvanceCoverDecision 결정 성공 확률
    }

    public override bool Decide(StateController controller)
    {
        if(controller.variables.waitRounds <= waitRound)
        {
            // controller.variables.waitRounds : 현재까지 이 Enemy가 기다린 라운드(시간)
            // waitRound : 현재 이 Enemy가 AdvanceCoverDecision을 하기 위해서 기다려야할 라운드(시간)
            return false;
        }

        controller.variables.waitRounds = 0;
        return controller.variables.advanceCoverDecision && !targetNear.Decide(controller);
        // AdvanceCoverDecision 결정 성공 및 타겟이 가깝지 않다.
    }
}
