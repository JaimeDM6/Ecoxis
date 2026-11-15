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
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoPuntuacionResumen;

    [Header("UI Misión Fallida")]
    public RectTransform panelMisionFallida;

    public float velocidadDeslizamiento = 3f;

    private float tiempoNivel = 0f;
    private bool nivelActivo = true;

    private void Awake()
    {
        if (instancia == null) instancia = this;
    }

    private void Start()
    {
        if (fondoOscurecido != null)
        {
            fondoOscurecido.gameObject.SetActive(false);
            Color c = fondoOscurecido.color;
            c.a = 0f;
            fondoOscurecido.color = c;
        }

        if (panelExplicacion != null) panelExplicacion.anchoredPosition = new Vector2(0, -1500f);
        if (panelResumenVictoria != null) panelResumenVictoria.anchoredPosition = new Vector2(0, -1500f);
        if (panelMisionFallida != null) panelMisionFallida.anchoredPosition = new Vector2(0, -1500f);

        StartCoroutine(GameStart());
    }

    private void Update()
    {
        if (nivelActivo)
        {
            tiempoNivel += Time.deltaTime;
        }
    }

    public void SumarPuntos(int puntos)
    {
        puntuacion += puntos;
        textoPuntuacion.text = puntuacion.ToString();
    }

    public void PerderVida()
    {
        if (vidas <= 0) return;

        vidas--;
        ActualizarCorazones();

        if (vidas <= 0)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Pipo>().Die();
        }
    }

    private void ActualizarCorazones()
    {
        for (int i = 0; i < corazonesUI.Length; i++)
        {
            if (i < vidas)
            {
                corazonesUI[i].sprite = corazonLleno;
            }
            else
            {
                corazonesUI[i].sprite = corazonVacio;
            }
        }
    }

    public void GameOver()
    {
        nivelActivo = false;
        Time.timeScale = 0f;

        if (panelMisionFallida != null)
        {
            StartCoroutine(AnimarPopup(panelMisionFallida));
        }
    }

    private IEnumerator GameStart()
    {
        if (imagenIris != null)
        {
            imagenIris.gameObject.SetActive(true);
            imagenIris.color = Color.white;

            Material matIris = new Material(imagenIris.material);
            imagenIris.material = matIris;

            float radioInicial = 0f;
            float radioFinal = 1.5f;
            float tiempo = 0f;

            while (tiempo < tiempoCierre)
            {
                tiempo += Time.unscaledDeltaTime;
                float radioActual = Mathf.Lerp(radioInicial, radioFinal, tiempo / tiempoCierre);
                matIris.SetFloat("_Radius", radioActual);
                yield return null;
            }

            matIris.SetFloat("_Radius", radioFinal);
            imagenIris.gameObject.SetActive(false);
        }

        if (panelExplicacion != null)
        {
            StartCoroutine(AnimarExplicacion());
        }
    }

    private IEnumerator AnimarExplicacion()
    {
        Vector2 posicionOculta = new Vector2(0, -1500f);
        Vector2 posicionCentro = Vector2.zero;
        panelExplicacion.anchoredPosition = posicionOculta;

        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * velocidadDeslizamiento;
            float curvaSuave = t * t * (3f - 2f * t);

            panelExplicacion.anchoredPosition = Vector2.Lerp(posicionOculta, posicionCentro, curvaSuave);

            if (fondoOscurecido != null)
            {
                Color colorFondo = fondoOscurecido.color;
                colorFondo.a = Mathf.Lerp(0f, opacidadMaxima, curvaSuave);
                fondoOscurecido.color = colorFondo;
            }

            yield return null;
        }

        panelExplicacion.anchoredPosition = posicionCentro;

        if (fondoOscurecido != null)
        {
            Color colorFinal = fondoOscurecido.color;
            colorFinal.a = opacidadMaxima;
            fondoOscurecido.color = colorFinal;
        }

        Time.timeScale = 0f;
    }

    public void CerrarExplicacionYEmpezar()
    {
        StartCoroutine(RutinaEmpezar());
    }

    private IEnumerator RutinaEmpezar()
    {
        Vector2 posicionCentro = Vector2.zero;
        Vector2 posicionOcultaAbajo = new Vector2(0, -1500f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * velocidadDeslizamiento;
            float curvaSuave = t * t * (3f - 2f * t);

            if (panelExplicacion != null)
            {
                panelExplicacion.anchoredPosition = Vector2.Lerp(posicionCentro, posicionOcultaAbajo, curvaSuave);
            }

            if (fondoOscurecido != null)
            {
                Color c = fondoOscurecido.color;
                c.a = Mathf.Lerp(opacidadMaxima, 0f, curvaSuave);
                fondoOscurecido.color = c;
            }
            yield return null;
        }

        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void MostrarResumen()
    {
        nivelActivo = false;
        Time.timeScale = 0f;

        int nivelesDesbloqueados = PlayerPrefs.GetInt("NivelesAbiertos", 1);

        if (numeroNivel >= nivelesDesbloqueados)
        {
            PlayerPrefs.SetInt("NivelesAbiertos", numeroNivel + 1);
            PlayerPrefs.Save();
        }

        int minutos = Mathf.FloorToInt(tiempoNivel / 60);
        int segundos = Mathf.FloorToInt(tiempoNivel % 60);
        textoTiempo.text = string.Format("TIEMPO TRANSCURRIDO: {0:00}:{1:00}", minutos, segundos);
        textoPuntuacionResumen.text = "PUNTUACION: " + puntuacion.ToString();

        if (panelResumenVictoria != null)
        {
            StartCoroutine(AnimarPopup(panelResumenVictoria));
        }
    }

    private IEnumerator AnimarPopup(RectTransform panel)
    {
        Vector2 posicionOculta = new Vector2(0, -1500f);
        Vector2 posicionCentro = Vector2.zero;
        panel.anchoredPosition = posicionOculta;

        if (fondoOscurecido != null) fondoOscurecido.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * velocidadDeslizamiento;

            float curvaSuave = t * t * (3f - 2f * t);

            panel.anchoredPosition = Vector2.Lerp(posicionOculta, posicionCentro, curvaSuave);

            if (fondoOscurecido != null)
            {
                Color colorFondo = fondoOscurecido.color;
                colorFondo.a = Mathf.Lerp(0f, opacidadMaxima, curvaSuave);
                fondoOscurecido.color = colorFondo;
            }

            yield return null;
        }

        panel.anchoredPosition = posicionCentro;

        if (fondoOscurecido != null)
        {
            Color colorFinal = fondoOscurecido.color;
            colorFinal.a = opacidadMaxima;
            fondoOscurecido.color = colorFinal;
        }
    }

    public void ReiniciarNivel()
    {
        StartCoroutine(TransicionCierreEscena(SceneManager.GetActiveScene().name, false));
    }

    public void VolverAlMenu()
    {
        StartCoroutine(TransicionCierreEscena(escenaMenu, true));
    }

    private IEnumerator TransicionCierreEscena(string nombreEscena, bool esMenu)
    {
        if (imagenIris != null)
        {
            imagenIris.gameObject.SetActive(true);

            Material matIris = new Material(imagenIris.material);
            imagenIris.material = matIris;

            float radioInicial = 1.5f;
            float radioFinal = 0f;
            float tiempo = 0f;

            while (tiempo < tiempoCierre)
            {
                tiempo += Time.unscaledDeltaTime;
                float radioActual = Mathf.Lerp(radioInicial, radioFinal, tiempo / tiempoCierre);
                matIris.SetFloat("_Radius", radioActual);
                yield return null;
            }

            matIris.SetFloat("_Radius", radioFinal);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        if (esMenu)
        {
            MenuNiveles.Niveles = true;
            volverNiveles = true;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }
}