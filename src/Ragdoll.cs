using UnityEngine;

public class Ragdoll : MonoBehaviour
{
	public float m_timeTillFreeze = 3f;

	public float m_explosionForce = 2000f;

	private float m_freezeTime;

	private LidClient m_client;

	private void Start()
	{
		m_freezeTime = Time.time + m_timeTillFreeze;
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		if (!(null != m_client))
		{
			return;
		}
		Vector3 nearbyExplosion = m_client.GetNearbyExplosion(base.transform.position);
		if (Vector3.zero != nearbyExplosion)
		{
			Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array = componentsInChildren;
			foreach (Rigidbody rigidbody in array)
			{
				rigidbody.AddExplosionForce(m_explosionForce, nearbyExplosion - Vector3.up, 10f);
			}
		}
	}

	private void Update()
	{
		if (!(Time.time > m_freezeTime))
		{
			return;
		}
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Transform transform = componentsInChildren[i].transform;
			CharacterJoint component = transform.GetComponent<CharacterJoint>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
			if ((bool)transform.collider)
			{
				Object.Destroy(transform.collider);
			}
			Object.Destroy(componentsInChildren[i]);
		}
		Object.Destroy(this);
	}
}
