using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSelectionView : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button[] abilityButtons;
    [SerializeField] private TMP_Text[] abilityTexts;
    [SerializeField] private TMP_Text statusText;

    private int[] currentChoices;

    private void Start()
    {
        SetVisible(false);
        BindButtons();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void BindButtons()
    {
        for (int i = 0; i < abilityButtons.Length; i++)
        {
            int slotIndex = i;

            abilityButtons[i].onClick.RemoveAllListeners();
            abilityButtons[i].onClick.AddListener(() => Select(slotIndex));
        }
    }

    private void Subscribe()
    {
        if (RewardSelectionManager.Instance == null)
        {
            return;
        }

        RewardSelectionManager.Instance.ChoicesReceived += OnChoicesReceived;
        RewardSelectionManager.Instance.ChoiceConfirmed += OnChoiceConfirmed;
        RewardSelectionManager.Instance.SelectionClosed += OnSelectionClosed;
    }

    private void Unsubscribe()
    {
        if (RewardSelectionManager.Instance == null)
        {
            return;
        }

        RewardSelectionManager.Instance.ChoicesReceived -= OnChoicesReceived;
        RewardSelectionManager.Instance.ChoiceConfirmed -= OnChoiceConfirmed;
        RewardSelectionManager.Instance.SelectionClosed -= OnSelectionClosed;
    }

    private void OnChoicesReceived(int[] choices)
    {
        currentChoices = choices;

        SetVisible(true);
        SetButtonsInteractable(true);

        if (statusText != null)
        {
            statusText.text = "능력을 하나 선택하세요.";
        }

        for (int i = 0; i < abilityButtons.Length; i++)
        {
            if (i >= choices.Length)
            {
                abilityButtons[i].gameObject.SetActive(false);
                continue;
            }

            abilityButtons[i].gameObject.SetActive(true);

            AbilityData ability = RewardSelectionManager.Instance.GetAbility(choices[i]);

            if (abilityTexts[i] != null)
            {
                abilityTexts[i].text = ability == null
                    ? "Unknown Ability"
                    : $"{ability.DisplayName}\n{ability.Description}";
            }
        }
    }

    private void Select(int slotIndex)
    {
        if (currentChoices == null)
        {
            return;
        }

        if (slotIndex < 0 || slotIndex >= currentChoices.Length)
        {
            return;
        }

        SetButtonsInteractable(false);

        if (statusText != null)
        {
            statusText.text = "선택 처리 중...";
        }

        RewardSelectionManager.Instance.SelectChoice(slotIndex);
    }

    private void OnChoiceConfirmed()
    {
        SetButtonsInteractable(false);

        if (statusText != null)
        {
            statusText.text = "선택 완료. 다른 플레이어를 기다리는 중...";
        }
    }

    private void OnSelectionClosed()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (Button button in abilityButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}