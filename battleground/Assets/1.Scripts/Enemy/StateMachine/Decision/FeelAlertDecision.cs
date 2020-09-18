using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경고를 감지했는지 판단
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/FeelAlert")]
public class FeelAlertDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        return controller.variables.feelAlert;
    }
}
