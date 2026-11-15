using UnityEngine;

/// <summary>
/// Activador de la estación final. La victoria requiere recoger todos los brotes.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DesertGoal : MonoBehaviour
{
    public DesertObjective objetivo;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (objetivo == null) objetivo = UnityEngine.Object.FindAnyObjectByType<DesertObjective>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (objetivo != null) objetivo.IntentarCompletar();
    }
}
