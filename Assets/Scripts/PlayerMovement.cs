using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float speed = 20;
	public float jumpPower = 9;

	public enum movementType {Force, TorqueCenter, TorqueEdge};
	public movementType useTorque = movementType.Force;

	public Vector2 pointToRotateAround = new Vector2(0, 0);

	public Transform groundCheck;

	public LayerMask groundLayer;

	[SerializeField]
	bool isGrounded = false;

	bool useInstantJumpAcceleration = false;
//
//	bool isGrounded = false;

	[SerializeField]
	Vector2 movementDirection = new Vector2(0, 0);
	bool jumping = false;

	Rigidbody2D rb;

	void Awake() {
		rb = GetComponent<Rigidbody2D>();

		if (rb == null)
			Debug.LogError("No Rigidbody2D component attached.");

		if (groundCheck == null)
			Debug.LogError("No Ground Check transform set.");
	}

	void Start() {
//		rb.AddForce(new Vector2(100, 1000));
	}

	// Update is called once per frame
	void Update () {
		float horizontalInput = Input.GetAxisRaw("Horizontal");
//		float verticalInput = Input.GetAxisRaw("Vertical");
//		movementDirection = (new Vector2(horizontalInput, verticalInput)).normalized;
		movementDirection = (new Vector2(horizontalInput, movementDirection.y)).normalized;

		isGrounded = Physics2D.Linecast(transform.position, groundCheck.position, LayerMask.GetMask("Ground"));

		if (Input.GetButtonDown("Jump") && isGrounded)
			jumping = true;
	}

	void FixedUpdate() {
		if (jumping) {
			if (useInstantJumpAcceleration)
				rb.velocity = new Vector2(rb.velocity.x, jumpPower);
			else
				rb.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
			
			jumping = false;
		}

		Move(movementDirection);
//		MoveAlongFloor(movementDirection);
	}

	void Move(Vector2 direction) {
		if (useTorque == movementType.Force)
			rb.AddForce(movementDirection * speed);
		else if (useTorque == movementType.TorqueCenter)
			rb.AddTorque(-movementDirection.x * speed);
		else if (useTorque == movementType.TorqueEdge) {
			Vector2 relativeRotationPoint = movementDirection.x * pointToRotateAround;
			Vector2 rotationPoint = (Vector2)transform.position + relativeRotationPoint;
		}
	}

	void MoveAlongFloor(Vector2 inputDirection) {
		
	}

}
