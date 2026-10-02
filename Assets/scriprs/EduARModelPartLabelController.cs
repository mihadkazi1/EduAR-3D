using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates readable screen-space callout labels for the educational AR models.
/// The labels follow the model on screen as it rotates/zooms and are shown only
/// while the matching image target is being tracked.
/// </summary>
public class EduARModelPartLabelController : MonoBehaviour
{
    public enum ModelType
    {
        Heart,
        Brain,
        Lungs,
        Pendulum,
        Water
    }

    private class LabelDefinition
    {
        public string name;
        public Vector3 normalizedAnchor;
        public Vector2 screenOffset;

        public LabelDefinition(string label, Vector3 anchor, Vector2 offset)
        {
            name = label;
            normalizedAnchor = anchor;
            screenOffset = offset;
        }
    }

    private class LabelUI
    {
        public LabelDefinition definition;
        public RectTransform root;
        public RectTransform line;
        public RectTransform dot;
        public RectTransform card;
        public TMP_Text text;
    }

    private Transform model;
    private Canvas canvas;
    private Transform uiParent;
    private RectTransform uiParentRect;
    private Camera uiCamera;
    private ModelType modelType;
    private Color accent = Color.white;
    private readonly List<LabelUI> labels = new List<LabelUI>();
    private Bounds localBounds;
    private bool visible;
    private bool configured;
    private Sprite circleSprite;

    // Only one model label set is allowed to be visible at a time. This
    // prevents stale labels from a previously tracked lesson appearing
    // when the camera switches from one target to another.
    private static EduARModelPartLabelController activeController;

    public void Configure(
        Transform targetModel,
        Canvas targetCanvas,
        Transform targetUiParent,
        ModelType type,
        Color accentColor)
    {
        model = targetModel;
        canvas = targetCanvas;
        uiParent = targetUiParent;
        uiParentRect = uiParent as RectTransform;
        modelType = type;
        accent = accentColor;
        configured = model != null && canvas != null && uiParentRect != null;

        ClearLabels();

        if (!configured)
            return;

        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RecalculateBounds();
        BuildLabels();
        SetVisible(false);
    }

    private void LateUpdate()
    {
        if (!configured || model == null || canvas == null)
            return;

        if (!visible || !model.gameObject.activeInHierarchy)
        {
            SetLabelRootsActive(false);
            return;
        }

        SetLabelRootsActive(true);
        UpdateLabelPositions();
    }

    public void SetVisible(bool value)
    {
        if (value && configured && model != null && model.gameObject.activeInHierarchy)
        {
            if (activeController != null && activeController != this)
            {
                activeController.visible = false;
                activeController.SetLabelRootsActive(false);
            }

            activeController = this;
        }

        visible = value;

        if (!value && activeController == this)
            activeController = null;

        SetLabelRootsActive(
            value &&
            configured &&
            model != null &&
            model.gameObject.activeInHierarchy &&
            activeController == this
        );
    }

    private void RecalculateBounds()
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
        {
            localBounds = new Bounds(Vector3.zero, Vector3.one);
            return;
        }

        bool initialized = false;
        Bounds result = new Bounds();

