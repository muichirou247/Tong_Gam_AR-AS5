using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARPlaneManager planeManager;

    [SerializeField] private GameObject enemyPrefab;

    [Header("Enemy Settings")]
    [SerializeField] private int enemyCount = 2;
    [SerializeField] private float spawnRate = 2.0f;
    [SerializeField] private float deSpawnRate = 4.0f;

    private List<GameObject> _spawnedEnemies = new List<GameObject>();
    private int _score = 0;

    private bool _gameStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void StartGame()
    {
        if (_gameStarted) return;
        _gameStarted = true;

        planeManager.enabled = false;
        foreach (var plane in planeManager.trackables)
        {
            var meshVisual = GetComponent<ARFaceMeshVisualizer>();
            if (meshVisual) meshVisual.enabled = false;

            var lineVisual = GetComponent<LineRenderer>();
            if (lineVisual) lineVisual.enabled = false;
        }

        
    }

    void RestartGame()
    {
        print("Restarted!");
        _gameStarted = false;
        arSession.Reset();
        planeManager.enabled = true;
    }

    void SpawnEnemy()
    {
        if (planeManager.trackables.count == 0) return;
        List<ARPlane> plane = new List<ARPlane>();
        foreach (var planes in planeManager.trackables)
        {
            plane.Add(planes);
        }

        var randomPlane = plane[Random.Range(0, plane.Count)];
        var randomPlanePosition = GetRandomPosition(randomPlane);

        var enemy = Instantiate(enemyPrefab, randomPlanePosition, Quaternion.identity);
        _spawnedEnemies.Add(enemy);



        Vector3 GetRandomPosition(ARPlane plane)
        {
            var center = plane.center;
            var size = plane.size * 0.35f;
            var randomX = Random.Range(-size.x, size.x);

            var randomZ = Random.Range(-size.y, size.y);


            return new Vector3(center.x + randomX, center.y, center.z + randomZ);
        }

        IEnumerator SpawnEnemies()
        {
            while (_gameStarted)
            {
                if (_spawnedEnemies.Count < enemyCount)
                {
                    SpawnEnemy();
                }

                yield return new WaitForSeconds(spawnRate);
            }
        }

        IEnumerator DeSpawnEnemies(GameObject enemy)
        {
            yield return new WaitForSeconds(deSpawnRate);

            if (_spawnedEnemies.Contains(enemy))
            {
                _spawnedEnemies.Remove(enemy);
                Destroy(enemy);
            }
        }

        void AddScore()
        {


        }
    }
}