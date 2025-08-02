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


    public void TakeDamage(int dmg, int playerNum, PlayerController script)
    {
        if (playerNum != lastPlayer)
        {
            lastPlayer = playerNum;
            comboMeter = Mathf.Min(6, comboMeter + 1);
            comboTimer = comboCD;
            GetComponent<MeshRenderer>().material = comboMats[playerNum];
            if (comboMeter == 6)
                script.SpecialMeter(10);
            else
                script.SpecialMeter(5);
            //show combo on UI
        }

        hp -= dmg;
        //TODO: update HP bar
        if (hp <= 0)
            Destroy(gameObject);
    }
}
