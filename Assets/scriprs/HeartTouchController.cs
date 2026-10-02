using UnityEngine;
using UnityEngine.InputSystem;

public class HeartTouchController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.22f;
    [SerializeField] private float maxPitch = 65f;

    [Header("Zoom")]
    [SerializeField] private float minZoomMultiplier = 0.60f;
    [SerializeField] private float maxZoomMultiplier = 2.00f;
    [SerializeField] private float zoomSensitivity = 1.00f;

    private Transform interactionPivot;
    private Transform originalParent;

    private Vector3 originalPivotLocalPosition;
    private Quaternion originalPivotLocalRotation;
    private Vector3 originalModelLocalPosition;
    private Quaternion originalModelLocalRotation;
    private Vector3 originalModelLocalScale;

    private float yaw;
    private float pitch;
    private int previousTouchCount;
    private float previousPinchDistance;
    private bool initialized;

    private void Start()
    {
        BuildInteractionPivot();
    }

    private void Update()
    {
        if (!initialized || Touchscreen.current == null)
            return;

        HandleTouch();
    }

    private void BuildInteractionPivot()
    {
        if (initialized)
            return;

        originalParent = transform.parent;

        Vector3 center = CalculateWorldBoundsCenter();

        GameObject pivotObject = new GameObject(
            transform.name + "_InteractionPivot"
        );

        interactionPivot = pivotObject.transform;
        interactionPivot.SetParent(originalParent, true);
        interactionPivot.position = center;
        interactionPivot.rotation = transform.rotation;

        // Keep the model exactly where it was visually.
        transform.SetParent(interactionPivot, true);

        originalPivotLocalPosition = interactionPivot.localPosition;
        originalPivotLocalRotation = interactionPivot.localRotation;
        originalModelLocalPosition = transform.localPosition;
        originalModelLocalRotation = transform.localRotation;
        originalModelLocalScale = transform.localScale;

        yaw = 0f;
        pitch = 0f;
        previousTouchCount = 0;
        previousPinchDistance = 0f;

        initialized = true;
    }

    private Vector3 CalculateWorldBoundsCenter()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
            return transform.position;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.center;
    }

    private void HandleTouch()
    {
        int firstIndex = -1;
        int secondIndex = -1;
        int touchCount = 0;

        for (int i = 0; i < Touchscreen.current.touches.Count; i++)
        {
            var touch = Touchscreen.current.touches[i];

            if (!touch.press.isPressed)
                continue;

            if (firstIndex == -1)
                firstIndex = i;
            else if (secondIndex == -1)
                secondIndex = i;

            touchCount++;
        }

        if (touchCount == 1 && firstIndex >= 0)
        {
            if (previousTouchCount == 1)
            {
                Vector2 delta =
                    Touchscreen.current.touches[firstIndex].delta.ReadValue();

                if (delta.sqrMagnitude > 0.01f)
                {
                    yaw += delta.x * rotationSpeed;
                    pitch -= delta.y * rotationSpeed;
                    pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

                    interactionPivot.localRotation =
                        originalPivotLocalRotation *
                        Quaternion.Euler(pitch, yaw, 0f);
                }
            }
        }
        else if (touchCount >= 2 && firstIndex >= 0 && secondIndex >= 0)
        {
            var touch0 = Touchscreen.current.touches[firstIndex];
            var touch1 = Touchscreen.current.touches[secondIndex];

            Vector2 current0 = touch0.position.ReadValue();
            Vector2 current1 = touch1.position.ReadValue();

            float currentDistance =
                Vector2.Distance(current0, current1);

            if (previousTouchCount < 2 || previousPinchDistance <= 0.1f)
            {
                previousPinchDistance = currentDistance;
            }
            else if (currentDistance > 0.1f)
            {
                float ratio =
                    currentDistance / previousPinchDistance;

                float currentMultiplier =
                    GetCurrentZoomMultiplier();

                float newMultiplier =
                    currentMultiplier *
                    Mathf.Pow(ratio, zoomSensitivity);

                newMultiplier = Mathf.Clamp(
                    newMultiplier,
                    minZoomMultiplier,
                    maxZoomMultiplier
                );

                transform.localScale =
                    originalModelLocalScale * newMultiplier;

                previousPinchDistance = currentDistance;
            }
        }
        else
        {
            previousPinchDistance = 0f;
        }

        previousTouchCount = touchCount;
    }

    private float GetCurrentZoomMultiplier()
    {
        float baseX = Mathf.Abs(originalModelLocalScale.x);

        if (baseX < 0.0001f)
            return 1f;

        return Mathf.Abs(transform.localScale.x) / baseX;
    }

    public void ResetModel()
    {
        if (!initialized)
            return;

        interactionPivot.localPosition = originalPivotLocalPosition;
        interactionPivot.localRotation = originalPivotLocalRotation;

        transform.localPosition = originalModelLocalPosition;
        transform.localRotation = originalModelLocalRotation;
        transform.localScale = originalModelLocalScale;

        yaw = 0f;
        pitch = 0f;
        previousTouchCount = 0;
        previousPinchDistance = 0f;
    }

    public void SetZoomMultiplier(float multiplier)
    {
        if (!initialized)
            return;

        multiplier = Mathf.Clamp(
            multiplier,
            minZoomMultiplier,
            maxZoomMultiplier
        );

        transform.localScale =
            originalModelLocalScale * multiplier;
    }
}
