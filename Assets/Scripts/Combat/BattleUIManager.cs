using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    private CombatManager combatManager;

    [Header("Main Panels")]
    public GameObject mainActionPanel;
    public GameObject skillPanel;
    public GameObject itemPanel;

    [Header("Main Buttons")]
    public Button attackBtn;
    public Button skillBtn;
    public Button itemBtn;
    public Button fleeBtn;

    [Header("Dynamic Buttons")]
    public Button[] skillButtons;
    public Button[] itemButtons;
    public Button backSkillBtn;
    public Button backItemBtn;

    [Header("HUD")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerMPText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI combatLogText;

    public void InitializeUI(CombatManager manager)
    {
        combatManager = manager;

        attackBtn.onClick.AddListener(() => combatManager.ExecutePlayerAttack());
        skillBtn.onClick.AddListener(ShowSkillPanel);
        itemBtn.onClick.AddListener(ShowItemPanel);
        fleeBtn.onClick.AddListener(() => combatManager.ExecuteFlee());

        backSkillBtn.onClick.AddListener(ShowMainPanel);
        backItemBtn.onClick.AddListener(ShowMainPanel);

        DisableAllInput();
        ShowMainPanel();
    }

    public void UpdateHUD(int pHP, int pMaxHP, int pMP, int pMaxMP, int eHP, int eMaxHP)
    {
        playerHPText.text = $"HP: {pHP}/{pMaxHP}";
        playerMPText.text = $"MP: {pMP}/{pMaxMP}";
        enemyHPText.text = $"Enemy HP: {Mathf.Max(0, eHP)}/{eMaxHP}";
    }

    public void ShowMessage(string msg)
    {
        combatLogText.text = msg;
    }

    public void ShowMainPanel()
    {
        mainActionPanel.SetActive(true);
        skillPanel.SetActive(false);
        itemPanel.SetActive(false);
    }

    private void ShowSkillPanel()
    {
        mainActionPanel.SetActive(false);
        skillPanel.SetActive(true);
        combatManager.PopulateSkillUI();
    }

    private void ShowItemPanel()
    {
        mainActionPanel.SetActive(false);
        itemPanel.SetActive(true);
        combatManager.PopulateItemUI();
    }

    public void EnableInput() { mainActionPanel.GetComponent<CanvasGroup>().interactable = true; }
    public void DisableAllInput() { mainActionPanel.GetComponent<CanvasGroup>().interactable = false; }
}