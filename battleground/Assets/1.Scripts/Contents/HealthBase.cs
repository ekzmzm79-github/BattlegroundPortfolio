using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerHealth, AnemeyHealth class가 상속받는 기본 클래스
/// </summary>
public class HealthBase : MonoBehaviour
{
    public class DamageInfo
    {
        #region Variable
        public Vector3 location, direction; // 피격 위치와 방향
        public float damage;
        public Collider bodyPart; // 특정 파트에 대한 특수 데미지 처리 예비용
        public GameObject origin; // 피격 이펙트 생성용 오브젝트

        public DamageInfo(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
        {
            this.location = location;
            this.direction = direction;
            this.damage = damage;
            this.bodyPart = bodyPart;
            this.origin = origin;
        }

        #endregion Variable
    }

    //[HideInInspector]  : public 이지만 인스펙터에 노출되지 않음
    [HideInInspector] public bool IsDead;
    protected Animator myAnimator;

    public virtual void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {

    }


    public void HitCallBack(DamageInfo damageInfo)
    {
        this.TakeDamage(damageInfo.location, damageInfo.direction, damageInfo.damage, damageInfo.bodyPart, damageInfo.origin);
    }
}
