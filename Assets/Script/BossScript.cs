using UnityEngine;

public class BossScript : MonoBehaviour
{
    public float speed = 1f;
    public float distance = 3f;
    public float health = 100000000000000000000f;


    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * distance;
        transform.position = new Vector3(startPos.x + x, startPos.y, startPos.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
            Die();
    }

    void Die()
    {
        // Your boss death logic (explosion, score, etc.)
        Destroy(gameObject);
    }
}
