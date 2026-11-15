using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Personaje : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 7.5f;

    [Header("Sonidos de Pasos")]
    public AudioClip step1;
    public AudioClip step2;
    public AudioClip harm;
    [Range(0f, 1f)] public float volumenPasos = 0.5f;
    [Range(0f, 1f)] public float volumenDano = 0.5f;
    public float tiempoEntrePasos = 0.5f;

    private AudioSource audioSource;
    private float temporizadorPasos = 0f;
    private bool tocaPaso1 = true;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private float inputHorizontal;
    private bool izquierdaTactil;
    private bool derechaTactil;
    private bool ground = false;
    private bool died = false;
    private bool hit = false;
    private float gravedadOriginal;
    private float tiempoSuelo = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        rb.freezeRotation = true;
        gravedadOriginal = rb.gravityScale;
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        tiempoSuelo -= Time.deltaTime;
        ground = tiempoSuelo > 0f;
        if (died || hit) return;
        inputHorizontal = LeerHorizontal();

        if (inputHorizontal > 0) sprite.flipX = false;
        else if (inputHorizontal < 0) sprite.flipX = true;

        if (SaltoTeclado())
        {
            Jump();
        }

        Animations();
        Steps();
    }

    private float LeerHorizontal()
    {
        if (izquierdaTactil || derechaTactil)
            return (derechaTactil ? 1f : 0f) - (izquierdaTactil ? 1f : 0f);
#if ENABLE_INPUT_SYSTEM
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return 0f;
        float derecha = teclado.dKey.isPressed || teclado.rightArrowKey.isPressed ? 1f : 0f;
        float izquierda = teclado.aKey.isPressed || teclado.leftArrowKey.isPressed ? 1f : 0f;
        return derecha - izquierda;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxisRaw("Horizontal");
#else
        return 0f;
#endif
    }

    private bool SaltoTeclado()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump");
#else
        return false;
#endif
    }

    public void PulsarIzquierda(bool pulsado) { izquierdaTactil = pulsado; }
    public void PulsarDerecha(bool pulsado) { derechaTactil = pulsado; }
    // Nombres que esperaban los controles del nivel original.
    public void MoveLeft() { PulsarIzquierda(true); }
    public void MoveRight() { PulsarDerecha(true); }
    public void Parar()
    {
        izquierdaTactil = false;
        derechaTactil = false;
        inputHorizontal = 0f;
    }

    private void OnDisable() { Parar(); }
    private void OnApplicationFocus(bool foco) { if (!foco) Parar(); }
    private void OnApplicationPause(bool pausa) { if (pausa) Parar(); }

    void FixedUpdate()
    {
        if (died)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (!hit)
        {
            rb.linearVelocity = new Vector2(inputHorizontal * velocidad, rb.linearVelocity.y);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (rb == null || rb.linearVelocity.y > 0.1f) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                tiempoSuelo = 0.15f;
                return;
            }
        }
    }

    public void Jump()
    {
        if (rb != null && isActiveAndEnabled && Time.timeScale > 0f && tiempoSuelo > 0f && !died && !hit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            anim.SetTrigger("Jump");
            tiempoSuelo = 0f;
            ground = false;
        }
    }

    private void Animations()
    {
        anim.SetFloat("VelocidadX", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("EnSuelo", ground);
    }

    private void Steps()
    {
        if (temporizadorPasos > 0f)
        {
            temporizadorPasos -= Time.deltaTime;
        }

        if (ground && Mathf.Abs(inputHorizontal) > 0.1f)
        {
            if (temporizadorPasos <= 0f)
            {
                AudioClip paso = tocaPaso1 ? step1 : step2;
                if (paso != null) audioSource.PlayOneShot(paso, volumenPasos);

                tocaPaso1 = !tocaPaso1;
                temporizadorPasos = tiempoEntrePasos;
            }
        }
        else
        {
            if (temporizadorPasos <= 0f)
            {
                tocaPaso1 = true;
            }
        }
    }

    public void Hit() { TryHit(); }

    public bool TryHit()
    {
        if (died || hit || !isActiveAndEnabled || Time.timeScale <= 0f || rb == null) return false;
        hit = true;
        tiempoSuelo = 0f;
        ground = false;
        if (harm != null) audioSource.PlayOneShot(harm, volumenDano);
        anim.SetTrigger("Hit");
        inputHorizontal = 0f;
        rb.linearVelocity = Vector2.zero;

        float direccionEmpuje = sprite.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(direccionEmpuje * 4f, 6f);

        Invoke(nameof(Recover), 0.5f);
        return true;
    }

    private void Recover() { hit = false; }

    public void Die()
    {
        if (died) return;
        died = true;

        CancelInvoke(nameof(Recover));
        anim.ResetTrigger("Hit");
        anim.SetFloat("VelocidadX", 0f);
        inputHorizontal = 0f;

        anim.SetTrigger("Die");
        StartCoroutine(SecuenciaMuerte());
    }

    private IEnumerator SecuenciaMuerte()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(1f);

        rb.gravityScale = gravedadOriginal;
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        if (GameManager.instancia != null)
        {
            GameManager.instancia.GameOver();
        }
    }
}
