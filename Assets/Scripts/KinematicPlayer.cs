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

    public bool colliding = false;

    public float halfWidth = 0.5f;
    public float skinWidth = 0.01f;

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

            if (jumpInput) {
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

    void CheckCollision() {
        Vector2 raycastOriginRight = rb.position + (halfWidth - skinWidth) * (Vector2)transform.right;
        Vector2 raycastOriginUp = rb.position + (halfWidth - skinWidth) * (Vector2)transform.up;
        Vector2 raycastOriginLeft = rb.position + (halfWidth - skinWidth) * -(Vector2)transform.right;
        Vector2 raycastOriginDown = rb.position + (halfWidth - skinWidth) * -(Vector2)transform.up;

        float raycastLengthRight = 0;
        if (velocity.x > 0) {
            raycastLengthRight = skinWidth + velocity.x * Time.deltaTime;
        }

        float raycastLengthUp = 0;
        if (velocity.y > 0) {
            raycastLengthUp = skinWidth + velocity.y * Time.deltaTime;
        }

        float raycastLengthLeft = 0;
        if (velocity.x < 0) {
            raycastLengthLeft = skinWidth + Mathf.Abs(velocity.x) * Time.deltaTime;
        }

        float raycastLengthDown = 0;
        if (velocity.y < 0) {
            raycastLengthDown = skinWidth + Mathf.Abs(velocity.y) * Time.deltaTime;
            Debug.Log("raycastLengthDown: " + raycastLengthDown);
        }

        int layerMask = LayerMask.GetMask("Tile");

        RaycastHit2D hitRight = Physics2D.Raycast(raycastOriginRight, transform.right, raycastLengthRight, layerMask);
        Debug.DrawRay(raycastOriginRight, transform.right * raycastLengthRight, Color.cyan);

        RaycastHit2D hitUp = Physics2D.Raycast(raycastOriginUp, transform.up, raycastLengthUp, layerMask);
        Debug.DrawRay(raycastOriginUp, transform.up * raycastLengthUp, Color.cyan);

        RaycastHit2D hitLeft = Physics2D.Raycast(raycastOriginLeft, -transform.right, raycastLengthLeft, layerMask);
        Debug.DrawRay(raycastOriginLeft, -transform.right * raycastLengthLeft, Color.cyan);

        RaycastHit2D hitDown = Physics2D.Raycast(raycastOriginDown, -transform.up, raycastLengthDown, layerMask);
        Debug.DrawRay(raycastOriginDown, -transform.up * raycastLengthDown, Color.cyan);

        // only right working correctly for sure
        // left is glitched?

        if (hitRight) {
            Debug.Log("Hit on right side: " + hitRight.distance);
            velocity.x = 0;

            Vector2 newPosition = rb.position + velocity*Time.fixedDeltaTime + (hitRight.distance - skinWidth) * (Vector2)transform.right;

            rb.MovePosition(newPosition);
        }

        if (hitUp) {
            Debug.Log("Hit on up side: " + hitUp.distance);
            velocity.y = 0;
            Vector2 newPosition = rb.position + velocity*Time.fixedDeltaTime + (hitUp.distance - skinWidth) * (Vector2)transform.up;

            rb.MovePosition(newPosition);
        }

        if (hitLeft) {
            Debug.Log("Hit on left side: " + hitLeft.distance);
            velocity.x = 0;
            Vector2 newPosition = rb.position + velocity*Time.fixedDeltaTime + (hitLeft.distance - skinWidth) * -(Vector2)transform.right;

            rb.MovePosition(newPosition);
        }
        if (hitDown) {
            Debug.Log("Hit on down side: " + hitDown.distance);
            velocity.y = 0;
            Vector2 newPosition = rb.position + velocity*Time.fixedDeltaTime + (hitDown.distance - skinWidth) * -(Vector2)transform.up;

            rb.MovePosition(newPosition);
        }

//        bool hit = (hitRight || hitUp || hitLeft || hitDown);
//        if (hit) {
//            Vector2 newPosition = rb.position + velocity * Time.fixedDeltaTime;
//            if (hitRight) {
//                Debug.Log("Hit on right side: " + hitRight.distance);
//                velocity.x = 0;
//                newPosition += (hitRight.distance - skinWidth) * (Vector2)transform.right;
//            }
//
//            if (hitUp) {
//                Debug.Log("Hit on up side: " + hitUp.distance);
//                velocity.y = 0;
//                newPosition += (hitUp.distance - skinWidth) * (Vector2)transform.up;
//            }
//
//            if (hitLeft) {
//                Debug.Log("Hit on left side: " + hitLeft.distance);
//                velocity.x = 0;
//                newPosition += (hitLeft.distance - skinWidth) * -(Vector2)transform.right;
//            }
//            if (hitDown) {
//                Debug.Log("Hit on down side: " + hitDown.distance);
//                velocity.y = 0;
//                newPosition += (hitDown.distance - skinWidth) * -(Vector2)transform.up;
//            }
//            rb.MovePosition(newPosition);
//        }
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
