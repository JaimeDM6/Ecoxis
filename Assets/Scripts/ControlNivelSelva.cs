using UnityEngine;

/// <summary>Control exclusivo de la escena selva; respeta movimiento, daño y navegación originales.</summary>
[DefaultExecutionOrder(-100)]
public class ControlNivelSelva : MonoBehaviour
{
    [Header("Referencias explícitas de esta escena")]
    public GameManager gameManager;
    public Personaje personaje;
    public MisionesSelva misiones;

    [Header("Caída fuera del nivel")]
    public float alturaMuerte = -9f;

    private Rigidbody2D cuerpo;
    private bool derrota;
    private bool victoria;

    public bool JugadorMuerto => derrota || (gameManager != null && gameManager.vidas <= 0)
        || (personaje != null && personaje.transform.position.y < alturaMuerte);
    public bool Victoria => victoria;
    public bool PuedeReforestar => isActiveAndEnabled && !victoria && !JugadorMuerto
        && gameManager != null && gameManager.gameObject.scene == gameObject.scene
        && personaje != null && personaje.gameObject.scene == gameObject.scene
        && personaje.isActiveAndEnabled && cuerpo != null && cuerpo.simulated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarReferenciaEstatica()
    {
        // Evita conservar una instancia destruida cuando el editor no recarga el dominio.
        // No se añade otro singleton ni se alteran las banderas de navegación del menú.
        GameManager.instancia = null;
    }

    private void Awake()
    {
        if (personaje != null)
            cuerpo = personaje.GetComponent<Rigidbody2D>();

        if (gameManager != null && gameManager.gameObject.scene == gameObject.scene)
        {
            // Harm y Personaje originales usan esta referencia: siempre apunta al GM local.
            GameManager.instancia = gameManager;
        }

        if (misiones != null)
        {
            if (misiones.gameManager == null)
                misiones.gameManager = gameManager;
            if (misiones.personaje == null)
                misiones.personaje = personaje;
            if (misiones.controlNivel == null)
                misiones.controlNivel = this;
        }
    }

    private void Update()
    {
        if (victoria || derrota || personaje == null || !personaje.gameObject.activeInHierarchy
            || personaje.gameObject.scene != gameObject.scene
            || (gameManager != null && gameManager.gameObject.scene != gameObject.scene))
            return;

        if (gameManager != null && gameManager.vidas <= 0)
            ProcesarMuerte(false);
        else if (personaje.transform.position.y < alturaMuerte)
            ProcesarMuerte(true);
    }

    private void ProcesarMuerte(bool porCaida)
    {
        // El estado se fija antes de PerderVida/Die para que ningún botón pueda plantar después.
        derrota = true;
        if (misiones != null)
            misiones.NotificarMuerte();

        if (porCaida && gameManager != null)
        {
            if (PuedeUsarPerderVidaOriginal())
            {
                // Con la UI del polo norte se conservan exactamente los corazones y la muerte.
                while (gameManager.vidas > 0)
                    gameManager.PerderVida();
            }
            else
            {
                // Referencias incompletas: el GM original no es null-safe; evitamos su excepción.
                gameManager.vidas = 0;
                if (gameManager.corazonesUI != null)
                {
                    foreach (UnityEngine.UI.Image corazon in gameManager.corazonesUI)
                    {
                        if (corazon != null)
                            corazon.sprite = gameManager.corazonVacio;
                    }
                }
            }
        }

        // Die ya es idempotente. No deshabilitamos ni congelamos su animación/corutina de muerte.
        if (personaje != null && personaje.gameObject.activeInHierarchy)
            personaje.Die();
    }

    private bool PuedeUsarPerderVidaOriginal()
    {
        if (personaje == null || !personaje.CompareTag("Player") || gameManager.corazonesUI == null)
            return false;

        foreach (UnityEngine.UI.Image corazon in gameManager.corazonesUI)
        {
            if (corazon == null)
                return false;
        }
        return true;
    }

    public void BloquearPorVictoria()
    {
        if (!PuedeReforestar)
            return;

        victoria = true;
        personaje.Parar();
        personaje.enabled = false;
        if (cuerpo != null)
        {
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            cuerpo.simulated = false;
        }
        // No se cambia gravedad, masa, constraints, colliders, Time.timeScale ni botones del GM.
    }

    private void LateUpdate()
    {
        // UnityEvent puede llamar Jump incluso con el componente deshabilitado; sin simulación
        // no se mueve y se descarta también cualquier velocidad escrita por controles móviles.
        if (victoria && cuerpo != null)
        {
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null && GameManager.instancia == gameManager)
            GameManager.instancia = null;
    }
}
