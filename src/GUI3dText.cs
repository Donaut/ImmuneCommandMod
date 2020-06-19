using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class GUI3dText : GUI3dButton
{
	public Color m_color = Color.white;

	public bool m_dropShadow;

	public Color m_shadowColor = Color.black;

	public Vector3 m_shadowOffset = new Vector3(-1f, -2f, 0f);

	public string m_LNGkey = string.Empty;

	private TextMesh m_shadowText;

	private TextMesh m_textMesh;

	protected override void Start()
	{
		base.Start();
		m_textMesh = GetComponent<TextMesh>();
		m_textMesh.renderer.material.color = m_color;
		if (m_LNGkey.Length > 0)
		{
			m_textMesh.text = LNG.Get(m_LNGkey);
		}
		if (m_dropShadow)
		{
			CreateShadow();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_dropShadow)
		{
			m_shadowText.text = m_textMesh.text;
		}
	}

	private void CreateShadow()
	{
		Transform transform = base.transform.Find("DropShadow");
		if (null != transform)
		{
			Object.DestroyImmediate(transform);
		}
		GameObject gameObject = (GameObject)Object.Instantiate(base.gameObject);
		Object.DestroyImmediate(gameObject.GetComponent<GUI3dText>());
		gameObject.layer = 2;
		gameObject.renderer.material.color = m_shadowColor;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localPosition = m_shadowOffset;
		m_shadowText = gameObject.GetComponent<TextMesh>();
	}
}
