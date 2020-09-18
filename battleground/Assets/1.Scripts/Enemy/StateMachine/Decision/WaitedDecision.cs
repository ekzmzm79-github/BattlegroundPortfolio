using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤하게 정해진 시간만큼 기다렸는지 체크
/// </summary>
[CreateAssetMenu(menuName = "PluggableAI/Decisions/Waited")]
public class WaitedDecision : Decision
{
    [Tooltip("기다리는 최대 시간")]
    public float maxTimeToWait;

    private float timeToWait; // 랜덤하게 정해질 기다리는 시간 
    private float startTime; // 기다리기 시작한 시간

    public override void OnEnableDecision(StateController controller)
    {
        timeToWait = Random.Range(0f, maxTimeToWait);
        startTime = Time.time;
    }

    public override bool Decide(StateController controller)
    {
        return (startTime - Time.time) >= timeToWait;
    }
}
