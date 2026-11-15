using System.Collections.Generic;
using UnityEngine;

/// <summary>Trigger de plantación: no mueve al jugador ni modifica colliders o Rigidbody2D.</summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ZonaReforestacion : MonoBehaviour
{
    [Header("Referencias")]
    public MisionesSelva misiones;
    public Sprite arbolRestaurado;

    [Header("Escala exclusivamente visual (opcional)")]
    public bool aplicarEscalaRestaurada = false;
    [Tooltip("Escala final del árbol respecto al padre de la zona; no es un multiplicador de la raíz.")]
    [SerializeField] private Vector3 escalaRestaurada = Vector3.one;

    private SpriteRenderer imagen;
    private bool recuperada;
    private readonly HashSet<Collider2D> contactosJugador = new HashSet<Collider2D>();

    public bool Recuperada => recuperada;
    public bool JugadorCerca
    {
        get
        {
            contactosJugador.RemoveWhere(ContactoInvalido);
            return contactosJugador.Count > 0;
        }
    }

    private static bool ContactoInvalido(Collider2D contacto)
    {
        return contacto == null || !contacto.enabled || !contacto.gameObject.activeInHierarchy;
    }

    private void Awake()
    {
        imagen = GetComponent<SpriteRenderer>();
        // El BoxCollider2D debe configurarse como trigger en la escena, no en este script.
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (recuperada || misiones == null || otro == null)
            return;

        Personaje jugador = otro.GetComponentInParent<Personaje>();
        if (jugador == null || jugador != misiones.personaje)
            return;

        // Varios colliders del mismo jugador no producen salidas prematuras ni progreso doble.
        if (contactosJugador.Add(otro))
            misiones.RegistrarEntrada(this);
    }

    private void OnTriggerExit2D(Collider2D otro)
    {
        if (otro != null && contactosJugador.Remove(otro) && contactosJugador.Count == 0 && misiones != null)
            misiones.RegistrarSalida(this);
    }

    /// <summary>El teclado no se procesa aquí; MisionesSelva controla teclado y botón táctil.</summary>
    public bool Reforestar()
    {
        if (!isActiveAndEnabled || recuperada || !JugadorCerca || misiones == null
            || !misiones.PuedeReforestar || imagen == null || arbolRestaurado == null)
            return false;

        recuperada = true;
        if (!misiones.RegistroZona(this))
        {
            recuperada = false;
            return false;
        }

        imagen.sprite = arbolRestaurado;
        if (aplicarEscalaRestaurada)
            CrearArbolVisualEscalado();

        contactosJugador.Clear();
        misiones.RegistrarSalida(this);
        return true;
    }

    private void CrearArbolVisualEscalado()
    {
        // Escalar la raíz también escalaría el trigger. Un hijo sin collider conserva la física.
        GameObject visual = new GameObject("Arbol restaurado (visual)");
        visual.layer = gameObject.layer;
        visual.transform.SetParent(transform, false);
        Vector3 escalaRaiz = transform.localScale;
        // Una raíz .45 y escala final .55 producen un hijo .55/.45; el collider sigue a .45.
        visual.transform.localScale = new Vector3(
            EscalaRelativa(escalaRestaurada.x, escalaRaiz.x),
            EscalaRelativa(escalaRestaurada.y, escalaRaiz.y),
            EscalaRelativa(escalaRestaurada.z, escalaRaiz.z));
        SpriteRenderer arbol = visual.AddComponent<SpriteRenderer>();
        arbol.sprite = arbolRestaurado;
        arbol.color = imagen.color;
        arbol.sharedMaterials = imagen.sharedMaterials;
        arbol.sortingLayerID = imagen.sortingLayerID;
        arbol.sortingOrder = imagen.sortingOrder;
        arbol.flipX = imagen.flipX;
        arbol.flipY = imagen.flipY;
        arbol.maskInteraction = imagen.maskInteraction;
        imagen.enabled = false;
    }

    private static float EscalaRelativa(float escalaFinal, float escalaRaiz)
    {
        return Mathf.Approximately(escalaRaiz, 0f) ? 1f : escalaFinal / escalaRaiz;
    }

    private void OnDisable()
    {
        contactosJugador.Clear();
        if (misiones != null)
            misiones.RegistrarSalida(this);
    }
}
