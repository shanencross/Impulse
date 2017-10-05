using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KinematicPlayer : MonoBehaviour {

    public KinematicPlayerJumpAndGravity playerGravity;
    public float acceleration = 5f;
    public float deceleration = 50f;
    public float friction = 5f;
    public float maxHorizontalSpeed = 15f;
    public float maxVerticalSpeed = 15f;
    public float checkGroundRayDistance = 0.2f;

    public bool colliding = false;

    public float halfWidth = 0.5f;
    public float skinWidth = 0.5f;
    public float margin = 0.01f;
    public int rayCount = 3;

//    public Transform groundCheck;

    [SerializeField]
    float horizontalInput = 0f;
    [SerializeField]
    float verticalInput = 0f;
    [SerializeField]
    bool jumpInput = false;
    [SerializeField]
    bool jumpPerformed = false;

    [SerializeField]
    Vector2 oldVelocity;
    [SerializeField]
    Vector2 velocity;

    Rigidbody2D rb;
    CircleCollider2D circleCollider;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();

        if (rb.bodyType != RigidbodyType2D.Kinematic)
            rb.bodyType = RigidbodyType2D.Kinematic;

        circleCollider = GetComponentInChildren<CircleCollider2D>();

        velocity = rb.velocity;
        oldVelocity = velocity;

        playerGravity = GetComponent<KinematicPlayerJumpAndGravity>();
    }

    void Update() {
        UpdateInput();
    }

    void FixedUpdate() {
        // update velocity variable in case rigidbody velocity has been modified
        velocity = rb.velocity;

        UpdateVelocity();
        CheckCollision();

        // set rigidbody velocity to enw velocity
        rb.velocity = velocity;
        oldVelocity = velocity;
    }

    void UpdateVelocity() {
        UpdateVelocityX();
        UpdateVelocityY();
    }

    void UpdateVelocityX() {
        UpdateFreeMovementX();
    }
        
    void UpdateVelocityY() {
//        UpdateFreeMovementY();

        if (playerGravity) {
            playerGravity.ApplyGravity(ref velocity);

            if (jumpInput && velocity.y <= 0) {
                playerGravity.Jump(ref velocity);
                jumpPerformed = true;
            }
        }
    }

    void UpdateFreeMovementX() {
        // if no input while stopped, don't accelerate
        float totalAcceleration = 0;

        // accelerating from stop or in the same direction as velocity
        if ((horizontalInput > 0 && velocity.x >= 0) || (horizontalInput < 0 && velocity.x <= 0)) {
            totalAcceleration = horizontalInput * acceleration;
        }
        // reversing direction
        else if ((horizontalInput > 0 && velocity.x < 0) || (horizontalInput < 0 && velocity.x > 0)) {
            totalAcceleration = horizontalInput * deceleration;
        }
        // slowing down from friction when releasing input while in motion
        else if (horizontalInput == 0 && velocity.x != 0) {
            totalAcceleration = -Mathf.Sign(velocity.x) * friction;
        }

        velocity.x += totalAcceleration * Time.fixedDeltaTime;

        // come to a stop when slowing down from friction (don't let friction reverse X velocity direction) 
        bool xVelocitySignChange = (oldVelocity.x < 0 && velocity.x > 0) || (oldVelocity.x > 0 && velocity.x < 0);
        if (horizontalInput == 0 && xVelocitySignChange) {
            velocity.x = 0;
        }

        // don't let new X speed exceed max X speed
        if (Mathf.Abs(velocity.x) >= maxHorizontalSpeed)
            velocity.x = Mathf.Sign(velocity.x) * maxHorizontalSpeed;
    }

    // Temp for testing; Free Y Movement
    void UpdateFreeMovementY() {
        // if no input while stopped, don't accelerate
        float totalAcceleration = 0;

        // accelerating from stop or in the same direction as velocity
        if ((verticalInput > 0 && velocity.y >= 0) || (verticalInput < 0 && velocity.y <= 0)) {
            totalAcceleration = verticalInput * acceleration;
        }
        // reversing direction
        else if ((verticalInput > 0 && velocity.y < 0) || (verticalInput < 0 && velocity.y > 0)) {
            totalAcceleration = verticalInput * deceleration;
        }
        // slowing down from friction when releasing input while in motion
        else if (verticalInput == 0 && velocity.y != 0) {
            totalAcceleration = -Mathf.Sign(velocity.y) * friction;
        }

        velocity.y += totalAcceleration * Time.fixedDeltaTime;

        // come to a stop when slowing down from friction (don't let friction reverse X velocity direction) 
        bool yVelocitySignChange = (oldVelocity.y < 0 && velocity.y > 0) || (oldVelocity.y > 0 && velocity.y < 0);
        if (verticalInput == 0 && yVelocitySignChange) {
            velocity.y = 0;
        }

        // don't let new X speed exceed max X speed
        if (Mathf.Abs(velocity.y) >= maxVerticalSpeed)
            velocity.y = Mathf.Sign(velocity.y) * maxVerticalSpeed;
    }

    Vector2 CheckDirection(Vector2 direction, Color color) {
        direction.Normalize();

        Vector2 collisionPositionOffset = Vector2.zero;
        for (int i = 0; i < rayCount; i++) {
            Vector2 raycastOrigin = rb.position + (halfWidth - skinWidth) * direction;

            Vector2 perpendicularDirection = (Vector2)Vector3.Cross(Vector3.forward, direction);

            if (rayCount != 1) {
                Vector2 originOffset = Mathf.Lerp(halfWidth - margin, -(halfWidth - margin), (float)i / ((float)rayCount - 1)) * perpendicularDirection;
                raycastOrigin += originOffset;
            }

            float raycastLength = skinWidth - margin;
            float velocityComponent = Vector2.Dot(velocity, direction);

            if (velocityComponent > 0) {
                raycastLength = skinWidth + velocityComponent * Time.deltaTime;
            }

            int layerMask = LayerMask.GetMask("Tile");

            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, direction, raycastLength, layerMask);
            Debug.DrawRay(raycastOrigin, direction * raycastLength, color);


            if (hit) {
                velocity -= velocityComponent * direction;
                collisionPositionOffset = (hit.distance - skinWidth) * direction;
            }
        }

        return collisionPositionOffset;

    }

    void CheckCollision() {
        Vector2 collisionPositionOffset = Vector2.zero;
        collisionPositionOffset += CheckDirection(transform.right, Color.blue);
        collisionPositionOffset += CheckDirection(-transform.right, Color.yellow);
        collisionPositionOffset += CheckDirection(transform.up, Color.white);
        collisionPositionOffset += CheckDirection(-transform.up, Color.magenta);

        if (collisionPositionOffset.magnitude > 0) {
            Vector2 newPosition = rb.position + velocity * Time.fixedDeltaTime + collisionPositionOffset;
            rb.MovePosition(newPosition);
        }
    }

    void CheckGround() {
        Vector2 checkGroundRayOrigin = rb.position;

    }

    void UpdateInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetButton("Jump");

        if (jumpPerformed && jumpInput) {
            jumpInput = false;
        }
        else if (jumpPerformed && !jumpInput) {
            jumpPerformed = false;
        }
        
    }
}
