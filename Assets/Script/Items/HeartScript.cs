using UnityEngine;

public class HeartScript : MonoBehaviour
{
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
                Debug.Log("Calling IncreaseHeart()!");
                player.IncreaseHeart();
            }

            PlayCollectSound(); // <- play sound
            Destroy(gameObject);
        }
    }
}
