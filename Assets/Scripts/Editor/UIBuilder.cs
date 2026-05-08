using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

// Tools > Neon Rift > Build UI Only
// Run this AFTER the scene already has UIManager in it
public class UIBuilder : EditorWindow
{
    [MenuItem("Tools/Neon Rift/Build UI Only")]
    static void Build()
    {
        // Remove old canvas
        var old = GameObject.Find("GameCanvas");
        if (old != null) DestroyImmediate(old);

        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui == null)
        {
            EditorUtility.DisplayDialog("Error","No UIManager found in scene!\nMake sure UIManager object exists.","OK");
            return;
        }

        // ── Canvas ────────────────────────────────────────────
        var cGO = new GameObject("GameCanvas");
        var canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // ── START SCREEN ──────────────────────────────────────
        var ss = MakePanel(cGO, "StartScreen", new Color(0.04f, 0.04f, 0.15f, 0.96f));
        MakeTMP(ss, "Sub", "ENDLESS RUNNER", new Vector2(0,100), new Vector2(700,80), 50,
            new Color(0.4f,0.8f,1f), FontStyles.Bold);
        MakeTMP(ss, "BestScoreText", "BEST: 0", new Vector2(0,-220), new Vector2(400,55), 34,
            new Color(1f,0.85f,0.3f));
        var startBtn = MakeBtn(ss, "StartBtn", "TAP TO RUN", new Vector2(0,-50),
            new Vector2(520,130), new Color(0f,0.72f,1f));
        // Wire start button
        var sb = startBtn.GetComponent<Button>();
        sb.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sb.onClick,
            new UnityEngine.Events.UnityAction(ui.OnStart));

        ui.startScreen   = ss;
        ui.startBestText = ss.transform.Find("BestScoreText").GetComponent<TextMeshProUGUI>();

        // ── HUD SCREEN ────────────────────────────────────────
        var hs = MakePanel(cGO, "HUDScreen", Color.clear);
        hs.SetActive(false);

        ui.scoreText = MakeTMP(hs,"ScoreText","0",
            new Vector2(0,-70), new Vector2(500,80), 68, Color.white, FontStyles.Bold,
            new Vector2(0.5f,1f), new Vector2(0.5f,1f)).GetComponent<TextMeshProUGUI>();

        ui.distText = MakeTMP(hs,"DistText","0m",
            new Vector2(0,-158), new Vector2(300,52), 36, new Color(0.6f,0.9f,1f),
            FontStyles.Normal, new Vector2(0.5f,1f), new Vector2(0.5f,1f)).GetComponent<TextMeshProUGUI>();

        ui.coinText = MakeTMP(hs,"CoinText","x0",
            new Vector2(110,-70), new Vector2(220,60), 40, new Color(1f,0.9f,0.2f),
            FontStyles.Bold, new Vector2(0f,1f), new Vector2(0f,1f)).GetComponent<TextMeshProUGUI>();

        // Hearts
        ui.lifeIcons = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var h = new GameObject("Heart"+i);
            h.transform.SetParent(hs.transform, false);
            h.AddComponent<Image>().color = new Color(1f,0.2f,0.3f);
            var rt = h.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f,1f); rt.anchorMax = new Vector2(1f,1f);
            rt.pivot     = new Vector2(1f,1f);
            rt.sizeDelta = new Vector2(55f,55f);
            rt.anchoredPosition = new Vector2(-70f - i*65f, -65f);
            var lbl = new GameObject("Lbl"); lbl.transform.SetParent(h.transform,false);
            var t = lbl.AddComponent<TextMeshProUGUI>();
            t.text="H"; t.fontSize=30; t.color=Color.white;
            t.alignment=TextAlignmentOptions.Center; t.fontStyle=FontStyles.Bold;
            var lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin=Vector2.zero; lrt.anchorMax=Vector2.one;
            lrt.sizeDelta=Vector2.zero; lrt.anchoredPosition=Vector2.zero;
            ui.lifeIcons[i] = h;
        }

        // Speed bar
        var sbg = new GameObject("SpeedBG");
        sbg.transform.SetParent(hs.transform,false);
        sbg.AddComponent<Image>().color = new Color(0.1f,0.1f,0.25f,0.9f);
        var sbgrt = sbg.GetComponent<RectTransform>();
        sbgrt.anchorMin = new Vector2(0,0); sbgrt.anchorMax = new Vector2(0,0);
        sbgrt.pivot = new Vector2(0,0);
        sbgrt.anchoredPosition = new Vector2(30,30);
        sbgrt.sizeDelta = new Vector2(230,16);
        var sfGO = new GameObject("SpeedFill");
        sfGO.transform.SetParent(sbg.transform,false);
        ui.speedBar = sfGO.AddComponent<Image>();
        ui.speedBar.color = new Color(0f,0.85f,1f);
        ui.speedBar.type = Image.Type.Filled;
        ui.speedBar.fillMethod = Image.FillMethod.Horizontal;
        ui.speedBar.fillAmount = 0.1f;
        var sfrt = sfGO.GetComponent<RectTransform>();
        sfrt.anchorMin=Vector2.zero; sfrt.anchorMax=Vector2.one;
        sfrt.sizeDelta=Vector2.zero; sfrt.anchoredPosition=Vector2.zero;

        // Pause button
        var pb = MakeBtn(hs,"PauseBtn","II",new Vector2(-65,-68),new Vector2(85,85),
            new Color(0.15f,0.15f,0.3f,0.9f),new Vector2(1f,1f),new Vector2(1f,1f));
        var pbb = pb.GetComponent<Button>();
        pbb.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pbb.onClick,
            new UnityEngine.Events.UnityAction(ui.OnPause));
        ui.hudScreen = hs;

        // ── PAUSE SCREEN ──────────────────────────────────────
        var ps = MakePanel(cGO,"PauseScreen",new Color(0,0,0,0.88f));
        ps.SetActive(false);
        MakeTMP(ps,"Title","PAUSED",new Vector2(0,230),new Vector2(500,110),82,Color.white,FontStyles.Bold);
        WireBtn(MakeBtn(ps,"ResBtn","RESUME",  new Vector2(0, 70),new Vector2(440,110),new Color(0,0.78f,0.38f)),ui.OnResume);
        WireBtn(MakeBtn(ps,"RstBtn","RESTART", new Vector2(0,-60),new Vector2(440,110),new Color(0,0.58f,1f)),  ui.OnRestart);
        WireBtn(MakeBtn(ps,"MenuBtn","MENU",   new Vector2(0,-190),new Vector2(440,110),new Color(0.48f,0.12f,0.78f)),ui.OnMenu);
        ui.pauseScreen = ps;

        // ── GAME OVER SCREEN ──────────────────────────────────
        var go2 = MakePanel(cGO,"GameOverScreen",new Color(0,0,0,0.92f));
        go2.SetActive(false);
        MakeTMP(go2,"Title","GAME OVER",new Vector2(0,340),new Vector2(680,130),78,
            new Color(1f,0.22f,0.22f),FontStyles.Bold);
        MakeTMP(go2,"Label","SCORE",new Vector2(0,220),new Vector2(300,55),30,new Color(0.6f,0.8f,1f));
        ui.goScoreText = MakeTMP(go2,"FinalScore","0",new Vector2(0,135),
            new Vector2(520,110),90,Color.white,FontStyles.Bold).GetComponent<TextMeshProUGUI>();
        ui.goBestText = MakeTMP(go2,"BestScore","BEST: 0",new Vector2(0,45),
            new Vector2(440,65),38,new Color(1f,0.85f,0.2f)).GetComponent<TextMeshProUGUI>();
        WireBtn(MakeBtn(go2,"RetryBtn","PLAY AGAIN",new Vector2(0,-90), new Vector2(440,110),new Color(0f,0.78f,1f)),   ui.OnRestart);
        WireBtn(MakeBtn(go2,"MenuBtn2","MENU",       new Vector2(0,-220),new Vector2(440,110),new Color(0.45f,0.12f,0.75f)),ui.OnMenu);
        ui.gameOverScreen = go2;

        EditorUtility.SetDirty(ui);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("UI Built!",
            "All 4 screens created and wired:\n\n" +
            "✅ Start Screen\n✅ HUD (score, dist, coins, hearts, speed bar)\n" +
            "✅ Pause Screen\n✅ Game Over Screen\n\n" +
            "Press Ctrl+S then Play!", "Let's Go!");
    }

    static void WireBtn(GameObject go, UnityEngine.Events.UnityAction action)
    {
        var b = go?.GetComponent<Button>(); if (b==null) return;
        b.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, action);
    }

    static GameObject MakePanel(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform,false);
        go.AddComponent<Image>().color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
        rt.sizeDelta=Vector2.zero; rt.anchoredPosition=Vector2.zero;
        return go;
    }

    static GameObject MakeTMP(GameObject parent, string name, string text,
        Vector2 pos, Vector2 size, float fs, Color col,
        FontStyles style=FontStyles.Normal,
        Vector2 aMin=default, Vector2 aMax=default)
    {
        if (aMin==default) aMin = new Vector2(0.5f,0.5f);
        if (aMax==default) aMax = new Vector2(0.5f,0.5f);
        var go = new GameObject(name); go.transform.SetParent(parent.transform,false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text=text; t.fontSize=fs; t.color=col;
        t.fontStyle=style; t.alignment=TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin=aMin; rt.anchorMax=aMax;
        rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchoredPosition=pos; rt.sizeDelta=size;
        return go;
    }

    static GameObject MakeBtn(GameObject parent, string name, string label,
        Vector2 pos, Vector2 size, Color col,
        Vector2 aMin=default, Vector2 aMax=default)
    {
        if (aMin==default) aMin = new Vector2(0.5f,0.5f);
        if (aMax==default) aMax = new Vector2(0.5f,0.5f);
        var go = new GameObject(name); go.transform.SetParent(parent.transform,false);
        go.AddComponent<Image>().color = col;
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.highlightedColor = col*1.35f; cb.pressedColor = col*0.6f; btn.colors = cb;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin=aMin; rt.anchorMax=aMax;
        rt.pivot=new Vector2(0.5f,0.5f);
        rt.anchoredPosition=pos; rt.sizeDelta=size;
        var lgo = new GameObject("Lbl"); lgo.transform.SetParent(go.transform,false);
        var t = lgo.AddComponent<TextMeshProUGUI>();
        t.text=label; t.fontSize=40; t.color=Color.white;
        t.fontStyle=FontStyles.Bold; t.alignment=TextAlignmentOptions.Center;
        var lrt = lgo.GetComponent<RectTransform>();
        lrt.anchorMin=Vector2.zero; lrt.anchorMax=Vector2.one;
        lrt.sizeDelta=Vector2.zero; lrt.anchoredPosition=Vector2.zero;
        return go;
    }
}
