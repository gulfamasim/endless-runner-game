using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    public enum Type { Barrier, Laser }
    public Type obstacleType = Type.Barrier;

    void Awake()
    {
        gameObject.tag = "Obstacle";
        // Ensure root has trigger collider
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
        else
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2.2f, 1.8f, 0.5f);
            box.center = new Vector3(0, 0.9f, 0);
        }
    }
}
