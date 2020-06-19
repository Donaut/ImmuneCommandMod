using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	private bool grounded;

	private byte currentState;

	private Rigidbody body;

	private CapsuleCollider capsule;

	private float runSpeed = 4f;

	private float jumpForce = 5f;

	private float groundedTime;

	[SerializeField]
	private LayerMask walkable = 0;

	private void Start()
	{
		LidgrenGameObject component = GetComponent<LidgrenGameObject>();
		if (component.IsMine)
		{
			capsule = (base.collider as CapsuleCollider);
			body = base.rigidbody;
			body.drag = 0f;
			body.freezeRotation = true;
			RPGThirdPersonCamera.Instance.Target = base.transform;
			RPGThirdPersonCamera.Instance.Camera.transform.localPosition = Vector3.zero;
			RPGThirdPersonCamera.Instance.Camera.transform.rotation = Quaternion.identity;
		}
		else
		{
			Object.Destroy(this);
			Object.Destroy(base.rigidbody);
		}
	}

	private void changeMovementState(byte state)
	{
		if (currentState != state)
		{
			currentState = state;
			GetComponent<PlayerAnimator>().OnPlayerMovement(state);
		}
	}

	private void FixedUpdate()
	{
		if (!(body != null) || !(capsule != null))
		{
			return;
		}
		bool flag = grounded;
		grounded = Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, 1.05f, walkable);
		if (grounded && !flag && Time.time - groundedTime > 0.4f)
		{
			changeMovementState(5);
		}
		if (grounded)
		{
			groundedTime = Time.time;
		}
		if (Input.GetMouseButton(1))
		{
			base.transform.Rotate(Vector3.up, Input.GetAxisRaw("Mouse X") * 200f * Time.fixedDeltaTime);
		}
		if (grounded)
		{
			Vector3 vector = Input.GetAxisRaw("Horizontal") * Vector3.right + Input.GetAxisRaw("Vertical") * Vector3.forward;
			if (vector != Vector3.zero)
			{
				vector.Normalize();
				vector = base.transform.rotation * vector * runSpeed;
				float num = RPGControllerUtils.SignedAngle(base.transform.forward, vector.normalized, Vector3.up);
				bool flag2 = num > 1f;
				switch (Mathf.RoundToInt(Mathf.Abs(num)))
				{
				case 0:
					changeMovementState(1);
					break;
				case 45:
					changeMovementState((byte)((!flag2) ? 9 : 10));
					break;
				case 90:
					changeMovementState((byte)((!flag2) ? 7 : 8));
					break;
				case 135:
					changeMovementState((byte)((!flag2) ? 11 : 12));
					break;
				case 180:
					changeMovementState(2);
					break;
				}
				if (Mathf.Abs(num) > 91f)
				{
					vector *= 0.5f;
				}
				body.velocity = vector;
			}
			else
			{
				changeMovementState(6);
				Rigidbody rigidbody = body;
				Vector3 velocity = body.velocity;
				rigidbody.velocity = new Vector3(0f, velocity.y, 0f);
			}
			if (Input.GetKeyDown(KeyCode.Space))
			{
				grounded = false;
				body.velocity += Vector3.up * jumpForce;
				changeMovementState(3);
			}
		}
		else if (Time.time - groundedTime > 0.4f)
		{
			changeMovementState(4);
		}
	}
}
