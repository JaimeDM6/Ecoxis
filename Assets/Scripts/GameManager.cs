using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;
    public static bool volverNiveles = false;

    [Header("Progreso del Juego")]
    public int numeroNivel = 1;

    [Header("Puntuación")]
    public int puntuacion = 0;
    public TextMeshProUGUI textoPuntuacion;

    [Header("Vidas")]
    public int vidas = 3;
    public Image[] corazonesUI;
    public Sprite corazonLleno;
    public Sprite corazonVacio;

    [Header("Transición")]
    public Image imagenIris;
    public float tiempoCierre = 1.5f;
    public string escenaMenu = "00_UI";

    [Header("Fondo Oscuro")]
    public Image fondoOscurecido;
    [Range(0f, 1f)] public float opacidadMaxima = 0.6f;

    [Header("Explicación de Nivel")]
    public RectTransform panelExplicacion;

    [Header("UI Resumen Fin de Nivel")]
    public RectTransform panelResumenVictoria;
    [Tooltip("Referencia antigua. Se conserva para Polo Norte, Desierto y Selva anteriores.")]
    public RectTransform panelResumen;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoPuntuacionResumen;
    public float velocidadDeslizamiento = 3f;
    [Tooltip("Permite editar TextoTiempo y TextoPuntos usando {tiempo} y {puntos}.")]
    public bool usarTextosEditablesResumen = false;
    private string plantillaTiempo;
    private string plantillaPuntos;

    [Header("UI Misión Fallida")]
    public RectTransform panelMisionFallida;

    [Header("Desierto (opcionales en los otros niveles)")]
    public InicioDesierto inicioDesierto;
    [Tooltip("Campo antiguo conservado. Los diálogos ahora siempre pausan el juego.")]
    public bool pausarAlCompletar = false;

    private float tiempoNivel = 0f;
    private bool nivelActivo = false;
    private bool resumenMostrado = false;
    private bool misionFallidaMostrada = false;
    private bool explicacionMostrada = false;
    private bool cerrandoExplicacion = false;
    private bool enTransicion = false;
    private Coroutine rutinaPanel;
    private Material matIris;
    public bool NivelActivo { get { return nivelActivo && !enTransicion; } }

    private void Awake()
    {
        instancia = this;
        Time.timeScale = 1f;
        // Mantener ambos nombres serializados, sin romper referencias antiguas.
        if (panelResumenVictoria == null) panelResumenVictoria = panelResumen;
        panelResumen = panelResumenVictoria;
        if (imagenIris == null) return;

        RectTransform rect = imagenIris.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        imagenIris.type = Image.Type.Simple;
        imagenIris.preserveAspect = false;
        imagenIris.raycastTarget = true;

        Canvas canvas = imagenIris.GetComponent<Canvas>();
        if (canvas == null) canvas = imagenIris.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;
        if (imagenIris.GetComponent<GraphicRaycaster>() == null)
            imagenIris.gameObject.AddComponent<GraphicRaycaster>();

        matIris = new Material(imagenIris.material);
        imagenIris.material = matIris;
        if (matIris.HasProperty("_Radius"))
        {
            matIris.SetColor("_Color", Color.black);
            matIris.SetFloat("_Radius", -0.02f);
            imagenIris.color = Color.white;
        }
        else imagenIris.color = Color.black;
        imagenIris.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (usarTextosEditablesResumen)
        {
            plantillaTiempo = textoTiempo != null ? textoTiempo.text : "TIEMPO: {tiempo}";
            plantillaPuntos = textoPuntuacionResumen != null ? textoPuntuacionResumen.text : "PUNTOS: {puntos}";
        }
        // Una escena antigua puede tener el panel solo en InicioDesierto.
        if (panelExplicacion == null && inicioDesierto != null && inicioDesierto.panelInstrucciones != null)
            panelExplicacion = inicioDesierto.panelInstrucciones.GetComponent<RectTransform>();
        OcultarPanel(panelExplicacion);
        OcultarPanel(panelResumenVictoria);
        OcultarPanel(panelMisionFallida);
        CambiarFondo(0f);
        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(false);
        nivelActivo = false;
        Time.timeScale = 0f;
        if (inicioDesierto != null) inicioDesierto.Preparar();
        StartCoroutine(GameStart());
    }

    private void Update()
    {
        // No cuenta la entrada, el tutorial ni los diálogos.
        if (NivelActivo) tiempoNivel += Time.deltaTime;
    }

    public void SumarPuntos(int puntos)
    {
        if (!NivelActivo) return;
        puntuacion += puntos;
        if (textoPuntuacion != null) textoPuntuacion.text = puntuacion.ToString();
    }

    public void PerderVida()
    {
        if (vidas <= 0 || !NivelActivo) return;
        vidas--;
        ActualizarCorazones();
        if (vidas <= 0)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            Personaje personaje = jugador != null ? jugador.GetComponent<Personaje>() : null;
            if (personaje != null) personaje.Die();
            else GameOver();
        }
    }

    private void ActualizarCorazones()
    {
        if (corazonesUI == null) return;
        for (int i = 0; i < corazonesUI.Length; i++)
            if (corazonesUI[i] != null)
                corazonesUI[i].sprite = i < vidas ? corazonLleno : corazonVacio;
    }

    public void GameOver()
    {
        if (!NivelActivo || resumenMostrado || misionFallidaMostrada) return;
        misionFallidaMostrada = true;
        nivelActivo = false;
        Time.timeScale = 0f;
        if (panelMisionFallida != null) MostrarPopup(panelMisionFallida);
        else
        {
            Debug.LogWarning("Falta Panel Mision Fallida en GameManager; se vuelve al selector como en las escenas antiguas.");
            IniciarCierre(escenaMenu, true);
        }
    }

    private float RadioPantalla()
    {
        float proporcion = (float)Screen.width / Mathf.Max(Screen.height, 1);
        return Mathf.Sqrt(proporcion * proporcion + 1f) * 0.5f + 0.05f;
    }

    private void DibujarIris(float apertura)
    {
        if (matIris != null && matIris.HasProperty("_Radius"))
            matIris.SetFloat("_Radius", Mathf.Lerp(-0.02f, RadioPantalla(), apertura));
        else if (imagenIris != null)
            imagenIris.color = new Color(0f, 0f, 0f, 1f - apertura);
    }

    private IEnumerator GameStart()
    {
        if (imagenIris != null)
        {
            float tiempo = 0f;
            float duracion = Mathf.Max(tiempoCierre, 0.01f);
            DibujarIris(0f);
            while (tiempo < duracion)
            {
                if (enTransicion) yield break;
                tiempo += Time.unscaledDeltaTime;
                DibujarIris(Mathf.Clamp01(tiempo / duracion));
                yield return null;
            }
            DibujarIris(1f);
            imagenIris.gameObject.SetActive(false);
        }
        if (enTransicion) yield break;
        if (inicioDesierto != null) inicioDesierto.Mostrar();
        else MostrarExplicacion();
    }

    public void MostrarExplicacion()
    {
        if (enTransicion || resumenMostrado || misionFallidaMostrada || explicacionMostrada || nivelActivo) return;
        if (panelExplicacion == null)
        {
            EmpezarNivel();
            return;
        }
        explicacionMostrada = true;
        nivelActivo = false;
        Time.timeScale = 0f;
        MostrarPopup(panelExplicacion);
    }

    public void CerrarExplicacionYEmpezar()
    {
        if (enTransicion || resumenMostrado || misionFallidaMostrada || nivelActivo || cerrandoExplicacion) return;
        cerrandoExplicacion = true;
        if (rutinaPanel != null) StopCoroutine(rutinaPanel);
        rutinaPanel = StartCoroutine(RutinaEmpezar());
    }

    private IEnumerator RutinaEmpezar()
    {
        Vector2 inicio = panelExplicacion != null ? panelExplicacion.anchoredPosition : Vector2.zero;
        float opacidadInicial = fondoOscurecido != null ? fondoOscurecido.color.a : 0f;
        float t = 0f;
        float velocidad = Mathf.Max(velocidadDeslizamiento, 0.01f);
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime * velocidad);
            float suave = t * t * (3f - 2f * t);
            if (panelExplicacion != null)
                panelExplicacion.anchoredPosition = Vector2.Lerp(inicio, new Vector2(0f, -1500f), suave);
            CambiarFondo(Mathf.Lerp(opacidadInicial, 0f, suave));
            yield return null;
        }
        OcultarPanel(panelExplicacion);
        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(false);
        rutinaPanel = null;
        EmpezarNivel();
    }

    private void EmpezarNivel()
    {
        if (enTransicion || resumenMostrado || misionFallidaMostrada) return;
        // Restaurar el componente antes de reanudar la física, nunca a mitad del tween.
        if (inicioDesierto != null) inicioDesierto.RestaurarControl();
        explicacionMostrada = false;
        cerrandoExplicacion = false;
        nivelActivo = true;
        Time.timeScale = 1f;
    }

    public void MostrarResumen()
    {
        if (!NivelActivo || resumenMostrado || misionFallidaMostrada) return;
        resumenMostrado = true;
        nivelActivo = false;
        Time.timeScale = 0f;

        // numeroNivel es la tarjeta logica (Desierto = 3), no el indice de escena.
        int abiertos = PlayerPrefs.GetInt("NivelesAbiertos", 1);
        if (numeroNivel >= abiertos)
        {
            PlayerPrefs.SetInt("NivelesAbiertos", numeroNivel + 1);
            PlayerPrefs.Save();
        }
        int minutos = Mathf.FloorToInt(tiempoNivel / 60f);
        int segundos = Mathf.FloorToInt(tiempoNivel % 60f);
        string tiempoFormateado = string.Format("{0:00}:{1:00}", minutos, segundos);
        if (textoTiempo != null)
            textoTiempo.text = usarTextosEditablesResumen
                ? (plantillaTiempo ?? textoTiempo.text).Replace("{tiempo}", tiempoFormateado)
                : "TIEMPO TRANSCURRIDO: " + tiempoFormateado;
        if (textoPuntuacionResumen != null)
            textoPuntuacionResumen.text = usarTextosEditablesResumen
                ? (plantillaPuntos ?? textoPuntuacionResumen.text).Replace("{puntos}", puntuacion.ToString())
                : "PUNTUACIÓN: " + puntuacion;

        if (panelResumenVictoria != null) MostrarPopup(panelResumenVictoria);
        else Debug.LogWarning("Falta Panel Resumen Victoria en GameManager. Asignalo en el Inspector.");
    }

    private void OcultarPanel(RectTransform panel)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(false);
        panel.anchoredPosition = new Vector2(0f, -1500f);
    }

    private void CambiarFondo(float opacidad)
    {
        if (fondoOscurecido == null) return;
        Color color = fondoOscurecido.color;
        color.a = opacidad;
        fondoOscurecido.color = color;
    }

    private void MostrarPopup(RectTransform panel)
    {
        if (rutinaPanel != null) StopCoroutine(rutinaPanel);
        // Nunca quedan dos ventanas activas detrás del mismo overlay.
        if (panel != panelExplicacion) OcultarPanel(panelExplicacion);
        if (panel != panelResumenVictoria) OcultarPanel(panelResumenVictoria);
        if (panel != panelMisionFallida) OcultarPanel(panelMisionFallida);
        panel.gameObject.SetActive(true);
        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(true);
        rutinaPanel = StartCoroutine(AnimarPopup(panel));
    }

    private IEnumerator AnimarPopup(RectTransform panel)
    {
        Vector2 oculta = new Vector2(0f, -1500f);
        panel.anchoredPosition = oculta;
        float opacidadInicial = fondoOscurecido != null ? fondoOscurecido.color.a : 0f;
        float t = 0f;
        float velocidad = Mathf.Max(velocidadDeslizamiento, 0.01f);
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime * velocidad);
            float suave = t * t * (3f - 2f * t);
            panel.anchoredPosition = Vector2.Lerp(oculta, Vector2.zero, suave);
            CambiarFondo(Mathf.Lerp(opacidadInicial, opacidadMaxima, suave));
            yield return null;
        }
        panel.anchoredPosition = Vector2.zero;
        CambiarFondo(opacidadMaxima);
        rutinaPanel = null;
    }

    public void ReiniciarNivel()
    {
        IniciarCierre(SceneManager.GetActiveScene().name, false);
    }

    public void VolverAlMenu()
    {
        IniciarCierre(escenaMenu, true);
    }

    private void IniciarCierre(string escena, bool esMenu)
    {
        if (enTransicion) return;
        enTransicion = true;
        nivelActivo = false;
        if (rutinaPanel != null) StopCoroutine(rutinaPanel);
        StartCoroutine(TransicionCierreEscena(escena, esMenu));
    }

    private IEnumerator TransicionCierreEscena(string nombreEscena, bool esMenu)
    {
        if (imagenIris != null)
        {
            imagenIris.gameObject.SetActive(true);
            float tiempo = 0f;
            float duracion = Mathf.Max(tiempoCierre, 0.01f);
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime;
                DibujarIris(1f - Mathf.Clamp01(tiempo / duracion));
                yield return null;
            }
            DibujarIris(0f);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        Time.timeScale = 1f;
        if (esMenu)
        {
            MenuNiveles.Niveles = true;
            volverNiveles = true;
        }
        SceneManager.LoadScene(nombreEscena);
    }

    private void OnDestroy()
    {
        if (matIris != null) Destroy(matIris);
        if (instancia == this) instancia = null;
    }
}
