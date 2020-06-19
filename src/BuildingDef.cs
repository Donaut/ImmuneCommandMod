public struct BuildingDef
{
	public string ident;

	public bool persistent;

	public double decayTime;

	public BuildingDef(string a_ident, bool a_persistent = false, double a_decayTime = 0.0)
	{
		ident = a_ident;
		persistent = a_persistent;
		decayTime = a_decayTime;
	}
}
