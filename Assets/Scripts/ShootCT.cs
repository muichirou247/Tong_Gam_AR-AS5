using UnityEngine;

public class ShootCT : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform cameraTransfrom;

    [Header("Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 3f;
    void Start()
    {
        UIManager.OnUIShootButton += Shoot;
    }

    
    void Update()
    {
        
    }

    void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, cameraTransfrom.position, Quaternion.identity);
        var rb = bullet.GetComponent<Rigidbody>();
        rb.AddForce(cameraTransfrom.forward * bulletSpeed, ForceMode.Impulse);
        Destroy(bullet, bulletLifeTime);
    }
}
