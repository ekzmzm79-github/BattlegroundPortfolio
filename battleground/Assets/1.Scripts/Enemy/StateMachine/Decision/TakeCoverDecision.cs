using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 엄폐물로 이동할 수 있는 상황인지 아닌지를 판단
/// 사격할 총알이 남아 있거나, 엄폐물로 이동하기 전에 대기시간이 남아있거나,
/// 숨을만한 엄폐물이 없을 경우에는 False Decision, 그 외에는 True Decision
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/TakeCover")]
public class TakeCoverDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        //지금 사격할 총알이 남아있거나, 대기시간이 더 필요하거나,
        //엄폐물 위치를 찾지 못했다면 false
        if (controller.variables.currentShots < controller.variables.shotInRounds ||
            controller.variables.waitInCoverTime > controller.variables.coverTime ||
            Equals(controller.CoverSpot, Vector3.positiveInfinity))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
