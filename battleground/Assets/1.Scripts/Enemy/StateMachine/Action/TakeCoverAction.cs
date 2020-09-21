using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/TakeCover")]
public class TakeCoverAction : Action
{
    private readonly int coverMin = 2; // 커버에 머무는 최소시간
    private readonly int coverMax = 5; // 커버에 머무는 최대시간

    public override void OnReadyAction(StateController controller)
    {
        controller.variables.feelAlert = false;
        controller.variables.waitInCoverTime = 0f;
        if (!Equals(controller.CoverSpot, Vector3.positiveInfinity))
        {
            controller.enemyAnimation.anim.SetBool(FC.AnimatorKey.Crouch, true);
            controller.variables.coverTime = Random.Range(coverMin, coverMax);
        }
        else
        {
            controller.variables.coverTime = 0.1f;
        }
    }

    private void Rotating(StateController controller)
    {
        Vector3 dirToVector = controller.personalTarget - controller.transform.position;
        if (dirToVector.sqrMagnitude < 0.001f || dirToVector.sqrMagnitude > 1000000.0f)
        {
            //dirToVector 크기가 이상하다면
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(dirToVector); // 타겟을 바라보는 각도
        if (Quaternion.Angle(controller.transform.rotation, targetRotation) > 5f) 
        {
            // 현재 각도와 타겟 각도 차이가 5이상이라면
            controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation,
                10f * Time.deltaTime); // 해당 각도로 서서히 변화
        }
    }

    public override void Act(StateController controller)
    {
        if(!controller.reloading)
        {
            controller.variables.waitInCoverTime += Time.deltaTime;
        }
        controller.variables.blindEngageTimer += Time.deltaTime;

        if (controller.enemyAnimation.anim.GetBool(FC.AnimatorKey.Crouch))
        {
            //웅크린 중에도 각도는 변경
            Rotating(controller);
        }
    }
}
