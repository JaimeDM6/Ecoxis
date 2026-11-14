using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BolaNieve : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadDerecha = 2f;
    public float velocidadRotacion = 400f;

    [Header("Inmunidad")]
    [Tooltip("Segundos que ignorará las trampas al nacer para poder bajar la rampa")]
    public float tiempoInmunidad = 1.5f;

    private Rigidbody2D rb;
    private bool hill = false;
    private float tiempoNacimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        tiempoNacimiento = Time.time;

        Destroy(gameObject, 15f);
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 0, -velocidadRotacion * Time.fixedDeltaTime);

        if (!hill)
        {
            rb.linearVelocity = new Vector2(velocidadDerecha, rb.linearVelocity.y);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < 0.9f && contact.normal.y > 0.1f)
            {
                hill = true;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Personaje>().Hit();

            if (GameManager.instancia != null)
                GameManager.instancia.PerderVida();

            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Trampa"))
        {
            if (Time.time > tiempoNacimiento + tiempoInmunidad)
            {
                Destroy(gameObject);
            }
        }
    }
}