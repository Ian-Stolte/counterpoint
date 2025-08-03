using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private float comboCD;
    private int lastPlayer;
    private float comboTimer;
    [HideInInspector] public int comboMeter;

    [SerializeField] private Material[] comboMats;
    [SerializeField] private GameObject[] comboIcons;

    [Header("HP")]
    [SerializeField] private float hp;


    private void Update()
    {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0)
        {
            comboMeter = 0;
            lastPlayer = 0;
            GetComponent<MeshRenderer>().material = comboMats[0];
            foreach (Transform child in transform.GetChild(0))
                Destroy(child.gameObject);
        }
    }


    public void TakeDamage(int dmg, int playerNum, PlayerController script)
    {
        float comboMultiplier = 1 + comboMeter * 0.15f;
        hp -= dmg * comboMultiplier;
        //TODO: update HP bar
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
            GetComponent<MeshRenderer>().material = comboMats[playerNum];
            //give special charge to attacking player
            if (comboMeter == 6)
                script.SpecialMeter(10);
            else
                script.SpecialMeter(5);
        }
    }
}
