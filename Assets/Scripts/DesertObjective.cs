using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DesertObjective : MonoBehaviour
{
    [Min(1)] public int brotesNecesarios = 5;
    public TextMeshProUGUI textoObjetivo;
    public GameObject panelVictoria;
    public Button botonVolverMenu;

    public int BrotesRecogidos { get; private set; }
    public bool Completado { get; private set; }
    private Coroutine mensajeTemporal;

    private void Awake()
    {
        Time.timeScale = 1f;
        BrotesRecogidos = 0;
        Completado = false;
        // El contador vuelve a estar visible al iniciar o repetir el nivel.
        if (textoObjetivo != null) textoObjetivo.gameObject.SetActive(true);
        ActualizarTexto();
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (botonVolverMenu != null)
        {
            botonVolverMenu.onClick.RemoveListener(VolverAlMenu);
            botonVolverMenu.onClick.AddListener(VolverAlMenu);
        }
    }

    public void RecogerBrote()
    {
        if (Completado || BrotesRecogidos >= brotesNecesarios) return;
        BrotesRecogidos++;
        if (mensajeTemporal != null)
        {
            StopCoroutine(mensajeTemporal);
            mensajeTemporal = null;
        }
        ActualizarTexto();
    }

    public void IntentarCompletar()
    {
        if (Completado) return;
        if (BrotesRecogidos < brotesNecesarios)
        {
            if (mensajeTemporal != null) StopCoroutine(mensajeTemporal);
            mensajeTemporal = StartCoroutine(MostrarFaltanBrotes());
            return;
        }

        GameManager manager = GameManager.instancia;
        bool tieneResumen = manager != null && manager.panelResumen != null;
        if (!tieneResumen && panelVictoria == null)
        {
            Debug.LogError("Asigna el Panel Resumen o el Panel Victoria antes de completar el Desierto.");
            return;
        }
        Completado = true;
        if (mensajeTemporal != null)
        {
            StopCoroutine(mensajeTemporal);
            mensajeTemporal = null;
        }
        // Solo se oculta el texto, no el Canvas que contiene el resumen.
        if (textoObjetivo != null) textoObjetivo.gameObject.SetActive(false);
        if (manager != null) manager.MostrarResumen();
        if (!tieneResumen) panelVictoria.SetActive(true);
        Time.timeScale = 0f;
    }

    private IEnumerator MostrarFaltanBrotes()
    {
        if (textoObjetivo != null)
            textoObjetivo.text = "¡FALTAN " + (brotesNecesarios - BrotesRecogidos) + " BROTES!";
        yield return new WaitForSecondsRealtime(2f);
        if (!Completado) ActualizarTexto();
        mensajeTemporal = null;
    }

    private void ActualizarTexto()
    {
        if (textoObjetivo != null)
            textoObjetivo.text = "BROTES " + BrotesRecogidos + "/" + brotesNecesarios + "   ·   LLEGA A LA ESTACIÓN";
    }

    public void VolverAlMenu()
    {
        if (GameManager.instancia != null)
        {
            GameManager.instancia.VolverAlMenu();
            return;
        }
        Time.timeScale = 1f;
        MenuNiveles.Niveles = true;
        GameManager.volverNiveles = true;
        SceneManager.LoadScene("00_UI");
    }
}
