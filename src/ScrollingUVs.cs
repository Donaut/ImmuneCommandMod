using UnityEngine;

public class ScrollingUVs : MonoBehaviour
{
	public int materialIndex;

	public Vector2 uvAnimationRate = new Vector2(1f, 0f);

	public string textureName = "_MainTex";

	public bool ScrollBump = true;

	public string bumpName = "_BumpMap";

	private Vector2 uvOffset = Vector2.zero;

	private void LateUpdate()
	{
		uvOffset += uvAnimationRate * Time.deltaTime;
		if (base.renderer.enabled)
		{
			base.renderer.materials[materialIndex].SetTextureOffset(textureName, uvOffset);
			if (ScrollBump)
			{
				base.renderer.materials[materialIndex].SetTextureOffset(bumpName, uvOffset);
			}
		}
	}
}
