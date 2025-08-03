using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class Player2 : PlayerController
{
    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackKB;


    public override void StartAttackCharge()
    {
        base.StartAttackCharge();
        //animate charge-up
    }

    public override IEnumerator Attack()
    {
        StartCoroutine(base.Attack());
        //start animation

        //snap to nearby enemies, prioritizing ones in the direction we're facing
        Transform closestEnemy = SnapToEnemies(4);
        Vector3 lookDir = (moveDir == Vector3.zero) ? transform.forward : moveDir;
        Vector3 attackDir = (closestEnemy == transform) ? lookDir : (closestEnemy.position - transform.position).normalized;

        //look in attack direction
        float elapsed = 0;
        while (elapsed < 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(attackDir.x, 0, attackDir.z)), rotationSpeed * 3f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.LookRotation(new Vector3(attackDir.x, 0, attackDir.z));

        bool moving = moveDir.magnitude > 0.2f;
        float charge = Mathf.Min(chargeTimer, 1);
        chargeTimer = 0;

        //if enemy far away, step toward them
        if (moving && (Vector3.Distance(closestEnemy.position, transform.position) > 2 || closestEnemy == transform))
        {
            elapsed = 0;
            while (elapsed < 0.1f)
            {
                rb.velocity = attackDir * dashForce / 2f * (-Mathf.Pow((elapsed / dashTime), 2) + 1);
                elapsed += Time.deltaTime;
                yield return null;
            }
            rb.velocity = Vector2.zero;
        }

        Physics.IgnoreLayerCollision(6, 7, true); //ignore only self, not ally?

        //check hitbox for any enemies
        attackHitbox.SetActive(true);
        Bounds b = attackHitbox.GetComponent<BoxCollider>().bounds;
        Collider[] hits = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, LayerMask.GetMask("Enemy"));
        
        // deal damage to all enemies & knock back in targeted direction
        foreach (Collider hit in hits)
        {
            Vector3 kbDir = (targetAlly) ? (ally.position - transform.position).normalized : attackDir;
            float comboMultiplier = 1 + hit.GetComponent<Enemy>().comboMeter * 0.15f;
            if (!grounded)
            {
                if (!targetAlly || ally.position.y > transform.position.y)
                    kbDir += new Vector3(0, 0.5f, 0);
                else
                    kbDir *= 1.3f;
            }
            float kbForce = attackKB * (1 + 0.8f * charge) * comboMultiplier;
            hit.GetComponent<Rigidbody>().AddForce(kbDir * kbForce);
            hit.GetComponent<Enemy>().TakeDamage((int)Mathf.Round(0 + 0 * charge), 2, this);
        }
        if (hits.Length > 0)
            Instantiate(impactVFX, transform.position + attackDir, Quaternion.identity);
        //TODO: if targetAlly, position yourself so enemies are between you and ally?

        jumpInputDelay = 0;

        float waitTime = (0.15f + 0.15f*charge);
        //if airborne and hit an enemy, keep height & refresh jump
        if (!grounded && hits.Length > 0)
        {
            elapsed = 0f;
            while (elapsed < waitTime)
            {
                float quadFactor = (Mathf.Pow((elapsed/waitTime), 2) + 0.5f);
                rb.velocity = new Vector3(rb.velocity.x, quadFactor, rb.velocity.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            airJumps++;
        }
        else
        {
            yield return new WaitForSeconds(waitTime); //lock controls to let anim finish
        }
        
        attackHitbox.SetActive(false);
        Physics.IgnoreLayerCollision(6, 7, true);
        attacking = false;
    }


    public override IEnumerator Dash()
    {
        StartCoroutine(base.Dash());
        //start dash anim

        yield return new WaitForSeconds(0.05f);

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
        Physics.IgnoreLayerCollision(6, 7, true);
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
        Physics.IgnoreLayerCollision(6, 7, false);
        dashing = false;
    }
}