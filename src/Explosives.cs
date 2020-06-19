public class Explosives : ServerBuilding
{
	public float m_maxDamage = 10000f;

	public float m_radius = 5f;

	private bool m_exploded;

	public override bool Use(ServerPlayer a_player)
	{
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		if (0.1f > GetState() && !m_exploded && null != m_server)
		{
			m_server.DealExplosionDamage(base.transform.position, m_maxDamage, m_radius);
			m_exploded = true;
		}
		base.Update();
	}
}
