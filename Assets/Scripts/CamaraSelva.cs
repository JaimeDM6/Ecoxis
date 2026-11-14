using UnityEngine;

public class CamaraSelva : MonoBehaviour
{
    public Transform objetivo;
    public float suavizado = 0.2f;

    private float velocidadSeguimiento;

    void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 posicion = transform.position;

        posicion.x = Mathf.SmoothDamp(
            posicion.x,
            objetivo.position.x,
            ref velocidadSeguimiento,
            suavizado
        );

        transform.position = posicion;
    }
}
