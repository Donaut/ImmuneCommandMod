using UnityEngine;

public class AntiRoll : MonoBehaviour
{
	public WheelCollider m_wheelFL;

	public WheelCollider m_wheelFR;

	public WheelCollider m_wheelRL;

	public WheelCollider m_wheelRR;

	public float m_antiRoll = 1f;

	private void FixedUpdate()
	{
		WheelHit hit = default(WheelHit);
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		float num4 = 1f;
		bool groundHit = m_wheelFL.GetGroundHit(out hit);
		if (groundHit)
		{
			Vector3 vector = m_wheelFL.transform.InverseTransformPoint(hit.point);
			num = (0f - vector.y - m_wheelFL.radius) / m_wheelFL.suspensionDistance;
		}
		bool groundHit2 = m_wheelFR.GetGroundHit(out hit);
		if (groundHit2)
		{
			Vector3 vector2 = m_wheelFR.transform.InverseTransformPoint(hit.point);
			num2 = (0f - vector2.y - m_wheelFR.radius) / m_wheelFR.suspensionDistance;
		}
		bool groundHit3 = m_wheelRL.GetGroundHit(out hit);
		if (groundHit3)
		{
			Vector3 vector3 = m_wheelRL.transform.InverseTransformPoint(hit.point);
			num3 = (0f - vector3.y - m_wheelRL.radius) / m_wheelRL.suspensionDistance;
		}
		bool groundHit4 = m_wheelRR.GetGroundHit(out hit);
		if (groundHit4)
		{
			Vector3 vector4 = m_wheelRR.transform.InverseTransformPoint(hit.point);
			num4 = (0f - vector4.y - m_wheelRR.radius) / m_wheelRR.suspensionDistance;
		}
		float num5 = num - num2;
		JointSpring suspensionSpring = m_wheelFL.suspensionSpring;
		float num6 = num5 * suspensionSpring.spring * m_antiRoll;
		float num7 = num3 - num4;
		JointSpring suspensionSpring2 = m_wheelRL.suspensionSpring;
		float num8 = num7 * suspensionSpring2.spring * m_antiRoll;
		if (groundHit)
		{
			base.rigidbody.AddForceAtPosition(m_wheelFL.transform.up * (0f - num6), m_wheelFL.transform.position);
		}
		if (groundHit2)
		{
			base.rigidbody.AddForceAtPosition(m_wheelFR.transform.up * num6, m_wheelFR.transform.position);
		}
		if (groundHit3)
		{
			base.rigidbody.AddForceAtPosition(m_wheelRL.transform.up * (0f - num8), m_wheelRL.transform.position);
		}
		if (groundHit4)
		{
			base.rigidbody.AddForceAtPosition(m_wheelRR.transform.up * num8, m_wheelRR.transform.position);
		}
	}
}
