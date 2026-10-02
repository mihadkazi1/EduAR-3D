using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class EduARUIBuilder : MonoBehaviour
{
    [Header("Vuforia Targets")]
    [SerializeField] private GameObject heartTarget;
    [SerializeField] private GameObject brainTarget;
    [SerializeField] private GameObject lungsTarget;

    [Header("Scene Canvas")]
    [SerializeField] private Canvas canvas;

    // =========================================================
    // COLORS
    // =========================================================

    private readonly Color backgroundColor =
        new Color(0.96f, 0.98f, 1.00f, 1f);

    private readonly Color cardColor =
        new Color(1f, 1f, 1f, 0.96f);

    private readonly Color primaryColor =
        new Color(0.12f, 0.38f, 0.78f, 1f);

    private readonly Color darkText =
        new Color(0.08f, 0.12f, 0.20f, 1f);

    private readonly Color softText =
        new Color(0.35f, 0.42f, 0.52f, 1f);

    private readonly Color correctColor =
        new Color(0.20f, 0.72f, 0.36f, 1f);

    private readonly Color wrongColor =
        new Color(0.88f, 0.22f, 0.24f, 1f);

    private readonly Color white = Color.white;

    // =========================================================
    // RUNTIME ROOTS
    // =========================================================

    private RectTransform fullScreenRoot;
    private RectTransform safeArea;

    // =========================================================
    // PANELS
    // =========================================================

    private GameObject homePanel;
    private GameObject learningPanel;
    private GameObject quizPanel;
    private GameObject resultPanel;

    // =========================================================
    // LEARNING UI
    // =========================================================

    private TMP_Text learningTitle;
    private TMP_Text learningText;

    // =========================================================
    // QUIZ UI
    // =========================================================

    private TMP_Text questionNumberText;
    private TMP_Text questionText;
    private TMP_Text scoreText;

    private Button optionA;
    private Button optionB;
    private Button optionC;
    private Button optionD;
    private Button nextButton;

    private TMP_Text optionAText;
    private TMP_Text optionBText;
    private TMP_Text optionCText;
    private TMP_Text optionDText;

    private Image optionAImage;
    private Image optionBImage;
    private Image optionCImage;
    private Image optionDImage;

    // =========================================================
    // QUIZ DATA
    // =========================================================

    private string currentTopic = "Heart";

    private string[] questions;
    private string[,] answers;
    private int[] correctAnswers;

    private int currentQuestion;
    private int score;
    private bool answerSelected;

    private readonly Color normalButtonColor =
        new Color(1f, 1f, 1f, 1f);

    // =========================================================
    // SCREEN TRACKING
    // =========================================================

    private int lastScreenWidth;
    private int lastScreenHeight;
    private Rect lastSafeArea;

    // =========================================================
    // UNITY START
    // =========================================================

    private void Start()
    {
        DisableAllTargets();

        if (!ValidateCanvas())
            return;

        CreateEventSystem();
        CreateRuntimeRoots();
        CreateUI();

        ShowHome();
    }

    // =========================================================
    // VALIDATE CANVAS
    // =========================================================

    private bool ValidateCanvas()
    {
        if (canvas == null)
        {
            Debug.LogError(
                "EduARUIBuilder: Canvas is not assigned. " +
                "Create a Canvas in the scene and drag it into " +
                "the Canvas field of EduARUI."
            );

            return false;
        }

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.pixelPerfect = false;

        CanvasScaler scaler =
            canvas.GetComponent<CanvasScaler>();

        if (scaler == null)
        {
            scaler =
                canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        return true;
    }

    // =========================================================
    // EVENT SYSTEM
    // =========================================================

    private void CreateEventSystem()
    {
        if (EventSystem.current != null)
        {
            InputSystemUIInputModule existingModule =
                EventSystem.current.GetComponent<InputSystemUIInputModule>();

            if (existingModule == null)
            {
                existingModule =
                    EventSystem.current.gameObject
                        .AddComponent<InputSystemUIInputModule>();
            }

            existingModule.enabled = true;

            return;
        }

        GameObject eventSystemObject =
            new GameObject("EventSystem");

        EventSystem eventSystem =
            eventSystemObject.AddComponent<EventSystem>();

        InputSystemUIInputModule inputModule =
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

        inputModule.enabled = true;
    }

    // =========================================================
    // RUNTIME ROOTS
    // =========================================================

    private void CreateRuntimeRoots()
    {
        fullScreenRoot =
            CreateUIObject(
                "FullScreenRoot",
                canvas.transform
            ).GetComponent<RectTransform>();

        StretchFull(fullScreenRoot);

        // -----------------------------------------------------
        // FULL SCREEN BACKGROUND
        // -----------------------------------------------------

        GameObject background =
            CreateUIObject(
                "Background",
                fullScreenRoot
            );

        Image backgroundImage =
            background.AddComponent<Image>();

        backgroundImage.color =
            backgroundColor;

        StretchFull(
            background.GetComponent<RectTransform>()
        );

        // -----------------------------------------------------
        // SAFE AREA FOR INTERACTIVE CONTENT
        // -----------------------------------------------------

        safeArea =
            CreateUIObject(
                "SafeArea",
                fullScreenRoot
            ).GetComponent<RectTransform>();

        ApplySafeArea();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
    }

    // =========================================================
    // SAFE AREA
    // =========================================================

    private void ApplySafeArea()
    {
        if (safeArea == null)
            return;

        Rect safe =
            Screen.safeArea;

        float width =
            Mathf.Max(Screen.width, 1);

        float height =
            Mathf.Max(Screen.height, 1);

        Vector2 min =
            new Vector2(
                safe.xMin / width,
                safe.yMin / height
            );

        Vector2 max =
            new Vector2(
                safe.xMax / width,
                safe.yMax / height
            );

        safeArea.anchorMin = min;
        safeArea.anchorMax = max;

        safeArea.offsetMin =
            Vector2.zero;

        safeArea.offsetMax =
            Vector2.zero;
    }

    // =========================================================
    // SCREEN CHANGE
    // =========================================================

    private void Update()
    {
        if (safeArea == null)
            return;

        bool screenChanged =
            Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight;

        bool safeAreaChanged =
            Screen.safeArea != lastSafeArea;

        if (screenChanged || safeAreaChanged)
        {
            ApplySafeArea();

            lastScreenWidth =
                Screen.width;

            lastScreenHeight =
                Screen.height;

            lastSafeArea =
                Screen.safeArea;
        }
    }

    // =========================================================
    // CREATE UI
    // =========================================================

    private void CreateUI()
    {
        CreateHomePanel();
        CreateLearningPanel();
        CreateQuizPanel();
        CreateResultPanel();
    }

    // =========================================================
    // HOME PANEL
    // =========================================================

    private void CreateHomePanel()
    {
        homePanel =
            CreatePanel(
                "HomePanel",
                safeArea
            );

        StretchFull(
            homePanel.GetComponent<RectTransform>()
        );

        homePanel.GetComponent<Image>()
            .color =
            backgroundColor;

        // -----------------------------------------------------
        // HEADER
        // -----------------------------------------------------

        GameObject header =
            CreateUIObject(
                "Header",
                homePanel.transform
            );

        RectTransform headerRect =
            header.GetComponent<RectTransform>();

        SetAnchor(
            headerRect,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f)
        );

        headerRect.sizeDelta =
            new Vector2(900, 250);

        headerRect.anchoredPosition =
            new Vector2(0, -170);

        TMP_Text title =
            CreateText(
                "EDUAR 3D",
                62,
                FontStyles.Bold,
                white,
                TextAlignmentOptions.Center,
                header.transform
            );

        title.rectTransform.sizeDelta =
            new Vector2(900, 90);

        title.rectTransform.anchoredPosition =
            new Vector2(0, 50);

        TMP_Text subtitle =
            CreateText(
                "Interactive AR Learning",
                30,
                FontStyles.Normal,
                white,
                TextAlignmentOptions.Center,
                header.transform
            );

        subtitle.rectTransform.sizeDelta =
            new Vector2(900, 60);

        subtitle.rectTransform.anchoredPosition =
            new Vector2(0, -40);

        // -----------------------------------------------------
        // TOPIC CARDS
        // -----------------------------------------------------

        CreateTopicCard(
            "❤️",
            "HUMAN HEART",
            "Explore the anatomy of the human heart.",
            "Heart",
            new Vector2(0, -500)
        );

        CreateTopicCard(
            "🧠",
            "HUMAN BRAIN",
            "Discover the structure of the human brain.",
            "Brain",
            new Vector2(0, -930)
        );

        CreateTopicCard(
            "🫁",
            "HUMAN LUNGS",
            "Learn how the respiratory system works.",
            "Lungs",
            new Vector2(0, -1360)
        );
    }

    // =========================================================
    // TOPIC CARD
    // =========================================================

    private void CreateTopicCard(
        string icon,
        string titleText,
        string descriptionText,
        string topic,
        Vector2 position
    )
    {
        GameObject card =
            CreateUIObject(
                topic + "Card",
                homePanel.transform
            );

        Image image =
            card.AddComponent<Image>();

        image.color =
            cardColor;

        AddShadow(card);

        RectTransform rect =
            card.GetComponent<RectTransform>();

        SetAnchor(
            rect,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f)
        );

        rect.sizeDelta =
            new Vector2(900, 340);

        rect.anchoredPosition =
            position;

        // -----------------------------------------------------
        // ICON
        // -----------------------------------------------------

        TMP_Text iconText =
            CreateText(
                icon,
                65,
                FontStyles.Normal,
                primaryColor,
                TextAlignmentOptions.Center,
                card.transform
            );

        iconText.rectTransform.sizeDelta =
            new Vector2(150, 120);

        iconText.rectTransform.anchoredPosition =
            new Vector2(-330, 0);

        // -----------------------------------------------------
        // TITLE
        // -----------------------------------------------------

        TMP_Text title =
            CreateText(
                titleText,
                34,
                FontStyles.Bold,
                darkText,
                TextAlignmentOptions.Left,
                card.transform
            );

        title.rectTransform.sizeDelta =
            new Vector2(550, 60);

        title.rectTransform.anchoredPosition =
            new Vector2(80, 75);

        // -----------------------------------------------------
        // DESCRIPTION
        // -----------------------------------------------------

        TMP_Text desc =
            CreateText(
                descriptionText,
                23,
                FontStyles.Normal,
                softText,
                TextAlignmentOptions.Left,
                card.transform
            );

        desc.enableWordWrapping = true;

        desc.rectTransform.sizeDelta =
            new Vector2(560, 100);

        desc.rectTransform.anchoredPosition =
            new Vector2(80, 10);

        // -----------------------------------------------------
        // BUTTON
        // -----------------------------------------------------

        Button button =
            CreateButton(
                "EXPLORE",
                card.transform,
                primaryColor,
                white
            );

        RectTransform buttonRect =
            button.GetComponent<RectTransform>();

        buttonRect.sizeDelta =
            new Vector2(270, 78);

        buttonRect.anchoredPosition =
            new Vector2(250, -95);

        button.onClick.AddListener(
            () => SelectTopic(topic)
        );

        AddButtonAnimation(button);
    }

    // =========================================================
    // LEARNING PANEL
    // =========================================================

    private void CreateLearningPanel()
    {
        learningPanel =
            CreatePanel(
                "LearningPanel",
                safeArea
            );

        StretchFull(
            learningPanel.GetComponent<RectTransform>()
        );

        learningPanel.GetComponent<Image>()
            .color =
            backgroundColor;

        GameObject topBar =
            CreateUIObject(
                "LearningTopBar",
                learningPanel.transform
            );

        Image topImage =
            topBar.AddComponent<Image>();

        topImage.color =
            primaryColor;

        RectTransform topRect =
            topBar.GetComponent<RectTransform>();

        SetAnchor(
            topRect,
            new Vector2(0, 1),
            new Vector2(1, 1)
        );

        topRect.sizeDelta =
            new Vector2(0, 180);

        topRect.anchoredPosition =
            new Vector2(0, -10);

        learningTitle =
            CreateText(
                "HUMAN HEART",
                43,
                FontStyles.Bold,
                white,
                TextAlignmentOptions.Center,
                topBar.transform
            );

        learningTitle.rectTransform.sizeDelta =
            new Vector2(700, 90);

        learningTitle.rectTransform.anchoredPosition =
            new Vector2(0, -70);

        GameObject content =
            CreatePanel(
                "InformationCard",
                learningPanel.transform
            );

        content.GetComponent<Image>()
            .color =
            cardColor;

        AddShadow(content);

        RectTransform contentRect =
            content.GetComponent<RectTransform>();

        SetAnchor(
            contentRect,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f)
        );

        contentRect.sizeDelta =
            new Vector2(900, 1050);

        contentRect.anchoredPosition =
            new Vector2(0, 50);

        learningText =
            CreateText(
                "",
                28,
                FontStyles.Normal,
                darkText,
                TextAlignmentOptions.Left,
                content.transform
            );

        learningText.enableWordWrapping = true;

        learningText.rectTransform.sizeDelta =
            new Vector2(760, 800);

        learningText.rectTransform.anchoredPosition =
            new Vector2(0, 80);

        Button quizButton =
            CreateButton(
                "START QUIZ",
                content.transform,
                primaryColor,
                white
            );

        RectTransform quizRect =
            quizButton.GetComponent<RectTransform>();

        quizRect.sizeDelta =
            new Vector2(600, 100);

        quizRect.anchoredPosition =
            new Vector2(0, -380);

        quizButton.onClick.AddListener(
            StartQuiz
        );

        AddButtonAnimation(quizButton);

        Button closeButton =
            CreateButton(
                "BACK",
                learningPanel.transform,
                new Color(0.5f, 0.55f, 0.62f),
                white
            );

        RectTransform closeRect =
            closeButton.GetComponent<RectTransform>();

        closeRect.anchorMin =
            new Vector2(0, 0);

        closeRect.anchorMax =
            new Vector2(0, 0);

        closeRect.pivot =
            new Vector2(0, 0);

        closeRect.sizeDelta =
            new Vector2(220, 75);

        closeRect.anchoredPosition =
            new Vector2(40, 40);

        closeButton.onClick.AddListener(
            ShowHome
        );
    }

    // =========================================================
    // QUIZ PANEL
    // =========================================================

    private void CreateQuizPanel()
    {
        quizPanel =
            CreatePanel(
                "QuizPanel",
                safeArea
            );

        StretchFull(
            quizPanel.GetComponent<RectTransform>()
        );

        quizPanel.GetComponent<Image>()
            .color =
            backgroundColor;

        GameObject header =
            CreatePanel(
                "QuizHeader",
                quizPanel.transform
            );

        header.GetComponent<Image>()
            .color =
            primaryColor;

        RectTransform headerRect =
            header.GetComponent<RectTransform>();

        SetAnchor(
            headerRect,
            new Vector2(0, 1),
            new Vector2(1, 1)
        );

        headerRect.sizeDelta =
            new Vector2(0, 250);

        headerRect.anchoredPosition =
            new Vector2(0, -10);

        TMP_Text quizTitle =
            CreateText(
                "HEART QUIZ",
                46,
                FontStyles.Bold,
                white,
                TextAlignmentOptions.Center,
                header.transform
            );

        quizTitle.rectTransform.sizeDelta =
            new Vector2(900, 80);

        quizTitle.rectTransform.anchoredPosition =
            new Vector2(0, -55);

        questionNumberText =
            CreateText(
                "QUESTION 1 OF 5",
                27,
                FontStyles.Bold,
                new Color(0.85f, 0.92f, 1f),
                TextAlignmentOptions.Center,
                header.transform
            );

        questionNumberText.rectTransform.sizeDelta =
            new Vector2(800, 55);

        questionNumberText.rectTransform.anchoredPosition =
            new Vector2(0, -145);

        questionText =
            CreateText(
                "",
                31,
                FontStyles.Bold,
                darkText,
                TextAlignmentOptions.Center,
                quizPanel.transform
            );

        questionText.enableWordWrapping = true;

        questionText.rectTransform.sizeDelta =
            new Vector2(850, 230);

        questionText.rectTransform.anchoredPosition =
            new Vector2(0, 500);

        optionA =
            CreateAnswerButton(
                "1. Answer A",
                new Vector2(0, 180),
                out optionAText,
                out optionAImage
            );

        optionB =
            CreateAnswerButton(
                "2. Answer B",
                new Vector2(0, 0),
                out optionBText,
                out optionBImage
            );

        optionC =
            CreateAnswerButton(
                "3. Answer C",
                new Vector2(0, -180),
                out optionCText,
                out optionCImage
            );

        optionD =
            CreateAnswerButton(
                "4. Answer D",
                new Vector2(0, -360),
                out optionDText,
                out optionDImage
            );

        optionA.onClick.AddListener(AnswerA);
        optionB.onClick.AddListener(AnswerB);
        optionC.onClick.AddListener(AnswerC);
        optionD.onClick.AddListener(AnswerD);

        scoreText =
            CreateText(
                "SCORE: 0/5",
                28,
                FontStyles.Bold,
                primaryColor,
                TextAlignmentOptions.Center,
                quizPanel.transform
            );

        scoreText.rectTransform.sizeDelta =
            new Vector2(500, 70);

        scoreText.rectTransform.anchoredPosition =
            new Vector2(-220, -650);

        nextButton =
            CreateButton(
                "NEXT",
                quizPanel.transform,
                primaryColor,
                white
            );

        RectTransform nextRect =
            nextButton.GetComponent<RectTransform>();

        nextRect.sizeDelta =
            new Vector2(310, 90);

        nextRect.anchoredPosition =
            new Vector2(260, -650);

        nextButton.onClick.AddListener(
            NextQuestion
        );

        Button backButton =
            CreateButton(
                "EXIT QUIZ",
                quizPanel.transform,
                new Color(0.50f, 0.55f, 0.62f),
                white
            );

        RectTransform backRect =
            backButton.GetComponent<RectTransform>();

        backRect.sizeDelta =
            new Vector2(250, 75);

        backRect.anchorMin =
            new Vector2(0.5f, 0);

        backRect.anchorMax =
            new Vector2(0.5f, 0);

        backRect.pivot =
            new Vector2(0.5f, 0);

        backRect.anchoredPosition =
            new Vector2(0, 35);

        backButton.onClick.AddListener(
            ShowHome
        );
    }

    // =========================================================
    // RESULT PANEL
    // =========================================================

    private void CreateResultPanel()
    {
        resultPanel =
            CreatePanel(
                "ResultPanel",
                safeArea
            );

        StretchFull(
            resultPanel.GetComponent<RectTransform>()
        );

        resultPanel.GetComponent<Image>()
            .color =
            backgroundColor;

        TMP_Text resultTitle =
            CreateText(
                "QUIZ COMPLETE!",
                52,
                FontStyles.Bold,
                primaryColor,
                TextAlignmentOptions.Center,
                resultPanel.transform
            );

        resultTitle.rectTransform.sizeDelta =
            new Vector2(900, 100);

        resultTitle.rectTransform.anchoredPosition =
            new Vector2(0, 400);

        TMP_Text resultMessage =
            CreateText(
                "Great job!",
                34,
                FontStyles.Normal,
                darkText,
                TextAlignmentOptions.Center,
                resultPanel.transform
            );

        resultMessage.rectTransform.sizeDelta =
            new Vector2(800, 100);

        resultMessage.rectTransform.anchoredPosition =
            new Vector2(0, 250);

        TMP_Text finalScore =
            CreateText(
                "0/5",
                90,
                FontStyles.Bold,
                correctColor,
                TextAlignmentOptions.Center,
                resultPanel.transform
            );

        finalScore.name =
            "FinalScore";

        finalScore.rectTransform.sizeDelta =
            new Vector2(800, 180);

        finalScore.rectTransform.anchoredPosition =
            new Vector2(0, 50);

        Button homeButton =
            CreateButton(
                "BACK TO HOME",
                resultPanel.transform,
                primaryColor,
                white
            );

        homeButton.GetComponent<RectTransform>()
            .sizeDelta =
            new Vector2(600, 110);

        homeButton.GetComponent<RectTransform>()
            .anchoredPosition =
            new Vector2(0, -300);

        homeButton.onClick.AddListener(
            ShowHome
        );

        AddButtonAnimation(homeButton);
    }

    // =========================================================
    // TOPIC SELECTION
    // =========================================================

    private void SelectTopic(string topic)
    {
        currentTopic =
            topic;

        DisableAllTargets();

        switch (topic)
        {
            case "Heart":

                if (heartTarget != null)
                    heartTarget.SetActive(true);

                break;

            case "Brain":

                if (brainTarget != null)
                    brainTarget.SetActive(true);

                break;

            case "Lungs":

                if (lungsTarget != null)
                    lungsTarget.SetActive(true);

                break;
        }

        UpdateLearningContent();

        HideAllPanels();

        learningPanel.SetActive(true);

        StartCoroutine(
            AnimatePanelIn(
                learningPanel
            )
        );
    }

    // =========================================================
    // LEARNING CONTENT
    // =========================================================

    private void UpdateLearningContent()
    {
        switch (currentTopic)
        {
            case "Heart":

                learningTitle.text =
                    "HUMAN HEART";

                learningText.text =
                    "The human heart is a muscular organ " +
                    "that pumps blood throughout the body.\n\n" +

                    "MAIN PARTS\n\n" +

                    "• Right Atrium\n" +
                    "• Right Ventricle\n" +
                    "• Left Atrium\n" +
                    "• Left Ventricle\n" +
                    "• Aorta\n\n" +

                    "The right side receives deoxygenated " +
                    "blood, while the left side pumps " +
                    "oxygenated blood to the body.";

                break;

            case "Brain":

                learningTitle.text =
                    "HUMAN BRAIN";

                learningText.text =
                    "The human brain is the central organ " +
                    "of the nervous system and controls " +
                    "many important body functions.\n\n" +

                    "MAIN PARTS\n\n" +

                    "• Cerebrum\n" +
                    "• Cerebellum\n" +
                    "• Brainstem\n" +
                    "• Frontal Lobe\n" +
                    "• Temporal Lobe\n\n" +

                    "The brain is responsible for thinking, " +
                    "memory, movement, sensation and " +
                    "coordination.";

                break;

            case "Lungs":

                learningTitle.text =
                    "HUMAN LUNGS";

                learningText.text =
                    "The lungs are major organs of the " +
                    "respiratory system. They exchange " +
                    "oxygen and carbon dioxide.\n\n" +

                    "MAIN PARTS\n\n" +

                    "• Trachea\n" +
                    "• Bronchi\n" +
                    "• Left Lung\n" +
                    "• Right Lung\n" +
                    "• Alveoli\n\n" +

                    "During breathing, oxygen enters the " +
                    "body while carbon dioxide is removed.";

                break;
        }
    }

    // =========================================================
    // QUIZ START
    // =========================================================

    private void StartQuiz()
    {
        currentQuestion = 0;
        score = 0;
        answerSelected = false;

        LoadQuizData();

        learningPanel.SetActive(false);

        quizPanel.SetActive(true);

        ResetQuizButtons();

        LoadQuestion();
    }

    // =========================================================
    // QUIZ DATA
    // =========================================================

    private void LoadQuizData()
    {
        switch (currentTopic)
        {
            case "Heart":

                questions =
                    new string[]
                    {
                        "Which chamber pumps oxygenated blood to the body?",
                        "Which organ pumps blood throughout the body?",
                        "Which blood vessel carries oxygen-rich blood from the heart?",
                        "How many chambers does the human heart have?",
                        "Which side of the heart receives deoxygenated blood?"
                    };

                answers =
                    new string[,]
                    {
                        {
                            "Right Atrium",
                            "Right Ventricle",
                            "Left Atrium",
                            "Left Ventricle"
                        },

                        {
                            "Brain",
                            "Heart",
                            "Lungs",
                            "Kidney"
                        },

                        {
                            "Aorta",
                            "Vena Cava",
                            "Pulmonary Vein",
                            "Pulmonary Artery"
                        },

                        {
                            "2",
                            "3",
                            "4",
                            "5"
                        },

                        {
                            "Left Side",
                            "Right Side",
                            "Both Sides",
                            "Neither Side"
                        }
                    };

                correctAnswers =
                    new int[]
                    {
                        3,
                        1,
                        0,
                        2,
                        1
                    };

                break;

            case "Brain":

                questions =
                    new string[]
                    {
                        "What is the main control center of the nervous system?",
                        "Which part controls balance and coordination?",
                        "What is the largest part of the brain?",
                        "Which structure connects the brain to the spinal cord?",
                        "Which part is strongly associated with memory?"
                    };

                answers =
                    new string[,]
                    {
                        {
                            "Heart",
                            "Brain",
                            "Lungs",
                            "Kidney"
                        },

                        {
                            "Cerebrum",
                            "Cerebellum",
                            "Brainstem",
                            "Medulla"
                        },

                        {
                            "Cerebellum",
                            "Brainstem",
                            "Cerebrum",
                            "Medulla"
                        },

                        {
                            "Cerebrum",
                            "Cerebellum",
                            "Brainstem",
                            "Hippocampus"
                        },

                        {
                            "Medulla",
                            "Hippocampus",
                            "Cerebellum",
                            "Brainstem"
                        }
                    };

                correctAnswers =
                    new int[]
                    {
                        1,
                        1,
                        2,
                        2,
                        1
                    };

                break;

            case "Lungs":

                questions =
                    new string[]
                    {
                        "What is the main function of the lungs?",
                        "Which tube carries air toward the lungs?",
                        "Where does gas exchange mainly occur?",
                        "How many lungs does a healthy person normally have?",
                        "Which gas is taken into the body during breathing?"
                    };

                answers =
                    new string[,]
                    {
                        {
                            "Pump Blood",
                            "Exchange Gases",
                            "Digest Food",
                            "Control Movement"
                        },

                        {
                            "Aorta",
                            "Trachea",
                            "Esophagus",
                            "Stomach"
                        },

                        {
                            "Alveoli",
                            "Heart",
                            "Atrium",
                            "Aorta"
                        },

                        {
                            "1",
                            "2",
                            "3",
                            "4"
                        },

                        {
                            "Carbon Dioxide",
                            "Nitrogen",
                            "Oxygen",
                            "Hydrogen"
                        }
                    };

                correctAnswers =
                    new int[]
                    {
                        1,
                        1,
                        0,
                        1,
                        2
                    };

                break;
        }
    }

    // =========================================================
    // LOAD QUESTION
    // =========================================================

    private void LoadQuestion()
    {
        answerSelected = false;

        questionNumberText.text =
            "QUESTION " +
            (currentQuestion + 1) +
            " OF " +
            questions.Length;

        questionText.text =
            questions[currentQuestion];

        optionAText.text =
            "1. " +
            answers[currentQuestion, 0];

        optionBText.text =
            "2. " +
            answers[currentQuestion, 1];

        optionCText.text =
            "3. " +
            answers[currentQuestion, 2];

        optionDText.text =
            "4. " +
            answers[currentQuestion, 3];

        optionA.interactable = true;
        optionB.interactable = true;
        optionC.interactable = true;
        optionD.interactable = true;

        ResetQuizButtons();

        nextButton.gameObject.SetActive(false);

        scoreText.text =
            "SCORE: " +
            score +
            "/" +
            questions.Length;
    }

    // =========================================================
    // ANSWERS
    // =========================================================

    private void AnswerA()
    {
        CheckAnswer(0);
    }

    private void AnswerB()
    {
        CheckAnswer(1);
    }

    private void AnswerC()
    {
        CheckAnswer(2);
    }

    private void AnswerD()
    {
        CheckAnswer(3);
    }

    private void CheckAnswer(int selectedIndex)
    {
        if (answerSelected)
            return;

        answerSelected = true;

        int correctIndex =
            correctAnswers[currentQuestion];

        if (selectedIndex == correctIndex)
        {
            score++;

            GetAnswerImage(
                selectedIndex
            ).color =
                correctColor;
        }
        else
        {
            GetAnswerImage(
                selectedIndex
            ).color =
                wrongColor;

            GetAnswerImage(
                correctIndex
            ).color =
                correctColor;
        }

        optionA.interactable = false;
        optionB.interactable = false;
        optionC.interactable = false;
        optionD.interactable = false;

        scoreText.text =
            "SCORE: " +
            score +
            "/" +
            questions.Length;

        nextButton.gameObject.SetActive(true);
    }

    // =========================================================
    // NEXT
    // =========================================================

    private void NextQuestion()
    {
        if (!answerSelected)
            return;

        currentQuestion++;

        if (currentQuestion <
            questions.Length)
        {
            LoadQuestion();
        }
        else
        {
            FinishQuiz();
        }
    }

    // =========================================================
    // FINISH QUIZ
    // =========================================================

    private void FinishQuiz()
    {
        quizPanel.SetActive(false);

        resultPanel.SetActive(true);

        Transform scoreTransform =
            resultPanel.transform.Find(
                "FinalScore"
            );

        if (scoreTransform != null)
        {
            TMP_Text finalScore =
                scoreTransform.GetComponent<TMP_Text>();

            if (finalScore != null)
            {
                finalScore.text =
                    score +
                    "/" +
                    questions.Length;
            }
        }

        Transform messageTransform =
            resultPanel.transform.Find(
                "Text"
            );

        if (messageTransform == null)
            return;

        TMP_Text resultMessage =
            messageTransform.GetComponent<TMP_Text>();

        if (resultMessage == null)
            return;

        if (score == questions.Length)
        {
            resultMessage.text =
                "Excellent! Perfect score!";
        }
        else if (score >= 3)
        {
            resultMessage.text =
                "Great job! Keep learning!";
        }
        else
        {
            resultMessage.text =
                "Good try! Review the topic and try again.";
        }
    }

    // =========================================================
    // BUTTON RESET
    // =========================================================

    private void ResetQuizButtons()
    {
        SetButtonColor(
            optionAImage,
            normalButtonColor
        );

        SetButtonColor(
            optionBImage,
            normalButtonColor
        );

        SetButtonColor(
            optionCImage,
            normalButtonColor
        );

        SetButtonColor(
            optionDImage,
            normalButtonColor
        );
    }

    private void SetButtonColor(
        Image image,
        Color color
    )
    {
        if (image != null)
            image.color = color;
    }

    private Image GetAnswerImage(int index)
    {
        switch (index)
        {
            case 0:
                return optionAImage;

            case 1:
                return optionBImage;

            case 2:
                return optionCImage;

            default:
                return optionDImage;
        }
    }

    // =========================================================
    // PANEL MANAGEMENT
    // =========================================================

    private void ShowHome()
    {
        DisableAllTargets();

        HideAllPanels();

        homePanel.SetActive(true);

        StartCoroutine(
            AnimatePanelIn(homePanel)
        );
    }

    private void HideAllPanels()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        if (learningPanel != null)
            learningPanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void DisableAllTargets()
    {
        if (heartTarget != null)
            heartTarget.SetActive(false);

        if (brainTarget != null)
            brainTarget.SetActive(false);

        if (lungsTarget != null)
            lungsTarget.SetActive(false);
    }

    // =========================================================
    // CREATE ANSWER BUTTON
    // =========================================================

    private Button CreateAnswerButton(
        string text,
        Vector2 position,
        out TMP_Text label,
        out Image buttonImage
    )
    {
        Button button =
            CreateButton(
                text,
                quizPanel.transform,
                normalButtonColor,
                darkText
            );

        RectTransform rect =
            button.GetComponent<RectTransform>();

        rect.sizeDelta =
            new Vector2(850, 125);

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            position;

        label =
            button.GetComponentInChildren<TMP_Text>();

        buttonImage =
            button.GetComponent<Image>();

        return button;
    }

    // =========================================================
    // CREATE PANEL
    // =========================================================

    private GameObject CreatePanel(
        string name,
        Transform parent
    )
    {
        GameObject panel =
            CreateUIObject(
                name,
                parent
            );

        panel.AddComponent<Image>();

        return panel;
    }

    // =========================================================
    // CREATE BUTTON
    // =========================================================

    private Button CreateButton(
        string text,
        Transform parent,
        Color background,
        Color textColor
    )
    {
        GameObject buttonObject =
            CreateUIObject(
                text + "Button",
                parent
            );

        Image image =
            buttonObject.AddComponent<Image>();

        image.color =
            background;

        AddShadow(buttonObject);

        Button button =
            buttonObject.AddComponent<Button>();

        button.transition =
            Selectable.Transition.None;

        TMP_Text label =
            CreateText(
                text,
                27,
                FontStyles.Bold,
                textColor,
                TextAlignmentOptions.Center,
                buttonObject.transform
            );

        StretchFull(
            label.rectTransform
        );

        label.margin =
            new Vector4(
                20,
                10,
                20,
                10
            );

        return button;
    }

    // =========================================================
    // CREATE TEXT
    // =========================================================

    private TMP_Text CreateText(
        string text,
        float fontSize,
        FontStyles style,
        Color color,
        TextAlignmentOptions alignment,
        Transform parent
    )
    {
        GameObject textObject =
            CreateUIObject(
                "Text",
                parent
            );

        TextMeshProUGUI tmp =
            textObject.AddComponent<TextMeshProUGUI>();

        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;

        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font =
                TMP_Settings.defaultFontAsset;
        }

        return tmp;
    }

    // =========================================================
    // CREATE UI OBJECT
    // =========================================================

    private GameObject CreateUIObject(
        string name,
        Transform parent
    )
    {
        GameObject objectUI =
            new GameObject(name);

        objectUI.transform.SetParent(
            parent,
            false
        );

        objectUI.AddComponent<RectTransform>();

        return objectUI;
    }

    // =========================================================
    // RECT HELPERS
    // =========================================================

    private void StretchFull(
        RectTransform rect
    )
    {
        rect.anchorMin =
            Vector2.zero;

        rect.anchorMax =
            Vector2.one;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        rect.localScale =
            Vector3.one;
    }

    private void SetAnchor(
        RectTransform rect,
        Vector2 min,
        Vector2 max
    )
    {
        rect.anchorMin = min;
        rect.anchorMax = max;

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );
    }

    // =========================================================
    // SHADOW
    // =========================================================

    private void AddShadow(
        GameObject objectUI
    )
    {
        Shadow shadow =
            objectUI.AddComponent<Shadow>();

        shadow.effectColor =
            new Color(
                0f,
                0f,
                0f,
                0.16f
            );

        shadow.effectDistance =
            new Vector2(
                0,
                -6
            );

        shadow.useGraphicAlpha =
            true;
    }

    // =========================================================
    // BUTTON ANIMATION
    // =========================================================

    private void AddButtonAnimation(
        Button button
    )
    {
        button.onClick.AddListener(
            () =>
                StartCoroutine(
                    ButtonClickAnimation(
                        button.transform
                    )
                )
        );
    }

    private IEnumerator ButtonClickAnimation(
        Transform target
    )
    {
        Vector3 original =
            target.localScale;

        Vector3 smaller =
            original * 0.94f;

        target.localScale =
            smaller;

        yield return new WaitForSeconds(
            0.08f
        );

        target.localScale =
            original;
    }

    // =========================================================
    // PANEL ANIMATION
    // =========================================================

    private IEnumerator AnimatePanelIn(
        GameObject panel
    )
    {
        RectTransform rect =
            panel.GetComponent<RectTransform>();

        Vector3 originalScale =
            rect.localScale;

        rect.localScale =
            Vector3.one * 0.92f;

        CanvasGroup group =
            panel.GetComponent<CanvasGroup>();

        if (group == null)
        {
            group =
                panel.AddComponent<CanvasGroup>();
        }

        group.alpha = 0f;

        float time = 0f;

        while (time < 0.25f)
        {
            time +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time / 0.25f
                );

            float smooth =
                t * t *
                (3f - 2f * t);

            rect.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.92f,
                    originalScale,
                    smooth
                );

            group.alpha =
                Mathf.Lerp(
                    0f,
                    1f,
                    smooth
                );

            yield return null;
        }

        rect.localScale =
            originalScale;

        group.alpha =
            1f;
    }
}