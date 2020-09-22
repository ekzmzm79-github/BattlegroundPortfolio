using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투가 끝나서 모든 감지가 해제되는 액션
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/ExitFocus")]
public class ExitFocusAction : Action
{
    public override void OnReadyAction(StateController controller)
    {
        controller.focusSight = false;
        controller.variables.feelAlert = false;
        controller.variables.hearAlert = false;
        controller.Strafing = false;
        controller.nav.destination = controller.personalTarget;
        controller.nav.speed = 0f;
    }

    public override void Act(StateController controller)
    {

    }
}
