public class Mission
{
	public eMissiontype m_type;

	public eObjectivesPerson m_objPerson;

	public eObjectivesObject m_objObject;

	public eLocation m_location;

	public float m_dieTime;

	public int m_xpReward;

	public bool IsEqual(Mission m)
	{
		return m != null && m_type == m.m_type && m_objObject == m.m_objObject && m_location == m.m_location;
	}
}
