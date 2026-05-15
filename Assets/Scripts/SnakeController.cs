using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float steeringSpeed = 180f;
    public float bodySpeed = 5f;
    public float segmentSpacing = 1.1f;
    public int gap = 10;
    public GameObject bodyPrefab;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationsHistory = new List<Quaternion>();

    private FoodSpawner spawner;
    private float canDieTimer = 1f;
    private float distanceSinceLastRecord = 0f;
    private float recordStep;

    void Start()
    {
        recordStep = segmentSpacing / gap;

        positionsHistory.Add(transform.position);
        rotationsHistory.Add(transform.rotation);
        // Actualizado para evitar el warning CS0618
        spawner = Object.FindFirstObjectByType<FoodSpawner>();

        if (spawner == null) Debug.LogError("FoodSpawner no encontrado!");
    }

    void Update()
    {
        if (canDieTimer > 0) canDieTimer -= Time.deltaTime;

        float moveDist = moveSpeed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDist);

        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * steerDirection * steeringSpeed * Time.deltaTime);

        distanceSinceLastRecord += moveDist;
        while (distanceSinceLastRecord >= recordStep)
        {
            positionsHistory.Insert(0, transform.position);
            rotationsHistory.Insert(0, transform.rotation);
            distanceSinceLastRecord -= recordStep;
        }

        int index = 0;
        foreach (var body in bodyParts)
        {
            int histIndex = Mathf.Min(index * gap, positionsHistory.Count - 1);
            Vector3 point = positionsHistory[histIndex];
            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * bodySpeed * Time.deltaTime;
            body.transform.rotation = rotationsHistory[histIndex];
            index++;
        }

        int maxHistory = (bodyParts.Count + 1) * gap + 100;
        if (positionsHistory.Count > maxHistory)
        {
            positionsHistory.RemoveAt(positionsHistory.Count - 1);
            rotationsHistory.RemoveAt(rotationsHistory.Count - 1);
        }
    }

    public void Grow()
    {
        int histIndex = Mathf.Min(bodyParts.Count * gap, positionsHistory.Count - 1);
        GameObject body = Instantiate(bodyPrefab, positionsHistory[histIndex], rotationsHistory[histIndex]);
        bodyParts.Add(body);
        canDieTimer = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Plane") return;

        Debug.Log("Colisión con: " + other.name + " | Tag: " + other.tag);

        if (other.CompareTag("Food"))
        {
            Grow();
            Destroy(other.gameObject);
            if (spawner != null) spawner.SpawnFood();
            if (GameManager.Instance != null) GameManager.Instance.AddScore();
        }
        else if (other.CompareTag("Body") || other.CompareTag("Wall"))
        {
            if (canDieTimer <= 0)
            {
                if (other.CompareTag("Body"))
                {
                    int segIndex = bodyParts.IndexOf(other.gameObject);
                    if (segIndex >= 0 && segIndex < gap) return;
                }

                Debug.Log("MUERTE por choque con " + other.tag);
                if (GameManager.Instance != null) GameManager.Instance.GameOver();
            }
        }
    }

    public List<GameObject> GetBodyParts()
    {
        return bodyParts;
    }
}
