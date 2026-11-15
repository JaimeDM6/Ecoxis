using UnityEngine;

/// <summary>Desplaza tres copias del fondo sin intervenir en la cámara ni en las físicas.</summary>
public class ParallaxSelva : MonoBehaviour
{
    public Transform camara;
    public float ancho = 23.04f;
    [Range(0f, 1f)] public float factor = 0.15f;

    private void LateUpdate()
    {
        if (camara == null || ancho <= 0f) return;
        Vector3 posicion = transform.localPosition;
        posicion.x = Mathf.Repeat(-camara.position.x * factor + ancho * 0.5f, ancho) - ancho * 0.5f;
        transform.localPosition = posicion;
    }
}
