using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MyMod
{
	public class LangCfgFile : LNG
	{
		private static Hashtable m_langCfgVars = new Hashtable();

		private static bool m_loadedLangCfg = false;
		public void Init()
		{ 
			
		}
		private void LoadConfig()
		{
			if (!m_loadedLangCfg)
			{
				m_loadedLangCfg = true;
				string path = "Plugin_data\\Lang\\default.ice"; //hmm maybe use the default translation as a proxy load until i can figure out the proper code for dynamic switch?
				string empty = string.Empty;
				try
				{
					if (File.Exists(path))
					{
						StreamReader streamReader = File.OpenText(path);
						string text = streamReader.ReadToEnd();
						text = text.Replace("\r", string.Empty).Replace(" ", string.Empty);
						string[] array = text.Split('\n');
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] != null && array[i].Contains("="))
							{
								string[] array2 = array[i].Split('=');
								if (array2 != null && array2.Length == 2 && 0 < array2[0].Length && 0 < array2[1].Length)
								{
									m_langCfgVars.Add(array2[0], array2[1]);
								}
								
							}
						}
					}
				}
				catch (Exception arg)
				{
					Debug.Log("LangCfgFile.cs: caught exception " + arg);
				}
			}
		}

		public string GetVar(string a_key, string a_emptyReturn = "")
		{
			LoadConfig();
			return (!m_langCfgVars.Contains(a_key)) ? a_emptyReturn : ((string)m_langCfgVars[a_key]);
		}
	}
}
