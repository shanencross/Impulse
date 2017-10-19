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

    public float maxSlopeClimbAngle = 90;

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
    bool isGrounded = true;

    [SerializeField]
    Vector2 velocity;
    [SerializeField]
    float groundSpeed;

    Rigidbody2D rb;
    BoxCollider2D boxCollider;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();

        if (rb.bodyType != RigidbodyType2D.Kinematic)
            rb.bodyType = RigidbodyType2D.Kinematic;

        boxCollider = GetComponentInChildren<BoxCollider2D>();

        velocity = Vector2.zero;
        groundSpeed = 0;

        playerGravity = GetComponent<KinematicPlayerJumpAndGravity>();
    }

    void Update() {
        UpdateInput();
    }

    void FixedUpdate() {
//        if (!isGrounded) {
//            angle = 0;
//            if (playerGravity) {
//                playerGravity.ApplyGravity();
//            }
//        }



         // update velocity variable in case rigidbody velocity has been modified
        Vector2 position = rb.position;

        UpdateGroundVelocity();
        SetVelocityFromGroundSpeed();

//        if (!isGrounded) {
//            playerGravity.ApplyGravity(ref velocity);
//        }

        float oldAngle = angle;
        Vector2 playerRight = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        Vector2 playerUp = (Vector2)Vector3.Cross(Vector3.forward, playerRight);
        Vector2 oldBottomCenter = position - playerUp * halfWidth;

        Vector2 collisionPositionOffset = CheckCollision(position);
        SetVelocityFromGroundSpeed();

        // teleport to location of slope contact
        position += collisionPositionOffset;

        Vector2 bottomCenter = position - playerUp * halfWidth;
        Debug.DrawRay(bottomCenter, -transform.up * 5, Color.yellow);

        // apply center position offset due to rotation about bottom center
        Vector2 rotationPositionOffset;
        float angleChangeRadians = (angle - oldAngle) * Mathf.Deg2Rad;
        float x = (position.x - bottomCenter.x) * Mathf.Cos(angleChangeRadians) - (position.y - bottomCenter.y) * Mathf.Sin(angleChangeRadians) + bottomCenter.x;
        float y = (position.x - bottomCenter.x) * Mathf.Sin(angleChangeRadians) + (position.y - bottomCenter.y) * Mathf.Cos(angleChangeRadians) + bottomCenter.y;
        rotationPositionOffset = new Vector2(x, y) - position;
        if (rotationPositionOffset != Vector2.zero) {
            position += rotationPositionOffset;
        }
            
        playerRight = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        playerUp = (Vector2)Vector3.Cross(Vector3.forward, playerRight);
        bottomCenter = position - playerUp * halfWidth;
      
        float distanceChanged = (bottomCenter - oldBottomCenter).magnitude;

        // if the distance teleported is greater than the distance we'd travel using current velocity,
        // set it equal to the current velocity so that we don't move any further afte teleporting
        if (velocity.magnitude * Time.deltaTime < distanceChanged) {
//            Debug.Log("Teleport distance to slope is greater than velocity travel distance");
//            Debug.Log("Velocity times time: " + velocity.magnitude * Time.deltaTime);
//            Debug.Log("Teleport distance: " + distanceChanged);

            distanceChanged = velocity.magnitude * Time.deltaTime;
        }

        position += velocity * Time.deltaTime - distanceChanged * velocity.normalized;

        rb.MoveRotation(angle);
        rb.MovePosition(position);

    }

    void SetVelocityFromGroundSpeed() {
        if (isGrounded)
            velocity = new Vector2(groundSpeed * Mathf.Cos(angle * Mathf.Deg2Rad), groundSpeed * Mathf.Sin(angle * Mathf.Deg2Rad));
        else
            velocity = new Vector2(groundSpeed, velocity.y);
    }

    void UpdateGroundVelocity() {
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
        if ((horizontalInput > 0 && groundSpeed >= 0) || (horizontalInput < 0 && groundSpeed <= 0)) {
            totalAcceleration = horizontalInput * acceleration;
        }
        // reversing direction
        else if ((horizontalInput > 0 && groundSpeed < 0) || (horizontalInput < 0 && groundSpeed > 0)) {
            totalAcceleration = horizontalInput * deceleration;
        }
        // slowing down from friction when releasing input while in motion
        else if (isGrounded && horizontalInput == 0 && groundSpeed != 0) {
            totalAcceleration = -Mathf.Sign(groundSpeed) * friction;
        }

        float oldGroundSpeed = groundSpeed;
        groundSpeed += totalAcceleration * Time.deltaTime;

        // come to a stop when slowing down from friction (don't let friction reverse X velocity direction)
        bool xVelocitySignChange = (oldGroundSpeed < 0 && groundSpeed > 0) || (oldGroundSpeed > 0 && groundSpeed < 0);
        if (horizontalInput == 0 && xVelocitySignChange) {
            groundSpeed = 0;
        }

        // don't let new X speed exceed max X speed
        if (Mathf.Abs(groundSpeed) >= maxHorizontalSpeed) {
            groundSpeed = Mathf.Sign(groundSpeed) * maxHorizontalSpeed;
        }
    }
        
        
    void UpdateVelocityY() {
        if (playerGravity) {
            playerGravity.ApplyGravity(ref velocity);

            if (jumpInput && isGrounded) {
                playerGravity.Jump(ref velocity);
                jumpPerformed = true;
            }
        }

        if (Mathf.Abs(velocity.y) >= maxVerticalSpeed)
            velocity.y = Mathf.Sign(velocity.y) * maxVerticalSpeed;

//        if (!isGrounded && velocity.y < 0 && velocity.y > -airDragFallSpeedThreshold && Mathf.Abs(velocity.x) >= airDragHorizontalSpeedThreshold) {
//            velocity.y *= airDrag;
//        }
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
        float velocityComponent = Vector2.Dot(velocity, direction);
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

    Vector2 CheckCollision(Vector2 position) {
        Vector2 collisionPositionOffset = Vector2.zero;
        collisionPositionOffset += CheckWall(position);
        collisionPositionOffset += CheckConcaveSlope(position);
        collisionPositionOffset += CheckConvexSlope(position);

        return collisionPositionOffset;
    }

//    Vector2 CheckFloor(Vector2 position) {
//        Vector2 collisionPositionOffset = Vector2.zero;
//
//        if (isGrounded)
//            return collisionPositionOffset;
//
//
//
//        int layerMask = LayerMask.GetMask("Tile");
//        Vector2 direction = velocity.normalized;
//        Vector2 floorRayOrigin = position - (halfWidth - margin) * direction;
//        Vector2 floorRayDistance =- margin * direction - velocity * Time.deltaTime;
//
//    }

    Vector2 CheckWall(Vector2 position) {
        Vector2 collisionPositionOffset = Vector2.zero;
        int layerMask = LayerMask.GetMask("Tile");
        Vector2 direction = velocity.normalized;
        Vector2 wallRayOrigin = position + (halfWidth - margin) * direction;
        Vector2 wallRayDistance = margin * direction + velocity * Time.deltaTime;

        RaycastHit2D wallRayHit = Physics2D.Raycast(wallRayOrigin, wallRayDistance.normalized, wallRayDistance.magnitude, layerMask);
        Debug.DrawRay(wallRayOrigin, wallRayDistance, Color.black);

        if (wallRayHit && wallRayHit.distance > 0) {
            float wallAngle = Vector2.SignedAngle(-Vector2.up, -wallRayHit.normal);
            Debug.Log("Wall angle: "+ wallAngle.ToString("F10"));
            float angleDifference = Mathf.Abs(wallAngle - angle);

            Debug.Log("Angle difference: " + angleDifference.ToString("F10"));
            if (angleDifference >= 180 - 0.001f) {
                angleDifference = Mathf.Abs(angleDifference - 360);
            }
            Debug.Log("Updated angle difference: " + angleDifference.ToString("F10"));

            if (angleDifference >= maxSlopeClimbAngle - 0.001f) {
                collisionPositionOffset = (wallRayHit.distance - margin) * direction;
                groundSpeed = 0;
            }
        }

        return collisionPositionOffset;
    }

    Vector2 CheckConcaveSlope(Vector2 position) {
        Vector2 collisionPositionOffset = Vector2.zero;
        int layerMask = LayerMask.GetMask("Tile");
        Vector2 playerRight = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        Vector2 playerUp = (Vector2)Vector3.Cross(Vector3.forward, playerRight);

        Vector2 direction = velocity.normalized;

        Vector2 slopeRayOrigin = position - (halfWidth - margin) * playerUp;
        Vector2 travelDistance = velocity * Time.deltaTime;
        RaycastHit2D slopeHit = Physics2D.Raycast(slopeRayOrigin, direction, travelDistance.magnitude, layerMask); 
        Debug.DrawRay(slopeRayOrigin, travelDistance, Color.cyan);

        if (slopeHit && slopeHit.distance > 0) {
//            isGrounded = true;
            Debug.DrawRay(slopeHit.point, slopeHit.normal * 10, Color.blue);

            collisionPositionOffset = slopeHit.point - position + halfWidth * playerUp;

            Vector2 bottomCenter = slopeRayOrigin - margin * playerUp;
            float oldAngle = angle;
            angle = Vector2.SignedAngle(-Vector2.up, -slopeHit.normal);
            float angleDifference = Mathf.Abs(angle - oldAngle);
            if (angleDifference >= 180 - 0.001f) {
                angleDifference = Mathf.Abs(angleDifference - 360);
            }

            if (angleDifference >= maxSlopeClimbAngle - 0.001f) {
                Debug.Log("nearly 90 degree collision");
                angle = oldAngle;
                collisionPositionOffset = Vector2.zero;
            }        
        }
        return collisionPositionOffset;
    }

    Vector2 CheckConvexSlope(Vector2 position) {
        Vector2 collisionPositionOffset = Vector2.zero;
        int layerMask = LayerMask.GetMask("Tile");
        Vector2 playerRight = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        Vector2 playerUp = (Vector2)Vector3.Cross(Vector3.forward, playerRight);

        Vector2 downSlopeRayOrigin = position - (halfWidth + margin) * playerUp + velocity * Time.deltaTime;
        Vector2 downSlopeRayTravelDistance = -velocity * Time.deltaTime;
        RaycastHit2D downSlopeRayHit = Physics2D.Raycast(downSlopeRayOrigin, downSlopeRayTravelDistance.normalized, downSlopeRayTravelDistance.magnitude, layerMask);
        Debug.DrawRay(downSlopeRayOrigin, downSlopeRayTravelDistance, Color.green);

        if (downSlopeRayHit && downSlopeRayHit.distance > 0) {
            //            UnityEditor.EditorApplication.isPaused = true;
            Debug.DrawRay(downSlopeRayHit.point, downSlopeRayHit.normal * 10, Color.blue);
            collisionPositionOffset = downSlopeRayHit.point - position + halfWidth * playerUp;

            float oldAngle = angle;
            angle = Vector2.SignedAngle(-Vector2.up, -downSlopeRayHit.normal);
            float angleDifference = Mathf.Abs(angle - oldAngle);


            if (angleDifference >= 180 - 0.001f) {
                angleDifference = Mathf.Abs(angleDifference - 360);
            }

            if (angleDifference >= maxSlopeClimbAngle - 0.001f) {
                Debug.Log("nearly 90 degree collision: " + angleDifference.ToString("F10"));
                angle = oldAngle;
//                angle = 0;
                collisionPositionOffset = Vector2.zero;
//                isGrounded = false;
            }
            else {
//                isGrounded = true;
            }
        }
        else if (downSlopeRayHit && downSlopeRayHit.distance == 0) {
            isGrounded = true;
        }

        return collisionPositionOffset;
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
