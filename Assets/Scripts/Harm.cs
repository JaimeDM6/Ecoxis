using UnityEngine;

public class Harm : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Golpear(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Golpear(collision);
    }

    private void Golpear(Collider2D otro)
    {
        Personaje jugador = otro.GetComponentInParent<Personaje>();
        if (jugador == null || !jugador.CompareTag("Player")) return;
        GameManager manager = GameManager.instancia;
        if (manager != null && !manager.NivelActivo) return;
        if (jugador.TryHit() && manager != null) manager.PerderVida();
    }
}
