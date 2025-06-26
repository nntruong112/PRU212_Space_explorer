using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject verticalEnemyPrefab;
    public float spawnInterval = 5f;
    public int minEnemies = 3;
    public int maxEnemies = 7;
    public float moveSpeed = 3f;
    public float slotReuseDelay = 3f;
    public float spacingX = 2f;
    public float spacingY = 0.3f;  // ✅ thêm spacingY

    private float timer;
    private List<float> allSlots = new List<float>();
    private List<float> availableSlots = new List<float>();
    private Dictionary<float, float> slotCooldown = new Dictionary<float, float>();
    private List<float> ySlotHistory = new List<float>();

    void Start()
    {
        float camHeight = Camera.main.orthographicSize * 2f;
        float camTopY = Camera.main.orthographicSize;
        float camBottomY = -Camera.main.orthographicSize;

        float marginY = 1f; // tránh sát mép
        float spawnMinY = 0f + marginY;                 // từ giữa màn hình trở lên
        float spawnMaxY = camTopY - marginY;            // cách đỉnh 1 đơn vị

        // Chia slot đều trong khoảng spawnMinY -> spawnMaxY
        int slotCount = 3;
        allSlots.Clear();
        for (int i = 0; i < slotCount; i++)
        {
            float t = (i + 1f) / (slotCount + 1f); // t = 0.25, 0.5, 0.75
            float slotY = Mathf.Lerp(spawnMinY, spawnMaxY, t);
            allSlots.Add(slotY);
        }

        availableSlots = new List<float>(allSlots);
    }

    void Update()
    {
        timer += Time.deltaTime;

        Debug.Log($"[EnemySpawner] Update - Timer: {timer:F2} / Interval: {spawnInterval}");

        List<float> cooledDown = new List<float>();
        foreach (var kvp in slotCooldown.ToList())
        {
            slotCooldown[kvp.Key] -= Time.deltaTime;
            if (slotCooldown[kvp.Key] <= 0)
            {
                cooledDown.Add(kvp.Key);
            }
        }

        foreach (float y in cooledDown)
        {
            slotCooldown.Remove(y);
            if (!availableSlots.Contains(y))
                availableSlots.Add(y);
        }

        Debug.Log($"[EnemySpawner] Available slots: {availableSlots.Count}");

        if (timer >= spawnInterval && availableSlots.Count > 0)
        {
            Debug.Log("[EnemySpawner] ✅ Conditions met → calling SpawnRow()");
            SpawnRow();
            timer = 0;
        }
    }

    void SpawnRow()
    {
        int rowCount = Random.Range(1, 3);

        List<float> usableSlots = allSlots.Except(ySlotHistory).ToList();
        if (usableSlots.Count < rowCount)
            rowCount = usableSlots.Count;

        for (int r = 0; r < rowCount; r++)
        {
            bool fromLeft = Random.value > 0.5f;
            float startX = fromLeft ? -11f : 11f;
            float direction = fromLeft ? 1f : -1f;

            int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

            int index = Random.Range(0, usableSlots.Count);
            float baseY = usableSlots[index];
            usableSlots.RemoveAt(index);

            if (!ySlotHistory.Contains(baseY))
            {
                ySlotHistory.Add(baseY);
                if (ySlotHistory.Count > 2)
                    ySlotHistory.RemoveAt(0);
            }

            float totalWidth = (enemyCount - 1) * spacingX;
            float originX = startX - direction * (totalWidth / 2f); // ✅ căn giữa hàng

            for (int i = 0; i < enemyCount; i++)
            {
                float xPos = originX + i * spacingX * direction;
                float yOffset = ((i - (enemyCount - 1) / 2f)) * spacingY;

                Vector3 spawnPos = new Vector3(xPos, baseY + yOffset, 0);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.GetComponent<EnemyController>().Initialize(direction * moveSpeed);
            }

            slotCooldown[baseY] = slotReuseDelay;
            availableSlots.Remove(baseY);

        }

        SpawnVerticalEnemies();
    }

    void SpawnVerticalEnemies()
    {
        for (int i = 0; i < 2; i++)
        {
            float camTopY = Camera.main.orthographicSize;
            float camRight = Camera.main.aspect * camTopY;

            bool spawnLeftHalf = (i == 0);
            float spawnX = spawnLeftHalf ? Random.Range(-camRight, 0f) : Random.Range(0f, camRight);
            float spawnY = camTopY + 1f;

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

            GameObject enemy = Instantiate(verticalEnemyPrefab, spawnPos, Quaternion.identity);

            Vector2 moveDir = spawnLeftHalf ? new Vector2(1f, -1f) : new Vector2(-1f, -1f);
            moveDir.Normalize();

            float diagonalSpeed = moveSpeed;

            enemy.GetComponent<VerticalEnemyController>().Initialize(moveDir * diagonalSpeed);
        }
    }
}
