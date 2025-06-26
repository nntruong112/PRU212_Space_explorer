using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;

    private Vector3 direction = Vector3.down;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
