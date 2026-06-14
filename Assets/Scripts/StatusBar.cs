using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class StatusBar : MonoBehaviour
{
    [Header("Layout")]
    public float segmentWidth  = 28f;
    public float segmentHeight = 18f;
    public float segmentGap    = 3f;
    public float slashWidth    = 3f;
    public float outlineWidth  = 2f;

    [Header("Colors")]
    public Color normalFill    = new Color(0.18f, 0.85f, 0.28f);
    public Color overhealFill  = new Color(0.20f, 0.75f, 1.00f);
    public Color emptyFill     = new Color(0.12f, 0.12f, 0.14f);
    public Color outlineColor  = new Color(0.60f, 0.65f, 0.60f);
    public Color flashGain     = new Color(0.30f, 1.00f, 0.40f);
    public Color flashLoss     = new Color(1.00f, 0.18f, 0.18f);

    [Header("Flash")]
    public float flashDuration = 0.35f;
    public int   flashPulses   = 2;

    private int baseMax      = 5;
    private int currentValue = 0;

    private RectTransform rt;
    private GameObject    container;

    private List<Image>      segFills        = new List<Image>();
    private List<GameObject> borderObjects   = new List<GameObject>();
    private List<Image>      slashes         = new List<Image>();
    private List<Image>      flashOverlays   = new List<Image>();

    private Coroutine flashRoutine;

    // ── Public API ────────────────────────────────────────────────────────

    public void Init(int baseMaxSegments, int initialValue)
    {
        baseMax      = baseMaxSegments;
        currentValue = initialValue;
        BuildBar();
    }

    public void SetValue(int newValue, int newBaseMax = -1)
    {
        bool gained  = newValue > currentValue;
        bool changed = newValue != currentValue;

        if (newBaseMax >= 0 && newBaseMax != baseMax)
        {
            baseMax = newBaseMax;
            changed = true;
        }

        int oldSegCount = VisibleSegCount(currentValue);
        int newSegCount = VisibleSegCount(newValue);

        currentValue = newValue;

        if (newSegCount != oldSegCount)
            RebuildSegments();
        else
            RefreshColors();

        if (changed)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashOverlays(gained ? flashGain : flashLoss));
        }
    }

    int VisibleSegCount(int value)
    {
        return value > baseMax ? value : baseMax;
    }

    // ── Build ─────────────────────────────────────────────────────────────

    void BuildBar()
    {
        rt = GetComponent<RectTransform>();

        if (container != null) Destroy(container);
        container = new GameObject("BarContainer");
        container.transform.SetParent(transform, false);
        RectTransform crt = container.AddComponent<RectTransform>();
        crt.anchorMin = Vector2.zero;
        crt.anchorMax = Vector2.one;
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;

        // No full-width background — each segment provides its own background
        // so no dark bar bleeds through the gaps between segments

        RebuildSegments();
    }

    void RebuildSegments()
    {
        foreach (var f in segFills)      if (f)  Destroy(f.gameObject);
        foreach (var b in borderObjects) if (b)  Destroy(b);
        foreach (var s in slashes)       if (s)  Destroy(s.gameObject);
        foreach (var o in flashOverlays) if (o)  Destroy(o.gameObject);
        segFills.Clear();
        borderObjects.Clear();
        slashes.Clear();
        flashOverlays.Clear();

        int   totalSegs = VisibleSegCount(currentValue);
        float totalW    = totalSegs * segmentWidth + Mathf.Max(0, totalSegs - 1) * segmentGap;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalW + outlineWidth * 2f);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   segmentHeight + outlineWidth * 2f);

        for (int i = 0; i < totalSegs; i++)
        {
            float x = outlineWidth + i * (segmentWidth + segmentGap);
            float y = outlineWidth;

            // ── Segment fill ──────────────────────────────────────────────
            GameObject fill  = CreateRect("Seg_" + i, container.transform);
            Image fillImg    = fill.AddComponent<Image>();
            PlaceRect(fill, x, y, segmentWidth, segmentHeight);
            segFills.Add(fillImg);

            // ── Per-segment outline (no vertical lines between segments) ──
            // Top
            SpawnBorderLine("BT_" + i, x, y + segmentHeight, segmentWidth, outlineWidth);
            // Bottom
            SpawnBorderLine("BB_" + i, x, y - outlineWidth,  segmentWidth, outlineWidth);
            // Left — first segment only
            if (i == 0)
                SpawnBorderLine("BL", x - outlineWidth, y - outlineWidth, outlineWidth, segmentHeight + outlineWidth * 2f);
            // Right — last segment only
            if (i == totalSegs - 1)
                SpawnBorderLine("BR", x + segmentWidth, y - outlineWidth, outlineWidth, segmentHeight + outlineWidth * 2f);

            // ── Flash overlay per segment (so gaps don't flash) ───────────
            GameObject flashObj = CreateRect("Flash_" + i, container.transform);
            Image flashImg      = flashObj.AddComponent<Image>();
            flashImg.color      = Color.clear;
            PlaceRect(flashObj, x, y, segmentWidth, segmentHeight);
            flashOverlays.Add(flashImg);

            // ── Diagonal slash separator ──────────────────────────────────
            if (i < totalSegs - 1)
            {
                float slashCX = x + segmentWidth + segmentGap * 0.5f;
                float slashCY = y + segmentHeight * 0.5f;

                GameObject slash = CreateRect("Slash_" + i, container.transform);
                Image slashImg   = slash.AddComponent<Image>();
                slashImg.color   = outlineColor;

                RectTransform sr    = slash.GetComponent<RectTransform>();
                sr.anchorMin        = Vector2.zero;
                sr.anchorMax        = Vector2.zero;
                sr.pivot            = new Vector2(0.5f, 0.5f);
                sr.anchoredPosition = new Vector2(slashCX, slashCY);
                sr.sizeDelta        = new Vector2(slashWidth, segmentHeight + outlineWidth * 2f + 2f);
                slash.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                slashes.Add(slashImg);
            }
        }

        RefreshColors();
    }

    void SpawnBorderLine(string name, float x, float y, float w, float h)
    {
        GameObject go = CreateRect(name, container.transform);
        Image img     = go.AddComponent<Image>();
        img.color     = outlineColor;
        PlaceRect(go, x, y, w, h);
        borderObjects.Add(go);
    }

    // ── Colors ─────────────────────────────────────────────────────────────

    void RefreshColors()
    {
        for (int i = 0; i < segFills.Count; i++)
        {
            if (segFills[i] == null) continue;
            if (i < currentValue)
                segFills[i].color = i < baseMax ? normalFill : overhealFill;
            else
                segFills[i].color = emptyFill;
        }
    }

    // ── Flash ──────────────────────────────────────────────────────────────

    IEnumerator FlashOverlays(Color flashColor)
    {
        for (int p = 0; p < flashPulses; p++)
        {
            float half = flashDuration / (flashPulses * 2f);
            float t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                Color c = Color.Lerp(Color.clear, flashColor, t / half);
                foreach (var o in flashOverlays) if (o) o.color = c;
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                Color c = Color.Lerp(flashColor, Color.clear, t / half);
                foreach (var o in flashOverlays) if (o) o.color = c;
                yield return null;
            }
        }
        foreach (var o in flashOverlays) if (o) o.color = Color.clear;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    GameObject CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    void PlaceRect(GameObject go, float x, float y, float w, float h)
    {
        RectTransform r     = go.GetComponent<RectTransform>();
        r.anchorMin         = Vector2.zero;
        r.anchorMax         = Vector2.zero;
        r.pivot             = Vector2.zero;
        r.anchoredPosition  = new Vector2(x, y);
        r.sizeDelta         = new Vector2(w, h);
    }
}