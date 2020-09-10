using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// feel shot, cover, repeat, patrol, attack decision
/// </summary>
[System.Serializable]
public class EnemyVariables
{
    public bool feelAlert; //위협 감지했나?
    public bool hearAlert;
    public bool advanceCoverDecision; // 현재 엄폐물보다 타겟보다 가까운 엄폐물이 존재하는가?
    public int waitRounds; // 플레이어가 몇 발을 쏜 뒤에 공격할지
    public bool repeatShot;
    public float waitInCoverTime;
    public float coverTime; // 현재까지 숨어있었던 시간
    public float patrolTimer; // 정찰 타이머
    public float shotTimer;
    public float startShootTimer;
    public float currentShots;
    public float shotInRounds; // 현재 교전중에 얼마나 쐈는가
    public float blindEngageTimer; // 시야에서 사라진 플레이어를 찾는 시간

}
