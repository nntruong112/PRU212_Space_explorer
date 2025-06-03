using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{

    public GameObject[] largeAsteroid;
    public GameObject[] mediumAsteroid;
    public GameObject[] smallAsteroid;
    public float baseSpawnInterval = 2f;

    private float timer;

    void Start()
    {
        
    }

    
    void Update()
    {
        timer += Time.deltaTime;

        float spawnInterval = baseSpawnInterval / (1f + GameManager.Instance.difficultyLevel * 0.5f);

        if (timer >= spawnInterval)
        {
            SpawnAsteroid();
            timer = 0;
        }
    }

    void SpawnAsteroid()
    {

        Vector3 leftTop = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
        Vector3 rightTop = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, Camera.main.nearClipPlane));

        float x = Random.Range(leftTop.x, rightTop.x);
        float y = leftTop.y + 1f;

        Vector2 spawnPos = new Vector2(x, y);

        GameObject prefab = ChooseAsteroidPrefab();
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    GameObject ChooseAsteroidPrefab()
    {
        int level = GameManager.Instance.difficultyLevel;

        float random = Random.value;

        switch (level)
        {
            case 1: // Level 1: 70% Small, 25% Medium, 5% Large
                if (random < 0.7f) 
                    return smallAsteroid[Random.Range(0, smallAsteroid.Length)];
                else if (random < 0.95f)
                    return mediumAsteroid[Random.Range(0, mediumAsteroid.Length)];
                else
                    return largeAsteroid[Random.Range(0, largeAsteroid.Length)];
            case 2: // Level 2: 50% Small, 35% Medium, 15% Large
                if (random < 0.5f)
                    return smallAsteroid[Random.Range(0, smallAsteroid.Length)];
                else if (random < 0.85f)
                    return mediumAsteroid[Random.Range(0, mediumAsteroid.Length)];
                else
                    return largeAsteroid[Random.Range(0, largeAsteroid.Length)];
            case 3: // Level 3: 40% Small, 40% Medium, 20% Large
                if (random < 0.4f)
                    return smallAsteroid[Random.Range(0, smallAsteroid.Length)];
                else if (random < 0.8f)
                    return mediumAsteroid[Random.Range(0, mediumAsteroid.Length)];
                else
                    return largeAsteroid[Random.Range(0, largeAsteroid.Length)];
            default: //Level 0: 80% Small, 20% Medium, 0% Large
                if (random < 0.8f)
                    return smallAsteroid[Random.Range(0, smallAsteroid.Length)];
                else
                    return mediumAsteroid[Random.Range(0, mediumAsteroid.Length)];
        }
    }
}
