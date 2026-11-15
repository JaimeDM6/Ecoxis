using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Misiones locales de selva; teclado y botón táctil comparten una sola acción.</summary>
public class MisionesSelva : MonoBehaviour
{
    [Header("Referencias de esta escena")]
    public TextMeshProUGUI textoMisiones;
    public TextMeshProUGUI textoAyuda;
    public GameManager gameManager;
    public Personaje personaje;
    public ControlNivelSelva controlNivel;

    [Header("Objetivo")]
    [Min(1)] public int totalZonas = 3;

    private readonly HashSet<ZonaReforestacion> zonasRecuperadas = new HashSet<ZonaReforestacion>();
    private readonly HashSet<ZonaReforestacion> zonasCercanas = new HashSet<ZonaReforestacion>();
    private Coroutine secuenciaFinal;
    private bool finalizando;
    private bool resumenMostrado;
    private bool jugadorMuerto;

    public int ZonasRecuperadas => Mathf.Clamp(zonasRecuperadas.Count, 0, Mathf.Max(1, totalZonas));
    public bool NivelCompletado => finalizando || resumenMostrado;
    public bool PuedeReforestar => isActiveAndEnabled && !jugadorMuerto && !NivelCompletado
        && gameManager != null && gameManager.vidas > 0
        && personaje != null && personaje.isActiveAndEnabled
        && controlNivel != null && gameManager == controlNivel.gameManager
        && personaje == controlNivel.personaje && controlNivel.PuedeReforestar;

    private void Awake()
    {
        totalZonas = Mathf.Max(1, totalZonas);
        // Solo se busca en el propio objeto, nunca en otros niveles ni en objetos globales.
        if (controlNivel == null)
            TryGetComponent(out controlNivel);
    }

    private void Start()
    {
        ActualizarTexto();
        ActualizarAyuda();
    }

    private void Update()
    {
        ActualizarAyuda();
        if (PuedeReforestar && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            IntentarReforestar();
    }

    /// <summary>Asignar a Button.onClick; restaura exclusivamente la zona válida más cercana.</summary>
    public void IntentarReforestar()
    {
        if (!PuedeReforestar)
            return;

        ZonaReforestacion zona = ObtenerZonaCercana();
        if (zona != null)
            zona.Reforestar();
    }

    // Compatibilidad con el nombre anterior, sin incrementar progreso de manera anónima.
    public void RegistrarZona()
    {
        IntentarReforestar();
    }

    /// <summary>Solo acepta zonas restauradas propias: una misma zona nunca cuenta dos veces.</summary>
    public bool RegistroZona(ZonaReforestacion zona)
    {
        if (!PuedeReforestar || zona == null || zona.misiones != this || !zona.Recuperada
            || !zonasCercanas.Contains(zona) || zonasRecuperadas.Count >= Mathf.Max(1, totalZonas)
            || !zonasRecuperadas.Add(zona))
            return false;

        zonasCercanas.Remove(zona);
        ActualizarTexto();

        if (ZonasRecuperadas >= Mathf.Max(1, totalZonas))
        {
            finalizando = true;
            zonasCercanas.Clear();
            // Bloqueo local inmediato: no hay daño ni movimiento durante la espera del resumen.
            controlNivel.BloquearPorVictoria();
            secuenciaFinal = StartCoroutine(MostrarResumenTrasEspera());
        }

        ActualizarAyuda();
        return true;
    }

    public void RegistrarEntrada(ZonaReforestacion zona)
    {
        if (zona != null && zona.misiones == this && !zona.Recuperada && !NivelCompletado && !jugadorMuerto)
            zonasCercanas.Add(zona);
        ActualizarAyuda();
    }

    public void RegistrarSalida(ZonaReforestacion zona)
    {
        if (zona != null)
            zonasCercanas.Remove(zona);
        ActualizarAyuda();
    }

    public void NotificarMuerte()
    {
        jugadorMuerto = true;
        zonasCercanas.Clear();
        if (secuenciaFinal != null)
        {
            StopCoroutine(secuenciaFinal);
            secuenciaFinal = null;
        }
        ActualizarAyuda();
    }

    private ZonaReforestacion ObtenerZonaCercana()
    {
        if (personaje == null)
            return null;

        ZonaReforestacion elegida = null;
        float distanciaMinima = float.PositiveInfinity;
        foreach (ZonaReforestacion zona in zonasCercanas)
        {
            if (zona == null || !zona.isActiveAndEnabled || zona.Recuperada || !zona.JugadorCerca)
                continue;

            float distancia = ((Vector2)(zona.transform.position - personaje.transform.position)).sqrMagnitude;
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                elegida = zona;
            }
        }
        return elegida;
    }

    private IEnumerator MostrarResumenTrasEspera()
    {
        // Mismo retraso que Seal. No se cambia Time.timeScale: iris y popup siguen funcionando.
        yield return new WaitForSeconds(2f);
        secuenciaFinal = null;
        if (resumenMostrado || jugadorMuerto || gameManager == null || gameManager.vidas <= 0)
            yield break;

        // Se marca antes de llamar para impedir resúmenes duplicados incluso ante callbacks.
        resumenMostrado = true;
        gameManager.MostrarResumen();
    }

    private void ActualizarTexto()
    {
        if (textoMisiones != null)
            textoMisiones.text = "Zonas recuperadas: " + ZonasRecuperadas + " / " + Mathf.Max(1, totalZonas);
    }

    private void ActualizarAyuda()
    {
        if (textoAyuda == null)
            return;

        if (jugadorMuerto || (gameManager != null && gameManager.vidas <= 0))
            textoAyuda.text = string.Empty;
        else if (NivelCompletado)
            textoAyuda.text = "¡Selva restaurada!";
        else if (PuedeReforestar && ObtenerZonaCercana() != null)
            textoAyuda.text = "Pulsa E o el botón Reforestar para plantar un árbol.";
        else
            textoAyuda.text = "Acércate a una zona para reforestar.";
    }

    private void OnDisable()
    {
        zonasCercanas.Clear();
        if (secuenciaFinal != null)
        {
            StopCoroutine(secuenciaFinal);
            secuenciaFinal = null;
        }
        if (textoAyuda != null)
            textoAyuda.text = string.Empty;
    }
}
