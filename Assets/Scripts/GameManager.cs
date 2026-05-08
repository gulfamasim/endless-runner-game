using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed")]
    public float startSpeed = 8f;
    public float maxSpeed   = 22f;
    public float speedRamp  = 0.04f;

    [Header("Lives")]
    public int   maxLives       = 3;
    public float invincibleTime = 2.5f;

    [Header("Refs — MUST be assigned in Inspector")]
    public PlayerController player;
    public TrackManager      trackManager;
    public UIManager         uiManager;
    public CameraController  camController;

    public bool  IsPlaying    { get; private set; }
    public float GameSpeed    { get; private set; }
    public float Score        { get; private set; }
    public float Distance     { get; private set; }
    public int   Lives        { get; private set; }
    public int   Coins        { get; private set; }
    public bool  IsInvincible { get; private set; }
    public float BestScore    { get; private set; }

    bool gameOverFired = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BestScore = PlayerPrefs.GetFloat("Best", 0f);

        // NEVER auto-start — always wait for button
        IsPlaying = false;
        Time.timeScale = 1f;
    }

    void Start()
    {
        // Auto-find refs if not assigned in Inspector
        if (player       == null) player       = FindFirstObjectByType<PlayerController>();
        if (trackManager == null) trackManager = FindFirstObjectByType<TrackManager>();
        if (uiManager    == null) uiManager    = FindFirstObjectByType<UIManager>();
        if (camController== null) camController= FindFirstObjectByType<CameraController>();

        // Log what we found so you can see in Console
        Debug.Log($"[GM] player={player}, track={trackManager}, ui={uiManager}, cam={camController}");

        // Stop player animating on start screen
        if (player != null)
        {
            player.gameObject.SetActive(false); // hide until game starts
        }

        // Show start screen — ALWAYS
        if (uiManager != null)
            uiManager.ShowStartScreen(BestScore);
        else
            Debug.LogError("[GM] UIManager not found! Canvas/UIManager missing from scene.");
    }

    void Update()
    {
        if (!IsPlaying) return;
        GameSpeed  = Mathf.Min(GameSpeed + speedRamp * Time.deltaTime, maxSpeed);
        Distance  += GameSpeed * Time.deltaTime;
        Score     += GameSpeed * Time.deltaTime;
        uiManager?.UpdateHUD(Score, Distance, Coins, GameSpeed / maxSpeed);
    }

    public void StartGame()
    {
        Debug.Log("[GM] StartGame called!");
        IsPlaying       = true;
        gameOverFired   = false;
        GameSpeed       = startSpeed;
        Score = 0; Distance = 0; Coins = 0;
        Lives           = maxLives;
        IsInvincible    = false;

        // Show player again
        if (player != null) player.gameObject.SetActive(true);

        trackManager?.ResetTrack();
        uiManager?.HideAll();
        uiManager?.ShowHUD(Lives, maxLives);
        player?.Revive();
    }

    public void PlayerHit()
    {
        if (!IsPlaying || IsInvincible || gameOverFired) return;
        Lives--;
        Debug.Log("[GM] Hit! Lives: " + Lives);
        uiManager?.UpdateLives(Lives, maxLives);
        camController?.Shake(0.35f, 0.25f);

        if (Lives <= 0)
        {
            gameOverFired = true;
            IsPlaying     = false;
            IsInvincible  = true;
            player?.TriggerDie();
            StartCoroutine(GameOverDelay());
        }
        else
        {
            StartCoroutine(InvincibleWindow());
        }
    }

    IEnumerator InvincibleWindow()
    {
        IsInvincible = true;
        var rends = player?.GetComponentsInChildren<Renderer>();
        float t = 0f;
        while (t < invincibleTime)
        {
            bool show = Mathf.FloorToInt(t * 8f) % 2 == 0;
            if (rends != null) foreach (var r in rends) r.enabled = show;
            yield return new WaitForSeconds(0.12f);
            t += 0.12f;
        }
        if (rends != null) foreach (var r in rends) r.enabled = true;
        IsInvincible = false;
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(2.2f);
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetFloat("Best", BestScore);
            PlayerPrefs.Save();
        }
        uiManager?.ShowGameOver(Mathf.RoundToInt(Score), Mathf.RoundToInt(BestScore));
    }

    public void CollectCoin(GameObject coin)
    {
        if (coin == null || !coin.activeInHierarchy) return;
        coin.SetActive(false);
        Coins++;
        Score += 100f;
        uiManager?.UpdateHUD(Score, Distance, Coins, GameSpeed / maxSpeed);
    }

    public void PauseGame()
    {
        if (!IsPlaying) return;
        Time.timeScale = 0f;
        uiManager?.ShowPause();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        uiManager?.ShowHUD(Lives, maxLives);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
