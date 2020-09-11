using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 숨을 곳을 찾아주는 컴포넌트
/// 플레이어보다 멀리 있는 장애물은 제외
/// </summary>
public class CoverLookUp : MonoBehaviour
{
    #region Variable

    private List<Vector3[]> allCoverSpots;
    private GameObject[] covers;
    private List<int> coverHashCodes; //cover unity ID;
    
    private Dictionary<float, Vector3> filteredSpots; // 이미 필터링 된 Spot들

    #endregion Variable
    #region Method

    /// <summary>
    /// 모든 리소스 오브젝트를 검색해서 해당 레이어 마스크에 해당하는 오브젝트 리스트 반환
    /// (추후에 검색 방식 수정 될 예정)
    /// </summary>
    private GameObject[] GetObjectsInLayerMask(int layerMask)
    {
        List<GameObject> ret = new List<GameObject>();
        foreach(GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if(go.activeInHierarchy && layerMask == (layerMask | (1 << go.layer)))
            {
                ret.Add(go);
            }
        }

        return ret.ToArray();
    }

    private void ProcessPoint(List<Vector3> vector3s, Vector3 nativePointe, float range)
    {
        NavMeshHit hit;
        //SamplePosition : 현재위치에서 nativePointe 지점이 네비메쉬 샘플링 했을때 유효한 지점인가?
        if (NavMesh.SamplePosition(nativePointe, out hit, range, NavMesh.AllAreas))
        {
            vector3s.Add(hit.position);
        }
    }

    private Vector3[] GetSpots(GameObject go, LayerMask obstacleMask)
    {
        List<Vector3> bounds = new List<Vector3>();
        foreach (Collider col in go.GetComponents<Collider>())
        {
            float baseHeight = (col.bounds.center - col.bounds.extents).y;
            float range = 2 * col.bounds.extents.y;

            Vector3 deslocalForward = go.transform.forward * go.transform.localScale.z * 0.5f;
            Vector3 deslocalRight = go.transform.right * go.transform.localScale.x * 0.5f;

            if (go.GetComponent<MeshCollider>())
            {
                /*
                 * 해당 오브젝트에 MeshCollider 가 있다면,
                 * 장애물 매쉬가 정사각형, 직사각형 등 다양한 모양일 수 있으므로
                 * Raycast를 통해서 해당 매쉬 크기 및 모양에 맞게 Enemy가 도착할 지점에 대한
                 * deslocalForward, deslocalRight를 세팅하는 과정
                 */

                float maxBounds = go.GetComponent<MeshCollider>().bounds.extents.z +
                    go.GetComponent<MeshCollider>().bounds.extents.x;
                Vector3 originForward = col.bounds.center + go.transform.forward * maxBounds;
                Vector3 originRight = col.bounds.center + go.transform.right * maxBounds;
                if (Physics.Raycast(originForward, col.bounds.center - originForward, out RaycastHit hit,
                    maxBounds, obstacleMask))
                {
                    deslocalForward = hit.point - col.bounds.center;
                }
                if (Physics.Raycast(originRight, col.bounds.center - originRight, out hit, maxBounds,
                    obstacleMask))
                {
                    deslocalRight = hit.point - col.bounds.center;
                }
            }
            else if (Vector3.Equals(go.transform.localScale, Vector3.one))
            {
                //MeshCollider 는 없으며 (1,1,1) 짜리 scale이면
                deslocalForward = go.transform.forward * col.bounds.extents.z;
                deslocalRight = go.transform.right * col.bounds.extents.x;
            }

            // 목표물(Obstacle) 인근에 12개의 점을 NavMesh.SamplePosition를 통해서 유효한지 검출
            float edgeFactor = 0.75f;
            ProcessPoint(bounds, col.bounds.center + deslocalRight + deslocalForward * edgeFactor, range); // 0.75앞 오른쪽
            ProcessPoint(bounds, col.bounds.center + deslocalForward + deslocalRight * edgeFactor, range); // 앞 0.75오른쪽
            ProcessPoint(bounds, col.bounds.center + deslocalForward, range); // 중심 앞
            ProcessPoint(bounds, col.bounds.center + deslocalForward - deslocalRight * edgeFactor, range); // 앞 0.75왼쪽
            ProcessPoint(bounds, col.bounds.center - deslocalRight + deslocalForward * edgeFactor, range); // 0.75앞 왼쪽
            ProcessPoint(bounds, col.bounds.center + deslocalRight, range); // 중심 오른쪽
            ProcessPoint(bounds, col.bounds.center + deslocalRight - deslocalForward * edgeFactor, range); // 0.75뒤 오른쪽
            ProcessPoint(bounds, col.bounds.center - deslocalForward + deslocalRight * edgeFactor, range); // 뒤 0.75오른쪽
            ProcessPoint(bounds, col.bounds.center - deslocalForward, range); // 중심 뒤
            ProcessPoint(bounds, col.bounds.center - deslocalForward - deslocalRight * edgeFactor, range); // 뒤 0.75왼쪽
            ProcessPoint(bounds, col.bounds.center - deslocalRight - deslocalForward * edgeFactor, range); // 0.75뒤 왼쪽
            ProcessPoint(bounds, col.bounds.center - deslocalRight, range); // 중심 왼쪽
        }
        return bounds.ToArray();
    }

