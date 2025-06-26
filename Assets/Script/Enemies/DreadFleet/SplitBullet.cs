using UnityEngine;

public class SplitBullet : MonoBehaviour
{
    public GameObject splitBulletPrefab;
    public int splitCount = 6;
    public float splitSpeed = 5f;
    public float maxTravelDistance = 5f; // khoảng cách để kích hoạt split

    private Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;
    }

    void Update()
    {
        float traveled = Vector3.Distance(spawnPosition, transform.position);

        if (traveled >= maxTravelDistance)
        {
            Split();
        }
    }

    void Split()
    {
        float angleStep = 360f / splitCount;
        float angle = 0f;

        for (int i = 0; i < splitCount; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

            GameObject bullet = Instantiate(splitBulletPrefab, transform.position, Quaternion.identity);
            Destroy(bullet, 5f);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = direction * splitSpeed;

            angle += angleStep;
        }

        Destroy(gameObject); // hủy viên đạn chính sau khi split
    }

}
