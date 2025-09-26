using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.Users;

public class PlayerController : MonoBehaviour
{
    private PlayerControls controls;
    protected Rigidbody rb;
    protected Animator anim;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    protected float slowFactor = 1;
    [SerializeField] protected float rotSpeed;
    protected Vector3 moveDir;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private bool canAirAttack;

    [Header("Jump")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float upGravity;
    [SerializeField] private float downGravity;
    [SerializeField] private float hangGravity;
    [SerializeField] private float hangPoint;

    [HideInInspector] public bool grounded;
    protected int airJumps;
    private float jumpDelay;
    protected float jumpInputDelay;

    [Header("Dash")]
    [SerializeField] protected float dashTime;
    [SerializeField] protected float dashForce;

    [SerializeField] private float dashCD;
    private float dashDelay;
    private float dashInputDelay;
    protected bool dashing;
    private Material baseMat;
    [SerializeField] private Material noDashMat;

    [SerializeField] private CanvasGroup dashIndicator;
    [SerializeField] private MeshRenderer face;

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
    [SerializeField] protected GameObject impactVFX;

    [Header("Special")]
    [SerializeField] private float specialCD;
    private float specialDelay;
    private float specialInputDelay;

    [SerializeField] private float specialCost;
    [SerializeField] private Image specialFill;
    protected float specialPct;
    [SerializeField] private GameObject specialSparks;

    [SerializeField] protected float specialTime;
    [SerializeField] protected float specialDashForce;
    [SerializeField] protected float specialKB;
    [SerializeField] protected int specialDmg;
    [SerializeField] protected GameObject specialVFX;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = transform.GetChild(0).GetComponent<Animator>();
        baseMat = face.material;
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

        controls.Player.LeftTrigger.performed += ctx => LeftTriggerPressed();
        controls.Player.LeftTrigger.canceled += ctx => LeftTriggerReleased();

        controls.Player.Special.performed += ctx => SpecialPressed();
    }



    ///
    /// MOVEMENT
    ///

