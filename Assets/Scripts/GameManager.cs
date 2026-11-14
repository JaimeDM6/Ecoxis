using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

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

    private void Start()
    {
        StartCoroutine(GameStart());
    }

    private void Awake()
    {
        if (instancia == null) instancia = this;
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
        StartCoroutine(RutinaGameOver());
    }

    private IEnumerator RutinaGameOver()
    {
        if (imagenIris == null) yield break;

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
        MenuNiveles.Niveles = true;
        SceneManager.LoadScene(escenaMenu);
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
}