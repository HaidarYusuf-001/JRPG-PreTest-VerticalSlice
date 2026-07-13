using UnityEngine;
using System;
using System.Collections;

public abstract class CombatAction : ScriptableObject
{
    public float attackDistanceOffset = 1.5f;
    public abstract IEnumerator ExecuteAction(BattleUnit caster, Transform target, Action onHitCallback, Action onReturnCallback);
}