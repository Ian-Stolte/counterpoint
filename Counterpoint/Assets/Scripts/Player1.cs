using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;

public class Player1 : PlayerController
{
    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackKB;

    [Header("Special")]
    [SerializeField] private GameObject specialHitbox;
    private Rigidbody draggedEnemy;


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

        // deal damage to all enemies & knock back in targeted direction
        foreach (Collider hit in hits)
        {
            Vector3 horizKB = (targetAlly) ? (ally.position - transform.position).normalized : attackDir;
            horizKB *= 1 + (0.8f*charge); //knock back further if longer charge
            float comboMultiplier = 1 + hit.GetComponent<Enemy>().comboMeter * 0.15f;

            //if jump key pressed, knock enemies into the air
            if (jumpInputDelay > 0f)
            {
                Vector3 upwardKB = Vector3.up * (1 + (0.4f * charge));
                if (!moving)
                    hit.GetComponent<Rigidbody>().AddForce(upwardKB * attackKB * comboMultiplier, ForceMode.Impulse);
                else
                    hit.GetComponent<Rigidbody>().AddForce((horizKB * 0.5f + upwardKB) * attackKB * comboMultiplier, ForceMode.Impulse);
            }
            else
            {
                hit.GetComponent<Rigidbody>().AddForce(horizKB * attackKB * comboMultiplier, ForceMode.Impulse);
            }
            hit.GetComponent<Enemy>().TakeDamage((int)Mathf.Round(0 + 0*charge), 1, this);
        }
        if (hits.Length > 0)
            Instantiate(impactVFX, transform.position + attackDir*2, Quaternion.identity);
        //TODO: if targetAlly, position yourself so enemies are between you and ally?
        
        jumpInputDelay = 0;

        yield return new WaitForSeconds(0.2f + 0.2f * charge); //lock controls to let anim finish
        //Physics.IgnoreLayerCollision(6, 7, false);
        attackHitbox.SetActive(false);
        attacking = false;
    }



    public override IEnumerator Special()
    {
        StartCoroutine(base.Special());
        //snap to nearby enemies, prioritizing ones in the direction we're facing
        Transform closestEnemy = SnapToEnemies(6);
        Vector3 lookDir = (moveDir == Vector3.zero) ? transform.forward : moveDir;
        Vector3 attackDir = (closestEnemy == transform) ? lookDir : (closestEnemy.position - transform.position).normalized;

        //look in attack direction
        float elapsed = 0;
        while (elapsed < 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(attackDir.x, 0, attackDir.z)), rotationSpeed * 2f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.LookRotation(new Vector3(attackDir.x, 0, attackDir.z));

        //dash forward
        Physics.IgnoreLayerCollision(6, 7, true);
        GetComponent<TrailRenderer>().emitting = true;
        draggedEnemy = null;

        elapsed = 0;
        while (elapsed < specialTime)
        {
            Vector3 dashVel = attackDir * specialDashForce * (-Mathf.Pow((elapsed/specialTime), 2) + 1);
            rb.velocity = dashVel;
            if (draggedEnemy != null)
                draggedEnemy.velocity = dashVel;

            //check for enemy to drag
            Bounds b = specialHitbox.GetComponent<BoxCollider>().bounds;
            Collider[] hits = Physics.OverlapBox(b.center, b.extents, Quaternion.identity, LayerMask.GetMask("Enemy"));
            if (draggedEnemy == null && hits.Length > 0)
            {
                foreach (Collider hit in hits)
                {
                    if (hit.GetComponent<Rigidbody>() != null)
                    {
                        draggedEnemy = hit.GetComponent<Rigidbody>(); //TODO: could use better logic for enemies hit in same frame
                        break;
                    }
                }
            }

            //if hit a wall, stop dash and dmg + kb dragged enemy
            b = attackHitbox.GetComponent<BoxCollider>().bounds;
            if (Physics.OverlapBox(b.center, b.extents, Quaternion.identity, LayerMask.GetMask("Terrain")).Length > 0)
            {
                draggedEnemy.GetComponent<Enemy>().TakeDamage(specialDmg, 1, this);
                draggedEnemy.velocity = Vector2.zero;
                Vector3 kbDir = -attackDir + new Vector3(0, 0.3f, 0);
                draggedEnemy.AddForce(kbDir * specialKB, ForceMode.Impulse);
                //stun enemy
                Instantiate(specialVFX, transform.position + attackDir, Quaternion.identity);
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;
        GetComponent<TrailRenderer>().emitting = false;

        yield return new WaitForSeconds(0.1f); //lock controls to let anim finish
        //Physics.IgnoreLayerCollision(6, 7, false);
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
        //Physics.IgnoreLayerCollision(6, 7, false);
        dashing = false;
    }
}