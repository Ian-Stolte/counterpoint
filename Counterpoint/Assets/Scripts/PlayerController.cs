using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    protected Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] protected float rotationSpeed;
    protected Vector3 moveDir;
   
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float upGravity;
    [SerializeField] private float downGravity;
    [SerializeField] private float hangGravity;
    [SerializeField] private float hangPoint;

    [HideInInspector] public bool grounded;
    private float jumpDelay;
    private float jumpInputDelay;

    [Header("Dash")]
    [SerializeField] protected float dashTime;
    [SerializeField] protected float dashForce;

    [SerializeField] private float dashCD;
    private float dashDelay;
    private float dashInputDelay;
    protected bool dashing;

    [Header("Targeting")]
    protected bool targetAlly;
    [SerializeField] protected Transform ally;

    [Header("Attack")]
    [SerializeField] private float attackCD;
    private float attackDelay;
    private float attackInputDelay;
    protected bool attacking;

    private bool wantToCharge;
    protected float chargeTimer;
    protected bool charging;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    //set up key bindings
    public void Initialize(PlayerControls controls_)
    {
        controls = controls_;

        controls.Player.Move.performed += ctx => MovePressed(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => MovePressed(Vector2.zero);

        controls.Player.Jump.performed += ctx => JumpPressed();

        controls.Player.Dash.performed += ctx => DashPressed();
        
        controls.Player.Attack.performed += ctx => AttackPressed();
        controls.Player.Attack.canceled += ctx => AttackReleased();

        controls.Player.ToAlly.performed += ctx => TargetAlly(true);
        controls.Player.ToAlly.canceled += ctx => TargetAlly(false);
    }



    ///
    /// MOVEMENT
    ///

    private void Update()
    {
        //Ground check
        Bounds b = groundCheck.GetComponent<SphereCollider>().bounds;
        grounded = (Physics.CheckSphere(b.center, 0.5f, groundLayer) && jumpDelay == 0);
        /*anim.SetBool("airborne", !grounded);
        anim.SetBool("jumpDelay", jumpDelay > 0);
        anim.SetFloat("yVel", rb.velocity.y);*/


        //Apply gravity
        if (Mathf.Abs(rb.velocity.y) < hangPoint)
            rb.AddForce(-9.8f * hangGravity * Vector3.up, ForceMode.Acceleration);
        else if (rb.velocity.y < 0)
            rb.AddForce(-9.8f * downGravity * Vector3.up, ForceMode.Acceleration);
        else
            rb.AddForce(-9.8f * upGravity * Vector3.up, ForceMode.Acceleration);


        //Jump
        jumpDelay = Mathf.Max(0, jumpDelay - Time.deltaTime);
        jumpInputDelay = Mathf.Max(0, jumpInputDelay - Time.deltaTime);
        if (grounded && jumpInputDelay > 0 && jumpDelay <= 0)
        {
            jumpDelay = 0.3f;
            Jump();
        }


        //Dash
        if (!dashing)
            dashDelay -= Time.deltaTime;
        dashInputDelay -= Time.deltaTime;
        if (dashInputDelay > 0 && dashDelay <= 0f && !attacking)
        {
            StartCoroutine(Dash());
        }


        //Attack
        if (wantToCharge && attackDelay <= 0 && !dashing) //start charging once attackCD is up
            StartAttackCharge();
        if (charging)
            chargeTimer += Time.deltaTime;
            
        if (!charging && !attacking)
            attackDelay -= Time.deltaTime;

        attackInputDelay -= Time.deltaTime;
        if (attackInputDelay > 0f && attackDelay <= 0f && !dashing)
            StartCoroutine(Attack());
    }

    private void MovePressed(Vector2 input)
    {
        moveDir = new Vector3(input.x, 0, input.y);
    }

    private void FixedUpdate()
    {
        //basic movement
        if (!attacking && !dashing)
        {
            float speed = (charging) ? moveSpeed * 0.2f : moveSpeed;
            rb.MovePosition(rb.position + moveDir * speed * Time.deltaTime);
            if (moveDir != Vector3.zero)
            {
                float rotSpeed = (charging) ? rotationSpeed * 0.2f : moveSpeed;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotSpeed * Time.deltaTime);
            }
        }
    }



    ///
    /// ACTIONS
    ///

    private void JumpPressed()
    {
        jumpInputDelay = 0.3f;
    }
    
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
    }


    public void DashPressed()
    {
        dashInputDelay = 0.3f;
    }

    public virtual IEnumerator Dash()
    {
        dashing = true;
        dashDelay = dashCD;
        yield return null;
    }


    public void AttackPressed() //mark that we are holding attack --- as soon as attackDelay is up, we will start charging
    {
        wantToCharge = true;
    }

    public virtual void StartAttackCharge() //actually start charge, including anim & chargeTimer
    {
        wantToCharge = false;
        charging = true;
        chargeTimer = 0;
    }

    public void AttackReleased() //mark that we want to release an attack --- if attackDelay is up within our inputDelay time, we will attack
    {
        wantToCharge = false;
        attackInputDelay = 0.5f;
        charging = false;
    }

    public virtual IEnumerator Attack() //actually attack
    {
        attacking = true;
        attackDelay = attackCD; //TODO: depends on chargeTime & attack type?
        yield return null;
    }


    public void TargetAlly(bool pressed)
    {
        //show outline or other visual indicator, maybe turn to face ally (while remembering previous orientation)
        if (ally != null)
            targetAlly = pressed;
    }
}