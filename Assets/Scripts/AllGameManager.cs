using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor;

public class BambooManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Game Prefabs")]
    public GameObject bambooPrefab;
    public GameObject redPandaPrefab;
    public ReticleBehaviour reticle;

    [Header("UI")]
    public Text timerText;
    public Text scoreText;

    [Header("Game Settings")]
    public float timeLimit = 10f;
    public float pandaMoveSpeed = 1.0f;

    private float currentTime;
    private int collectedBamboos = 0;
    private int bambooCount = 5;

    private GameObject[] spawnedBamboos;
    private GameObject spawnedPanda;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public float gameOverDelay = 3f;


    void Start()
    {
        currentTime = timeLimit;
        SpawnBamboos();
        UpdateScoreUI();
        gameOverPanel.SetActive(false);
    
    }

    void Update()
    {
        HandlePandaSpawnAndMove();
        HandleTimer();       
    }

    // === 🐼 แพนด้าเดินตาม Reticle ===
    private void HandlePandaSpawnAndMove()
    {
        if (spawnedPanda == null && WasTapped() && reticle.CurrentPlane != null)
        {
            spawnedPanda = Instantiate(redPandaPrefab, reticle.transform.position, Quaternion.identity);
            spawnedPanda.tag = "Player";
        }

        if (spawnedPanda != null)
        {
            Vector3 targetPosition = reticle.transform.position;
            Vector3 currentPosition = spawnedPanda.transform.position;
            spawnedPanda.transform.position = Vector3.MoveTowards(currentPosition, targetPosition, pandaMoveSpeed * Time.deltaTime);

            Vector3 direction = (targetPosition - currentPosition).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                spawnedPanda.transform.rotation = Quaternion.Slerp(spawnedPanda.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }

    private bool WasTapped()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
    }

    // === 🕒 จัดการเวลา ===
    private void HandleTimer()
    {
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0, currentTime);

        if (timerText != null)
        {
            timerText.text = "Time Left: " + currentTime.ToString("F1") + "s";
        }

        if (currentTime <= 0)
        {
            StartCoroutine(HandleGameOver());
        }
    }

    // === 🎋 Spawn ไม้ไผ่ ===
    private void SpawnBamboos()
    {
        spawnedBamboos = new GameObject[bambooCount];

        // ✅ แก้ไขตรงนี้
        List<ARPlane> planes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            planes.Add(plane);
        }

        for (int i = 0; i < bambooCount; i++)
        {
            if (planes.Count == 0) break;

            ARPlane plane = planes[Random.Range(0, planes.Count)];
            Vector2 randomInPlane = Random.insideUnitCircle * 0.5f * plane.size.x;
            Vector3 randomPoint = plane.center + new Vector3(randomInPlane.x, 0, randomInPlane.y);
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector3 rayOrigin = randomPoint + Vector3.up * 1.5f;

            if (raycastManager.Raycast(new Ray(rayOrigin, Vector3.down), hits, TrackableType.PlaneWithinBounds))
            {
                Pose hitPose = hits[0].pose;
                GameObject bamboo = Instantiate(bambooPrefab, hitPose.position, Quaternion.identity);

                BambooObject bambooObj = bamboo.AddComponent<BambooObject>();
                bambooObj.manager = this;

                spawnedBamboos[i] = bamboo;
            }
        }

        bambooCount = Mathf.Max(1, 5 - collectedBamboos);
    }

    // === ✅ เรียกเมื่อชนไม้ไผ่ ===
    public void CollectBamboo(GameObject bamboo)
    {
        for (int i = 0; i < spawnedBamboos.Length; i++)
        {
            if (spawnedBamboos[i] == bamboo)
            {
                Destroy(bamboo);
                spawnedBamboos[i] = null;
                collectedBamboos++;
                UpdateScoreUI();
                break;
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + collectedBamboos;
        }
    }

    public ARSession arSession;

    private IEnumerator HandleGameOver()
    {
        if (timerText != null)
            timerText.text = "YOU LOSE";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // ⛔ หยุดเกมทั้งหมด

        yield return new WaitForSecondsRealtime(gameOverDelay); // ต้องใช้ Realtime เพราะ Time.timeScale = 0

        Time.timeScale = 1f; // ✅ คืนค่าปกติก่อนโหลดใหม่

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



}
