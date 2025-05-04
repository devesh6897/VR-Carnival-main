using UnityEngine;
using System.Collections;

public class BulletShooter : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform gunSlider; // The sliding part of the gun

    [Header("Bullet Settings")]
    public float bulletSpeed = 20f;

    [Header("Slider Settings")]
    public float recoilDistance = 0.1f;
    public float recoilSpeed = 5f;

    private Vector3 sliderInitialPosition;

    void Start()
    {
        if (gunSlider != null)
        {
            sliderInitialPosition = gunSlider.localPosition;
        }
    }

    public void Fire()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Bullet Prefab or Fire Point not assigned.");
            return;
        }

        // Instantiate and shoot bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }
        else
        {
            Debug.LogWarning("Bullet prefab needs a Rigidbody component.");
        }

        // Start recoil
        if (gunSlider != null)
        {
            StopAllCoroutines();
            StartCoroutine(RecoilSlider());
        }
    }

    private IEnumerator RecoilSlider()
    {
        // Move back
        Vector3 backPosition = sliderInitialPosition - new Vector3(0, recoilDistance, 0);
        float elapsed = 0;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * recoilSpeed;
            gunSlider.localPosition = Vector3.Lerp(sliderInitialPosition, backPosition, elapsed);
            yield return null;
        }

        // Move forward
        elapsed = 0;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * recoilSpeed;
            gunSlider.localPosition = Vector3.Lerp(backPosition, sliderInitialPosition, elapsed);
            yield return null;
        }

        gunSlider.localPosition = sliderInitialPosition;
    }
}
