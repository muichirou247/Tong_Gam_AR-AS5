using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class UIManager : MonoBehaviour
{
    [Header("Button Setup")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button shootButton;

    [Header("UI Setup")]
    [SerializeField] private TMP_Text greetingText;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image crosshair;

    public static event Action OnUIStartButton;
    public static event Action OnUIRestartButton;
    public static event Action OnUIShootButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startButton.onClick.AddListener(StartButtonPressed);
        restartButton.onClick.AddListener(RestartButtonPressed);
        shootButton.onClick.AddListener(ShootButtonPressed);

        restartButton.gameObject.SetActive(false);
        shootButton.gameObject.SetActive(false);
        crosshair.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
    }

    void StartButtonPressed()
    {
        OnUIStartButton?.Invoke();
        greetingText.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);
        shootButton.gameObject.SetActive(true);

        scoreText.gameObject.SetActive(true);
        crosshair.gameObject.SetActive(true);
    }

    void RestartButtonPressed()
    {
        OnUIRestartButton?.Invoke();
        greetingText.gameObject.SetActive(true);
        startButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(false);
        shootButton.gameObject.SetActive(false);

        crosshair.gameObject?.SetActive(false);
        scoreText.gameObject.SetActive(false);
    }

    void ShootButtonPressed() 
    { 
        OnUIShootButton?.Invoke();
    }

    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}