using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pausa inicial exclusiva de la selva. El Canvas, el texto y el botón se crean en la escena.
/// Debe vivir en una raíz activa, separada del panel que se oculta al comenzar.
/// </summary>
[DefaultExecutionOrder(-200)]
public class IntroduccionSelva : MonoBehaviour
{
    [Header("Referencias explícitas de la escena")]
    [Tooltip("Panel con el oscurecimiento, VINI - JUNGLA, las reglas y COMENZAR.")]
    public GameObject panelIntroduccion;
    [Tooltip("Grupo de botones de movimiento y salto; no el Canvas completo.")]
    public GameObject controles;
    public GameManager gameManager;
    public Personaje personaje;

    /// <summary>También permite que misiones y otros controles rechacen acciones durante la introducción.</summary>
    public bool Mostrando { get; private set; }

    private Rigidbody2D cuerpo;
    private bool simulacionOriginal;
    private bool simulacionGuardada;
    private bool controlesActivosOriginales;
    private bool controlesGuardados;
    private bool gameManagerHabilitadoOriginal;
    private bool gameManagerGuardado;
    private float escalaTiempoOriginal;
    private Image irisOriginal;
    private bool irisGuardado;
    private bool personajePasoPrimerFrame;
    private bool introduccionFinalizada;

    private void Awake()
    {
        // Sin un panel visible no se captura ningún estado ni se pausa el resto del juego.
        if (!isActiveAndEnabled || panelIntroduccion == null || !panelIntroduccion.activeInHierarchy)
            return;

        // Ocultar este panel no debe desactivar el componente que administra la pausa,
        // ni el GameManager o el personaje que deben seguir existiendo al comenzar.
        if (ContieneObjetoProtegido(panelIntroduccion, false))
        {
            Debug.LogWarning("IntroduccionSelva: asigna un panel separado de la raíz, GameManager y Personaje.", this);
            return;
        }

        escalaTiempoOriginal = Time.timeScale;
        Mostrando = true;

        if (gameManager != null)
        {
            gameManagerHabilitadoOriginal = gameManager.enabled;
            gameManagerGuardado = true;
            gameManager.enabled = false;

            // Awake precede al Start del GM: sin referencia, GameStart no abre un iris negro.
            irisOriginal = gameManager.imagenIris;
            irisGuardado = true;
            gameManager.imagenIris = null;
            if (irisOriginal != null)
            {
                GameObject objetoIris = irisOriginal.gameObject;
                if (!ContieneObjetoProtegido(objetoIris, true))
                    objetoIris.SetActive(false);
                else
                    Debug.LogWarning("IntroduccionSelva: el iris debe ser un objeto visual separado; no se desactivó su raíz.", this);
            }
        }

        if (personaje != null)
        {
            // No se deshabilita Personaje: sus referencias privadas se preparan en Start.
            cuerpo = personaje.GetComponent<Rigidbody2D>();
            if (cuerpo != null)
            {
                simulacionOriginal = cuerpo.simulated;
                simulacionGuardada = true;
                CongelarCuerpo();
            }
        }

        if (controles != null)
        {
            if (!ContieneObjetoProtegido(controles, true))
            {
                controlesActivosOriginales = controles.activeSelf;
                controlesGuardados = true;
                controles.SetActive(false);
            }
            else
                Debug.LogWarning("IntroduccionSelva: controles debe contener sólo los botones de gameplay.", this);
        }

        // Pausa también spawners, coleccionables, animaciones y esperas con tiempo escalado.
        // La UI y Start siguen funcionando; no se cambia fixedDeltaTime ni ningún material.
        Time.timeScale = 0f;
    }

    private void LateUpdate()
    {
        if (!Mostrando)
            return;

        // Todos los Start/Update normales del frame ya han ocurrido. Parar sólo limpia
        // los botones mantenidos; la simulación y la velocidad bloquean también el teclado.
        if (personaje != null && personaje.isActiveAndEnabled)
        {
            personajePasoPrimerFrame = true;
            personaje.Parar();
        }
        CongelarCuerpo();

        // Un script de plataforma puede activar los botones en su propio Start.
        if (controlesGuardados && controles != null && controles.activeSelf)
            controles.SetActive(false);
    }

