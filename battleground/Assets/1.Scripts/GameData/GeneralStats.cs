using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CreateAssetMenu : 해당 클래스를 어셋화 시킬 수 있음
[CreateAssetMenu(menuName = "PluggableAI/GeneralStats")]

//ScriptableObject 
//대량의 데이터를 저장하는 데 사용할 수 있는 데이터 컨테이너
//값의 사본이 생성되는 것을 방지
public class GeneralStats : ScriptableObject
{
    #region Variable

    [Header("General")]
    [Tooltip("npc 정찰 속도 clear state")]
    public float patrolSpeed = 2f;
    [Tooltip("npc 추적 속도 warning state")]
    public float chaseSpeed = 2f;
    [Tooltip("npc 회피 속도 engage state")]
    public float evadeSpeed = 15f;
    [Tooltip("npc가 waypoine에서 대기하는 시간")]
    public float PatrolWaitTime = 2f;
    [Header("Animation")]
    [Tooltip("장애물 레이어 마스크")]
    public LayerMask obstacleMask;
    [Tooltip("조준시 깜빡임을 피하기위한 최소 확정 앵글")]
    public float angleDeadZone = 5f; // 조준 유효 범위
    [Tooltip("속도 댐핑 시간")]
    public float speedDampTime = 0.4f;
    [Tooltip("각 속도 댐핑 시간")]
    public float angularSpeedDampTime = 0.2f;
    [Tooltip("각 속도 안에서 각도 회전에 따른 반응 시간")]
    public float angleResponseTime = 0.2f;

    [Header("Cover")]
    [Tooltip("장애물로 인식하는 최소 높이값")]
    public float aboveCoverHeight = 1.5f;
    [Tooltip("장애물 레이어 마스크")]
    public LayerMask coverMask;
    [Tooltip("사격 레이어 마스크")]
    public LayerMask shotMask;
    [Tooltip("타겟 레이어 마스크")]
    public LayerMask targetMask;

    #endregion Variable
}
