using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public Vector2 boardSize = new Vector2(9, 9); // Assuming a 20x20 plane centered at 0,0
    public SnakeController snake;

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
        // Check distance to head
        if (Vector3.Distance(pos, snake.transform.position) < 1f) return false;

        // Check distance to body parts
        foreach (var body in snake.GetBodyParts())
        {
            if (Vector3.Distance(pos, body.transform.position) < 1f) return false;
        }

        return true;
    }
}
