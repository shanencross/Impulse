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
    public float airDragFallSpeedThreshold = 7.5f;

    public float maxHorizontalSpeed = 30f;
    public float maxVerticalSpeed = 30f;
    public float checkGroundRayDistance = 0.2f;

    public bool colliding = false;
    public float angle = 0;

    public float halfWidth = 0.5f;
    public float skinWidth = 0.5f;
    public float margin = 0.01f;
    public int rayNum = 3;

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
    Vector2 oldLocalVelocity;
    [SerializeField]
    Vector2 localVelocity;
    [SerializeField]
    Vector2 worldVelocity;

    Rigidbody2D rb;
    CircleCollider2D circleCollider;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();

        if (rb.bodyType != RigidbodyType2D.Kinematic)
            rb.bodyType = RigidbodyType2D.Kinematic;

        circleCollider = GetComponentInChildren<CircleCollider2D>();

        worldVelocity = rb.velocity;
        oldLocalVelocity = localVelocity;
        localVelocity = transform.InverseTransformDirection(worldVelocity);

        playerGravity = GetComponent<KinematicPlayerJumpAndGravity>();
    }

    void Update() {
        UpdateInput();
//        Debug.DrawRay(transform.position, new Vector2(3, 0), Color.cyan);
    }

    void FixedUpdate() {
         // update velocity variable in case rigidbody velocity has been modified

        worldVelocity = rb.velocity;

        UpdateVelocity();
        worldVelocity = transform.TransformVector(localVelocity);
        Vector2 collisionPositionOffset = CheckCollision();
        worldVelocity = transform.TransformVector(localVelocity);

        if (collisionPositionOffset.magnitude > 0) {
            Vector2 newPosition = rb.position + worldVelocity * Time.deltaTime + collisionPositionOffset;
            rb.MovePosition(newPosition);
        }

        // set rigidbody velocity to new velocity
        rb.velocity = worldVelocity;
        oldLocalVelocity = localVelocity;
    }

    void UpdateVelocity() {

        UpdateVelocityX();
        UpdateVelocityY();


    }

    void UpdateVelocityX() {
        UpdateFreeMovementX();
    }
        
    void UpdateVelocityY() {
        if (playerGravity) {
            playerGravity.ApplyGravity(ref localVelocity);

            if (jumpInput && isGrounded) {
                playerGravity.Jump(ref localVelocity);
                jumpPerformed = true;
            }
        }

        if (Mathf.Abs(localVelocity.y) >= maxVerticalSpeed)
            localVelocity.y = Mathf.Sign(localVelocity.y) * maxVerticalSpeed;

//        if (!isGrounded && velocity.y < 0 && velocity.y > -airDragFallSpeedThreshold && Mathf.Abs(velocity.x) >= airDragHorizontalSpeedThreshold) {
//            velocity.y *= airDrag;
//        }
    }

    void UpdateFreeMovementX() {
        localVelocity = transform.InverseTransformVector(worldVelocity);

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
        if ((horizontalInput > 0 && localVelocity.x >= 0) || (horizontalInput < 0 && localVelocity.x <= 0)) {
            totalAcceleration = horizontalInput * acceleration;
        }
        // reversing direction
        else if ((horizontalInput > 0 && localVelocity.x < 0) || (horizontalInput < 0 && localVelocity.x > 0)) {
            totalAcceleration = horizontalInput * deceleration;
        }
        // slowing down from friction when releasing input while in motion
        else if (isGrounded && horizontalInput == 0 && localVelocity.x != 0) {
            totalAcceleration = -Mathf.Sign(localVelocity.x) * friction;
        }

        localVelocity.x += totalAcceleration * Time.deltaTime;

        // come to a stop when slowing down from friction (don't let friction reverse X velocity direction) 
        bool xVelocitySignChange = (oldLocalVelocity.x < 0 && localVelocity.x > 0) || (oldLocalVelocity.x > 0 && localVelocity.x < 0);
        if (horizontalInput == 0 && xVelocitySignChange) {
            localVelocity.x = 0;
        }

        // don't let new X speed exceed max X speed
        if (Mathf.Abs(localVelocity.x) >= maxHorizontalSpeed)
            localVelocity.x = Mathf.Sign(localVelocity.x) * maxHorizontalSpeed;
    }

    Vector2 CheckDirection(Vector2 direction, int rayCount, Color color) {
        List<RaycastHit2D> hits = CastDirection(direction, rayCount, color);
        Vector2 collisionPositionOffset = CheckHits(hits, direction);
        return collisionPositionOffset;
    }

    List<RaycastHit2D> CastDirection(Vector2 direction, int rayCount, Color color) {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        direction.Normalize();

        int layerMask = LayerMask.GetMask("Tile");
//        float raycastLength = skinWidth;
        float raycastLength = 0;
        float velocityComponent = Vector2.Dot(worldVelocity, direction);
        Vector2 perpendicularDirection = (Vector2)Vector3.Cross(Vector3.forward, direction);
        if (velocityComponent > 0) {
            raycastLength = skinWidth + velocityComponent * Time.deltaTime;
        }

        if (direction == -(Vector2)transform.up) {
            raycastLength = skinWidth + 0.1f;
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

            if (hit) {
                hits.Add(hit);
            }
        }

        return hits;
    }

    Vector2 CheckHits(List<RaycastHit2D> hits, Vector2 direction) {
        Vector2 collisionPositionOffset = Vector2.zero;
        float velocityComponent = Vector2.Dot(localVelocity, direction);

        foreach (RaycastHit2D hit in hits) {
            if (hit && velocityComponent >= 0) {
                localVelocity -= velocityComponent * direction;
                collisionPositionOffset = (hit.distance - skinWidth) * direction;
                return collisionPositionOffset;
            }
        }

        return collisionPositionOffset;
    }

    Vector2 CheckCollision() {
//        Vector2 collisionPositionOffset = Vector2.zero;
//        collisionPositionOffset += CheckDirection(transform.right, Color.blue);
//        collisionPositionOffset += CheckDirection(-transform.right, Color.yellow);
//        collisionPositionOffset += CheckDirection(transform.up, Color.white);
//        collisionPositionOffset += CheckDirection(-transform.up, Color.magenta);

        List<RaycastHit2D> rightHits = CastDirection(transform.right, 1, Color.blue);
        List<RaycastHit2D> leftHits = CastDirection(-transform.right, 1, Color.yellow);
//        List<RaycastHit2D> upHits = CastDirection(transform.up, 1, Color.white);
        List<RaycastHit2D> downHits = CastDirection(-transform.up, 1, Color.magenta);

        Vector2 collisionPositionOffset = Vector2.zero;


//        if (!rotatedRight)
            collisionPositionOffset += CheckHits(rightHits, transform.right);
//        if (!rotatedLeft)
            collisionPositionOffset += CheckHits(leftHits, -transform.right);
//        collisionPositionOffset += CheckHits(upHits, transform.up);
        collisionPositionOffset += CheckHits(downHits, -transform.up);

        int layerMask = LayerMask.GetMask("Tile");
        RaycastHit2D slopeDetection = Physics2D.Raycast(rb.position + worldVelocity * Time.deltaTime, -transform.up, 0.6f, layerMask); 
        Debug.DrawRay(rb.position, -transform.up * 0.6f, Color.cyan);


        angle = 0;
        if (slopeDetection) {
            isGrounded = true;
            angle = Vector2.Angle(Vector2.up, slopeDetection.normal);

        }
        else {
            isGrounded = false;
        }

//        Debug.Log("Angle: " + angle);
        if (angle != rb.rotation) {
            transform.eulerAngles = new Vector3(0, 0, angle);
//            rb.MoveRotation(angle);
        }

        return collisionPositionOffset;
   
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
