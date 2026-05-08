using UnityEngine;

public class CoinBehavior : MonoBehaviour
{
    public float spinSpeed = 200f;
    public float bobAmt    = 0.12f;
    public float bobSpeed  = 3f;
    float startY;

    void Awake()
    {
        gameObject.tag = "Coin";
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    void Start() => startY = transform.position.y;

    void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.World);
        var p = transform.position;
        p.y = startY + Mathf.Sin(Time.time * bobSpeed) * bobAmt;
        transform.position = p;
    }
}
