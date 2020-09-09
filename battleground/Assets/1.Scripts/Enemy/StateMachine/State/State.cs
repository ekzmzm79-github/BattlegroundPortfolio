using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject
{
    public Action[] actions;
    public Transition[] transitions;
    public Color sceneGizmoColor = Color.gray;

    public void DoAction(StateController controller)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(controller);
        }
    }

    /// <summary>
    /// State가 바뀌는 순간 호출되는 메소드
    /// 바뀐 State가 가지고 있는 모든 actions를 Ready시키고 transitions 내부의 decision 활성화
    /// </summary>
    public void OnEnableActions(StateController controller)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].OnReadyAction(controller);
        }
        for (int i = transitions.Length - 1; i >= 0; i--)
        {
            transitions[i].decision.OnEnableDecision(controller);
        }
    }

    public void CheckTransitions(StateController controller)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            bool decision = transitions[i].decision.Decide(controller);
            if(decision) // decision의 결과 값에 따라 nextState 분기
            {
                controller.TransitionToState(transitions[i].trueState, transitions[i].decision);
            }
            else
            {
                controller.TransitionToState(transitions[i].falseState, transitions[i].decision);
            }

            if(controller.currentState != this) 
            {
                // 위 결과로 바뀐 currentState가 현재 this가 아니라면
                // State가 교체되었다는 뜻
                controller.currentState.OnEnableActions(controller);
                break;
            }
        }
    }

}
