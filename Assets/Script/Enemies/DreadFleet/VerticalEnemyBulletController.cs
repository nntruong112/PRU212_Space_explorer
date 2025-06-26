using UnityEngine;

public class VerticalEnemyBulletController : MonoBehaviour
{
    public Vector2 direction = Vector2.down;
    public float speed = 7f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }
}
