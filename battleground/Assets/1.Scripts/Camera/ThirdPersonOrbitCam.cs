using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//카메라 속성 중 중요 속성 하나는 카메라로부터 오프셋 벡터, 피봇 오프셋 벡터
//위치 오프셋 벡터는 충돌 처리용으로 사용하고 피봇 오프셋 벡터는 시선이동에 사용
//충돌 체크 : 이중 충돌 체크 기능( 캐릭터로부터 카메라, 카메라로부터 캐릭터 사이)
//사격 반동을 위한 기능, FOV(시야각) 변경 기능
[RequireComponent(typeof(Camera))]
public class ThirdPersonOrbitCam : MonoBehaviour
{
    #region Variable
    public Transform player;
    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    public float smooth = 10f; //카메라 반응 속도
    public float horizontalAimingSpeed = 6.0f; // 수평 회전 속도
    public float verticalAimingSpeed = 6.0f; // 수직 회전 속도
    public float maxVerticalAngle = 30.0f; // 카메라 수직 회전 최대값(각도)
    public float minVerticalAngle = -60.0f; // 카메라 수직 회전 최소값(각도)
    public float recoilAngleBounce = 5.0f; // 사격 반동 바운스 값( 사격 반동이 돌아오는 수치)

    private float angleH = 0.0f; // 마우스 이동에 따른 카메라 수평 이동 수치
    public float GetH
    {
        get
        {

            return angleH;
        }
    }

    private float angleV = 0.0f; // 마우스 이동에 따른 카메라 수직 이동 수치
    private Transform cameraTransform; // 카메라 트랜스폼 캐싱용
    private Camera myCamer;
    private Vector3 relCameraPos; // 플레이어로부터 카메라까지의 벡터 
    private float relCameraPosMag; // 플레이어로부터 카메라 사이의 거리
    private Vector3 smoothPivotOffset; // 카메라 피봇 보간용 벡터
    private Vector3 smoothCamOffset; // 카메라 위치 보간용 벡터
    private Vector3 targetPivotOffset; // 카메라 피봇 보간용 벡터
    private Vector3 targetCamOffset; // 카메라 위치 보간용 벡터
    private float defaultFOV; // 기본 시야값
    private float targetFOV; // 타겟 시야값
    private float targetMaxVerticalAngle; // 반동값으로 인한 카메라 수직 최대 각도
    private float recoilAngle = 0.0f; // 총기 반동의 정도를 나타내는 각도

    #endregion Variable

    #region Method
    private void Awake()
    {
        //캐싱
        cameraTransform = transform;
        myCamer = cameraTransform.GetComponent<Camera>();

        //카메라 기본 포지션 세팅: pivotOffset-> 약간 위, camOffset -> 어깨 뒤
        cameraTransform.position = player.position + Quaternion.identity * pivotOffset +
            Quaternion.identity * camOffset;
        cameraTransform.rotation = Quaternion.identity;

        // 카메라와 플레이어간의 상대 벡터 -> 충돌 체크에 사용
        relCameraPos = cameraTransform.position - player.position; //플레이어 -> 카메라 향하는 벡터
        relCameraPosMag = relCameraPos.magnitude - 0.5f; // 플레이어 자체는 충돌에서 제외(-0.5f)

        //기본 세팅
        smoothPivotOffset = pivotOffset;
        smoothCamOffset = camOffset;
        defaultFOV = myCamer.fieldOfView;
        angleH = player.eulerAngles.y;

        ResetTargetOffsets();
        ResetFOV();
        ResetMaxVerticalAngle();
    }

