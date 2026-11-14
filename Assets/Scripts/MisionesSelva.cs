using UnityEngine;
using TMPro;

public class MisionesSelva : MonoBehaviour
{
    public TextMeshProUGUI textoMisiones;
    public int totalZonas = 3;

    private int zonasRecuperadas = 0;

    void Start()
    {
        ActualizarTexto();
    }

    public void RegistrarZona()
    {
        zonasRecuperadas++;
        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (textoMisiones != null)
        {
            textoMisiones.text =
                "Zonas recuperadas: " + zonasRecuperadas + " / " + totalZonas;
        }
    }
}