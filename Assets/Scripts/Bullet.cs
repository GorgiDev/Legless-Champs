using NUnit.Framework;
using PurrNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkIdentity
{
    public float lifeTime = 5f;
    [SerializeField] private LayerMask ground;
    [SerializeField] private Collider2D shooterCol;
    [SerializeField] private ParticleSystem bulletImpactPSPref;

    private Gun gunScript;
    private int bulletDmg;
    private string shooterTag;
    private bool wasShotFromSniper;
    

    public void InitializePlayer(Collider2D shooterCol, Gun.GunType gunType, Gun gunScript)
    {
        this.gunScript = gunScript;

        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null || shooterCol == null)
            return;

        GameObject shooterGO = shooterCol.gameObject;
        Collider2D[] shooterCols = shooterGO.GetComponents<Collider2D>();

        foreach (Collider2D col in shooterCols)
        {
            Physics2D.IgnoreCollision(myCol, col, true);
        }

        shooterTag = shooterGO.tag;

        if (shooterGO.CompareTag(Tags.Enemies.Turret))
            bulletDmg = 10;
        else
            bulletDmg = 5;

        this.shooterCol = shooterCol;

        switch (gunType)
        {
            case Gun.GunType.rifle:
                bulletDmg = 30;
                wasShotFromSniper = false;
                break;
            case Gun.GunType.shotgun:
                bulletDmg = 10;
                wasShotFromSniper = false;
                break;
            case Gun.GunType.pistol:
                bulletDmg = 20;
                wasShotFromSniper = false;
                break;
            case Gun.GunType.sniper:
                bulletDmg = 50;
                wasShotFromSniper = true;
                break;
        }
    }

    public void InitializeEnemy(Collider2D shooterCol) 
    {
        wasShotFromSniper = false;

        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol == null || shooterCol == null)
            return;

        GameObject shooterGO = shooterCol.gameObject;
        Collider2D[] shooterCols = shooterGO.GetComponents<Collider2D>();

        foreach (Collider2D col in shooterCols)
        {
            Physics2D.IgnoreCollision(myCol, col, true);
        }

        shooterTag = shooterGO.tag;

        if (shooterGO.CompareTag(Tags.Enemies.Turret))
            bulletDmg = 10;
        else
            bulletDmg = 5;

        this.shooterCol = shooterCol;
    }

    private void Awake()
    {
        StartCoroutine(DestroyAfterTime());
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        string tag = col.gameObject.tag;
        int pierce = 0;

        if (col.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = col.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
                player.TakeDamage(bulletDmg);
            Destroy(gameObject);
        }
        else if (Tags.Enemies.isEnemy(tag))
        {
            if (Tags.Enemies.isEnemy(shooterTag))
            {
                PlayBulletImpactParticle();
                Destroy(gameObject);
                return;
            }

            if (col.gameObject.CompareTag(Tags.Enemies.Turret))
            {
                EnemyTurret turret = col.gameObject.GetComponentInChildren<EnemyTurret>();
                if (turret != null)
                    turret.GetDisabled();
                if (!wasShotFromSniper)
                    Destroy(gameObject);
                else
                {
                    pierce++;
                    if (pierce == gunScript.enemiesCanPierce)
                        Destroy(gameObject);
                }
            }
            else if (col.gameObject.CompareTag(Tags.Enemies.DroneScrambler))
            {
                EnemyScrambler scrambler = col.gameObject.GetComponent<EnemyScrambler>();
                if (scrambler != null)
                    scrambler.TakeDamage(bulletDmg);
                if (!wasShotFromSniper)
                    Destroy(gameObject);
                else
                {
                    pierce++;
                    if (pierce == gunScript.enemiesCanPierce)
                        Destroy(gameObject);
                }
            }
            else if (col.gameObject.CompareTag(Tags.Enemies.Router))
            {
                EnemyRouter router = col.gameObject.GetComponent<EnemyRouter>();
                if (router != null)
                    router.TakeDamage(bulletDmg);
                if (!wasShotFromSniper)
                    Destroy(gameObject);
                else
                {
                    pierce++;
                    if (pierce == gunScript.enemiesCanPierce)
                        Destroy(gameObject);
                }
            }
        }
        else if (col.gameObject.CompareTag(Tags.Other.BreakableBox))
        {
            BreakableBox breakableBox = col.gameObject.GetComponent<BreakableBox>();
            
            breakableBox.Shatter();
            Destroy(gameObject);
        }
        else if (col.gameObject.CompareTag(Tags.Other.ExplosiveBarrel))
        {
            ExplosiveBarrel explosiveBarrel = col.gameObject.GetComponent<ExplosiveBarrel>();
            
            explosiveBarrel.Explode();
            Destroy(gameObject);
        }
        else if (((1 << col.gameObject.layer) & ground) != 0)
        {
            PlayBulletImpactParticle();
            Destroy(gameObject);
        }
    }

    private void PlayBulletImpactParticle()
    {
        if (bulletImpactPSPref == null)
            return;

        ParticleSystem ps = Instantiate(bulletImpactPSPref, transform.position, transform.rotation);
        var main = ps.main;

        ps.Play();

        Destroy(ps.gameObject, main.startLifetime.constantMax + 0.1f);
    }
}
