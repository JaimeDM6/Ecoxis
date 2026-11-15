using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DesertSeed : MonoBehaviour
{
    public DesertObjective objetivo;
    public AudioClip sonidoRecogida;
    public int puntos = 10;
    private bool recogido;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (objetivo == null) objetivo = UnityEngine.Object.FindAnyObjectByType<DesertObjective>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recogido) return;
        Personaje jugador = other.GetComponentInParent<Personaje>();
        if (jugador == null || !jugador.CompareTag("Player")) return;
        if (objetivo == null) objetivo = UnityEngine.Object.FindAnyObjectByType<DesertObjective>();
        if (objetivo == null || objetivo.Completado || objetivo.BrotesRecogidos >= objetivo.brotesNecesarios) return;

        recogido = true;
        GetComponent<Collider2D>().enabled = false;
        if (GameManager.instancia != null) GameManager.instancia.SumarPuntos(puntos);
        objetivo.RecogerBrote();
        AudioSource audio = jugador.GetComponent<AudioSource>();
        if (sonidoRecogida != null && audio != null) audio.PlayOneShot(sonidoRecogida, 0.6f);
        Destroy(gameObject);
    }
}
