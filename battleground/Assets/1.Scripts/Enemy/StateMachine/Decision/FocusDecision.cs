using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인지타입(Sense)에 따라 특정 거리로부터(가깝지 않다), 
/// 시야가 막히지 않은 상태에서 위험요소를 감지하거나
/// 너무 가까운 거리에 타겟(플레이어)이 있는지를 판단
/// </summary>
//CreateAssetMenu : 해당 클래스를 어셋화 시킬 수 있음
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Focus")]
public class FocusDecision : Decision
{
    public enum Sense
    {
        NEAR,
        PERCEPTION,
        VIEW,
    }
    [Tooltip("어떤 크기로 위험요소 감지를 하겠는가?")]
    public Sense sense;
    [Tooltip("현재 엄폐물을 해제할까요?")]
    public bool invalidateCoverSpot;

    private float radius; // sense 상태에 따른 감지 범위

    /// <summary>
    /// Decision이 활성화되고 실제 실행되기전에 초기화하는 함수
    /// </summary>
    public override void OnEnableDecision(StateController controller)
    {
        switch(sense)
        {
            case Sense.NEAR:
                radius = controller.nearRaius;
                break;
            case Sense.PERCEPTION:
                radius = controller.perceptionRadius;
                break;
            case Sense.VIEW:
                radius = controller.viewRadius;
                break;
            default:
                Debug.Log("정의되지 않은 Sense 타입이 감지되었습니다.");
                radius = controller.nearRaius;
                break;
        }
    }

    /// <summary>
    /// controller가 플레이어를 발견했는가?(hasTarget)
    /// 발견했다면 targetsInHearRadius 충돌체 반환
    /// </summary>
    private bool MyHandleTargets(StateController controller, bool hasTarget, 
        Collider[] targetsInHearRadius)
    {
        //타겟이 존재하고 시야가 막히지 않았다면
        if(hasTarget && !controller.BlockedSight())
        {
            if(invalidateCoverSpot) // 현재 엄폐물 해제
            {
                //더미값 세팅
                controller.CoverSpot = Vector3.positiveInfinity;
            }

            controller.targetInSight = true;
            controller.personalTarget = controller.aimTarget.position;
            return true; // true Desicion -> 해당 Transition을 통해 해당 State로 변화
        }
        return false; // false Desicion
    }

    public override bool Decide(StateController controller)
    {
        return (sense != Sense.NEAR && controller.variables.feelAlert && !controller.BlockedSight()) 
            || CheckTargetInRadius(controller, radius, MyHandleTargets);
        // NEAR가 아닌데, 경고를 느꼈고, 시야가 막혀있지 않거나 (||)
        // 타겟(플레이어)이 인지 범위안에서 감지 되었다면 true
    }

}
