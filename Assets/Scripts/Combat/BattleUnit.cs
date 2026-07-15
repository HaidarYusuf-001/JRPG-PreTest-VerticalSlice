using UnityEngine;
using System;
using System.Collections;

public class BattleUnit : MonoBehaviour
{
    public Animator UnitAnimator { get; private set; }

    [Header("Cinematic Targeting")]
    public Transform cameraTrackTarget; // Titik di mana kamera akan fokus (LookAt/Follow)

    private void Awake()
    {
        UnitAnimator = GetComponentInChildren<Animator>();
    }

    public void SetupForCombat()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void PerformAction(CombatAction actionToPerform, Transform target, Action onHit, Action onReturn)
    {
        if (actionToPerform != null)
        {
            StartCoroutine(actionToPerform.ExecuteAction(this, target, onHit, onReturn));
        }
        else
        {
            Debug.LogError("No Combat Action assigned to this unit!");
        }
    }
}