using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertChecker : MonoBehaviour
{
    [Range(0,50)] public float alertRadius; // 알림 범위
    public int extraWaves = 1; // 알림 횟수

    public LayerMask alertMask = FC.TagAndLayer.LayerMasking.Enemy;
    private Vector3 current;
    private bool alert;

    private void Start()
    {
        InvokeRepeating("PlngAlert", 1, 1);
    }

    private void AlertNearBy(Vector3 origin, Vector3 target, int wave =0)
    {
        if(wave > this.extraWaves)
        {
            return;
        }
        /*
         * 중점과 반지름으로 가상의 원을 만들어 
         * 추출하려는 반경 이내에 들어와 있는 콜라이더들을 반환하는 함수
         */
        Collider[] targetsInViewRadius = Physics.OverlapSphere(origin, alertRadius, alertMask);
        foreach(Collider collider in targetsInViewRadius)
        {
            /*
             * SendMessage : 해당 오브젝트의 모든 컴포넌트에서 해당 이름의 함수를 모두 호출
             * SendMessageUpwards : SendMessage + 모든 부모에게서도 검색해서 호출
             * BroadcastMessage : SendMessage + 모든 자손들까지 전부 검색해서 호출
             */
            collider.SendMessageUpwards("AlertCallback", target, SendMessageOptions.DontRequireReceiver);
            AlertNearBy(collider.transform.position, target, wave + 1); // 알림을 받은 적도 주변 적에게 알림 (wave + 1)
        }
    }

    public void RootAlertNearBy(Vector3 origin)
    {
        current = origin;
        alert = true;
    }

    void PlngAlert()
    {
        if(alert)
        {
            alert = false;
            AlertNearBy(current, current);
        }
    }
}
