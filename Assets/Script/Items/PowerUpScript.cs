using UnityEngine;

public class PowerUpScript : MonoBehaviour
{
    public float damageBoost = 5f;
    public AudioClip collectSound;

    void Start()
    {

    }

    void Update()
    {

    }

    void PlayCollectSound()
    {
        AudioManager.PlayClip(collectSound, transform.position, 0.3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger detected with: " + collision.name);

        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player detected!");

            PlayerScript player = collision.GetComponent<PlayerScript>() ??
                                  collision.GetComponentInParent<PlayerScript>();
            if (player != null)
            {
                Debug.Log("Calling IncreaseDamage()!");
                player.IncreaseDamage(damageBoost);
            }

            PlayCollectSound(); // <--- Play the collect sound
            Destroy(gameObject);
        }
    }
}
