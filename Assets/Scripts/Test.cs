using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + 0f * transform.up, transform.right, 5, LayerMask.GetMask("Tile"));
        Debug.DrawRay(transform.position + 0f * transform.up, transform.right * 5);


        RaycastHit2D reverseHit = Physics2D.Raycast(transform.position + 5 * transform.right, -transform.right, 5, LayerMask.GetMask("Tile"));
        Debug.DrawRay(transform.position + 5 * transform.right, -transform.right * 5);

        Debug.Log("Updating...");
        if (hit)
            Debug.Log("Hit : " + hit.point);
        if (reverseHit)
            Debug.Log("Reverse Hit: " + reverseHit.point);
	}
}
