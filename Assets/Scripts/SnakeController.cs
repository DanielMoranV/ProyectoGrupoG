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
    public GameObject efectoParticulas;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Collider> bodyPhysicsColliders = new List<Collider>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private List<Quaternion> rotationsHistory = new List<Quaternion>();

    private FoodSpawner spawner;
    private Collider headCollider;
    private float canDieTimer = 1f;
    private float distanceSinceLastRecord = 0f;
    private float recordStep;

    void Start()
    {
        recordStep = segmentSpacing / gap;

        positionsHistory.Add(transform.position);
        rotationsHistory.Add(transform.rotation);
        spawner = Object.FindFirstObjectByType<FoodSpawner>();
        headCollider = GetComponent<Collider>();

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

            if (index < bodyPhysicsColliders.Count)
                ResolveBodyWallPenetration(body, bodyPhysicsColliders[index]);

            index++;
        }

        int maxHistory = (bodyParts.Count + 1) * gap + 100;
        if (positionsHistory.Count > maxHistory)
        {
            positionsHistory.RemoveAt(positionsHistory.Count - 1);
            rotationsHistory.RemoveAt(rotationsHistory.Count - 1);
        }
    }

    public float speedIncrease = 0.8f;
    public float maxMoveSpeed = 12f;

    public void Grow()
    {
        int histIndex = Mathf.Min(bodyParts.Count * gap, positionsHistory.Count - 1);
        GameObject body = Instantiate(bodyPrefab, positionsHistory[histIndex], rotationsHistory[histIndex]);
        bodyParts.Add(body);

        // Añadir un BoxCollider no-trigger para depenetración física con muros.
        // El trigger original sigue siendo el que detecta colisiones de juego (Body, Food, etc).
        BoxCollider triggerCol = body.GetComponent<BoxCollider>();
        BoxCollider physCol = body.AddComponent<BoxCollider>();
        if (triggerCol != null)
        {
            physCol.center = triggerCol.center;
            physCol.size = triggerCol.size;
        }
        physCol.isTrigger = false;

        // Evitar que la cabeza sea bloqueada físicamente por este collider
        if (headCollider != null)
            Physics.IgnoreCollision(headCollider, physCol);

        bodyPhysicsColliders.Add(physCol);

        moveSpeed += speedIncrease;
        bodySpeed += speedIncrease;

        moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
        bodySpeed = Mathf.Min(bodySpeed, maxMoveSpeed);

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
            Vector3 position = other.transform.position;
            if (efectoParticulas != null)
            {
                GameObject particulas = Instantiate(efectoParticulas, position, Quaternion.identity);
                Destroy(particulas, 2f);
            }
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

    private void ResolveBodyWallPenetration(GameObject body, Collider physCol)
    {
        Collider[] overlaps = Physics.OverlapBox(
            physCol.bounds.center,
            physCol.bounds.extents + Vector3.one * 0.1f,
            body.transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (var wall in overlaps)
        {
            if (!wall.CompareTag("Wall") || wall.gameObject == body) continue;

            Vector3 dir;
            float dist;
            if (Physics.ComputePenetration(
                    physCol, body.transform.position, body.transform.rotation,
                    wall, wall.transform.position, wall.transform.rotation,
                    out dir, out dist))
            {
                body.transform.position += dir * (dist + 0.02f);
            }
        }
    }
}