        for (int i = 0; i < renderers.Length; i++)
        {
            Bounds worldBounds = renderers[i].bounds;
            Vector3[] corners = GetBoundsCorners(worldBounds);

            for (int c = 0; c < corners.Length; c++)
            {
                Vector3 localPoint = model.InverseTransformPoint(corners[c]);

                if (!initialized)
                {
                    result = new Bounds(localPoint, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    result.Encapsulate(localPoint);
                }
            }
        }

        localBounds = initialized ? result : new Bounds(Vector3.zero, Vector3.one);
    }

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        return new[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };
    }

    private void BuildLabels()
    {
        List<LabelDefinition> definitions = GetDefinitions(modelType);

        for (int i = 0; i < definitions.Count; i++)
        {
            LabelDefinition definition = definitions[i];

            GameObject rootObject = new GameObject(
                "ARLabel_" + definition.name.Replace(" ", "")
            );
            rootObject.transform.SetParent(uiParentRect, false);

            RectTransform root = rootObject.AddComponent<RectTransform>();
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(220f, 50f);

            // Connecting line.
            GameObject lineObject = new GameObject("Line");
            lineObject.transform.SetParent(rootObject.transform, false);
            Image lineImage = lineObject.AddComponent<Image>();
            lineImage.color = new Color(accent.r, accent.g, accent.b, 0.62f);
            lineImage.raycastTarget = false;
            RectTransform line = lineImage.rectTransform;
            line.anchorMin = new Vector2(0.5f, 0.5f);
            line.anchorMax = new Vector2(0.5f, 0.5f);
            line.pivot = new Vector2(0f, 0.5f);
            line.sizeDelta = new Vector2(80f, 2f);

            // Anchor dot.
            GameObject dotObject = new GameObject("Anchor");
            dotObject.transform.SetParent(rootObject.transform, false);
            Image dotImage = dotObject.AddComponent<Image>();
            dotImage.sprite = GetCircleSprite();
            dotImage.color = accent;
            dotImage.raycastTarget = false;
            RectTransform dot = dotImage.rectTransform;
            dot.anchorMin = new Vector2(0.5f, 0.5f);
            dot.anchorMax = new Vector2(0.5f, 0.5f);
            dot.pivot = new Vector2(0.5f, 0.5f);
            dot.sizeDelta = new Vector2(9f, 9f);

            // Readable label card. IMPORTANT: keep Image and TMP text on
            // different GameObjects. Unity UI allows only one Graphic
            // component per GameObject.
            GameObject cardObject = new GameObject("Card");
            cardObject.transform.SetParent(rootObject.transform, false);
            Image cardImage = cardObject.AddComponent<Image>();
            cardImage.color = new Color(0.035f, 0.055f, 0.09f, 0.94f);
            cardImage.raycastTarget = false;
            RectTransform card = cardImage.rectTransform;
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);

            float width = Mathf.Clamp(34f + definition.name.Length * 8.5f, 106f, 190f);
            card.sizeDelta = new Vector2(width, 34f);

            GameObject textObject = new GameObject("LabelText");
            textObject.transform.SetParent(cardObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(7f, 2f);
            textRect.offsetMax = new Vector2(-7f, -2f);

            TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = definition.name;
            text.fontSize = 18f;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Truncate;

            // Keep labels above the AR background but below the main HUD.
            // The AR screen background is sibling 0.
            root.SetSiblingIndex(1);

            labels.Add(new LabelUI
            {
                definition = definition,
                root = root,
                line = line,
                dot = dot,
                card = card,
                text = text
            });
        }
    }

    private void UpdateLabelPositions()
    {
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;

        float canvasWidth = ((RectTransform)canvas.transform).rect.width;
        float canvasHeight = ((RectTransform)canvas.transform).rect.height;
        float padding = 18f;

        for (int i = 0; i < labels.Count; i++)
        {
            LabelUI label = labels[i];
            Vector3 localAnchor = center + Vector3.Scale(
                extents,
                label.definition.normalizedAnchor
            );

            Vector3 worldAnchor = model.TransformPoint(localAnchor);

            Camera screenCamera = Camera.main;
            if (screenCamera == null)
                screenCamera = UnityEngine.Object.FindObjectOfType<Camera>();

            if (screenCamera == null)
                continue;

            Vector3 screenPoint = screenCamera.WorldToScreenPoint(worldAnchor);

            if (screenPoint.z <= 0f)
            {
                label.root.gameObject.SetActive(false);
                continue;
            }

            label.root.gameObject.SetActive(true);

            RectTransform canvasRect = uiParentRect;
            Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (uiCamera != null ? uiCamera : Camera.main);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPoint,
                    eventCamera,
                    out Vector2 anchorLocal))
            {
                continue;
            }

            Vector2 labelLocal = anchorLocal + label.definition.screenOffset;
            labelLocal.x = Mathf.Clamp(
                labelLocal.x,
                -canvasWidth * 0.5f + padding,
                canvasWidth * 0.5f - padding
            );
            labelLocal.y = Mathf.Clamp(
                labelLocal.y,
                -canvasHeight * 0.5f + 34f,
                canvasHeight * 0.5f - 34f
            );

            label.dot.anchoredPosition = anchorLocal;
            label.card.anchoredPosition = labelLocal;

            Vector2 delta = labelLocal - anchorLocal;
            float distance = delta.magnitude;

            label.line.anchoredPosition = anchorLocal;
            label.line.sizeDelta = new Vector2(Mathf.Max(18f, distance), 2f);
            label.line.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
            );
        }
    }

    private void SetLabelRootsActive(bool active)
    {
        for (int i = 0; i < labels.Count; i++)
        {
            if (labels[i].root != null)
                labels[i].root.gameObject.SetActive(active);
        }
    }

    private List<LabelDefinition> GetDefinitions(ModelType type)
    {
        switch (type)
        {
            case ModelType.Heart:
                return new List<LabelDefinition>
                {
                    new LabelDefinition("Aorta", new Vector3(0.10f, 0.95f, 0.05f), new Vector2(78f, 35f)),
                    new LabelDefinition("Right Atrium", new Vector3(-0.72f, 0.50f, 0.12f), new Vector2(-85f, 22f)),
                    new LabelDefinition("Left Atrium", new Vector3(0.72f, 0.52f, 0.12f), new Vector2(88f, 24f)),
                    new LabelDefinition("Right Ventricle", new Vector3(-0.60f, -0.58f, 0.10f), new Vector2(-100f, -30f)),
                    new LabelDefinition("Left Ventricle", new Vector3(0.60f, -0.58f, 0.10f), new Vector2(104f, -32f)),
                    new LabelDefinition("Septum", new Vector3(0.05f, -0.08f, 0.78f), new Vector2(0f, -72f))
                };

            case ModelType.Brain:
                return new List<LabelDefinition>
                {
                    new LabelDefinition("Cerebrum", new Vector3(0f, 0.68f, 0.15f), new Vector2(0f, 66f)),
                    new LabelDefinition("Frontal Lobe", new Vector3(0.72f, 0.38f, 0.08f), new Vector2(96f, 32f)),
                    new LabelDefinition("Cerebellum", new Vector3(-0.56f, -0.56f, -0.15f), new Vector2(-100f, -32f)),
                    new LabelDefinition("Brainstem", new Vector3(0f, -0.82f, 0.12f), new Vector2(0f, -66f)),
                    new LabelDefinition("Temporal Lobe", new Vector3(-0.72f, 0.02f, 0.10f), new Vector2(-105f, 0f))
                };

            case ModelType.Lungs:
                return new List<LabelDefinition>
                {
                    new LabelDefinition("Trachea", new Vector3(0f, 0.94f, 0.18f), new Vector2(0f, 66f)),
                    new LabelDefinition("Right Lung", new Vector3(-0.75f, 0.05f, 0.12f), new Vector2(-95f, 0f)),
                    new LabelDefinition("Left Lung", new Vector3(0.75f, 0.05f, 0.12f), new Vector2(95f, 0f)),
                    new LabelDefinition("Bronchi", new Vector3(0f, 0.28f, 0.64f), new Vector2(0f, 70f)),
                    new LabelDefinition("Alveoli", new Vector3(0.48f, -0.58f, 0.35f), new Vector2(95f, -35f))
                };

            case ModelType.Pendulum:
                return new List<LabelDefinition>
                {
                    new LabelDefinition("Pivot", new Vector3(0f, 0.95f, 0f), new Vector2(-88f, 20f)),
                    new LabelDefinition("String", new Vector3(0f, 0.15f, 0f), new Vector2(88f, 0f)),
                    new LabelDefinition("Bob", new Vector3(0f, -0.88f, 0f), new Vector2(88f, -20f))
                };

            case ModelType.Water:
                return new List<LabelDefinition>
                {
                    new LabelDefinition("Oxygen (O)", new Vector3(0f, 0.40f, 0.30f), new Vector2(-98f, 45f)),
                    new LabelDefinition("Hydrogen (H1)", new Vector3(-0.80f, -0.35f, 0.10f), new Vector2(-96f, -35f)),
                    new LabelDefinition("Hydrogen (H2)", new Vector3(0.80f, -0.35f, 0.10f), new Vector2(96f, -35f))
                };

            default:
                return new List<LabelDefinition>();
        }
    }

    private Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 32;
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
                texture.SetPixel(
                    x,
                    y,
                    distance <= radius ? Color.white : Color.clear
                );
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

    private void OnDestroy()
    {
        if (activeController == this)
            activeController = null;
    }

    private void ClearLabels()
    {
        for (int i = 0; i < labels.Count; i++)
        {
            if (labels[i].root != null)
                Destroy(labels[i].root.gameObject);
        }
        labels.Clear();
    }
}
