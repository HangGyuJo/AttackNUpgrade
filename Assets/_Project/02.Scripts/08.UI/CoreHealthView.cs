using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoreHealthView : MonoBehaviour
{
    [SerializeField] private CoreHealth coreHealth;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    private void Start()
    {
        if (coreHealth == null)
        {
            coreHealth = CoreHealth.Instance;
        }

        if (hpSlider == null)
        {
            hpSlider = GameObject.Find("CoreHPSlider").GetComponent<Slider>();
        }

        if (hpText == null)
        {
            hpText = GameObject.Find("CoreHPText").GetComponent<TMP_Text>();
        }

        Refresh();
    }

    private void Update()
    {
        if (coreHealth == null)
        {
            coreHealth = CoreHealth.Instance;
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (coreHealth == null)
        {
            return;
        }

        int currentHp = coreHealth.CurrentHealth.Value;
        int maxHp = coreHealth.MaxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"Core HP: {currentHp} / {maxHp}";
        }
    }
}