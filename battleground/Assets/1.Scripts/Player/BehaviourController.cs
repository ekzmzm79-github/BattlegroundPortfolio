using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 동작, 기본 동작, 오버라이딩 동작, 잠긴 동작, 마우스 이동값,
/// 땅에 서있는지, GenericBehaviour를 상속받은 동작들을 업데이트 시킴
/// </summary>
public class BehaviourController : MonoBehaviour
{
    #region Variable
    private List<GenericBehaviour> behaviours; // 동작들을 담아두는 리스트
    private List<GenericBehaviour> overrideBehaviours; // 우선시 되는 동작 리스트
    private int currentBehaviour; // 현재 동작 해시코드
    private int defaultBehaviour; // 기본 동작 해시코드
    private int BehaviourLocked; // 잠긴 동작 해시코드

    //캐싱할 것들
    public Transform playerCamera;
    private Animator myAnimator;
    private Rigidbody myRigidbody;
    private ThirdPersonOrbitCam camScript;
    private Transform myTransform;

    //속성 값들
    private float h; // horizontal axis;
    private float v; // vertical axis;
    public float turnSmoothing = 0.06f; // 카메라를 향하도록 움직일때의 회전속도
    private bool changeFOV; // 달리기 동작에 의해 카메라 시야각이 변경되었는지를 체크
    public float sprintFOV = 100; // 달리기 시야각
    private Vector3 lastDirection;
    private bool sprint; // 현재 달리는 중인가 체크
    private int hFloat;// 애니메이터 가로축 키값용
    private int vFloat;// 애니메이터 세로축 키값용
    private int groundedBool; // 애니메이터에 지상에 있는지를 알려주는 변수
    private Vector3 colExtents; // 땅과의 충돌체크를 위한 충돌체 영역

    #endregion Variable

    #region Method
    //프로퍼티
    //public Vector3 LastDirection { get => lastDirection; set => lastDirection = value; }
    public float GetH { get => h; }
    public float GetV { get => v; }
    public ThirdPersonOrbitCam GetCamScript { get => camScript; }
    public Rigidbody GetRigidbody { get => myRigidbody; }
    public Animator GetAnimator { get => myAnimator; }
    public int GetDefaultBehaviour { get => defaultBehaviour; }



    public void Awake()
    {
        behaviours = new List<GenericBehaviour>();
        overrideBehaviours = new List<GenericBehaviour>();
        myAnimator = GetComponent<Animator>();
        hFloat = Animator.StringToHash(FC.AnimatorKey.Horizontal);
        vFloat = Animator.StringToHash(FC.AnimatorKey.Vertical);
        camScript = playerCamera.GetComponent<ThirdPersonOrbitCam>();
        myRigidbody = GetComponent<Rigidbody>();
        myTransform = transform;
        //ground?
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    public bool IsMoving()
    {
        // return (h != 0) || (v != 0); -> 부동소수점 표현 때문에 잘못된 코드

        //Epsilon: 실수가 가질 수 있는 가장 작은 값
        //Epsilon보다 h || v 가 크다면 현재 이동 중이라고 판정
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }
    public bool IsHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }
    public bool CanSprint()
    {
        // 모든 상태들을 돌면서 sprint가 불가능한 상태인지 찾아낸다.
        foreach(GenericBehaviour behaviour in behaviours)
        {
            if(!behaviour.AllowSprint)
            {
                return false;
            }
        }
        foreach(GenericBehaviour behaviour in overrideBehaviours)
        {
            if(!behaviour.AllowSprint)
            {
                return false;
            }
        }

        return true;
    }
    public bool IsSprinting()
    {
        return sprint && IsMoving() && CanSprint();
    }

    public bool IsGrounded()
    {
        Ray ray = new Ray(myTransform.position + Vector3.up * 2 * colExtents.x, Vector3.down);
        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        myAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
        myAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);

        sprint = Input.GetButton(ButtonName.Sprint);
        if(IsSprinting())
        {
            changeFOV = true;
            camScript.SetFOV(sprintFOV); // 달리고 있다면 FOV가 서서히 바뀐다
        }
        else if(changeFOV)
        {
            // 달리기가 멈췄다면 FOV 리셋
            camScript.ResetFOV();
            changeFOV = false;
        }

