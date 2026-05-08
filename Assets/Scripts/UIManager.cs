using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject hudScreen;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;

    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI distText;
    public TextMeshProUGUI coinText;
    public Image           speedBar;
    public GameObject[]    lifeIcons;

    [Header("Game Over")]
    public TextMeshProUGUI goScoreText;
    public TextMeshProUGUI goBestText;

    [Header("Start Screen")]
    public TextMeshProUGUI startBestText;

    // Called once at scene start
    public void ShowStartScreen(float best)
    {
        // Force-find screens by name if references are missing
        if (startScreen    == null) startScreen    = GameObject.Find("StartScreen");
        if (hudScreen      == null) hudScreen      = GameObject.Find("HUDScreen");
        if (pauseScreen    == null) pauseScreen    = GameObject.Find("PauseScreen");
        if (gameOverScreen == null) gameOverScreen = GameObject.Find("GameOverScreen");

        HideAll();
        if (startScreen != null)
        {
            startScreen.SetActive(true);
            // Update best score text
            if (startBestText == null)
                startBestText = startScreen.GetComponentInChildren<TextMeshProUGUI>();
            var texts = startScreen.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var t in texts)
                if (t.gameObject.name == "BestScoreText")
                    t.text = "BEST: " + Mathf.RoundToInt(best);
        }
        else
        {
            Debug.LogError("[UI] StartScreen GameObject not found!");
        }
    }

    public void HideAll()
    {
        SetScreen(startScreen,    false);
        SetScreen(hudScreen,      false);
        SetScreen(pauseScreen,    false);
        SetScreen(gameOverScreen, false);
        // Also search by name as fallback
        ForceHide("StartScreen");
        ForceHide("HUDScreen");
        ForceHide("PauseScreen");
        ForceHide("GameOverScreen");
    }

    public void ShowHUD(int lives, int maxLives)
    {
        HideAll();
        SetScreen(hudScreen, true);
        UpdateLives(lives, maxLives);
    }

    public void ShowPause()
    {
        HideAll();
        SetScreen(pauseScreen, true);
    }

    public void ShowGameOver(int score, int best)
    {
        HideAll();
        SetScreen(gameOverScreen, true);
        if (goScoreText) goScoreText.text = score.ToString("N0");
        if (goBestText)  goBestText.text  = "BEST: " + best.ToString("N0");
    }

    public void UpdateHUD(float score, float dist, int coins, float speedRatio)
    {
        if (scoreText) scoreText.text = Mathf.RoundToInt(score).ToString("N0");
        if (distText)  distText.text  = Mathf.RoundToInt(dist) + "m";
        if (coinText)  coinText.text  = "x" + coins;
        if (speedBar)  speedBar.fillAmount = Mathf.Clamp01(speedRatio);
    }

    public void UpdateLives(int lives, int maxLives)
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
            if (lifeIcons[i]) lifeIcons[i].SetActive(i < lives);
    }

    // Button callbacks
    public void OnStart()
    {
        Debug.Log("[UI] OnStart clicked!");
        GameManager.Instance?.StartGame();
    }
    public void OnPause()   => GameManager.Instance?.PauseGame();
    public void OnResume()  => GameManager.Instance?.ResumeGame();
    public void OnRestart() => GameManager.Instance?.RestartGame();
    public void OnMenu()    => GameManager.Instance?.GoToMenu();

    void SetScreen(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    void ForceHide(string objName)
    {
        // Only hide if it's not already tracked by reference
        var go = GameObject.Find(objName);
        if (go != null) go.SetActive(false);
    }
}
