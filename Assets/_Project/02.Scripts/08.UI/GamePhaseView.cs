using TMPro;
using UnityEngine;

public class GamePhaseView : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text resultText;

    private void Awake()
    {
        roundText = GameObject.Find("RoundText").GetComponent<TMP_Text>();
        phaseText = GameObject.Find("PhaseText").GetComponent<TMP_Text>();
        timeText = GameObject.Find("TimeText").GetComponent<TMP_Text>();
        resultText = GameObject.Find("ResultText").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        NetworkGamePhaseManager phaseManager = NetworkGamePhaseManager.Instance;

        if (phaseManager == null)
        {
            roundText.text = "Round: -";
            phaseText.text = "Phase: Waiting";
            timeText.text = "Time: -";

            if (resultText != null)
            {
                resultText.text = "";
            }

            return;
        }

        roundText.text = $"Round: {phaseManager.CurrentRound.Value}";
        phaseText.text = $"Phase: {phaseManager.CurrentPhase.Value}";
        timeText.text = $"Time: {phaseManager.RemainingTime.Value:0.0}";

        if (resultText != null)
        {
            GameResult result = phaseManager.CurrentResult.Value;
            resultText.text = result == GameResult.None
                ? ""
                : $"Result: {result}";
        }
    }
}