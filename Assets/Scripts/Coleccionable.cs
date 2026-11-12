using UnityEngine;

public class Coleccionable : MonoBehaviour
{
    [Header("Efecto Flotante")]
    public float velocidadFlotar = 2f;
    public float amplitudFlotar = 0.15f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float nuevoY = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotar) * amplitudFlotar;
        transform.position = new Vector3(posicionInicial.x, nuevoY, posicionInicial.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // GameManager.instancia.SumarCopo();
            Destroy(gameObject);
        }
    }
}