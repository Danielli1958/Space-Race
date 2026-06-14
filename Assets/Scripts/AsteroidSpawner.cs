using UnityEngine;
using System.Collections.Generic;

public class AsteroidSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject asteroidPrefab;

    [Header("Spawn Settings")]
    public float spawnAheadDistance = 20f;
    public float patternSpacing = 10f;
    public float asteroidSize = 0.6f;
    public GameObject heartPrefab;
    [Range(0f, 1f)] public float heartSpawnChance = 0.2f;

    public GameObject ammoPrefab;
    [Range(0f, 1f)] public float ammoSpawnChance = 0.15f;

    [Header("Field Width")]
    public float fieldPadding = 0.5f;

    private Camera mainCamera;
    private float nextSpawnY;
    private float fieldLeft;
    private float fieldRight;
    private float fieldWidth;
    private float fieldCenterX;

    private enum Pattern
    {
        // Original 8
        DiagonalLine,
        AntiDiagonalLine,
        VShape,
        InvertedV,
        Gate,
        Cluster,
        ZigZag,
        Arc,
        // New 6
        DiagonalGate,
        XShape,
        CrossShape,
        WallOfVShapes,
        MultipleClusters,
        DiamondRing,
    }

    void Start()
    {
        mainCamera = Camera.main;
        UpdateFieldBounds();

        nextSpawnY = mainCamera.transform.position.y
                   + mainCamera.orthographicSize
                   + spawnAheadDistance;

        for (int i = 0; i < 6; i++)
        {
            float y = nextSpawnY;
            SpawnNextPattern();
            MaybeSpawnHeart(y);
            MaybeSpawnAmmo(y);
            nextSpawnY += patternSpacing;
        }
    }
    void MaybeSpawnAmmo(float y)
    {
        if (ammoPrefab == null) return;
        if (Random.value > ammoSpawnChance) return;

        // Offset X from hearts so they don't overlap
        float x = fieldLeft + fieldWidth * Random.Range(0.2f, 0.8f);
        Instantiate(ammoPrefab, new Vector3(x, y + patternSpacing * 0.3f, 0f), Quaternion.identity);
    }
    void MaybeSpawnHeart(float y)
    {
        // Debug: show why a heart may or may not spawn
        float roll = Random.value;

        if (heartPrefab == null) return;
        if (roll > heartSpawnChance) return;

        // Spawn at a random X position, vertically centred in the pattern gap
        float x = fieldLeft + fieldWidth * Random.Range(0.2f, 0.8f);
        Vector3 spawnPos = new Vector3(x, y + patternSpacing * 0.5f, 0f);
        Instantiate(heartPrefab, spawnPos, Quaternion.identity);
    }
    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        UpdateFieldBounds();

        float spawnThreshold = mainCamera.transform.position.y
                             + mainCamera.orthographicSize
                             + spawnAheadDistance;

        while (nextSpawnY < spawnThreshold)
        {
            float y = nextSpawnY;
            SpawnNextPattern();
            MaybeSpawnHeart(y);
            MaybeSpawnAmmo(y);
            nextSpawnY += patternSpacing;
        }
    }

    void UpdateFieldBounds()
    {
        float halfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        fieldLeft = mainCamera.transform.position.x - halfWidth + fieldPadding;
        fieldRight = mainCamera.transform.position.x + halfWidth - fieldPadding;
        fieldWidth = fieldRight - fieldLeft;
        fieldCenterX = (fieldLeft + fieldRight) / 2f;
    }

    void SpawnNextPattern()
    {
        Pattern pattern = (Pattern)Random.Range(0, System.Enum.GetValues(typeof(Pattern)).Length);
        float y = nextSpawnY;

        switch (pattern)
        {
            case Pattern.DiagonalLine: SpawnDiagonalLine(y, 1); break;
            case Pattern.AntiDiagonalLine: SpawnDiagonalLine(y, -1); break;
            case Pattern.VShape: SpawnVShape(y, false); break;
            case Pattern.InvertedV: SpawnVShape(y, true); break;
            case Pattern.Gate: SpawnGate(y); break;
            case Pattern.Cluster: SpawnCluster(y, fieldCenterX + Random.Range(-fieldWidth * 0.2f, fieldWidth * 0.2f)); break;
            case Pattern.ZigZag: SpawnZigZag(y); break;
            case Pattern.Arc: SpawnArc(y); break;
            case Pattern.DiagonalGate: SpawnDiagonalGate(y); break;
            case Pattern.XShape: SpawnXShape(y); break;
            case Pattern.CrossShape: SpawnCrossShape(y); break;
            case Pattern.WallOfVShapes: SpawnWallOfVShapes(y); break;
            case Pattern.MultipleClusters: SpawnMultipleClusters(y); break;
            case Pattern.DiamondRing: SpawnDiamondRing(y); break;
        }
    }

    // -------------------------------------------------------
    // ORIGINAL PATTERNS
    // -------------------------------------------------------

    void SpawnDiagonalLine(float startY, int direction)
    {
        int count = Random.Range(6, 10);
        float stepX = direction * (fieldWidth / count) * 0.8f;
        float stepY = (patternSpacing * 0.6f) / count;
        int gapIndex = Random.Range(1, count - 2);

        float startX = direction > 0
            ? fieldLeft + fieldWidth * 0.1f
            : fieldRight - fieldWidth * 0.1f;

        for (int i = 0; i < count; i++)
        {
            if (i == gapIndex || i == gapIndex + 1) continue;
            SpawnAsteroid(startX + stepX * i, startY + stepY * i);
        }
    }

    void SpawnVShape(float y, bool inverted)
    {
        int halfCount = Random.Range(4, 7);
        float spread = fieldWidth * 0.4f;
        float height = patternSpacing * 0.5f;
        float centerX = fieldLeft + fieldWidth * Random.Range(0.3f, 0.7f);

        for (int i = 0; i < halfCount; i++)
        {
            float t = (float)i / halfCount;
            float offsetX = spread * t;
            float offsetY = inverted ? height * t : height * (1f - t);
            if (t > 0.35f && t < 0.65f) continue;
            SpawnAsteroid(centerX - offsetX, y + offsetY);
            SpawnAsteroid(centerX + offsetX, y + offsetY);
        }
    }

    void SpawnGate(float y)
    {
        float gapWidth = asteroidSize * Random.Range(3f, 5f);
        float gapCenterX = fieldLeft + fieldWidth * Random.Range(0.3f, 0.7f);
        int wallHeight = Random.Range(3, 6);
        float stepY = asteroidSize * 1.2f;

        for (int row = 0; row < wallHeight; row++)
        {
            float rowY = y + row * stepY;
            for (float x = fieldLeft; x < gapCenterX - gapWidth * 0.5f; x += asteroidSize * 1.1f)
                SpawnAsteroid(x, rowY);
            for (float x = gapCenterX + gapWidth * 0.5f; x < fieldRight; x += asteroidSize * 1.1f)
                SpawnAsteroid(x, rowY);
        }
    }

    void SpawnCluster(float y, float centerX)
    {
        int count = Random.Range(8, 14);
        float clusterRadius = fieldWidth * 0.28f;
        float safeZone = asteroidSize * 3f;

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(clusterRadius * 0.3f, clusterRadius);
            float x = centerX + Mathf.Cos(angle) * radius;
            float posY = y + Mathf.Sin(angle) * radius * 0.5f;
            if (Mathf.Abs(x - centerX) < safeZone * 0.5f) continue;
            if (x > fieldLeft && x < fieldRight) SpawnAsteroid(x, posY);
        }
    }

    void SpawnZigZag(float y)
    {
        int segments = Random.Range(3, 5);
        float segHeight = patternSpacing * 0.5f / segments;
        float wallWidth = fieldWidth * 0.45f;

        for (int seg = 0; seg < segments; seg++)
        {
            float segY = y + seg * segHeight;
            bool left = seg % 2 == 0;
            float startX = left ? fieldLeft : fieldRight - wallWidth;
            for (float x = startX; x < startX + wallWidth; x += asteroidSize * 1.1f)
                SpawnAsteroid(x, segY);
        }
    }

    void SpawnArc(float y)
    {
        int count = Random.Range(8, 12);
        float radius = fieldWidth * Random.Range(0.3f, 0.45f);
        float centerX = fieldLeft + fieldWidth * Random.Range(0.35f, 0.65f);
        float gapStart = Random.Range(200f, 340f) * Mathf.Deg2Rad;
        float gapSize = 60f * Mathf.Deg2Rad;

        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.PI + (Mathf.PI * i / count);
            float normalised = Mathf.Repeat(angle - gapStart, Mathf.PI * 2f);
            if (normalised < gapSize) continue;
            float x = centerX + Mathf.Cos(angle) * radius;
            float posY = y + Mathf.Sin(angle) * radius * 0.4f;
            if (x > fieldLeft && x < fieldRight) SpawnAsteroid(x, posY);
        }
    }

    // -------------------------------------------------------
    // NEW PATTERNS
    // -------------------------------------------------------

    // Two parallel diagonal walls with a navigable corridor between them
    void SpawnDiagonalGate(float y)
    {
        int count = Random.Range(7, 11);
        float gapWidth = asteroidSize * Random.Range(3f, 4.5f);
        int direction = Random.value > 0.5f ? 1 : -1;
        float stepX = direction * (fieldWidth / count) * 0.75f;
        float stepY = (patternSpacing * 0.55f) / count;

        float startX = direction > 0
            ? fieldLeft + fieldWidth * 0.1f
            : fieldRight - fieldWidth * 0.1f;

        for (int i = 0; i < count; i++)
        {
            float cx = startX + stepX * i;
            float cy = y + stepY * i;

            // Spawn two walls offset by gapWidth — player flies between them
            float wallOffset = gapWidth * 0.5f;

            // Perpendicular offset to the diagonal direction
            float perpX = -stepY;
            float perpY = stepX;
            float perpLen = Mathf.Sqrt(perpX * perpX + perpY * perpY);
            perpX /= perpLen;
            perpY /= perpLen;

            SpawnAsteroid(cx + perpX * wallOffset, cy + perpY * wallOffset);
            SpawnAsteroid(cx - perpX * wallOffset, cy - perpY * wallOffset);
        }
    }

    // Two diagonal lines crossing in the centre forming an X, with 4 gaps at the tips
    void SpawnXShape(float y)
    {
        int count = Random.Range(6, 9);
        float halfW = fieldWidth * 0.38f;
        float halfH = patternSpacing * 0.45f;
        float centerX = fieldLeft + fieldWidth * Random.Range(0.35f, 0.65f);
        float centerY = y + halfH;

        // Gap: skip the 2 asteroids closest to each tip so players can slip through
        int tipSkip = 2;

        for (int i = tipSkip; i < count - tipSkip; i++)
        {
            float t = (float)i / (count - 1) * 2f - 1f; // -1 to 1
            float dx = t * halfW;
            float dy = t * halfH;

            // \ diagonal
            SpawnAsteroid(centerX + dx, centerY + dy);
            // / diagonal (mirror X)
            SpawnAsteroid(centerX - dx, centerY + dy);
        }
    }

    // A plus-sign cross with a gap at the centre intersection and each arm tip
    void SpawnCrossShape(float y)
    {
        float centerX = fieldLeft + fieldWidth * Random.Range(0.35f, 0.65f);
        float centerY = y + patternSpacing * 0.4f;
        float armLen = fieldWidth * 0.32f;
        float step = asteroidSize * 1.15f;
        float gapZone = asteroidSize * 1.8f; // clear space around centre

        // Horizontal arm
        for (float dx = -armLen; dx <= armLen; dx += step)
        {
            if (Mathf.Abs(dx) < gapZone) continue;     // centre gap
            if (Mathf.Abs(dx) > armLen - step) continue; // tip gap
            SpawnAsteroid(centerX + dx, centerY);
        }

        // Vertical arm
        for (float dy = -armLen * 0.7f; dy <= armLen * 0.7f; dy += step)
        {
            if (Mathf.Abs(dy) < gapZone) continue;
            if (Mathf.Abs(dy) > armLen * 0.7f - step) continue;
            SpawnAsteroid(centerX, centerY + dy);
        }
    }

    // A horizontal row of V-shapes — player must pick one funnel to fly through
    void SpawnWallOfVShapes(float y)
    {
        int vCount = Random.Range(2, 4);           // 2–3 Vs across the screen
        float vWidth = fieldWidth / vCount;
        float vHeight = patternSpacing * 0.45f;
        bool invert = Random.value > 0.5f;

        // One random V is left open (no asteroids) as the safe path
        int safeV = Random.Range(0, vCount);

        for (int v = 0; v < vCount; v++)
        {
            if (v == safeV) continue;

            float vCenterX = fieldLeft + vWidth * v + vWidth * 0.5f;
            int steps = Random.Range(4, 6);

            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                float offsetX = (vWidth * 0.45f) * t;
                float offsetY = invert ? vHeight * t : vHeight * (1f - t);

                SpawnAsteroid(vCenterX - offsetX, y + offsetY);
                SpawnAsteroid(vCenterX + offsetX, y + offsetY);
            }
        }
    }

    // 2–3 clusters spread across the screen with clear corridors between them
    void SpawnMultipleClusters(float y)
    {
        int clusterCount = Random.Range(2, 4);
        float slotWidth = fieldWidth / clusterCount;

        // One slot is always empty — guaranteed corridor
        int safeSlot = Random.Range(0, clusterCount);

        for (int c = 0; c < clusterCount; c++)
        {
            if (c == safeSlot) continue;

            float slotCenterX = fieldLeft + slotWidth * c + slotWidth * 0.5f;
            // Offset center slightly within slot for variety
            float cx = slotCenterX + Random.Range(-slotWidth * 0.15f, slotWidth * 0.15f);
            SpawnCluster(y, cx);
        }
    }

    // A diamond (square rotated 45°) outline with a gap on one side
    void SpawnDiamondRing(float y)
    {
        float centerX = fieldLeft + fieldWidth * Random.Range(0.3f, 0.7f);
        float centerY = y + patternSpacing * 0.4f;
        float size = fieldWidth * Random.Range(0.22f, 0.32f);
        int count = Random.Range(12, 18);

        // Gap side: 0=top, 1=right, 2=bottom, 3=left
        int gapSide = Random.Range(0, 4);

        // Diamond = 4 sides, each going diagonally
        Vector2[] corners = new Vector2[]
        {
            new Vector2(centerX,          centerY + size),  // top
            new Vector2(centerX + size,   centerY),         // right
            new Vector2(centerX,          centerY - size),  // bottom
            new Vector2(centerX - size,   centerY),         // left
        };

        int perSide = count / 4;
        for (int side = 0; side < 4; side++)
        {
            if (side == gapSide) continue; // leave this side open

            Vector2 from = corners[side];
            Vector2 to = corners[(side + 1) % 4];

            for (int i = 1; i < perSide; i++)
            {
                float t = (float)i / perSide;
                float x = Mathf.Lerp(from.x, to.x, t);
                float sy = Mathf.Lerp(from.y, to.y, t);
                if (x > fieldLeft && x < fieldRight)
                    SpawnAsteroid(x, sy);
            }
        }
    }

    // -------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------

    void SpawnAsteroid(float x, float y)
    {
        if (asteroidPrefab == null) return;
        Vector3 pos = new Vector3(x, y, 0f);
        GameObject a = Instantiate(asteroidPrefab, pos, Quaternion.identity);
        a.transform.localScale = Vector3.one * asteroidSize;
        a.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }
}