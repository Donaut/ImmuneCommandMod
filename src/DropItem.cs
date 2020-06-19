using System;

[Serializable]
public class DropItem
{
	public int typeFrom;

	public int typeTo;

	public int chance;

	public int min;

	public int max;

	public DropItem(int a_typeFrom, int a_typeTo, int a_chance = 100, int a_min = 1, int a_max = 1)
	{
		typeFrom = a_typeFrom;
		typeTo = a_typeTo;
		chance = a_chance;
		min = a_min;
		max = a_max;
	}
}
