using System;
using UnityEngine;
using CallbackEvents;

public class GroundPoundContext : EventContext
{
    public Vector3 Location;
    public readonly float Radius;
    public readonly float Damage;
    public readonly bool LinearFalloff;

    public GroundPoundContext(Vector3 location, float radius, float damage, bool linearFalloff)
    {
        Location = location;
        Radius = radius;
        Damage = damage;
        LinearFalloff = linearFalloff;
    }
}

public class ZombieAI : MonoBehaviour
{
    //components
    public Animator animator;
    
    //Animator Tags
    private static readonly int IsAgro = Animator.StringToHash("isAgro");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

    //attack settings
    public float attackDistance; //distance the player can attack from
    public float groundPoundRadius; //the radius at which a ground pound attack lands a hit
    public float groundPoundDamage; //the amount of damage the ground pound attack will do
    public bool groundPoundLinearFalloff; //wether or not to drop the damage linearly based off the distance from the player
    public bool invisible; //if the player is invisible this frame
    public float attackChargeSpeed = 6.0f;
    
    //damage setting


    //the speed the ai is suppose to move at (pulled from the navigate comp)
    private float _normalSpeed;

    //state flags
    private bool _isAgro;
    private bool _isAttacking;
    private bool _isAttackCoolingDown;

    // Responsibility Interfaces
    private IVision _vision;
    private IHealth _health;
    private ISfxController _sfxController;
    private IPathFinding _pathFinding;
    private IMovement _movement;
    private IRotation _rotation;
    private IDamageDisplayManager _simpleDamageDisplayManager;

    private void Start()
    {
        //get the animator
        animator = GetComponent<Animator>();

        //get interfaces
        _vision = GetComponent<IVision>();
        _health = GetComponent<IHealth>();
        _sfxController = GetComponent<ISfxController>();
        _pathFinding = GetComponent<IPathFinding>();
        _movement = GetComponent<IMovement>();
        _rotation = GetComponent<IRotation>();
        _simpleDamageDisplayManager = GetComponent<IDamageDisplayManager>();

        //initial properties
        _normalSpeed = _movement.GetSpeed();

        //initial states
        _isAgro = false;
        _isAttacking = false;
        _isAttackCoolingDown = false;
        invisible = false;

        //register event listener
        EventSystem.Current.RegisterEventListener<BulletHitCtx>(OnBulletHit);
    }

    private void Update()
    {
        //set animation states
        animator.SetBool(IsAttacking, _isAttacking);
        animator.SetBool(IsAgro, _isAgro);

        if (_isAgro)
        {
            if (!_vision.CanSeeObject()) return;
            
            // Move towards players
            var position = _vision.GetVisibleObjects()[0].transform.position;
            var path = _pathFinding.GetPath(position);
            _movement.SetPath(path);
                
            // Look at player
            _rotation.RotateTowards(position);

            if (!_isAttacking && Vector3.Distance(transform.position, position) <= attackDistance &&
                !_isAttackCoolingDown)
            {
                _isAttacking = true;
            }
        }
        else
        {
            // Wander
            _movement.SetPath(null);
            //TODO: wander around
            _isAgro = _vision.CanSeeObject();
        }
        
        if (_health.GetHealth() <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnAttackCooldownFinished()
    {
        _isAttackCoolingDown = false;
    }

    public void OnPrepAttack1()
    {
        _movement.SetSpeed(attackChargeSpeed);
        _sfxController.PlayEffect("zombieAttackPrep1");
    }

    public void OnPrepAttack2()
    {
        _sfxController.PlayEffect("zombieAttackPrep2");
    }

    public void OnAttack()
    {
        _movement.SetSpeed(0);
        _sfxController.PlayEffect("zombieAttackSmash");
        var position = transform.position;
        EventSystem.Current.FireEvent(new GroundPoundContext(new Vector3(position.x, position.y, position.z), groundPoundRadius, groundPoundDamage, groundPoundLinearFalloff));
    }

    public void OnAttackFinished()
    {
        _movement.SetSpeed(_normalSpeed);
        _isAttacking = false;
        _isAttackCoolingDown = true;
    }

    private void OnBulletHit(BulletHitCtx ctx)
    {
        if (gameObject.Equals(ctx.hit.collider.gameObject))
        {
            TakeDamage(ctx.damage);
        }
    }

    private void TakeDamage(float damage)
    {
        if (!invisible)
        {
            invisible = false;
            _health.ApplyDamage(damage);
        }
        
        _simpleDamageDisplayManager.TakeDamage();
    }
}
