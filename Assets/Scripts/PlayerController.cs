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
            if (!exhausted) {
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

    [SerializeField] private int maxExhaust;
    private int m_exhaust;
    private int exhaust {
        get {return m_exhaust;}
        set {
            resourceDisplay.SetExhaust(value, exhausted);
            m_exhaust = value;
        }
    }
    private bool exhausted;
    [SerializeField] private int exhaustPerTicks;
    private int exhaustTick;
    

// 1 is facing right, -1 facing left
    private int facing = 1;
    private int dashFacing = 0;

    // Start is called before the first frame update
    void Start()
    {
        resourceDisplay.Setup(maxHealth, maxMeter, maxExhaust);

        health = maxHealth;
        exhaust = maxExhaust;
        meter = 0;

        fCollider.onCollisionEnter += OnCollide;

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        facing = Fix64.Sign(GameManager.Instance.GetOtherPlayer(this).fTransform.position.x - fTransform.position.x);
        if (facing == 0)
            facing = 1;

        Vec2Fix movement = fBody.velocity;
        bool grounded = fTransform.position.y == Fix64.Zero;
        
        
        if (grounded) {
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

            if (!input.Dash() && !input.Forward(dashFacing))
                dashFacing = 0;
            
            if (input.Crouch()) {
                movement.x = Fix64.Zero;
                dashFacing = 0;
            } else if (input.Jump()) {
                movement.y = jumpHeight;
                movement.x = jumpSpeed * (Fix64)input.Direction();
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

        if (exhausted) {
            if (exhaustTick == 0) {
                exhaustTick = exhaustPerTicks;
                exhaust++;
                if (exhaust == maxExhaust) {
                    exhausted = false;
                }
            } else {
                exhaustTick--;
            }
        }

        fBody.velocity = movement;
    }

    void OnCollide(CollisionInfo info) {
        
        Debug.Log("colliding!");

        if (input.GetButtonState(InputParser.Button.L1, InputParser.ButtonState.PRESSED)) {
            regenTimer = regenDelay;
            regenTick = 0;
            int amt = 10;

            if (!exhausted) {
                exhaust -= amt / 2;
                if (exhaust < 0) {
                    exhaust = 0;
                    exhausted = true;
                    exhaustTick = exhaustPerTicks;
                    whiteHealth = 0;
                }
            }
            

            if (health > 1) {
                int realAmt = amt;
                if (health > amt + 1) {
                    health -= amt;
                    whiteHealth += amt;
                } else {
                    realAmt = health - 1;
                    whiteHealth += realAmt;
                    health = 1;
                }
            } else if (whiteHealth > 0) {
                whiteHealth -= amt * 2;
            } else {
                health = 0;
                whiteHealth = 0;
            }
            
        }
            
        if (input.GetButtonState(InputParser.Button.H1, InputParser.ButtonState.PRESSED)) {
            health -= 40;
            whiteHealth = 0;
            
        }
    }
}
