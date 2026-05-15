using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float steeringSpeed = 180f;
    public float bodySpeed = 5f;
    public int gap = 10;
    public GameObject bodyPrefab;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();

    private FoodSpawner spawner;
    private float canDieTimer = 1f;

    void Start()
    {
        positionsHistory.Add(transform.position);
        // Actualizado para evitar el warning CS0618
        spawner = Object.FindFirstObjectByType<FoodSpawner>();

        if (spawner == null) Debug.LogError("FoodSpawner no encontrado!");
    }

    void Update()
    {
        if (canDieTimer > 0) canDieTimer -= Time.deltaTime;

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        float steerDirection = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * steerDirection * steeringSpeed * Time.deltaTime);

        positionsHistory.Insert(0, transform.position);

        int index = 0;
        foreach (var body in bodyParts)
        {
            Vector3 point = positionsHistory[Mathf.Min(index * gap, positionsHistory.Count - 1)];
            Vector3 moveDirection = point - body.transform.position;
            body.transform.position += moveDirection * bodySpeed * Time.deltaTime;
            body.transform.LookAt(point);
            index++;
        }

        if (positionsHistory.Count > (bodyParts.Count + 1) * gap + 100)
        {
            positionsHistory.RemoveAt(positionsHistory.Count - 1);
        }
    }

    public void Grow()
    {
        GameObject body = Instantiate(bodyPrefab, positionsHistory[Mathf.Min(bodyParts.Count * gap, positionsHistory.Count - 1)], Quaternion.identity);
        bodyParts.Add(body);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar el suelo
        if (other.name == "Plane") return;

        // Debug para ver con qué estamos chocando exactamente
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
            // Evitar morir con la parte del cuerpo que acaba de nacer
            // Las partes del cuerpo nuevas tardan un poco en ser peligrosas
            if (canDieTimer <= 0)
            {
                // Solo morimos si chocamos con una parte que ya se movió de la cabeza
                if (other.CompareTag("Body"))
                {
                    // Si el objeto está muy cerca de la cabeza, ignoramos (es el cuello)
                    if (Vector3.Distance(transform.position, other.transform.position) < 0.5f) return;
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
