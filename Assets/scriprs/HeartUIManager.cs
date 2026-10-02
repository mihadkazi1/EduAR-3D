using UnityEngine;

public class HeartUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject infoPanel;

    [Header("Main UI")]
    public GameObject title;
    public GameObject description;
    public GameObject learnButton;

    public void ShowInfo()
    {
        // Hide the main UI
        title.SetActive(false);
        description.SetActive(false);
        learnButton.SetActive(false);

        // Show information panel
        infoPanel.SetActive(true);
    }

    public void HideInfo()
    {
        // Hide information panel
        infoPanel.SetActive(false);

        // Show the main UI again
        title.SetActive(true);
        description.SetActive(true);
        learnButton.SetActive(true);
    }
}