        myAnimator.SetBool(groundedBool, IsGrounded());
    }

    /// <summary>
    /// 캐릭터 방향에 이상이 생길때를 대비해서 리포지셔닝
    /// </summary>
    public void Repositioning()
    {
        if(lastDirection != Vector3.zero)
        {
            lastDirection.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
            //보간
            Quaternion newRotation = Quaternion.Slerp(myRigidbody.rotation, targetRotation, turnSmoothing);
            myRigidbody.MoveRotation(newRotation);
        }
    }
    /// <summary>
    /// behaviours 중애서 현재 활성화 된것이 있다면 찾아서 해당 behaviour.LocalFixedUpdate();
    /// 각 상태마다 잠그는 상태가 존재하고 상황에 따라 overrideBehaviours가 증감하므로
    /// 그 상황상황마다 계속해서 체크해서 각자 fixedUpdate
    /// </summary>
    private void FixedUpdate()
    {
        bool isAnyBehaviourActive = false;
        if (BehaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach(GenericBehaviour behaviour in behaviours)
            {
                if(behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    isAnyBehaviourActive = true;
                    behaviour.LocalFixedUpdate();
                }
            }
        }
        else
        {
            foreach (GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalFixedUpdate();
            }
        }
        if (!isAnyBehaviourActive && overrideBehaviours.Count == 0)
        {
            myRigidbody.useGravity = true;
            Repositioning();
        }
    }


    private void LateUpdate()
    {
        if (BehaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            foreach (GenericBehaviour behaviour in behaviours)
            {
                if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode)
                {
                    behaviour.LocalLateUpdate(); // 카메라는 캐릭터 움직인 뒤에 움직여야 한다
                }
            }
        }
        else
        {
            foreach (GenericBehaviour behaviour in overrideBehaviours)
            {
                behaviour.LocalLateUpdate();
            }
        }
    }

    public void SubScribeBehaviour(GenericBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }
    public void RegisterDefaultBehaviour(int behaviourCode)
    {
        defaultBehaviour = behaviourCode;
        currentBehaviour = behaviourCode;
    }
    public void RegisterBehaviour(int behaviourCode)
    {
        if(currentBehaviour == defaultBehaviour)
        {
            currentBehaviour = behaviourCode;
        }
    }
    public void UnRegisterBehaviour(int behaviourCode)
    {
        if (currentBehaviour == behaviourCode)
        {
            currentBehaviour = defaultBehaviour;
        }
    }

    public bool OverrideWithBehaviour(GenericBehaviour behaviour)
    {
        if(!overrideBehaviours.Contains(behaviour))
        {
            if(overrideBehaviours.Count == 0)
            {
                foreach(GenericBehaviour _behaviour in behaviours)
                {
                    if(_behaviour.isActiveAndEnabled && currentBehaviour == _behaviour.GetBehaviourCode)
                    {
                        _behaviour.OnOverride();
                        break;
                    }
                }
            }

            overrideBehaviours.Add(behaviour);
            return true;
        }

        return false;
    }

    public bool RevokeOverrideingBehaviour(GenericBehaviour behaviour)
    {
        if (overrideBehaviours.Contains(behaviour))
        {
            overrideBehaviours.Remove(behaviour);
            return true;
        }

        return false;
    }

    public bool IsOverriding(GenericBehaviour behaviour = null)
    {
        if(behaviour == null)
        {
            return overrideBehaviours.Count > 0;
        }

        return overrideBehaviours.Contains(behaviour);
    }

    public bool IsCurrentBehaviour(int behaviourCode)
    {
        return this.currentBehaviour == behaviourCode;
    }
    public bool GetTempLockStatus(int behaviourCode = 0)
    {
        return (BehaviourLocked != 0 && BehaviourLocked != behaviourCode);
    }

    public void LockTempBehaviour(int behaviourCode)
    {
        if(BehaviourLocked == 0)
        {
            BehaviourLocked = behaviourCode;
        }
    }
    public void UnLockTempBehaviour(int behaviourCode)
    {
        if (BehaviourLocked == behaviourCode)
        {
            BehaviourLocked = 0;
        }
    }

    public Vector3 GetLastDirection()
    {
        return lastDirection;
    }
    public void SetLastDirection(Vector3 direction)
    {
        this.lastDirection = direction;

    }

    #endregion Method

}

/// <summary>
/// 각 행동 상태를 담을 수 있는 상위 추상 클래스, 틀
/// </summary>
public abstract class GenericBehaviour : MonoBehaviour
{
    protected int speedFloat;
    protected BehaviourController behaviourController;
    protected int behaviourCode; // 해시 코드
    protected bool canSprint;

    private void Awake()
    {
        this.behaviourController = GetComponent<BehaviourController>();
        speedFloat = Animator.StringToHash(FC.AnimatorKey.Speed);
        canSprint = true;

        // 동작 타입을 해시코드로 가지고 있다가 추후에 구별용으로 사용
        // 각 behaviour 마다 정수 코드를 가짐 
        behaviourCode = this.GetType().GetHashCode();

    }

    public int GetBehaviourCode
    {
        get => behaviourCode;
    }

    public bool AllowSprint
    {
        get => canSprint;
    }

    public virtual void LocalLateUpdate()
    {

    }
    public virtual void LocalFixedUpdate()
    {

    }
    public virtual void OnOverride()
    {

    }

}