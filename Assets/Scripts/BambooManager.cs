using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;


public class BambooManager: MonoBehaviour
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
    public float pandaMoveSpeed = 1.0f;
    public int bambooPerSpawn = 5;
    public float bambooSpawnInterval = 10f;
    public int maxBambooToCollect = 30;
    public float normalSpeed = 1.0f;
    public float boostSpeed = 3.0f;

    private int collectedBamboos = 0;
    public float elapsedTime = 0f;
    private float spawnTimer = 0f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaDrainRate = 15f;
    public float staminaRegenRate = 10f;
    public float minStaminaToBoost = 10f;
    private bool isBoosting = false;
    private bool isRecovering = false;

    public Slider staminaSlider;

    private GameObject[] spawnedBamboos;
    private GameObject spawnedPanda;

    [Header("Map")]
    public BambooMapmaker mapmaker;

    [Header("Win UI")]
    public GameObject winTextUI;

    void Start()
    {
        SpawnBamboos();
        UpdateScoreUI();
        currentStamina = maxStamina;
        if (staminaSlider != null) staminaSlider.maxValue = maxStamina;
    }

    void Update()
    {
        HandlePandaSpawnAndMove();
        UpdateTimer();
        HandleBambooRespawn();
        UpdateStamina();

    }

    private void UpdateTimer() //อัปเดตเวลาในเกม
    {
        elapsedTime -= Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = "Time: " + elapsedTime.ToString("F1") + "s";
        }
    }
        
    private void HandleBambooRespawn() //Respawn ไผ่แต่ละรอบ ทุก ๆ 5 วิ
    {
        if (spawnTimer >= bambooSpawnInterval)
        {
            RemoveAllBamboos();
            if (collectedBamboos < maxBambooToCollect)
            {
                SpawnBamboos();
            }
            spawnTimer = 0f;
        }
    }

    private void RemoveAllBamboos() //ลบไผ่ทุกอันในเกม
    {
        if (spawnedBamboos != null)
        {
            foreach (var bamboo in spawnedBamboos)
            {
                if (bamboo != null)
                    Destroy(bamboo);
            }
        }
    }

    private void SpawnBamboos() //Spawm ไผ่่แบบสุ่มในระยะการมองเห็น
    {
        spawnedBamboos = new GameObject[bambooPerSpawn];
        List<ARPlane> planes = new List<ARPlane>();
        
        foreach (var plane in planeManager.trackables)
        {
            planes.Add(plane);
        }

        for (int i = 0; i < bambooPerSpawn; i++)
        {
            if (planes.Count == 0) break;

            ARPlane plane = planes[Random.Range(0, planes.Count)];
            Vector2 randomInPlane = Random.insideUnitCircle * 0.5f * plane.size.x;
            Vector3 randomPoint = plane.center + new Vector3(randomInPlane.x, 0, randomInPlane.y);
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector3 rayOrigin = randomPoint + Vector3.up * 1.5f;

            if (raycastManager.Raycast(new Ray(rayOrigin, Vector3.down), hits, TrackableType.PlaneWithinBounds) && hits.Count > 0)
            {
                Pose hitPose = hits[0].pose;
                GameObject bamboo = Instantiate(bambooPrefab, hitPose.position, Quaternion.identity);

                GameObject redDot = null;
                if (mapmaker != null)
                {
                    mapmaker.PlaceRedDotOnMap(bamboo.transform.position);
                    redDot = mapmaker.GetLastRedDot(); // ได้ red dot ล่าสุด
                }

                BambooObject bambooObj = bamboo.AddComponent<BambooObject>();
                bambooObj.manager = this;
                bambooObj.redDot = redDot;

                spawnedBamboos[i] = bamboo;

                
            }
        }
  }  
                public void CollectBamboo(GameObject bamboo) //เก็บไผ่ 
        {
            for (int i = 0; i < spawnedBamboos.Length; i++)
            {
                if (spawnedBamboos[i] == bamboo)
                {
                    
                    BambooObject bambooObj = bamboo.GetComponent<BambooObject>();
                    if (bambooObj != null && bambooObj.redDot != null)
                    {
                        Destroy(bambooObj.redDot);
                    }

                    Destroy(bamboo);
                    spawnedBamboos[i] = null;
                    collectedBamboos++;
                    UpdateScoreUI();
                    break;
                }
            }

            if (collectedBamboos >= maxBambooToCollect)
            {                
                RemoveAllBamboos();
                if (winTextUI != null)
                winTextUI.SetActive(true);

                StartCoroutine(ShowWinThenGoToCredits());
        }
}

            private void UpdateScoreUI() //อัปเดตสกอร์
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + collectedBamboos + " / " + maxBambooToCollect;
        }
            }

        private void HandlePandaSpawnAndMove() //Spawn แพนด้าแดงออกมา
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
            float speed = isBoosting ? boostSpeed : normalSpeed;
            spawnedPanda.transform.position = Vector3.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);


            Vector3 direction = (targetPosition - currentPosition).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                spawnedPanda.transform.rotation = Quaternion.Slerp(spawnedPanda.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        }

        private bool WasTapped() //บังคับการเดินแพนด้าแดง
        {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
        }
        private void UpdateStamina() //อัปเดต Stamina บูส
        {
        bool isPressing = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary);

        if (isPressing && !isRecovering)
        {
            isBoosting = true;
        }
        else
        {
            isBoosting = false;
        }

        if (isBoosting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isRecovering = true;
                isBoosting = false;
            }
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;

            if (currentStamina >= maxStamina)
            {
                currentStamina = maxStamina;
                isRecovering = false;
            }
        }

        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
        }
    
        private IEnumerator ShowWinThenGoToCredits()
        {
        yield return new WaitForSeconds(4f);  
        SceneManager.LoadScene("Credit");  
        }

}