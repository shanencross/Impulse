using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float moveForce = 20;
    public float brakeForce = 50;
    public Vector2 maxVelocity = new Vector2(50, 50);
    public float jumpPower = 9;
    public float gravityScale = 1;
    public float velocityThreshold = 1;
    public float maxSlope = 46;
    public bool airControl = true;
    public bool midairSpin = true;
    public bool rigidbodyRotation = true;
    public bool airFlip = false;
    public bool uprightInAir = true;

    public enum movementType {Force, TorqueCenter, TorqueEdge, ForceAtAngle};
    public movementType moveType = movementType.Force;

    public Vector2 pointToRotateAround = new Vector2(0, 0);

    public Transform groundCheck;
    public Transform groundCheckLeft;
    public Transform groundCheckRight;

    public LayerMask groundLayer;

    [SerializeField]
    bool isGrounded = false;

    [SerializeField]
    float angle = 0;
    [SerializeField]

    Vector2 jumpDirection = Vector2.up;

    [SerializeField]
    Vector2 movementDirection = new Vector2(0, 0);
    [SerializeField]
    bool jumpPressed = false;
    [SerializeField]
    bool jumpingOffGround = false; // jumping, but isGrounded detector hasn't left ground yet

    Rigidbody2D rb;


    void Awake() {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            Debug.LogError("No Rigidbody2D component attached.");

        if (groundCheck == null)
            Debug.LogError("No Ground Check transform set.");

        if (groundCheckLeft == null)
            Debug.LogError("No Ground Check Left transform set.");

        if (groundCheckRight == null)
            Debug.LogError("No Ground Check Right transform set.");

        rb.gravityScale = gravityScale;
    }

    void Update () {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        movementDirection = (new Vector2(horizontalInput, movementDirection.y)).normalized;

        RaycastHit2D hitCenter = Physics2D.Linecast(transform.position, groundCheck.position, groundLayer);
        Debug.DrawLine(transform.position, groundCheck.position, Color.blue);

        RaycastHit2D hitLeft = Physics2D.Linecast(transform.position - transform.right * 0.5f, groundCheckLeft.position, groundLayer);
        Debug.DrawLine(transform.position - transform.right *  0.5f, groundCheckLeft.position, Color.blue);

        RaycastHit2D hitRight = Physics2D.Linecast(transform.position + transform.right * 0.5f, groundCheckRight.position, groundLayer);
        Debug.DrawLine(transform.position + transform.right * 0.5f, groundCheckRight.position, Color.blue);

        bool wasGrounded = isGrounded;

//        RaycastHit2D hit = hitLeft;
//        if (!hit) {
//            hit = hitRight;
//
//            if (!hit)
//                hit = hitCenter;
//        }

        RaycastHit2D hit = hitCenter;

        isGrounded = (bool)hit;


//        if (!isGrounded)
//            UnityEditor.EditorApplication.isPaused = true;

        if (jumpingOffGround && !isGrounded)
            jumpingOffGround = false;

        if (isGrounded) {
            jumpDirection = hit.normal;
            angle = Vector2.SignedAngle(Vector2.up, hit.normal);

            if (midairSpin && !rigidbodyRotation)
                transform.rotation = Quaternion.LookRotation(transform.forward, hit.normal);
        }
        else if (uprightInAir){
            angle = 0;
            jumpDirection = Vector2.up;
        }

        if (airFlip && wasGrounded && !isGrounded) {
            angle = Mathf.Sign(angle) * (Mathf.Abs(angle) - 180);
            jumpDirection *= -1;
        }

        if (Input.GetButtonDown("Jump") && isGrounded && !jumpPressed)
            jumpPressed = true;

        if (!midairSpin && !rigidbodyRotation) {
            transform.rotation = Quaternion.LookRotation(transform.forward, jumpDirection);
        }

        Debug.DrawLine(transform.position, groundCheck.position, Color.cyan);

//        transform.position += new Vector3(Time.deltaTime, 0, 0);
    }

    void FixedUpdate() {
        if (rigidbodyRotation) {
            if (!midairSpin || (midairSpin && isGrounded))
                rb.MoveRotation (angle);
//                rb.rotation = angle;
        }
        if (Mathf.Abs(angle) <= maxSlope 
                && Mathf.Abs(rb.velocity.magnitude) <= velocityThreshold 
                && movementDirection == Vector2.zero 
                && !jumpPressed 
                && !jumpingOffGround 
                && isGrounded) {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        } 
            
        else {
            rb.gravityScale = gravityScale;
            Move(movementDirection);
            if (jumpPressed) {
                Jump();
            }
            CheckMaxVelocity();
        }
            
    }

    void CheckMaxVelocity() {
        Vector2 velocity = rb.velocity;
        if (Mathf.Abs(velocity.x) >= maxVelocity.x) {
//            Debug.Log("Limiting velocity.x (" + velocity.x + ") to maxVelocity.x (" + maxVelocity.x + ")");
            velocity.x = Mathf.Sign(velocity.x) * maxVelocity.x;
        }
        if (Mathf.Abs(velocity.y) >= maxVelocity.y) {
//            Debug.Log("Limiting velocity.y (" + velocity.y + ") to maxVelocity.y (" + maxVelocity.y + ")");
            velocity.y = Mathf.Sign(velocity.y) * maxVelocity.y;
        }
        rb.velocity = velocity;
        Debug.Log(rb.velocity);
    }

    void Jump() {
        Debug.Log("jumping");
        rb.AddForce(jumpDirection * jumpPower, ForceMode2D.Impulse);
        jumpingOffGround = true;
        jumpPressed = false;
    }

    void Move(Vector2 direction) {
        if (moveType == movementType.Force)
            rb.AddForce(movementDirection * moveForce);
        else if (moveType == movementType.TorqueCenter)
            rb.AddTorque(-movementDirection.x * moveForce);
        else if (moveType == movementType.TorqueEdge) {
            Vector2 relativeRotationPoint = movementDirection.x * pointToRotateAround;
            Vector2 rotationPoint = (Vector2)transform.position + relativeRotationPoint;
            // unfinished
        } 
        else if (moveType == movementType.ForceAtAngle) {

            Vector2 forceDirection;
            if (airControl && !isGrounded) {
                forceDirection = movementDirection;
            }
            else {
                forceDirection = (Vector2)transform.right * movementDirection.x;
            }

            Vector2 force;
            if (isGrounded && ((rb.velocity.x > 0 && forceDirection.x < 0) || (rb.velocity.x < 0 && forceDirection.x > 0))) {
                force = forceDirection * brakeForce;
//                Debug.Log("Decellerating");
            }
            else {
                force = forceDirection * moveForce;
//                Debug.Log("Accelerating");
            }

            rb.AddForce(force);
        }
    }
}
