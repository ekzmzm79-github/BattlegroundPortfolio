using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해당 목표로 이동하지 않고 Focus
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Actions/SpotFocus")]
public class SpotFocusAction : Action
{
    public override void Act(StateController controller)
    {
        controller.nav.destination = controller.personalTarget;
        controller.nav.speed = 0;
    }
}
