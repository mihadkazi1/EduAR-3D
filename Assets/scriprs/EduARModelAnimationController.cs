using UnityEngine;

/// <summary>
/// Code-driven educational animations for EduAR 3D.
/// The controller runs on a presentation pivot above the touch-interaction pivot,
/// so model rotation/zoom remains controlled by HeartTouchController.
/// </summary>
public class EduARModelAnimationController : MonoBehaviour
{
    public enum AnimationStyle
    {
        HeartBeat,
        BrainPulse,
        LungBreathing,
        PendulumSwing,
        WaterRotate
    }

    [SerializeField] private Transform animatedVisual;
    [SerializeField] private AnimationStyle style;

    private Vector3 baseScale = Vector3.one;
    private Vector3 basePosition = Vector3.zero;
    private Quaternion baseRotation = Quaternion.identity;
    private float elapsed;
    private bool isPlaying = true;
    private bool configured;

    public bool IsPlaying => isPlaying;

    public void Configure(Transform visual, AnimationStyle animationStyle)
    {
        animatedVisual = visual;
        style = animationStyle;

        baseScale = transform.localScale;
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
        elapsed = 0f;
        isPlaying = true;
        configured = animatedVisual != null;
    }

    private void LateUpdate()
    {
        if (!configured || animatedVisual == null)
            return;

        if (!animatedVisual.gameObject.activeInHierarchy)
            return;

        if (!isPlaying)
            return;

        elapsed += Time.unscaledDeltaTime;

        switch (style)
        {
            case AnimationStyle.HeartBeat:
                ApplyHeartBeat();
                break;

            case AnimationStyle.BrainPulse:
                ApplyBrainPulse();
                break;

            case AnimationStyle.LungBreathing:
                ApplyLungBreathing();
                break;

            case AnimationStyle.PendulumSwing:
                ApplyPendulumSwing();
                break;

            case AnimationStyle.WaterRotate:
                ApplyWaterRotate();
                break;
        }
    }

    private void ApplyHeartBeat()
    {
        // Distinct lub-dub rhythm followed by a clear rest period.
        float phase = Mathf.Repeat(elapsed * 1.02f, 1f);
        float first = Mathf.Exp(-Mathf.Pow((phase - 0.13f) / 0.042f, 2f));
        float second = 0.60f * Mathf.Exp(-Mathf.Pow((phase - 0.30f) / 0.050f, 2f));
        float pulse = Mathf.Clamp01(first + second);

        float scale = 1f + pulse * 0.12f;
        transform.localScale = baseScale * scale;
        transform.localPosition = basePosition + Vector3.up * (pulse * 0.016f);
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, pulse * 1.4f);
    }

    private void ApplyBrainPulse()
    {
        // Slow, subtle cortical pulse. The movement is intentionally restrained
        // so the brain looks alive without appearing to wobble.
        float slow = Mathf.Repeat(elapsed * 0.42f, 1f);
        float pulse = Mathf.Sin(slow * Mathf.PI);
        float scale = 1f + pulse * 0.030f;

        transform.localScale = baseScale * scale;
        transform.localPosition = basePosition +
            new Vector3(
                Mathf.Sin(elapsed * 0.55f) * 0.0025f,
                pulse * 0.008f,
                Mathf.Cos(elapsed * 0.43f) * 0.0020f
            );

        transform.localRotation = baseRotation * Quaternion.Euler(
            Mathf.Sin(elapsed * 0.38f) * 1.8f,
            Mathf.Sin(elapsed * 0.24f) * 5.0f,
            Mathf.Cos(elapsed * 0.31f) * 1.4f
        );
    }

    private void ApplyLungBreathing()
    {
        // About one calm breathing cycle every ~4 seconds.
        // Expansion is stronger side-to-side than vertically, matching a chest
        // expansion impression while keeping the whole lung assembly together.
        const float breathingRate = 0.25f;
        float cycle = Mathf.Repeat(elapsed * breathingRate, 1f);
        float breath = 0.5f - 0.5f * Mathf.Cos(cycle * Mathf.PI * 2f);

        // Slight hold around full inhale for a more natural breathing rhythm.
        float inhale = Mathf.SmoothStep(0f, 1f, breath);
        float horizontal = 1f + inhale * 0.075f;
        float vertical = 1f + inhale * 0.035f;
        float depth = 1f + inhale * 0.055f;

        Vector3 breathingScale = baseScale;
        breathingScale.x *= horizontal;
        breathingScale.y *= vertical;
        breathingScale.z *= depth;
        transform.localScale = breathingScale;

        transform.localPosition = basePosition +
            Vector3.down * (Mathf.Sin(cycle * Mathf.PI) * 0.006f);

        transform.localRotation = baseRotation * Quaternion.Euler(
            Mathf.Sin(cycle * Mathf.PI * 2f) * 0.8f,
            0f,
            Mathf.Sin(cycle * Mathf.PI * 2f + 0.6f) * 0.7f
        );
    }

    private void ApplyPendulumSwing()
    {
        // Natural oscillation around the hinge pivot supplied by the AR setup.
        float angle = Mathf.Sin(elapsed * 1.35f) * 24f;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void ApplyWaterRotate()
    {
        float angle = elapsed * 28f;
        transform.localRotation = baseRotation * Quaternion.Euler(
            Mathf.Sin(elapsed * 0.85f) * 7f,
            angle,
            Mathf.Cos(elapsed * 0.70f) * 5f
        );
        transform.localPosition = basePosition +
            Vector3.up * (Mathf.Sin(elapsed * 1.05f) * 0.005f);
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Toggle()
    {
        isPlaying = !isPlaying;
    }

    public void ResetAnimation()
    {
        elapsed = 0f;
        transform.localScale = baseScale;
        transform.localPosition = basePosition;
        transform.localRotation = baseRotation;
    }
}
