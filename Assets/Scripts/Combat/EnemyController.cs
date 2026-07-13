using UnityEngine;
using System.Collections;
using System;

public class EnemyController : MonoBehaviour
{
    public float actionMovementSpeed = 8f;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ExecuteMeleeAttack(Transform target, float stopDistanceOffset, Action onHitCallback, Action onReturnCallback)
    {
        StartCoroutine(AttackRoutine(target, stopDistanceOffset, onHitCallback, onReturnCallback));
    }

    private IEnumerator AttackRoutine(Transform target, float stopDistanceOffset, Action onHitCallback, Action onReturnCallback)
    {
        Vector3 startPosition = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 targetPosition = target.position - (directionToTarget * stopDistanceOffset);

        if (animator != null) animator.SetBool("isRunning", true);

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, actionMovementSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null) animator.SetBool("isRunning", false);
        yield return new WaitForSeconds(0.5f);

        onHitCallback?.Invoke();
        yield return new WaitForSeconds(0.5f);

        if (animator != null) animator.SetBool("isRunning", true);

        Vector3 directionBack = (startPosition - transform.position).normalized;
        if (directionBack != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionBack);
        }

        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, actionMovementSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null) animator.SetBool("isRunning", false);
        transform.rotation = Quaternion.LookRotation(-directionBack);

        yield return new WaitForSeconds(0.5f);
        onReturnCallback?.Invoke();
    }
}