using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAnimation : MonoBehaviour
{
    #region Variable
    
    [HideInInspector] public Animator anim;
    [HideInInspector] public float currentAimingAngleGap;
    [HideInInspector] public Transform gunMuzzle;
    [HideInInspector] public float angularSpeed; // 각속도

    private StateController controller;
    private NavMeshAgent nav;
    private bool pendingAim; // 조준을 기다리는 시간 -> 캐릭터를 자연스럽게 돌리는 동작을 위해서
    private Transform hips, spine; //bone trans
    private Vector3 initialRootRotation;
    private Vector3 initialHipsRotation;
    private Vector3 initialSpineRotation;
    private Quaternion lastRotation;
    private float timeCountAim, timeCountGuard; // 조준 중인 시간, 가드 중인 시간
    private readonly float turnSpeed = 25f; // strafing turn speed;
    #endregion Variable

    #region Method
    private void Awake()
    {
        //setup
        controller = GetComponent<StateController>();
        nav = GetComponent<NavMeshAgent>();
        nav.updateRotation = false; // 회전은 직접 작성하기 때문에
        anim = GetComponent<Animator>();

        hips = anim.GetBoneTransform(HumanBodyBones.Hips);
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipsRotation = hips.localEulerAngles;
        initialSpineRotation = spine.localEulerAngles;

        anim.SetTrigger(FC.AnimatorKey.ChangeWeapon);
        anim.SetInteger(FC.AnimatorKey.Weapon, (int)System.Enum.Parse(typeof(WeaponType), 
            controller.classStats.WeaponType));

        foreach(Transform child in anim.GetBoneTransform(HumanBodyBones.RightHand))
        {
            gunMuzzle = child.Find("muzzle");
            if(gunMuzzle != null)
            {
                break;
            }
        }
        foreach(Rigidbody member in GetComponentsInChildren<Rigidbody>())
        {
            member.isKinematic = true;
        }

    }

    void Setup(float speed, float angle, Vector3 strafeDirection)
    {
        /*
         * 각속도 (회전력) : angle(radian 기준) / time(일정한 각도를 도는데 걸리는 시간)
         */
        angle *= Mathf.Deg2Rad;
        angularSpeed = angle / controller.generalStats.angleResponseTime;

        //animator가 현재 가진 값에서 세팅시키려는 값으로 서서히 damp타임동안에 변화시킴
        anim.SetFloat(FC.AnimatorKey.Speed, speed, controller.generalStats.speedDampTime, Time.deltaTime);
        anim.SetFloat(FC.AnimatorKey.AngularSpeed, angularSpeed, controller.generalStats.angularSpeedDampTime,
            Time.deltaTime);
        anim.SetFloat(FC.AnimatorKey.Horizontal, strafeDirection.x, controller.generalStats.speedDampTime,
            Time.deltaTime);
        anim.SetFloat(FC.AnimatorKey.Vertical, strafeDirection.y, controller.generalStats.speedDampTime,
            Time.deltaTime);
    }

    /// <summary>
    /// NavMeshAgent 컴퍼넌트에 의해서 Animator가 셋업시키는 함수
    /// </summary>
    void NavAnimSetup()
    {
        float speed;
        float angle;
        speed = Vector3.Project(nav.desiredVelocity, transform.forward).magnitude;
        if(controller.focusSight)
        {
            Vector3 dest = (controller.personalTarget - transform.position);
            dest.y = 0.0f;
            //SignedAngle : from ~ to 사이의 각도 값 (axis 회전 중심 벡터?)
            angle = Vector3.SignedAngle(transform.forward, dest, transform.up);
            if(controller.Strafing) // Strafing 중이냐?
            {
                dest = dest.normalized;
                // dest 값이 너무 작으면 LookRotation 오작동 할 수 있다.
                Quaternion targetStrafeRotation = Quaternion.LookRotation(dest);

                //현재 Enemy의 회전 값에서 -> 타겟을 바라보는 값으로 서서히 변화
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    targetStrafeRotation, turnSpeed * Time.deltaTime);
            }
        }
        else // Strafing 중이 아니다
        {
            // desiredVelocity: 회피를 고려한 NavMeshAgent의 목표 속도
            if (nav.desiredVelocity == Vector3.zero)
            {
                angle = 0.0f;
            }
            else
            {
                angle = Vector3.SignedAngle(transform.forward, nav.desiredVelocity, transform.up);
            }
        }
        // 플레이어를 향하려 할때 깜빡거리지 않도록 각도 데드존을 적용 
        // -> 회전하려는 angle이 너무 작으면 캐릭터가 작게 진동하는 현상을 위해서
        if (!controller.Strafing && Mathf.Abs(angle) < controller.generalStats.angleDeadZone)
        {
            transform.LookAt(transform.position + nav.desiredVelocity);
            angle = 0.0f;
            if (pendingAim && controller.focusSight)
            {
                //아직 조준 쿨타임인데, 타겟을 바라보고 있다
                controller.Aiming = true;
                pendingAim = false;
            }
        }

        //Strafe direction
        Vector3 direction = nav.desiredVelocity;
        direction.y = 0.0f;
        direction = direction.normalized;
        // Quaternion 4차원 벡터이므로 역벡터 곱셈을 통해서 3차원 벡터로 변환시킨다
        direction = Quaternion.Inverse(transform.rotation) * direction;
        Setup(speed, angle, direction);

    }

    private void Update()
    {
        NavAnimSetup();
    }

    private void OnAnimatorMove()
    {
        if(Time.timeScale > 0 && Time.deltaTime > 0)
        {
            nav.velocity = anim.deltaPosition / Time.deltaTime;
            if(!controller.Strafing)
            {
                transform.rotation = anim.rootRotation;
            }
        }
    }

    private void LateUpdate()
    {
        if(controller.Aiming)
        {
            // enemy -> player로 향하는 벡터
            Vector3 direction = controller.personalTarget - spine.position;
            if(direction.magnitude < 0.01f || direction.magnitude > 1000000.0f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(initialRootRotation);
            targetRotation *= Quaternion.Euler(initialHipsRotation);
            targetRotation *= Quaternion.Euler(initialSpineRotation);

            targetRotation *= Quaternion.Euler(FC.VectorHelper.ToVector(controller.classStats.AimOffset));
            Quaternion frameRotation = Quaternion.Slerp(lastRotation, targetRotation, timeCountAim);
            // 엉덩이를 기준으로 척추 회전이 60도 이하인 경우는 계속 조준이 가능
            if (Quaternion.Angle(frameRotation, hips.rotation) <= 60.0f)
            {
                spine.rotation = frameRotation;
                timeCountAim += Time.deltaTime;
            }
            else
            {
                // 아직 조준을 하지 않았고 회전이 70도 초과인 경우 딜레이 주면서 사격모션
                if(timeCountAim == 0 && Quaternion.Angle(frameRotation, hips.rotation) > 70.0f)
                {
                    StartCoroutine(controller.UnstuckAim(2.0f));
                }
                spine.rotation = lastRotation;
                timeCountAim = 0;
            }

            lastRotation = spine.rotation;
            Vector3 target = controller.personalTarget - gunMuzzle.position;
            Vector3 forward = gunMuzzle.forward;
            currentAimingAngleGap = Vector3.Angle(target, forward);

            timeCountGuard = 0;
        }
        else
        {
            lastRotation = spine.rotation;
            spine.rotation *= Quaternion.Slerp(Quaternion.Euler(FC.VectorHelper.ToVector(
                controller.classStats.AimOffset)), Quaternion.identity, timeCountGuard);
            timeCountGuard += Time.deltaTime;
        }
    }

    public void ActivatePendingAim()
    {
        pendingAim = true;
    }
    public void AbortPendingAim()
    {
        pendingAim = false;
        controller.Aiming = false;
    }

    #endregion Method
}
