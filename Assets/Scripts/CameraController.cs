using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Position")]
    public Transform target;
    public float height   = 4.5f;
    public float distance = 7f;
    public float smooth   = 14f;
    public float xSmooth  = 10f;

    [Header("FOV")]
    public float baseFOV = 62f;
    public float maxFOV  = 78f;

    Camera cam;
    float  shakeAmt, shakeDur, shakeT;
    GameManager gm;

    void Awake()
    {
        cam = GetComponent<Camera>();
        // Solid sky-blue background — no white flash
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.42f, 0.62f, 0.85f);
        cam.farClipPlane     = 600f;
        cam.fieldOfView      = baseFOV;
    }

    void Start()
    {
        gm = GameManager.Instance;
        if (target == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p) target = p.transform;
        }
        if (target) SnapNow();
    }

    void SnapNow()
    {
        transform.position = CalcPos(target.position);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    Vector3 CalcPos(Vector3 t) =>
        new Vector3(t.x, t.y + height, t.z - distance);

    void LateUpdate()
    {
        if (!target) return;
        gm = gm ?? GameManager.Instance;

        Vector3 desired = CalcPos(target.position);
        Vector3 cur     = transform.position;

        // Snap Y and Z, smooth X for lane changes
        float newX = Mathf.Lerp(cur.x, desired.x, xSmooth * Time.deltaTime);
        Vector3 next = new Vector3(newX,
            Mathf.Lerp(cur.y, desired.y, smooth * Time.deltaTime),
            Mathf.Lerp(cur.z, desired.z, smooth * Time.deltaTime));

        // Screen shake
        if (shakeT > 0f)
        {
            shakeT -= Time.deltaTime;
            float s = shakeAmt * (shakeT / shakeDur);
            next   += new Vector3(
                Random.Range(-s, s),
                Random.Range(-s * 0.5f, s * 0.5f),
                0f);
        }

        transform.position = next;
        transform.LookAt(target.position + Vector3.up * 1.5f);

        // FOV increases with speed
        if (cam != null && gm != null)
        {
            float t = Mathf.Clamp01(gm.GameSpeed / gm.maxSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
                Mathf.Lerp(baseFOV, maxFOV, t), 2f * Time.deltaTime);
        }
    }

    public void Shake(float amt, float dur)
    {
        shakeAmt = amt;
        shakeDur = shakeT = dur;
    }
}
