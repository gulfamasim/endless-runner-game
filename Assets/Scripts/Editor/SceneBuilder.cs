using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// ONE tool that builds the entire scene + all prefabs from scratch
// Tools > Neon Rift > BUILD COMPLETE GAME
public class SceneBuilder : EditorWindow
{
    [MenuItem("Tools/Neon Rift/BUILD COMPLETE GAME")]
    static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("Build Complete Game",
            "This clears the scene and rebuilds EVERYTHING.\nRun this on a fresh empty scene.",
            "Yes, Build It!", "Cancel")) return;

        // Clear scene
        foreach (var go in UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects())
            DestroyImmediate(go);

        EnsureFolder("Assets/NeonRift");
        EnsureFolder("Assets/NeonRift/Materials");
        EnsureFolder("Assets/NeonRift/Prefabs");

        AddTag("Player"); AddTag("Obstacle"); AddTag("Coin"); AddTag("Ground");

        // ── MATERIALS ─────────────────────────────────────────
        // Road
        var mRoad   = Mat("Road",    new Color(0.18f,0.18f,0.20f));
        var mLine   = Mat("Line",    new Color(0.95f,0.90f,0.15f));
        var mKerb   = Mat("Kerb",    new Color(0.80f,0.15f,0.10f));
        var mDirt   = Mat("Dirt",    new Color(0.52f,0.36f,0.20f));
        var mGrass  = Mat("Grass",   new Color(0.20f,0.55f,0.16f));
        // Nature
        var mTrunk  = Mat("Trunk",   new Color(0.36f,0.22f,0.10f));
        var mLeafA  = Mat("LeafA",   new Color(0.12f,0.52f,0.14f));
        var mLeafB  = Mat("LeafB",   new Color(0.22f,0.60f,0.10f));
        var mLeafC  = Mat("LeafC",   new Color(0.08f,0.38f,0.10f));
        var mRock   = Mat("Rock",    new Color(0.50f,0.47f,0.44f));
        var mRockD  = Mat("RockD",   new Color(0.32f,0.30f,0.27f));
        var mBush   = Mat("Bush",    new Color(0.16f,0.46f,0.12f));
        var mMushR  = Mat("MushR",   new Color(0.82f,0.14f,0.10f));
        var mMushW  = Mat("MushW",   new Color(0.94f,0.92f,0.88f));
        var mFlower = Mat("Flower",  new Color(0.95f,0.78f,0.10f));
        // Obstacles
        var mBarr   = Mat("Barrier", new Color(0.72f,0.10f,0.08f));
        var mGlow   = Mat("Glow",    new Color(0.00f,0.90f,1.00f));
        var mPost   = Mat("Post",    new Color(0.25f,0.25f,0.28f));
        // Coin
        var mCoin   = Mat("Coin",    new Color(1.00f,0.82f,0.10f));
        var mCoinR  = Mat("CoinRim", new Color(1.00f,0.95f,0.40f));
        // Background
        var mMtn    = Mat("Mountain",new Color(0.55f,0.52f,0.50f));
        var mMtnSnow= Mat("MtnSnow", new Color(0.92f,0.92f,0.95f));
        var mBldg   = Mat("Bldg",    new Color(0.30f,0.32f,0.38f));
        var mBldgW  = Mat("BldgWin", new Color(0.85f,0.82f,0.50f));
        var mGround = Mat("GroundP", new Color(0.24f,0.52f,0.18f));

        // ── BUILD PREFABS ─────────────────────────────────────
        var chunkPrefab = BuildTrackChunk(mRoad,mLine,mKerb,mDirt,mGrass);
        var barrierPrefab = BuildBarrier(mBarr,mGlow);
        var laserPrefab   = BuildLaser(mPost,mGlow);
        var coinPrefab    = BuildCoin(mCoin,mCoinR);

        var sceneryPrefabs = new List<GameObject> {
            BuildPineTall(mTrunk,mLeafA),
            BuildPineShort(mTrunk,mLeafC),
            BuildOak(mTrunk,mLeafB),
            BuildBush(mBush),
            BuildRockBig(mRock),
            BuildRockCluster(mRock,mRockD),
            BuildMushroom(mMushR,mMushW),
            BuildFlowers(mFlower,mLeafA),
        };

        // ── SCENE OBJECTS ─────────────────────────────────────
        // Ground plane (huge, just visual)
        var groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundPlane.name = "GroundPlane";
        groundPlane.transform.localScale = new Vector3(10f, 1f, 500f);
        groundPlane.transform.position = new Vector3(0, -0.15f, 1000f);
        groundPlane.GetComponent<Renderer>().sharedMaterial = mGround;
        DestroyImmediate(groundPlane.GetComponent<MeshCollider>());

        // Background mountains
        BuildBackground(mMtn, mMtnSnow, mBldg, mBldgW);

        // Lighting
        var lightGO = new GameObject("Sun");
        var sun = lightGO.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.95f, 0.85f);
        sun.intensity = 1.3f;
        sun.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(52f, -25f, 0f);
        RenderSettings.ambientLight = new Color(0.45f,0.50f,0.60f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance   = 180f;
        RenderSettings.fogColor = new Color(0.65f,0.75f,0.90f);

        // ── PLAYER ───────────────────────────────────────────
        var player = new GameObject("Player");
        player.tag  = "Player";
        player.transform.position = new Vector3(0, 1f, 0);
        // Visual capsule
        var vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        vis.name = "Mesh"; vis.transform.parent = player.transform;
        vis.transform.localPosition = new Vector3(0,0,0);
        vis.transform.localScale    = new Vector3(0.6f,1f,0.6f);
        vis.GetComponent<Renderer>().sharedMaterial = Mat("PlayerMat", new Color(0.2f,0.5f,1f));
        DestroyImmediate(vis.GetComponent<CapsuleCollider>());
        var pc = player.AddComponent<PlayerController>();

        // ── CAMERA ───────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.AddComponent<AudioListener>();
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 62f;
        cam.farClipPlane = 600f;
        var cc = camGO.AddComponent<CameraController>();
        cc.target = player.transform;
        camGO.transform.position = new Vector3(0, 5.5f, -7f);
        camGO.transform.LookAt(new Vector3(0,1.5f,0));

        // ── MANAGERS ─────────────────────────────────────────
        var mgrs = new GameObject("--- MANAGERS ---");
        var gmGO = new GameObject("GameManager"); gmGO.transform.parent = mgrs.transform;
        var tmGO = new GameObject("TrackManager"); tmGO.transform.parent = mgrs.transform;
        var uiGO = new GameObject("UIManager");   uiGO.transform.parent = mgrs.transform;

        var gm = gmGO.AddComponent<GameManager>();
        var tm = tmGO.AddComponent<TrackManager>();
        var ui = uiGO.AddComponent<UIManager>();

        // Assign TrackManager prefabs
        tm.trackChunkPrefab = chunkPrefab;
        tm.obstaclePrefabs  = new GameObject[]{ barrierPrefab, laserPrefab };
        tm.coinPrefab        = coinPrefab;
        tm.sceneryPrefabs    = sceneryPrefabs.ToArray();
        tm.chunkLength       = 40f;
        tm.poolSize          = 10;
        tm.scenerySpread     = 10f;
        tm.sceneryPerChunk   = 7;

        gm.player      = pc;
        gm.trackManager = tm;
        gm.uiManager   = ui;
        gm.camController = cc;

        // ── UI ────────────────────────────────────────────────
        BuildUI(uiGO, ui, gm);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("DONE!",
            "Complete game scene built!\n\n" +
            "Optional: Drag your FBX character as child of Player, then run\n" +
            "Tools > Neon Rift > Setup Character Animator\n\n" +
            "Press Ctrl+S to save, then Play!", "Let's Play!");
    }

    // ─────────────────────────────────────────────────────────
    // TRACK CHUNK — seamless, no gaps
    // ─────────────────────────────────────────────────────────
    static GameObject BuildTrackChunk(Material road,Material line,Material kerb,
                                       Material dirt,Material grass)
    {
        float L = 40f;
        var root = new GameObject("TrackChunk_Default");

        // Road
        var r = Cube("Road", root, Vector3.zero, new Vector3(7.5f, 0.1f, L), road);
        DestroyImmediate(r.GetComponent<BoxCollider>());

        // Lane dashes (centre)
        for (int i = 0; i < 8; i++)
        {
            float z = -L/2f + 2.5f + i * (L/8f);
            var d = Cube("DC"+i, root, new Vector3(0,0.06f,z), new Vector3(0.1f,0.05f,1.8f), line);
            DestroyImmediate(d.GetComponent<BoxCollider>());
        }
        // Lane dashes (sides)
        foreach (float lx in new[]{-2.5f, 2.5f})
            for (int i = 0; i < 8; i++)
            {
                float z = -L/2f + 2.5f + i * (L/8f);
                var d = Cube("DS"+i, root, new Vector3(lx,0.06f,z), new Vector3(0.06f,0.05f,1.2f), line);
                DestroyImmediate(d.GetComponent<BoxCollider>());
            }

        // Kerbs
        foreach (float kx in new[]{-3.95f, 3.95f})
        {
            var k = Cube("Kerb", root, new Vector3(kx,0.1f,0), new Vector3(0.4f,0.2f,L), kerb);
            DestroyImmediate(k.GetComponent<BoxCollider>());
        }

        // Dirt shoulders
        foreach (float dx in new[]{-5.8f, 5.8f})
        {
            var d = Cube("Dirt", root, new Vector3(dx,0.02f,0), new Vector3(3.5f,0.06f,L), dirt);
            DestroyImmediate(d.GetComponent<BoxCollider>());
        }

        // Grass
        foreach (float gx in new[]{-10f, 10f})
        {
            var g = Cube("Grass", root, new Vector3(gx,0.01f,0), new Vector3(8f,0.05f,L), grass);
            DestroyImmediate(g.GetComponent<BoxCollider>());
        }

        return SavePrefab(root, "TrackChunk_Default");
    }

    // ─────────────────────────────────────────────────────────
    // OBSTACLES
    // ─────────────────────────────────────────────────────────
    static GameObject BuildBarrier(Material mat, Material glow)
    {
        var root = new GameObject("Obstacle_Barrier");
        root.tag = "Obstacle";

        var body = Cube("Body", root, new Vector3(0,0.5f,0), new Vector3(2.3f,1f,0.4f), mat);
        DestroyImmediate(body.GetComponent<BoxCollider>());
        var strip = Cube("Strip", root, new Vector3(0,1.02f,0), new Vector3(2.3f,0.08f,0.08f), glow);
        DestroyImmediate(strip.GetComponent<BoxCollider>());
        for (int i = 0; i < 4; i++)
        {
            float x = -0.9f + i * 0.6f;
            var s = Cube("S"+i, root, new Vector3(x,0.5f,0.21f), new Vector3(0.12f,1f,0.02f), glow);
            DestroyImmediate(s.GetComponent<BoxCollider>());
        }

        var bc = root.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center = new Vector3(0,0.5f,0);
        bc.size   = new Vector3(2.3f,1f,0.5f);
        root.AddComponent<ObstacleBase>();
        return SavePrefab(root, "Obstacle_Barrier");
    }

    static GameObject BuildLaser(Material post, Material glow)
    {
        var root = new GameObject("Obstacle_Laser");
        root.tag = "Obstacle";

        foreach (float px in new[]{-1.1f, 1.1f})
        {
            var p = Cyl("Post", root, new Vector3(px,0.9f,0), new Vector3(0.14f,0.9f,0.14f), post);
            DestroyImmediate(p.GetComponent<CapsuleCollider>());
        }
        var beam = Cube("Beam", root, new Vector3(0,1.55f,0), new Vector3(2.2f,0.08f,0.08f), glow);
        DestroyImmediate(beam.GetComponent<BoxCollider>());

        var bc = root.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center = new Vector3(0,1.55f,0);
        bc.size   = new Vector3(2.4f,0.12f,0.6f);
        var ob = root.AddComponent<ObstacleBase>();
        ob.obstacleType = ObstacleBase.Type.Laser;
        return SavePrefab(root, "Obstacle_Laser");
    }

    // ─────────────────────────────────────────────────────────
    // COIN
    // ─────────────────────────────────────────────────────────
    static GameObject BuildCoin(Material gold, Material rim)
    {
        var root = new GameObject("Coin");
        root.tag = "Coin";

        var disc = Cyl("Disc", root, Vector3.zero, new Vector3(0.48f,0.07f,0.48f), gold);
        DestroyImmediate(disc.GetComponent<CapsuleCollider>());
        var ring = Cyl("Ring", root, Vector3.zero, new Vector3(0.54f,0.055f,0.54f), rim);
        DestroyImmediate(ring.GetComponent<CapsuleCollider>());

        var cc = root.AddComponent<CapsuleCollider>();
        cc.isTrigger = true; cc.radius = 0.35f; cc.height = 0.3f; cc.direction = 1;
        root.AddComponent<CoinBehavior>();
        return SavePrefab(root, "Coin");
    }

    // ─────────────────────────────────────────────────────────
    // TREES
    // ─────────────────────────────────────────────────────────
    static GameObject BuildPineTall(Material trunk, Material leaf)
    {
        var r = new GameObject("Tree_PineTall");
        Cyl("T", r, new Vector3(0,2f,0),    new Vector3(0.20f,4f,0.20f), trunk);
        Sph("L0",r, new Vector3(0,1.5f,0),  new Vector3(2.2f,0.9f,2.2f), leaf);
        Sph("L1",r, new Vector3(0,2.6f,0),  new Vector3(1.7f,0.8f,1.7f), leaf);
        Sph("L2",r, new Vector3(0,3.4f,0),  new Vector3(1.2f,0.8f,1.2f), leaf);
        Sph("L3",r, new Vector3(0,4.1f,0),  new Vector3(0.7f,0.7f,0.7f), leaf);
        return SavePrefab(r, "Tree_PineTall");
    }

    static GameObject BuildPineShort(Material trunk, Material leaf)
    {
        var r = new GameObject("Tree_PineShort");
        Cyl("T", r, new Vector3(0,1f,0),    new Vector3(0.18f,2f,0.18f), trunk);
        Sph("L0",r, new Vector3(0,0.9f,0),  new Vector3(1.8f,0.8f,1.8f), leaf);
        Sph("L1",r, new Vector3(0,1.7f,0),  new Vector3(1.3f,0.7f,1.3f), leaf);
        Sph("L2",r, new Vector3(0,2.3f,0),  new Vector3(0.7f,0.6f,0.7f), leaf);
        return SavePrefab(r, "Tree_PineShort");
    }

    static GameObject BuildOak(Material trunk, Material leaf)
    {
        var r = new GameObject("Tree_Oak");
        Cyl("T",  r, new Vector3(0,1.5f,0),      new Vector3(0.28f,3f,0.28f),   trunk);
        Cyl("BL", r, new Vector3(-0.7f,2.5f,0),   new Vector3(0.12f,1.2f,0.12f), trunk);
        Cyl("BR", r, new Vector3(0.6f, 2.8f,0.3f),new Vector3(0.10f,1.0f,0.10f), trunk);
        Sph("C0", r, new Vector3(0,4.2f,0),       new Vector3(2.4f,2.0f,2.4f),   leaf);
        Sph("C1", r, new Vector3(-1.1f,3.6f,0.4f),new Vector3(1.6f,1.5f,1.6f),   leaf);
        Sph("C2", r, new Vector3(1.0f, 3.8f,-0.3f),new Vector3(1.5f,1.4f,1.5f),  leaf);
        return SavePrefab(r, "Tree_Oak");
    }

    // ─────────────────────────────────────────────────────────
    // GROUND DETAILS
    // ─────────────────────────────────────────────────────────
    static GameObject BuildBush(Material mat)
    {
        var r = new GameObject("Bush");
        Sph("B0",r,new Vector3(0,0.4f,0),     new Vector3(1.0f,0.7f,0.9f),mat);
        Sph("B1",r,new Vector3(0.7f,0.3f,0.2f),new Vector3(0.7f,0.55f,0.65f),mat);
        Sph("B2",r,new Vector3(-0.6f,0.3f,0.1f),new Vector3(0.7f,0.55f,0.65f),mat);
        Sph("B3",r,new Vector3(0.1f,0.28f,-0.55f),new Vector3(0.6f,0.5f,0.55f),mat);
        return SavePrefab(r, "Bush");
    }

    static GameObject BuildRockBig(Material mat)
    {
        var r = new GameObject("Rock_Big");
        Sph("M", r,new Vector3(0,0.55f,0),     new Vector3(1.5f,1.1f,1.3f),mat);
        Sph("S1",r,new Vector3(0.8f,0.3f,0.4f),new Vector3(0.9f,0.7f,0.8f),mat);
        Sph("S2",r,new Vector3(-0.6f,0.28f,-0.3f),new Vector3(0.8f,0.6f,0.7f),mat);
        return SavePrefab(r, "Rock_Big");
    }

    static GameObject BuildRockCluster(Material light, Material dark)
    {
        var r = new GameObject("Rock_Cluster");
        Sph("R0",r,Vector3.zero,            new Vector3(0.9f,0.65f,0.8f),light);
        Sph("R1",r,new Vector3(0.9f,-0.1f,0.4f),new Vector3(0.6f,0.5f,0.55f),dark);
        Sph("R2",r,new Vector3(-0.75f,-0.15f,0.3f),new Vector3(0.55f,0.45f,0.5f),light);
        Sph("R3",r,new Vector3(0.2f,-0.2f,-0.65f),new Vector3(0.5f,0.4f,0.45f),dark);
        return SavePrefab(r, "Rock_Cluster");
    }

    static GameObject BuildMushroom(Material cap, Material stem)
    {
        var r = new GameObject("Mushroom");
        Cyl("St",r,new Vector3(0,0.28f,0),new Vector3(0.16f,0.55f,0.16f),stem);
        Sph("C", r,new Vector3(0,0.65f,0),new Vector3(0.58f,0.38f,0.58f),cap);
        Sph("S0",r,new Vector3(0.13f,0.75f,0.13f),new Vector3(0.10f,0.06f,0.10f),stem);
        Sph("S1",r,new Vector3(-0.11f,0.72f,0.1f),new Vector3(0.08f,0.05f,0.08f),stem);
        return SavePrefab(r, "Mushroom");
    }

    static GameObject BuildFlowers(Material flower, Material leaf)
    {
        var r = new GameObject("Flowers");
        Vector3[] pts = {Vector3.zero,new Vector3(0.45f,0,0.2f),
                          new Vector3(-0.35f,0,0.3f),new Vector3(0.1f,0,-0.45f)};
        foreach (var p in pts)
        {
            Cyl("St",r,p+Vector3.up*0.22f,new Vector3(0.04f,0.44f,0.04f),leaf);
            Sph("H", r,p+Vector3.up*0.50f,new Vector3(0.22f,0.16f,0.22f),flower);
        }
        return SavePrefab(r, "Flowers");
    }

    // ─────────────────────────────────────────────────────────
    // BACKGROUND — mountains + city buildings
    // ─────────────────────────────────────────────────────────
    static void BuildBackground(Material mtn, Material snow, Material bldg, Material win)
    {
        var bg = new GameObject("Background");
        bg.transform.position = new Vector3(0, 0, 300f);

        // Mountain range — left side
        float[] mtnH  = {28f,40f,55f,35f,48f,32f,44f};
        float[] mtnX  = {-80f,-55f,-35f,-15f,15f,35f,55f};
        float[] mtnW  = {22f,28f,22f,18f,20f,24f,20f};
        for (int i = 0; i < mtnH.Length; i++)
        {
            var m = Sph("M"+i, bg,
                new Vector3(mtnX[i], mtnH[i]*0.4f, -20f + i*3f),
                new Vector3(mtnW[i], mtnH[i], mtnW[i]*0.7f), mtn);
            // Snow cap
            Sph("S"+i, bg,
                new Vector3(mtnX[i], mtnH[i]*0.72f, -20f + i*3f),
                new Vector3(mtnW[i]*0.4f, mtnH[i]*0.3f, mtnW[i]*0.35f), snow);
        }

        // City buildings — right side
        (float x, float h, float w)[] bldgs = {
            (70f,30f,8f),(82f,45f,10f),(95f,20f,7f),(108f,55f,9f),
            (120f,35f,8f),(132f,25f,6f),(144f,42f,9f),(156f,18f,5f)
        };
        foreach (var (x,h,w) in bldgs)
        {
            var b = Cube("B", bg, new Vector3(x, h*0.5f, 0), new Vector3(w, h, w*0.9f), bldg);
            DestroyImmediate(b.GetComponent<BoxCollider>());
            // Window rows
            for (int wy = 1; wy < (int)(h/6); wy++)
                for (int wx = 0; wx < 2; wx++)
                {
                    float wox = -w*0.25f + wx * w*0.5f;
                    var wn = Cube("W", bg,
                        new Vector3(x+wox, wy*6f, w*0.46f),
                        new Vector3(w*0.2f, 1.2f, 0.2f), win);
                    DestroyImmediate(wn.GetComponent<BoxCollider>());
                }
        }
    }

    // ─────────────────────────────────────────────────────────
    // UI — complete canvas
    // ─────────────────────────────────────────────────────────
    static void BuildUI(GameObject uiGO, UIManager ui, GameManager gm)
    {
        var cGO = new GameObject("GameCanvas");
        var canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080,1920);
        scaler.matchWidthOrHeight = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // START SCREEN
        var ss = Panel(cGO,"StartScreen",new Color(0.04f,0.04f,0.15f,0.96f));
        TMP(ss,"Sub","ENDLESS RUNNER",  new Vector2(0,100),new Vector2(700,80),50,
            new Color(0.4f,0.8f,1f),FontStyles.Bold);
        TMP(ss,"BestScoreText","BEST: 0",new Vector2(0,-220),new Vector2(400,55),34,
            new Color(1f,0.85f,0.3f));
        var startBtn = Btn(ss,"StartBtn","TAP TO RUN",new Vector2(0,-50),
            new Vector2(520,130),new Color(0f,0.72f,1f));
        Wire(startBtn, ui, "OnStart");
        ui.startScreen   = ss;
        ui.startBestText = ss.transform.Find("BestScoreText").GetComponent<TextMeshProUGUI>();

        // HUD SCREEN
        var hs = Panel(cGO,"HUDScreen",Color.clear);
        hs.SetActive(false);
        ui.scoreText = TMP(hs,"ScoreText","0",
            new Vector2(0,-70),new Vector2(500,80),68,Color.white,FontStyles.Bold,
            new Vector2(0.5f,1f),new Vector2(0.5f,1f)).GetComponent<TextMeshProUGUI>();
        ui.distText  = TMP(hs,"DistText","0m",
            new Vector2(0,-158),new Vector2(300,52),36,new Color(0.6f,0.9f,1f),
            FontStyles.Normal,new Vector2(0.5f,1f),new Vector2(0.5f,1f)).GetComponent<TextMeshProUGUI>();
        ui.coinText  = TMP(hs,"CoinText","x0",
            new Vector2(110,-70),new Vector2(220,60),40,new Color(1f,0.9f,0.2f),
            FontStyles.Bold,new Vector2(0f,1f),new Vector2(0f,1f)).GetComponent<TextMeshProUGUI>();

        // Hearts (life icons)
        ui.lifeIcons = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var h = new GameObject("Heart"+i);
            h.transform.SetParent(hs.transform, false);
            var img = h.AddComponent<Image>();
            img.color = new Color(1f,0.2f,0.3f);
            var rt = h.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f,1f); rt.anchorMax = new Vector2(1f,1f);
            rt.pivot = new Vector2(1f,1f);
            rt.sizeDelta = new Vector2(55f,55f);
            rt.anchoredPosition = new Vector2(-70f - i*65f, -65f);
            // Heart text
            var ht = new GameObject("H"); ht.transform.SetParent(h.transform,false);
            var tmp = ht.AddComponent<TextMeshProUGUI>();
            tmp.text = "H"; tmp.fontSize = 32; tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
            var hrt = ht.GetComponent<RectTransform>();
            hrt.anchorMin=Vector2.zero; hrt.anchorMax=Vector2.one;
            hrt.sizeDelta=Vector2.zero; hrt.anchoredPosition=Vector2.zero;
            ui.lifeIcons[i] = h;
        }

        // Speed bar
        var sbg = new GameObject("SpeedBG");
        sbg.transform.SetParent(hs.transform,false);
        sbg.AddComponent<Image>().color = new Color(0.1f,0.1f,0.25f,0.9f);
        var sbgrt = sbg.GetComponent<RectTransform>();
        sbgrt.anchorMin=new Vector2(0,0); sbgrt.anchorMax=new Vector2(0,0);
        sbgrt.pivot=new Vector2(0,0); sbgrt.anchoredPosition=new Vector2(30,30);
        sbgrt.sizeDelta=new Vector2(230,16);
        var sf = new GameObject("SpeedFill"); sf.transform.SetParent(sbg.transform,false);
        ui.speedBar = sf.AddComponent<Image>();
        ui.speedBar.color = new Color(0f,0.85f,1f);
        ui.speedBar.type = Image.Type.Filled;
        ui.speedBar.fillMethod = Image.FillMethod.Horizontal;
        ui.speedBar.fillAmount = 0.1f;
        var sfrt = sf.GetComponent<RectTransform>();
        sfrt.anchorMin=Vector2.zero; sfrt.anchorMax=Vector2.one;
        sfrt.sizeDelta=Vector2.zero; sfrt.anchoredPosition=Vector2.zero;

        // Pause button
        var pb = Btn(hs,"PauseBtn","II",new Vector2(-65,-68),new Vector2(85,85),
            new Color(0.15f,0.15f,0.3f,0.9f),new Vector2(1f,1f),new Vector2(1f,1f));
        Wire(pb, ui, "OnPause");
        ui.hudScreen = hs;

        // PAUSE SCREEN
        var ps = Panel(cGO,"PauseScreen",new Color(0,0,0,0.88f));
        ps.SetActive(false);
        TMP(ps,"Title","PAUSED",new Vector2(0,230),new Vector2(500,110),82,Color.white,FontStyles.Bold);
        Wire(Btn(ps,"ResBtn","RESUME",  new Vector2(0,70), new Vector2(440,110),new Color(0,0.78f,0.38f)),ui,"OnResume");
        Wire(Btn(ps,"RstBtn","RESTART", new Vector2(0,-60),new Vector2(440,110),new Color(0,0.58f,1f)),ui,"OnRestart");
        Wire(Btn(ps,"MenuBtn","MENU",   new Vector2(0,-190),new Vector2(440,110),new Color(0.48f,0.12f,0.78f)),ui,"OnMenu");
        ui.pauseScreen = ps;

        // GAME OVER SCREEN
        var go2 = Panel(cGO,"GameOverScreen",new Color(0,0,0,0.92f));
        go2.SetActive(false);
        TMP(go2,"Title","GAME OVER",new Vector2(0,340),new Vector2(680,130),78,
            new Color(1f,0.22f,0.22f),FontStyles.Bold);
        TMP(go2,"Label","SCORE",new Vector2(0,220),new Vector2(300,55),30,new Color(0.6f,0.8f,1f));
        ui.goScoreText = TMP(go2,"FinalScore","0",new Vector2(0,135),
            new Vector2(520,110),90,Color.white,FontStyles.Bold).GetComponent<TextMeshProUGUI>();
        ui.goBestText  = TMP(go2,"BestScore","BEST: 0",new Vector2(0,45),
            new Vector2(440,65),38,new Color(1f,0.85f,0.2f)).GetComponent<TextMeshProUGUI>();
        Wire(Btn(go2,"RetryBtn","PLAY AGAIN",new Vector2(0,-90), new Vector2(440,110),new Color(0f,0.78f,1f)),ui,"OnRestart");
        Wire(Btn(go2,"MenuBtn", "MENU",      new Vector2(0,-220),new Vector2(440,110),new Color(0.45f,0.12f,0.75f)),ui,"OnMenu");
        ui.gameOverScreen = go2;

        EditorUtility.SetDirty(ui);
    }

    // ─────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────
    static void Wire(GameObject btn, UIManager ui, string method)
    {
        var b = btn?.GetComponent<Button>(); if (b==null||ui==null) return;
        b.onClick.RemoveAllListeners();
        System.Action act = method switch {
            "OnStart"   => (System.Action)ui.OnStart,
            "OnPause"   => ui.OnPause,
            "OnResume"  => ui.OnResume,
            "OnRestart" => ui.OnRestart,
            "OnMenu"    => ui.OnMenu,
            _ => null
        };
        if (act!=null)
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                b.onClick, new UnityEngine.Events.UnityAction(act));
    }

    static GameObject Panel(GameObject p,string name,Color c)
    {
        var go=new GameObject(name); go.transform.SetParent(p.transform,false);
        go.AddComponent<Image>().color=c;
        var rt=go.GetComponent<RectTransform>();
        rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
        rt.sizeDelta=Vector2.zero;rt.anchoredPosition=Vector2.zero;
        return go;
    }

    static GameObject TMP(GameObject parent,string name,string text,
        Vector2 pos,Vector2 size,float fs,Color col,
        FontStyles style=FontStyles.Normal,
        Vector2 aMin=default,Vector2 aMax=default)
    {
        if(aMin==default)aMin=new Vector2(0.5f,0.5f);
        if(aMax==default)aMax=new Vector2(0.5f,0.5f);
        var go=new GameObject(name); go.transform.SetParent(parent.transform,false);
        var t=go.AddComponent<TextMeshProUGUI>();
        t.text=text;t.fontSize=fs;t.color=col;
        t.fontStyle=style;t.alignment=TextAlignmentOptions.Center;
        var rt=go.GetComponent<RectTransform>();
        rt.anchorMin=aMin;rt.anchorMax=aMax;
        rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchoredPosition=pos;rt.sizeDelta=size;
        return go;
    }

    static GameObject Btn(GameObject parent,string name,string label,
        Vector2 pos,Vector2 size,Color col,
        Vector2 aMin=default,Vector2 aMax=default)
    {
        if(aMin==default)aMin=new Vector2(0.5f,0.5f);
        if(aMax==default)aMax=new Vector2(0.5f,0.5f);
        var go=new GameObject(name); go.transform.SetParent(parent.transform,false);
        go.AddComponent<Image>().color=col;
        var btn=go.AddComponent<Button>();
        var cb=btn.colors; cb.highlightedColor=col*1.35f; cb.pressedColor=col*0.6f; btn.colors=cb;
        var rt=go.GetComponent<RectTransform>();
        rt.anchorMin=aMin;rt.anchorMax=aMax;rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchoredPosition=pos;rt.sizeDelta=size;
        var lgo=new GameObject("Lbl"); lgo.transform.SetParent(go.transform,false);
        var t=lgo.AddComponent<TextMeshProUGUI>();
        t.text=label;t.fontSize=40;t.color=Color.white;
        t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;
        var lrt=lgo.GetComponent<RectTransform>();
        lrt.anchorMin=Vector2.zero;lrt.anchorMax=Vector2.one;
        lrt.sizeDelta=Vector2.zero;lrt.anchoredPosition=Vector2.zero;
        return go;
    }

    static GameObject Cube(string n,GameObject p,Vector3 pos,Vector3 sc,Material mat)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=n;go.transform.parent=p.transform;
        go.transform.localPosition=pos;go.transform.localScale=sc;
        go.GetComponent<Renderer>().sharedMaterial=mat;
        return go;
    }

    static GameObject Sph(string n,GameObject p,Vector3 pos,Vector3 sc,Material mat)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name=n;go.transform.parent=p.transform;
        go.transform.localPosition=pos;go.transform.localScale=sc;
        go.GetComponent<Renderer>().sharedMaterial=mat;
        DestroyImmediate(go.GetComponent<SphereCollider>());
        return go;
    }

    static GameObject Cyl(string n,GameObject p,Vector3 pos,Vector3 sc,Material mat)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name=n;go.transform.parent=p.transform;
        go.transform.localPosition=pos;go.transform.localScale=sc;
        go.GetComponent<Renderer>().sharedMaterial=mat;
        DestroyImmediate(go.GetComponent<CapsuleCollider>());
        return go;
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        string path = "Assets/NeonRift/Prefabs/" + name + ".prefab";
        var prefab  = PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
        return prefab;
    }

    static Material Mat(string name, Color col)
    {
        string path = "Assets/NeonRift/Materials/" + name + ".mat";
        var ex = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (ex != null) { ex.color = col; return ex; }
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        m.color = col;
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static void AddTag(string tag)
    {
        var so = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = so.FindProperty("tags");
        for (int i=0;i<tags.arraySize;i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize-1).stringValue = tag;
        so.ApplyModifiedProperties();
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/'); var cur = parts[0];
        for (int i=1;i<parts.Length;i++)
        {
            var next = cur+"/"+parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur,parts[i]);
            cur = next;
        }
    }
}
