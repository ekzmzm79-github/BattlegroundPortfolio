using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//ScriptableObject 
//대량의 데이터를 저장하는 데 사용할 수 있는 데이터 컨테이너
//값의 사본이 생성되는 것을 방지
/// <summary>
/// 실제 행동을 하게 되는 컴포넌트
/// </summary>
public abstract class Action : ScriptableObject
{
    public abstract void Act(StateController controller);
    public virtual void OnReadyAction(StateController controller)
    {

    }
}
