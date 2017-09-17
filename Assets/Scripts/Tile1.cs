using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile1 : MonoBehaviour {

//    void OnCollisionEnter2D(Collision2D col) {
//        Player1 player = col.collider.gameObject.GetComponentInParent<Player1>();
//        if (player) {
//            DoCollision(col.collider);
//        }
//    }

    public void DoCollision(Collider2D collider) {
        Debug.Log("Doing Collision with " + collider.transform.parent.name + " at time " + Time.time);

        Player1 player = collider.transform.parent.GetComponent<Player1>();

        player.movementSpeed = 0;
    }
}
