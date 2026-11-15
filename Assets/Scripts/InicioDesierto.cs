using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

// Explica el objetivo antes de permitir que Fenn se mueva.
public class InicioDesierto : MonoBehaviour
{
    public GameObject panelInstrucciones;
    public Button botonComenzar;
    public Personaje personaje;
    public RectTransform zonaControles;
    public RectTransform botonIzquierda;
    public RectTransform botonDerecha;
    public RectTransform botonSalto;
#if ENABLE_INPUT_SYSTEM
    public InputSystemUIInputModule moduloUI;
#endif
    private bool personajeEstabaActivo;
    private bool preparado;
    private Rect areaAnterior;

    public void Preparar()
    {
        if (preparado) return;
        preparado = true;
#if ENABLE_INPUT_SYSTEM
        if (moduloUI != null) moduloUI.AssignDefaultActions();
#endif
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
        if (botonComenzar != null)
        {
            botonComenzar.onClick.RemoveListener(Comenzar);
            // Si la escena ya tiene este callback, no añadir otro en tiempo de ejecución.
            bool tieneInicio = false;
            for (int i = 0; i < botonComenzar.onClick.GetPersistentEventCount(); i++)
            {
                if (botonComenzar.onClick.GetPersistentListenerState(i) == UnityEngine.Events.UnityEventCallState.Off) continue;
                Object destino = botonComenzar.onClick.GetPersistentTarget(i);
                string metodo = botonComenzar.onClick.GetPersistentMethodName(i);
                if ((destino == this && metodo == nameof(Comenzar)) ||
                    (destino == GameManager.instancia && metodo == nameof(GameManager.CerrarExplicacionYEmpezar)))
                    tieneInicio = true;
            }
            if (!tieneInicio) botonComenzar.onClick.AddListener(Comenzar);
        }
        if (personaje != null)
        {
            personajeEstabaActivo = personaje.enabled;
            personaje.enabled = false;
        }
        AjustarControles();
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (Screen.safeArea != areaAnterior) AjustarControles();
    }

    private void AjustarControles()
    {
        if (zonaControles == null || botonIzquierda == null || botonDerecha == null || botonSalto == null) return;
        Rect area = Screen.safeArea;
        areaAnterior = area;
        float anchoPantalla = Mathf.Max(Screen.width, 1);
        float altoPantalla = Mathf.Max(Screen.height, 1);
        zonaControles.anchorMin = new Vector2(area.xMin / anchoPantalla, area.yMin / altoPantalla);
        zonaControles.anchorMax = new Vector2(area.xMax / anchoPantalla, area.yMax / altoPantalla);
        zonaControles.offsetMin = zonaControles.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        float ancho = zonaControles.rect.width;
        float alto = zonaControles.rect.height;
        float tamano = Mathf.Clamp(Mathf.Min(ancho * 0.09f, alto * 0.18f), 60f, 180f);
        float margen = Mathf.Max(12f, ancho * 0.035f);
        Colocar(botonIzquierda, false, margen, margen, tamano);
        Colocar(botonDerecha, false, margen * 2f + tamano, margen, tamano);
        Colocar(botonSalto, true, -margen, margen, tamano);
    }

    private void Colocar(RectTransform boton, bool ladoDerecho, float x, float y, float tamano)
    {
        Vector2 esquina = ladoDerecho ? new Vector2(1f, 0f) : Vector2.zero;
        boton.anchorMin = boton.anchorMax = esquina;
        boton.pivot = esquina;
        boton.anchoredPosition = new Vector2(x, y);
        boton.sizeDelta = new Vector2(tamano, tamano);
    }

    public void Mostrar()
    {
        if (panelInstrucciones == null || botonComenzar == null)
        {
            Debug.LogWarning("Faltan referencias del aviso inicial; el nivel empieza sin aviso.");
            Comenzar();
            return;
        }
        if (GameManager.instancia != null)
        {
            GameManager.instancia.MostrarExplicacion();
            return;
        }
        panelInstrucciones.SetActive(true);
    }

    public void Comenzar()
    {
        if (GameManager.instancia != null)
        {
            GameManager.instancia.CerrarExplicacionYEmpezar();
            return;
        }
        RestaurarControl();
        Time.timeScale = 1f;
    }

    // Solo restaura controles: no llama de nuevo al gestor ni cambia Time.timeScale.
    public void RestaurarControl()
    {
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
        if (!preparado) return;
        if (personaje != null)
        {
            personaje.Parar();
            personaje.enabled = personajeEstabaActivo;
        }
        preparado = false;
    }

    private void OnDestroy()
    {
        if (botonComenzar != null) botonComenzar.onClick.RemoveListener(Comenzar);
    }
}
