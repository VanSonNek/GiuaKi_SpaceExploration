using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    // Thời gian chờ giữa hai lần bắn, tính bằng giây.
    [Min(0.01f)]
    public float fireRate = 0.2f;

    private float nextFireTime;

    void Start()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Hãy gán Bullet Prefab và Fire Point cho PlayerShooting.", this);
        }
    }

    void Update()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        // Giữ Space để bắn liên tục theo fireRate.
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
    }
}
