using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public float mass = 1;
    public float gravityScale = 1;
    public float movementSpeed = 1;
    public float jumpForce = 10;

    public enum updateType {regularUpdate, fixedUpdate};
    public updateType update = updateType.fixedUpdate;

    public enum movementType {MovePosition, ChangeVelocity, Translate}
    public movementType movement = movementType.MovePosition;

    float gravity;
    CircleCollider2D circleCollider;
    Rigidbody2D rb;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponentInChildren<CircleCollider2D>(); 
        gravity = gravityScale * Physics2D.gravity.magnitude;
    }

    void Start() {
        if (movement == movementType.ChangeVelocity)
            rb.velocity = new Vector2(movementSpeed, 0);
    }

    void Update() {
        if (update == updateType.regularUpdate)
            DoUpdate();
    }

    void FixedUpdate() {
        if (update == updateType.fixedUpdate)
            DoUpdate();
    }

    void DoUpdate() {


        Collider2D[] colliders = new Collider2D[10];
        int colliderCount = circleCollider.GetContacts(colliders);

        if (colliderCount > 0) {
            Debug.Log(name + " collided with " + colliders[0].name); 

            if (movement == movementType.ChangeVelocity)
                rb.velocity = new Vector2(0, rb.velocity.y);

            return;
        }





        if (movement == movementType.MovePosition) {
            Vector2 newPosition = rb.position + movementSpeed * Time.deltaTime * Vector2.right;

            rb.MovePosition(newPosition);
        }
        else if (movement == movementType.Translate) {
            transform.Translate(movementSpeed * Time.deltaTime * Vector2.right);
        }



    }
}
