using UnityEngine;

public class Minimap : MonoBehaviour
{
	public float m_mapRadius = 500f;

	public Transform m_mapMarker;

	public Renderer m_hpBar;

	public Renderer m_energyBar;

	private Vector3 m_markerStartOffset = Vector3.zero;

	private LidClient m_client;

	private void Start()
	{
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_markerStartOffset = m_mapMarker.localPosition;
	}

	private void Update()
	{
		if (!Global.isServer && null != m_client && m_client.enabled)
		{
			float value = 1f - Mathf.Clamp01(m_client.GetHealth() * 0.01f) * 0.93f;
			float value2 = 1f - Mathf.Clamp01(m_client.GetEnergy() * 0.01f) * 0.93f;
			m_hpBar.material.SetFloat("_Cutoff", value);
			m_energyBar.material.SetFloat("_Cutoff", value2);
			Vector3 pos = m_client.GetPos();
			pos.x = pos.x / m_mapRadius * 0.25f;
			pos.y = pos.z / m_mapRadius * 0.25f;
			pos.z = 0f;
			m_mapMarker.localPosition = m_markerStartOffset + pos;
		}
	}
}