    public void Setup(LayerMask coverMask)
    {
        covers = GetObjectsInLayerMask(coverMask);

        coverHashCodes = new List<int>();
        allCoverSpots = new List<Vector3[]>();
        foreach(GameObject cover in covers)
        {
            allCoverSpots.Add(GetSpots(cover, coverMask));
            coverHashCodes.Add(cover.GetHashCode());
        }
    }

    /// <summary>
    /// 목표물이 경로에 있는지 확인하는 역활
    /// 대상이 각도 안에 있고 지점보다 가까이 있는가?
    /// -> Enemy가 스스로 Cover를 찾을 때, Player 인근 Cover는 무시하기 위해서
    /// </summary>
    private bool TargetInPath(Vector3 origin, Vector3 spot, Vector3 target, float angle)
    {
        Vector3 dirToTarget = (target - origin).normalized; // 타겟으로 향하는 벡터
        Vector3 dirToSpot = (spot - origin).normalized; // 스팟으로 향하는 벡터

        if(Vector3.Angle(dirToSpot, dirToTarget) <= angle) // 두 벡터 사이의 각도가 angle보다 작냐?
        {
            float targetDist = (target - origin).sqrMagnitude; // == Vector3.Distance()
            float spotDist = (spot - origin).sqrMagnitude;
            return targetDist <= spotDist; // 타겟이 스팟보다 가깝거나 같다면 true
        }

        return false;
    }

    /// <summary>
    /// 가장 가까운 유효한 cover 지점을 거리와 함께 찾아서 리턴
    /// (필터 스팟은 제외시킴)
    /// </summary>
    private ArrayList FilterSpots(StateController controller)
    {
        float minDist = Mathf.Infinity;
        filteredSpots = new Dictionary<float, Vector3>();
        int nextCoverHash = -1;
        for (int i = 0; i < allCoverSpots.Count; i++)
        {
            if (!covers[i].activeSelf || coverHashCodes[i] == controller.coverHash)
            {
                // false 상태인 cover이거나 이미 coverHashCodes에 추가한 것이라면
                continue;
            }
            foreach(Vector3 spot in allCoverSpots[i])
            {
                Vector3 vectorDist = controller.personalTarget - spot; //spot -> player로 향하는 벡터
                float searchDist = (controller.transform.position - spot).sqrMagnitude; // spot에서 Enemy까지의 거리

                /* 
                 * vectorDist 크기(spot -> player 벡터 크기)가 보이는 범위 크기보다 작거나 같고
                 * spot에서 vectorDist 방향으로 Raycast를 쐈을때 또다른 cover가 hit 되었다면
                 */
                if (vectorDist.sqrMagnitude <= controller.viewRadius * controller.viewRadius &&
                    Physics.Raycast(spot, vectorDist, out RaycastHit hit, vectorDist.sqrMagnitude,
                    controller.generalStats.coverMask))
                {
                    /*
                     * hit 콜라이더가 covers[i]와 같고
                     * Enemy 시야의 1/4 각도 안에서 타겟이 스팟보다 가까이 있지 않다면
                     */
                    if (hit.collider == covers[i].GetComponent<Collider>() &&
                       !TargetInPath(controller.transform.position, spot, controller.personalTarget,
                       controller.viewAngle / 4))
                    {

                        if (!filteredSpots.ContainsKey(searchDist))
                        {
                            // filteredSpots에 searchDist에 해당하는 Spot이 없다면
                            filteredSpots.Add(searchDist, spot);
                        }
                        else
                        {
                            continue;
                        }


                        if(minDist > searchDist)
                        {
                            searchDist = minDist;
                            nextCoverHash = coverHashCodes[i];
                        }

                    }

                }
            }
        }

        /*
         * 결론적으로 nextCoverHash, minDist 는 
         * 현재 Enemy가 Player에서 가장 가까운 Cover 정보를 포함
         */

        ArrayList returnArray = new ArrayList();
        returnArray.Add(nextCoverHash);
        returnArray.Add(minDist);
        return returnArray;

    }

    public ArrayList GetBestCoverSpot(StateController controller)
    {
        ArrayList nextCoverData = FilterSpots(controller);
        int nextCoverHash = (int)nextCoverData[0];
        float minDist = (float)nextCoverData[1];

        ArrayList returnArray = new ArrayList();
        if(filteredSpots.Count == 0)
        {
            //cover를 하나도 못 찾았다는 뜻이므로 무시할 값을 add
            returnArray.Add(-1);
            returnArray.Add(Vector3.positiveInfinity);
        }
        else
        {
            // nextCoverHash, filteredSpots[minDist]가 
            // 현 Enemy가 봤을때, BestCoverSpot에 해당함
            returnArray.Add(nextCoverHash);
            returnArray.Add(filteredSpots[minDist]);
        }

        return returnArray;
    }

    #endregion Method


}
