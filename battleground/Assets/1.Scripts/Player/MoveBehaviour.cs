using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동과 점프 동작을 담당
/// 충돌 처리에 대한 기능 포함
/// 기본 동작으로서 작동하는 컴포넌트
/// </summary>

public class MoveBehaviour : GenericBehaviour
{
    #region Variable

    public float walkSpeed = 0.15f;
    public float runSpeed = 1.0f;
    public float sprintSpeed = 2.0f;
    public float speedDampTime = 0.1f;

    public float jumpHeight = 1.5f;
    public float jumpInertialForce = 10f; // 점프 관성
    public float speed, speedSeeker; // 스피드 및 스피드 조절 변수
    private int jumpBool; // 점프 중인지 체크하는 애니메이터 해시값
    private int groundedBool; // 땅인지 체크하는 애니메이터 해시값
    private bool jump; // 점프가 가능한가
    private bool isColliding; // 충돌 중인가
    private CapsuleCollider capsuleCollider;
    private Transform myTransform;

    #endregion Variable

    private void Start()
    {
        myTransform = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        jumpBool = Animator.StringToHash(FC.AnimatorKey.Jump);
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        behaviourController.GetAnimator.SetBool(groundedBool, true);

        // MoveBehaviour 등록
        behaviourController.SubScribeBehaviour(this);
        behaviourController.RegisterDefaultBehaviour(this.behaviourCode);

        speedSeeker = runSpeed;

    }

    Vector3 Rotating(float horizontal, float vertical)
    {
        // 플레이어 카메라의 포워드 방향을 얻어옴
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);

        forward.y = 0.0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x); // forward에 직교 하는 벡터
        Vector3 targetDirection = Vector3.zero;
        targetDirection = forward * vertical + right * horizontal;

        if (behaviourController.IsMoving() && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            //타켓 방향으로 부드러운 회전
            Quaternion newRotation = Quaternion.Slerp(behaviourController.GetRigidbody.rotation, 
                targetRotation, behaviourController.turnSmoothing);
            behaviourController.GetRigidbody.MoveRotation(newRotation);
            behaviourController.SetLastDirection(targetDirection);
        }

        //가만히 있거나 카메라가 마지막 방향으로 회전 중이면 Repositioning
        if (!(Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f))
        {
            behaviourController.Repositioning();
        }

        return targetDirection;
    }

    private void RemoveVecticalVelocity()
    {
        //y 값만 0.0f으로 만들고 다시 복구
        Vector3 horizontalVelocity = behaviourController.GetRigidbody.velocity;
        horizontalVelocity.y = 0.0f;
        behaviourController.GetRigidbody.velocity = horizontalVelocity;
    }

    void MovementManagement(float horizontal, float vertical)
    {
        if (behaviourController.IsGrounded())
        {
            //땅에 붙어있다면 중력을 켠다
            behaviourController.GetRigidbody.useGravity = true;
        }
        else if(!behaviourController.GetAnimator.GetBool(jumpBool) 
            && behaviourController.GetRigidbody.velocity.y > 0 )
        {
            // 점프 중인 아닌데, y값이 0이 아니라는 것은 어딘가에 끼어있다는 뜻
            RemoveVecticalVelocity();
        }

        Rotating(horizontal, vertical);
        Vector2 dir = new Vector2(horizontal, vertical);
        speed = Vector2.ClampMagnitude(dir, 1.0f).magnitude;
        speedSeeker += Input.GetAxis("Mouse ScrollWheel");
        speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
        speed *= speedSeeker;

        if(behaviourController.IsSprinting())
        {
            speed = sprintSpeed;
        }

        //speedFloat 값을 서서히 변화 시켜서 서서히 뛰어가는 모션 구현 
        behaviourController.GetAnimator.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
    }

    //충돌 처리

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
        if(behaviourController.IsCurrentBehaviour(GetBehaviourCode) &&
            collision.GetContact(0).normal.y <= 0.1f) // 경사면에 부딪침
        {
            // 미끄러지게 만들기
            float vel = behaviourController.GetAnimator.velocity.magnitude;
            Vector3 targentMove = Vector3.ProjectOnPlane(myTransform.forward,
                collision.GetContact(0).normal).normalized * vel;
            behaviourController.GetRigidbody.AddForce(targentMove, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    void JumpManagement()
    {
        if(jump && !behaviourController.GetAnimator.GetBool(jumpBool) &&
            behaviourController.IsGrounded()) // 점프 가능
        {
            behaviourController.LockTempBehaviour(behaviourCode); // 이동 행동 잠금
            behaviourController.GetAnimator.SetBool(jumpBool, true);

            if(behaviourController.GetAnimator.GetFloat(speedFloat) > 0.1f) // 이동 중에 점프
            {
                // 마찰 없애기
                capsuleCollider.material.dynamicFriction = 0f;
                capsuleCollider.material.staticFriction = 0f;

                RemoveVecticalVelocity();
                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                behaviourController.GetRigidbody.AddForce(Vector3.up, ForceMode.VelocityChange);
            }
            
        }
        else if (behaviourController.GetAnimator.GetBool(jumpBool)) // 점프 중
        {
            if (!behaviourController.IsGrounded() && !isColliding &&
                behaviourController.GetTempLockStatus())
            {
                behaviourController.GetRigidbody.AddForce(myTransform.forward * jumpInertialForce
                    * Physics.gravity.magnitude * sprintSpeed, ForceMode.Acceleration);
            }
            if(behaviourController.GetRigidbody.velocity.y < 0f &&
                behaviourController.IsGrounded()) // 땅에 떨어진 순간
            {
                behaviourController.GetAnimator.SetBool(groundedBool, true);
                capsuleCollider.material.dynamicFriction = 0.6f; // 마찰력 복구
                capsuleCollider.material.staticFriction = 0.6f; // 마찰력 복구
                jump = false;
                behaviourController.GetAnimator.SetBool(jumpBool, false);
                behaviourController.UnLockTempBehaviour(this.behaviourCode);
            }
        }
    }


    private void Update()
    {
        if(!jump && Input.GetButtonDown(ButtonName.Jump) 
            && behaviourController.IsCurrentBehaviour(behaviourCode)
            && !behaviourController.IsOverriding())
        {
            jump = true;
        }
    }

    public override void LocalFixedUpdate()
    {

        MovementManagement(behaviourController.GetH, behaviourController.GetV);
        JumpManagement();
    }

}
