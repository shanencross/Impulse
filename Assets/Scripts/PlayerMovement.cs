using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float speed = 20;
	public float jumpPower = 9;
	public float velocityThreshold = 1;
	public float maxSlope = 46;

	public enum movementType {Force, TorqueCenter, TorqueEdge, ForceAtAngle};
	public movementType moveType = movementType.Force;

	public Vector2 pointToRotateAround = new Vector2(0, 0);

	public Transform groundCheck;

	public LayerMask groundLayer;

	[SerializeField]
	bool isGrounded = false;
	[SerializeField]
	bool airControl = true;

	[SerializeField]
	float angle = 0;
	[SerializeField]
	Vector2 jumpDirection = Vector2.up;

	[SerializeField]
	Vector2 movementDirection = new Vector2(0, 0);
	[SerializeField]
	bool jumpPressed = false;
	[SerializeField]
	bool jumpingOffGround = false; // jumping, but isGrounded detector hasn't left ground yet

	Rigidbody2D rb;


	void Awake() {
		rb = GetComponent<Rigidbody2D>();

		if (rb == null)
			Debug.LogError("No Rigidbody2D component attached.");

		if (groundCheck == null)
			Debug.LogError("No Ground Check transform set.");
	}

	// Update is called once per frame
	void Update () {
		float horizontalInput = Input.GetAxisRaw("Horizontal");
		movementDirection = (new Vector2(horizontalInput, movementDirection.y)).normalized;

		RaycastHit2D hit = Physics2D.Linecast(transform.position, groundCheck.position, groundLayer);

		bool wasGrounded = isGrounded;
		isGrounded = (bool)hit;

		if (jumpingOffGround && !isGrounded)
			jumpingOffGround = false;


//		angleSign = Mathf.Sign(-Mathf.Atan(hit.normal.x/hit.normal.y) * 180 / Mathf.PI); 

		jumpDirection = hit.normal;
		if (isGrounded) {
			angle = Vector2.SignedAngle(Vector2.up, hit.normal);
		}
//		else
//			angle = 0;

		if (wasGrounded && !isGrounded)
			angle = Mathf.Sign(angle) * (Mathf.Abs(angle) - 180);

		if (Input.GetButtonDown("Jump") && isGrounded && !jumpPressed)
			jumpPressed = true;
		
	}

	void FixedUpdate() {
		if (!float.IsNaN(angle)) {
			rb.MoveRotation(angle);
		}


		if (Mathf.Abs(angle) <= maxSlope 
				&& Mathf.Abs(rb.velocity.magnitude) <= velocityThreshold 
				&& movementDirection == Vector2.zero 
				&& !jumpPressed 
				&& !jumpingOffGround 
				&& isGrounded) {
			rb.velocity = Vector2.zero;
			rb.gravityScale = 0;
		} 


		else {
			rb.gravityScale = 1;
			Move(movementDirection);
			if (jumpPressed) {
				Jump();
			}
		}
			
	}

	void Jump() {
		Debug.Log("jumping");
		rb.AddForce(jumpDirection * jumpPower, ForceMode2D.Impulse);
		jumpingOffGround = true;
		jumpPressed = false;
	}

	void Move(Vector2 direction) {
		if (moveType == movementType.Force)
			rb.AddForce(movementDirection * speed);
		else if (moveType == movementType.TorqueCenter)
			rb.AddTorque(-movementDirection.x * speed);
		else if (moveType == movementType.TorqueEdge) {
			Vector2 relativeRotationPoint = movementDirection.x * pointToRotateAround;
			Vector2 rotationPoint = (Vector2)transform.position + relativeRotationPoint;
		} 
		else if (moveType == movementType.ForceAtAngle) {

			if (airControl && !isGrounded) {
					rb.AddForce(movementDirection * speed);
				return;
			} 
//
//			float angle_radians = angle * Mathf.PI / 180;
//
//			Vector2 forceDirection = new Vector2(Mathf.Cos(angle_radians), Mathf.Sin(angle_radians));
//
//			forceDirection *= movementDirection.x;

			Vector2 forceDirection = (Vector2)transform.right * movementDirection.x;

			rb.AddForce(forceDirection * speed);
		}
	}
}
