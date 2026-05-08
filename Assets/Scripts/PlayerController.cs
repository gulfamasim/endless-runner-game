using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Lanes")]
    public float[] lanes = { -2.5f, 0f, 2.5f };
    public int   lane      = 1;
    public float laneSpeed = 14f;

    [Header("Jump")]
    public float jumpPower   = 11f;
    public float fallGravity = 28f;
    public float groundY     = 1f;

    [Header("Slide")]
    public float slideDuration = 0.75f;

    public Animator animator { get; private set; }

    Rigidbody       rb;
    CapsuleCollider cap;
    float   targetX, velY;
    bool    grounded = true, sliding, dead;
    Vector2 swipeStart;
    Vector3 capCenter;
    float   capHeight;

    // prevent holding key from spamming lane changes
    bool laneChangeCooldown = false;

    static int H_Run   = Animator.StringToHash("IsRunning");
    static int H_Jump  = Animator.StringToHash("IsJumping");
    static int H_Slide = Animator.StringToHash("IsSliding");
    static int H_TurnL = Animator.StringToHash("TurnLeft");
    static int H_TurnR = Animator.StringToHash("TurnRight");
    static int H_Die   = Animator.StringToHash("Die");
    static int H_Rev   = Animator.StringToHash("Revive");

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        cap = GetComponent<CapsuleCollider>();
        cap.height = 2f; cap.radius = 0.38f;
        cap.center = Vector3.zero;
        cap.isTrigger = true;
        capCenter = cap.center;
        capHeight = cap.height;

        gameObject.tag = "Player";
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        targetX = lanes[lane];
        transform.position = new Vector3(targetX, groundY, 0);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (dead || gm == null) return;

        // Pause toggle
        if (SafeKeyDown(KeyCode.Escape) || SafeKeyDown(KeyCode.P))
        {
            if (Time.timeScale > 0) gm.PauseGame();
            else                    gm.ResumeGame();
            return;
        }

        if (!gm.IsPlaying) return;

        HandleInput();

        // Smooth lateral movement
        Vector3 p = transform.position;
        p.x = Mathf.MoveTowards(p.x, targetX, laneSpeed * Time.deltaTime);

        // Gravity / jump arc
        if (!grounded)
        {
            velY -= fallGravity * Time.deltaTime;
            p.y  += velY * Time.deltaTime;
            if (p.y <= groundY)
            {
                p.y = groundY;
                grounded = true; velY = 0;
                SetB(H_Jump, false);
            }
        }
        transform.position = p;
    }

    void HandleInput()
    {
        // Keyboard lane change
        if (SafeKeyDown(KeyCode.LeftArrow)  || SafeKeyDown(KeyCode.A)) ChangeLane(-1);
        if (SafeKeyDown(KeyCode.RightArrow) || SafeKeyDown(KeyCode.D)) ChangeLane(1);

        // Jump
        if ((SafeKeyDown(KeyCode.Space) || SafeKeyDown(KeyCode.UpArrow) || SafeKeyDown(KeyCode.W))
             && grounded && !sliding)
            StartCoroutine(DoJump());

        // Slide
        if ((SafeKeyDown(KeyCode.DownArrow) || SafeKeyDown(KeyCode.S))
             && grounded && !sliding)
            StartCoroutine(DoSlide());

        // Touch swipe
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                swipeStart = t.position;

            if (t.phase == TouchPhase.Ended)
            {
                Vector2 delta = t.position - swipeStart;
                if (delta.magnitude < 40f) return;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    // Horizontal swipe = lane change
                    if (delta.x > 0) ChangeLane(1);
                    else             ChangeLane(-1);
                }
                else
                {
                    // Vertical swipe = jump or slide
                    if (delta.y > 0 && grounded && !sliding)       StartCoroutine(DoJump());
                    else if (delta.y < 0 && grounded && !sliding)  StartCoroutine(DoSlide());
                }
            }
        }
    }

    void ChangeLane(int dir)
    {
        if (laneChangeCooldown) return;
        int n = Mathf.Clamp(lane + dir, 0, lanes.Length - 1);
        if (n == lane) return;
        lane = n;
        targetX = lanes[lane];
        Trig(dir < 0 ? H_TurnL : H_TurnR);
        StartCoroutine(LaneCooldown());
    }

    IEnumerator LaneCooldown()
    {
        laneChangeCooldown = true;
        yield return new WaitForSeconds(0.15f);
        laneChangeCooldown = false;
    }

    IEnumerator DoJump()
    {
        grounded = false;
        velY     = jumpPower;
        SetB(H_Jump, true);
        yield return null;
    }

    IEnumerator DoSlide()
    {
        sliding = true;
        // Shrink collider so player fits under laser
        cap.center = new Vector3(0, -0.5f, 0);
        cap.height = 1f;
        transform.localScale = new Vector3(1f, 0.55f, 1f);
        SetB(H_Slide, true);

        yield return new WaitForSeconds(slideDuration);

        cap.center = capCenter;
        cap.height = capHeight;
        transform.localScale = Vector3.one;
        SetB(H_Slide, false);
        sliding = false;
    }

    public void Revive()
    {
        dead    = false;
        grounded = true;
        sliding = false;
        velY    = 0f;
        laneChangeCooldown = false;
        transform.localScale = Vector3.one;
        cap.center = capCenter;
        cap.height = capHeight;
        transform.position = new Vector3(lanes[lane], groundY, 0);
        SetB(H_Run, true);
        Trig(H_Rev);
    }

    public void TriggerDie()
    {
        if (dead) return;
        dead = true;
        SetB(H_Run,   false);
        SetB(H_Jump,  false);
        SetB(H_Slide, false);
        Trig(H_Die);
    }

    void OnTriggerEnter(Collider other)
    {
        var gm = GameManager.Instance;
        if (gm == null || dead) return;
        if      (other.CompareTag("Obstacle")) gm.PlayerHit();
        else if (other.CompareTag("Coin"))     gm.CollectCoin(other.gameObject);
    }

    bool SafeKeyDown(KeyCode k)
    {
        try { return Input.GetKeyDown(k); }
        catch { return false; }
    }

    void SetB(int h, bool v) { animator?.SetBool(h, v); }
    void Trig(int h)         { animator?.SetTrigger(h); }
}
