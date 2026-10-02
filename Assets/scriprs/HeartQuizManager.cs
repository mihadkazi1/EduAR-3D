using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeartQuizManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject quizPanel;

    [Header("Quiz UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Answer Buttons")]
    [SerializeField] private Button optionA;
    [SerializeField] private Button optionB;
    [SerializeField] private Button optionC;
    [SerializeField] private Button optionD;
    [SerializeField] private Button nextButton;

    [Header("Answer Text")]
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;
    [SerializeField] private TMP_Text optionCText;
    [SerializeField] private TMP_Text optionDText;

    [Header("Topic Manager")]
    [SerializeField] private TopicUIManager topicUIManager;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField]
    private Color correctColor =
        new Color(0.2f, 0.8f, 0.3f);
    [SerializeField]
    private Color wrongColor =
        new Color(0.9f, 0.2f, 0.2f);

    private string[] questions;

    private string[,] answers;

    private int[] correctAnswers;

    private int currentQuestion;
    private int score;
    private bool answerSelected;

    private readonly string[] heartQuestions =
    {
        "Which chamber pumps oxygenated blood to the body?",
        "Which organ pumps blood throughout the body?",
        "Which blood vessel carries oxygen-rich blood from the heart?",
        "How many chambers does the human heart have?",
        "Which side of the heart receives deoxygenated blood?"
    };

    private readonly string[,] heartAnswers =
    {
        { "Right Atrium", "Right Ventricle", "Left Atrium", "Left Ventricle" },
        { "Brain", "Heart", "Lungs", "Kidney" },
        { "Aorta", "Vena Cava", "Pulmonary Vein", "Pulmonary Artery" },
        { "2", "3", "4", "5" },
        { "Left Side", "Right Side", "Both Sides", "Neither Side" }
    };

    private readonly int[] heartCorrect = { 3, 1, 0, 2, 1 };

    private readonly string[] brainQuestions =
    {
        "What is the main control center of the nervous system?",
        "Which part of the brain controls balance and coordination?",
        "What is the largest part of the brain?",
        "Which structure connects the brain to the spinal cord?",
        "Which part is strongly associated with memory?"
    };

    private readonly string[,] brainAnswers =
    {
        { "Heart", "Brain", "Lungs", "Kidney" },
        { "Cerebrum", "Cerebellum", "Brainstem", "Medulla" },
        { "Cerebellum", "Brainstem", "Cerebrum", "Medulla" },
        { "Cerebrum", "Cerebellum", "Brainstem", "Hippocampus" },
        { "Medulla", "Hippocampus", "Cerebellum", "Brainstem" }
    };

    private readonly int[] brainCorrect = { 1, 1, 2, 2, 1 };

    private readonly string[] lungsQuestions =
    {
        "What is the main function of the lungs?",
        "Which tube carries air from the throat toward the lungs?",
        "Where does gas exchange mainly occur?",
        "How many lungs does a healthy person normally have?",
        "Which blood gas is taken into the body during breathing?"
    };

    private readonly string[,] lungsAnswers =
    {
        { "Pump Blood", "Exchange Gases", "Digest Food", "Control Movement" },
        { "Aorta", "Trachea", "Esophagus", "Bronchus" },
        { "Alveoli", "Heart", "Atrium", "Aorta" },
        { "1", "2", "3", "4" },
        { "Carbon Dioxide", "Nitrogen", "Oxygen", "Hydrogen" }
    };

    private readonly int[] lungsCorrect = { 1, 1, 0, 1, 2 };

    private void Start()
    {
        quizPanel.SetActive(false);
        nextButton.gameObject.SetActive(false);

        ResetButtons();
    }

    public void StartQuiz()
    {
        currentQuestion = 0;
        score = 0;
        answerSelected = false;

        infoPanel.SetActive(false);
        quizPanel.SetActive(true);

        SetQuizData(topicUIManager.GetCurrentTopic());

        optionA.gameObject.SetActive(true);
        optionB.gameObject.SetActive(true);
        optionC.gameObject.SetActive(true);
        optionD.gameObject.SetActive(true);

        LoadQuestion();
    }

    private void SetQuizData(string topic)
    {
        switch (topic)
        {
            case "Heart":
                questions = heartQuestions;
                answers = heartAnswers;
                correctAnswers = heartCorrect;
                break;

            case "Brain":
                questions = brainQuestions;
                answers = brainAnswers;
                correctAnswers = brainCorrect;
                break;

            case "Lungs":
                questions = lungsQuestions;
                answers = lungsAnswers;
                correctAnswers = lungsCorrect;
                break;

            default:
                questions = heartQuestions;
                answers = heartAnswers;
                correctAnswers = heartCorrect;
                break;
        }
    }

    private void LoadQuestion()
    {
        answerSelected = false;

        questionText.text =
            "QUESTION " + (currentQuestion + 1) +
            " OF " + questions.Length +
            "\n\n" +
            questions[currentQuestion];

        optionAText.text =
            "1. " + answers[currentQuestion, 0];

        optionBText.text =
            "2. " + answers[currentQuestion, 1];

        optionCText.text =
            "3. " + answers[currentQuestion, 2];

        optionDText.text =
            "4. " + answers[currentQuestion, 3];

        optionA.interactable = true;
        optionB.interactable = true;
        optionC.interactable = true;
        optionD.interactable = true;

        optionA.transition = Selectable.Transition.None;
        optionB.transition = Selectable.Transition.None;
        optionC.transition = Selectable.Transition.None;
        optionD.transition = Selectable.Transition.None;

        ResetButtons();

        nextButton.gameObject.SetActive(false);

        scoreText.text =
            "SCORE: " + score + "/" + questions.Length;
    }

    public void AnswerA()
    {
        CheckAnswer(0);
    }

    public void AnswerB()
    {
        CheckAnswer(1);
    }

    public void AnswerC()
    {
        CheckAnswer(2);
    }

    public void AnswerD()
    {
        CheckAnswer(3);
    }

    private void CheckAnswer(int selectedIndex)
    {
        if (answerSelected)
            return;

        answerSelected = true;

        int correctIndex = correctAnswers[currentQuestion];

        if (selectedIndex == correctIndex)
        {
            score++;

            GetButton(selectedIndex)
                .GetComponent<Image>()
                .color = correctColor;
        }
        else
        {
            GetButton(selectedIndex)
                .GetComponent<Image>()
                .color = wrongColor;

            GetButton(correctIndex)
                .GetComponent<Image>()
                .color = correctColor;
        }

        optionA.interactable = false;
        optionB.interactable = false;
        optionC.interactable = false;
        optionD.interactable = false;

        scoreText.text =
            "SCORE: " + score + "/" + questions.Length;

        nextButton.gameObject.SetActive(true);
    }

    private Button GetButton(int index)
    {
        switch (index)
        {
            case 0:
                return optionA;

            case 1:
                return optionB;

            case 2:
                return optionC;

            default:
                return optionD;
        }
    }

    private void ResetButtons()
    {
        SetButtonColor(optionA, normalColor);
        SetButtonColor(optionB, normalColor);
        SetButtonColor(optionC, normalColor);
        SetButtonColor(optionD, normalColor);
    }

    private void SetButtonColor(Button button, Color color)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();

        if (image != null)
            image.color = color;
    }

    public void NextQuestion()
    {
        if (!answerSelected)
            return;

        currentQuestion++;

        if (currentQuestion < questions.Length)
        {
            LoadQuestion();
        }
        else
        {
            FinishQuiz();
        }
    }

    private void FinishQuiz()
    {
        questionText.text = "QUIZ COMPLETE!";

        scoreText.text =
            "FINAL SCORE: " +
            score + "/" +
            questions.Length;

        optionA.gameObject.SetActive(false);
        optionB.gameObject.SetActive(false);
        optionC.gameObject.SetActive(false);
        optionD.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(false);
    }
}