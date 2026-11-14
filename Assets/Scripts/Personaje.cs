using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Personaje : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 12f;

    [Header("Sonidos de Pasos")]
    public AudioClip step1;
    public AudioClip step2;
    public float tiempoEntrePasos = 0.5f;

    private AudioSource audioSource;
    private float temporizadorPasos = 0f;
    private bool tocaPaso1 = true;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private float inputHorizontal;
    private bool ground = false;
    private bool died = false;
    private bool hit = false;
    private float gravedadOriginal;
    private float tiempoSuelo = 0f;

    void Start()
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
        if (died || hit) return;

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            inputHorizontal = Input.GetAxisRaw("Horizontal");
        }
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
                 Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            inputHorizontal = 0f;
        }

        if (inputHorizontal > 0) sprite.flipX = false;
        else if (inputHorizontal < 0) sprite.flipX = true;

        tiempoSuelo -= Time.deltaTime;
        ground = tiempoSuelo > 0f;

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        Animations();
        Steps();
    }

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
        if (ground && !died && !hit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            anim.SetTrigger("Jump");
            tiempoSuelo = 0f;
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
                if (tocaPaso1) audioSource.PlayOneShot(step1);
                else audioSource.PlayOneShot(step2);

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

    public void Hit()
    {
        if (died || hit) return;

        hit = true;
        anim.SetTrigger("Hit");
        inputHorizontal = 0f;
        rb.linearVelocity = Vector2.zero;

        float direccionEmpuje = sprite.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(direccionEmpuje * 4f, 6f);

        Invoke(nameof(Recover), 0.5f);
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