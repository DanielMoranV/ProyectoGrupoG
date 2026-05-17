using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Vector2 boardSize = new Vector2(9, 9);
    public SnakeController snake;

    public float minDistanceFromSnake = 2f;
    public float minDistanceFromObstacle = 1.5f;

    private List<GameObject> obstacles = new List<GameObject>();

    public void SpawnObstacle()
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
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            obstacle.tag = "Obstacle";
            obstacles.Add(obstacle);
        }
    }

    public bool IsPositionValid(Vector3 pos)
    {
        if (Vector3.Distance(pos, snake.transform.position) < minDistanceFromSnake)
            return false;

        foreach (var body in snake.GetBodyParts())
        {
            if (Vector3.Distance(pos, body.transform.position) < minDistanceFromSnake)
                return false;
        }

        foreach (var obstacle in obstacles)
        {
            if (Vector3.Distance(pos, obstacle.transform.position) < minDistanceFromObstacle)
                return false;
        }

        return true;
    }

    public List<GameObject> GetObstacles()
    {
        return obstacles;
    }
}