    private void CongelarCuerpo()
    {
        if (!simulacionGuardada || cuerpo == null)
            return;

        cuerpo.simulated = false;
        cuerpo.linearVelocity = Vector2.zero;
        cuerpo.angularVelocity = 0f;
    }

    /// <summary>Enlazar a Button.onClick de COMENZAR. Sólo el primer clic tiene efecto.</summary>
    public void Comenzar()
    {
        if (!Mostrando || introduccionFinalizada)
            return;

        FinalizarIntroduccion();
    }

    private void FinalizarIntroduccion()
    {
        // Se marca primero: OnDisable o un segundo UnityEvent no pueden restaurar dos veces.
        Mostrando = false;
        introduccionFinalizada = true;

        if (panelIntroduccion != null)
            panelIntroduccion.SetActive(false);

        Time.timeScale = escalaTiempoOriginal;

        if (personajePasoPrimerFrame && personaje != null && personaje.isActiveAndEnabled)
            personaje.Parar();

        if (simulacionGuardada && cuerpo != null)
        {
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            cuerpo.simulated = simulacionOriginal;
        }
        simulacionGuardada = false;

        if (controlesGuardados && controles != null)
            controles.SetActive(controlesActivosOriginales);
        controlesGuardados = false;

        if (gameManagerGuardado && gameManager != null)
        {
            // Sigue siendo null cuando el GM se habilita: su Start puede ejecutarse
            // inmediatamente o al principio del siguiente frame, sin una segunda apertura.
            GameManager manager = gameManager;
            Image iris = irisOriginal;
            bool restaurarIris = irisGuardado;
            manager.enabled = gameManagerHabilitadoOriginal;

            if (restaurarIris)
            {
                if (manager.isActiveAndEnabled)
                {
                    // El GM aloja la corutina: ocultar/destruir nuestra raíz no la cancela.
                    manager.StartCoroutine(RestaurarIrisDespuesDelInicio(manager, iris));
                }
                else if (manager.imagenIris == null)
                {
                    // Si ya estaba deshabilitado, no se fuerza el comienzo de gameplay.
                    manager.imagenIris = iris;
                }
            }
        }
        gameManagerGuardado = false;
        irisGuardado = false;
        irisOriginal = null;
    }

    private static IEnumerator RestaurarIrisDespuesDelInicio(GameManager manager, Image iris)
    {
        // yield null no usa tiempo escalado. Dos frames dejan pasar Start incluso si
        // COMENZAR se invocó durante el despacho de eventos del mismo frame.
        yield return null;
        yield return null;

        // No se pisa una nueva referencia asignada por otro sistema ni se reactiva el iris.
        // TransicionCierreEscena lo activará normalmente en victoria/reinicio/menú.
        if (manager != null && manager.imagenIris == null)
            manager.imagenIris = iris;
    }

    private bool ContieneObjetoProtegido(GameObject grupo, bool protegerPanel)
    {
        if (grupo == null)
            return false;

        Transform raiz = grupo.transform;
        return transform.IsChildOf(raiz)
            || (gameManager != null && gameManager.transform.IsChildOf(raiz))
            || (personaje != null && personaje.transform.IsChildOf(raiz))
            || (protegerPanel && panelIntroduccion != null
                && panelIntroduccion.transform.IsChildOf(raiz));
    }

    private void OnDisable()
    {
        // Cancelación/cambio de escena: nunca dejar el siguiente menú con timeScale = 0.
        // Tras COMENZAR no se toca ningún estado, incluida una eventual victoria.
        if (Mostrando)
            FinalizarIntroduccion();
    }

    private void OnDestroy()
    {
        if (Mostrando)
            FinalizarIntroduccion();
    }
}
