using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타겟이 죽었는지 체크
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/TargetDead")]
public class TargetDeadDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        try
        {
            return controller.aimTarget.root.GetComponent<HealthBase>().IsDead;
            // aimTarget에 HealthBase 컴포넌트가 없다면 예외 발생
        }
        catch (UnassignedReferenceException)
        {
            Debug.LogError("생명력 관리 컴포넌트 HealthBase를 추가해주세요. " + controller.name, 
                controller.gameObject); // 어떤 오브젝트가 해당 로그를 남겼는지 알려줌
        }

        return false;
    }
}
