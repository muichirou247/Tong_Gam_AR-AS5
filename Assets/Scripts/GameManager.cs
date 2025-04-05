using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARPlaneManager planeManager;

    private bool _gameStarted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIManager.OnUIStartButton += StartGame;
        UIManager.OnUIRestartButton += RestartGame;
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
}