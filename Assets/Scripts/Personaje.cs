using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class Personaje : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 12f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    private float inputHorizontal;
    private bool ground = false;
    private bool died = false;
    private bool hit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
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

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        Animations();
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
                ground = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ground = false;
    }

    public void MoveLeft() { inputHorizontal = -1f; }
    public void MoveRight() { inputHorizontal = 1f; }
    public void Parar() { inputHorizontal = 0f; }

    public void Jump()
    {
        if (ground && !died && !hit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            anim.SetTrigger("Jump");
            ground = false;
        }
    }

    private void Animations()
    {
        anim.SetFloat("VelocidadX", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("EnSuelo", ground);
    }

    public void Hit()
    {
        if (died || hit) return;

        hit = true;
        anim.SetTrigger("Hit");

        float direccionEmpuje = sprite.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(direccionEmpuje * 5f, 5f);

        Invoke(nameof(Recover), 0.5f);
    }

    private void Recover() { hit = false; }

    public void Morir()
    {
        if (died) return;

        died = true;
        anim.SetTrigger("Die");
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
    }
}