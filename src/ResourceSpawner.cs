using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
	public int m_resourceBuildingType = 1;

	public float m_radius = 20f;

	public Color m_gizmoColor = Color.blue;

	private void OnDrawGizmos()
	{
		Gizmos.color = m_gizmoColor;
		Gizmos.DrawSphere(base.transform.position + Vector3.up * 0.5f, m_radius);
	}
}
