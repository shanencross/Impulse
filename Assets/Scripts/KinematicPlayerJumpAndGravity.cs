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
        velocity += gravity * Time.fixedDeltaTime;
    }

    public void Jump(ref Vector2 velocity) {
        Vector2 direction = (Vector2)transform.up;
       
        float velocityComponent = Vector2.Dot(velocity, direction);

        velocity -= velocityComponent * direction;
        velocity += jumpSpeed * direction;
        Debug.Log(velocity.x);
    }
    
}

