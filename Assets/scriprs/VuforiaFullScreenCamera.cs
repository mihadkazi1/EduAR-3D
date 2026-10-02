using System.Collections;
using UnityEngine;
using Vuforia;

public class VuforiaFullScreenCamera : MonoBehaviour
{
    private Camera arCamera;

    //    private void Awake()
    //    {
    //        arCamera = GetComponent<Camera>();

    //        if (arCamera == null)
    //            arCamera = Camera.main;

    //#if UNITY_ANDROID && !UNITY_EDITOR
    //        // Force portrait before Vuforia starts rendering.
    //        Screen.orientation = ScreenOrientation.Portrait;

    //        Screen.autorotateToPortrait = true;
    //        Screen.autorotateToPortraitUpsideDown = false;
    //        Screen.autorotateToLandscapeLeft = false;
    //        Screen.autorotateToLandscapeRight = false;

    //        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    //        Screen.fullScreen = true;
    //#endif
    //    }

    private void Awake()
    {
        arCamera = GetComponent<Camera>();

        if (arCamera == null)
            arCamera = GetComponentInChildren<Camera>();

        if (arCamera == null)
            arCamera = Camera.main;

#if UNITY_ANDROID && !UNITY_EDITOR
    Screen.orientation = ScreenOrientation.Portrait;

    Screen.autorotateToPortrait = true;
    Screen.autorotateToPortraitUpsideDown = false;
    Screen.autorotateToLandscapeLeft = false;
    Screen.autorotateToLandscapeRight = false;

    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    Screen.fullScreen = true;
#endif
    }

    private void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;

        StartCoroutine(InitialFix());
    }

    private void OnDestroy()
    {
        if (VuforiaApplication.Instance != null)
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
    }

    private void OnVuforiaStarted()
    {
        Debug.Log("[EDUAR] Vuforia started.");

        ForceCameraFullScreen();

        StartCoroutine(ReapplyCameraLayout());
    }

    private IEnumerator InitialFix()
    {
        yield return null;

        ForceCameraFullScreen();

        yield return new WaitForEndOfFrame();

        ForceCameraFullScreen();

        yield return new WaitForSecondsRealtime(0.2f);

        ForceCameraFullScreen();

        yield return new WaitForSecondsRealtime(0.5f);

        ForceCameraFullScreen();

        PrintViewportInfo();
    }

    private IEnumerator ReapplyCameraLayout()
    {
        yield return null;
        ForceCameraFullScreen();

        yield return new WaitForEndOfFrame();
        ForceCameraFullScreen();

        yield return new WaitForSecondsRealtime(0.2f);
        ForceCameraFullScreen();

        yield return new WaitForSecondsRealtime(0.5f);
        ForceCameraFullScreen();

        PrintViewportInfo();
    }

    private void ForceCameraFullScreen()
    {
        if (arCamera == null)
            return;

        // Unity Camera viewport = entire Unity render view.
        arCamera.rect = new Rect(0f, 0f, 1f, 1f);

        // Explicit pixel viewport.
        arCamera.pixelRect = new Rect(
            0,
            0,
            Screen.width,
            Screen.height
        );

        arCamera.targetDisplay = 0;
    }

    private void PrintViewportInfo()
    {
        Debug.Log("========== EDUAR CAMERA DEBUG ==========");

        Debug.Log(
            "[EDUAR] Screen = " +
            Screen.width + " x " +
            Screen.height
        );

        Debug.Log(
            "[EDUAR] SafeArea = " +
            Screen.safeArea
        );

        Debug.Log(
            "[EDUAR] Camera.rect = " +
            arCamera.rect
        );

        Debug.Log(
            "[EDUAR] Camera.pixelRect = " +
            arCamera.pixelRect
        );

        Debug.Log(
            "[EDUAR] Camera.aspect = " +
            arCamera.aspect
        );

        if (VuforiaApplication.Instance != null &&
            VuforiaApplication.Instance.IsRunning &&
            VuforiaBehaviour.Instance != null)
        {
            Rect videoRect =
                VuforiaBehaviour.Instance.CameraDevice
                    .GetVideoBackgroundRectInViewPort();

            Debug.Log(
                "[EDUAR] Vuforia Video Background Rect = " +
                videoRect
            );

            VideoModeData mode =
                VuforiaBehaviour.Instance.CameraDevice.GetVideoMode();

            Debug.Log(
                "[EDUAR] Vuforia Video Mode = " +
                mode
            );
        }

        Debug.Log("========================================");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            return;

        ForceCameraFullScreen();

        StartCoroutine(ReapplyCameraLayout());
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            return;

        ForceCameraFullScreen();

        StartCoroutine(ReapplyCameraLayout());
    }
}