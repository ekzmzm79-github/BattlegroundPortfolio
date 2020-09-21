using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Actions/Reload")]
public class ReloadAction : Action
{

    public override void OnReadyAction(StateController controller)
    {

    }

    public override void Act(StateController controller)
    {
        if(!controller.reloading && controller.bullets <= 0)
        {
            // 현재 재정전 중이 아니고 소유 총알 없다면
            controller.enemyAnimation.anim.SetTrigger(FC.AnimatorKey.Reload); // 재장전 애니메이션
            controller.reloading = true;
            SoundManager.Instance.PlayOneShotEffect((int)SoundList.reloadWeapon,
                controller.enemyAnimation.gunMuzzle.position, 2f);
        }
    }
}
