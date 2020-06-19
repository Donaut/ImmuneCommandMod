using System.Collections;
using UnityEngine;

public class TrackPoint : MonoBehaviour
{
	public int m_branch;

	public float m_chunkLength = 10f;

	public TrackPoint m_nextPoint;

	public TrackPoint m_prevPoint;

	private ArrayList m_subPoints = new ArrayList();

	private Vector3 m_lastPos = new Vector3(0f, -1123f, 0f);

	public void UpdateTrack()
	{
		m_subPoints.Clear();
		Vector3 vector = Vector3.zero;
		if ((bool)m_prevPoint && m_prevPoint.m_subPoints.Count > 0)
		{
			Vector3 b = (Vector3)m_prevPoint.m_subPoints[m_prevPoint.m_subPoints.Count - 1];
			vector = (base.transform.position - b).normalized;
		}
		if ((bool)m_nextPoint)
		{
			Vector3 dirToNextPoint = m_nextPoint.transform.position - base.transform.position;
			if (Vector3.zero == vector)
			{
				vector = dirToNextPoint.normalized;
			}
			CreateSubPoints(vector, dirToNextPoint);
			if (m_nextPoint.gameObject.name != "1")
			{
				m_nextPoint.UpdateTrack();
			}
		}
	}

	private void OnDrawGizmos()
	{
		if ((m_lastPos - base.transform.position).sqrMagnitude > 0f)
		{
			TrackPoint trackPoint = this;
			while (trackPoint.gameObject.name != "1")
			{
				trackPoint = trackPoint.m_prevPoint;
			}
			trackPoint.UpdateTrack();
			m_lastPos = base.transform.position;
		}
		if ((bool)m_nextPoint)
		{
			Vector3 from = base.transform.position;
			foreach (Vector3 subPoint in m_subPoints)
			{
				Gizmos.DrawLine(from, subPoint);
				from = subPoint;
			}
			Gizmos.DrawLine(from, m_nextPoint.transform.position);
		}
	}

	private void CreateSubPoints(Vector3 startLookDir, Vector3 dirToNextPoint)
	{
		int num = (int)((base.transform.position - m_nextPoint.transform.position).magnitude / m_chunkLength);
		Vector3 position = base.transform.position;
		Vector3 eulerAngles = Quaternion.LookRotation(startLookDir).eulerAngles;
		float y = eulerAngles.y;
		for (int i = 1; i < num + 1; i++)
		{
			float num2 = Mathf.Clamp((float)i / (float)num, 0f, 1f);
			Vector3 eulerAngles2 = Quaternion.LookRotation(m_nextPoint.transform.position - position).eulerAngles;
			float num3 = eulerAngles2.y - y;
			if (num3 > 180f)
			{
				num3 -= 360f;
			}
			else if (num3 < -180f)
			{
				num3 += 360f;
			}
			position += Quaternion.Euler(0f, num3 * num2, 0f) * startLookDir * m_chunkLength;
			m_subPoints.Add(position);
		}
	}
}
