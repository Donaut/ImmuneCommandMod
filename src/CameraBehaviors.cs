using UnityEngine;

public class CameraBehaviors : MonoBehaviour
{
	public float maxSpeed = 1f;

	public float breakSpeed = 0.1f;

	public float BoundTop;

	public float BoundBottom;

	public float BoundLeft;

	public float BoundRight;

	public bool InvertX;

	public bool InvertZ;

	private float dest_speedX;

	private float dest_speedZ;

	private float speedX;

	private float speedZ;

	private void UpdateInput()
	{
		dest_speedX = Input.GetAxis("Horizontal");
		dest_speedZ = Input.GetAxis("Vertical");
		speedX = Mathf.Lerp(speedX, dest_speedX, breakSpeed);
		speedZ = Mathf.Lerp(speedZ, dest_speedZ, breakSpeed);
		Mathf.Clamp(speedX, 0f - maxSpeed, maxSpeed);
		Mathf.Clamp(speedZ, 0f - maxSpeed, maxSpeed);
	}

	private void UpdatePosition()
	{
		Vector3 position = base.transform.position;
		if (InvertX)
		{
			position.x -= speedX;
			if (position.x > BoundLeft)
			{
				position.x = BoundLeft;
			}
			if (position.x < BoundRight)
			{
				position.x = BoundRight;
			}
		}
		else
		{
			position.x += speedX;
			if (position.x < BoundLeft)
			{
				position.x = BoundLeft;
			}
			if (position.x > BoundRight)
			{
				position.x = BoundRight;
			}
		}
		if (InvertZ)
		{
			position.z -= speedZ;
			if (position.z > BoundTop)
			{
				position.z = BoundTop;
			}
			if (position.z < BoundBottom)
			{
				position.z = BoundBottom;
			}
		}
		else
		{
			position.z += speedZ;
			if (position.z < BoundTop)
			{
				position.z = BoundTop;
			}
			if (position.z > BoundBottom)
			{
				position.z = BoundBottom;
			}
		}
		base.transform.position = position;
	}

	private void Update()
	{
		UpdateInput();
		UpdatePosition();
	}
}
