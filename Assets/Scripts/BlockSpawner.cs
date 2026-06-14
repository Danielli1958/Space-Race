using UnityEngine;
using System.Collections.Generic;

public class BlockSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;

    [Header("Grid Settings")]
    public int columnCount = 8;
    public float rowSpacing = 2f;

    [Header("Timing")]
    public float startDelay = 2f;

    [Header("Maze Settings")]
    public int minGapWidth = 2;         // Minimum width of any gap (true path or dead end)
    public int maxGapWidth = 3;         // Maximum width of any gap
    public int minDeadEnds = 0;         // Minimum extra dead-end branches per row
    public int maxDeadEnds = 2;         // Maximum extra dead-end branches per row
    public int deadEndLifeMin = 3;      // How many rows a dead end stays open (min)
    public int deadEndLifeMax = 6;      // How many rows a dead end stays open (max)

    private float spawnY;
    private float columnWidth;
    private float screenLeftEdge;

    // The single guaranteed true path
    private int trueGapStart;
    private int trueGapEnd;

    // Active dead ends: each entry is (gapStart, gapEnd, rowsRemaining)
    private List<(int start, int end, int life)> deadEnds = new List<(int, int, int)>();

    void Start()
    {
        Camera cam = Camera.main;
        spawnY = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y + 1f;
        screenLeftEdge = cam.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenRightEdge = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        columnWidth = (screenRightEdge - screenLeftEdge) / columnCount;

        // Start the true path in the middle
        int gapWidth = Random.Range(minGapWidth, maxGapWidth + 1);
        trueGapStart = (columnCount - gapWidth) / 2;
        trueGapEnd = trueGapStart + gapWidth - 1;

        InvokeRepeating(nameof(SpawnNextRow), startDelay, GetSpawnInterval());
    }

    void SpawnNextRow()
    {
        if (GameManager.Instance.IsGameOver) return;
        SpawnRow(spawnY);
        CancelInvoke(nameof(SpawnNextRow));
        InvokeRepeating(nameof(SpawnNextRow), GetSpawnInterval(), GetSpawnInterval());
    }

    float GetSpawnInterval()
    {
        float speed = ScrollManager.Instance != null ? ScrollManager.Instance.CurrentSpeed : 3f;
        return rowSpacing / speed;
    }

    void SpawnRow(float yPos)
    {
        // Step 1: Advance the true path
        AdvanceTruePath();

        // Step 2: Age dead ends, remove expired ones
        AgeDeadEnds();

        // Step 3: Maybe spawn new dead ends
        MaybeSpawnDeadEnds();

        // Step 4: Build the open columns set from true path + active dead ends
        HashSet<int> openColumns = GetAllOpenColumns();

        // Step 5: Spawn blocks in every closed column
        for (int col = 0; col < columnCount; col++)
        {
            if (!openColumns.Contains(col))
            {
                float xPos = screenLeftEdge + (col * columnWidth) + (columnWidth / 2f);
                GameObject block = Instantiate(blockPrefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
                block.transform.localScale = new Vector3(columnWidth, rowSpacing * 1.1f, 1f);
            }
        }
    }

    // --- True Path ---

    void AdvanceTruePath()
    {
        int gapWidth = Random.Range(minGapWidth, maxGapWidth + 1);

        // Shift left or right by 1 — guaranteed overlap with previous true path
        int shift = Random.Range(-2, 3);
        int newStart = Mathf.Clamp(trueGapStart + shift, 0, columnCount - gapWidth);
        int newEnd = newStart + gapWidth - 1;

        // Safety: ensure overlap
        bool overlaps = newEnd >= trueGapStart && newStart <= trueGapEnd;
        if (!overlaps)
        {
            newStart = Mathf.Clamp(trueGapStart, 0, columnCount - gapWidth);
            newEnd = newStart + gapWidth - 1;
        }

        trueGapStart = newStart;
        trueGapEnd = newEnd;
    }

    // --- Dead Ends ---

    void AgeDeadEnds()
    {
        List<(int, int, int)> surviving = new List<(int, int, int)>();
        foreach (var (start, end, life) in deadEnds)
        {
            if (life - 1 > 0)
                surviving.Add((start, end, life - 1));
            // When life hits 0, the dead end is simply not added back — it seals off
        }
        deadEnds = surviving;
    }

    void MaybeSpawnDeadEnds()
    {
        int count = Random.Range(minDeadEnds, maxDeadEnds + 1);

        for (int i = 0; i < count; i++)
        {
            int gapWidth = Random.Range(minGapWidth, maxGapWidth + 1);

            // Try a few random positions, skip if they overlap the true path
            // (dead ends should be clearly separate from the real path)
            for (int attempt = 0; attempt < 10; attempt++)
            {
                int start = Random.Range(0, columnCount - gapWidth + 1);
                int end = start + gapWidth - 1;

                // Reject if it touches the true path — we want them visually distinct
                bool touchesTruePath = end >= trueGapStart - 1 && start <= trueGapEnd + 1;
                if (touchesTruePath) continue;

                // Reject if it overlaps an existing dead end
                bool touchesDeadEnd = false;
                foreach (var (ds, de, _) in deadEnds)
                {
                    if (end >= ds - 1 && start <= de + 1) { touchesDeadEnd = true; break; }
                }
                if (touchesDeadEnd) continue;

                // Valid position found — check if there's an existing dead end nearby
                // that we should extend rather than create a new one
                bool extended = false;
                for (int j = 0; j < deadEnds.Count; j++)
                {
                    var (ds, de, dl) = deadEnds[j];
                    // If a dead end is within 1 column of this new one, continue it
                    bool adjacent = end >= ds - 1 && start <= de + 1;
                    if (adjacent)
                    {
                        deadEnds[j] = (ds, de, Mathf.Min(dl + 1, deadEndLifeMax));
                        extended = true;
                        break;
                    }
                }

                if (!extended)
                {
                    int life = Random.Range(deadEndLifeMin, deadEndLifeMax + 1);
                    deadEnds.Add((start, end, life));
                }

                break;
            }
        }
    }

    HashSet<int> GetAllOpenColumns()
    {
        HashSet<int> open = new HashSet<int>();

        for (int col = trueGapStart; col <= trueGapEnd; col++)
            open.Add(col);

        foreach (var (start, end, _) in deadEnds)
            for (int col = start; col <= end; col++)
                open.Add(col);

        return open;
    }
}