using System.Collections;
using UnityEngine;

public class LaserWarningLine : MonoBehaviour
{
    public GameObject beamPrefab;
    public float delayBeforeFire = 1f;
    public float beamDuration = 1.5f;

    private Vector2 direction;
    private float length;

    public void Setup(Vector2 dir, float dist)
    {
        direction = dir;
        length = dist;

        // Scale warning line (assumes line scales on X)
        Vector3 localScale = transform.localScale;
        localScale.x = length;
        transform.localScale = localScale;

        StartCoroutine(FireBeamAfterDelay());
    }

    IEnumerator FireBeamAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeFire);

        Destroy(gameObject); // Remove warning line

        // Spawn beam at same position & rotation
        GameObject beam = Instantiate(beamPrefab, transform.position, transform.rotation);

        // Scale beam to match warning line
        Vector3 beamScale = beam.transform.localScale;
        beamScale.x = length;
        beam.transform.localScale = beamScale;

        Destroy(beam, beamDuration);
    }
}