    protected void Update()
    {
        //Ground check
        Bounds b = groundCheck.GetComponent<SphereCollider>().bounds;
        grounded = (Physics.CheckSphere(b.center, 0.5f, LayerMask.GetMask("Ground")) && jumpDelay == 0);
        if (grounded)
            airJumps = 0;
        /*anim.SetBool("airborne", !grounded);
        anim.SetBool("jumpDelay", jumpDelay > 0);
        anim.SetFloat("yVel", rb.velocity.y);*/


        //Dash
        if (!dashing)
            dashDelay -= Time.deltaTime;
        if (grounded || canAirAttack)
            dashInputDelay -= Time.deltaTime;
        if (dashInputDelay > 0 && dashDelay <= 0f && !attacking && (grounded || canAirAttack))
        {
            StartCoroutine(Dash());
        }
        face.material = (dashDelay <= 0f) ? baseMat : noDashMat;
        dashIndicator.alpha = (dashDelay <= 0f) ? 1f : 0.2f;


        //Attack
        if (wantToCharge && attackDelay <= 0 && !dashing && (grounded || canAirAttack)) //start charging once attackCD is up
            StartAttackCharge();
        if (charging)
            chargeTimer += Time.deltaTime;

        if (!charging && !attacking)
            attackDelay -= Time.deltaTime;

        attackInputDelay -= Time.deltaTime;
        if (attackInputDelay > 0f && attackDelay <= 0f && !dashing && (grounded || canAirAttack))
            StartCoroutine(Attack());

        
        //Special
        specialInputDelay -= Time.deltaTime;

        if (!attacking)
            specialDelay -= Time.deltaTime;
        if (specialInputDelay > 0 && specialDelay <= 0f && specialPct >= specialCost && !dashing && !attacking)
            {
                StartCoroutine(Special());
                SpecialMeter(-specialCost);
            }


        //Jump
        jumpDelay = Mathf.Max(0, jumpDelay - Time.deltaTime);
        if (!attacking && !dashing)
            jumpInputDelay = Mathf.Max(0, jumpInputDelay - Time.deltaTime);
        if ((grounded || airJumps > 0) && jumpInputDelay > 0 && jumpDelay <= 0 && !attacking && !dashing && (!charging || canAirAttack))
        {
            jumpDelay = 0.3f;
            Jump();
            airJumps = Mathf.Max(0, airJumps-1);
        }
        

        //Apply gravity
        float gravity = (charging && chargeTimer < 1f) ? -2f : -9.8f; //less hang/down gravity if charging an attack
        if (Mathf.Abs(rb.linearVelocity.y) < hangPoint)
            rb.AddForce(gravity * hangGravity * Vector3.up, ForceMode.Acceleration);
        else if (rb.linearVelocity.y < 0)
            rb.AddForce(gravity * downGravity * Vector3.up, ForceMode.Acceleration);
        else
            rb.AddForce(-9.8f * upGravity * Vector3.up, ForceMode.Acceleration);
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
            rb.MovePosition(rb.position + moveDir * moveSpeed * slowFactor * Time.deltaTime);
            if (moveDir != Vector3.zero)
            {
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
        if (grounded)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpPower, rb.linearVelocity.z);
        else
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpPower*0.8f, rb.linearVelocity.z);
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
        if (!charging)
            slowFactor *= 0.2f;
        charging = true;
        chargeTimer = 0;
    }

    public void AttackReleased() //mark that we want to release an attack --- if attackDelay is up within our inputDelay time, we will attack
    {
        wantToCharge = false;
        attackInputDelay = 0.5f;
        if (charging)
            slowFactor /= 0.2f;
        charging = false;
    }

    public virtual IEnumerator Attack() //actually attack
    {
        attacking = true;
        attackDelay = attackCD;
        yield return null;
    }


    public void SpecialPressed()
    {
        specialInputDelay = 0.5f;
    }

    public virtual IEnumerator Special()
    {
        attacking = true;
        chargeTimer = 0f;
        specialDelay = specialCD;
        specialInputDelay = 0f;
        yield return null;
    }


    /*public virtual void TargetAlly(bool pressed)
    {
        //TODO: show outline or other visual indicator, maybe turn to face ally (while remembering previous orientation)?
        if (ally != null)
            targetAlly = pressed;
    }*/

    public virtual void LeftTriggerPressed() { Debug.Log("Left Trigger pressed!"); }
    public virtual void LeftTriggerReleased() { Debug.Log("Left Trigger pressed!"); }

    


    ///
    /// HELPER FUNCTIONS
    /// 

    protected Transform SnapToEnemies(float checkDist)
    {
        Vector3 lookDir = (moveDir == Vector3.zero) ? transform.forward : moveDir;

        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, checkDist, LayerMask.GetMask("Enemy"));
        Collider closestEnemy = null;
        float closestDist = float.MaxValue;

        // First, check within 90 degrees of facing direction
        foreach (var enemy in nearbyEnemies)
        {
            if (Mathf.Abs(enemy.transform.position.y - transform.position.y) > 0.5f)
                continue;
            Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(lookDir, toEnemy);
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (angle <= 45f && dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = enemy;
            }
        }

        // If none found, check within 180 degrees
        if (closestEnemy == null)
        {
            foreach (var enemy in nearbyEnemies)
            {
                if (Mathf.Abs(enemy.transform.position.y - transform.position.y) > 1.3f)
                    continue;
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(lookDir, toEnemy);
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (angle <= 90f && dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = enemy;
                }
            }
        }

        return (closestEnemy == null) ? transform : closestEnemy.transform; //return self if no enemy to prevent null references
    }


    public void SpecialMeter(float addedPct)
    {
        if (addedPct == 0 || (specialPct >= 100 && addedPct > 0))
            return;

        specialPct = Mathf.Min(100, specialPct + addedPct);
        specialFill.fillAmount = specialPct / 100;
        specialFill.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.Round(specialPct) + "%";

        specialSparks.SetActive(specialPct >= specialCost) ;
    }
}