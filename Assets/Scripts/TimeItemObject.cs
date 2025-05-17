using UnityEngine;

public class TimeItemObject : MonoBehaviour
{
    public BambooManager manager;
    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return; // ป้องกันเก็บซ้ำ

        if (other.CompareTag("Player"))
        {
            collected = true;
            manager.CollectTimeItem(this.gameObject);
        }
    }
}