using UnityEngine;

public class SwarmCenter : MonoBehaviour
{
	public float speed;

	private void Start()
	{
		base.transform.Rotate(new Vector3(0f, Random.Range(0f, 360f), 0f));
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, speed * Time.deltaTime, 0f));
	}
}
