using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 더블체크:
/// 인근에 장애물이나 엄폐물이 가깝게 있는지 체크 (1)
/// 타겟 목표까지 장애물이나 엄폐물이 있는지 체크 (2) 
/// -> 만약 처음 검출된 충돌체가 플레이어라면 막히지 않았다는 의미
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/ClearShot")]
public class ClearShotDecition : Decision
{
    [Header("Extra Decision")]
    public FocusDecision targetNear;

    /// <summary>
    /// 현재 상태에서 클리어샷이 가능한지 아닌지 판단
    /// </summary>
    private bool HaveClearShot(StateController controller)
    {
        // 사격 위치 (높이)
        Vector3 shotOrigin = controller.transform.position +
            Vector3.up * (controller.generalStats.aboveCoverHeight + controller.nav.radius);
        Vector3 shotDirection = controller.personalTarget - shotOrigin;

        // 1번째 체크
        bool blockedShot = Physics.SphereCast(shotOrigin, controller.nav.radius, shotDirection, out RaycastHit hit,
            controller.nearRaius, controller.generalStats.coverMask | controller.generalStats.obstacleMask);
        if (!blockedShot)
        {
            // 2번째 체크
            blockedShot = Physics.Raycast(shotOrigin, shotDirection, out hit, shotDirection.magnitude,
                controller.generalStats.coverMask | controller.generalStats.obstacleMask);
            if (blockedShot)
            {
                // hit.transform.root == controller.aimTarget.root 
                // -> true라면 Player가 ray에서 검출되었다는 의미
                blockedShot = !(hit.transform.root == controller.aimTarget.root);
            }
        }

        // HaveClearShot 이므로 blockedShot과 반대값을 리턴
        return !blockedShot;
    }

    public override bool Decide(StateController controller)
    {
        // 타겟이 가깝거나 클리어샷이 가능하다면 True Transition
        return targetNear.Decide(controller) || HaveClearShot(controller);
    }
}
