using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicTile : MonoBehaviour {

//    void OnCollisionEnter2D(Collision2D col) {
//        if (col.gameObject.layer == LayerMask.NameToLayer("Player")) {
////            Debug.Log("Relative Velocity: " + col.relativeVelocity);
////            Debug.Log("Rigidbody Velocity: " + col.rigidbody.velocity);
////            Debug.Log("0th Contact Normal Impulse: " + col.contacts[0].normalImpulse);
//
////            CollideWithPlayer(col);
//        }
//    }
//
//    void OnCollisionExit2D(Collision2D col) {
//        KinematicPlayer player = col.gameObject.GetComponentInParent<KinematicPlayer>();
//        player.colliding = false;
//    }
//
//    void CollideWithPlayer(Collision2D col) {
//        KinematicPlayer player = col.gameObject.GetComponentInParent<KinematicPlayer>();
//
//        if (player) {
////            Vector2 separation = (Vector2)transform.position - col.rigidbody.position;
////            separation.x = Mathf.Abs(separation.x);
////            separation.y = Mathf.Abs(separation.y);
////
////            if (separation.x < 1) {
////                Debug.Log(1 - separation.x);
////                Vector2 newPosition = col.rigidbody.position;
////                newPosition.x -= (1.5f - separation.x);
////
////                if (col.rigidbody.velocity.x < 0)
////                    newPosition.x *= -1;
////
////                col.rigidbody.MovePosition(newPosition);
////            }
//            player.colliding = true;
//        }
//    }
}
