using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Combo")]
    public int comboMeter;
    public int lastPlayer;
    public float comboTimer;
    [SerializeField] private float comboCD;
    [SerializeField] private Material[] comboMats;

    [Header("HP")]
    [SerializeField] private int hp;


    private void Update()
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0)
        {
            comboMeter = 0;
            lastPlayer = 0;
            GetComponent<MeshRenderer>().material = comboMats[0];
        }
    }


    public void TakeDamage(int dmg, int playerNum)
    {
        if (playerNum != lastPlayer)
        {
            lastPlayer = playerNum;
            comboMeter++;
            comboTimer = comboCD;
            GetComponent<MeshRenderer>().material = comboMats[playerNum];
        }

        hp -= dmg;
        //TODO: update HP bar
        if (hp <= 0)
            Destroy(gameObject);
    }
}
