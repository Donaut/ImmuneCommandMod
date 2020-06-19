using System;
using UnityEngine;

public class Bird : MonoBehaviour
{
	public int animCount = 2;

	public float speedX;

	public float speedY;

	public float speedZ;

	public float amplitudeX;

	public float amplitudeY;

	public float amplitudeZ;

	private Animator anim;

	private bool canChangeAnim;

	private float angleX;

	private float angleY;

	private float angleZ;

	private Vector3 lastPosition;

	private void Start()
	{
		anim = GetComponent<Animator>();
		angleX = UnityEngine.Random.Range(0, 360);
		angleY = UnityEngine.Random.Range(0, 360);
		angleZ = UnityEngine.Random.Range(0, 360);
		lastPosition = GetNewPos();
	}

	private void OnAnimatorMove()
	{
		if (anim.GetCurrentAnimatorStateInfo(0).IsTag("NewAnim"))
		{
			if (canChangeAnim)
			{
				anim.SetInteger("AnimNum", UnityEngine.Random.Range(0, animCount + 1));
				canChangeAnim = false;
				Debug.Log("Bird anim: " + anim.GetInteger("AnimNum"));
			}
		}
		else
		{
			canChangeAnim = true;
		}
		Vector3 newPos = GetNewPos();
		base.transform.position += newPos - lastPosition;
		lastPosition = newPos;
		angleX = Mathf.MoveTowardsAngle(angleX, angleX + speedX * Time.deltaTime, speedX * Time.deltaTime);
		angleY = Mathf.MoveTowardsAngle(angleY, angleY + speedY * Time.deltaTime, speedY * Time.deltaTime);
		angleZ = Mathf.MoveTowardsAngle(angleZ, angleZ + speedZ * Time.deltaTime, speedZ * Time.deltaTime);
	}

	private Vector3 GetNewPos()
	{
		Vector3 result = default(Vector3);
		result.x = Mathf.Sin(angleX * ((float)Math.PI / 180f)) * amplitudeX;
		result.y = Mathf.Sin(angleY * ((float)Math.PI / 180f)) * amplitudeY;
		result.z = Mathf.Sin(angleZ * ((float)Math.PI / 180f)) * amplitudeZ;
		return result;
	}
}
