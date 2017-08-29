using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float speed = 1;
	public bool useTorque = false;

	[SerializeField]
	Vector2 movementDirection = new Vector2(0, 0);

	Rigidbody2D rigidbody;

	void Awake() {
		rigidbody = GetComponent<Rigidbody2D>();

		if (rigidbody == null)
			Debug.LogError("No Rigidbody2D component attached.");
	}

	void Start() {
//		rigidbody.AddForce(new Vector2(100, 1000));
	}

	// Update is called once per frame
	void Update () {
		float horizontalInput = Input.GetAxisRaw("Horizontal");


		movementDirection = (new Vector2(horizontalInput, movementDirection.y)).normalized;
	}

	void FixedUpdate() {
		Move(movementDirection);
	}

	void Move(Vector2 direction) {
		if (useTorque)
			rigidbody.AddTorque(-movementDirection.x * speed);
		else
			rigidbody.AddForce(movementDirection * speed);
	}

}
