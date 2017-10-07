using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KinematicPlayer : MonoBehaviour {

    public KinematicPlayerJumpAndGravity playerGravity;
    public float groundAcceleration = 10f;
    public float groundDeceleration = 100f;
    public float friction = 10f;

    public float airAcceleration = 20f;
    public float airDrag = 0.96875f;
    public float airDragHorizontalSpeedThreshold = 0.625f;
    public float airDragFallSpeedThreshold = 20f;

    public float maxHorizontalSpeed = 30f;
    public float maxVerticalSpeed = 30f;
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
    bool isGrounded = false;

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
//        Debug.DrawRay(transform.position, new Vector2(3, 0), Color.cyan);
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

            if (jumpInput && velocity.y <= 0 && isGrounded) {
                playerGravity.Jump(ref velocity);
                jumpPerformed = true;
            }
        }

        if (Mathf.Abs(velocity.y) >= maxVerticalSpeed)
            velocity.y = Mathf.Sign(velocity.y) * maxVerticalSpeed;

        if (!isGrounded && velocity.y < 0 && velocity.y > -airDragFallSpeedThreshold && Mathf.Abs(velocity.x) >= airDragHorizontalSpeedThreshold) {
            velocity.y *= airDrag;
        }
    }

    void UpdateFreeMovementX() {
        // if no input while stopped, don't accelerate
        float totalAcceleration = 0;

        float acceleration;
        float deceleration;
        if (isGrounded) {
            acceleration = groundAcceleration; 
            deceleration = groundDeceleration;
        }
        else {
            acceleration = airAcceleration;
            deceleration = airAcceleration;
        }

        // accelerating from stop or in the same direction as velocity
        if ((horizontalInput > 0 && velocity.x >= 0) || (horizontalInput < 0 && velocity.x <= 0)) {
            totalAcceleration = horizontalInput * acceleration;
        }
        // reversing direction
        else if ((horizontalInput > 0 && velocity.x < 0) || (horizontalInput < 0 && velocity.x > 0)) {
            totalAcceleration = horizontalInput * deceleration;
        }
        // slowing down from friction when releasing input while in motion
        else if (isGrounded && horizontalInput == 0 && velocity.x != 0) {
            totalAcceleration = -Mathf.Sign(velocity.x) * friction;
        }

        velocity.x += totalAcceleration * Time.deltaTime;

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

        float acceleration;
        float deceleration;
        if (isGrounded) {
            acceleration = groundAcceleration; 
            deceleration = groundDeceleration;
        }
        else {
            acceleration = airAcceleration;
            deceleration = airAcceleration;
        }

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

        velocity.y += totalAcceleration * Time.deltaTime;

        // come to a stop when slowing down from friction (don't let friction reverse X velocity direction) 
        bool yVelocitySignChange = (oldVelocity.y < 0 && velocity.y > 0) || (oldVelocity.y > 0 && velocity.y < 0);
        if (verticalInput == 0 && yVelocitySignChange) {
            velocity.y = 0;
        }

        // don't let new Y speed exceed max Y speed
        if (Mathf.Abs(velocity.y) >= maxVerticalSpeed)
            velocity.y = Mathf.Sign(velocity.y) * maxVerticalSpeed;
    }

    Vector2 CheckDirection(Vector2 direction, Color color) {
        List<RaycastHit2D> hits = CastDirection(direction, color);
        Vector2 collisionPositionOffset = CheckHits(hits, direction);
        return collisionPositionOffset;
    }

    List<RaycastHit2D> CastDirection(Vector2 direction, Color color) {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        direction.Normalize();

        int layerMask = LayerMask.GetMask("Tile");
        float raycastLength = skinWidth;
        float velocityComponent = Vector2.Dot(velocity, direction);
        Vector2 perpendicularDirection = (Vector2)Vector3.Cross(Vector3.forward, direction);
        if (velocityComponent > 0) {
            raycastLength = skinWidth + velocityComponent * Time.deltaTime;
        }

        Vector2 collisionPositionOffset = Vector2.zero;
        for (int i = 0; i < rayCount; i++) {
            
            Vector2 raycastOrigin = rb.position + (halfWidth - skinWidth) * direction;

            if (rayCount != 1) {
                Vector2 originOffset = Mathf.Lerp(halfWidth - margin, -(halfWidth - margin), (float)i / ((float)rayCount - 1)) * perpendicularDirection;
                raycastOrigin += originOffset;
            }

            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, direction, raycastLength, layerMask);
            Debug.DrawRay(raycastOrigin, direction * raycastLength, color);

            if (hit)
                hits.Add(hit);
        }

        return hits;
    }

    Vector2 CheckHits(List<RaycastHit2D> hits, Vector2 direction) {
        Vector2 collisionPositionOffset = Vector2.zero;
        float velocityComponent = Vector2.Dot(velocity, direction);

        foreach (RaycastHit2D hit in hits) {
            if (hit && velocityComponent >= 0) {
                velocity -= velocityComponent * direction;
                collisionPositionOffset = (hit.distance - skinWidth) * direction;
                return collisionPositionOffset;
            }
        }

        return collisionPositionOffset;
    }

    void CheckCollision() {
//        Vector2 collisionPositionOffset = Vector2.zero;
//        collisionPositionOffset += CheckDirection(transform.right, Color.blue);
//        collisionPositionOffset += CheckDirection(-transform.right, Color.yellow);
//        collisionPositionOffset += CheckDirection(transform.up, Color.white);
//        collisionPositionOffset += CheckDirection(-transform.up, Color.magenta);

        List<RaycastHit2D> rightHits = CastDirection(transform.right, Color.blue);
        List<RaycastHit2D> leftHits = CastDirection(-transform.right, Color.yellow);
        List<RaycastHit2D> upHits = CastDirection(transform.up, Color.white);
        List<RaycastHit2D> downHits = CastDirection(-transform.up, Color.magenta);

        Vector2 collisionPositionOffset = Vector2.zero;
//
//        bool rotatedRight = false;
//        if (rightHits.Count > 0) {
//            RaycastHit2D bottomRightHit = rightHits[rightHits.Count - 1];
//            Vector2 normal = bottomRightHit.normal;
//            Debug.DrawRay(bottomRightHit.point, normal, Color.cyan);
//            float angle = Vector2.Angle(transform.up, normal);
//            if (angle > 0) {
//                rb.MoveRotation(rb.rotation + angle);
//                rotatedRight = true;
//            }
//        }

//        bool rotatedLeft = false;
//        if (leftHits.Count > 0) {
//            RaycastHit2D bottomLeftHit = leftHits[0];
//            Vector2 normal = bottomLeftHit.normal;
//            Debug.DrawRay(bottomLeftHit.point, normal, Color.grey);
//            float angle = Vector2.Angle(transform.up, normal);
//            if (angle > 0) {
//                rb.MoveRotation(rb.rotation - angle);
//                rotatedRight = true;
//            }
//        }



//        if (!rotatedRight)
            collisionPositionOffset += CheckHits(rightHits, transform.right);
//        if (!rotatedLeft)
            collisionPositionOffset += CheckHits(leftHits, -transform.right);
        collisionPositionOffset += CheckHits(upHits, transform.up);
        collisionPositionOffset += CheckHits(downHits, -transform.up);

        if (downHits.Count > 0) {
            isGrounded = true;
        }
        else {
            isGrounded = false;
        }


        if (collisionPositionOffset.magnitude > 0) {
            Vector2 newPosition = rb.position + velocity * Time.deltaTime + collisionPositionOffset;
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
