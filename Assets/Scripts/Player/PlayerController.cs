using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixMath.NET;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputParser input;
    [SerializeField] private FixedTransform fTransform;
    [SerializeField] private FixedBody fBody;
    [SerializeField] private FixedCollider fCollider;
    [SerializeField] private FixedCollider hurtBox;
    [SerializeField] private ActionController playerActions;

    [Header("UI")]
    [SerializeField] private ResourceDisplayController resourceDisplay;

    [Header("Stats")]
    [SerializeField] private Fix64 forwardSpeed;
    [SerializeField] private Fix64 backSpeed;
    [SerializeField] private Fix64 dashSpeed;
    [SerializeField] private Fix64 jumpSpeed;
    [SerializeField] private Fix64 jumpHeight;
    [SerializeField] private int maxHealth = 250;

    private int m_health;
    private int health {
        get {return m_health;}
        set {
            resourceDisplay.SetHealth(value);
            m_health = value;
        }
    }
    private int m_whiteHealth;
    private int whiteHealth {
        get {return m_whiteHealth;}
        set {
            if (momentumState != -1) {
                resourceDisplay.SetWhiteHealth(value + health);
                m_whiteHealth = value;
                return;
            }
            resourceDisplay.SetWhiteHealth(0);
            m_whiteHealth = 0;
            
        }
    }
    [SerializeField] private int regenDelay;
    [SerializeField] private int regenPerTicks;
    private int regenTimer;
    private int regenTick;

    [SerializeField] private int maxMeter;
    private int m_meter;
    private int meter {
        get {return m_meter;}
        set {
            m_meter = value;
            if (m_meter >= maxMeter) {
                m_meter -= maxMeter;
                meterStocks++;
                resourceDisplay.SetMeterStocks(meterStocks);
            }
            resourceDisplay.SetMeter(m_meter);
        }
    }
    private int meterStocks;

    [SerializeField] private int maxMomentum;
    private int m_momentum;
    private int momentum {
        get {return m_momentum;}
        set {
            resourceDisplay.SetExhaust(value, momentumState == -1);
            m_momentum = value;
        }
    }
    private int momentumState = 0; // -1 lost the initiative, +1 gained the initiative, 0 momentum neutral 
    [SerializeField] private int momentumPerTicks;
    private int momentumTick;
    

