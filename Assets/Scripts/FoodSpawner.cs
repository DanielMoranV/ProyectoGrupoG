using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public Vector2 boardSize = new Vector2(9, 9); // Assuming a 20x20 plane centered at 0,0
    public SnakeController snake;
    public ObstacleSpawner obstacleSpawner;

    public float minDistanceFromSnake = 1f;
    public float minDistanceFromObstacle = 1.5f;

    void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        Vector3 spawnPos;
        bool validPosition = false;
        int attempts = 0;

        do
        {
            float x = Random.Range(-boardSize.x, boardSize.x);
            float z = Random.Range(-boardSize.y, boardSize.y);
            spawnPos = new Vector3(x, 0.5f, z);

            validPosition = IsPositionValid(spawnPos);
            attempts++;
        } while (!validPosition && attempts < 100);

        if (validPosition)
        {
            Instantiate(foodPrefab, spawnPos, Quaternion.identity);
        }
    }

    bool IsPositionValid(Vector3 pos)
    {
         if (snake != null)
        {
            if (Vector3.Distance(pos, snake.transform.position) < minDistanceFromSnake) return false;

            foreach (var body in snake.GetBodyParts())
            {
                if (Vector3.Distance(pos, body.transform.position) < minDistanceFromSnake) return false;
            }
        }

        if (obstacleSpawner != null)
        {
            foreach (var obstacle in obstacleSpawner.GetObstacles())
            {
                if (Vector3.Distance(pos, obstacle.transform.position) < minDistanceFromObstacle)
                    return false;
            }
        }

        return true;
    }
}
