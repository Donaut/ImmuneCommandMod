public class EsportPlayer
{
	public float skill;

	public float experience;

	public float motivation;

	public float strength;

	public bool alive;

	public int kills;

	public int deaths;

	public EsportPlayer(float a_skill, float a_experience, float a_motivation)
	{
		skill = a_skill;
		experience = a_experience;
		motivation = a_motivation;
		alive = true;
		kills = 0;
		deaths = 0;
		strength = (skill * 3f + experience * 2f + motivation * 1f) / 6f;
	}
}
