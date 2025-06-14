﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class BambooManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    [Header("Game Prefabs")]
    public GameObject bambooPrefab;
    public GameObject redPandaPrefab;
    public GameObject timeItemPrefab; // ไอเท็มเพิ่มเวลา
    public ReticleBehaviour reticle;

    [Header("UI")]
    public Text timerText;
    public Text scoreText;

    [Header("Game Settings")]
    public float pandaMoveSpeed = 1.0f;
    public int bambooPerSpawn = 5;
    public int timeItemPerSpawn = 2; // จำนวนไอเท็มเพิ่มเวลาที่สุ่มเกิด
    public float bambooSpawnInterval = 10f;
    public int maxBambooToCollect = 30;
    public float normalSpeed = 1.0f;
    public float boostSpeed = 3.0f;
    public float timeBonusAmount = 15f; // เวลาที่เพิ่มเมื่อเก็บไอเท็ม

    private int collectedBamboos = 0;
    public float elapsedTime = 60f; // ตั้งเวลาตั้งต้น 60 วิ
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
    private GameObject[] spawnedTimeItems;

    [Header("Map")]
    public BambooMapmaker mapmaker;

    [Header("Win UI")]
    public GameObject winTextUI;

    [Header("Audio")]
    public AudioClip timeItemCollectSound;
    public AudioClip walkingClip;
    public AudioClip runningClip;
    private AudioSource audioSource;
    private AudioSource footstepSource;
    void Start()
    {
        SpawnBamboos();
        SpawnTimeItems();
        UpdateScoreUI();
        currentStamina = maxStamina;
        if (staminaSlider != null) staminaSlider.maxValue = maxStamina;
        audioSource = gameObject.AddComponent<AudioSource>();
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.loop = true; // ให้เล่นวน
    }

    void Update()
    {
        HandlePandaSpawnAndMove();
        UpdateTimer();
        HandleBambooRespawn();
        UpdateStamina();
        UpdateFootstepAudio();
    }

    private void UpdateTimer()
    {
        elapsedTime -= Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Max(elapsedTime, 0f).ToString("F1") + "s";
        }

        if (elapsedTime <= 0)
        {
            // เวลาหมดแล้ว จบเกม
            if (winTextUI != null)
            {
                winTextUI.SetActive(true);
                winTextUI.GetComponent<Text>().text = "Time's Up! Game Over";
            }
            StartCoroutine(ShowWinThenGoToCredits());
        }
    }

    private void HandleBambooRespawn()
    {
        if (spawnTimer >= bambooSpawnInterval)
        {
            RemoveAllBamboos();
            RemoveAllTimeItems();

            if (collectedBamboos < maxBambooToCollect)
            {
                SpawnBamboos();
                SpawnTimeItems();
            }

            spawnTimer = 0f;
        }
    }

    private void RemoveAllBamboos()
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

    private void RemoveAllTimeItems()
    {
        if (spawnedTimeItems != null)
        {
            foreach (var item in spawnedTimeItems)
            {
                if (item != null)
                    Destroy(item);
            }
        }
    }

    private void SpawnBamboos()
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
                    redDot = mapmaker.GetLastRedDot();
                }

                BambooObject bambooObj = bamboo.AddComponent<BambooObject>();
                bambooObj.manager = this;
                bambooObj.redDot = redDot;

                spawnedBamboos[i] = bamboo;
            }
        }
    }

    private void SpawnTimeItems()
    {
        spawnedTimeItems = new GameObject[timeItemPerSpawn];
        List<ARPlane> planes = new List<ARPlane>();

        foreach (var plane in planeManager.trackables)
        {
            planes.Add(plane);
        }

        for (int i = 0; i < timeItemPerSpawn; i++)
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
                GameObject timeItem = Instantiate(timeItemPrefab, hitPose.position, Quaternion.identity);

                // เพิ่ม component จัดการเก็บไอเท็มเวลา
                TimeItemObject timeItemObj = timeItem.AddComponent<TimeItemObject>();
                timeItemObj.manager = this;

                spawnedTimeItems[i] = timeItem;
            }
        }
    }

    public void CollectBamboo(GameObject bamboo)
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
            RemoveAllTimeItems();

            if (winTextUI != null)
                winTextUI.SetActive(true);

            StartCoroutine(ShowWinThenGoToCredits());
        }
    }

    public void CollectTimeItem(GameObject timeItem)
    {
        // ตรวจสอบว่ามีไอเท็มนี้ใน spawnedTimeItems จริง ๆ
        for (int i = 0; i < spawnedTimeItems.Length; i++)
        {
            if (spawnedTimeItems[i] == timeItem)
            {
                Destroy(timeItem);
                spawnedTimeItems[i] = null;

                elapsedTime += timeBonusAmount;
                Debug.Log("Collected time item! Added " + timeBonusAmount + " seconds.");
                if (timeItemCollectSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(timeItemCollectSound);
                }
                return;
            }
        }

        // ถ้าไม่เจอในลิสต์ อาจโดนเก็บซ้ำ หรือถูกลบไปก่อน
        Debug.LogWarning("Attempted to collect a time item that was already removed.");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + collectedBamboos + " / " + maxBambooToCollect;
        }
    }

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

    private bool WasTapped()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
    }

    private void UpdateStamina()
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
    private void UpdateFootstepAudio()
    {
        if (spawnedPanda == null) return;

        float distance = Vector3.Distance(spawnedPanda.transform.position, reticle.transform.position);

        // ถ้ากำลังเคลื่อนไหว
        if (distance > 0.05f)
        {
            AudioClip clipToPlay = isBoosting ? runningClip : walkingClip;

            if (footstepSource.clip != clipToPlay)
            {
                footstepSource.clip = clipToPlay;
                footstepSource.Play();
            }
        }
        else
        {
            // หยุดเดิน หยุดเสียง
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }
}
