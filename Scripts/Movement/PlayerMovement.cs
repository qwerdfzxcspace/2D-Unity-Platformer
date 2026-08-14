using System;
using System.Collections;
using Unity.Hierarchy;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/* <summary>
Этот класс является CharacterController, который управляет прыжками, передвижением, отскокам от стен, звуками и анимациями передвежения персонажа
</summary> */

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float wallCheckDistance = 10f;
    private int wallLayer = 1 << 6;
    private bool isWallLeft;
    private bool isWallRight;
    [SerializeField] private float startWallSlidingVelocity; // скорость слайда от стен при старте
    [SerializeField] private float maxWallSlidingVelocity; // максимальная скорость слайда от стен
    [SerializeField] private float newmaxWallSlidingVelocity;
    [SerializeField] private float lerpFactor;
    [SerializeField] private float breakTime;
    [SerializeField] private float dashForce;
    [SerializeField] private float DashDuration;
    [SerializeField] private float DashCD;
    [SerializeField] private float maxFallingVelocity; // максимальная скорость падения
    [SerializeField] private float moveSpeed = 1.1f;
    [SerializeField] private float jumpForce = 2.7f;
    [SerializeField] private float XWallJump = 1f; // Сила прыжка от стены по оси X
    [SerializeField] private float YWallJump = 2.7f; // Сила прыжка от стены по оси Y
    [SerializeField] private float StopSpeed;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpStartSound;
    [SerializeField] private AudioClip jumpLandSound;
    [SerializeField] private AudioClip dashSound;

    // Переменные состояний персонажа
    private bool flipped;
    private bool canWallJump;
    private bool inDash;
    private bool InWallJump;
    private float dirX;
    private float dirY;
    private bool canJump = false;
    private bool canDash = true;

    private ActionsClass controls;
    private Vector2 lastposition = new Vector2(0, 0);
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private SpriteRenderer spriterenenderer;
    
    void Awake()
    {
        Application.targetFrameRate = 144;
        controls = new ActionsClass();

        // Соеденение с InputSystem
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => Jump();
        controls.Player.Dash.performed += ctx => StartDash();
    }

    void OnEnable()
    {
        controls.Enable();
        Groundcheck.OnGround += CheckGround;
        wallcheck.OnWall += CheckWall;
    }

    void OnDisable()
    {
        controls.Disable();
        Groundcheck.OnGround -= CheckGround;
        wallcheck.OnWall -= CheckWall;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriterenenderer = transform.GetComponentInChildren<SpriteRenderer>();
    }

    void StartDash()
    {
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        if (canDash)
        {
            audioSource.PlayOneShot(dashSound);
            canDash = false;
            inDash = true;
            float oldGravity = rb.gravityScale;
            rb.gravityScale = 0;

            if (moveInput.y > 0.5)
            {
                rb.linearVelocity = moveInput * (dashForce * 0.75f);
            }
            else if (!canJump)
            {
                rb.linearVelocity = moveInput * (dashForce * 1.15f);
            }
            yield return new WaitForSeconds(DashDuration);
            inDash = false;
            rb.gravityScale = oldGravity;
        }
    }
    
    void Jump()
    {
        if (canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            audioSource.PlayOneShot(jumpStartSound);
        }
        else if (canWallJump)
        {
            audioSource.PlayOneShot(jumpStartSound);
            if (InWallJump == false)
            {
                WhatWall();
                
                if (isWallLeft)
                {
                    rb.linearVelocity = new Vector2(XWallJump, YWallJump);
                    StartCoroutine(OnJump(0.2f));
                }

                if (isWallRight)
                {
                    rb.linearVelocity = new Vector2(-XWallJump, YWallJump);
                    StartCoroutine(OnJump(0.2f));
                }
            }
        }
    }
    
    IEnumerator OnJump(float duration)
    {
        InWallJump = true;
        yield return new WaitForSeconds(duration);
        InWallJump = false;
    }

    void FixedUpdate()
    {
        if (!InWallJump) // При WallJump-е нельзя менять направление персонажа
        {
            if (moveInput.x != 0)
            {
                if (math.sign(moveInput.x) == math.sign(rb.linearVelocity.x) || math.abs(rb.linearVelocity.x) < 0.01f) // Если идет в то же направление что и скорость персонажа, то постепенно ускоряется
                {
                    animator.SetBool("isRunning", true);
                    if (math.abs(rb.linearVelocity.x) < moveSpeed)
                    {
                        float x = moveInput.x * moveSpeed;
                        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
                    }
                    else // Если на полу
                    {
                        {
                            float x = rb.linearVelocity.x + ((moveInput.x * moveSpeed) - rb.linearVelocity.x) * lerpFactor * Time.deltaTime;
                            rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
                        }
                    }
                }
                else // Если не идет в то же направление что и скорость персонажа, то постепенно останавливается
                {
                    animator.SetBool("isRunning", true);
                    if (!canJump)
                    {
                        float x = rb.linearVelocity.x + moveInput.x * breakTime * Time.fixedDeltaTime;
                        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
                    }
                    else
                    {
                        float x = moveInput.x * moveSpeed;
                        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
                    }
                }
            }   else // Если импута игрока нет
            {
                animator.SetBool("isRunning", false);
                rb.linearVelocity = new Vector2(Mathf.MoveTowards(rb.linearVelocity.x, 0, StopSpeed * Time.fixedDeltaTime), rb.linearVelocity.y);
            }
        }
        
        if (rb.linearVelocity.y < -maxFallingVelocity) // Если скорость падения меньше, чем максимальная скорость падения то ускоряется вниз
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallingVelocity);
        }
        
        if (canDash == false) // Если после дэша находится на полу, то ДэшКД сбрасывается
        {
            if (canJump == true && inDash == false)
            {
                canDash = true;
            }
        }
    }

    private void Update()
    {
        if (canWallJump) // Если находится у стены, то проверяет максимальную скорость слайда и ускоряет при направлении по оси Y вниз
        {
            if (!canJump)
            {
                if (rb.linearVelocity.y < -startWallSlidingVelocity)
                {
                    if (newmaxWallSlidingVelocity < maxWallSlidingVelocity)
                    {
                        newmaxWallSlidingVelocity *= Mathf.Pow(1.05f, Time.deltaTime * 60f);
                    }
                    else if (newmaxWallSlidingVelocity > maxWallSlidingVelocity)
                    {
                        newmaxWallSlidingVelocity = maxWallSlidingVelocity;
                    }

                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -newmaxWallSlidingVelocity);
                }
            }
            else // Если стоит у подножия стены, то скорость слайда не меняется
            {
                newmaxWallSlidingVelocity = startWallSlidingVelocity;
            }
        }

        dirY = rb.linearVelocityY;
        
        animator.SetFloat("Slidespeed", newmaxWallSlidingVelocity);

        if (newmaxWallSlidingVelocity > 1.01 && flipped == false) // Отражает модель персонажа относительно стены
        {
            spriterenenderer.flipX = !spriterenenderer.flipX;
            flipped = true;
            WhatWall();
            if (isWallRight)
            {
                spriterenenderer.transform.position = new Vector3(spriterenenderer.transform.position.x + 0.22f,spriterenenderer.transform.position.y,spriterenenderer.transform.position.z);
            }
        }
        else if (newmaxWallSlidingVelocity < 1.01 && flipped == true)
        {
            spriterenenderer.flipX = !spriterenenderer.flipX;
            if (isWallRight)
            {
                spriterenenderer.transform.position = new Vector3(spriterenenderer.transform.position.x - 0.22f, spriterenenderer.transform.position.y, spriterenenderer.transform.position.z);
            }
            flipped = false;
        }
        
        if (moveInput.x > 0f && flipped == false) // Поворачивает персонажа в сторону ходьбы
        {
            spriterenenderer.flipX = false;
        }
        else if (moveInput.x < 0f && flipped == false)
        {
            spriterenenderer.flipX = true;
        }
        
        if (!canJump) // Смена анимация во время прыжка относительно направления по оси Y
        {
            if (dirY > 3f)
            {
                if (animator.GetInteger("inAir") != 1) 
                {
                    animator.SetInteger("inAir", 1);
                }
            } else if (dirY < -3f)
            {
                if (animator.GetInteger("inAir") != 3) 
                {
                    animator.SetInteger("inAir", 3);
                }
            }
            else
            {
                if (animator.GetInteger("inAir") != 2) 
                {
                    animator.SetInteger("inAir", 2);
                }
            }
        }
        else
        {
            if (animator.GetInteger("inAir") != 0)
            {
                animator.SetInteger("inAir", 0);
            }
        }
    }

    void CheckGround(GameObject other, float groundchecks) // Проверяет на полу или нет и меняет готовность прыжка
    {
        if (groundchecks <= 0)
        {
            canJump = false;
            StopSpeed = 30;
        }
        else
        {
            audioSource.PlayOneShot(jumpLandSound);
            canJump = true;
            StopSpeed = 90;
        }
    }
    
    void CheckWall(GameObject other, float wallchecks) // Проверяет возле стены или нет и меняет готовность walljump-а
    {
        if (wallchecks <= 0)
        {
            canWallJump = false;
            newmaxWallSlidingVelocity = startWallSlidingVelocity;
        }
        else
        {
            canWallJump = true;
        }
    }
    
    void WhatWall() // Проверяет с какой стороны стена относительно персонажа
    {
        RaycastHit2D hitLeft =
            Physics2D.Raycast(
                new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z),
                Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D hitRight =
            Physics2D.Raycast(
                new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z),
                Vector2.right, wallCheckDistance, wallLayer);

        isWallLeft = hitLeft.collider != null;
        isWallRight = hitRight.collider != null;

        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z),
            Vector2.left * wallCheckDistance, Color.red, duration: 2f);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z),
            Vector2.right * wallCheckDistance, Color.blue, duration: 2f);

    }
}