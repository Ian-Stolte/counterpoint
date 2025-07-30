using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class Player1 : PlayerController
{
    public override void StartAttackCharge()
    {
        base.StartAttackCharge();
        //animate charge-up
    }

    public override IEnumerator Attack()
    {
        StartCoroutine(base.Attack());
        //start animation

        yield return new WaitForSeconds(0.1f);

        bool moving = moveDir.magnitude > 0.2f;
        string target = (targetAlly) ? "partner" : "moveDir";
        Debug.Log("P1: " + chargeTimer + " charge | targeting " + target + " | " + moving + " | ");

        if (moving)
        {
            //dash in moveDir before attacking
        }

        //check hitbox for any enemies
            // deal damage to all enemies & knock back in targeted direction
            // if targetAlly, position yourself so enemies are between you and ally (TODO: figure out exactly how)

        yield return new WaitForSeconds(0.5f); //lock controls to let anim finish (duration dependent on attack type)
        attacking = false;
    }

    
    public override IEnumerator Dash()
    {
        StartCoroutine(base.Dash());
        //start dash anim

        yield return new WaitForSeconds(0.05f);

        string target = (targetAlly) ? "partner" : "moveDir";
        Debug.Log("P2 DASH: | " + target + " |");

        if (targetAlly)
        {
            //rotate to face ally
        }
        //apply force

        yield return new WaitForSeconds(0.3f); //lock controls to let anim finish
        dashing = false;
    }
}