using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum HeroStates
{
    idle,
    run,
    jump,
    jumpDown,
    fire
}

/**
 * Внимание, движущиеся платформы должны иметь тег FloatingPlatform.
 */
public class Hero : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float jumpForce = 15f;
    bool isGrounded = false;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sprite;

    private HeroStates State
    {
        get { return (HeroStates)anim.GetInteger("State"); }
        set { anim.SetInteger("State", (int)value); }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (Input.GetButton("Fire1")) Fire();
        if (Input.GetButton("Horizontal")) Run();
        if (isGrounded && Input.GetButtonDown("Jump")) Jump();

        // Перезагрузка сцены, если персонаж улетел в пропасть.
        if (gameObject.transform.position.y < -25)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void FixedUpdate()
    {
        Grounded();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Решение проблемы с движущимися платформами:
        // https://qna.habr.com/q/500426
        // https://youtu.be/oHjXmnF8mj0?t=316
        if (collision.gameObject.tag.Equals("FloatingPlatform"))
        {
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("FloatingPlatform"))
        {
            transform.parent = null;
        }
    }

    void Run()
    {
        if (isGrounded) State = HeroStates.run;

        Vector3 dir = transform.right * Input.GetAxis("Horizontal");

        transform.position = (
            Vector3.MoveTowards(
                transform.position,
                transform.position + dir,
                speed * Time.deltaTime
            )
        );

        // Изменение направления спрайта, чтобы персонаж смотрел в ту же сторону, куда передвигается.
        sprite.flipX = dir.x < 0.0f;
    }

    void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }

    void Fire()
    {
        State = HeroStates.fire;
    }

    void Grounded()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        isGrounded = collider.Length > 1;

        if (isGrounded) State = HeroStates.idle;
        if (!isGrounded) State = HeroStates.jump;
    }
}
