using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Track")]
    public GameObject trackChunkPrefab;
    public float chunkLength = 40f;
    public int   poolSize    = 10; // fixed pool — chunks recycle

    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;

    [Header("Coins")]
    public GameObject coinPrefab;

    [Header("Scenery")]
    public GameObject[] sceneryPrefabs;
    public float scenerySpread   = 12f;
    public int   sceneryPerChunk = 6;

    // Pool of reusable chunks
    GameObject[]     pool;
    // Which logical chunk index each pool slot is currently showing
    int[]            poolIndex;
    // Children spawned ON each pool slot (obstacles, coins, scenery)
    List<GameObject>[] poolExtras;

    float   distance  = 0f;  // how far player has run
    int     nextChunk = 0;   // next logical chunk index to assign
    GameManager gm;

    void Start()
    {
        gm = GameManager.Instance;
    }

    public void ResetTrack()
    {
        distance  = 0f;
        nextChunk = 0;

        // Destroy old pool if exists
        if (pool != null)
            foreach (var g in pool) if (g) Destroy(g);

        pool       = new GameObject[poolSize];
        poolIndex  = new int[poolSize];
        poolExtras = new List<GameObject>[poolSize];

        if (trackChunkPrefab == null)
        {
            Debug.LogError("[Track] trackChunkPrefab not assigned!"); return;
        }

        // Build pool and place chunks end-to-end starting at Z=0
        for (int i = 0; i < poolSize; i++)
        {
            poolExtras[i] = new List<GameObject>();
            pool[i] = Instantiate(trackChunkPrefab);
            poolIndex[i] = i;
            PositionChunk(i, i);      // slot i gets logical chunk i
            PopulateChunk(i, i);
        }
        nextChunk = poolSize;
    }

    void Update()
    {
        if (gm == null || !gm.IsPlaying) return;

        distance += gm.GameSpeed * Time.deltaTime;

        // Move all pool chunks toward camera
        float move = gm.GameSpeed * Time.deltaTime;
        for (int i = 0; i < poolSize; i++)
        {
            if (pool[i] == null) continue;
            var p = pool[i].transform.position;
            p.z -= move;
            pool[i].transform.position = p;

            // Also move extras
            foreach (var ex in poolExtras[i])
            {
                if (ex == null) continue;
                var ep = ex.transform.position;
                ep.z -= move;
                ex.transform.position = ep;
            }
        }

        // Recycle chunks that have passed behind the player
        // Player is at Z=0, chunk back edge = chunkZ - chunkLength/2
        for (int i = 0; i < poolSize; i++)
        {
            if (pool[i] == null) continue;
            float backEdge = pool[i].transform.position.z - chunkLength * 0.5f;
            if (backEdge < -chunkLength)
            {
                // Recycle: move this chunk to the front
                RecycleChunk(i);
            }
        }
    }

    void PositionChunk(int slot, int logicalIndex)
    {
        // Chunk centre = logicalIndex * chunkLength + half
        float z = logicalIndex * chunkLength + chunkLength * 0.5f;
        pool[slot].transform.position = new Vector3(0, 0, z);
        poolIndex[slot] = logicalIndex;
    }

    void RecycleChunk(int slot)
    {
        // Clear old extras
        foreach (var ex in poolExtras[slot])
            if (ex) Destroy(ex);
        poolExtras[slot].Clear();

        // Assign next logical index
        int idx = nextChunk++;
        poolIndex[slot] = idx;

        // Find furthest current chunk Z to place after it
        float maxZ = -Mathf.Infinity;
        for (int i = 0; i < poolSize; i++)
            if (pool[i] != null)
                maxZ = Mathf.Max(maxZ, pool[i].transform.position.z + chunkLength * 0.5f);

        pool[slot].transform.position = new Vector3(0, 0, maxZ + chunkLength * 0.5f);

        PopulateChunk(slot, idx);
    }

    void PopulateChunk(int slot, int logicalIndex)
    {
        // First 2 chunks = safe zone, no obstacles
        if (logicalIndex < 2) { SpawnScenery(slot); return; }

        SpawnObstacles(slot);
        SpawnCoins(slot);
        SpawnScenery(slot);
    }

    void SpawnObstacles(int slot)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        float[] laneX = { -2.5f, 0f, 2.5f };
        float baseZ   = pool[slot].transform.position.z - chunkLength * 0.5f;

        int toBlock = Random.Range(1, 3);
        var used = new List<int>();
        for (int i = 0; i < toBlock; i++)
        {
            int l = Random.Range(0, 3);
            if (used.Contains(l)) continue;
            used.Add(l);
            var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            float zOff = Random.Range(chunkLength * 0.3f, chunkLength * 0.7f);
            var go = Instantiate(prefab, new Vector3(laneX[l], 0, baseZ + zOff), Quaternion.identity);
            poolExtras[slot].Add(go);
        }
    }

    void SpawnCoins(int slot)
    {
        if (coinPrefab == null) return;
        float[] laneX = { -2.5f, 0f, 2.5f };
        float baseZ   = pool[slot].transform.position.z - chunkLength * 0.5f;
        int lane = Random.Range(0, 3);
        for (int i = 0; i < 5; i++)
        {
            var go = Instantiate(coinPrefab,
                new Vector3(laneX[lane], 1.2f, baseZ + 4f + i * 2.5f),
                Quaternion.identity);
            poolExtras[slot].Add(go);
        }
    }

    void SpawnScenery(int slot)
    {
        if (sceneryPrefabs == null || sceneryPrefabs.Length == 0) return;
        float baseZ = pool[slot].transform.position.z - chunkLength * 0.5f;
        for (int i = 0; i < sceneryPerChunk; i++)
        {
            var prefab = sceneryPrefabs[Random.Range(0, sceneryPrefabs.Length)];
            if (prefab == null) continue;
            float side = Random.value > 0.5f ? 1f : -1f;
            float x    = side * (scenerySpread + Random.Range(0f, 5f));
            float zOff = Random.Range(2f, chunkLength - 2f);
            var go = Instantiate(prefab,
                new Vector3(x, 0, baseZ + zOff),
                Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            poolExtras[slot].Add(go);
        }
    }
}
