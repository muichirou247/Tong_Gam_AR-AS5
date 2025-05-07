using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;

    [Header("Score UI")]
    [SerializeField] private Text scoreText;

    public static event Action OnUIStartButton;
    public static event Action OnUIRestartButton;

    void Start()
    {
        startButton.onClick.AddListener(StartButtonPressed);
        restartButton.onClick.AddListener(RestartButtonPressed);

        restartButton.gameObject.SetActive(false);  // ซ่อนไว้ตอนเริ่ม
        scoreText.gameObject.SetActive(false);      // ซ่อนไว้ตอนเริ่ม
    }

    void StartButtonPressed()
    {
        OnUIStartButton?.Invoke();

        startButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
    }

    void RestartButtonPressed()
    {
        OnUIRestartButton?.Invoke();

        startButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}
