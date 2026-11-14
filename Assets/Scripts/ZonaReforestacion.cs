using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ZonaReforestacion : MonoBehaviour
{
    public MisionesSelva misiones;

    private SpriteRenderer imagen;
    private bool jugadorCerca;
    private bool recuperada;

    void Awake()
    {
        imagen = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (jugadorCerca && !recuperada && Input.GetKeyDown(KeyCode.E))
        {
            if (misiones == null)
            {
                Debug.LogWarning("Falta asignar Misiones en esta zona.", this);
                return;
            }

            recuperada = true;
            imagen.color = Color.green;
            misiones.RegistrarZona();
        }
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponentInParent<Personaje>() != null)
            jugadorCerca = true;
    }

    void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.GetComponentInParent<Personaje>() != null)
            jugadorCerca = false;
    }
}