using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public UnitData playerUnitData;
    public UnitData pendingEnemyData;

    public Vector3 savedPlayerPosition;
    public Quaternion savedPlayerRotation;

    public bool isReturningFromBattle;
    public bool isRandomEncounter;
    public string lastInteractedNpcID;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}