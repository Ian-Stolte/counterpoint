using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class Player1Controller : MonoBehaviour
{
    private PlayerControls controls;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    private Vector3 moveDir;
   
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump")]
    [SerializeField] private float jumpPower;
    private float jumpDelay;
    private float jumpInputDelay;
    [SerializeField] private float upGravity;
    [SerializeField] private float downGravity;
    [SerializeField] private float hangGravity;
    [SerializeField] private float hangPoint;
    [HideInInspector] public bool grounded;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(PlayerControls controls_)
    {
        controls = controls_;

        controls.Player1.Move.performed += ctx => MovePressed(ctx.ReadValue<Vector2>());
        controls.Player1.Move.canceled += ctx => MovePressed(Vector2.zero);

        controls.Player1.Jump.performed += ctx => JumpPressed();
    }


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
        jumpDelay = Mathf.Max(0, jumpDelay-Time.deltaTime);
        jumpInputDelay = Mathf.Max(0, jumpInputDelay-Time.deltaTime);
        if (grounded && jumpInputDelay > 0 && jumpDelay == 0)
        {
            jumpDelay = 0.3f;
            Jump();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);
        if (moveDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
    }


    private void MovePressed(Vector2 input)
    {
        moveDir = new Vector3(input.x, 0, input.y);
    }

    private void JumpPressed()
    {
        jumpInputDelay = 0.3f;
    }
    
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
    }
}