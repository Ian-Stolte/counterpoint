using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float upGravity;
    [SerializeField] private float downGravity;
    [SerializeField] private float hangGravity;
    [SerializeField] private float hangPoint;
    [HideInInspector] public bool grounded;
    [SerializeField] private SphereCollider groundCheck;
    private Rigidbody rb;

    [Header("Combo")]
    [SerializeField] private float comboCD;
    private int lastPlayer;
    private float comboTimer;
    [HideInInspector] public int comboMeter;

    [SerializeField] private Material[] comboMats;
    [SerializeField] private GameObject[] comboIcons;

    [Header("HP")]
    [SerializeField] private float hp;

    [Header("Stun")]
    private float stunTimer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform target;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        //Reset combo timer if haven't been hit in a while
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0)
        {
            comboMeter = 0;
            lastPlayer = 0;
            GetComponent<MeshRenderer>().material = comboMats[0];
            foreach (Transform child in transform.GetChild(0))
                Destroy(child.gameObject);
        }

        //Gravity
        Bounds b = groundCheck.GetComponent<SphereCollider>().bounds;
        grounded = (Physics.CheckSphere(b.center, 0.5f, LayerMask.GetMask("Ground")));

        if (Mathf.Abs(rb.velocity.y) < hangPoint)
            rb.AddForce(-9.8f * hangGravity * Vector3.up, ForceMode.Acceleration);
        else if (rb.velocity.y < 0)
            rb.AddForce(-9.8f * downGravity * Vector3.up, ForceMode.Acceleration);
        else
            rb.AddForce(-9.8f * upGravity * Vector3.up, ForceMode.Acceleration);

        //Control rb knockback
        if (rb.velocity.magnitude < 3f)
        {
            rb.velocity = new Vector3(rb.velocity.x*0.8f, rb.velocity.y, rb.velocity.z*0.8f);
        }

        //Show stun
        stunTimer -= Time.deltaTime;
        if (lastPlayer != 0)
        {
            if (stunTimer > 0)
                GetComponent<MeshRenderer>().material = comboMats[lastPlayer];
            else
                GetComponent<MeshRenderer>().material = comboMats[lastPlayer+2];
        }
    }

    private void FixedUpdate()
    {
        //Movement
        if (stunTimer <= 0)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            if (dir.magnitude > 4f)
                rb.MovePosition(rb.position + dir.normalized * moveSpeed * Time.deltaTime);
        }
    }



    public void TakeDamage(int dmg, float stun, int playerNum, PlayerController script)
    {
        float comboMultiplier = 1 + comboMeter * 0.15f;
        hp -= dmg * comboMultiplier;
        //TODO: update HP bar
        stunTimer = stun;
        if (hp <= 0)
            Destroy(gameObject);

        //combo if hit by a different player than the last attack
        if (playerNum != lastPlayer)
        {
            lastPlayer = playerNum;
            //show UI icons
            if (comboMeter < 6)
            {
                GameObject comboIcon = Instantiate(comboIcons[playerNum - 1], transform.position, Quaternion.identity, transform.GetChild(0));
                if (comboMeter % 2 == 0) //shift symbols when starting a new pair
                {
                    foreach (Transform child in transform.GetChild(0))
                    {
                        child.GetComponent<RectTransform>().anchoredPosition += new Vector2(-215, 0);
                    }
                }
                else //flip by 180 degrees when completing a pair
                {
                    Vector3 s = comboIcon.transform.localScale;
                    comboIcon.transform.localScale = new Vector3(-1 * s.x, s.y, s.z);
                    comboIcon.GetComponent<RectTransform>().anchoredPosition += new Vector2(10, 0);
                }
                comboIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(215 * (comboMeter / 2), -160);
            }
            //increment combo
            comboMeter = Mathf.Min(6, comboMeter + 1);
            comboTimer = comboCD;
            //give special charge to attacking player
            if (comboMeter == 6)
                script.SpecialMeter(10);
            else
                script.SpecialMeter(5);
        }
    }
}