    public void ResetTargetOffsets()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }

    public void ResetFOV()
    {
        targetFOV = defaultFOV;
    }

    public void ResetMaxVerticalAngle()
    {
        targetMaxVerticalAngle = maxVerticalAngle;
    }

    public void BounceVertical(float degree)
    {
        recoilAngle = degree;
    }
    public void SetTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }
    public void SetFOV(float customFOV)
    {
        targetFOV = customFOV;
    }

    /*  카메라 -> 플레이어 충돌 체크
     *  플레이어 -> 카메라 충돌 체크
     *  더블 체크로 카메라와 플레이어 사이에 오브젝트 충돌을 체크한다
    */

    //플레이어의 기본 위치는 발바닥에 해당하기 때문에 플레이어의 높이 값을 따로 받는다
    /// <summary>
    /// 카메라 -> 플레이어 충돌 체크
    /// 플레이어의 기본 위치는 발바닥에 해당하기 때문에 플레이어의 높이 값을 따로 받는다
    /// </summary>
    bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
    {
        Vector3 target = player.position + (Vector3.up * deltaPlayerHeight);

        /*
         * Physics.Cast : 콜라이더 컴포넌트 없이, 매 프레임이 아니라 원하는 순간에만 콜라이더 충돌 검출 가능
         * SphereCast 목표지점으로 해당 반지름 크기의 원을 발사해서 충돌 체크
        */
        if (Physics.SphereCast(checkPos, 0.2f, target - checkPos, out RaycastHit hit, relCameraPosMag))
        {
            
            if(hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                // 플레이어가 아니고 트리거 콜라이더도 아니다
                return false; //충돌체가 존재한다.
            } 
        }

        return true; // 충돌체가 없다.
    }

    /// <summary>
    /// 플레이어 -> 카메라 충돌 체크
    /// </summary>
    bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight, float maxDistance)
    {
        Vector3 origin = player.position + (Vector3.up * deltaPlayerHeight);

        
        if (Physics.SphereCast(origin, 0.2f, checkPos - origin, out RaycastHit hit, maxDistance))
        {

            if (hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                // 플레이어가 아니고 자기자신도 아니며 트리거 콜라이더도 아니다
                return false; //충돌체가 존재한다.
            }
        }

        return true; // 충돌체가 없다.
    }

    bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
    {
        float playerFocusHeight = player.GetComponent<CapsuleCollider>().height * 0.75f;

        //더블 체크로 둘다 true가 리턴되어야 문제가 없다
        bool reCheck = ViewingPosCheck(checkPos, playerFocusHeight) 
            && ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);

        return reCheck;
    }

    private void Update()
    {
        //마우스 이동 값
        //Clamp(,-1.0f, 1.0f) : -1.0f, 1.0f 사이값만 받음
        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1.0f, 1.0f) * horizontalAimingSpeed;
        angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1.0f, 1.0f) * verticalAimingSpeed;

        //수직 이동 제한
        angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);
        //수직 카메라 바운스. 
        // LerpAngle(flat a, float b, float t) : t시간 동안 a부터 b까지 변경되는 각도를 반환하는 보간함수
        angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10.0f * Time.deltaTime);

        //카메라 회전
        Quaternion camYRotation = Quaternion.Euler(0.0f, angleH, 0.0f);
        Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0.0f);
        cameraTransform.rotation = aimRotation;

        //set FOV
        myCamer.fieldOfView = Mathf.Lerp(myCamer.fieldOfView, targetFOV, Time.deltaTime);

        Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
        //조준시 카메라의 오프셋 값(조준할때와 평소시와는 다르다)
        // 카메라와 플레이어 사이에 충돌체가 있다면 카메라는 충돌체 바로 앞까지만 이동함
        // 그게 아니라면 원래 오프셋만큼 이동
        Vector3 noCollisionOffset = targetCamOffset;

        for (float zOffset = targetCamOffset.z; zOffset <= 0f; zOffset += 0.5f)
        {
            noCollisionOffset.z = zOffset;
            if (zOffset == 0 || DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset, Mathf.Abs(zOffset)))
            {
                break;
            }
        }

        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);
        smoothCamOffset = Vector3.Lerp(smoothCamOffset, noCollisionOffset, smooth * Time.deltaTime);

        cameraTransform.position = player.position + camYRotation * smoothPivotOffset
            + aimRotation * smoothCamOffset;

        // recoilAngle의 복귀(총기 발사후 올라가거나 내려간뒤의 역방향 반동)
        if (recoilAngle > 0.0f)
        {
            recoilAngle -= recoilAngleBounce * Time.deltaTime;
        }
        else if(recoilAngle < 0.0f)
        {
            recoilAngle += recoilAngleBounce * Time.deltaTime;
        }
    }

    /// <summary>
    /// smoothPivotOffset -> finalPivotOffset 으로 향하는 벡터의 크기 반환
    /// </summary>
    public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
    }

    #endregion Method

}
