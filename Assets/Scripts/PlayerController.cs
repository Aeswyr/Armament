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

    [Header("UI")]
    [SerializeField] private ResourceDisplayController resourceDisplay;

    [Header("Stats")]
    [SerializeField] private Fix64 forwardSpeed;
    [SerializeField] private Fix64 backSpeed;
    [SerializeField] private Fix64 dashSpeed;
    [SerializeField] private Fix64 jumpSpeed;
    [SerializeField] private Fix64 jumpHeight;
    [SerializeField] private int maxHealth = 250;
    private int health;
    private int whiteHealth;
    [SerializeField] private int regenDelay;
    [SerializeField] private int regenPerTicks;
    private int regenTimer;
    private int regenTick;
    

// 1 is facing right, -1 facing left
    private int facing = 1;
    private int dashFacing = 0;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        fCollider.onCollisionEnter += OnCollide;

        resourceDisplay.Setup(maxHealth);
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
            if (input.Forward(facing))
                movement.x = forwardSpeed * (Fix64)facing;
            else if (input.Backward(facing)) 
                movement.x = backSpeed * -(Fix64)facing;
            else 
                movement.x = Fix64.Zero;

            if (input.DashStart(facing) && dashFacing == 0) {
                dashFacing = facing;
            } else if (input.BackDash(facing) && dashFacing == 0) {

            }

            if (dashFacing != 0) {
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
                resourceDisplay.Heal(1);
            }
        }

        fBody.velocity = movement;
    }

    void OnCollide(CollisionInfo info) {
        if (input.GetButtonState(InputParser.Button.L1, InputParser.ButtonState.DOWN)) {
            regenTimer = regenDelay;
            regenTick = 0;
            int amt = 10;

            if (health > 1) {
                if (health > amt + 1) {
                    health -= amt;
                    whiteHealth += amt;
                } else {
                    whiteHealth += health - 1;
                    health = 1;
                }

                resourceDisplay.ChipDamage(10);

            } else if (whiteHealth > 0) {
                whiteHealth -= 20;
                resourceDisplay.ChipDamage(20);
            } else {
                health = 0;
                resourceDisplay.Damage(1);
            }
            
        }
            
        if (input.GetButtonState(InputParser.Button.H1, InputParser.ButtonState.DOWN)) {
            health -= 40;
            whiteHealth = 0;
            resourceDisplay.Damage(40);
        }
    }
}
