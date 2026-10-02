using System.Collections;
using UnityEngine;
using Vuforia;

public class VuforiaFullscreenFix : MonoBehaviour
{
    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ApplyFullscreen();

        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        }

        StartCoroutine(ReapplyFullscreen());
#endif
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        }
#endif
    }

    private void OnVuforiaStarted()
    {
        ApplyFullscreen();
    }

    private IEnumerator ReapplyFullscreen()
    {
        // Important: permission/Vuforia can change the Android window
        yield return new WaitForSeconds(0.2f);
        ApplyFullscreen();

        yield return new WaitForSeconds(0.5f);
        ApplyFullscreen();

        yield return new WaitForSeconds(1.0f);
        ApplyFullscreen();

        yield return new WaitForSeconds(2.0f);
        ApplyFullscreen();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (hasFocus)
        {
            StartCoroutine(ApplyAfterFocus());
        }
#endif
    }

    private IEnumerator ApplyAfterFocus()
    {
        yield return new WaitForSeconds(0.2f);
        ApplyFullscreen();

        yield return new WaitForSeconds(0.5f);
        ApplyFullscreen();
    }

    private void ApplyFullscreen()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            using (AndroidJavaClass unityPlayer =
                   new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity =
                       unityPlayer.GetStatic<AndroidJavaObject>(
                           "currentActivity"))
                {
                    using (AndroidJavaObject window =
                           activity.Call<AndroidJavaObject>("getWindow"))
                    {
                        // Android 11 / API 30+
                        using (AndroidJavaClass version =
                               new AndroidJavaClass(
                                   "android.os.Build$VERSION"))
                        {
                            int sdk =
                                version.GetStatic<int>("SDK_INT");

                            if (sdk >= 30)
                            {
                                // IMPORTANT:
                                // Tell Android NOT to resize/reflow
                                // Unity content around system bars.
                                window.Call(
                                    "setDecorFitsSystemWindows",
                                    false
                                );

                                using (AndroidJavaObject decorView =
                                       window.Call<AndroidJavaObject>(
                                           "getDecorView"))
                                {
                                    using (AndroidJavaObject controller =
                                           window.Call<AndroidJavaObject>(
                                               "getInsetsController"))
                                    {
                                        if (controller != null)
                                        {
                                            using (AndroidJavaClass typeClass =
                                                   new AndroidJavaClass(
                                                       "android.view.WindowInsets$Type"))
                                            {
                                                int systemBars =
                                                    typeClass.CallStatic<int>(
                                                        "systemBars"
                                                    );

                                                // Hide status + navigation bars
                                                controller.Call(
                                                    "hide",
                                                    systemBars
                                                );

                                                // Allow temporary reveal
                                                // with edge swipe.
                                                controller.Call(
                                                    "setSystemBarsBehavior",
                                                    2
                                                );
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Older Android fallback
                                using (AndroidJavaObject decorView =
                                       window.Call<AndroidJavaObject>(
                                           "getDecorView"))
                                {
                                    int flags =
                                        0x00000002 | // FULLSCREEN
                                        0x00000004 | // HIDE_NAVIGATION
                                        0x00000100 | // LAYOUT_STABLE
                                        0x00000200 | // LAYOUT_HIDE_NAVIGATION
                                        0x00000400 | // LAYOUT_FULLSCREEN
                                        0x00001000;  // IMMERSIVE_STICKY

                                    decorView.Call(
                                        "setSystemUiVisibility",
                                        flags
                                    );
                                }
                            }
                        }
                    }
                }
            }

            // Unity-side fullscreen as additional protection
            Screen.fullScreen = true;
            Screen.fullScreenMode =
                FullScreenMode.FullScreenWindow;
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "VuforiaFullscreenFix: " + e.Message
            );
        }

#endif
    }
}