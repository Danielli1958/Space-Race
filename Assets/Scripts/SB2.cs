// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// using System.Collections.Generic;

// [RequireComponent(typeof(RectTransform))]
// public class StatusBar : MonoBehaviour
// {
//     [Header("Layout")]
//     public float segmentWidth  = 28f;
//     public float segmentHeight = 18f;
//     public float separatorWidth = 2f;   // Width of the vertical line between segments
//     public float outlineWidth  = 2f;

//     [Header("Colors")]
//     public Color normalFill    = new Color(0.18f, 0.85f, 0.28f);
//     public Color overhealFill  = new Color(0.20f, 0.75f, 1.00f);
//     public Color emptyFill     = new Color(0.12f, 0.12f, 0.14f);
//     public Color outlineColor  = new Color(0.55f, 0.60f, 0.55f);
//     public Color separatorColor = new Color(0.55f, 0.60f, 0.55f);
//     public Color flashGain     = new Color(0.30f, 1.00f, 0.40f);
//     public Color flashLoss     = new Color(1.00f, 0.18f, 0.18f);

//     [Header("Flash")]
//     public float flashDuration = 0.35f;
//     public int   flashPulses   = 2;

//     private int baseMax      = 5;
//     private int currentValue = 0;

//     private RectTransform    rt;
//     private GameObject       container;
//     private List<Image>      segFills      = new List<Image>();
//     private List<Image>      flashOverlays = new List<Image>();
//     private List<GameObject> allObjects    = new List<GameObject>();
//     private Coroutine        flashRoutine;

//     // ── Public API ────────────────────────────────────────────────────────

//     public void Init(int baseMaxSegments, int initialValue)
//     {
//         baseMax      = baseMaxSegments;
//         currentValue = initialValue;
//         Build();
//     }

//     public void SetValue(int newValue, int newBaseMax = -1)
//     {
//         bool gained  = newValue > currentValue;
//         bool changed = newValue != currentValue;

//         if (newBaseMax >= 0 && newBaseMax != baseMax)
//         {
//             baseMax = newBaseMax;
//             changed = true;
//         }

//         int oldCount = VisibleCount(currentValue);
//         int newCount = VisibleCount(newValue);
//         currentValue = newValue;

//         if (newCount != oldCount)
//             Rebuild();
//         else
//             RefreshColors();

//         if (changed)
//         {
//             if (flashRoutine != null) StopCoroutine(flashRoutine);
//             flashRoutine = StartCoroutine(Flash(gained ? flashGain : flashLoss));
//         }
//     }

//     int VisibleCount(int v) => v > baseMax ? v : baseMax;

//     // ── Build ─────────────────────────────────────────────────────────────

//     void Build()
//     {
//         rt           = GetComponent<RectTransform>();
//         rt.anchorMin = new Vector2(0, 1);
//         rt.anchorMax = new Vector2(0, 1);
//         rt.pivot     = new Vector2(0, 1);

//         if (container != null) Destroy(container);
//         container = new GameObject("BarContainer");
//         container.transform.SetParent(transform, false);
//         var crt       = container.AddComponent<RectTransform>();
//         crt.anchorMin = Vector2.zero;
//         crt.anchorMax = Vector2.one;
//         crt.offsetMin = Vector2.zero;
//         crt.offsetMax = Vector2.zero;

//         Rebuild();
//     }

//     void Rebuild()
//     {
//         foreach (var o in allObjects) if (o) Destroy(o);
//         allObjects.Clear();
//         segFills.Clear();
//         flashOverlays.Clear();

//         int   n      = VisibleCount(currentValue);

//         // Total width: segments + separators between them + outline on both sides
//         float totalW = outlineWidth * 2f
//                      + n * segmentWidth
//                      + (n - 1) * separatorWidth;
//         float totalH = segmentHeight + outlineWidth * 2f;

//         rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalW);
//         rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   totalH);

//         // ── Outer border (4 lines) ──────────────────────────────────────
//         // Top
//         Spawn("BTop",    0,            totalH - outlineWidth, totalW,        outlineWidth, outlineColor);
//         // Bottom
//         Spawn("BBot",    0,            0,                     totalW,        outlineWidth, outlineColor);
//         // Left
//         Spawn("BLeft",   0,            0,                     outlineWidth,  totalH,       outlineColor);
//         // Right
//         Spawn("BRight",  totalW - outlineWidth, 0,            outlineWidth,  totalH,       outlineColor);

//         // ── Segments and separators ─────────────────────────────────────
//         for (int i = 0; i < n; i++)
//         {
//             // X position: after left outline + i segments + i separators
//             float x = outlineWidth + i * (segmentWidth + separatorWidth);
//             float y = outlineWidth;

//             // Fill
//             GameObject fillGO = Spawn("Fill_" + i, x, y, segmentWidth, segmentHeight, Color.white);
//             segFills.Add(fillGO.GetComponent<Image>());

//             // Flash overlay sits on top of fill
//             GameObject flashGO = Spawn("Flash_" + i, x, y, segmentWidth, segmentHeight, Color.clear);
//             flashOverlays.Add(flashGO.GetComponent<Image>());

//             // Vertical separator after every segment except the last
//             if (i < n - 1)
//             {
//                 float sepX = x + segmentWidth;
//                 Spawn("Sep_" + i, sepX, y, separatorWidth, segmentHeight, separatorColor);
//             }
//         }

//         RefreshColors();
//     }

//     GameObject Spawn(string name, float x, float y, float w, float h, Color color)
//     {
//         var go        = new GameObject(name);
//         go.transform.SetParent(container.transform, false);
//         allObjects.Add(go);

//         var img       = go.AddComponent<Image>();
//         img.color     = color;
//         img.raycastTarget = false;

//         var r              = go.AddComponent<RectTransform>();
//         r.anchorMin        = Vector2.zero;
//         r.anchorMax        = Vector2.zero;
//         r.pivot            = Vector2.zero;
//         r.anchoredPosition = new Vector2(x, y);
//         r.sizeDelta        = new Vector2(w, h);

//         return go;
//     }

//     // ── Colors ─────────────────────────────────────────────────────────────

//     void RefreshColors()
//     {
//         for (int i = 0; i < segFills.Count; i++)
//         {
//             if (segFills[i] == null) continue;
//             segFills[i].color = i < currentValue
//                 ? (i < baseMax ? normalFill : overhealFill)
//                 : emptyFill;
//         }
//     }

//     // ── Flash ──────────────────────────────────────────────────────────────

//     IEnumerator Flash(Color flashColor)
//     {
//         for (int p = 0; p < flashPulses; p++)
//         {
//             float half = flashDuration / (flashPulses * 2f);
//             for (float t = 0; t < half; t += Time.deltaTime)
//             {
//                 Color c = Color.Lerp(Color.clear, flashColor, t / half);
//                 foreach (var o in flashOverlays) if (o) o.color = c;
//                 yield return null;
//             }
//             for (float t = 0; t < half; t += Time.deltaTime)
//             {
//                 Color c = Color.Lerp(flashColor, Color.clear, t / half);
//                 foreach (var o in flashOverlays) if (o) o.color = c;
//                 yield return null;
//             }
//         }
//         foreach (var o in flashOverlays) if (o) o.color = Color.clear;
//     }
// }