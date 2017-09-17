using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public float mass = 1;
    public float gravityScale = 1;
    public float movementSpeed = 1;
    public float jumpForce = 10;

    public bool moving = true;
    public bool customCollision = true;

    public float simulationStepTime = 0.02f;


    public enum updateType {regularUpdate, fixedUpdate, customUpdate};
    public updateType update = updateType.fixedUpdate;

    public enum movementType {MovePosition, ChangeVelocity, Translate}
    public movementType movement = movementType.MovePosition;

    float gravity;
    CircleCollider2D circleCollider;
    Rigidbody2D rb;

    float accumulator = 0;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponentInChildren<CircleCollider2D>(); 
        gravity = gravityScale * Physics2D.gravity.magnitude;

        if (rb == null)
            Debug.LogError("No Rigidbody2D attached.");
        if (circleCollider == null)
            Debug.LogError("No CircleCollider2D attached.");
    }

    void Start() {
        if (movement == movementType.ChangeVelocity)
            rb.velocity = new Vector2(movementSpeed, 0);
    }

    void Update() {

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
            moving = true;
        else
            moving = false;

        if (horizontalInput > 0)
            movementSpeed = Mathf.Abs(movementSpeed);
        else if (horizontalInput < 0)
            movementSpeed = -Mathf.Abs(movementSpeed);

        if (update == updateType.regularUpdate) {
            DoUpdate(Time.deltaTime);
        }

        if (update == updateType.customUpdate) {
            Physics2D.autoSimulation = false;
            CustomUpdate();
        }
    }

    void CustomUpdate() {
        accumulator += Time.deltaTime;

        while (accumulator >= simulationStepTime) {
            accumulator -= Time.deltaTime;
            DoUpdate(simulationStepTime);
            Physics2D.Simulate(simulationStepTime);
        }
    }

    void FixedUpdate() {
        if (update == updateType.fixedUpdate) {
            Physics2D.autoSimulation = true;
            DoUpdate(Time.fixedDeltaTime);
        }
    }

    void DoUpdate(float deltaTime) {
        if (customCollision) {
//            Collider2D[] colliders = new Collider2D[10];
//            int colliderCount = circleCollider.GetContacts(colliders);
//
//            if (colliderCount > 0) {
//       
//                Tile tile = colliders[0].GetComponent<Tile>();
//
//                if (tile != null)
//                    tile.DoCollision(circleCollider);
//            }

//            Vector2 raycastOrigin = (Vector2)transform.position + 0.4f * Vector2.right;
//            Vector2 raycastDir = Vector2.right;
//            float raycastDist = 0.2f;
//            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDir, raycastDist, LayerMask.GetMask("Tile"));
//            Debug.DrawRay(raycastOrigin, raycastDir * raycastDist, Color.blue);
//
//            if (hit) {
//                Tile tile = hit.collider.GetComponent<Tile>();
//                
//                if (tile != null)
//                    tile.DoCollision(circleCollider);
//            }

            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Tile");
            filter.useLayerMask = true;
            Collider2D[] results = new Collider2D[10];
            int overlap = circleCollider.OverlapCollider(filter, results);

            if (overlap > 0) {
                Tile tile = results[0].GetComponent<Tile>();

                if (tile != null)
                    tile.DoCollision(circleCollider);
            }

        }

        if (moving) {

            if (movement == movementType.MovePosition) {
                Vector2 newPosition = rb.position + movementSpeed * deltaTime * Vector2.right;

                rb.MovePosition(newPosition);
            }
            else if (movement == movementType.Translate) {
                transform.Translate(movementSpeed * deltaTime * Vector2.right);
            }
            else if (movement == movementType.ChangeVelocity && rb.velocity == Vector2.zero)
                rb.velocity = new Vector2(movementSpeed, 0);
                

        }
        else if (movement == movementType.ChangeVelocity) {
            rb.velocity = Vector2.zero;
        }
    }
}
