using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public Button attackButton;
    private CombatManager activeCombatManager;

    public void InitializeUI(CombatManager combatManager, int playerHP, int enemyHP)
    {
        activeCombatManager = combatManager;
        UpdatePlayerHealth(playerHP);
        UpdateEnemyHealth(enemyHP);
        attackButton.onClick.AddListener(ExecutePlayerAttack);
        DisableAttackButton();
    }

    public void UpdatePlayerHealth(int health)
    {
        playerHealthText.text = "Player HP: " + health.ToString();
    }

    public void UpdateEnemyHealth(int health)
    {
        enemyHealthText.text = "Enemy HP: " + health.ToString();
    }

    public void EnableAttackButton()
    {
        attackButton.interactable = true;
    }

    public void DisableAttackButton()
    {
        attackButton.interactable = false;
    }

    private void ExecutePlayerAttack()
    {
        DisableAttackButton();
        activeCombatManager.ProcessPlayerTurn();
    }
}