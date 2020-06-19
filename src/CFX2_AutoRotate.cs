using UnityEngine;

public class CFX2_AutoRotate : MonoBehaviour
{
	public Vector3 speed = new Vector3(0f, 40f, 0f);

	private void Update()
	{
		base.transform.Rotate(speed * Time.deltaTime);
	}
}
