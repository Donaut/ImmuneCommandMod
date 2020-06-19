using System;
using UnityEngine;

public class Swarm : MonoBehaviour
{
	public GameObject bird;

	public int birdsCount;

	public float swarmRadius;

	public float birdsDistance;

	public float amplitude;

	public float speed;

	private float angle;

	private Vector3 lastPosition;

	private void Start()
	{
		angle = UnityEngine.Random.Range(0f, 360f);
		lastPosition = GetNewPos();
		float max = swarmRadius / birdsDistance;
		for (int i = 0; i < birdsCount; i++)
		{
			Vector3 position = new Vector3(UnityEngine.Random.Range(0f, max) * birdsDistance, UnityEngine.Random.Range(0f, max) * birdsDistance, UnityEngine.Random.Range(0f, max) * birdsDistance);
			position += base.transform.position;
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(bird, position, base.transform.rotation);
			gameObject.transform.parent = base.transform;
		}
	}

	private void FixedUpdate()
	{
		Vector3 newPos = GetNewPos();
		base.transform.position += newPos - lastPosition;
		lastPosition = newPos;
		angle = Mathf.MoveTowardsAngle(angle, angle + speed * Time.deltaTime, speed * Time.deltaTime);
	}

	private Vector3 GetNewPos()
	{
		Vector3 result = default(Vector3);
		result.x = 0f;
		result.y = Mathf.Sin(angle * ((float)Math.PI / 180f)) * amplitude;
		result.z = 0f;
		return result;
	}
}
