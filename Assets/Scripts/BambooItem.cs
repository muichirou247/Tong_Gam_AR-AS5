using UnityEngine;

public class BambooObject : MonoBehaviour
{
    public BambooManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.CollectBamboo(this.gameObject);
        }
    }
}