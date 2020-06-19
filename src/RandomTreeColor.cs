using UnityEngine;

public class RandomTreeColor : MonoBehaviour
{
	public Color m_fromColor = Color.black;

	public Color m_toColor = Color.white;

	private void Start()
	{
		base.renderer.material.color = new Color(Random.Range(m_fromColor.r, m_toColor.r), Random.Range(m_fromColor.g, m_toColor.g), Random.Range(m_fromColor.b, m_toColor.b));
	}
}
