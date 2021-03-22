using UnityEngine;
using CallbackEvents;

public class ShootAI : MonoBehaviour
{
    // Animation Tags
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsAgro = Animator.StringToHash("isAgro");
    
    //components
    public Animator animator;

    //navigation settings
    public int lookForLostPlayerMs; //the amount of time the bot should look for a lost player

    //attack settings
    public float minStrafeDistance;
    public float maxStrafeDistance;
    public float projectileDamage;
    public float projectileSpeed;
    public bool invisible; //if the player is invisible this frame
    public GameObject projectilePrefab;

    //Navigation Properties
    private float _lastSawPlayerTime;
    private Vector3 _lastSeenPlayerPos;

    //state flags
    private bool _isAgro;
    private bool _isStrafing;
    private bool _movingCloser;
    private bool _movingAway;
    private bool _hasLastPlayerPos;
    private bool _isAttacking;
    private bool _isAttackCoolingDown;

    // Responsibility Interfaces
    private IVision _vision;
    private IHealth _health;
    private ISfxController _sfxController;
    private IPathFinding _pathFinding;
    private IMovement _movement;
    private IRotation _rotation;
    private IDamageDisplayManager _damageDisplayManager;
    
    private void Start()
    {
        //get interfaces
        _vision = GetComponent<IVision>();
        _health = GetComponent<IHealth>();
        _sfxController = GetComponent<ISfxController>();
        _pathFinding = GetComponent<IPathFinding>();
        _movement = GetComponent<IMovement>();
        _rotation = GetComponent<IRotation>();
        _damageDisplayManager = GetComponent<IDamageDisplayManager>();

        //get the animator
        if (animator == null)
            animator = GetComponent<Animator>();

        //initial properties
        _lastSeenPlayerPos = new Vector3();

        //initial
        _isAgro = false;
        _hasLastPlayerPos = false;
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
            if (!_isStrafing && !_movingCloser && !_movingAway)
            {
                _movingCloser = true;
            }

            if (_vision.CanSeeObject())
            {
                if (_isStrafing)
                {
                    var path = _pathFinding.GetPath(transform.position);
                    _movement.SetPath(path);

                    if (!_isAttacking && !_isAttackCoolingDown)
                    {
                        _isAttacking = true;
                    }
                    
                    var distance = Vector3.Distance(transform.position, _vision.GetVisibleObjects()[0].transform.position);

                    if (distance < minStrafeDistance)
                    {
                        _movingCloser = false;
                        _movingAway = true;
                        _isStrafing = false;
                    }
                    else if (distance > maxStrafeDistance)
                    {
                        _movingCloser = true;
                        _movingAway = false;
                        _isStrafing = false;
                    }
                    else if (Mathf.Abs(maxStrafeDistance - distance) < 0.3)
                    {
                        _movingCloser = false;
                        _movingAway = false;
                        _isStrafing = true;
                    }
                    else if (distance > minStrafeDistance && distance < maxStrafeDistance)
                    {
                        float ran = Random.Range(0, 1);
                        if (ran < 0.6)
                        {
                            _movingCloser = false;
                            _movingAway = false;
                            _isStrafing = true;
                        }
                    }

                    _rotation.RotateTowards(_vision.GetVisibleObjects()[0].transform.position);
                }
                else if (_movingCloser)
                {
                    _lastSeenPlayerPos = _vision.GetVisibleObjects()[0].transform.position;
                    _lastSawPlayerTime = Time.time;
                    _hasLastPlayerPos = true;
                    var position = transform.position;
                    var path = _pathFinding.GetPath(position);
                    _movement.SetPath(path);

                    var distance = Vector3.Distance(position, _vision.GetVisibleObjects()[0].transform.transform.position);

                    if (distance < minStrafeDistance)
                    {
                        _movingCloser = false;
                        _movingAway = true;
                        _isStrafing = false;
                    }
                    else if (distance > maxStrafeDistance)
                    {
                        _movingCloser = true;
                        _movingAway = false;
                        _isStrafing = false;
                    }
                    else if (Mathf.Abs(maxStrafeDistance - distance) < 0.3)
                    {
                        _movingCloser = false;
                        _movingAway = false;
                        _isStrafing = true;
                    }
                    else if (distance > minStrafeDistance && distance < maxStrafeDistance)
                    {
                        float ran = Random.Range(0, 1);
                        if (ran < 0.6)
                        {
                            _movingCloser = false;
                            _movingAway = false;
                            _isStrafing = true;
                        }
                    }

                    var lookPos = _vision.GetVisibleObjects()[0].transform.position - transform.position;
                    lookPos.y = 0;
                    var rotation = Quaternion.LookRotation(lookPos);
                    
                    _rotation.RotateTowards(_vision.GetVisibleObjects()[0].transform.position);
                }
                else if (_movingAway)
                {
                    if (!_isAttacking && !_isAttackCoolingDown)
                    {
                        _isAttacking = true;
                    }
                    
                    _lastSeenPlayerPos = _vision.GetVisibleObjects()[0].transform.position;
                    _lastSawPlayerTime = Time.time;
                    _hasLastPlayerPos = true;
                    
                    
                    var backwardTarget = transform.position - transform.forward;
                    var path = _pathFinding.GetPath(backwardTarget);
                    _movement.SetPath(path);
                    _rotation.RotateTowards(_vision.GetVisibleObjects()[0].transform.position);

                    var distance = Vector3.Distance(transform.position, _vision.GetVisibleObjects()[0].transform.transform.position);

                    if (distance < minStrafeDistance)
                    {
                        _movingCloser = false;
                        _movingAway = true;
                        _isStrafing = false;
                    }
                    else if (distance > maxStrafeDistance)
                    {
                        _movingCloser = true;
                        _movingAway = false;
                        _isStrafing = false;
                    }
                    else if (Mathf.Abs(maxStrafeDistance - distance) < 0.3)
                    {
                        _movingCloser = false;
                        _movingAway = false;
                        _isStrafing = true;
                    }
                    else if (distance > minStrafeDistance && distance < maxStrafeDistance)
                    {
                        float ran = Random.Range(0, 1);
                        if (ran < 0.6)
                        {
                            _movingCloser = false;
                            _movingAway = false;
                            _isStrafing = true;
                        }
                    }
                }
            }
            else
            {
                var path = _pathFinding.GetPath(_hasLastPlayerPos ? _lastSeenPlayerPos : transform.position);
                _movement.SetPath(path);

                if (!(Time.time - _lastSawPlayerTime >= lookForLostPlayerMs)) return;
                _isAgro = false;
                _hasLastPlayerPos = false;
            }
        }
        else
        {
            var path = _pathFinding.GetPath(transform.position);
            _movement.SetPath(path);
            
            //TODO: wander around
            _hasLastPlayerPos = false;
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

    public void OnPrepAttack()
    {
        _sfxController.PlayEffect("attackPrep1Sound");
    }

    public void OnAttack()
    {
        _sfxController.PlayEffect("attackSound");

        var ob = Instantiate(projectilePrefab);
        var aiTransform = transform;
        var aiPosition = aiTransform.position;
        var spawnOffset = (aiTransform.up * 1.35f);

        Vector3 targetPos;

        if (_vision.CanSeeObject())
        {
            targetPos = _vision.GetVisibleObjects()[0].transform.transform.position;
        }
        else if (_hasLastPlayerPos)
        {
            targetPos = _lastSeenPlayerPos;
        }
        else
        {
            targetPos = (aiPosition + spawnOffset) + aiTransform.forward;
        }

        var forward = (targetPos - (aiPosition + spawnOffset)).normalized;
        ob.transform.position = aiPosition + forward + spawnOffset;
        
        var projectile = ob.GetComponent<Projectile>();
        projectile.velocity = forward * projectileSpeed;
        projectile.damage = projectileDamage;
        
        _isAttackCoolingDown = true;
        _isAttacking = false;
    }

    public void OnAttackFinished()
    {
        _isAttacking = false;
    }

    private void OnBulletHit(BulletHitCtx ctx)
    {
        if (gameObject.Equals(ctx.hit.collider.gameObject))
        {
            TakeDamage(ctx.damage);
        }
    }

    /*
    .##.....##.########.##.......########..########.########...######.
    .##.....##.##.......##.......##.....##.##.......##.....##.##....##
    .##.....##.##.......##.......##.....##.##.......##.....##.##......
    .#########.######...##.......########..######...########...######.
    .##.....##.##.......##.......##........##.......##...##.........##
    .##.....##.##.......##.......##........##.......##....##..##....##
    .##.....##.########.########.##........########.##.....##..######.
    */

    private void TakeDamage(float damage)
    {
        if (invisible) return;
        _health.ApplyDamage(damage);
        _damageDisplayManager.TakeDamage();
    }
}
