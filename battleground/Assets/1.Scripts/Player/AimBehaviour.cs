using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 마우스 오른쪽버튼으로 조준 구현, 다른 동작보다 우선시 해서 동작함(overrideBehaviour)
/// 마우스 휠버튼으로 좌우 카메라 위치 변경(어깨 양쪽)
/// 벽의 모서리에서 조준할때 상체를 살짝 기울여주는 기능
/// </summary>
public class AimBehaviour : GenericBehaviour
{
    #region Variable
    public Texture2D crossHair; // 크로스 헤어 이미지
    public float aimTurnSmoothing = 0.15f; // 카메라를 향하도록 조준할때 회전속도
    public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0.0f); // 평소 카메라 오프셋
    public Vector3 aimCamOffset = new Vector3(0.0f, 0.4f, -0.7f); // 조준시 카메라 오프셋

    private int aimBool; // 애니메이터 조준 파라미터
    private bool aim; // 조준 중인가?
    private int cornerBool; // 애니메이터 코너 파라미터
    private bool peekCorner; // 플레이어가 코너 모서리에 있는지 여부
    
    //IK : Inverse Kinematics (역운동학 : 관절 -> 손 x, 손 -> 관절 o)
    private Vector3 initialRootRotation; // 루트 본 로컬 회전값
    private Vector3 initialHipRotation; 
    private Vector3 initialSpineRotation;
    private Transform myTransform;
    #endregion Variable

    #region Method
    

    private void Start()
    {
        myTransform = transform;
        //setup
        aimBool = Animator.StringToHash(FC.AnimatorKey.Aim);
        cornerBool = Animator.StringToHash(FC.AnimatorKey.Corner);

        //value
        Transform hips = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipRotation = hips.localEulerAngles;
        initialSpineRotation = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
    }

    //카메라에 따라 플레이어를 올바른 방향으로 회전
    void Rotating()
    {
        Vector3 forward = behaviourController.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        Quaternion targetRotation = Quaternion.Euler(0f, behaviourController.GetCamScript.GetH, 0f);
        float minSpeed = Quaternion.Angle(transform.rotation, targetRotation) * aimTurnSmoothing;

        if(peekCorner) // 모서리라면
        {
            //조중 중일때 플레이어 상체만 살짝 기울여 주는 작업
            // IK 방식이기 때문에 반대방향으로(-)
            myTransform.rotation = Quaternion.LookRotation(-behaviourController.GetLastDirection());
            targetRotation *= Quaternion.Euler(initialRootRotation);
            targetRotation *= Quaternion.Euler(initialHipRotation);
            targetRotation *= Quaternion.Euler(initialSpineRotation);
            Transform spine = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = targetRotation; // 카메라 현재 회전값으로 spine 회전값 세팅
        }
        else
        {
            behaviourController.SetLastDirection(forward);
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, minSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 조중 줄일때를 관리하는 함수
    /// </summary>
    void AimManagement()
    {
        Rotating();
    }

    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);
        //조준이 불가능한 상태일때에 대한 예외처리
        if(behaviourController.GetTempLockStatus(this.behaviourCode) ||
            behaviourController.IsOverriding(this))
        {
            // 현재 조준 불가 상태이거나 이미 조중 상태라면
            yield return false;
        }
        else
        {
            aim = true;
            int signal = 1;
            if(peekCorner)
            {
                signal = (int)Mathf.Sign(behaviourController.GetH);
            }

            //보간 작업
            aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
            aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
            yield return new WaitForSeconds(0.1f);

            behaviourController.GetAnimator.SetFloat(speedFloat, 0.0f); // 이동 멈춤(속도에 따라 이동하므로)
            behaviourController.OverrideWithBehaviour(this); // 조준 행동을 우선순위로 덮어씌움

        }
    }

    private IEnumerator ToggleAimOff()
    {
        aim = false;
        yield return new WaitForSeconds(0.3f);
        behaviourController.GetCamScript.ResetTargetOffsets(); // 카메라 리셋;
        behaviourController.GetCamScript.ResetMaxVerticalAngle();
        yield return new WaitForSeconds(0.1f);
        behaviourController.RevokeOverrideingBehaviour(this); // 조준 행동 취소
    }

    public override void LocalFixedUpdate()
    {
        if(aim) 
        {
            // 조중 중일때는 카메라의 오프셋 값이 조준용으로 변경됨
            behaviourController.GetCamScript.SetTargetOffset(aimPivotOffset, aimCamOffset);
        }
    }

    public override void LocalLateUpdate()
    {
        AimManagement();
    }

    private void Update()
    {
        peekCorner = behaviourController.GetAnimator.GetBool(cornerBool);

        /*
         * GetAxisRaw : 입력값이 무엇이든 무조건 -1, 0, 1 중 하나의 값을 반환
         * GetAxis : -1~1 사이의 float 값 반환
         * 
        */
        if (Input.GetAxisRaw(ButtonName.Aim) != 0 && !aim)
        {
            StartCoroutine(ToggleAimOn());
        }
        else if(aim && Input.GetAxisRaw(ButtonName.Aim) == 0)
        {
            StartCoroutine(ToggleAimOff());
        }

        //조중 중일때는 전력질주 불가
        canSprint = !aim;
        if(aim && Input.GetButtonDown(ButtonName.Shoulder) && !peekCorner)
        {
            //마우스 휠 버튼으로 숄더뷰 위치 변경
            aimCamOffset.x = aimCamOffset.x * (-1);
            aimPivotOffset.x = aimPivotOffset.x * (-1);

        }
        behaviourController.GetAnimator.SetBool(aimBool, aim);
    }

    private void OnGUI() // 크로스 헤어
    {
        if(crossHair != null)
        {
            float length = behaviourController.GetCamScript.GetCurrentPivotMagnitude(aimPivotOffset);
            if(length < 0.05f)
            {
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - (crossHair.width * 0.5f),
                    Screen.height * 0.5f - (crossHair.height * 0.5f),
                    crossHair.width, crossHair.height), crossHair);
            }
        }
    }

    #endregion Method
}
