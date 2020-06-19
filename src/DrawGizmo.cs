using UnityEngine;

public class DrawGizmo : MonoBehaviour
{
	public float m_radius = 0.5f;

	public Color m_color = Color.blue;

	public bool m_isSphere = true;

	private void OnDrawGizmos()
	{
		Gizmos.color = m_color;
		if (m_isSphere)
		{
			Gizmos.DrawSphere(base.transform.position + Vector3.up * 0.5f, m_radius);
		}
		else
		{
			Gizmos.DrawCube(base.transform.position, new Vector3(m_radius * 2f, 1f, m_radius * 2f));
		}
	}
}
