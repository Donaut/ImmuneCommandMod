using UnityEngine;

public class ChangeRenderQueue : MonoBehaviour
{
	public int m_queueChange = 1;

	public bool m_withChildren;

	private void Awake()
	{
		if (m_withChildren)
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(true);
			if (componentsInChildren == null)
			{
				return;
			}
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				if (null != renderer && null != renderer.material)
				{
					renderer.material.renderQueue += m_queueChange;
				}
			}
		}
		else if (null != base.renderer && null != base.renderer.material)
		{
			base.renderer.material.renderQueue += m_queueChange;
		}
	}
}
