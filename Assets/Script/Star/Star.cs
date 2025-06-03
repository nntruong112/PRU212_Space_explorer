using UnityEngine;

public class Star : MonoBehaviour
{
    public float scoreValue = 100f;
    public float speed = 2f;
    public AudioClip collectSound;

    void Start()
    {
        // Init value
        scoreValue = 100f;
    }
    void PlayCollectSound()
    {
        AudioManager.PlayClip(collectSound, transform.position);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        // Optional: Add score to player here
        if (GameManager.Instance != null)
        {
                GameManager.Instance.AddScore(scoreValue);
        }
        PlayCollectSound();
        Destroy(gameObject);
    }
}
}
