using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float delay = 0.6f;

    void Start()
    {
        Destroy(gameObject, delay); // huỷ sau animation nổ
    }
}
