using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public ObstacleSpawner obstacleSpawner;

    private int score = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Time.timeScale = 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        else
            Debug.LogWarning("GameOverPanel no asignado en el GameManager");
            
        UpdateScoreUI();
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI();

        if (score % 5 == 0)
        {
            if (obstacleSpawner != null)
                obstacleSpawner.SpawnObstacle();
            else
                Debug.LogWarning("ObstacleSpawner no asignado en el GameManager");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        Time.timeScale = 0;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOver: panel activado correctamente");
        }
        else
        {
            Debug.LogError("GameOver: gameOverPanel es NULL, asígnalo en el Inspector");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
