using UnityEngine;
using System;
using System.Collections;

[CreateAssetMenu(fileName = "StationaryAction", menuName = "Combat/Actions/Stationary (Cast-Throw)")]
public class StationaryAction : CombatAction
{
    public override IEnumerator ExecuteAction(BattleUnit caster, Transform target, Action onHitCallback, Action onReturnCallback)
    {
        Animator animator = caster.UnitAnimator;

        if (animator != null) animator.SetTrigger("castTrigger");

        yield return new WaitForSeconds(1f);
        onHitCallback?.Invoke();
        yield return new WaitForSeconds(0.5f);

        onReturnCallback?.Invoke();
    }
}