using UnityEngine;
using System;
using System.Collections;

[CreateAssetMenu(fileName = "JumpAttackAction", menuName = "Combat/Actions/Jump Attack")]
public class JumpAttackAction : CombatAction
{
    public float movementSpeed = 8f;
    public float jumpHeight = 3f; // Atur seberapa tinggi dia melompat di Inspector

    public override IEnumerator ExecuteAction(BattleUnit caster, Transform target, Action onHitCallback, Action onReturnCallback)
    {
        Vector3 startPosition = caster.transform.position;
        Vector3 directionToTarget = (target.position - caster.transform.position).normalized;
        Vector3 targetPosition = target.position - (directionToTarget * attackDistanceOffset);

        Animator animator = caster.UnitAnimator;

        // Memicu animasi Jump Up
        if (animator != null) animator.SetTrigger("jumpTrigger");

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        float fractionOfJourney = 0f;

        // Maju dengan rumus Parabola (Melompat)
        while (fractionOfJourney < 1f)
        {
            float distCovered = (Time.time - startTime) * movementSpeed;
            fractionOfJourney = distCovered / journeyLength;

            if (fractionOfJourney > 1f) fractionOfJourney = 1f;

            // Hitung posisi linear X dan Z
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            // Tambahkan lengkungan (Arc) pada sumbu Y menggunakan Sinus
            currentPos.y += jumpHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);

            caster.transform.position = currentPos;
            yield return null;
        }

        // Animasi serang saat mendarat
        if (animator != null) animator.SetTrigger("attackTrigger");

        yield return new WaitForSeconds(0.5f);
        onHitCallback?.Invoke();
        yield return new WaitForSeconds(0.5f);

        // Kembali dengan jalan kaki (sesuai request Anda, menggunakan animasi jalan)
        if (animator != null) animator.SetBool("isAttacking", true); // Dianggap bool jalan

        Vector3 directionBack = (startPosition - caster.transform.position).normalized;
        if (directionBack != Vector3.zero) caster.transform.rotation = Quaternion.LookRotation(directionBack);

        while (Vector3.Distance(caster.transform.position, startPosition) > 0.1f)
        {
            // Kembali secara lurus seperti biasa
            caster.transform.position = Vector3.MoveTowards(caster.transform.position, startPosition, movementSpeed * Time.deltaTime);
            yield return null;
        }

        if (animator != null) animator.SetBool("isAttacking", false);
        caster.transform.rotation = Quaternion.LookRotation(-directionBack);

        yield return new WaitForSeconds(0.5f);
        onReturnCallback?.Invoke();
    }
}