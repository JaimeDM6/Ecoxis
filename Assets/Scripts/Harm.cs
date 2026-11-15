using UnityEngine;

public class Harm : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HarmPlayer(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HarmPlayer(collision.gameObject);
        }
    }

    private void HarmPlayer(GameObject jugador)
    {
        jugador.GetComponent<Personaje>().Hit();

        if (GameManager.instancia != null)
        {
            GameManager.instancia.PerderVida();
        }
    }
}