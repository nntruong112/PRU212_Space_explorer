using UnityEngine;

public class EnemyBulletController : MonoBehaviour
{
    public Vector2 direction;
    public float speed = 5f;
    public float lifetime = 6f;

    void Start()
    {
    }

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
        Destroy(gameObject, lifetime);
    }
}
