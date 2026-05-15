using UnityEngine;
using UnityEngine.SceneManagement;

public class MoverPlayer : MonoBehaviour
{
    public float velocidad = 5f;
    public float sensibilidadMouse = 2f;
    public float fuerzaSalto = 5f;
    public Camera camaraFPS;

    private Rigidbody rb;
    private float rotacionX = 0f;
    private bool estaEnSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Bloqueamos el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;

        if (camaraFPS == null)
            camaraFPS = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Rotación con el Mouse (Cámara)
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        camaraFPS.transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Salto
        //if (Input.GetButtonDown("Jump") && estaEnSuelo)
        //{
        //    rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        //    estaEnSuelo = false;
        //}
    }

    void FixedUpdate()
    {
        // Movimiento relativo a la orientación del jugador
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movimiento = (transform.right * moveX + transform.forward * moveZ) * velocidad;
        rb.linearVelocity = new Vector3(movimiento.x, rb.linearVelocity.y, movimiento.z);
    }


}
