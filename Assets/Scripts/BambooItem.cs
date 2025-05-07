using UnityEngine;
using System.Collections;

public class BambooObject : MonoBehaviour
{
    public BambooManager manager;

    public AudioClip collectSound; // ใส่เสียงเก็บถ้ามี
    public GameObject collectEffect; // ใส่พาร์ติเคิลถ้ามี

    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            StartCoroutine(CollectWithEffect());
        }
    }

    private IEnumerator CollectWithEffect()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        int flashCount = 5;
        float flashInterval = 0.1f;

        for (int i = 0; i < flashCount; i++)
        {
            foreach (var rend in renderers) rend.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            foreach (var rend in renderers) rend.enabled = true;
            yield return new WaitForSeconds(flashInterval);
        }

        // เล่นเสียงถ้ามี
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // สร้างพาร์ติเคิลถ้ามี
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // อัปเดตคะแนนผ่าน manager
        if (manager != null)
        {
            manager.CollectBamboo(gameObject);
        }

        Destroy(gameObject); // ลบหลังจากกระพริบเสร็จ
    }
}
