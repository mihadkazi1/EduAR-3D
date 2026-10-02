using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Vuforia;

using UIImage = UnityEngine.UI.Image;

public class EduARExperience : MonoBehaviour
{
    // =========================================================
    // AR TARGETS (VUFORIA)
    // =========================================================

    [Header("AR Targets")]
    [SerializeField] private GameObject heartTarget;
    [SerializeField] private GameObject brainTarget;
    [SerializeField] private GameObject lungsTarget;
    [SerializeField] private GameObject physicsTarget;
    [SerializeField] private GameObject chemistryTarget;

    // =========================================================
    // AR MODELS
    // =========================================================

    [Header("AR Models")]
    [SerializeField] private Transform heartModel;
    [SerializeField] private Transform brainModel;
    [SerializeField] private Transform lungsModel;
    [SerializeField] private Transform physicsModel;
    [SerializeField] private Transform chemistryModel;

    // =========================================================
    // COLOR HELPER
    // =========================================================

    private static Color Hex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.white;
    }

    // =========================================================
    // NEXT-GEN CYBER DARK THEME
    // =========================================================

    private readonly Color bgDark = Hex("#080C14");         // Ultra-deep obsidian
    private readonly Color cardDark = Hex("#111726");       // Elevated dark glass panel
    private readonly Color cardBorder = Hex("#1E293B");     // Crisp card outline
    private readonly Color glassBorder = new Color(1f, 1f, 1f, 0.15f);
    private readonly Color textWhite = Hex("#FFFFFF");      // High contrast text
    private readonly Color textMuted = Hex("#94A3B8");      // Slate gray secondary text

    // Subject Signatures (Vivid Neons)
    private readonly Color bioColor = Hex("#10B981");       // Neon Emerald
    private readonly Color bioTint = new Color(0.06f, 0.72f, 0.51f, 0.15f);
    private readonly Color physColor = Hex("#06B6D4");      // Electric Cyan
    private readonly Color physTint = new Color(0.02f, 0.71f, 0.83f, 0.15f);
    private readonly Color chemColor = Hex("#A855F7");      // Vivid Orchid
    private readonly Color chemTint = new Color(0.66f, 0.33f, 0.97f, 0.15f);
    private readonly Color mathColor = Hex("#F59E0B");      // Amber Gold
    private readonly Color mathTint = new Color(0.96f, 0.62f, 0.04f, 0.15f);

    // Interactive States
    private readonly Color brandIndigo = Hex("#6366F1");    // High-tech CTA Indigo
    private readonly Color scanTomato = Hex("#FF6347");      // Tomato scan CTA
    private readonly Color wrongRed = Hex("#EF4444");
    private readonly Color correctGreen = Hex("#10B981");
    private readonly Color glassHUD = new Color(0.06f, 0.09f, 0.15f, 0.92f);
    private readonly Color buttonPillDark = new Color(1f, 1f, 1f, 0.10f);

    // =========================================================
    // TYPOGRAPHY
    // =========================================================

    [Header("Typography")]
    [SerializeField] private TMP_FontAsset appFont;

    // =========================================================
    // CANVAS & SCREENS
    // =========================================================

    private Canvas canvas;
    private RectTransform safeArea;
    private bool keyboardLayoutActive;

    private GameObject homeScreen;
    private GameObject topicScreen;
    private GameObject arScreen;
    private GameObject quizScreen;
    private GameObject resultScreen;
    private GameObject universalScanScreen;

    // =========================================================
    // STATE
    // =========================================================

    private string currentSubject = "Biology";
    private string currentTopic = "Heart";

    // =========================================================
    // AR UI REFERENCES
    // =========================================================

    private TMP_Text arTitle;
    private TMP_Text arSubtitle;
    private TMP_Text trackingText;
    private GameObject trackingPill;
    private float trackingLostTimer;

    private GameObject bottomDrawer;
    private TMP_Text drawerCategory;
    private TMP_Text drawerTitle;
    private TMP_Text drawerDescription;

    private Button arLearnButton;
    private Button arQuizButton;
    private Button arAnimationButton;
    private Button arPartsButton;
    private bool partLabelsVisible = true;

    private EduARModelAnimationController heartAnimation;
    private EduARModelAnimationController brainAnimation;
    private EduARModelAnimationController lungsAnimation;
    private EduARModelAnimationController physicsAnimation;
    private EduARModelAnimationController chemistryAnimation;

    // Readable educational callouts that follow the detected 3D model.
    private EduARModelPartLabelController heartPartLabels;
    private EduARModelPartLabelController brainPartLabels;
    private EduARModelPartLabelController lungsPartLabels;
    private EduARModelPartLabelController physicsPartLabels;
    private EduARModelPartLabelController chemistryPartLabels;

    [Header("AR Model Presentation")]
    [SerializeField] private float mobileModelScaleMultiplier = 1.40f;

    // =========================================================
    // UNIVERSAL SCANNER UI
    // =========================================================

    private TMP_Text universalScanTitle;
    private TMP_Text universalScanSubtitle;
    private TMP_Text universalScanStatus;
    private TMP_Text universalScanHint;
    private TMP_Text universalScanSupported;
    private GameObject universalScannerFrame;
    private GameObject universalScannerGlow;
    private Coroutine universalScannerPulseRoutine;
    private bool universalScanActive;
    private bool universalScanLocked;

    // Prevents the same AR scan from being recorded more than once
    // while moving from detection into the AR lesson screen.
    private bool currentScanRecorded;

    private GameObject lastDetectedTarget;

    // Universal scanner stability and feedback state. Initial acquisition
    // uses TRACKED only; Extended Tracking is still used after the AR lesson opens.
    private readonly Dictionary<GameObject, int> scannerStableFrames =
        new Dictionary<GameObject, int>();
    private float scannerNoTargetTimer;
    private const int scannerStableFramesRequired = 1;
    private const float scannerHintDelay = 1.2f;

    private GameObject scannerStatusCard;
    private TMP_Text scannerStatusCardTitle;
    private TMP_Text scannerStatusCardBody;
    private Coroutine scannerSuccessRoutine;

    // Detection result card shown after a lesson image is recognized.

    private GameObject learningPopup;
    private TMP_Text learningPopupTitle;
    private TMP_Text learningPopupCategory;
    private TMP_Text learningPopupBody;
    private Button learningPopupClose;

    // =========================================================
    // QUIZ REFERENCES
    // =========================================================

    private TMP_Text quizTitle;
    private TMP_Text quizProgress;
    private TMP_Text quizQuestion;
    private TMP_Text quizScore;

    private Button option1Button;
    private Button option2Button;
    private Button option3Button;
    private Button option4Button;

    private TMP_Text option1Text;
    private TMP_Text option2Text;
    private TMP_Text option3Text;
    private TMP_Text option4Text;

    private UIImage option1Image;
    private UIImage option2Image;
    private UIImage option3Image;
    private UIImage option4Image;

    private Button nextQuizButton;
    private UIImage quizProgressFill;

    // =========================================================
    // RESULT REFERENCES
    // =========================================================

    private TMP_Text resultScore;
    private TMP_Text resultMessage;

    // =========================================================
    // AUTHENTICATION UI REFERENCES
    // =========================================================

    private GameObject authScreen;
    private GameObject loginPanel;
    private GameObject registerPanel;

    private TMP_InputField loginEmailInput;
    private TMP_InputField loginPasswordInput;

    private TMP_InputField registerNameInput;
    private TMP_InputField registerStudentIdInput;
    private TMP_InputField registerEmailInput;
    private TMP_InputField registerPasswordInput;
    private TMP_InputField registerConfirmPasswordInput;

    private TMP_Text loginStatusText;
    private TMP_Text registerStatusText;

    // =========================================================
    // QUIZ DATA STRUCTURES
    // =========================================================

    private readonly List<QuestionData> currentQuiz = new List<QuestionData>();
    private int currentQuestionIndex;
    private int currentScore;
    private bool answerSelected;

    [Serializable]
    private class QuestionData
    {
        public string question;
        public string[] answers;
        public int correctIndex;

        public QuestionData(string question, string[] answers, int correctIndex)
        {
            this.question = question;
            this.answers = answers;
            this.correctIndex = correctIndex;
        }

        public QuestionData Clone()
        {
            return new QuestionData(question, (string[])answers.Clone(), correctIndex);
        }
    }

    private class AnswerPair
    {
        public string text;
        public bool correct;
    }

    private readonly List<QuestionData> heartQuiz = new List<QuestionData>
    {
        new QuestionData("Which chamber pumps oxygenated blood to the body?", new[] { "Right atrium", "Right ventricle", "Left atrium", "Left ventricle" }, 3),
        new QuestionData("Which organ pumps blood throughout the body?", new[] { "Brain", "Heart", "Lungs", "Kidney" }, 1),
        new QuestionData("Which blood vessel carries oxygen-rich blood from the heart?", new[] { "Aorta", "Vena cava", "Pulmonary vein", "Pulmonary artery" }, 0),
        new QuestionData("How many chambers does the human heart have?", new[] { "2", "3", "4", "5" }, 2),
        new QuestionData("Which side of the heart receives deoxygenated blood?", new[] { "Left side", "Right side", "Both sides", "Neither side" }, 1)
    };

    private readonly List<QuestionData> brainQuiz = new List<QuestionData>
    {
        new QuestionData("What is the main control center of the nervous system?", new[] { "Heart", "Brain", "Lungs", "Kidney" }, 1),
        new QuestionData("Which part of the brain controls balance and coordination?", new[] { "Cerebrum", "Cerebellum", "Brainstem", "Medulla" }, 1),
        new QuestionData("What is the largest part of the brain?", new[] { "Cerebellum", "Brainstem", "Cerebrum", "Medulla" }, 2),
        new QuestionData("Which structure connects the brain to the spinal cord?", new[] { "Cerebrum", "Cerebellum", "Brainstem", "Hippocampus" }, 2),
        new QuestionData("Which structure is strongly associated with memory?", new[] { "Medulla", "Hippocampus", "Cerebellum", "Brainstem" }, 1)
    };

    private readonly List<QuestionData> lungsQuiz = new List<QuestionData>
    {
        new QuestionData("What is the main function of the lungs?", new[] { "Pump blood", "Exchange gases", "Digest food", "Control movement" }, 1),
        new QuestionData("Which tube carries air toward the lungs?", new[] { "Aorta", "Trachea", "Esophagus", "Stomach" }, 1),
        new QuestionData("Where does gas exchange mainly occur?", new[] { "Alveoli", "Heart", "Atrium", "Aorta" }, 0),
        new QuestionData("How many lungs does a healthy person normally have?", new[] { "1", "2", "3", "4" }, 1),
        new QuestionData("Which gas is taken into the body during breathing?", new[] { "Carbon dioxide", "Nitrogen", "Oxygen", "Hydrogen" }, 2)
    };

    private readonly List<QuestionData> physicsQuiz = new List<QuestionData>
    {
        new QuestionData("What force pulls a pendulum toward the ground?", new[] { "Friction", "Gravity", "Magnetism", "Electricity" }, 1),
        new QuestionData("What type of motion does a simple pendulum demonstrate?", new[] { "Oscillatory motion", "Linear motion", "Random motion", "Projectile motion" }, 0),
        new QuestionData("What happens to the pendulum's speed at its lowest point?", new[] { "It is highest", "It becomes zero", "It remains constant", "It disappears" }, 0),
        new QuestionData("What mainly affects the period of a simple pendulum?", new[] { "String length", "Color", "Room shape", "Material color" }, 0),
        new QuestionData("A longer pendulum generally has a...", new[] { "Shorter period", "Longer period", "Zero period", "Random period" }, 1)
    };

    private readonly List<QuestionData> chemistryQuiz = new List<QuestionData>
    {
        new QuestionData("What is the chemical formula of water?", new[] { "CO2", "H2O", "O2", "NaCl" }, 1),
        new QuestionData("How many hydrogen atoms are in one water molecule?", new[] { "1", "2", "3", "4" }, 1),
        new QuestionData("Which element is represented by O in H2O?", new[] { "Oxygen", "Hydrogen", "Carbon", "Nitrogen" }, 0),
        new QuestionData("What type of bond connects hydrogen and oxygen in water?", new[] { "Covalent bond", "Metallic bond", "Nuclear bond", "Magnetic bond" }, 0),
        new QuestionData("Water is best described as a...", new[] { "Compound", "Pure element", "Metal", "Mixture of metals" }, 0)
    };

    // =========================================================
    // START & LIFECYCLE
    // =========================================================

    private void Start()
    {
        DisableAllTargets();

        EduARLocalAuth.Initialize();

        CreateEventSystem();
        CreateCanvas();

        BuildAuthScreen();
        BuildHomeScreen();
        BuildTopicScreen();
        BuildARScreen();
        BuildUniversalScannerScreen();
        BuildQuizScreen();
        BuildResultScreen();
        BuildFeatureScreens();
        BuildHomeFeatureUI();
        SetupModelAnimationControllers();
        SetupModelTouchControllers();
        SetupModelPartLabels();

        // Always begin on the Home screen. The camera scanner opens only
        // after the user presses the single SCAN button on Home.
        homeScreen.SetActive(false);
        universalScanScreen.SetActive(false);

        // Unity fullscreen state.
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = true;

        // Android immersive fullscreen.
        ApplyImmersiveMode();
        StartCoroutine(ForceImmersiveAfterStartup());

        // Local authentication comes first.
        // Account records are stored on the device; no online database is required.
        if (EduARLocalAuth.TryGetCurrentUser(out EduARLocalUser sessionUser))
        {
            EduARLocalProgress.Initialize(sessionUser.userId);
            ShowHome();
        }
        else
        {
            ShowLogin();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            return;

        // Android can restore system bars when a permission dialog,
        // camera initialization, or another Activity transition finishes.
        ApplyImmersiveMode();
        StartCoroutine(ReapplyImmersiveAfterFocus());
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            return;

        ApplyImmersiveMode();
        StartCoroutine(ReapplyImmersiveAfterFocus());
    }

    private IEnumerator ForceImmersiveAfterStartup()
    {
        // Give Android/Unity/Vuforia time to finish creating the Activity/window.
        yield return null;
        ApplyImmersiveMode();

        yield return new WaitForEndOfFrame();
        ApplyImmersiveMode();

        yield return new WaitForSecondsRealtime(0.15f);
        ApplyImmersiveMode();

        yield return new WaitForSecondsRealtime(0.35f);
        ApplyImmersiveMode();

        yield return new WaitForSecondsRealtime(0.75f);
        ApplyImmersiveMode();
    }

    private IEnumerator ReapplyImmersiveAfterFocus()
    {
        // Reapply after Android has finished restoring focus/window flags.
        yield return null;
        ApplyImmersiveMode();

        yield return new WaitForEndOfFrame();
        ApplyImmersiveMode();

        yield return new WaitForSecondsRealtime(0.15f);
        ApplyImmersiveMode();

        yield return new WaitForSecondsRealtime(0.35f);
        ApplyImmersiveMode();
    }

    public static void ApplyImmersiveMode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Get the current Activity only to schedule work on Android's UI thread.
            // The callback obtains a fresh Activity reference so we never use a
            // disposed AndroidJavaObject from outside the UI-thread callback.
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                if (currentActivity == null)
                    return;

                currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        using (var callbackUnityPlayer =
                               new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                        using (var activity =
                               callbackUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            if (activity == null)
                                return;

                            using (var window = activity.Call<AndroidJavaObject>("getWindow"))
                            using (var decorView = window.Call<AndroidJavaObject>("getDecorView"))
                            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                            {
                                int sdkInt = version.GetStatic<int>("SDK_INT");

                                // -------------------------------------------------
                                // A. Force a real fullscreen window state.
                                // -------------------------------------------------

                                // WindowManager.LayoutParams.FLAG_FULLSCREEN = 0x00000004
                                window.Call("addFlags", 0x00000004);

                                // Clear FLAG_FORCE_NOT_FULLSCREEN if another
                                // component previously set it.
                                // WindowManager.LayoutParams.FLAG_FORCE_NOT_FULLSCREEN = 0x00000800
                                window.Call("clearFlags", 0x00000800);

                                // -------------------------------------------------
                                // B. Allow content to use display-cutout areas.
                                // -------------------------------------------------

                                if (sdkInt >= 28)
                                {
                                    using (var layoutParams =
                                           window.Call<AndroidJavaObject>("getAttributes"))
                                    {
                                        // API 30+ accepts ALWAYS (3). Older supported
                                        // versions use SHORT_EDGES (1).
                                        int cutoutMode = sdkInt >= 30 ? 3 : 1;
                                        layoutParams.Set("layoutInDisplayCutoutMode", cutoutMode);
                                        window.Call("setAttributes", layoutParams);
                                    }
                                }

                                // -------------------------------------------------
                                // C. Android 11+ modern WindowInsets API.
                                // -------------------------------------------------

                                if (sdkInt >= 30)
                                {
                                    // Edge-to-edge window layout.
                                    try
                                    {
                                        window.Call("setDecorFitsSystemWindows", false);
                                    }
                                    catch
                                    {
                                        // Ignore on devices where this call is unavailable.
                                    }

                                    try
                                    {
                                        using (var insetsController =
                                               window.Call<AndroidJavaObject>("getInsetsController"))
                                        using (var insetsType =
                                               new AndroidJavaClass("android.view.WindowInsets$Type"))
                                        {
                                            if (insetsController != null)
                                            {
                                                // Type.systemBars() includes the status
                                                // and navigation bars (and caption bar).
                                                int systemBars =
                                                    insetsType.CallStatic<int>("systemBars");

                                                // Hide the system bars.
                                                insetsController.Call("hide", systemBars);

                                                // BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE = 2
                                                insetsController.Call(
                                                    "setSystemBarsBehavior",
                                                    2
                                                );
                                            }
                                        }
                                    }
                                    catch (Exception insetsEx)
                                    {
                                        Debug.LogWarning(
                                            "[EduAR] WindowInsets immersive call failed: " +
                                            insetsEx.Message
                                        );
                                    }
                                }

                                // -------------------------------------------------
                                // D. Legacy immersive-sticky flags.
                                // Apply them even on modern Android as an additional
                                // compatibility layer for Unity/device skins.
                                // -------------------------------------------------

                                const int SYSTEM_UI_FLAG_LAYOUT_STABLE = 0x00000100;
                                const int SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION = 0x00000200;
                                const int SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN = 0x00000400;
                                const int SYSTEM_UI_FLAG_FULLSCREEN = 0x00000004;
                                const int SYSTEM_UI_FLAG_HIDE_NAVIGATION = 0x00000002;
                                const int SYSTEM_UI_FLAG_IMMERSIVE_STICKY = 0x00001000;

                                int immersiveFlags =
                                    SYSTEM_UI_FLAG_LAYOUT_STABLE |
                                    SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION |
                                    SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN |
                                    SYSTEM_UI_FLAG_FULLSCREEN |
                                    SYSTEM_UI_FLAG_HIDE_NAVIGATION |
                                    SYSTEM_UI_FLAG_IMMERSIVE_STICKY;

                                decorView.Call("setSystemUiVisibility", immersiveFlags);

                                // A second application helps with OEM/Unity transitions
                                // that immediately rewrite the decor-view flags.
                                decorView.Call("setSystemUiVisibility", immersiveFlags);

                                // Keep status/navigation bars transparent where supported.
                                // This is useful when the bars briefly animate/appear.
                                try
                                {
                                    window.Call("setStatusBarColor", 0x00000000);
                                }
                                catch { }

                                try
                                {
                                    window.Call("setNavigationBarColor", 0x00000000);
                                }
                                catch { }

                                // Reduce the chance of the 3-button navigation mode
                                // adding a contrast scrim on supported API levels.
                                if (sdkInt >= 29)
                                {
                                    try
                                    {
                                        window.Call("setNavigationBarContrastEnforced", false);
                                    }
                                    catch { }
                                }

                                Debug.Log(
                                    "[EduAR] Immersive fullscreen applied. Android SDK=" +
                                    sdkInt
                                );
                            }
                        }
                    }
                    catch (Exception callbackEx)
                    {
                        Debug.LogWarning(
                            "[EduAR] Immersive UI-thread error: " +
                            callbackEx.Message
                        );
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[EduAR] Immersive mode error: " +
                ex.Message
            );
        }
