using UnityEngine;

public class GeneradorBolas : MonoBehaviour
{
    public GameObject prefabBola;
    public float tiempoAparicion = 10f;

    void Start()
    {
        InvokeRepeating(nameof(CrearBola), 0f, tiempoAparicion);
    }

    void CrearBola()
    {
        Instantiate(prefabBola, transform.position, Quaternion.identity);
    }
}