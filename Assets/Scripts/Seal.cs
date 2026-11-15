using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Seal : MonoBehaviour
{
    [Header("Configuración de Rescate")]
    public int toquesNecesarios = 3;
    private int toquesActuales = 0;

    [Tooltip("Tiempo de invulnerabilidad entre toques para que Pipo no los quite de golpe")]
    public float cooldownToque = 0.5f;
    private bool puedeSerGolpeada = true;
    private bool rescatada = false;

    [Header("Efecto de Vibración")]
    public float duracionVibracion = 0.2f;
    public float magnitudVibracion = 0.1f;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !rescatada && puedeSerGolpeada)
        {
            toquesActuales++;

            if (toquesActuales < toquesNecesarios)
            {
                StartCoroutine(EfectoImpacto());
            }
            else
            {
                EjecutarRescateFinal();
            }
        }
    }

    private IEnumerator EfectoImpacto()
    {
        puedeSerGolpeada = false;

        Vector3 posicionOriginal = transform.localPosition;
        float tiempo = 0f;

        while (tiempo < duracionVibracion)
        {
            float desplazamientoX = Random.Range(-magnitudVibracion, magnitudVibracion);
            transform.localPosition = new Vector3(posicionOriginal.x + desplazamientoX, posicionOriginal.y, posicionOriginal.z);

            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = posicionOriginal;

        yield return new WaitForSeconds(cooldownToque);
        puedeSerGolpeada = true;
    }

    private void EjecutarRescateFinal()
    {
        rescatada = true;
        anim.SetTrigger("Rescued");
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(LlamarResumen());
    }

    private IEnumerator LlamarResumen()
    {
        yield return new WaitForSeconds(2f);

        if (GameManager.instancia != null)
        {
            GameManager.instancia.MostrarResumen();
        }
    }
}