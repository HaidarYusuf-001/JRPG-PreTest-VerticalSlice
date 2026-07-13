using UnityEngine;
using System;
using System.Collections;

[CreateAssetMenu(fileName = "BasicMeleeAction", menuName = "Combat/Actions/Basic Melee")]
public class BasicMeleeAction : CombatAction
{
    public float movementSpeed = 8f;

    public override IEnumerator ExecuteAction(BattleUnit caster, Transform target, Action onHitCallback, Action onReturnCallback)
    {
        Vector3 startPosition = caster.transform.position;
        Vector3 directionToTarget = (target.position - caster.transform.position).normalized;
        Vector3 targetPosition = target.position - (directionToTarget * attackDistanceOffset);

        Animator animator = caster.UnitAnimator;

        if (animator != null) animator.SetBool("isAttacking", true);

        // Maju Lurus
        while (Vector3.Distance(caster.transform.position, targetPosition) > 0.1f)
        {
            caster.transform.position = Vector3.MoveTowards(caster.transform.position, targetPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null)
        {
            animator.SetBool("isAttacking", false);
            animator.SetTrigger("attackTrigger");
        }

        yield return new WaitForSeconds(0.5f);
        onHitCallback?.Invoke();
        yield return new WaitForSeconds(0.5f);

        // Mundur Lurus
        if (animator != null) animator.SetBool("isAttacking", true);

        Vector3 directionBack = (startPosition - caster.transform.position).normalized;
        if (directionBack != Vector3.zero) caster.transform.rotation = Quaternion.LookRotation(directionBack);

        while (Vector3.Distance(caster.transform.position, startPosition) > 0.1f)
        {
            caster.transform.position = Vector3.MoveTowards(caster.transform.position, startPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null) animator.SetBool("isAttacking", false);
        caster.transform.rotation = Quaternion.LookRotation(-directionBack);

        yield return new WaitForSeconds(0.5f);
        onReturnCallback?.Invoke();
    }
}