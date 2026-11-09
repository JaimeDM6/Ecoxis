using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuNiveles : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelNiveles;

    [Header("Efecto Transición")]
    public CanvasGroup panelTransicion;
    public float velocidadFundido = 3f;

    [Header("Tarjetas de Nivel")]
    public Button[] botonesNivel;
    public GameObject[] iconosCandado;

    void Start()
    {
        panelPrincipal.SetActive(true);
        panelNiveles.SetActive(false);
        panelTransicion.alpha = 0f;
        panelTransicion.blocksRaycasts = false;

        ConfigurarCandados();
    }

    public void MostrarSeleccionNiveles()
    {
        StartCoroutine(EfectoTransicion(panelPrincipal, panelNiveles));
    }

    public void MostrarMenuPrincipal()
    {
        StartCoroutine(EfectoTransicion(panelNiveles, panelPrincipal));
    }

    private IEnumerator EfectoTransicion(GameObject panelSaliente, GameObject panelEntrante)
    {
        panelTransicion.blocksRaycasts = true;

        while (panelTransicion.alpha < 1f)
        {
            panelTransicion.alpha += Time.deltaTime * velocidadFundido;
            yield return null;
        }

        panelSaliente.SetActive(false);
        panelEntrante.SetActive(true);

        while (panelTransicion.alpha > 0f)
        {
            panelTransicion.alpha -= Time.deltaTime * velocidadFundido;
            yield return null;
        }

        panelTransicion.blocksRaycasts = false;
    }

    private void ConfigurarCandados()
    {
        int nivelesDesbloqueados = PlayerPrefs.GetInt("NivelesAbiertos", 1);

        for (int i = 0; i < botonesNivel.Length; i++)
        {
            if (i < nivelesDesbloqueados)
            {
                botonesNivel[i].interactable = true;
                iconosCandado[i].SetActive(false);
            }
            else
            {
                botonesNivel[i].interactable = false;
                iconosCandado[i].SetActive(true);
            }
        }
    }

    public void CargarEscenaNivel(int indiceEscena)
    {
        SceneManager.LoadScene(indiceEscena);
    }
}