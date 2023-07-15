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
    [SerializeField] private int maxHealth;

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
            resourceDisplay.SetWhiteHealth(value + health);
            m_whiteHealth = value;
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
            if (m_meter > maxMeter)
                m_meter = maxMeter;
            resourceDisplay.SetMeter(m_meter);
        }
    }

    Fix64 proration = Fix64.One;

// 1 is facing right, -1 facing left
    private int facing = 1;
    private int dashFacing = 0;

    // Start is called before the first frame update
    void Start()
    {
        resourceDisplay.Setup(maxHealth, maxMeter);

        health = maxHealth;
        meter = 0;

        hurtBox.onCollisionEnter += OnHit;

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!playerActions.IsInCombo())
            proration = Fix64.One;

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
                meter += 2;
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
        } else if (grounded) {
            playerActions.UpdateMomentum(ref movement);
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

        MoveProperty firedMove = null;

        if (input.GetButtonState(InputParser.Button.H1, InputParser.ButtonState.PRESSED)) {
            var move = playerActions.FireMove(InputParser.Action.H1, input.GetMotions(facing), !grounded);
            if (move != null)
                firedMove = move;
        }
        if (input.GetButtonState(InputParser.Button.L1, InputParser.ButtonState.PRESSED)) {
            var move = playerActions.FireMove(InputParser.Action.L1, input.GetMotions(facing), !grounded);
            if (move != null)
                firedMove = move;
        }
        if (input.GetButtonState(InputParser.Button.H2, InputParser.ButtonState.PRESSED)) {
            var move = playerActions.FireMove(InputParser.Action.H2, input.GetMotions(facing), !grounded);
            if (move != null)
                firedMove = move;
        }
        if (input.GetButtonState(InputParser.Button.L2, InputParser.ButtonState.PRESSED)) {
            var move = playerActions.FireMove(InputParser.Action.L2, input.GetMotions(facing), !grounded);
            if (move != null)
                firedMove = move;
        }
        if (input.GetButtonState(InputParser.Button.R, InputParser.ButtonState.PRESSED)) {
            var move = playerActions.FireMove(InputParser.Action.R, input.GetMotions(facing), !grounded);
            if (move != null)
                firedMove = move;
        }

        if (firedMove != null && firedMove.MoveType == MoveType.SPECIAL)
            meter += 20;

        fBody.velocity = movement;
    }
    void OnHit(CollisionInfo info) {

        var hitInfo = info.secondary.gameObject.GetComponent<HitboxInfo>();
        int damage = hitInfo.Properties.HitData.Damage;

        damage = (int)((Fix64)damage * proration);

        //trigger cancel window for attacker
        hitInfo.Owner.SetCancellable(hitInfo.Properties);
        
        if (IsBlocking(hitInfo.Properties.HitData.BlockProperty)) { //handle blocking situation
            regenTimer = regenDelay;
            regenTick = 0;

            hitInfo.Owner.transform.GetComponent<PlayerController>().meter += 10;
            meter += 20;

            if (health > 1) { // convert chip to white health
                Fix64 chip = (Fix64)0.15f;
                if ((int)hitInfo.Properties.MoveType >= (int)MoveType.SPECIAL)
                    chip = (Fix64)0.3f;
                damage = (int)Fix64.Max(Fix64.One, (Fix64)damage * chip);
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

            playerActions.SetBlockstun(10);
            
        } else { //handle unblocked situation
            hitInfo.Owner.transform.GetComponent<PlayerController>().meter += damage;

            health -= Mathf.Max(hitInfo.Properties.HitData.MinimumDamage, damage);
            whiteHealth = 0;

            if (!playerActions.IsInCombo())
                proration = hitInfo.Properties.HitData.Proration;
            else if (proration > hitInfo.Properties.HitData.ForcedProration)
                proration = hitInfo.Properties.HitData.ForcedProration;

            playerActions.SetHitstun(10);
            
        }
    }

    public bool IsBlocking(BlockPropertyType blockProperty) {
        return true;

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
}
