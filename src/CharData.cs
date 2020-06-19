public struct CharData
{
	public string name;

	public ulong aid;

	public int handItem;

	public int look;

	public int body;

	public int skin;

	public int rank;

	public int karma;

	public eCharType type;

	public CharData(eCharType a_type)
	{
		name = string.Empty;
		aid = 0uL;
		handItem = 0;
		look = 0;
		skin = 0;
		body = 0;
		rank = 0;
		karma = 100;
		type = a_type;
	}
}
