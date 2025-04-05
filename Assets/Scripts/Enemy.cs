    using UnityEngine;
    using System;
    //using UnityEditor.XR.Management;

public class Enemy : MonoBehaviour
{
    public event Action OnEnemyDestroyed;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            OnEnemyDestroyed?.Invoke();            
            Destroy(gameObject);
            
        }
    }
}
