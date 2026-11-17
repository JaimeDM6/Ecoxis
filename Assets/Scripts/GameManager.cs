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

    [Header("UI - Fondos y Explicación")]
    public Image fondoOscurecido;
    [Range(0f, 1f)] public float opacidadMaxima = 0.6f;
    public RectTransform panelExplicacion;

    [Header("UI - Resumen de Nivel")]
    public RectTransform panelResumenVictoria;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoPuntuacionResumen;
    public float velocidadDeslizamiento = 3f;

    [Header("UI - Derrota")]
    public RectTransform panelMisionFallida;

    private float tiempoNivel = 0f;
    private bool nivelActivo = false;
    private bool enTransicion = false;
    private Coroutine rutinaActual;
    private Material matIris;

    public bool NivelActivo => nivelActivo && !enTransicion;

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else if (instancia != this) Destroy(gameObject);

        Time.timeScale = 1f;

        if (imagenIris != null)
        {
            matIris = new Material(imagenIris.material);
            imagenIris.material = matIris;
            imagenIris.gameObject.SetActive(true);
            DibujarIris(0f); 
        }
    }

    private void Start()
    {
        OcultarPanel(panelExplicacion);
        OcultarPanel(panelResumenVictoria);
        OcultarPanel(panelMisionFallida);
        
        if (fondoOscurecido != null)
        {
            fondoOscurecido.color = new Color(0, 0, 0, 0);
            fondoOscurecido.gameObject.SetActive(false);
        }

        nivelActivo = false;
        Time.timeScale = 0f;

        StartCoroutine(RutinaInicioNivel());
    }

    private void Update()
    {
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
            if (jugador != null && jugador.TryGetComponent<Personaje>(out var personaje))
            {
                personaje.Die();
            }
            else
            {
                GameOver();
            }
        }
    }

    private void ActualizarCorazones()
    {
        if (corazonesUI == null) return;
        
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            if (corazonesUI[i] != null)
            {
                corazonesUI[i].sprite = (i < vidas) ? corazonLleno : corazonVacio;
            }
        }
    }

    private IEnumerator RutinaInicioNivel()
    {
        if (imagenIris != null)
        {
            float tiempo = 0f;
            while (tiempo < tiempoCierre)
            {
                if (enTransicion) yield break;
                tiempo += Time.unscaledDeltaTime;
                DibujarIris(Mathf.Clamp01(tiempo / tiempoCierre));
                yield return null;
            }
            DibujarIris(1f);
            imagenIris.gameObject.SetActive(false);
        }

        if (enTransicion) yield break;

        if (panelExplicacion != null)
        {
            MostrarPopup(panelExplicacion);
        }
        else
        {
            ArrancarJuego();
        }
    }

    public void CerrarExplicacionYEmpezar()
    {
        if (enTransicion || nivelActivo) return;
        
        if (rutinaActual != null) StopCoroutine(rutinaActual);
        rutinaActual = StartCoroutine(RutinaOcultarExplicacion());
    }

    private IEnumerator RutinaOcultarExplicacion()
    {
        if (panelExplicacion != null)
        {
            Vector2 inicio = panelExplicacion.anchoredPosition;
            Vector2 destino = new Vector2(0f, -1500f);
            float t = 0f;

            while (t < 1f)
            {
                t = Mathf.Min(1f, t + Time.unscaledDeltaTime * velocidadDeslizamiento);
                float curva = t * t * (3f - 2f * t);
                
                panelExplicacion.anchoredPosition = Vector2.Lerp(inicio, destino, curva);
                CambiarOpacidadFondo(Mathf.Lerp(opacidadMaxima, 0f, curva));
                
                yield return null;
            }
            
            OcultarPanel(panelExplicacion);
        }
        
        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(false);
        rutinaActual = null;
        
        ArrancarJuego();
    }

    private void ArrancarJuego()
    {
        nivelActivo = true;
        Time.timeScale = 1f;
    }

    public void MostrarResumen()
    {
        if (!NivelActivo) return;
        
        nivelActivo = false;
        Time.timeScale = 0f;

        // Guardado de progreso
        int abiertos = PlayerPrefs.GetInt("NivelesAbiertos", 1);
        if (numeroNivel >= abiertos)
        {
            PlayerPrefs.SetInt("NivelesAbiertos", numeroNivel + 1);
            PlayerPrefs.Save();
        }

        if (textoTiempo != null)
        {
            int min = Mathf.FloorToInt(tiempoNivel / 60f);
            int seg = Mathf.FloorToInt(tiempoNivel % 60f);
            textoTiempo.text = $"TIEMPO TRANSCURRIDO: {min:00}:{seg:00}";
        }
        
        if (textoPuntuacionResumen != null)
        {
            textoPuntuacionResumen.text = $"PUNTUACION: {puntuacion}";
        }

        if (panelResumenVictoria != null) MostrarPopup(panelResumenVictoria);
    }

    public void GameOver()
    {
        if (!NivelActivo) return;
        
        nivelActivo = false;
        Time.timeScale = 0f;

        if (panelMisionFallida != null)
        {
            MostrarPopup(panelMisionFallida);
        }
        else
        {
            IniciarCierreEscena(escenaMenu, true);
        }
    }

    private void MostrarPopup(RectTransform panel)
    {
        if (rutinaActual != null) StopCoroutine(rutinaActual);
        
        if (panel != panelExplicacion) OcultarPanel(panelExplicacion);
        if (panel != panelResumenVictoria) OcultarPanel(panelResumenVictoria);
        if (panel != panelMisionFallida) OcultarPanel(panelMisionFallida);

        panel.gameObject.SetActive(true);
        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(true);

        rutinaActual = StartCoroutine(RutinaMostrarPopup(panel));
    }

    private IEnumerator RutinaMostrarPopup(RectTransform panel)
    {
        Vector2 inicio = new Vector2(0f, -1500f);
        panel.anchoredPosition = inicio;
        
        float opacidadInicial = fondoOscurecido != null ? fondoOscurecido.color.a : 0f;
        float t = 0f;

        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime * velocidadDeslizamiento);
            float curva = t * t * (3f - 2f * t);

            panel.anchoredPosition = Vector2.Lerp(inicio, Vector2.zero, curva);
            CambiarOpacidadFondo(Mathf.Lerp(opacidadInicial, opacidadMaxima, curva));
            
            yield return null;
        }

        panel.anchoredPosition = Vector2.zero;
        CambiarOpacidadFondo(opacidadMaxima);
        rutinaActual = null;
    }

    private void OcultarPanel(RectTransform panel)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(false);
        panel.anchoredPosition = new Vector2(0f, -1500f);
    }

    private void CambiarOpacidadFondo(float opacidad)
    {
        if (fondoOscurecido == null) return;
        Color c = fondoOscurecido.color;
        c.a = opacidad;
        fondoOscurecido.color = c;
    }

    private void DibujarIris(float apertura)
    {
        if (matIris != null && matIris.HasProperty("_Radius"))
        {
            // RadioPantalla() simplificado fijo para 16:9/Panorámico
            matIris.SetFloat("_Radius", Mathf.Lerp(-0.02f, 1.5f, apertura));
        }
        else if (imagenIris != null)
        {
            imagenIris.color = new Color(0f, 0f, 0f, 1f - apertura);
        }
    }

    public void ReiniciarNivel() => IniciarCierreEscena(SceneManager.GetActiveScene().name, false);
    public void VolverAlMenu() => IniciarCierreEscena(escenaMenu, true);

    private void IniciarCierreEscena(string escena, bool esMenu)
    {
        if (enTransicion) return;
        enTransicion = true;
        nivelActivo = false;
        
        if (rutinaActual != null) StopCoroutine(rutinaActual);
        StartCoroutine(RutinaTransicionCierre(escena, esMenu));
    }

    private IEnumerator RutinaTransicionCierre(string nombreEscena, bool esMenu)
    {
        if (imagenIris != null)
        {
            imagenIris.gameObject.SetActive(true);
            float tiempo = 0f;
            while (tiempo < tiempoCierre)
            {
                tiempo += Time.unscaledDeltaTime;
                DibujarIris(1f - Mathf.Clamp01(tiempo / tiempoCierre));
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
    }
}