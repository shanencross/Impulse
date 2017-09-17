using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    void OnCollisionEnter2D(Collision2D col) {
        Player player = col.collider.gameObject.GetComponentInParent<Player>();
        if (player && !player.customCollision) {
            DoCollision(col.collider);
        }
    }

    public void DoCollision(Collider2D collider) {
        Debug.Log("Doing Collision with " + collider.transform.parent.name + " at time " + Time.time);

        Player player = collider.gameObject.GetComponentInParent<Player>();

        player.moving = false;
    }
}
