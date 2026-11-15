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

    [Header("Transición Game Over")]
    public Image imagenIris;
    public float tiempoCierre = 1.5f;
    public string escenaMenu = "00_UI";

    [Header("UI Resumen Fin de Nivel")]
    public RectTransform panelResumen;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoPuntuacionResumen;
    public float velocidadDeslizamiento = 3f;

    private float tiempoNivel = 0f;
    private bool nivelActivo = true;

    private void Awake()
    {
        if (instancia == null) instancia = this;
    }

    private void Start()
    {
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
            GameObject.FindGameObjectWithTag("Player").GetComponent<Personaje>().Die();
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
        StartCoroutine(TransicionCierreEscena(escenaMenu, true));
    }

    private IEnumerator GameStart()
    {
        if (imagenIris == null) yield break;

        imagenIris.gameObject.SetActive(true);
        imagenIris.color = Color.white;

        Material matIris = new Material(imagenIris.material);
        imagenIris.material = matIris;

        float radioInicial = 0f;
        float radioFinal = 1.5f;
        float tiempo = 0f;

        while (tiempo < tiempoCierre)
        {
            tiempo += Time.deltaTime;
            float radioActual = Mathf.Lerp(radioInicial, radioFinal, tiempo / tiempoCierre);
            matIris.SetFloat("_Radius", radioActual);
            yield return null;
        }

        matIris.SetFloat("_Radius", radioFinal);
        imagenIris.gameObject.SetActive(false);
    }

    public void MostrarResumen()
    {
        nivelActivo = false;

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

        StartCoroutine(AnimarPopup());
    }

    private IEnumerator AnimarPopup()
    {
        Vector2 posicionOculta = new Vector2(0, -1500f);
        Vector2 posicionCentro = Vector2.zero;
        panelResumen.anchoredPosition = posicionOculta;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * velocidadDeslizamiento;
            float curvaSuave = t * t * (3f - 2f * t);
            panelResumen.anchoredPosition = Vector2.Lerp(posicionOculta, posicionCentro, curvaSuave);
            yield return null;
        }

        panelResumen.anchoredPosition = posicionCentro;
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
                tiempo += Time.deltaTime;
                float radioActual = Mathf.Lerp(radioInicial, radioFinal, tiempo / tiempoCierre);
                matIris.SetFloat("_Radius", radioActual);
                yield return null;
            }

            matIris.SetFloat("_Radius", radioFinal);
            yield return new WaitForSeconds(0.5f);
        }

        if (esMenu)
        {
            MenuNiveles.Niveles = true;
            volverNiveles = true;
        }

        SceneManager.LoadScene(nombreEscena);
    }
}