// 1 is facing right, -1 facing left
    private int facing = 1;
    private int dashFacing = 0;

    // Start is called before the first frame update
    void Start()
    {
        resourceDisplay.Setup(maxHealth, maxMeter, maxMomentum);

        health = maxHealth;
        momentum = maxMomentum / 2;
        meter = 0;

        hurtBox.onCollisionEnter += OnHit;

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        facing = Fix64.Sign(GameManager.Instance.GetOtherPlayer(this).fTransform.position.x - fTransform.position.x);
        if (facing == 0)
            facing = 1;
            
        input.CheckCharge(facing);

        Vec2Fix movement = fBody.velocity;
        bool grounded = fTransform.position.y == Fix64.Zero;
        
        
        if (playerActions.Actionable() && grounded) {
            if (input.Forward(facing)) {
                movement.x = forwardSpeed * (Fix64)facing;
            } else if (input.Backward(facing)) 
                movement.x = backSpeed * -(Fix64)facing;
            else 
                movement.x = Fix64.Zero;

            if (input.DashStart(facing) && dashFacing == 0) {
                dashFacing = facing;
            } else if (input.BackDash(facing) && dashFacing == 0) {

            }

            if (dashFacing != 0) {
                meter += 1;
                movement.x = dashSpeed * (Fix64)dashFacing;
            }

            if (!input.DashMacro() && !input.Forward(dashFacing)) {
                dashFacing = 0;
            }
            
            if (input.Crouch()) {
                movement.x = Fix64.Zero;
                dashFacing = 0;
            } else if (input.Jump()) {
                movement.y = jumpHeight;
                movement.x = jumpSpeed * (Fix64)input.Direction();
                if (dashFacing != 0)
                    movement.x += jumpSpeed * Fix64.Half * (Fix64)dashFacing;
                dashFacing = 0;
            }
        }
        
        if (regenTimer > 0) {
            regenTimer--;
        } else if( whiteHealth > 0) {
            if (regenTick > 0) {
                regenTick--;
            } else {
                regenTick = regenPerTicks;
                whiteHealth--;
                health++;
            }
        }

        if (momentumState == -1) {
            if (momentumTick == 0) {
                momentumTick = momentumPerTicks;
                momentum++;
                if (momentum == maxMomentum) {
                    GameManager.Instance.ResetMomentum(this);
                }
            } else {
                momentumTick--;
            }
        }

        if (input.GetButtonState(InputParser.Button.H1, InputParser.ButtonState.PRESSED)) {
            playerActions.FireMove(InputParser.Action.H1, input.GetMotions(facing), !grounded);
        }
        if (input.GetButtonState(InputParser.Button.L1, InputParser.ButtonState.PRESSED)) {
            playerActions.FireMove(InputParser.Action.L1, input.GetMotions(facing), !grounded);
        }
        if (input.GetButtonState(InputParser.Button.H2, InputParser.ButtonState.PRESSED)) {
            playerActions.FireMove(InputParser.Action.H2, input.GetMotions(facing), !grounded);
        }
        if (input.GetButtonState(InputParser.Button.L2, InputParser.ButtonState.PRESSED)) {
            playerActions.FireMove(InputParser.Action.L2, input.GetMotions(facing), !grounded);
        }
        if (input.GetButtonState(InputParser.Button.R, InputParser.ButtonState.PRESSED)) {
            playerActions.FireMove(InputParser.Action.R, input.GetMotions(facing), !grounded);
        }

        fBody.velocity = movement;
    }
    void OnHit(CollisionInfo info) {

        var hitInfo = info.secondary.gameObject.GetComponent<HitboxInfo>();
        int damage = hitInfo.Properties.Damage;
        int momentumDamage = hitInfo.Properties.MomentumDamage;

        //trigger cancel window for attacker
        hitInfo.Owner.SetCancellable(hitInfo.Properties);

        
        if (IsBlocking(hitInfo.Properties.BlockProperty)) { //handle blocking situation
            regenTimer = regenDelay;
            regenTick = 0;

            GameManager.Instance.ChangeMomentumBar(-momentumDamage, this);
            if (hitInfo.Properties.MoveLevel >= CancelType.SPECIAL || momentumState == -1) {
                if (health > 1) { // convert chip to white health
                    damage = (int)Fix64.Max(Fix64.One, (Fix64)damage * (Fix64)0.25f);
                    int realAmt = damage;
                    if (health > damage + 1) {
                        health -= damage;
                        whiteHealth += damage;
                    } else {
                        realAmt = health - 1;
                        whiteHealth += realAmt;
                        health = 1;
                    }
                } else if (whiteHealth > 0) {
                    whiteHealth -= damage * 2;
                } else {
                    health = 0;
                    whiteHealth = 0;
                }
            }
            
        } else { //handle unblocked situation
            health -= damage;
            whiteHealth = 0;
            
        }
    }

    public bool IsBlocking(BlockPropertyType blockProperty) {
        if (blockProperty == BlockPropertyType.THROW)
            return false;
        
        if (!input.Backward(facing) || !playerActions.Actionable())
            return false;

        if (blockProperty == BlockPropertyType.LOW && !input.Crouch())
            return false;

        if (blockProperty == BlockPropertyType.HIGH && input.Crouch())
            return false;

        return true;
    }

    public void AdjustMomentumBar(int amt) {
        if (momentumState == 1
            || (momentumState == -1 && amt < 0))
            amt = 0;
        
        momentum += amt;
        if (momentumState == 0 ) {
            if (momentum < 0) {
                momentum = 0;
                momentumState = -1;
                momentumTick = momentumPerTicks;
                whiteHealth = 0;
            } else if (momentum >= maxMomentum) {
                momentum = maxMomentum;
                momentumState = 1;
            }
        }
    }

    public void ResetMomentum() {
        momentum = maxMomentum / 2;
        momentumState = 0;
    }
}
