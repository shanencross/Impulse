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

    public void ApplyGravity(ref Vector2 localVelocity) {
//        Vector2 localGravity = transform.InverseTransformVector(gravity);
//        localVelocity += localGravity * Time.deltaTime;
        localVelocity += gravity * Time.deltaTime;
    }

    public void Jump(ref Vector2 localVelocity) {
        Vector2 direction = (Vector2)transform.up;

//        localVelocity += jumpSpeed * Vector2.up;
       
        float velocityComponent = Vector2.Dot(localVelocity, direction);

        localVelocity -= velocityComponent * direction;
        localVelocity += jumpSpeed * direction;
    }
    
}

