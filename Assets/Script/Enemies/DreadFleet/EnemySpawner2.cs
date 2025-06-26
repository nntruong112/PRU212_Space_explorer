using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner2 : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    public GameObject bulletPrefab; // Prefab đạn

    public int columns = 7;
    public int rows = 5;
    public float spacing = 1.5f;
    public float rowSpacing = 1.2f;
    public float spawnDelay = 0.2f;
    public float moveSpeed = 4f;
    public float baseY = 4f;
    public float waveAmplitudeX = 2f;
    public float waveFrequencyX = 1f;
    public float waveAmplitudeY = 0.5f;
    public float waveFrequencyY = 0.8f;
    public float minShootInterval = 1f; // Thời gian tối thiểu giữa các lần bắn
    public float maxShootInterval = 3f; // Thời gian tối đa giữa các lần bắn
    public float bulletSpeed = 5f; // Tốc độ đạn

    private Transform formationRoot;
    private bool formationComplete = false;
    private int enemiesSpawned = 0;
    private int enemiesInPosition = 0;
    private List<GameObject> enemies = new List<GameObject>();

    private bool isActive = true;

    void Awake()
    {
        formationRoot = new GameObject("FormationRoot").transform;
    }

    void Start()
    {
        if (isActive)
            StartCoroutine(SpawnWavesForTwoMinutes());
    }

    public void EnableSpawning(bool enable)
    {
        isActive = enable;

        if (!enable)
        {
            StopAllCoroutines();
            enemies.Clear();
            formationComplete = false;
            Debug.Log("EnemySpawner2 đã dừng hoạt động.");
        }
    }

    IEnumerator SpawnWavesForTwoMinutes()
    {
        float duration = 120f; // 2 phút
        float timer = 0f;

        while (timer < duration && isActive)
        {
            yield return StartCoroutine(SpawnInBatches());

            // Đợi đến khi toàn bộ enemy bị tiêu diệt
            while (enemies.Exists(e => e != null))
            {
                if (!isActive) yield break;
                yield return null;
            }

            formationComplete = false;
            formationRoot.position = Vector3.zero;

            yield return new WaitForSeconds(1f); // nghỉ 1 chút trước đợt mới

            timer += Time.deltaTime; // tiếp tục tính thời gian
        }

        Debug.Log("Spawn kết thúc sau 2 phút.");
    }

    void Update()
    {
        if (formationComplete)
        {
            float xOffset = Mathf.Sin(Time.time * waveFrequencyX) * waveAmplitudeX;
            float yOffset = Mathf.Cos(Time.time * waveFrequencyY) * waveAmplitudeY;
            formationRoot.position = new Vector3(xOffset, yOffset, 0);
        }
    }

    IEnumerator SpawnInBatches()
    {
        enemiesSpawned = 0;
        enemiesInPosition = 0;
        enemies.Clear();

        for (int row = 0; row < rows; row += 2)
        {
            Coroutine row1 = StartCoroutine(SpawnRow(row));
            Coroutine row2 = null;
            if (row + 1 < rows)
                row2 = StartCoroutine(SpawnRow(row + 1));

            float waitTime = columns * spawnDelay + 1f;
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator SpawnRow(int rowIndex)
    {
        float screenCenterX = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)).x;
        bool spawnFromLeft = rowIndex % 2 == 0;
        Transform spawnPoint = spawnFromLeft ? leftSpawnPoint : rightSpawnPoint;

        for (int col = 0; col < columns; col++)
        {
            if (!isActive) yield break;

            int actualCol = spawnFromLeft ? (columns - 1 - col) : col;
            float xOffset = ((float)actualCol - ((columns - 1f) / 2f)) * spacing;
            float yOffset = rowIndex * rowSpacing;
            Vector3 targetPos = new Vector3(screenCenterX + xOffset, baseY + yOffset, 0);

            GameObject enemy = Instantiate(enemyPrefab);
            enemy.transform.parent = formationRoot;
            enemiesSpawned++;
            enemies.Add(enemy);

            Enemy3Movement enemyMovement = enemy.GetComponent<Enemy3Movement>();
            enemyMovement.Init(spawnPoint.position, targetPos, moveSpeed);
            enemyMovement.OnReachedTarget += () =>
            {
                enemiesInPosition++;

                // Cập nhật số lượng enemy còn sống
                int aliveEnemies = enemies.FindAll(e => e != null).Count;

                // Nếu tất cả các enemy còn sống đã vào đúng vị trí
                if (!formationComplete && enemiesInPosition >= aliveEnemies)
                {
                    formationComplete = true;
                    StartCoroutine(ShootRandomly());
                }
            };

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    IEnumerator ShootRandomly()
    {
        while (formationComplete)
        {
            if (!isActive) yield break;

            enemies.RemoveAll(enemy => enemy == null); // Xoá enemy bị destroy
            if (enemies.Count == 0) yield break;

            if (enemies.Count > 0)
            {
                int shotsThisRound = Random.Range(5, 10); // Số lượng enemy bắn trong đợt
                List<GameObject> alreadyShot = new List<GameObject>();

                for (int i = 0; i < shotsThisRound; i++)
                {
                    GameObject randomEnemy = enemies[Random.Range(0, enemies.Count)];

                    if (randomEnemy != null && !alreadyShot.Contains(randomEnemy))
                    {
                        ShootBullet(randomEnemy.transform);
                        alreadyShot.Add(randomEnemy);

                        // Thêm delay ngẫu nhiên giữa từng lần bắn
                        float smallDelay = Random.Range(0.1f, 0.5f);
                        yield return new WaitForSeconds(smallDelay);
                    }
                }
            }

            // Delay giữa các đợt bắn
            float shootDelay = Random.Range(minShootInterval, maxShootInterval);
            yield return new WaitForSeconds(shootDelay);
        }
    }

    void ShootBullet(Transform enemyTransform)
    {
        // Tìm FirePoint trong enemy
        Transform firePoint = enemyTransform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not found on enemy: " + enemyTransform.name);
            return; // Bỏ qua nếu không tìm thấy FirePoint
        }

        // Tạo đạn từ vị trí FirePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        VerticalEnemyBulletController bulletController = bullet.GetComponent<VerticalEnemyBulletController>();
        if (bulletController == null)
        {
            Debug.LogError("EnemyBulletController NOT found on bullet: " + bullet.name);
            return;
        }
        bulletController.direction = Vector2.down; // Bắn xuống dưới
        bulletController.speed = bulletSpeed;
    }
}