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

        yield return new WaitForSeconds(0.15f); //wait longer for feeling of heavier dash

        Vector3 dashDir = (moveDir == Vector3.zero) ? transform.forward : moveDir;
        if (targetAlly)
        {
            //rotate to face ally
            dashDir = ally.position - transform.position;
            dashDir.y = Mathf.Min(0, dashDir.y); //can't dash into the air
            dashDir = dashDir.normalized * 1.5f; //dash further if aiming at ally
            transform.rotation = Quaternion.LookRotation(new Vector3(dashDir.x, 0, dashDir.z));
        }

        //perform dash
        GetComponent<TrailRenderer>().emitting = true;
        float elapsed = 0;
        while (elapsed < dashTime)
        {
            rb.velocity = dashDir * dashForce * (-Mathf.Pow((elapsed/dashTime), 2) + 1);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(dashDir.x, 0, dashDir.z)), rotationSpeed * Time.deltaTime);

            elapsed += Time.deltaTime;
            //if targeting ally, avoid passing them
            if (targetAlly && Vector3.Distance(ally.position, transform.position) < 0.5f)
                elapsed += (dashTime-elapsed) * 0.1f;
            yield return null;
        }
        rb.velocity = Vector2.zero;

        GetComponent<TrailRenderer>().emitting = false;
        dashing = false;
    }
}