using UnityEngine;

public class GlowPulse : MonoBehaviour
{
	private void Update()
	{
		float value = (FastSin.Get(Time.time * 2f) + 1f) * 0.5f;
		if (null != base.renderer)
		{
			base.renderer.material.SetFloat("_GlowStrength", value);
		}
	}
}
