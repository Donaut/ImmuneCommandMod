using UnityEngine;

public class ServerTeleport : MonoBehaviour
{
	public Transform m_target;

	public int m_teleportLayer;

	private void OnTriggerEnter(Collider a_collider)
	{
		if (null != m_target && null != a_collider && null != a_collider.transform && m_teleportLayer == a_collider.gameObject.layer)
		{
			Vector3 position = m_target.position;
			position.y = 0f;
			a_collider.transform.position = position;
		}
	}
}
