using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KinematicPlayer))]
public class KinematicPlayerJumpAndGravity : MonoBehaviour
{
    public KinematicPlayer player;
    public float gravityScale = 1;
    public float jumpSpeed = 10;

    public bool editorTestingFlag = true;
   
    Vector2 gravity;

    void Awake() {
        player = GetComponent<KinematicPlayer>();
        gravity = gravityScale * Physics2D.gravity;
    }


    // For testing
    void Update() {
        if (editorTestingFlag) {
            gravity = gravityScale * Physics2D.gravity;
        }
            
    }

    public void ApplyGravity(ref Vector2 velocity) {
//        Vector2 localGravity = transform.InverseTransformVector(gravity);
//        localVelocity += localGravity * Time.deltaTime;
        velocity += gravity * Time.deltaTime;
    }

    public void Jump(ref Vector2 velocity, float angle) {
        Vector2 playerRight = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        Vector2 playerUp = (Vector2)Vector3.Cross(Vector3.forward, playerRight);

        Debug.DrawRay(transform.position, playerUp * 10, Color.cyan);


        float velocityComponent = Vector2.Dot(velocity, playerUp);

//        velocity -= velocityComponent * playerUp;
        velocity += jumpSpeed * playerUp;
    }
    
}

