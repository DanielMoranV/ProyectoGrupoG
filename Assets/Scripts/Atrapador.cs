using UnityEngine;

public class Atrapador : MonoBehaviour
{
    int puntos = 0;
    public GameObject TextoVictoria;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Objetivo"))
        {
            Destroy(other.gameObject);
            puntos++;

            if (puntos >= 4)
            {
                TextoVictoria.SetActive(true);
            }
        }
    }
}
