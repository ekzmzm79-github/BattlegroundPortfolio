using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 발자국 소리 출력 
/// </summary>
public class PlayerFootStep : MonoBehaviour
{
    #region Variable
    
    public SoundList[] stepSounds;
    private Animator myAnimator;
    private int index;
    private Transform leftFoot, rightFoot;
    private float dist;
    private int groundedBool, coverBool, aimBool, crouchFloat; //animator value
    private bool grounded;

    public enum Foot
    {
        LEFT,
        RIGHT,
    }
    private Foot step = Foot.LEFT;
    private float oldDist, maxDist = 0f; // 걷는 동안에 몇 프레임은 두 dist 값이 같다

    #endregion Variable

    #region Method

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        leftFoot = myAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = myAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
        groundedBool = Animator.StringToHash(FC.AnimatorKey.Grounded);
        coverBool = Animator.StringToHash(FC.AnimatorKey.Cover);
        aimBool = Animator.StringToHash(FC.AnimatorKey.Aim);
        crouchFloat = Animator.StringToHash(FC.AnimatorKey.Crouch);
    }

    private void PlayFootStep()
    {
        if(oldDist < maxDist)
        {
            return;
        }

        oldDist = maxDist = 0;
        int oldIndex = index;
        while(oldIndex == index)
        {
            //index를 랜덤하게
            index = Random.Range(0, stepSounds.Length);
        }

        //랜덤한 stepSounds[index] 재생
        SoundManager.Instance.PlayOneShotEffect((int)stepSounds[index], transform.position, 0.2f);
    }

    private void Update()
    {
        if(!grounded && myAnimator.GetBool(groundedBool))
        {
            PlayFootStep();
        }

        grounded = myAnimator.GetBool(groundedBool);

        float factor = 0.15f; // 임의의 거리값 -> 이거보다 작다면 footSound 재생
        if(grounded && myAnimator.velocity.magnitude > 1.6f) // 땅에 있고 움직이고 있다면
        {
            oldDist = maxDist;
            switch(step)
            {
                case Foot.LEFT:
                    //transform.position 은 발다닥을 기준으로 한다
                    dist = leftFoot.position.y - transform.position.y;
                    maxDist = dist > maxDist ? dist : maxDist;
                    if(dist <= factor)
                    {
                        PlayFootStep();
                        step = Foot.RIGHT;
                    }
                    break;
                case Foot.RIGHT:
                    dist = rightFoot.position.y - transform.position.y;
                    maxDist = dist > maxDist ? dist : maxDist;
                    if (dist <= factor)
                    {
                        PlayFootStep();
                        step = Foot.LEFT;
                    }
                    break;
            }
        }
    }

    #endregion Method

}
