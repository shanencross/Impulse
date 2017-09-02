using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCharacter : MonoBehaviour {

	public float speed = 5;
	public float gravity = 10;

	[SerializeField]
	Vector2 movement = Vector2.zero;

	CharacterController controller;

	void Awake() {
		controller = GetComponent<CharacterController>();
	}

	void Update() {
		float horizontalInput = Input.GetAxisRaw("Horizontal");

		movement.x = horizontalInput * speed;


		if (controller.isGrounded)
			movement.y = 0;
		else
			movement.y -= gravity * Time.deltaTime;


		controller.Move(movement * Time.deltaTime);
	}
}