#endif
    }

    private void Update()
    {
        UpdateSafeArea();
        UpdateTrackingStatus();
        UpdateUniversalScannerStatus();
        UpdateKeyboardAwareAuthLayout();
    }

    // =========================================================
    // EVENT SYSTEM & CANVAS SETUP
    // =========================================================

    private void CreateEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EduAR EventSystem");
            eventSystem = eventObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("EduAR Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject safeObject = CreateUIObject("SafeArea", canvasObject.transform);
        safeArea = safeObject.GetComponent<RectTransform>();
        StretchFull(safeArea);

        UpdateSafeArea();
    }

    private void UpdateSafeArea()
    {
        if (safeArea == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect area = Screen.safeArea;
        Vector2 min = area.position;
        Vector2 max = area.position + area.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        safeArea.anchorMin = min;
        safeArea.anchorMax = max;
        safeArea.offsetMin = Vector2.zero;
        safeArea.offsetMax = Vector2.zero;
    }


    // =========================================================
    // AUTHENTICATION SCREEN
    // =========================================================

    private void BuildAuthScreen()
    {
        authScreen = CreateScreen("Authentication");
        CreateBackground(authScreen, bgDark);

        // Brand header - same visual language as the existing app.
        TMP_Text brand = CreateText(
            "EDUAR 3D",
            44,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            authScreen.transform
        );

        SetRect(
            brand.rectTransform,
            new Vector2(0.08f, 0.86f),
            new Vector2(0.92f, 0.93f)
        );

        TMP_Text subtitle = CreateText(
            "Interactive AR Learning",
            21,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            authScreen.transform
        );

        SetRect(
            subtitle.rectTransform,
            new Vector2(0.08f, 0.82f),
            new Vector2(0.92f, 0.87f)
        );

        loginPanel = CreateRoundedPanel(
            "LoginPanel",
            authScreen.transform,
            cardDark
        );
        AddBorder(loginPanel, cardBorder);

        SetRect(
            loginPanel.GetComponent<RectTransform>(),
            new Vector2(0.06f, 0.16f),
            new Vector2(0.94f, 0.79f)
        );

        registerPanel = CreateRoundedPanel(
            "RegisterPanel",
            authScreen.transform,
            cardDark
        );
        AddBorder(registerPanel, cardBorder);

        SetRect(
            registerPanel.GetComponent<RectTransform>(),
            new Vector2(0.06f, 0.08f),
            new Vector2(0.94f, 0.81f)
        );

        BuildLoginPanel();
        BuildRegisterPanel();

        registerPanel.SetActive(false);
        authScreen.SetActive(false);
    }

    private void BuildLoginPanel()
    {
        TMP_Text title = CreateText(
            "Welcome Back",
            36,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Left,
            loginPanel.transform
        );

        SetRect(
            title.rectTransform,
            new Vector2(0.08f, 0.83f),
            new Vector2(0.92f, 0.92f)
        );

        TMP_Text intro = CreateText(
            "Sign in to continue your learning journey.",
            20,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Left,
            loginPanel.transform
        );

        SetTextWrapping(intro);

        SetRect(
            intro.rectTransform,
            new Vector2(0.08f, 0.74f),
            new Vector2(0.92f, 0.82f)
        );

        loginEmailInput = CreateAuthInputField(
            "Email",
            false,
            loginPanel.transform,
            new Vector2(0.08f, 0.56f),
            new Vector2(0.92f, 0.66f)
        );

        loginPasswordInput = CreateAuthInputField(
            "Password",
            true,
            loginPanel.transform,
            new Vector2(0.08f, 0.41f),
            new Vector2(0.92f, 0.51f)
        );

        Button loginButton = CreateButton(
            "LOGIN",
            loginPanel.transform,
            brandIndigo,
            Color.white,
            22
        );

        SetRect(
            loginButton.GetComponent<RectTransform>(),
            new Vector2(0.08f, 0.28f),
            new Vector2(0.92f, 0.38f)
        );

        loginButton.onClick.AddListener(HandleLogin);
        AddPressAnimation(loginButton);

        loginStatusText = CreateText(
            "",
            18,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            loginPanel.transform
        );

        SetTextWrapping(loginStatusText);

        SetRect(
            loginStatusText.rectTransform,
            new Vector2(0.08f, 0.18f),
            new Vector2(0.92f, 0.26f)
        );

        TMP_Text registerPrompt = CreateText(
            "New to EduAR 3D?",
            18,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Left,
            loginPanel.transform
        );

        SetRect(
            registerPrompt.rectTransform,
            new Vector2(0.08f, 0.06f),
            new Vector2(0.43f, 0.13f)
        );

        Button registerButton = CreateButton(
            "CREATE ACCOUNT",
            loginPanel.transform,
            buttonPillDark,
            brandIndigo,
            20
        );

        AddBorder(registerButton.gameObject, glassBorder);

        SetRect(
            registerButton.GetComponent<RectTransform>(),
            new Vector2(0.44f, 0.055f),
            new Vector2(0.92f, 0.135f)
        );

        registerButton.onClick.AddListener(ShowRegister);
        AddPressAnimation(registerButton);
    }

    private void BuildRegisterPanel()
    {
        TMP_Text title = CreateText(
            "Create Account",
            32,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Left,
            registerPanel.transform
        );

        SetRect(
            title.rectTransform,
            new Vector2(0.08f, 0.87f),
            new Vector2(0.92f, 0.95f)
        );

        TMP_Text intro = CreateText(
            "Create a student profile for EduAR 3D.",
            20,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Left,
            registerPanel.transform
        );

        SetRect(
            intro.rectTransform,
            new Vector2(0.08f, 0.80f),
            new Vector2(0.92f, 0.86f)
        );

        registerNameInput = CreateAuthInputField(
            "Full Name",
            false,
            registerPanel.transform,
            new Vector2(0.08f, 0.66f),
            new Vector2(0.92f, 0.74f)
        );

        registerStudentIdInput = CreateAuthInputField(
            "Student ID",
            false,
            registerPanel.transform,
            new Vector2(0.08f, 0.55f),
            new Vector2(0.92f, 0.63f)
        );

        registerEmailInput = CreateAuthInputField(
            "Email",
            false,
            registerPanel.transform,
            new Vector2(0.08f, 0.44f),
            new Vector2(0.92f, 0.52f)
        );

        registerPasswordInput = CreateAuthInputField(
            "Password",
            true,
            registerPanel.transform,
            new Vector2(0.08f, 0.33f),
            new Vector2(0.92f, 0.41f)
        );

        registerConfirmPasswordInput = CreateAuthInputField(
            "Confirm Password",
            true,
            registerPanel.transform,
            new Vector2(0.08f, 0.22f),
            new Vector2(0.92f, 0.30f)
        );

        Button registerButton = CreateButton(
            "CREATE ACCOUNT",
            registerPanel.transform,
            brandIndigo,
            Color.white,
            20
        );

        SetRect(
            registerButton.GetComponent<RectTransform>(),
            new Vector2(0.08f, 0.12f),
            new Vector2(0.92f, 0.19f)
        );

        registerButton.onClick.AddListener(HandleRegister);
        AddPressAnimation(registerButton);

        registerStatusText = CreateText(
            "",
            18,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            registerPanel.transform
        );

        SetTextWrapping(registerStatusText);

        SetRect(
            registerStatusText.rectTransform,
            new Vector2(0.08f, 0.06f),
            new Vector2(0.92f, 0.115f)
        );

        Button loginButton = CreateButton(
            "ALREADY HAVE AN ACCOUNT? LOGIN",
            registerPanel.transform,
            buttonPillDark,
            textWhite,
            18
        );

        AddBorder(loginButton.gameObject, glassBorder);

        SetRect(
            loginButton.GetComponent<RectTransform>(),
            new Vector2(0.14f, 0.008f),
            new Vector2(0.86f, 0.055f)
        );

        loginButton.onClick.AddListener(ShowLogin);
        AddPressAnimation(loginButton);
    }

    private TMP_InputField CreateAuthInputField(
        string placeholder,
        bool password,
        Transform parent,
        Vector2 min,
        Vector2 max
    )
    {
        GameObject inputObject = CreateRoundedPanel(
            "Input_" + placeholder.Replace(" ", ""),
            parent,
            new Color(0.09f, 0.12f, 0.19f, 1f)
        );

        AddBorder(inputObject, glassBorder);

        SetRect(
            inputObject.GetComponent<RectTransform>(),
            min,
            max
        );

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = password
            ? TMP_InputField.ContentType.Password
            : TMP_InputField.ContentType.Standard;

        GameObject textObject = CreateUIObject(
            "Text",
            inputObject.transform
        );

        TMP_Text textComponent =
            textObject.AddComponent<TextMeshProUGUI>();

        if (appFont != null)
            textComponent.font = appFont;

        textComponent.fontSize = 21;
        textComponent.color = textWhite;
        textComponent.alignment = TextAlignmentOptions.Left;

        SetRect(
            textComponent.rectTransform,
            new Vector2(0.05f, 0.05f),
            new Vector2(0.95f, 0.95f)
        );

        GameObject placeholderObject = CreateUIObject(
            "Placeholder",
            inputObject.transform
        );

        TMP_Text placeholderText =
            placeholderObject.AddComponent<TextMeshProUGUI>();

        if (appFont != null)
            placeholderText.font = appFont;

        placeholderText.text = placeholder;
        placeholderText.fontSize = 21;
        placeholderText.color = textMuted;
        placeholderText.alignment = TextAlignmentOptions.Left;

        SetRect(
            placeholderText.rectTransform,
            new Vector2(0.05f, 0.05f),
            new Vector2(0.95f, 0.95f)
        );

        input.textComponent = textComponent;
        input.placeholder = placeholderText;

        return input;
    }

    private void UpdateKeyboardAwareAuthLayout()
    {
        if (authScreen == null || !authScreen.activeSelf)
        {
            RestoreAuthPanelPosition();
            return;
        }

        bool keyboardVisible = false;
#if UNITY_ANDROID || UNITY_IOS
        keyboardVisible = TouchScreenKeyboard.visible;
#endif

        GameObject activePanel = null;
        if (registerPanel != null && registerPanel.activeSelf)
            activePanel = registerPanel;
        else if (loginPanel != null && loginPanel.activeSelf)
            activePanel = loginPanel;

        if (activePanel == null || !keyboardVisible)
        {
            RestoreAuthPanelPosition();
            return;
        }

        RectTransform panelRect = activePanel.GetComponent<RectTransform>();
        if (panelRect == null)
            return;

        float keyboardHeight = 0f;
#if UNITY_ANDROID || UNITY_IOS
        keyboardHeight = TouchScreenKeyboard.area.height;
#endif

        if (keyboardHeight <= 0f)
            keyboardHeight = Screen.height * 0.34f;

        float scale = canvas != null ? Mathf.Max(canvas.scaleFactor, 0.01f) : 1f;
        float keyboardCanvasHeight = keyboardHeight / scale;

        // Move the panel upward enough to keep the active field above the keyboard.
        float shift = Mathf.Clamp(keyboardCanvasHeight * 0.42f, 170f, 380f);

        GameObject selected = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        if (registerPanel != null && registerPanel.activeSelf)
        {
            if (selected == registerConfirmPasswordInput?.gameObject)
                shift = Mathf.Clamp(shift + 60f, 220f, 400f);
            else if (selected == registerPasswordInput?.gameObject)
                shift = Mathf.Clamp(shift + 30f, 200f, 390f);
        }

        panelRect.anchoredPosition = new Vector2(0f, shift);
        keyboardLayoutActive = true;
    }

    private void RestoreAuthPanelPosition()
    {
        if (!keyboardLayoutActive)
            return;

        if (registerPanel != null)
            registerPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        if (loginPanel != null)
            loginPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        keyboardLayoutActive = false;
    }

    private void ShowLogin()
    {
        if (authScreen == null)
            return;

        DisableAllTargets();
        HideLearningPopup();

        homeScreen?.SetActive(false);
        topicScreen?.SetActive(false);
        arScreen?.SetActive(false);
        quizScreen?.SetActive(false);
        resultScreen?.SetActive(false);

        registerPanel?.SetActive(false);
        loginPanel?.SetActive(true);
        authScreen.SetActive(true);

        if (loginStatusText != null)
            loginStatusText.text = "";

        StartCoroutine(FadeInScreen(authScreen));
    }

    private void ShowRegister()
    {
        if (authScreen == null)
            return;

        DisableAllTargets();
        HideLearningPopup();

        homeScreen?.SetActive(false);
        topicScreen?.SetActive(false);
        arScreen?.SetActive(false);
        quizScreen?.SetActive(false);
        resultScreen?.SetActive(false);

        loginPanel?.SetActive(false);
        registerPanel?.SetActive(true);
        authScreen.SetActive(true);

        if (registerStatusText != null)
            registerStatusText.text = "";

        StartCoroutine(FadeInScreen(authScreen));
    }

    private void HandleLogin()
    {
        if (loginEmailInput == null || loginPasswordInput == null)
            return;

        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrEmpty(password))
        {
            SetLoginStatus(
                "Enter your email and password.",
                wrongRed
            );
            return;
        }

        if (!EduARLocalAuth.TryLogin(
                email,
                password,
                out EduARLocalUser user,
                out string error))
        {
            SetLoginStatus(error, wrongRed);
            return;
        }

        Debug.Log("[EduAR] Local login successful: " + user.fullName);
        EduARLocalProgress.Initialize(user.userId);
        ShowHome();
    }

    private void HandleRegister()
    {
        string fullName = registerNameInput.text.Trim();
        string studentId = registerStudentIdInput.text.Trim();
        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(studentId) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(confirmPassword))
        {
            SetRegisterStatus(
                "Please complete all fields.",
                wrongRed
            );
            return;
        }

        if (!email.Contains("@") || !email.Contains("."))
        {
            SetRegisterStatus(
                "Enter a valid email address.",
                wrongRed
            );
            return;
        }

        if (password.Length < 6)
        {
            SetRegisterStatus(
                "Password must be at least 6 characters.",
                wrongRed
            );
            return;
        }

        if (password != confirmPassword)
        {
            SetRegisterStatus(
                "Passwords do not match.",
                wrongRed
            );
            return;
        }

        if (!EduARLocalAuth.TryRegister(
                fullName,
                studentId,
                email,
                password,
                out EduARLocalUser user,
                out string error))
        {
            SetRegisterStatus(error, wrongRed);
            return;
        }

        Debug.Log("[EduAR] Local registration successful: " + user.fullName);

        loginEmailInput.text = email;
        loginPasswordInput.text = "";

        SetRegisterStatus(
            "Account created. Please log in.",
            correctGreen
        );

        ShowLogin();
        SetLoginStatus(
            "Account created successfully. Please log in.",
            correctGreen
        );
    }

    private void SetLoginStatus(string message, Color color)
    {
        if (loginStatusText == null)
            return;

        loginStatusText.color = color;
        loginStatusText.text = message;
    }

    private void SetRegisterStatus(string message, Color color)
    {
        if (registerStatusText == null)
            return;

        registerStatusText.color = color;
        registerStatusText.text = message;
    }

    // =========================================================
    // HOME SCREEN
    // =========================================================

    private void BuildHomeScreen()
    {
        homeScreen = CreateScreen("Home");
        CreateBackground(homeScreen, bgDark);

        // =====================================================
        // CLEAN MOBILE-FIRST HOME
        // The home screen has one primary action: scanning a
        // registered lesson image. Subject/model selection is not
        // required here; Vuforia identifies the lesson after scan.
        // Avatar + DASHBOARD are added by BuildHomeFeatureUI().
        // =====================================================

        TMP_Text welcomeTitle = CreateText(
            "LEARN WITH AR",
            34,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            homeScreen.transform
        );
        SetRect(welcomeTitle.rectTransform,
            new Vector2(0.08f, 0.70f),
            new Vector2(0.92f, 0.77f));

        TMP_Text welcomeSubtitle = CreateText(
            "Scan your lesson image to start an interactive 3D lesson.",
            20,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            homeScreen.transform
        );
        SetTextWrapping(welcomeSubtitle);
        SetRect(welcomeSubtitle.rectTransform,
            new Vector2(0.12f, 0.62f),
            new Vector2(0.88f, 0.70f));

        GameObject scanCard = CreateRoundedPanel(
            "ScanLessonCard",
            homeScreen.transform,
            cardDark
        );
        AddBorder(scanCard, new Color(1f, 0.39f, 0.28f, 0.24f));
        SetRect(scanCard.GetComponent<RectTransform>(),
            new Vector2(0.06f, 0.30f),
            new Vector2(0.94f, 0.58f));

        TMP_Text scanLabel = CreateText(
            "LESSON SCANNER",
            18,
            FontStyles.Bold,
            scanTomato,
            TextAlignmentOptions.Center,
            scanCard.transform
        );
        SetRect(scanLabel.rectTransform,
            new Vector2(0.08f, 0.74f),
            new Vector2(0.92f, 0.92f));

        TMP_Text scanInfo = CreateText(
            "Point your camera at any registered lesson image.",
            19,
            FontStyles.Normal,
            textWhite,
            TextAlignmentOptions.Center,
            scanCard.transform
        );
        SetTextWrapping(scanInfo);
        SetRect(scanInfo.rectTransform,
            new Vector2(0.10f, 0.57f),
            new Vector2(0.90f, 0.74f));

        Button scanButton = CreateButton(
            "SCAN LESSON",
            scanCard.transform,
            scanTomato,
            Color.white,
            26
        );
        AddBorder(scanButton.gameObject, new Color(1f, 1f, 1f, 0.10f));
        SetRect(scanButton.GetComponent<RectTransform>(),
            new Vector2(0.10f, 0.20f),
            new Vector2(0.90f, 0.49f));
        scanButton.onClick.AddListener(OpenUniversalScanner);
        AddPressAnimation(scanButton);

        TMP_Text scanHint = CreateText(
            "Automatic detection • 3D model appears instantly",
            17,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            scanCard.transform
        );
        SetTextWrapping(scanHint);
        SetRect(scanHint.rectTransform,
            new Vector2(0.08f, 0.06f),
            new Vector2(0.92f, 0.18f));

        TMP_Text footerHint = CreateText(
            "No subject selection needed",
            18,
            FontStyles.Bold,
            textMuted,
            TextAlignmentOptions.Center,
            homeScreen.transform
        );
        SetRect(footerHint.rectTransform,
            new Vector2(0.10f, 0.21f),
            new Vector2(0.90f, 0.26f));
    }

    // =========================================================
    // TOPIC SCREEN
    private void SetDrawerCategoryText(string subject, Color subjectColor)
    {
        if (drawerCategory == null)
            return;

        // Make the current subject the visual focus while keeping the module
        // descriptor secondary for readability on mobile screens.
        drawerCategory.richText = true;
        drawerCategory.fontSize = 26f;
        drawerCategory.fontStyle = FontStyles.Bold;
        drawerCategory.color = subjectColor;
        drawerCategory.text =
            "<size=108%>" + subject.ToUpper() +
            "</size> <size=72%>• 3D MODULE</size>";
    }

    // =========================================================

    private void BuildTopicScreen()
    {
        topicScreen = CreateScreen("Topics");
        CreateBackground(topicScreen, bgDark);
    }

    private void ShowTopicBrowser(string subject)
    {
        currentSubject = subject;
        ClearChildren(topicScreen);

        CreateBackground(topicScreen, bgDark);
        Color subjectColor = GetSubjectColor(subject);

        // Highly visible < Back button
        Button backBtn = CreateButton("< Back", topicScreen.transform, buttonPillDark, textWhite, 20);
        AddBorder(backBtn.gameObject, glassBorder);
        SetRect(backBtn.GetComponent<RectTransform>(), new Vector2(0.05f, 0.90f), new Vector2(0.26f, 0.96f));
        backBtn.onClick.AddListener(ShowHome);

        TMP_Text title = CreateText(subject, 42, FontStyles.Bold, textWhite, TextAlignmentOptions.Center, topicScreen.transform);
        SetRect(title.rectTransform, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.89f));

        TMP_Text subtitle = CreateText("SELECT A 3D TOPIC", 19, FontStyles.Bold, subjectColor, TextAlignmentOptions.Center, topicScreen.transform);
        SetRect(subtitle.rectTransform, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.82f));

        if (subject == "Biology")
        {
            CreateTopicCard("Human Heart", "Explore chambers, blood flow and major vessels.", 0.64f, subjectColor, () => OpenAR("Heart"));
            CreateTopicCard("Human Brain", "Discover major brain structures and functions.", 0.48f, subjectColor, () => OpenAR("Brain"));
            CreateTopicCard("Human Lungs", "Explore breathing and gas exchange mechanics.", 0.32f, subjectColor, () => OpenAR("Lungs"));
        }
        else if (subject == "Physics")
        {
            CreateTopicCard("Simple Pendulum", "Explore gravity, oscillation and kinetic energy.", 0.56f, subjectColor, () => OpenAR("Pendulum"));
        }
        else if (subject == "Chemistry")
        {
            CreateTopicCard("Water Molecule", "Explore H2O geometry and covalent bonding.", 0.56f, subjectColor, () => OpenAR("Water"));
        }

        topicScreen.SetActive(true);
        StartCoroutine(FadeInScreen(topicScreen));
    }

    private void CreateTopicCard(string title, string description, float centerY, Color subjectColor, UnityEngine.Events.UnityAction action)
    {
        GameObject card = CreateRoundedPanel("TopicCard", topicScreen.transform, cardDark);
        AddBorder(card, cardBorder);

        SetRect(card.GetComponent<RectTransform>(),
            new Vector2(0.05f, centerY - 0.065f),
            new Vector2(0.95f, centerY + 0.065f));

        TMP_Text titleText = CreateText(title, 30, FontStyles.Bold, textWhite, TextAlignmentOptions.Left, card.transform);
        SetRect(titleText.rectTransform, new Vector2(0.06f, 0.52f), new Vector2(0.68f, 0.88f));

        TMP_Text descText = CreateText(description, 19, FontStyles.Normal, textMuted, TextAlignmentOptions.Left, card.transform);
        SetTextWrapping(descText);
        SetRect(descText.rectTransform, new Vector2(0.06f, 0.12f), new Vector2(0.68f, 0.50f));

        Button open = CreateButton("OPEN", card.transform, subjectColor, Color.white, 20);
        SetRect(open.GetComponent<RectTransform>(), new Vector2(0.70f, 0.25f), new Vector2(0.95f, 0.75f));

        open.onClick.AddListener(action);
        AddPressAnimation(open);
    }

    // =========================================================
    // UNIVERSAL AR SCANNER
    // =========================================================

    private void BuildUniversalScannerScreen()
    {
        universalScanScreen = CreateScreen("UniversalScanner");
        CreateBackground(universalScanScreen, new Color(0f, 0f, 0f, 0.12f));

        GameObject header = CreateRoundedPanel(
            "UniversalScannerHeader",
            universalScanScreen.transform,
            glassHUD
        );
        AddBorder(header, glassBorder);
        SetRect(header.GetComponent<RectTransform>(),
            new Vector2(0.04f, 0.885f),
            new Vector2(0.96f, 0.985f));

        Button back = CreateButton(
            "< Back",
            header.transform,
            buttonPillDark,
            textWhite,
            21
        );
        AddBorder(back.gameObject, glassBorder);
        SetRect(back.GetComponent<RectTransform>(),
            new Vector2(0.03f, 0.16f),
            new Vector2(0.24f, 0.84f));
        back.onClick.AddListener(StopUniversalScanner);
        AddPressAnimation(back);

        universalScanTitle = CreateText(
            "SCAN TO LEARN",
            32,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Left,
            header.transform
        );
        SetRect(universalScanTitle.rectTransform,
            new Vector2(0.28f, 0.50f),
            new Vector2(0.76f, 0.90f));

        universalScanSubtitle = CreateText(
            "Point • Detect • Learn",
            20,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Left,
            header.transform
        );
        SetRect(universalScanSubtitle.rectTransform,
            new Vector2(0.28f, 0.10f),
            new Vector2(0.76f, 0.49f));

        universalScanStatus = CreateText(
            "READY",
            18,
            FontStyles.Bold,
            correctGreen,
            TextAlignmentOptions.Center,
            header.transform
        );
        SetRect(universalScanStatus.rectTransform,
            new Vector2(0.77f, 0.18f),
            new Vector2(0.97f, 0.82f));

        // Central scanning frame.
        universalScannerFrame = CreateRoundedPanel(
            "UniversalScannerFrame",
            universalScanScreen.transform,
            new Color(0f, 0f, 0f, 0.03f)
        );
        AddBorder(
            universalScannerFrame,
            new Color(1f, 0.388f, 0.278f, 0.92f)
        );
        SetRect(universalScannerFrame.GetComponent<RectTransform>(),
            new Vector2(0.10f, 0.31f),
            new Vector2(0.90f, 0.73f));

        universalScannerGlow = CreateRoundedPanel(
            "ScannerGlow",
            universalScannerFrame.transform,
            new Color(1f, 0.388f, 0.278f, 0.08f)
        );
        SetRect(universalScannerGlow.GetComponent<RectTransform>(),
            new Vector2(0.02f, 0.02f),
            new Vector2(0.98f, 0.98f));
        universalScannerGlow.transform.SetAsFirstSibling();

        TMP_Text centerTitle = CreateText(
            "POINT CAMERA AT A LESSON",
            30,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            universalScanScreen.transform
        );
        SetRect(centerTitle.rectTransform,
            new Vector2(0.14f, 0.57f),
            new Vector2(0.86f, 0.65f));

        universalScanHint = CreateText(
            "No subject selection needed\nMove closer • Keep steady",
            22,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            universalScanScreen.transform
        );
        SetTextWrapping(universalScanHint);
        SetRect(universalScanHint.rectTransform,
            new Vector2(0.14f, 0.41f),
            new Vector2(0.86f, 0.56f));

        // Quick scanning tips.
        GameObject tipCard = CreateRoundedPanel(
            "ScannerTips",
            universalScanScreen.transform,
            new Color(0.06f, 0.09f, 0.15f, 0.82f)
        );
        AddBorder(tipCard, glassBorder);
        SetRect(tipCard.GetComponent<RectTransform>(),
            new Vector2(0.16f, 0.20f),
            new Vector2(0.84f, 0.29f));

        TMP_Text tips = CreateText(
            "GOOD LIGHT • FULL IMAGE IN VIEW • HOLD STEADY\nHEART • BRAIN • LUNGS • PENDULUM • H₂O",
            16,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            tipCard.transform
        );
        SetTextWrapping(tips);
        SetRect(tips.rectTransform,
            new Vector2(0.05f, 0.16f),
            new Vector2(0.95f, 0.84f));

        // Scanner feedback/result card.
        scannerStatusCard = CreateRoundedPanel(
            "ScannerStatusCard",
            universalScanScreen.transform,
            glassHUD
        );
        AddBorder(scannerStatusCard, glassBorder);
        SetRect(scannerStatusCard.GetComponent<RectTransform>(),
            new Vector2(0.06f, 0.07f),
            new Vector2(0.94f, 0.18f));

        scannerStatusCardTitle = CreateText(
            "AUTO DETECTION",
            20,
            FontStyles.Bold,
            scanTomato,
            TextAlignmentOptions.Center,
            scannerStatusCard.transform
        );
        SetRect(scannerStatusCardTitle.rectTransform,
            new Vector2(0.05f, 0.52f),
            new Vector2(0.95f, 0.90f));

        scannerStatusCardBody = CreateText(
            "Ready to detect a supported lesson image.",
            18,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            scannerStatusCard.transform
        );
        SetTextWrapping(scannerStatusCardBody);
        SetRect(scannerStatusCardBody.rectTransform,
            new Vector2(0.05f, 0.08f),
            new Vector2(0.95f, 0.50f));

        // Kept for compatibility with the existing code/reference.
        universalScanSupported = CreateText(
            "",
            16,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            universalScanScreen.transform
        );
        universalScanSupported.gameObject.SetActive(false);

        universalScanScreen.SetActive(false);
    }

    private void OpenUniversalScanner()
    {
        HideLearningPopup();
        CloseFeatureScreens();

        homeScreen.SetActive(false);
        topicScreen.SetActive(false);
        arScreen.SetActive(false);
        quizScreen.SetActive(false);
        resultScreen.SetActive(false);
        universalScanScreen.SetActive(true);

        universalScanActive = true;
        universalScanLocked = false;
        currentScanRecorded = false;

        if (universalScannerFrame != null)
            universalScannerFrame.SetActive(true);
        lastDetectedTarget = null;
        scannerNoTargetTimer = 0f;
        scannerStableFrames.Clear();

        // Keep all registered targets active for universal recognition, but hide
        // their models until a real image has been acquired.
        HideAllARModels();
        EnableAllTargets();

        universalScanStatus.text = "SCANNING";
        universalScanStatus.color = scanTomato;
        universalScanHint.text =
            "No subject selection needed\nMove closer • Keep steady";

        scannerStatusCardTitle.text = "AUTO DETECTION";
        scannerStatusCardTitle.color = scanTomato;
        scannerStatusCardBody.text =
            "Ready to detect a supported lesson image.";

        if (universalScannerPulseRoutine != null)
            StopCoroutine(universalScannerPulseRoutine);
        universalScannerPulseRoutine = StartCoroutine(PulseUniversalScanner());

        if (scannerSuccessRoutine != null)
            StopCoroutine(scannerSuccessRoutine);
        scannerSuccessRoutine = null;

        StartCoroutine(FadeInScreen(universalScanScreen));
    }

    private void StopUniversalScanner()
    {
        universalScanActive = false;
        universalScanLocked = false;
        lastDetectedTarget = null;
        scannerNoTargetTimer = 0f;
        scannerStableFrames.Clear();

        if (universalScannerPulseRoutine != null)
        {
            StopCoroutine(universalScannerPulseRoutine);
            universalScannerPulseRoutine = null;
        }

        if (scannerSuccessRoutine != null)
        {
            StopCoroutine(scannerSuccessRoutine);
            scannerSuccessRoutine = null;
        }

        PauseAllModelAnimations();
        DisableAllTargets();
        HideAllARModels();

        if (universalScanScreen != null)
            universalScanScreen.SetActive(false);

        ShowHome();
    }

    private IEnumerator PulseUniversalScanner()
    {
        if (universalScannerFrame == null)
            yield break;

        RectTransform frame = universalScannerFrame.GetComponent<RectTransform>();
        RectTransform glow = universalScannerGlow != null
            ? universalScannerGlow.GetComponent<RectTransform>()
            : null;

        while (universalScanActive && frame != null)
        {
            float t = (Mathf.Sin(Time.unscaledTime * 2.4f) + 1f) * 0.5f;
            float scale = Mathf.Lerp(0.985f, 1.015f, t);
            frame.localScale = new Vector3(scale, scale, 1f);

            if (glow != null)
            {
                float alpha = Mathf.Lerp(0.04f, 0.12f, t);
                UIImage img = glow.GetComponent<UIImage>();
                if (img != null)
                    img.color = new Color(1f, 0.388f, 0.278f, alpha);
            }

            yield return null;
        }
    }

    private void UpdateUniversalScannerStatus()
    {
        if (!universalScanActive ||
            universalScanLocked ||
            universalScanScreen == null ||
            !universalScanScreen.activeSelf)
            return;

        GameObject[] targets =
        {
            heartTarget,
            brainTarget,
            lungsTarget,
            physicsTarget,
            chemistryTarget
        };

        bool anyTracked = false;
        GameObject candidate = null;
        string candidateSubject = null;
        string candidateTopic = null;
        int candidateStreak = 0;

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i];
            if (target == null || !target.activeSelf)
                continue;

            ObserverBehaviour observer = target.GetComponent<ObserverBehaviour>();
            if (observer == null)
                continue;

            TargetStatus status = observer.TargetStatus;

            // Initial acquisition must come from the real image in the camera.
            // Extended Tracking is deliberately NOT accepted here because it can
            // keep a target alive after the phone has moved away from the image.
            bool targetVisible = status.Status == Status.TRACKED;

            if (!targetVisible)
            {
                scannerStableFrames[target] = 0;
                continue;
            }

            anyTracked = true;

            if (!TryGetTopicInfo(target, out string subject, out string topic))
                continue;

            int streak = 0;
            scannerStableFrames.TryGetValue(target, out streak);
            streak++;
            scannerStableFrames[target] = streak;

            if (candidate == null || streak > candidateStreak)
            {
                candidate = target;
                candidateSubject = subject;
                candidateTopic = topic;
                candidateStreak = streak;
            }
        }

        if (!anyTracked)
        {
            scannerNoTargetTimer += Time.unscaledDeltaTime;
            universalScanStatus.text = "SCANNING";
            universalScanStatus.color = scanTomato;
            scannerStatusCardTitle.text = "SEARCHING";
            scannerStatusCardTitle.color = scanTomato;

            if (scannerNoTargetTimer > scannerHintDelay)
            {
                universalScanHint.text =
                    "Move the image into the frame\nKeep it steady and well lit";
                scannerStatusCardBody.text =
                    "No image detected yet. Try moving closer or improving the lighting.";
            }
            else
            {
                universalScanHint.text =
                    "No subject selection needed\nMove closer • Keep steady";
                scannerStatusCardBody.text =
                    "Looking for a supported lesson image...";
            }

            return;
        }

        scannerNoTargetTimer = 0f;
        universalScanStatus.text = "STABILIZING";
        universalScanStatus.color = scanTomato;
        scannerStatusCardTitle.text = "IMAGE DETECTED";
        scannerStatusCardTitle.color = scanTomato;

        if (candidate == null)
            return;

        universalScanHint.text =
            "Hold steady\n" + GetTopicDisplayName(candidateTopic);
        scannerStatusCardBody.text =
            "Confirming " + GetTopicDisplayName(candidateTopic) + "...";

        if (candidateStreak >= scannerStableFramesRequired)
        {
            LockUniversalScanResult(
                candidate,
                candidateSubject,
                candidateTopic
            );
        }
    }

    private void LockUniversalScanResult(
        GameObject target,
        string subject,
        string topic)
    {
        if (universalScanLocked)
            return;

        universalScanLocked = true;
        universalScanActive = false;
        lastDetectedTarget = target;

        currentSubject = subject;
        currentTopic = topic;

        universalScanStatus.text = "FOUND";
        universalScanStatus.color = correctGreen;
        universalScanHint.text =
            "FOUND\n" + GetTopicDisplayName(topic);
        scannerStatusCardTitle.text =
            "✓ " + GetTopicDisplayName(topic).ToUpper();
        scannerStatusCardTitle.color = correctGreen;
        scannerStatusCardBody.text =
            "Lesson identified. Opening the 3D lesson...";

        EduARLocalProgress.MarkARScan(subject, topic);
        currentScanRecorded = true;

        if (universalScannerPulseRoutine != null)
        {
            StopCoroutine(universalScannerPulseRoutine);
            universalScannerPulseRoutine = null;
        }

        if (scannerSuccessRoutine != null)
        {
            StopCoroutine(scannerSuccessRoutine);
            scannerSuccessRoutine = null;
        }

        // Detection completes the scan. Open the correct AR lesson immediately.
        OpenAR(topic);
    }

    private bool TryGetTopicInfo(GameObject target, out string subject, out string topic)
    {
        subject = null;
        topic = null;

        if (target == null)
            return false;

        if (target == heartTarget)
        {
            subject = "Biology";
            topic = "Heart";
            return true;
        }

        if (target == brainTarget)
        {
            subject = "Biology";
            topic = "Brain";
            return true;
        }

        if (target == lungsTarget)
        {
            subject = "Biology";
            topic = "Lungs";
            return true;
        }

        if (target == physicsTarget)
        {
            subject = "Physics";
            topic = "Pendulum";
            return true;
        }

        if (target == chemistryTarget)
        {
            subject = "Chemistry";
            topic = "Water";
            return true;
        }

        return false;
    }

    private void EnableAllTargets()
    {
        if (heartTarget != null) heartTarget.SetActive(true);
        if (brainTarget != null) brainTarget.SetActive(true);
        if (lungsTarget != null) lungsTarget.SetActive(true);
        if (physicsTarget != null) physicsTarget.SetActive(true);
        if (chemistryTarget != null) chemistryTarget.SetActive(true);
    }

    // =========================================================
    // NEXT-LEVEL AR SCREEN (PRO BACK BUTTON + FROSTED HUD)
    // =========================================================

    private void BuildARScreen()
    {
        arScreen = CreateScreen("AR");

        // -----------------------------------------------------
        // TOP FLOATING GLASS HEADER
        // -----------------------------------------------------
        GameObject header = CreateRoundedPanel("ARHeader", arScreen.transform, glassHUD);
        AddBorder(header, glassBorder);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0.04f, 0.895f), new Vector2(0.96f, 0.985f));

        // HIGH VISIBILITY FROSTED BACK BUTTON
        Button back = CreateButton("< Back", header.transform, buttonPillDark, textWhite, 20);
        AddBorder(back.gameObject, glassBorder);
        SetRect(back.GetComponent<RectTransform>(), new Vector2(0.03f, 0.18f), new Vector2(0.24f, 0.82f));
        back.onClick.AddListener(BackFromAR);
        AddPressAnimation(back);

        arTitle = CreateText("Model Name", 32, FontStyles.Bold, textWhite, TextAlignmentOptions.Left, header.transform);
        SetRect(arTitle.rectTransform, new Vector2(0.27f, 0.48f), new Vector2(0.68f, 0.88f));

        arSubtitle = CreateText("AR Learning Mode", 20, FontStyles.Normal, textMuted, TextAlignmentOptions.Left, header.transform);
        SetRect(arSubtitle.rectTransform, new Vector2(0.27f, 0.12f), new Vector2(0.68f, 0.48f));

        // Tracking Badge
        trackingPill = CreateRoundedPanel("TrackingPill", header.transform, new Color(0f, 0f, 0f, 0.40f));
        AddBorder(trackingPill, glassBorder);
        SetRect(trackingPill.GetComponent<RectTransform>(), new Vector2(0.70f, 0.18f), new Vector2(0.97f, 0.82f));

        trackingText = CreateText("SEARCHING", 18, FontStyles.Bold, textMuted, TextAlignmentOptions.Center, trackingPill.transform);
        StretchFull(trackingText.rectTransform);

        // -----------------------------------------------------
        // BOTTOM DRAWER (NEXT-LEVEL POLISHED CARD)
        // -----------------------------------------------------
        bottomDrawer = CreateRoundedPanel("ARBottomDrawer", arScreen.transform, glassHUD);
        AddBorder(bottomDrawer, glassBorder);
        SetRect(bottomDrawer.GetComponent<RectTransform>(), new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.29f));

        // Drag bar indicator
        GameObject handle = CreateRoundedPanel("DrawerHandle", bottomDrawer.transform, new Color(1f, 1f, 1f, 0.35f));
        SetRect(handle.GetComponent<RectTransform>(), new Vector2(0.44f, 0.94f), new Vector2(0.56f, 0.958f));

        drawerCategory = CreateText("BIOLOGY  •  3D MODULE", 26, FontStyles.Bold, bioColor, TextAlignmentOptions.Left, bottomDrawer.transform);
        SetRect(drawerCategory.rectTransform, new Vector2(0.06f, 0.79f), new Vector2(0.68f, 0.91f));

        drawerTitle = CreateText("Model Title", 32, FontStyles.Bold, textWhite, TextAlignmentOptions.Left, bottomDrawer.transform);
        SetRect(drawerTitle.rectTransform, new Vector2(0.06f, 0.58f), new Vector2(0.68f, 0.79f));

        drawerDescription = CreateText("Description...", 20, FontStyles.Normal, textMuted, TextAlignmentOptions.Left, bottomDrawer.transform);
        SetTextWrapping(drawerDescription);
        SetRect(drawerDescription.rectTransform, new Vector2(0.06f, 0.40f), new Vector2(0.68f, 0.57f));

        // -----------------------------------------------------
        // RIGHT-SIDE AR CONTROL BUTTONS
        // -----------------------------------------------------

        // HIDE PARTS
        // -----------------------------------------------------
        // RIGHT-SIDE AR CONTROL BUTTONS
        // -----------------------------------------------------

        // -----------------------------------------------------
        // RIGHT-SIDE AR CONTROL BUTTONS — MOVED UP
        // -----------------------------------------------------

        // HIDE PARTS
        arPartsButton = CreateButton(
            "HIDE PARTS",
            bottomDrawer.transform,
            scanTomato,
            textWhite,
            16
        );

        AddBorder(
            arPartsButton.gameObject,
            new Color(1f, 1f, 1f, 0.10f)
        );

        SetRect(
            arPartsButton.GetComponent<RectTransform>(),
            new Vector2(0.72f, 0.78f),
            new Vector2(0.95f, 0.93f)
        );

        arPartsButton.onClick.AddListener(TogglePartLabels);
        AddPressAnimation(arPartsButton);


        // PAUSE ANIM
        arAnimationButton = CreateButton(
            "PAUSE ANIM",
            bottomDrawer.transform,
            buttonPillDark,
            textWhite,
            16
        );

        AddBorder(
            arAnimationButton.gameObject,
            glassBorder
        );

        SetRect(
            arAnimationButton.GetComponent<RectTransform>(),
            new Vector2(0.72f, 0.59f),
            new Vector2(0.95f, 0.74f)
        );

        arAnimationButton.onClick.AddListener(ToggleCurrentAnimation);
        AddPressAnimation(arAnimationButton);


        // RESET
        Button arReset = CreateButton(
            "RESET",
            bottomDrawer.transform,
            buttonPillDark,
            textWhite,
            16
        );

        AddBorder(
            arReset.gameObject,
            glassBorder
        );

        SetRect(
            arReset.GetComponent<RectTransform>(),
            new Vector2(0.72f, 0.40f),
            new Vector2(0.95f, 0.55f)
        );

        arReset.onClick.AddListener(ResetCurrentModel);
        AddPressAnimation(arReset);


        // -----------------------------------------------------
        // BOTTOM CTA BUTTONS
        // -----------------------------------------------------

        // LEARN
        arLearnButton = CreateButton(
            "LEARN",
            bottomDrawer.transform,
            bioColor,
            Color.white,
            25
        );

        SetRect(
            arLearnButton.GetComponent<RectTransform>(),
            new Vector2(0.05f, 0.08f),
            new Vector2(0.48f, 0.34f)
        );

        arLearnButton.onClick.AddListener(ShowLearningPopup);
        AddPressAnimation(arLearnButton);


        // QUIZ
        arQuizButton = CreateButton(
            "QUIZ",
            bottomDrawer.transform,
            brandIndigo,
            Color.white,
            25
        );

        SetRect(
            arQuizButton.GetComponent<RectTransform>(),
            new Vector2(0.52f, 0.08f),
            new Vector2(0.95f, 0.34f)
        );

        arQuizButton.onClick.AddListener(StartQuiz);
        AddPressAnimation(arQuizButton);


        // Two Big Rounded CTA Buttons
        arLearnButton = CreateButton("LEARN", bottomDrawer.transform, bioColor, Color.white, 25);
        SetRect(arLearnButton.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.48f, 0.34f));
        arLearnButton.onClick.AddListener(ShowLearningPopup);
        AddPressAnimation(arLearnButton);

        arQuizButton = CreateButton("QUIZ", bottomDrawer.transform, brandIndigo, Color.white, 25);
        SetRect(arQuizButton.GetComponent<RectTransform>(), new Vector2(0.52f, 0.08f), new Vector2(0.95f, 0.34f));
        arQuizButton.onClick.AddListener(StartQuiz);
        AddPressAnimation(arQuizButton);

        BuildLearningPopup();
        arScreen.SetActive(false);
    }

    private void OpenAR(string topic)
    {
        universalScanActive = false;
        lastDetectedTarget = GetTarget(topic);

        if (universalScannerPulseRoutine != null)
        {
            StopCoroutine(universalScannerPulseRoutine);
            universalScannerPulseRoutine = null;
        }

        if (universalScanScreen != null)
            universalScanScreen.SetActive(false);

        bool detectedScanAlreadyRecorded =
            lastDetectedTarget != null &&
            lastDetectedTarget == GetTarget(topic) &&
            currentScanRecorded;

        currentTopic = topic;
        currentScanRecorded = detectedScanAlreadyRecorded;
        GameObject target = GetTarget(topic);

        if (target == null)
        {
            ShowComingSoon(GetTopicDisplayName(topic) + "\n\nAR Target not yet assigned.");
            return;
        }

        // Keep every registered Vuforia target available so the user can simply
        // move the phone to another lesson image without pressing another button.
        EnableAllTargets();
        HideAllARModels();
        target.SetActive(true);
        SetModelActiveForTopic(topic, true);

        HideLearningPopup();
        partLabelsVisible = true;
        SetCurrentPartLabelsVisible(false);
        if (arPartsButton != null)
        {
            TMP_Text partButtonText = arPartsButton.GetComponentInChildren<TMP_Text>();
            if (partButtonText != null) partButtonText.text = "HIDE PARTS";
            SetButtonImageColor(arPartsButton, scanTomato);
        }
        homeScreen.SetActive(false);
        topicScreen.SetActive(false);
        quizScreen.SetActive(false);
        resultScreen.SetActive(false);
        arScreen.SetActive(true);

        Color subjectColor = GetSubjectColor(currentSubject);

        arTitle.text = GetTopicDisplayName(topic);
        arSubtitle.text = currentSubject + " • AR Learning";

        SetDrawerCategoryText(currentSubject, subjectColor);
        drawerTitle.text = GetTopicDisplayName(topic);
        drawerDescription.text = GetShortDescription(topic);

        SetButtonImageColor(arLearnButton, subjectColor);
        SetButtonImageColor(arQuizButton, brandIndigo);

        SetCurrentAnimationPlaying(true);
        RefreshAnimationButton();

        trackingText.text = "◌ SCANNING";
        trackingText.color = textMuted;
        trackingLostTimer = 0f;
        ResetTrackingFeatureState();
        SetLastFeatureTopic(currentSubject, currentTopic);

        StartCoroutine(FadeInScreen(arScreen));
    }

    private void UpdateTrackingStatus()
    {
        if (arScreen == null || !arScreen.activeSelf)
            return;

        GameObject[] targets =
        {
            heartTarget,
            brainTarget,
            lungsTarget,
            physicsTarget,
            chemistryTarget
        };

        GameObject trackedTarget = null;
        string trackedSubject = null;
        string trackedTopic = null;

        // Only a real TRACKED state is treated as an attached physical image.
        // This prevents model/label content from floating after the phone leaves
        // the printed lesson image.
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i];
            if (target == null || !target.activeSelf)
                continue;

            ObserverBehaviour observer = target.GetComponent<ObserverBehaviour>();
            if (observer == null)
                continue;

            if (observer.TargetStatus.Status != Status.TRACKED)
                continue;

            if (!TryGetTopicInfo(target, out string subject, out string topic))
                continue;

            trackedTarget = target;
            trackedSubject = subject;
            trackedTopic = topic;
            break;
        }

        if (trackedTarget != null)
        {
            trackingLostTimer = 0f;
            trackingText.text = "● TRACKED";
            trackingText.color = correctGreen;

            // Switch lessons automatically when a different printed image enters
            // the camera view. No manual scan action is required.
            if (currentTopic != trackedTopic || currentSubject != trackedSubject)
            {
                currentSubject = trackedSubject;
                currentTopic = trackedTopic;

                HideAllARModels();
                HideAllPartLabels();

                SetModelActiveForTopic(currentTopic, true);
                ResetTrackingFeatureState();
                SetLastFeatureTopic(currentSubject, currentTopic);
                RefreshARLessonContent();
            }
            else
            {
                // Ensure the currently tracked model is visible immediately.
                SetModelActiveForTopic(currentTopic, true);
            }

            SetCurrentPartLabelsVisible(partLabelsVisible);

            if (RegisterTrackingFeatureIfNeeded() && !currentScanRecorded)
            {
                EduARLocalProgress.MarkARScan(currentSubject, currentTopic);
                currentScanRecorded = true;
            }

            return;
        }

        // No real image is currently tracked. Hide the AR content immediately so
        // the model and educational callouts never remain floating in space.
        trackingLostTimer += Time.unscaledDeltaTime;
        HideAllARModels();
        HideAllPartLabels();

        if (trackingLostTimer > 0.35f)
        {
            trackingText.text = "! TARGET LOST";
            trackingText.color = wrongRed;
        }
        else
        {
            trackingText.text = "◌ SCANNING";
            trackingText.color = textMuted;
        }
    }

    private void RefreshARLessonContent()
    {
        Color subjectColor = GetSubjectColor(currentSubject);

        arTitle.text = GetTopicDisplayName(currentTopic);
        arSubtitle.text = currentSubject + " • AR Learning";

        SetDrawerCategoryText(currentSubject, subjectColor);
        drawerTitle.text = GetTopicDisplayName(currentTopic);
        drawerDescription.text = GetShortDescription(currentTopic);

        SetButtonImageColor(arLearnButton, subjectColor);
        SetButtonImageColor(arQuizButton, brandIndigo);

        SetCurrentAnimationPlaying(true);
        RefreshAnimationButton();
    }

    private ObserverBehaviour GetCurrentObserver()
    {
        GameObject target = GetTarget(currentTopic);
        if (target == null) return null;
        return target.GetComponent<ObserverBehaviour>();
    }

    // =========================================================
    // LEARNING POPUP
    // =========================================================

    private void BuildLearningPopup()
    {
        learningPopup = CreateRoundedPanel("LearningPopup", arScreen.transform, Hex("#0E1422"));
        AddBorder(learningPopup, glassBorder);
        SetRect(learningPopup.GetComponent<RectTransform>(), new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.85f));

        learningPopupCategory = CreateText("MODULE OVERVIEW", 24, FontStyles.Bold, brandIndigo, TextAlignmentOptions.Left, learningPopup.transform);
        SetRect(learningPopupCategory.rectTransform, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.94f));

        learningPopupTitle = CreateText("Topic Title", 34, FontStyles.Bold, textWhite, TextAlignmentOptions.Left, learningPopup.transform);
        SetRect(learningPopupTitle.rectTransform, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.88f));

        learningPopupBody = CreateText("", 26, FontStyles.Normal, textMuted, TextAlignmentOptions.Left, learningPopup.transform);
        SetTextWrapping(learningPopupBody);
        SetRect(learningPopupBody.rectTransform, new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.78f));

        learningPopupClose = CreateButton(
     "CLOSE",
     learningPopup.transform,
     brandIndigo,
     textWhite,
     22
 );

        SetRect(
            learningPopupClose.GetComponent<RectTransform>(),
            new Vector2(0.34f, 0.06f),
            new Vector2(0.94f, 0.15f)
        );

        // Make CLOSE text clearly visible
        TMP_Text closeText = learningPopupClose.GetComponentInChildren<TMP_Text>();

        if (closeText != null)
        {
            closeText.text = "CLOSE";
            closeText.fontSize = 22;
            closeText.fontStyle = FontStyles.Bold;
            closeText.color = textWhite;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.rectTransform.anchorMin = Vector2.zero;
            closeText.rectTransform.anchorMax = Vector2.one;
            closeText.rectTransform.offsetMin = Vector2.zero;
            closeText.rectTransform.offsetMax = Vector2.zero;
        }

        learningPopupClose.onClick.AddListener(HideLearningPopup);
        BuildFeatureLearningControls();

        learningPopup.SetActive(false);
    }

    private void ShowLearningPopup()
    {
        if (learningPopup == null) return;

        Color subjectColor = GetSubjectColor(currentSubject);

        learningPopupTitle.text = GetTopicDisplayName(currentTopic);
        learningPopupCategory.text = currentSubject.ToUpper() + " • LEARNING MODULE";
        learningPopupCategory.color = subjectColor;
        learningPopupBody.text = GetInformation(currentTopic);
        EduARLocalProgress.MarkSummaryRead(currentSubject, currentTopic);

        SetButtonImageColor(learningPopupClose, subjectColor);
        RefreshLearningFavoriteButton();

        learningPopup.SetActive(true);
        StartCoroutine(FadeInScreen(learningPopup));
    }

    private void HideLearningPopup()
    {
        if (learningPopup != null)
            learningPopup.SetActive(false);
    }

    // =========================================================
    // QUIZ SCREEN
    // =========================================================

    private void BuildQuizScreen()
    {
        quizScreen = CreateScreen("Quiz");
        CreateBackground(quizScreen, bgDark);

        Button quizBack = CreateButton("< Back", quizScreen.transform, buttonPillDark, textWhite, 22);
        AddBorder(quizBack.gameObject, glassBorder);
        SetRect(quizBack.GetComponent<RectTransform>(), new Vector2(0.04f, 0.915f), new Vector2(0.25f, 0.975f));
        quizBack.onClick.AddListener(BackFromQuiz);
        quizBack.transform.SetAsLastSibling();
        AddPressAnimation(quizBack);

        quizTitle = CreateText("Subject Quiz", 38, FontStyles.Bold, textWhite, TextAlignmentOptions.Center, quizScreen.transform);
        SetRect(quizTitle.rectTransform, new Vector2(0.06f, 0.91f), new Vector2(0.94f, 0.97f));

        quizProgress = CreateText("Question 1 of 5", 21, FontStyles.Bold, brandIndigo, TextAlignmentOptions.Center, quizScreen.transform);
        SetRect(quizProgress.rectTransform, new Vector2(0.06f, 0.86f), new Vector2(0.94f, 0.90f));

        GameObject progressBg = CreateRoundedPanel("ProgressBg", quizScreen.transform, cardDark);
        SetRect(progressBg.GetComponent<RectTransform>(), new Vector2(0.08f, 0.83f), new Vector2(0.92f, 0.845f));

        GameObject progress = CreateRoundedPanel("ProgressFill", progressBg.transform, brandIndigo);
        quizProgressFill = progress.GetComponent<UIImage>();
        SetRect(progress.GetComponent<RectTransform>(), Vector2.zero, new Vector2(0.20f, 1f));

        GameObject qCard = CreateRoundedPanel("QCard", quizScreen.transform, cardDark);
        AddBorder(qCard, cardBorder);
        SetRect(qCard.GetComponent<RectTransform>(), new Vector2(0.06f, 0.64f), new Vector2(0.94f, 0.80f));

        quizQuestion = CreateText("", 28, FontStyles.Bold, textWhite, TextAlignmentOptions.Center, qCard.transform);
        SetTextWrapping(quizQuestion);
        SetRect(quizQuestion.rectTransform, new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.95f));

        option1Button = CreateQuizOption(0.50f, out option1Text, out option1Image);
        option2Button = CreateQuizOption(0.38f, out option2Text, out option2Image);
        option3Button = CreateQuizOption(0.26f, out option3Text, out option3Image);
        option4Button = CreateQuizOption(0.14f, out option4Text, out option4Image);

        quizScore = CreateText("Score: 0/5", 22, FontStyles.Bold, textMuted, TextAlignmentOptions.Left, quizScreen.transform);
        SetRect(quizScore.rectTransform, new Vector2(0.08f, 0.03f), new Vector2(0.45f, 0.08f));

        nextQuizButton = CreateButton("NEXT →", quizScreen.transform, brandIndigo, Color.white, 20);
        SetRect(nextQuizButton.GetComponent<RectTransform>(), new Vector2(0.58f, 0.025f), new Vector2(0.94f, 0.085f));
        nextQuizButton.onClick.AddListener(NextQuizQuestion);
    }

    private Button CreateQuizOption(float centerY, out TMP_Text label, out UIImage image)
    {
        Button button = CreateButton("", quizScreen.transform, cardDark, textWhite, 24);
        AddBorder(button.gameObject, cardBorder);

        SetRect(button.GetComponent<RectTransform>(), new Vector2(0.06f, centerY), new Vector2(0.94f, centerY + 0.095f));

        image = button.GetComponent<UIImage>();
        label = button.GetComponentInChildren<TMP_Text>();
        label.fontSize = 26;
        label.alignment = TextAlignmentOptions.Left;
        SetTextWrapping(label);

        label.rectTransform.offsetMin = new Vector2(30f, 0f);
        label.rectTransform.offsetMax = new Vector2(-20f, 0f);

        return button;
    }

    private void StartQuiz()
    {
        PauseAllModelAnimations();
        SetCurrentPartLabelsVisible(false);
        universalScanActive = false;

        if (universalScannerPulseRoutine != null)
        {
            StopCoroutine(universalScannerPulseRoutine);
            universalScannerPulseRoutine = null;
        }

        if (universalScanScreen != null)
            universalScanScreen.SetActive(false);

        LoadQuizData();
        currentQuestionIndex = 0;
        currentScore = 0;
        answerSelected = false;
        BeginFeatureQuizSession();

        HideLearningPopup();
        arScreen.SetActive(false);
        quizScreen.SetActive(true);
        resultScreen.SetActive(false);

        LoadQuizQuestion();
        StartCoroutine(FadeInScreen(quizScreen));
    }

    private void LoadQuizData()
    {
        currentQuiz.Clear();
        List<QuestionData> source;

        switch (currentTopic)
        {
            case "Brain": source = brainQuiz; break;
            case "Lungs": source = lungsQuiz; break;
            case "Pendulum": source = physicsQuiz; break;
            case "Water": source = chemistryQuiz; break;
            default: source = heartQuiz; break;
        }

        List<QuestionData> shuffled = new List<QuestionData>(source);
        Shuffle(shuffled);

        foreach (QuestionData question in shuffled)
        {
            QuestionData copy = question.Clone();
            ShuffleAnswers(copy);
            currentQuiz.Add(copy);
        }
    }

    private void LoadQuizQuestion()
    {
        if (currentQuiz.Count == 0) return;

        QuestionData question = currentQuiz[currentQuestionIndex];
        Color subjectColor = GetSubjectColor(currentSubject);

        quizTitle.text = currentSubject + " Quiz";
        quizProgress.text = "QUESTION " + (currentQuestionIndex + 1) + " OF " + currentQuiz.Count;
        quizProgress.color = subjectColor;

        quizQuestion.text = question.question;

        option1Text.text = "1.  " + question.answers[0];
        option2Text.text = "2.  " + question.answers[1];
        option3Text.text = "3.  " + question.answers[2];
        option4Text.text = "4.  " + question.answers[3];

        ResetQuizVisuals();

        option1Button.interactable = true;
        option2Button.interactable = true;
        option3Button.interactable = true;
        option4Button.interactable = true;

        nextQuizButton.gameObject.SetActive(false);
        quizScore.text = "Score: " + currentScore + "/" + currentQuiz.Count;
        answerSelected = false;

        if (quizProgressFill != null)
        {
            float progress = (float)(currentQuestionIndex + 1) / currentQuiz.Count;
            quizProgressFill.rectTransform.anchorMax = new Vector2(progress, 1f);
            quizProgressFill.color = subjectColor;
        }

        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();
        option3Button.onClick.RemoveAllListeners();
        option4Button.onClick.RemoveAllListeners();

        option1Button.onClick.AddListener(() => SelectAnswer(0));
        option2Button.onClick.AddListener(() => SelectAnswer(1));
        option3Button.onClick.AddListener(() => SelectAnswer(2));
        option4Button.onClick.AddListener(() => SelectAnswer(3));
    }

    private void SelectAnswer(int selectedIndex)
    {
        if (answerSelected) return;
        answerSelected = true;

        QuestionData question = currentQuiz[currentQuestionIndex];

        if (selectedIndex == question.correctIndex)
        {
            currentScore++;
            GetOptionImage(selectedIndex).color = correctGreen;
        }
        else
        {
            GetOptionImage(selectedIndex).color = wrongRed;
            GetOptionImage(question.correctIndex).color = correctGreen;
        }

        RecordFeatureQuizAnswer(question, selectedIndex);

        option1Button.interactable = false;
        option2Button.interactable = false;
        option3Button.interactable = false;
        option4Button.interactable = false;

        quizScore.text = "Score: " + currentScore + "/" + currentQuiz.Count;
        nextQuizButton.gameObject.SetActive(true);
    }

    private UIImage GetOptionImage(int index)
    {
        switch (index)
        {
            case 0: return option1Image;
            case 1: return option2Image;
            case 2: return option3Image;
            default: return option4Image;
        }
    }

    private void ResetQuizVisuals()
    {
        option1Image.color = cardDark;
        option2Image.color = cardDark;
        option3Image.color = cardDark;
        option4Image.color = cardDark;
    }

    private void NextQuizQuestion()
    {
        if (!answerSelected) return;

        currentQuestionIndex++;
        if (currentQuestionIndex < currentQuiz.Count)
        {
            LoadQuizQuestion();
        }
        else
        {
            ShowResult();
        }
    }

    // =========================================================
    // RESULT SCREEN
    // =========================================================

    private void BuildResultScreen()
    {
        resultScreen = CreateScreen("Result");
        CreateBackground(resultScreen, bgDark);

        Button resultBack = CreateButton("< Back", resultScreen.transform, buttonPillDark, textWhite, 22);
        AddBorder(resultBack.gameObject, glassBorder);
        SetRect(resultBack.GetComponent<RectTransform>(), new Vector2(0.04f, 0.915f), new Vector2(0.25f, 0.975f));
        resultBack.onClick.AddListener(BackFromResult);
        resultBack.transform.SetAsLastSibling();
        AddPressAnimation(resultBack);

        TMP_Text brand = CreateText("EDUAR 3D", 24, FontStyles.Bold, brandIndigo, TextAlignmentOptions.Center, resultScreen.transform);
        SetRect(brand.rectTransform, new Vector2(0.10f, 0.82f), new Vector2(0.90f, 0.88f));

        TMP_Text title = CreateText("Session Complete", 40, FontStyles.Bold, textWhite, TextAlignmentOptions.Center, resultScreen.transform);
        SetRect(title.rectTransform, new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.80f));

        resultScore = CreateText("0/5", 88, FontStyles.Bold, textWhite, TextAlignmentOptions.Center, resultScreen.transform);
        SetRect(resultScore.rectTransform, new Vector2(0.10f, 0.50f), new Vector2(0.90f, 0.68f));

        resultMessage = CreateText("", 22, FontStyles.Normal, textMuted, TextAlignmentOptions.Center, resultScreen.transform);
        SetTextWrapping(resultMessage);
        SetRect(resultMessage.rectTransform, new Vector2(0.08f, 0.35f), new Vector2(0.92f, 0.46f));

        Button retry = CreateButton("TRY AGAIN", resultScreen.transform, brandIndigo, textWhite, 22);
        SetRect(retry.GetComponent<RectTransform>(), new Vector2(0.10f, 0.18f), new Vector2(0.48f, 0.27f));
        retry.onClick.AddListener(StartQuiz);

        Button home = CreateButton("HOME", resultScreen.transform, cardDark, textWhite, 22);
        AddBorder(home.gameObject, cardBorder);
        SetRect(home.GetComponent<RectTransform>(), new Vector2(0.52f, 0.18f), new Vector2(0.90f, 0.27f));
        home.onClick.AddListener(ShowHome);
    }

    private void ShowResult()
    {
        quizScreen.SetActive(false);
        resultScreen.SetActive(true);

        Color subjectColor = GetSubjectColor(currentSubject);
        resultScore.color = subjectColor;
        resultScore.text = currentScore + "/" + currentQuiz.Count;

        CompleteFeatureQuizSession();

        float percentage = (float)currentScore / currentQuiz.Count * 100f;
        if (percentage >= 80f)
        {
            resultMessage.text = "Outstanding performance!\nYou have mastered this topic.";
        }
        else if (percentage >= 60f)
        {
            resultMessage.text = "Well done!\nKeep exploring in AR to reinforce concepts.";
        }
        else
        {
            resultMessage.text = "Good attempt!\nReview the 3D model and test again.";
        }

        StartCoroutine(FadeInScreen(resultScreen));
    }

    // =========================================================
    // GENERAL APP & VUFORIA CONTROLS
    // =========================================================

    private void ShowHome()
    {
        PauseAllModelAnimations();
        HideAllPartLabels();
        universalScanActive = false;
        universalScanLocked = false;
        lastDetectedTarget = null;

        if (universalScannerPulseRoutine != null)
        {
            StopCoroutine(universalScannerPulseRoutine);
            universalScannerPulseRoutine = null;
        }

        DisableAllTargets();
        HideLearningPopup();

        if (universalScanScreen != null)
            universalScanScreen.SetActive(false);

        currentSubject = "Biology";
        currentTopic = "Heart";
        currentScanRecorded = false;

        if (authScreen != null)
            authScreen.SetActive(false);

        homeScreen.SetActive(true);
        topicScreen.SetActive(false);
        arScreen.SetActive(false);
        quizScreen.SetActive(false);
        resultScreen.SetActive(false);
        CloseFeatureScreens();
        RefreshHomeFeatureUI();

        StartCoroutine(FadeInScreen(homeScreen));
    }

    private GameObject GetTarget(string topic)
    {
        switch (topic)
        {
            case "Heart": return heartTarget;
            case "Brain": return brainTarget;
            case "Lungs": return lungsTarget;
            case "Pendulum": return physicsTarget;
            case "Water": return chemistryTarget;
            default: return null;
        }
    }

    private void HideAllARModels()
    {
        SetModelActive(heartModel, false, heartTarget);
        SetModelActive(brainModel, false, brainTarget);
        SetModelActive(lungsModel, false, lungsTarget);
        SetModelActive(physicsModel, false, physicsTarget);
        SetModelActive(chemistryModel, false, chemistryTarget);
    }

    private void SetModelActiveForTopic(string topic, bool active)
    {
        switch (topic)
        {
            case "Heart": SetModelActive(heartModel, active, heartTarget); break;
            case "Brain": SetModelActive(brainModel, active, brainTarget); break;
            case "Lungs": SetModelActive(lungsModel, active, lungsTarget); break;
            case "Pendulum": SetModelActive(physicsModel, active, physicsTarget); break;
            case "Water": SetModelActive(chemistryModel, active, chemistryTarget); break;
        }
    }

    private void SetModelActive(Transform model, bool active, GameObject target)
    {
        if (model == null)
            return;

        // Never disable the Vuforia target root itself.
        if (target != null && model.gameObject == target)
            return;

        model.gameObject.SetActive(active);
    }

    private void DisableAllTargets()
    {
        if (heartTarget != null) heartTarget.SetActive(false);
        if (brainTarget != null) brainTarget.SetActive(false);
        if (lungsTarget != null) lungsTarget.SetActive(false);
        if (physicsTarget != null) physicsTarget.SetActive(false);
        if (chemistryTarget != null) chemistryTarget.SetActive(false);
    }

    private string GetTopicDisplayName(string topic)
    {
        switch (topic)
        {
            case "Heart": return "Human Heart";
            case "Brain": return "Human Brain";
            case "Lungs": return "Human Lungs";
            case "Pendulum": return "Simple Pendulum";
            case "Water": return "Water Molecule";
            default: return topic;
        }
    }

    private string GetShortDescription(string topic)
    {
        switch (topic)
        {
            case "Heart": return "Explore chambers, blood flow and major vessels.";
            case "Brain": return "Discover cortex, lobes and nervous coordination.";
            case "Lungs": return "Examine airways, alveoli and respiration.";
            case "Pendulum": return "Gravitational forces, period and kinetic energy.";
            case "Water": return "Molecular dipole and covalent bonding structure.";
            default: return "";
        }
    }

    private string GetInformation(string topic)
    {
        switch (topic)
        {
            case "Heart":
                return "The human heart is a muscular organ pumping blood through the circulatory network.\n\n" +
                       "Key Structures\n" +
                       "• Right & Left Atria\n" +
                       "• Right & Left Ventricles\n" +
                       "• Aorta & Vena Cava\n" +
                       "• Tricuspid & Mitral Valves";

            case "Brain":
                return "The brain serves as the command center for the entire nervous system.\n\n" +
                       "Key Structures\n" +
                       "• Cerebrum (Higher thought & reasoning)\n" +
                       "• Cerebellum (Balance & coordination)\n" +
                       "• Brainstem (Autonomic functions)\n" +
                       "• Hippocampus (Spatial memory)";

            case "Lungs":
                return "The respiratory lungs facilitate gas exchange between oxygen and carbon dioxide.\n\n" +
                       "Key Structures\n" +
                       "• Trachea & Primary Bronchi\n" +
                       "• Bronchiole Network\n" +
                       "• Alveoli (Microscopic gas transfer)\n" +
                       "• Pleural Cavity";

            case "Pendulum":
                return "A simple pendulum demonstrates resonant harmonic motion.\n\n" +
                       "Key Principles\n" +
                       "• Restoring force is driven by gravity\n" +
                       "• Period is determined by string length\n" +
                       "• Kinetic and potential energy transition";

            case "Water":
                return "Water (H2O) is a bent polar compound essential to organic biochemistry.\n\n" +
                       "Key Principles\n" +
                       "• Polar covalent bonds\n" +
                       "• Electronegative oxygen pole\n" +
                       "• 104.5 degree molecular bond angle";

            default:
                return "";
        }
    }

    private Color GetSubjectColor(string subject)
    {
        switch (subject)
        {
            case "Physics": return physColor;
            case "Chemistry": return chemColor;
            case "Mathematics": return mathColor;
            default: return bioColor;
        }
    }

    // =========================================================
    // UI BUILDER HELPERS
    // =========================================================



    private GameObject CreateScreen(string name)
    {
        GameObject screen = CreateUIObject(name, canvas.transform);

        StretchFull(screen.GetComponent<RectTransform>());

        screen.SetActive(false);

        return screen;
    }

    private void CreateBackground(GameObject screen, Color color)
    {
        GameObject background = CreateUIObject("Background", screen.transform);
        UIImage image = background.AddComponent<UIImage>();
        image.color = color;
        StretchFull(background.GetComponent<RectTransform>());
        background.transform.SetAsFirstSibling();
    }

    private GameObject CreateRoundedPanel(string name, Transform parent, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        UIImage image = panel.AddComponent<UIImage>();
        image.sprite = GetRoundedSprite();
        image.type = UIImage.Type.Sliced;
        image.color = color;
        image.pixelsPerUnitMultiplier = 1.4f;
        return panel;
    }

    private Button CreateButton(string text, Transform parent, Color background, Color textColor, float fontSize = 22)
    {
        GameObject buttonObject = CreateUIObject("Button", parent);
        UIImage image = buttonObject.AddComponent<UIImage>();
        image.sprite = GetRoundedSprite();
        image.type = UIImage.Type.Sliced;
        image.color = background;
        image.pixelsPerUnitMultiplier = 1.4f;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor = background * 1.15f;
        colors.pressedColor = background * 0.85f;
        colors.selectedColor = background;
        button.colors = colors;

        TMP_Text label = CreateText(text, fontSize, FontStyles.Bold, textColor, TextAlignmentOptions.Center, buttonObject.transform);
        label.raycastTarget = false;
        StretchFull(label.rectTransform);

        return button;
    }

    private TMP_Text CreateText(string text, float size, FontStyles style, Color color, TextAlignmentOptions alignment, Transform parent)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TMP_Text textComponent = textObject.AddComponent<TextMeshProUGUI>();

        if (appFont != null)
            textComponent.font = appFont;

        textComponent.text = text;
        textComponent.fontSize = Mathf.Max(size, 20f);
        textComponent.raycastTarget = false;
        textComponent.fontStyle = style;
        textComponent.color = color;
        textComponent.alignment = alignment;
        textComponent.enableWordWrapping = true;
        return textComponent;
    }

    private void SetTextWrapping(TMP_Text text)
    {
        text.textWrappingMode = TextWrappingModes.Normal;
    }

    private void SetButtonImageColor(Button button, Color color)
    {
        if (button == null) return;
        UIImage image = button.GetComponent<UIImage>();
        if (image != null) image.color = color;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        button.colors = colors;
    }

    private void AddBorder(GameObject target, Color color)
    {
        Outline outline = target.GetComponent<Outline>();
        if (outline == null) outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        outline.useGraphicAlpha = true;
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ClearChildren(GameObject parent)
    {
        if (parent == null) return;
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
    }

    // =========================================================
    // PROCEDURAL SMOOTH ROUNDED CORNER SPRITE
    // =========================================================

    private static Sprite roundedSprite;
    private Sprite GetRoundedSprite()
    {
        if (roundedSprite != null) return roundedSprite;

        const int size = 64;
        const int radius = 18;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = IsInsideRoundedRect(x, y, size, radius);
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }

    private bool IsInsideRoundedRect(int x, int y, int size, int radius)
    {
        if (x >= radius && x < size - radius) return true;
        if (y >= radius && y < size - radius) return true;

        float cx = x < radius ? radius : size - radius - 1;
        float cy = y < radius ? radius : size - radius - 1;
        float dx = x - cx;
        float dy = y - cy;

        return dx * dx + dy * dy <= radius * radius;
    }

    // =========================================================
    // ANIMATIONS
    // =========================================================

    private IEnumerator FadeInScreen(GameObject screen)
    {
        if (screen == null) yield break;

        CanvasGroup group = screen.GetComponent<CanvasGroup>();
        if (group == null) group = screen.AddComponent<CanvasGroup>();

        RectTransform rect = screen.GetComponent<RectTransform>();
        group.alpha = 0f;
        rect.localScale = Vector3.one * 0.98f;

        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.18f);
            float smooth = t * t * (3f - 2f * t);

            group.alpha = smooth;
            rect.localScale = Vector3.Lerp(Vector3.one * 0.98f, Vector3.one, smooth);
            yield return null;
        }

        group.alpha = 1f;
        rect.localScale = Vector3.one;
    }

    private void AddPressAnimation(Button button)
    {
        if (button == null) return;
        button.onClick.AddListener(() => StartCoroutine(PressAnimation(button.transform)));
    }

    private IEnumerator PressAnimation(Transform target)
    {
        if (target == null) yield break;
        Vector3 orig = target.localScale;
        target.localScale = orig * 0.95f;
        yield return new WaitForSeconds(0.06f);
        target.localScale = orig;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void ShuffleAnswers(QuestionData question)
    {
        if (question == null || question.answers == null || question.answers.Length == 0)
            return;

        List<AnswerPair> pairs = new List<AnswerPair>();
        for (int i = 0; i < question.answers.Length; i++)
        {
            pairs.Add(new AnswerPair { text = question.answers[i], correct = i == question.correctIndex });
        }

        Shuffle(pairs);

        for (int i = 0; i < pairs.Count; i++)
        {
            question.answers[i] = pairs[i].text;
            if (pairs[i].correct) question.correctIndex = i;
        }
    }

    private void ShowComingSoon(string message)
    {
        StartCoroutine(ToastMessage(message));
    }





    private IEnumerator ToastMessage(string message)
    {
        if (canvas == null) yield break;

        GameObject toast = CreateRoundedPanel("Toast", canvas.transform, cardDark);
        AddBorder(toast, cardBorder);

        SetRect(toast.GetComponent<RectTransform>(), new Vector2(0.12f, 0.44f), new Vector2(0.88f, 0.56f));

        TMP_Text text = CreateText(message, 24, FontStyles.Bold, textWhite, TextAlignmentOptions.Center, toast.transform);
        SetTextWrapping(text);
        StretchFull(text.rectTransform);

        yield return new WaitForSeconds(1.6f);
        if (toast != null) Destroy(toast);
    }

    // =========================================================
    // ADDITIONAL PROFILE / PROGRESS / DASHBOARD FEATURES
    // =========================================================

    // =========================================================
    // LOCAL LEARNING / PROFILE FEATURES
    // =========================================================

    private GameObject profileScreen;
    private GameObject progressScreen;
    private GameObject achievementsScreen;
    private GameObject mistakesScreen;

    private Button homeProfileButton;
    private TMP_Text homeProfileButtonText;
    private TMP_Text homeGreetingText;
    private GameObject homeAvatarRoot;
    private UIImage homeAvatarImage;
    private TMP_Text homeAvatarInitial;

    private Texture2D homeAvatarTexture;
    private Sprite homeAvatarSprite;
    private Sprite circleSprite;

    private Button learningFavoriteButton;

    private bool trackingFeatureWasRegistered;
    private string lastFeatureSubject = "";
    private string lastFeatureTopic = "";

    private readonly List<EduARQuizAnswerData> featureQuizAnswers =
        new List<EduARQuizAnswerData>();

    // =========================================================
    // FEATURE SCREENS
    // =========================================================

    private void BuildFeatureScreens()
    {
        BuildProfileScreen();
        BuildProgressScreen();
        BuildAchievementsScreen();
        BuildMistakesScreen();
    }

    private void BuildHomeFeatureUI()
    {
        if (homeScreen == null || homeProfileButton != null)
            return;

        // ---------------------------------------------------------
        // HOME GREETING (LEFT)
        // ---------------------------------------------------------
        homeAvatarRoot = CreateAvatarView(
            homeScreen.transform,
            new Vector2(0.09f, 0.918f),
            54f,
            out homeAvatarImage,
            out homeAvatarInitial
        );

        homeGreetingText = CreateText(
            "HI, STUDENT",
            24,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Left,
            homeScreen.transform
        );
        SetRect(
            homeGreetingText.rectTransform,
            new Vector2(0.135f, 0.890f),
            new Vector2(0.60f, 0.950f)
        );

        // ---------------------------------------------------------
        // DASHBOARD (RIGHT)
        // ---------------------------------------------------------
        homeProfileButton = CreateButton(
            "DASHBOARD",
            homeScreen.transform,
            buttonPillDark,
            textWhite,
            23
        );

        AddBorder(homeProfileButton.gameObject, glassBorder);

        SetRect(
            homeProfileButton.GetComponent<RectTransform>(),
            new Vector2(0.70f, 0.888f),
            new Vector2(0.95f, 0.950f)
        );

        homeProfileButtonText =
            homeProfileButton.GetComponentInChildren<TMP_Text>();

        homeProfileButton.onClick.AddListener(ShowProfileScreen);
        AddPressAnimation(homeProfileButton);

        RefreshHomeFeatureUI();
    }

    private void RefreshHomeFeatureUI()
    {
        if (homeProfileButton == null)
            return;

        EduARLocalUser user = EduARLocalAuth.CurrentUser;

        if (user == null)
        {
            homeGreetingText.text = "HI, STUDENT";
            homeProfileButtonText.text = "DASHBOARD";
            SetAvatarVisual(
                homeAvatarImage,
                homeAvatarInitial,
                null,
                "S"
            );
            return;
        }

        homeGreetingText.text =
            "HI, " + GetFirstName(user.fullName).ToUpper();

        homeProfileButtonText.text = "DASHBOARD";

        SetAvatarVisual(
            homeAvatarImage,
            homeAvatarInitial,
            user.avatarPath,
            GetInitial(user.fullName)
        );
    }

    // =========================================================
    // AVATAR HELPERS
    // =========================================================

    private GameObject CreateAvatarView(
        Transform parent,
        Vector2 centerAnchor,
        float size,
        out UIImage photoImage,
        out TMP_Text initialText)
    {
        GameObject root = CreateUIObject("AvatarRoot", parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();

        // Use equal anchors plus an explicit square size. This avoids the
        // AspectRatioFitter/percentage-anchor interaction that can make the
        // avatar expand into a huge circle on portrait layouts.
        rootRect.anchorMin = centerAnchor;
        rootRect.anchorMax = centerAnchor;
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(size, size);

        GameObject border = CreateUIObject("AvatarBorder", root.transform);
        UIImage borderImage = border.AddComponent<UIImage>();
        borderImage.sprite = GetCircleSprite();
        borderImage.color = new Color(1f, 1f, 1f, 0.18f);
        StretchFull(border.GetComponent<RectTransform>());

        GameObject fill = CreateUIObject("AvatarFill", root.transform);
        UIImage fillImage = fill.AddComponent<UIImage>();
        fillImage.sprite = GetCircleSprite();
        fillImage.color = new Color(0.39f, 0.40f, 0.95f, 0.24f);
        fillImage.rectTransform.anchorMin = new Vector2(0.08f, 0.08f);
        fillImage.rectTransform.anchorMax = new Vector2(0.92f, 0.92f);
        fillImage.rectTransform.offsetMin = Vector2.zero;
        fillImage.rectTransform.offsetMax = Vector2.zero;

        GameObject maskObject = CreateUIObject("AvatarMask", root.transform);
        UIImage maskImage = maskObject.AddComponent<UIImage>();
        maskImage.sprite = GetCircleSprite();
        maskImage.color = Color.white;
        StretchFull(maskImage.rectTransform);

        Mask mask = maskObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject imageObject = CreateUIObject("AvatarPhoto", maskObject.transform);
        photoImage = imageObject.AddComponent<UIImage>();
        // The saved avatar is cropped to a square by EduARAvatarManager, so
        // allow it to fill the circular mask completely.
        photoImage.type = UIImage.Type.Simple;
        photoImage.preserveAspect = false;
        photoImage.color = Color.white;
        StretchFull(photoImage.rectTransform);

        initialText = CreateText(
            "S",
            24,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            maskObject.transform
        );
        StretchFull(initialText.rectTransform);

        return root;
    }

    private void SetAvatarVisual(
        UIImage image,
        TMP_Text initial,
        string avatarPath,
        string initials)
    {
        if (image == null || initial == null)
            return;

        initial.text = string.IsNullOrWhiteSpace(initials) ? "S" : initials.ToUpper();

        if (image.sprite != null)
        {
            Sprite oldSprite = image.sprite;
            Texture2D oldTexture = oldSprite != null ? oldSprite.texture : null;

            image.sprite = null;

            if (oldSprite != null)
                Destroy(oldSprite);

            // Release dynamically loaded dashboard/profile avatar textures.
            // The currently displayed Home texture is kept until replaced.
            if (oldTexture != null && oldTexture != homeAvatarTexture)
                Destroy(oldTexture);
        }

        Texture2D texture = EduARAvatarManager.LoadAvatarTexture(avatarPath);

        if (texture == null)
        {
            image.color = new Color(1f, 1f, 1f, 0f);
            initial.color = textWhite;
            return;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        image.type = UIImage.Type.Simple;
        image.preserveAspect = false;
        image.sprite = sprite;
        image.color = Color.white;
        initial.color = new Color(1f, 1f, 1f, 0f);

        // Store the most recently displayed Home avatar resources so we can
        // clean them when the next avatar is loaded.
        if (ReferenceEquals(image, homeAvatarImage))
        {
            if (homeAvatarSprite != null)
                Destroy(homeAvatarSprite);
            if (homeAvatarTexture != null)
                Destroy(homeAvatarTexture);

            homeAvatarSprite = sprite;
            homeAvatarTexture = texture;
        }
    }

    private Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f
        );

        return circleSprite;
    }

    private string GetInitial(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "S";

        return fullName.Trim().Substring(0, 1).ToUpper();
    }

    private void PickDashboardAvatar()
    {
        EduARLocalUser user = EduARLocalAuth.CurrentUser;
        if (user == null)
        {
            ShowLogin();
            return;
        }

        EduARAvatarManager.PickAndStoreAvatar(
            user.userId,
            storedPath =>
            {
                if (string.IsNullOrWhiteSpace(storedPath))
                    return;

                EduARLocalAuth.SetAvatarPath(user.userId, storedPath);

                RefreshHomeFeatureUI();
                RefreshProfileScreen();
            }
        );
    }

    // =========================================================
    // PROFILE SCREEN
    // =========================================================

    private void BuildProfileScreen()
    {
        profileScreen = CreateScreen("Profile");
        CreateBackground(profileScreen, bgDark);
        profileScreen.SetActive(false);
    }

    private void ShowProfileScreen()
    {
        CloseFeatureScreens();
        DisableAllTargets();
        HideLearningPopup();

        RefreshProfileScreen();
        profileScreen.SetActive(true);
        StartCoroutine(FadeInScreen(profileScreen));
    }

    private void RefreshProfileScreen()
    {
        if (profileScreen == null)
            return;

        ClearChildren(profileScreen);
        CreateBackground(profileScreen, bgDark);

        EduARLocalUser user = EduARLocalAuth.CurrentUser;

        if (user == null)
        {
            ShowLogin();
            return;
        }

        Button back = CreateFeatureBackButton(profileScreen.transform, ShowHome);

        TMP_Text title = CreateText(
            "DASHBOARD",
            40,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            profileScreen.transform
        );
        SetRect(title.rectTransform, new Vector2(0.26f, 0.91f), new Vector2(0.74f, 0.98f));

        TMP_Text subtitle = CreateText(
            "LEARNING OVERVIEW  •  PERSONAL SPACE",
            15,
            FontStyles.Bold,
            scanTomato,
            TextAlignmentOptions.Center,
            profileScreen.transform
        );
        SetRect(subtitle.rectTransform, new Vector2(0.20f, 0.875f), new Vector2(0.80f, 0.915f));

        GameObject userCard = CreateRoundedPanel(
            "ProfileCard",
            profileScreen.transform,
            cardDark
        );
        AddBorder(userCard, new Color(scanTomato.r, scanTomato.g, scanTomato.b, 0.30f));
        SetRect(userCard.GetComponent<RectTransform>(), new Vector2(0.06f, 0.73f), new Vector2(0.94f, 0.88f));

        UIImage dashboardAvatar = null;
        TMP_Text dashboardInitial = null;
        CreateAvatarView(
            userCard.transform,
            new Vector2(0.12f, 0.50f),
            140f,
            out dashboardAvatar,
            out dashboardInitial
        );
        SetAvatarVisual(
            dashboardAvatar,
            dashboardInitial,
            user.avatarPath,
            GetInitial(user.fullName)
        );

        TMP_Text name = CreateText(
            user.fullName,
            30,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Left,
            userCard.transform
        );
        SetRect(name.rectTransform, new Vector2(0.24f, 0.56f), new Vector2(0.67f, 0.90f));

        TMP_Text identity = CreateText(
            "Student ID: " + user.studentId + "\n" + user.email,
            19,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Left,
            userCard.transform
        );
        SetTextWrapping(identity);
        SetRect(identity.rectTransform, new Vector2(0.24f, 0.10f), new Vector2(0.67f, 0.54f));

        Button changePhoto = CreateButton(
            EduARAvatarManager.HasAvatar(user.avatarPath) ? "CHANGE PHOTO" : "ADD PHOTO",
            userCard.transform,
            buttonPillDark,
            brandIndigo,
            18
        );
        AddBorder(changePhoto.gameObject, glassBorder);
        SetRect(changePhoto.GetComponent<RectTransform>(), new Vector2(0.70f, 0.52f), new Vector2(0.95f, 0.73f));
        changePhoto.onClick.AddListener(() => PickDashboardAvatar());
        AddPressAnimation(changePhoto);

        Button logout = CreateButton(
            "LOG OUT",
            userCard.transform,
            buttonPillDark,
            wrongRed,
            18
        );
        AddBorder(logout.gameObject, glassBorder);
        SetRect(logout.GetComponent<RectTransform>(), new Vector2(0.70f, 0.22f), new Vector2(0.95f, 0.43f));
        logout.onClick.AddListener(LogoutUser);
        AddPressAnimation(logout);

        GameObject overallCard = CreateRoundedPanel(
            "OverallProgress",
            profileScreen.transform,
            cardDark
        );
        AddBorder(overallCard, cardBorder);
        SetRect(overallCard.GetComponent<RectTransform>(), new Vector2(0.06f, 0.56f), new Vector2(0.94f, 0.70f));

        float overall = EduARLocalProgress.GetOverallProgress();

        TMP_Text overallLabel = CreateText(
            "OVERALL LEARNING PROGRESS",
            21,
            FontStyles.Bold,
            brandIndigo,
            TextAlignmentOptions.Left,
            overallCard.transform
        );
        SetRect(overallLabel.rectTransform, new Vector2(0.06f, 0.62f), new Vector2(0.75f, 0.90f));

        TMP_Text overallValue = CreateText(
            Mathf.RoundToInt(overall) + "%",
            34,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Right,
            overallCard.transform
        );
        SetRect(overallValue.rectTransform, new Vector2(0.76f, 0.55f), new Vector2(0.94f, 0.90f));

        CreateProgressBar(overallCard.transform, 0.06f, 0.25f, 0.94f, 0.44f, overall, brandIndigo);

        CreateProfileStatCard("AR SCANS", EduARLocalProgress.GetTotalArScans().ToString(), 0.06f, 0.43f, 0.28f, 0.53f, physColor, profileScreen.transform);
        CreateProfileStatCard("QUIZ ATTEMPTS", EduARLocalProgress.GetTotalQuizAttempts().ToString(), 0.36f, 0.43f, 0.58f, 0.53f, brandIndigo, profileScreen.transform);
        CreateProfileStatCard("AVG SCORE", EduARLocalProgress.GetAverageQuizScore() + "%", 0.66f, 0.43f, 0.94f, 0.53f, bioColor, profileScreen.transform);

        CreateSubjectProgressCard(profileScreen.transform, 0.06f, 0.20f, 0.94f, 0.40f);

        Button progress = CreateButton(
            "MY PROGRESS",
            profileScreen.transform,
            scanTomato,
            textWhite,
            22
        );
        SetRect(progress.GetComponent<RectTransform>(), new Vector2(0.06f, 0.105f), new Vector2(0.47f, 0.17f));
        progress.onClick.AddListener(ShowProgressScreen);
        AddPressAnimation(progress);

        Button achievements = CreateButton(
            "ACHIEVEMENTS",
            profileScreen.transform,
            brandIndigo,
            textWhite,
            22
        );
        AddBorder(achievements.gameObject, cardBorder);
        SetRect(achievements.GetComponent<RectTransform>(), new Vector2(0.53f, 0.105f), new Vector2(0.94f, 0.17f));
        achievements.onClick.AddListener(ShowAchievementsScreen);
        AddPressAnimation(achievements);

        Button mistakes = CreateButton(
            "REVIEW MISTAKES",
            profileScreen.transform,
            wrongRed,
            textWhite,
            22
        );
        AddBorder(mistakes.gameObject, new Color(1f, 1f, 1f, 0.08f));
        SetRect(mistakes.GetComponent<RectTransform>(), new Vector2(0.06f, 0.025f), new Vector2(0.94f, 0.095f));
        mistakes.onClick.AddListener(ShowMistakesScreen);
        AddPressAnimation(mistakes);
    }

    // =========================================================
    // PROGRESS SCREEN
    // =========================================================

    private void BuildProgressScreen()
    {
        progressScreen = CreateScreen("Progress");
        CreateBackground(progressScreen, bgDark);
        progressScreen.SetActive(false);
    }

    private void ShowProgressScreen()
    {
        CloseFeatureScreens();
        DisableAllTargets();
        HideLearningPopup();

        RefreshProgressScreen();
        progressScreen.SetActive(true);
        StartCoroutine(FadeInScreen(progressScreen));
    }

    private void RefreshProgressScreen()
    {
        if (progressScreen == null)
            return;

        ClearChildren(progressScreen);
        CreateBackground(progressScreen, bgDark);

        CreateFeatureBackButton(progressScreen.transform, ShowProfileScreen);

        TMP_Text title = CreateText(
            "MY PROGRESS",
            40,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            progressScreen.transform
        );
        SetRect(title.rectTransform, new Vector2(0.26f, 0.91f), new Vector2(0.74f, 0.98f));

        GameObject overallCard = CreateRoundedPanel(
            "Overall",
            progressScreen.transform,
            cardDark
        );
        AddBorder(overallCard, cardBorder);
        SetRect(overallCard.GetComponent<RectTransform>(), new Vector2(0.06f, 0.76f), new Vector2(0.94f, 0.89f));

        float overall = EduARLocalProgress.GetOverallProgress();

        TMP_Text overallText = CreateText(
            "OVERALL PROGRESS",
            24,
            FontStyles.Bold,
            brandIndigo,
            TextAlignmentOptions.Left,
            overallCard.transform
        );
        SetRect(overallText.rectTransform, new Vector2(0.06f, 0.60f), new Vector2(0.70f, 0.88f));

        TMP_Text percentage = CreateText(
            Mathf.RoundToInt(overall) + "%",
            36,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Right,
            overallCard.transform
        );
        SetRect(percentage.rectTransform, new Vector2(0.75f, 0.52f), new Vector2(0.94f, 0.92f));

        CreateProgressBar(overallCard.transform, 0.06f, 0.20f, 0.94f, 0.42f, overall, brandIndigo);

        GameObject subjectGraph = CreateRoundedPanel(
            "SubjectGraph",
            progressScreen.transform,
            cardDark
        );
        AddBorder(subjectGraph, cardBorder);
        SetRect(subjectGraph.GetComponent<RectTransform>(), new Vector2(0.06f, 0.44f), new Vector2(0.94f, 0.72f));

        TMP_Text graphTitle = CreateText(
            "SUBJECT PROGRESS",
            24,
            FontStyles.Bold,
            textMuted,
            TextAlignmentOptions.Left,
            subjectGraph.transform
        );
        SetRect(graphTitle.rectTransform, new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.96f));

        string[] subjects = { "Biology", "Physics", "Chemistry", "Mathematics" };
        Color[] colors = { bioColor, physColor, chemColor, mathColor };
        float[] rows = { 0.66f, 0.48f, 0.30f, 0.12f };

        for (int i = 0; i < subjects.Length; i++)
        {
            float value = EduARLocalProgress.GetSubjectProgress(subjects[i]);
            CreateBarRow(
                subjectGraph.transform,
                subjects[i],
                value,
                colors[i],
                0.06f,
                rows[i],
                0.94f
            );
        }

        GameObject quizGraph = CreateRoundedPanel(
            "QuizGraph",
            progressScreen.transform,
            cardDark
        );
        AddBorder(quizGraph, cardBorder);
        SetRect(quizGraph.GetComponent<RectTransform>(), new Vector2(0.06f, 0.19f), new Vector2(0.94f, 0.40f));

        TMP_Text quizTitle = CreateText(
            "RECENT QUIZ PERFORMANCE",
            18,
            FontStyles.Bold,
            textMuted,
            TextAlignmentOptions.Left,
            quizGraph.transform
        );
        SetRect(quizTitle.rectTransform, new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.96f));

        CreateQuizHistoryGraph(quizGraph.transform);

        GameObject goalCard = CreateRoundedPanel(
            "DailyGoal",
            progressScreen.transform,
            cardDark
        );
        AddBorder(goalCard, cardBorder);
        SetRect(goalCard.GetComponent<RectTransform>(), new Vector2(0.06f, 0.07f), new Vector2(0.94f, 0.16f));

        int goal = EduARLocalProgress.GetDailyGoalCompletedCount();
        int streak = EduARLocalProgress.GetStreak();

        TMP_Text goalText = CreateText(
            "DAILY GOAL  " + goal + "/3",
            24,
            FontStyles.Bold,
            physColor,
            TextAlignmentOptions.Left,
            goalCard.transform
        );
        SetRect(goalText.rectTransform, new Vector2(0.06f, 0.56f), new Vector2(0.37f, 0.92f));

        TMP_Text streakText = CreateText(
            "STREAK  " + streak + "D",
            24,
            FontStyles.Bold,
            bioColor,
            TextAlignmentOptions.Left,
            goalCard.transform
        );
        SetRect(streakText.rectTransform, new Vector2(0.38f, 0.56f), new Vector2(0.62f, 0.92f));

        CreateProgressBar(goalCard.transform, 0.06f, 0.18f, 0.62f, 0.35f, goal / 3f * 100f, physColor);

        List<string> recent = EduARLocalProgress.GetRecentTopics(1);
        string continueLabel = recent.Count > 0 ? "CONTINUE" : "START";

        Button continueButton = CreateButton(
            continueLabel,
            goalCard.transform,
            buttonPillDark,
            textWhite,
            18
        );
        AddBorder(continueButton.gameObject, glassBorder);
        SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(0.67f, 0.22f), new Vector2(0.94f, 0.82f));
        continueButton.onClick.AddListener(OpenMostRecentTopic);
        AddPressAnimation(continueButton);
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private void BuildAchievementsScreen()
    {
        achievementsScreen = CreateScreen("Achievements");
        CreateBackground(achievementsScreen, bgDark);
        achievementsScreen.SetActive(false);
    }

    private void ShowAchievementsScreen()
    {
        CloseFeatureScreens();
        DisableAllTargets();
        HideLearningPopup();

        RefreshAchievementsScreen();
        achievementsScreen.SetActive(true);
        StartCoroutine(FadeInScreen(achievementsScreen));
    }

    private void RefreshAchievementsScreen()
    {
        ClearChildren(achievementsScreen);
        CreateBackground(achievementsScreen, bgDark);
        CreateFeatureBackButton(achievementsScreen.transform, ShowProfileScreen);

        TMP_Text title = CreateText(
            "ACHIEVEMENTS",
            40,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            achievementsScreen.transform
        );
        SetRect(title.rectTransform, new Vector2(0.26f, 0.91f), new Vector2(0.74f, 0.98f));

        CreateAchievementCard("FIRST STEPS", "Complete your first quiz.", "first_quiz", 0.06f, 0.69f, 0.47f, 0.84f, brandIndigo);
        CreateAchievementCard("AR EXPLORER", "Explore 10 AR sessions.", "ar_explorer", 0.53f, 0.69f, 0.94f, 0.84f, physColor);
        CreateAchievementCard("QUIZ CHAMPION", "Get three perfect quiz scores.", "quiz_champion", 0.06f, 0.51f, 0.47f, 0.66f, mathColor);
        CreateAchievementCard("ANATOMY MASTER", "Master Heart, Brain and Lungs.", "anatomy_master", 0.53f, 0.51f, 0.94f, 0.66f, bioColor);
        CreateAchievementCard("SCIENCE EXPLORER", "Explore at least three subjects.", "science_explorer", 0.06f, 0.33f, 0.47f, 0.48f, chemColor);
        CreateAchievementCard("7 DAY STREAK", "Learn on seven consecutive days.", "streak_7", 0.53f, 0.33f, 0.94f, 0.48f, brandIndigo);

        int unlocked = 0;
        string[] ids = { "first_quiz", "ar_explorer", "quiz_champion", "anatomy_master", "science_explorer", "streak_7" };
        for (int i = 0; i < ids.Length; i++)
        {
            if (EduARLocalProgress.IsAchievementUnlocked(ids[i]))
                unlocked++;
        }

        TMP_Text count = CreateText(
            unlocked + "/6 UNLOCKED",
            24,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            achievementsScreen.transform
        );
        SetRect(count.rectTransform, new Vector2(0.25f, 0.20f), new Vector2(0.75f, 0.25f));

        TMP_Text note = CreateText(
            "Keep exploring, learning and testing your knowledge.",
            19,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Center,
            achievementsScreen.transform
        );
        SetTextWrapping(note);
        SetRect(note.rectTransform, new Vector2(0.10f, 0.12f), new Vector2(0.90f, 0.19f));
    }

    private void CreateAchievementCard(
        string title,
        string description,
        string achievementId,
        float minX,
        float minY,
        float maxX,
        float maxY,
        Color accent)
    {
        bool unlocked = EduARLocalProgress.IsAchievementUnlocked(achievementId);
        Color panelColor = unlocked ? cardDark : new Color(0.08f, 0.10f, 0.15f, 1f);
        Color titleColor = unlocked ? accent : textMuted;

        GameObject card = CreateRoundedPanel(
            "Achievement_" + achievementId,
            achievementsScreen.transform,
            panelColor
        );
        AddBorder(card, unlocked ? glassBorder : cardBorder);
        SetRect(card.GetComponent<RectTransform>(), new Vector2(minX, minY), new Vector2(maxX, maxY));

        TMP_Text state = CreateText(
            unlocked ? "UNLOCKED" : "LOCKED",
            18,
            FontStyles.Bold,
            titleColor,
            TextAlignmentOptions.Right,
            card.transform
        );
        SetRect(state.rectTransform, new Vector2(0.45f, 0.72f), new Vector2(0.94f, 0.94f));

        TMP_Text titleText = CreateText(
            title,
            23,
            FontStyles.Bold,
            titleColor,
            TextAlignmentOptions.Left,
            card.transform
        );
        SetTextWrapping(titleText);
        SetRect(titleText.rectTransform, new Vector2(0.07f, 0.48f), new Vector2(0.90f, 0.76f));

        TMP_Text body = CreateText(
            description,
            18,
            FontStyles.Normal,
            textMuted,
            TextAlignmentOptions.Left,
            card.transform
        );
        SetTextWrapping(body);
        SetRect(body.rectTransform, new Vector2(0.07f, 0.08f), new Vector2(0.92f, 0.45f));
    }

    // =========================================================
    // MISTAKE REVIEW
    // =========================================================

    private void BuildMistakesScreen()
    {
        mistakesScreen = CreateScreen("Mistakes");
        CreateBackground(mistakesScreen, bgDark);
        mistakesScreen.SetActive(false);
    }

    private void ShowMistakesScreen()
    {
        CloseFeatureScreens();
        DisableAllTargets();
        HideLearningPopup();

        RefreshMistakesScreen();
        mistakesScreen.SetActive(true);
        StartCoroutine(FadeInScreen(mistakesScreen));
    }

    private void RefreshMistakesScreen()
    {
        ClearChildren(mistakesScreen);
        CreateBackground(mistakesScreen, bgDark);
        CreateFeatureBackButton(mistakesScreen.transform, ShowProfileScreen);

        TMP_Text title = CreateText(
            "REVIEW MISTAKES",
            40,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            mistakesScreen.transform
        );
        SetRect(title.rectTransform, new Vector2(0.26f, 0.91f), new Vector2(0.74f, 0.98f));

        List<EduARMistakeEntry> mistakes =
            EduARLocalProgress.GetMistakes(5);

        if (mistakes.Count == 0)
        {
            TMP_Text empty = CreateText(
                "No mistakes saved yet.\nComplete a quiz to build your review list.",
                20,
                FontStyles.Normal,
                textMuted,
                TextAlignmentOptions.Center,
                mistakesScreen.transform
            );
            SetTextWrapping(empty);
            SetRect(empty.rectTransform, new Vector2(0.10f, 0.42f), new Vector2(0.90f, 0.58f));
            return;
        }

        float[] centers = { 0.73f, 0.57f, 0.41f, 0.25f, 0.09f };

        for (int i = 0; i < mistakes.Count; i++)
        {
            float center = centers[i];

            GameObject card = CreateRoundedPanel(
                "MistakeCard" + i,
                mistakesScreen.transform,
                cardDark
            );
            AddBorder(card, cardBorder);
            SetRect(card.GetComponent<RectTransform>(), new Vector2(0.06f, center - 0.065f), new Vector2(0.94f, center + 0.065f));

            TMP_Text topic = CreateText(
                mistakes[i].topic.ToUpper() + " • " + mistakes[i].subject.ToUpper(),
                18,
                FontStyles.Bold,
                wrongRed,
                TextAlignmentOptions.Left,
                card.transform
            );
            SetRect(topic.rectTransform, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.94f));

            TMP_Text question = CreateText(
                mistakes[i].question,
                24,
                FontStyles.Bold,
                textWhite,
                TextAlignmentOptions.Left,
                card.transform
            );
            SetTextWrapping(question);
            SetRect(question.rectTransform, new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.72f));

            TMP_Text answer = CreateText(
                "Your: " + mistakes[i].selectedAnswer + "  |  Correct: " + mistakes[i].correctAnswer,
                21,
                FontStyles.Normal,
                textMuted,
                TextAlignmentOptions.Left,
                card.transform
            );
            SetTextWrapping(answer);
            SetRect(answer.rectTransform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.36f));
        }
    }

    // =========================================================
    // FAVORITES
    // =========================================================

    private void BuildFeatureLearningControls()
    {
        if (learningPopup == null || learningFavoriteButton != null)
            return;

        learningFavoriteButton = CreateButton(
            "FAVORITE",
            learningPopup.transform,
            buttonPillDark,
            textWhite,
            20
        );
        AddBorder(learningFavoriteButton.gameObject, glassBorder);

        SetRect(
    learningFavoriteButton.GetComponent<RectTransform>(),
    new Vector2(0.06f, 0.06f),
    new Vector2(0.30f, 0.15f)
);

        learningFavoriteButton.onClick.AddListener(ToggleCurrentFavorite);
        AddPressAnimation(learningFavoriteButton);
    }

    private void RefreshLearningFavoriteButton()
    {
        if (learningFavoriteButton == null)
            return;

        bool favorite = EduARLocalProgress.IsFavorite(currentSubject, currentTopic);
        TMP_Text text = learningFavoriteButton.GetComponentInChildren<TMP_Text>();

        if (text != null)
            text.text = favorite ? "FAVORITED" : "FAVORITE";

        SetButtonImageColor(
            learningFavoriteButton,
            favorite ? GetSubjectColor(currentSubject) : buttonPillDark
        );
    }

    private void ToggleCurrentFavorite()
    {
        EduARLocalProgress.ToggleFavorite(currentSubject, currentTopic);
        RefreshLearningFavoriteButton();
    }

    // =========================================================
    // QUIZ / AR TRACKING HOOKS
    // =========================================================

    private void BeginFeatureQuizSession()
    {
        featureQuizAnswers.Clear();
    }

    private void RecordFeatureQuizAnswer(
        QuestionData question,
        int selectedIndex)
    {
        if (question == null || question.answers == null)
            return;

        featureQuizAnswers.Add(new EduARQuizAnswerData
        {
            question = question.question,
            selectedAnswer = question.answers[selectedIndex],
            correctAnswer = question.answers[question.correctIndex],
            isCorrect = selectedIndex == question.correctIndex
        });
    }

    private void CompleteFeatureQuizSession()
    {
        EduARLocalProgress.SaveQuizResult(
            currentSubject,
            currentTopic,
            currentScore,
            currentQuiz.Count,
            featureQuizAnswers
        );
    }

    private void ResetTrackingFeatureState()
    {
        trackingFeatureWasRegistered = false;
    }

    private bool RegisterTrackingFeatureIfNeeded()
    {
        if (trackingFeatureWasRegistered)
            return false;

        trackingFeatureWasRegistered = true;
        lastFeatureSubject = currentSubject;
        lastFeatureTopic = currentTopic;
        return true;
    }

    private void SetLastFeatureTopic(string subject, string topic)
    {
        lastFeatureSubject = subject;
        lastFeatureTopic = topic;
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void BackFromAR()
    {
        PauseAllModelAnimations();
        HideAllPartLabels();
        DisableAllTargets();
        HideLearningPopup();
        arScreen.SetActive(false);
        quizScreen.SetActive(false);
        resultScreen.SetActive(false);
        ShowHome();
    }

    private void BackFromQuiz()
    {
        HideLearningPopup();
        SetCurrentPartLabelsVisible(false);

        // Leave quiz first, then restore the exact AR topic.
        quizScreen.SetActive(false);
        resultScreen.SetActive(false);

        if (arScreen != null)
            arScreen.SetActive(false);

        DisableAllTargets();

        GameObject target = GetTarget(currentTopic);
        if (target == null)
        {
            ShowTopicBrowser(currentSubject);
            return;
        }

        target.SetActive(true);
        SetModelActiveForTopic(currentTopic, true);
        arScreen.SetActive(true);

        Color subjectColor = GetSubjectColor(currentSubject);
        arTitle.text = GetTopicDisplayName(currentTopic);
        arSubtitle.text = currentSubject + " • AR Learning";
        SetDrawerCategoryText(currentSubject, subjectColor);
        drawerTitle.text = GetTopicDisplayName(currentTopic);
        drawerDescription.text = GetShortDescription(currentTopic);
        SetButtonImageColor(arLearnButton, subjectColor);
        SetButtonImageColor(arQuizButton, brandIndigo);
        SetCurrentAnimationPlaying(true);
        RefreshAnimationButton();
        trackingText.text = "◌ SCANNING";
        trackingText.color = textMuted;
        ResetTrackingFeatureState();
        SetLastFeatureTopic(currentSubject, currentTopic);

        StartCoroutine(FadeInScreen(arScreen));
    }

    private void BackFromResult()
    {
        resultScreen.SetActive(false);
        quizScreen.SetActive(false);
        BackFromQuiz();
    }

    private void CloseFeatureScreens()
    {
        profileScreen?.SetActive(false);
        progressScreen?.SetActive(false);
        achievementsScreen?.SetActive(false);
        mistakesScreen?.SetActive(false);
    }

    private void LogoutUser()
    {
        HideAllPartLabels();
        DisableAllTargets();
        HideLearningPopup();

        EduARLocalProgress.EndSession();
        EduARLocalAuth.Logout();

        CloseFeatureScreens();
        ShowLogin();
    }

    // =========================================================
    // MODEL CONTROLLER SETUP
    // =========================================================

    private void SetupModelAnimationControllers()
    {
        heartAnimation = CreateModelAnimationPivot(
            heartModel,
            EduARModelAnimationController.AnimationStyle.HeartBeat,
            "Heart"
        );

        brainAnimation = CreateModelAnimationPivot(
            brainModel,
            EduARModelAnimationController.AnimationStyle.BrainPulse,
            "Brain"
        );

        lungsAnimation = CreateModelAnimationPivot(
            lungsModel,
            EduARModelAnimationController.AnimationStyle.LungBreathing,
            "Lungs"
        );

        physicsAnimation = CreateModelAnimationPivot(
            physicsModel,
            EduARModelAnimationController.AnimationStyle.PendulumSwing,
            "Pendulum"
        );

        chemistryAnimation = CreateModelAnimationPivot(
            chemistryModel,
            EduARModelAnimationController.AnimationStyle.WaterRotate,
            "Water"
        );
    }

    private EduARModelAnimationController CreateModelAnimationPivot(
        Transform model,
        EduARModelAnimationController.AnimationStyle style,
        string label)
    {
        if (model == null)
            return null;

        Transform existingPivot = model.parent;
        if (existingPivot != null)
        {
            EduARModelAnimationController existing =
                existingPivot.GetComponent<EduARModelAnimationController>();

            if (existing != null)
                return existing;
        }

        Transform originalParent = model.parent;
        GameObject pivotObject = new GameObject("__EduARAnimationPivot_" + label);
        Transform pivot = pivotObject.transform;
        pivot.SetParent(originalParent, false);

        // Preserve the original AR placement. Put the larger baseline scale
        // directly on the model so HeartTouchController captures that scale
        // as its normal mobile size. The animation pivot stays at scale 1.
        Vector3 originalModelScale = model.localScale;

        // Pendulum needs a real hinge point rather than rotating around its
        // geometric center; this keeps the motion inside the camera view.
        Vector3 pivotWorld = model.position;
        if (style == EduARModelAnimationController.AnimationStyle.PendulumSwing)
        {
            Bounds bounds = CalculateModelWorldBounds(model);
            pivotWorld = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        pivot.position = pivotWorld;
        pivot.rotation = model.rotation;
        pivot.localScale = Vector3.one;

        model.SetParent(pivot, true);
        model.localRotation = Quaternion.identity;

        float topicScaleFactor = GetTopicScaleFactor(label);
        model.localScale = originalModelScale *
            Mathf.Max(0.05f, mobileModelScaleMultiplier * topicScaleFactor);

        // The pendulum asset is authored with extra negative vertical offset.
        // Lift it so the complete assembly sits on/just above the scanned image.
        if (style == EduARModelAnimationController.AnimationStyle.PendulumSwing && originalParent != null)
        {
            Bounds scaledBounds = CalculateModelWorldBounds(model);
            Vector3 targetUp = originalParent.up;
            float signedBottom = Vector3.Dot(scaledBounds.min - originalParent.position, targetUp);
            float lift = 0.06f - signedBottom;
            if (lift > 0f)
                model.position += targetUp * lift;
        }

        EduARModelAnimationController controller =
            pivotObject.AddComponent<EduARModelAnimationController>();
        controller.Configure(model, style);
        return controller;
    }

    private float GetTopicScaleFactor(string label)
    {
        switch (label)
        {
            case "Heart": return 0.86f;      // Keep the current Heart size unchanged.
            case "Brain": return 0.80f;      // Close to Heart size.
            case "Lungs": return 0.76f;      // Close to Heart size without overfilling.
            case "Pendulum": return 0.74f;   // Larger and easier to see on mobile.
            case "Water": return 0.72f;      // Slightly larger presentation.
            default: return 0.70f;
        }
    }

    private Bounds CalculateModelWorldBounds(Transform model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
            return new Bounds(model.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    private EduARModelAnimationController GetCurrentAnimationController()
    {
        switch (currentTopic)
        {
            case "Heart": return heartAnimation;
            case "Brain": return brainAnimation;
            case "Lungs": return lungsAnimation;
            case "Pendulum": return physicsAnimation;
            case "Water": return chemistryAnimation;
            default: return null;
        }
    }

    private void SetCurrentAnimationPlaying(bool playing)
    {
        EduARModelAnimationController controller = GetCurrentAnimationController();
        if (controller == null)
            return;

        if (playing)
            controller.Play();
        else
            controller.Pause();
    }

    private void PauseAllModelAnimations()
    {
        if (heartAnimation != null) heartAnimation.Pause();
        if (brainAnimation != null) brainAnimation.Pause();
        if (lungsAnimation != null) lungsAnimation.Pause();
        if (physicsAnimation != null) physicsAnimation.Pause();
        if (chemistryAnimation != null) chemistryAnimation.Pause();
    }

    private void ToggleCurrentAnimation()
    {
        EduARModelAnimationController controller = GetCurrentAnimationController();
        if (controller == null)
            return;

        controller.Toggle();
        RefreshAnimationButton();
    }

    private void RefreshAnimationButton()
    {
        if (arAnimationButton == null)
            return;

        EduARModelAnimationController controller = GetCurrentAnimationController();
        bool playing = controller != null && controller.IsPlaying;

        TMP_Text label = arAnimationButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = playing ? "PAUSE ANIM" : "PLAY ANIM";

        SetButtonImageColor(
            arAnimationButton,
            playing ? GetSubjectColor(currentSubject) : buttonPillDark
        );
    }

    private void SetupModelPartLabels()
    {
        heartPartLabels = CreatePartLabels(heartModel, EduARModelPartLabelController.ModelType.Heart, bioColor);
        brainPartLabels = CreatePartLabels(brainModel, EduARModelPartLabelController.ModelType.Brain, bioColor);
        lungsPartLabels = CreatePartLabels(lungsModel, EduARModelPartLabelController.ModelType.Lungs, bioColor);
        physicsPartLabels = CreatePartLabels(physicsModel, EduARModelPartLabelController.ModelType.Pendulum, physColor);
        chemistryPartLabels = CreatePartLabels(chemistryModel, EduARModelPartLabelController.ModelType.Water, chemColor);
        HideAllPartLabels();
    }

    private EduARModelPartLabelController CreatePartLabels(
        Transform model,
        EduARModelPartLabelController.ModelType type,
        Color accent)
    {
        if (model == null || canvas == null)
            return null;

        EduARModelPartLabelController controller =
            model.GetComponent<EduARModelPartLabelController>();

        if (controller == null)
            controller = model.gameObject.AddComponent<EduARModelPartLabelController>();

        controller.Configure(model, canvas, arScreen.transform, type, accent);
        return controller;
    }

    private void SetCurrentPartLabelsVisible(bool visible)
    {
        // Only one model's labels may exist on screen at a time. This is
        // especially important when switching directly from Heart -> Brain ->
        // Lungs without leaving the AR screen.
        HideAllPartLabels();

        if (!visible)
            return;

        switch (currentTopic)
        {
            case "Heart":
                if (heartPartLabels != null) heartPartLabels.SetVisible(visible);
                break;
            case "Brain":
                if (brainPartLabels != null) brainPartLabels.SetVisible(visible);
                break;
            case "Lungs":
                if (lungsPartLabels != null) lungsPartLabels.SetVisible(visible);
                break;
            case "Pendulum":
                if (physicsPartLabels != null) physicsPartLabels.SetVisible(visible);
                break;
            case "Water":
                if (chemistryPartLabels != null) chemistryPartLabels.SetVisible(visible);
                break;
        }
    }

    private void TogglePartLabels()
    {
        partLabelsVisible = !partLabelsVisible;
        SetCurrentPartLabelsVisible(partLabelsVisible);

        if (arPartsButton != null)
        {
            TMP_Text label = arPartsButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = partLabelsVisible ? "HIDE PARTS" : "SHOW PARTS";

            SetButtonImageColor(arPartsButton, partLabelsVisible ? scanTomato : buttonPillDark);
        }
    }

    private void HideAllPartLabels()
    {
        if (heartPartLabels != null) heartPartLabels.SetVisible(false);
        if (brainPartLabels != null) brainPartLabels.SetVisible(false);
        if (lungsPartLabels != null) lungsPartLabels.SetVisible(false);
        if (physicsPartLabels != null) physicsPartLabels.SetVisible(false);
        if (chemistryPartLabels != null) chemistryPartLabels.SetVisible(false);
    }

    private void SetupModelTouchControllers()
    {
        AddTouchControllerIfNeeded(heartModel);
        AddTouchControllerIfNeeded(brainModel);
        AddTouchControllerIfNeeded(lungsModel);
        AddTouchControllerIfNeeded(physicsModel);
        AddTouchControllerIfNeeded(chemistryModel);
    }

    private void AddTouchControllerIfNeeded(Transform model)
    {
        if (model == null)
            return;

        if (model.GetComponent<HeartTouchController>() == null)
            model.gameObject.AddComponent<HeartTouchController>();
    }

    private void ResetCurrentModel()
    {
        Transform model = null;

        switch (currentTopic)
        {
            case "Heart": model = heartModel; break;
            case "Brain": model = brainModel; break;
            case "Lungs": model = lungsModel; break;
            case "Pendulum": model = physicsModel; break;
            case "Water": model = chemistryModel; break;
        }

        if (model == null)
            return;

        HeartTouchController controller =
            model.GetComponent<HeartTouchController>();

        if (controller == null)
        {
            controller = model.gameObject.AddComponent<HeartTouchController>();
        }

        controller.ResetModel();

        EduARModelAnimationController animation = GetCurrentAnimationController();
        if (animation != null)
            animation.ResetAnimation();
    }

    private void OpenMostRecentTopic()
    {
        List<string> recent = EduARLocalProgress.GetRecentTopics(1);

        if (recent.Count == 0)
        {
            ShowHome();
            return;
        }

        string[] parts = recent[0].Split('|');
        if (parts.Length != 2)
        {
            ShowHome();
            return;
        }

        currentSubject = parts[0];
        currentTopic = parts[1];

        if (GetTarget(currentTopic) == null)
        {
            ShowComingSoon(GetTopicDisplayName(currentTopic) + "\n\nAR Target not yet assigned.");
            return;
        }

        OpenAR(currentTopic);
    }

    // =========================================================
    // REUSABLE PROFILE / GRAPH HELPERS
    // =========================================================

    private Button CreateFeatureBackButton(
        Transform parent,
        UnityEngine.Events.UnityAction action)
    {
        Button back = CreateButton(
            "< Back",
            parent,
            buttonPillDark,
            textWhite,
            20
        );

        AddBorder(back.gameObject, glassBorder);
        SetRect(back.GetComponent<RectTransform>(), new Vector2(0.04f, 0.915f), new Vector2(0.25f, 0.975f));
        back.onClick.AddListener(action);
        AddPressAnimation(back);
        return back;
    }

    private void CreateProfileStatCard(
        string title,
        string value,
        float minX,
        float minY,
        float maxX,
        float maxY,
        Color accent,
        Transform parent)
    {
        GameObject card = CreateRoundedPanel(
            title.Replace(" ", "") + "Stat",
            parent,
            cardDark
        );
        AddBorder(card, cardBorder);
        SetRect(card.GetComponent<RectTransform>(), new Vector2(minX, minY), new Vector2(maxX, maxY));

        TMP_Text valueText = CreateText(
            value,
            48,
            FontStyles.Bold,
            accent,
            TextAlignmentOptions.Center,
            card.transform
        );
        SetRect(valueText.rectTransform, new Vector2(0.03f, 0.40f), new Vector2(0.97f, 0.92f));

        TMP_Text titleText = CreateText(
            title,
            17,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Center,
            card.transform
        );
        SetTextWrapping(titleText);
        SetRect(titleText.rectTransform, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.30f));
    }

    private void CreateSubjectProgressCard(
        Transform parent,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        GameObject card = CreateRoundedPanel(
            "SubjectProgress",
            parent,
            cardDark
        );
        AddBorder(card, cardBorder);
        SetRect(card.GetComponent<RectTransform>(), new Vector2(minX, minY), new Vector2(maxX, maxY));

        TMP_Text title = CreateText(
            "SUBJECT PROGRESS",
            28,
            FontStyles.Bold,
            textMuted,
            TextAlignmentOptions.Left,
            card.transform
        );
        SetRect(title.rectTransform, new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.98f));

        string[] subjects = { "Biology", "Physics", "Chemistry", "Mathematics" };
        Color[] colors = { bioColor, physColor, chemColor, mathColor };
        float[] rows = { 0.66f, 0.48f, 0.30f, 0.12f };

        for (int i = 0; i < subjects.Length; i++)
        {
            float value = EduARLocalProgress.GetSubjectProgress(subjects[i]);
            CreateBarRow(card.transform, subjects[i], value, colors[i], 0.05f, rows[i], 0.95f);
        }
    }

    private void CreateProgressBar(
        Transform parent,
        float minX,
        float minY,
        float maxX,
        float maxY,
        float percentage,
        Color accent)
    {
        GameObject background = CreateRoundedPanel(
            "ProgressBackground",
            parent,
            new Color(1f, 1f, 1f, 0.07f)
        );
        SetRect(background.GetComponent<RectTransform>(), new Vector2(minX, minY), new Vector2(maxX, maxY));

        GameObject fill = CreateRoundedPanel(
            "ProgressFill",
            background.transform,
            accent
        );

        float normalized = Mathf.Clamp01(percentage / 100f);
        SetRect(fill.GetComponent<RectTransform>(), Vector2.zero, new Vector2(normalized, 1f));
    }

    private void CreateBarRow(
        Transform parent,
        string label,
        float percentage,
        Color accent,
        float minX,
        float centerY,
        float maxX)
    {
        // Keep label, bar and percentage on the SAME horizontal center line.
        // This prevents the larger mobile text from appearing too high/low.
        TMP_Text labelText = CreateText(
            label,
            22,
            FontStyles.Bold,
            textMuted,
            TextAlignmentOptions.Left,
            parent
        );
        SetRect(
            labelText.rectTransform,
            new Vector2(minX, centerY - 0.055f),
            new Vector2(0.29f, centerY + 0.055f)
        );

        TMP_Text valueText = CreateText(
            Mathf.RoundToInt(percentage) + "%",
            25,
            FontStyles.Bold,
            textWhite,
            TextAlignmentOptions.Right,
            parent
        );
        SetRect(
            valueText.rectTransform,
            new Vector2(0.82f, centerY - 0.055f),
            new Vector2(maxX, centerY + 0.055f)
        );

        // Thick, clearly visible progress bar.
        GameObject bg = CreateRoundedPanel(
            "SubjectBarBG",
            parent,
            new Color(1f, 1f, 1f, 0.12f)
        );
        SetRect(
            bg.GetComponent<RectTransform>(),
            new Vector2(0.31f, centerY - 0.027f),
            new Vector2(0.79f, centerY + 0.027f)
        );

        GameObject fill = CreateRoundedPanel(
            "SubjectBarFill",
            bg.transform,
            accent
        );
        SetRect(
            fill.GetComponent<RectTransform>(),
            Vector2.zero,
            new Vector2(Mathf.Clamp01(percentage / 100f), 1f)
        );
    }

    private void CreateQuizHistoryGraph(Transform parent)
    {
        List<EduARQuizHistoryEntry> history =
            EduARLocalProgress.GetQuizHistory(5);

        if (history.Count == 0)
        {
            TMP_Text empty = CreateText(
                "Complete quizzes to see your performance graph.",
                17,
                FontStyles.Normal,
                textMuted,
                TextAlignmentOptions.Center,
                parent
            );
            SetRect(empty.rectTransform, new Vector2(0.10f, 0.35f), new Vector2(0.90f, 0.65f));
            return;
        }

        for (int i = 0; i < history.Count; i++)
        {
            float xMin = 0.07f + i * 0.18f;
            float xMax = xMin + 0.12f;
            float percentage = Mathf.Clamp(history[history.Count - 1 - i].percentage, 0f, 100f);

            GameObject bg = CreateRoundedPanel(
                "QuizColumnBG" + i,
                parent,
                new Color(1f, 1f, 1f, 0.05f)
            );
            SetRect(bg.GetComponent<RectTransform>(), new Vector2(xMin, 0.18f), new Vector2(xMax, 0.70f));

            GameObject bar = CreateRoundedPanel(
                "QuizColumn" + i,
                bg.transform,
                GetSubjectColor(history[history.Count - 1 - i].subject)
            );

            float normalized = Mathf.Clamp01(percentage / 100f);
            SetRect(bar.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, normalized));

            TMP_Text value = CreateText(
                Mathf.RoundToInt(percentage) + "%",
                16,
                FontStyles.Bold,
                textWhite,
                TextAlignmentOptions.Center,
                parent
            );
            SetRect(value.rectTransform, new Vector2(xMin - 0.02f, 0.73f), new Vector2(xMax + 0.02f, 0.84f));

            TMP_Text label = CreateText(
                GetShortTopicLabel(history[history.Count - 1 - i].topic),
                15,
                FontStyles.Bold,
                textMuted,
                TextAlignmentOptions.Center,
                parent
            );
            SetRect(label.rectTransform, new Vector2(xMin - 0.02f, 0.04f), new Vector2(xMax + 0.02f, 0.17f));
        }
    }

    private string GetShortTopicLabel(string topic)
    {
        switch (topic)
        {
            case "Heart": return "Heart";
            case "Brain": return "Brain";
            case "Lungs": return "Lungs";
            case "Pendulum": return "Pend.";
            case "Water": return "H2O";
            case "Geometry": return "Geo";
            default: return topic;
        }
    }

    private string GetFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Student";

        string[] parts = fullName.Trim().Split(' ');
        return parts.Length > 0 ? parts[0] : "Student";
    }

}
