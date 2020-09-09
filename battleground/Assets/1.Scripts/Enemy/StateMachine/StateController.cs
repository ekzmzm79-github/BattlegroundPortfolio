using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// state -> actions update -> transition (decision) check..
/// state에 필요한 기능들. 애니메이션 콜백들
/// 시야 체크, 찾아둔 엄폐물 장소 중 최단거리 위치를 찾는 기능.
/// </summary>
public class StateController : MonoBehaviour
{
    #region Variable

    
    public GeneralStats generalStats;
    public ClassStats statData;
    public string classID = string.Empty; // PISTOL, RIFLE, AK

    public ClassStats.Param classStats
    {
        get
        {
            if(this.classID != string.Empty)
            {
                return this.classStats;
            }

            foreach(ClassStats.Sheet sheet in statData.sheets)
            {
                foreach(ClassStats.Param param in sheet.list)
                {
                    if(param.ID.Equals(this.classID))
                    {
                        classID = param.ID;
                        classStats = param;
                        return param;
                    }
                }
            }

            return null;
        }

        set
        {
            classStats = value;
        }
    }

    public State currentState;
    public State remainState; // currentState에서 트랜지션이 없을때

    public Transform aimTarget;
    public List<Transform> patrolWaypoints; // 웨이포인트들

    public int bullets; // 가지고 있는 총알의 개수
    [Range(0,50)]
    public float viewRadius; // 볼 수 있는 시야 반경
    [Range(0, 360)]
    public float viewAngle;
    [Range(0, 25)]
    public float perceptionRadius;

    [HideInInspector] public float nearRaius;
    [HideInInspector] public NavMeshAgent nav;
    [HideInInspector] public int wayPointeIndex;
    [HideInInspector] public int maximumBurst = 7; //유효한 총알 개수
    [HideInInspector] public float blindEngageTime = 30f; // 대상이 시야에서 사라진 뒤에도 유지하며 타겟을 찾는 시간

    [HideInInspector] public bool targetInSight;
    [HideInInspector] public bool focusSight;
    [HideInInspector] public bool reloading;
    [HideInInspector] public bool hadClearShot; // before (이전까지 clearShot을 했었나?)
    [HideInInspector] public bool haveClearShot; // now (현재 clearShot을 했나?)
    [HideInInspector] public int coverHash = -1; // 각 cover마다 Enemy의 인스턴스 ID를 부여해서 동일한 cover에 여러 Enemy가 없도록

    [HideInInspector] public EnemyVariables variables; // 여러 state에서 사용할 변수 모음
    [HideInInspector] public Vector3 personalTarget = Vector3.zero; // Enemy 개개인의 타겟

    private int magBullets; // 잔탄량
    private bool aiActive;

    private static Dictionary<int, Vector3> coverSpot; // 각 cover의 위치를 모아둔 static dictionary
    private bool strafing; // 움직이면서 플레이어를 조준하는 중이냐?
    private bool aiming;
    private bool checkedOnLoop, blockedSight;

    [HideInInspector] public EnemyAnimation enemyAnimation;
    [HideInInspector] public CoverLookUp coverLookUp;

    public Vector3 CoverSpot
    {
        
        get
        {
            // GetInstanceID 와 GetHashCode 둘다 유사하게 해당 객체 고유의 인스턴스 id를 리턴하지만
            // GetHashCode 는 단순 변수나 문자열의 인스턴스 id는 유일하지 않다.
            // why GetHashCode()? not GetInstanceID() : 
            return coverSpot[this.GetHashCode()];
        }
        set { coverSpot[this.GetHashCode()] = value; }
        
    }

    public void TransitionToState(State nextState, Decision decision)
    {
        if(nextState != remainState)
        {
            currentState = nextState;
        }
    }

    #endregion Variable
}
