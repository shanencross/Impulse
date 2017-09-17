using UnityEngine;
using System.Collections;

[RequireComponent(typeof(KinematicPlayer))]
public class KinematicPlayerJumpAndGravity : MonoBehaviour
{
    public KinematicPlayer player;
    public float gravityScale = 1;
    public float jumpSpeed = 10;
   
    Vector2 gravity;

    void Awake() {
        player = GetComponent<KinematicPlayer>();
        gravity = gravityScale * Physics2D.gravity;
    }

    public void ApplyGravity(ref Vector2 velocity) {
        velocity += gravity * Time.fixedDeltaTime;
    }

    public void Jump(ref Vector2 velocity) {
        velocity.y = jumpSpeed;
    }
    
}

