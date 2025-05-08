using UnityEngine;
using System.Collections;

public class BambooObject : MonoBehaviour
{
    public BambooManager manager;

    public AudioClip collectSound; 
    public GameObject collectEffect; 
    public GameObject redDot; 


    private bool isCollected = false;

    private void OnTriggerEnter(Collider other) //เช็คการชนผู้เล่นกับไผ่
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            StartCoroutine(CollectWithEffect());
        }
    }

    private IEnumerator CollectWithEffect() //ชนแล้วให้มีการกระพริบละเก็บ
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

        
        if (collectSound != null) //เล่นเสียง
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

                
        if (manager != null) //เก็บไผ่และอัปเดตสกอร์
        {
            manager.CollectBamboo(gameObject);
        }

        Destroy(gameObject); // ลบไผ่หลังจากกระพริบเสร็จ
    }

}
