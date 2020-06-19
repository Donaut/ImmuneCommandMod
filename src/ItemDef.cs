public struct ItemDef
{
	public string ident;

	public float healing;

	public float damage;

	public float attackdur;

	public float range;

	public float durability;

	public int ammoItemType;

	public int wood;

	public int metal;

	public int stone;

	public int cloth;

	public int rankReq;

	public int buildingIndex;

	public ItemDef(string a_ident, float a_healing = 0f, float a_damage = 5f, float a_attackdur = 1f, float a_range = 1.3f, float a_durability = 0f, int a_ammoItemIndex = 0, int a_wood = 0, int a_metal = 0, int a_stone = 0, int a_cloth = 0, int a_rankReq = 0, int a_buildingIndex = 0)
	{
		ident = a_ident;
		healing = a_healing;
		damage = a_damage;
		attackdur = a_attackdur;
		range = a_range;
		durability = a_durability;
		ammoItemType = a_ammoItemIndex;
		wood = a_wood;
		metal = a_metal;
		stone = a_stone;
		cloth = a_cloth;
		rankReq = a_rankReq;
		buildingIndex = a_buildingIndex;
	}
}
