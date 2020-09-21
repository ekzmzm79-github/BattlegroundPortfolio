using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타겟이 보이지만 유효사격 거리가 아닐때, 유효사격 거리까지 이동
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/GotoShotSpot")]
public class GotoShotSpotAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.nav.destination = controller.personalTarget;
        controller.nav.speed = controller.generalStats.chaseSpeed;
        controller.enemyAnimation.AbortPendingAim();
    }

    public override void Act(StateController controller)
    {

    }
}
