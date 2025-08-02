using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class Player1 : PlayerController
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
        while (elapsed < 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(attackDir.x, 0, attackDir.z)), rotationSpeed * 2f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.LookRotation(new Vector3(attackDir.x, 0, attackDir.z));

        bool moving = moveDir.magnitude > 0.2f;
        float charge = Mathf.Min(chargeTimer, 2);
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
        foreach (Collider hit in hits)
        {
            // deal damage to all enemies & knock back in targeted direction
            Vector3 kbDir = attackDir;
            if (targetAlly)
            {
                kbDir = (ally.position - transform.position).normalized;
            }
            float kbForce = attackKB + attackKB*0.8f*charge;
            hit.GetComponent<Rigidbody>().AddForce(kbDir * kbForce);
            hit.GetComponent<Enemy>().TakeDamage((int)Mathf.Round(0 + 0*charge), 1, this);
        }
        if (hits.Length > 0)
            Instantiate(impactVFX, transform.position + attackDir*2, Quaternion.identity);
        // if targetAlly, position yourself so enemies are between you and ally (TODO: figure out exactly how)

            yield return new WaitForSeconds(0.2f + 0.2f * charge); //lock controls to let anim finish (duration dependent on attack type)
        Physics.IgnoreLayerCollision(6, 7, false);
        attackHitbox.SetActive(false);
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