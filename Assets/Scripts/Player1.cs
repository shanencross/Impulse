using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1 : MonoBehaviour {
    public float movementSpeed = 1;

    public bool customPhysicsStep;

    public float simulationStepTime;


    float gravity;
    CircleCollider2D circleCollider;
    Rigidbody2D rb;

    float accumulator = 0;

    void Awake() {
        simulationStepTime = Time.fixedDeltaTime;
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponentInChildren<CircleCollider2D>(); 

        if (rb == null)
            Debug.LogError("No Rigidbody2D attached.");
        if (circleCollider == null)
            Debug.LogError("No CircleCollider2D attached.");
    }

    void Start() {
        if (customPhysicsStep)
            Physics2D.autoSimulation = false;
//        rb.velocity = new Vector2(movementSpeed, 0);
    }

    void FixedUpdate() {
        if (!customPhysicsStep) {

//            transform.Translate(movementSpeed * Time.fixedDeltaTime * Vector2.right);
//            Vector2 newPosition = rb.position + movementSpeed * Time.fixedDeltaTime * Vector2.right;
//
//            rb.MovePosition(newPosition);
        }
    }

    void Update() {

        if (Physics2D.autoSimulation) {
            transform.Translate(movementSpeed * Time.deltaTime * Vector2.right);
            return;
        }

        if (customPhysicsStep) {
            accumulator += Time.deltaTime;

            while (accumulator >= simulationStepTime) {
                accumulator -= Time.deltaTime;

                transform.Translate(movementSpeed * simulationStepTime * Vector2.right);
//                Vector2 newPosition = rb.position + movementSpeed * simulationStepTime * Vector2.right;
//
//                rb.MovePosition(newPosition);

                Physics2D.Simulate(simulationStepTime);
                CollisionCheck();
            }
        }
    }

    void CollisionCheck() {
        Collider2D[] colliders = new Collider2D[10];
        int colliderCount = circleCollider.GetContacts(colliders);

        if (colliderCount > 0) {

            Tile1 tile = colliders[0].GetComponent<Tile1>();

            if (tile != null)
                tile.DoCollision(circleCollider);
        }
    }
       
        
}
