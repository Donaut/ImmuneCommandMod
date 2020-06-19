using UnityEngine;

public class ExplodePhysics : MonoBehaviour
{
	public GameObject m_explosion;

	private void Start()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Transform transform = renderer.transform;
			transform.parent = null;
			transform.gameObject.AddComponent<TimedDestroy>();
			transform.gameObject.AddComponent<BoxCollider>();
			transform.gameObject.AddComponent<Rigidbody>();
		}
		Vector3 position = base.transform.position + Vector3.up * 0.5f + (Camera.main.transform.position - base.transform.position) * 0.25f;
		GameObject gameObject = (GameObject)Object.Instantiate(m_explosion, position, Quaternion.identity);
		gameObject.transform.parent = Camera.main.transform;
		Object.Destroy(base.gameObject);
	}
}
