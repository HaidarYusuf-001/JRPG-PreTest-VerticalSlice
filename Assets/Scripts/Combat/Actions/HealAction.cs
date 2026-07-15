using UnityEngine;
using System;
using System.Collections;

[CreateAssetMenu(fileName = "HealAction", menuName = "Combat/Actions/Heal")]
public class HealAction : CombatAction
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