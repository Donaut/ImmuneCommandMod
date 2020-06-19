using UnityEngine;

public class EmitAndDie : MonoBehaviour
{
	public int emitCount = 1;

	public float timeToLife = 1f;

	private float dieTime;

	private void Start()
	{
		if (null != base.transform.particleEmitter)
		{
			base.transform.particleEmitter.Emit(emitCount);
		}
		dieTime = Time.time + timeToLife;
	}

	private void Update()
	{
		if (Time.time > dieTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
