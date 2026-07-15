using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Player Progression")]
    public UnitData playerUnitData;
    public int playerLevel = 1;
    public int currentExp = 0;
    public int playerMaxHP;
    public int playerMaxMP;
    public int playerCurrentHP;
    public int playerCurrentMP;
    public int playerBaseAttack;

    [Header("State Persistence")]
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
            InitializePlayerData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePlayerData()
    {
        if (playerUnitData != null)
        {
            playerLevel = playerUnitData.level;
            playerMaxHP = playerUnitData.maxHealth;
            playerMaxMP = playerUnitData.maxMana;
            playerBaseAttack = playerUnitData.attackPower;
            playerCurrentHP = playerMaxHP;
            playerCurrentMP = playerMaxMP;
        }
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;
        int expNeeded = playerLevel * 50;

        while (currentExp >= expNeeded)
        {
            currentExp -= expNeeded;
            playerLevel++;
            playerMaxHP += 15;
            playerMaxMP += 10;
            playerBaseAttack += 3;
            playerCurrentHP = playerMaxHP;
            playerCurrentMP = playerMaxMP;
            expNeeded = playerLevel * 50;
            Debug.Log("Level Up! Current Level: " + playerLevel);
        }
    }
}