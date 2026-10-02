using UnityEngine;
using TMPro;

public class TopicUIManager : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject learnButton;

    [Header("Information Panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text infoTitle;
    [SerializeField] private TMP_Text infoText;

    [Header("Quiz Panel")]
    [SerializeField] private GameObject quizPanel;

    private string currentTopic = "Heart";

    private void Start()
    {
        infoPanel.SetActive(false);
        quizPanel.SetActive(false);

        title.SetActive(false);
        learnButton.SetActive(false);
    }

    public void ShowTopic(string topic)
    {
        currentTopic = topic;

        title.SetActive(true);
        learnButton.SetActive(true);

        infoPanel.SetActive(false);
        quizPanel.SetActive(false);

        TMP_Text titleText = title.GetComponent<TMP_Text>();
        TMP_Text buttonText = learnButton.GetComponentInChildren<TMP_Text>();

        switch (topic)
        {
            case "Heart":
                titleText.text = "HUMAN HEART";
                buttonText.text = "LEARN ABOUT HEART";
                break;

            case "Brain":
                titleText.text = "HUMAN BRAIN";
                buttonText.text = "LEARN ABOUT BRAIN";
                break;

            case "Lungs":
                titleText.text = "HUMAN LUNGS";
                buttonText.text = "LEARN ABOUT LUNGS";
                break;
        }
    }

    public void ShowInfo()
    {
        title.SetActive(false);
        learnButton.SetActive(false);

        infoPanel.SetActive(true);

        switch (currentTopic)
        {
            case "Heart":
                infoTitle.text = "HUMAN HEART";

                infoText.text =
                    "The human heart is a muscular organ " +
                    "that pumps blood throughout the body.\n\n" +
                    "Main Parts:\n\n" +
                    "• Right Atrium\n" +
                    "• Right Ventricle\n" +
                    "• Left Atrium\n" +
                    "• Left Ventricle\n" +
                    "• Aorta";
                break;

            case "Brain":
                infoTitle.text = "HUMAN BRAIN";

                infoText.text =
                    "The brain is the control center of " +
                    "the nervous system.\n\n" +
                    "Main Parts:\n\n" +
                    "• Cerebrum\n" +
                    "• Cerebellum\n" +
                    "• Brainstem\n" +
                    "• Frontal Lobe\n" +
                    "• Temporal Lobe";
                break;

            case "Lungs":
                infoTitle.text = "HUMAN LUNGS";

                infoText.text =
                    "The lungs are major organs of the " +
                    "respiratory system. They exchange " +
                    "oxygen and carbon dioxide.\n\n" +
                    "Main Parts:\n\n" +
                    "• Trachea\n" +
                    "• Bronchi\n" +
                    "• Left Lung\n" +
                    "• Right Lung\n" +
                    "• Alveoli";
                break;
        }
    }

    public void HideInfo()
    {
        infoPanel.SetActive(false);

        title.SetActive(true);
        learnButton.SetActive(true);
    }

    public string GetCurrentTopic()
    {
        return currentTopic;
    }
}