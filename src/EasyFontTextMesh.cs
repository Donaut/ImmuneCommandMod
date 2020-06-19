using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class EasyFontTextMesh : MonoBehaviour
{
	public enum TEXT_ANCHOR
	{
		UpperLeft,
		UpperRight,
		UpperCenter,
		MiddleLeft,
		MiddleRight,
		MiddleCenter,
		LowerLeft,
		LowerRight,
		LowerCenter
	}

	public enum TEXT_ALIGNMENT
	{
		left,
		right,
		center
	}

	private enum TEXT_COMPONENT
	{
		Main,
		Shadow,
		Outline
	}

	[Serializable]
	public class TextProperties
	{
		public string text = "Hello World!";

		public Font font;

		public Material customFillMaterial;

		public int fontSize = 16;

		public float size = 16f;

		public TEXT_ANCHOR textAnchor;

		public TEXT_ALIGNMENT textAlignment;

		public float lineSpacing = 1f;

		public Color fontColorTop = new Color(1f, 1f, 1f, 1f);

		public Color fontColorBottom = new Color(1f, 1f, 1f, 1f);

		public bool enableShadow;

		public Color shadowColor = new Color(0f, 0f, 0f, 1f);

		public Vector3 shadowDistance = new Vector3(0f, -1f, 0f);

		public bool enableOutline;

		public Color outlineColor = new Color(0f, 0f, 0f, 1f);

		public float outLineWidth = 0.3f;
	}

	[HideInInspector]
	public TextProperties _privateProperties;

	public bool dontOverrideMaterials;

	private Mesh textMesh;

	private MeshFilter textMeshFilter;

	private Material fontMaterial;

	private Renderer textRenderer;

	private char[] textChars;

	private bool isDirty;

	private int currentLineBreak;

	private float heightSum;

	private List<int> lineBreakCharCounter = new List<int>();

	private List<float> lineBreakAccumulatedDistance = new List<float>();

	private Vector3[] vertices;

	private int[] triangles;

	private Vector2[] uv;

	private Vector2[] uv2;

	private Color[] colors;

	[HideInInspector]
	public bool GUIChanged;

	private char LINE_BREAK = Convert.ToChar(10);

	public string Text
	{
		get
		{
			return _privateProperties.text;
		}
		set
		{
			_privateProperties.text = value;
			isDirty = true;
		}
	}

	public Font FontType
	{
		get
		{
			return _privateProperties.font;
		}
		set
		{
			_privateProperties.font = value;
			ChangeFont();
		}
	}

	public Material CustomFillMaterial
	{
		get
		{
			return _privateProperties.customFillMaterial;
		}
		set
		{
			_privateProperties.customFillMaterial = value;
			isDirty = true;
		}
	}

	public int FontSize
	{
		get
		{
			return _privateProperties.fontSize;
		}
		set
		{
			_privateProperties.fontSize = value;
			isDirty = true;
		}
	}

	public float Size
	{
		get
		{
			return _privateProperties.size;
		}
		set
		{
			_privateProperties.size = value;
			isDirty = true;
		}
	}

	public TEXT_ANCHOR Textanchor
	{
		get
		{
			return _privateProperties.textAnchor;
		}
		set
		{
			_privateProperties.textAnchor = value;
			isDirty = true;
		}
	}

	public TEXT_ALIGNMENT Textalignment
	{
		get
		{
			return _privateProperties.textAlignment;
		}
		set
		{
			_privateProperties.textAlignment = value;
			isDirty = true;
		}
	}

	public float LineSpacing
	{
		get
		{
			return _privateProperties.lineSpacing;
		}
		set
		{
			_privateProperties.lineSpacing = value;
			isDirty = true;
		}
	}

	public Color FontColorTop
	{
		get
		{
			return _privateProperties.fontColorTop;
		}
		set
		{
			_privateProperties.fontColorTop = value;
			SetColor(_privateProperties.fontColorTop, _privateProperties.fontColorBottom);
		}
	}

	public Color FontColorBottom
	{
		get
		{
			return _privateProperties.fontColorBottom;
		}
		set
		{
			_privateProperties.fontColorBottom = value;
			SetColor(_privateProperties.fontColorTop, _privateProperties.fontColorBottom);
		}
	}

	public bool EnableShadow
	{
		get
		{
			return _privateProperties.enableShadow;
		}
		set
		{
			_privateProperties.enableShadow = value;
			isDirty = true;
		}
	}

	public Color ShadowColor
	{
		get
		{
			return _privateProperties.shadowColor;
		}
		set
		{
			_privateProperties.shadowColor = value;
			SetShadowColor(_privateProperties.shadowColor);
		}
	}

	public Vector3 ShadowDistance
	{
		get
		{
			return _privateProperties.shadowDistance;
		}
		set
		{
			_privateProperties.shadowDistance = value;
			isDirty = true;
		}
	}

	public bool EnableOutline
	{
		get
		{
			return _privateProperties.enableOutline;
		}
		set
		{
			_privateProperties.enableOutline = value;
			isDirty = true;
		}
	}

	public Color OutlineColor
	{
		get
		{
			return _privateProperties.outlineColor;
		}
		set
		{
			_privateProperties.outlineColor = value;
			SetOutlineColor(_privateProperties.outlineColor);
		}
	}

	public float OutLineWidth
	{
		get
		{
			return _privateProperties.outLineWidth;
		}
		set
		{
			_privateProperties.outLineWidth = value;
			isDirty = true;
		}
	}

	private void OnEnable()
	{
		Font font = _privateProperties.font;
		font.textureRebuildCallback = (Font.FontTextureRebuildCallback)Delegate.Combine(font.textureRebuildCallback, new Font.FontTextureRebuildCallback(FontTexureRebuild));
	}

	private void Start()
	{
		CacheTextVars();
		RefreshMesh(true);
	}

	public void CacheTextVars()
	{
		textMeshFilter = GetComponent<MeshFilter>();
		if (textMeshFilter == null)
		{
			textMeshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		textMesh = textMeshFilter.sharedMesh;
		if (textMesh == null)
		{
			textMesh = new Mesh();
			textMesh.name = base.gameObject.name + GetInstanceID();
			textMeshFilter.sharedMesh = textMesh;
		}
		textRenderer = base.renderer;
		if (textRenderer == null)
		{
			textRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (dontOverrideMaterials)
		{
			return;
		}
		if (_privateProperties.customFillMaterial != null)
		{
			if (_privateProperties.enableShadow || _privateProperties.enableOutline)
			{
				if (textRenderer.sharedMaterials.Length < 2)
				{
					textRenderer.sharedMaterials = new Material[2]
					{
						_privateProperties.font.material,
						_privateProperties.customFillMaterial
					};
				}
				_privateProperties.customFillMaterial.mainTexture = _privateProperties.font.material.mainTexture;
				textRenderer.sharedMaterial = _privateProperties.font.material;
			}
			else
			{
				_privateProperties.customFillMaterial.mainTexture = _privateProperties.font.material.mainTexture;
				textRenderer.sharedMaterial = _privateProperties.customFillMaterial;
			}
		}
		else if (textRenderer.sharedMaterials == null)
		{
			textRenderer.sharedMaterials = new Material[1]
			{
				_privateProperties.font.material
			};
		}
		else
		{
			textRenderer.sharedMaterials = new Material[1]
			{
				textRenderer.sharedMaterial
			};
		}
	}

	private void RefreshMesh(bool _updateTexureInfo)
	{
		if (_updateTexureInfo)
		{
			_privateProperties.font.RequestCharactersInTexture(_privateProperties.text, _privateProperties.fontSize);
		}
		textChars = null;
		textChars = _privateProperties.text.ToCharArray();
		AnalizeText();
		int num = 1;
		if (_privateProperties.enableShadow && _privateProperties.enableOutline)
		{
			num = 6;
		}
		else if (_privateProperties.enableOutline)
		{
			num = 5;
		}
		else if (_privateProperties.enableShadow)
		{
			num = 2;
		}
		vertices = new Vector3[textChars.Length * 4 * num];
		triangles = new int[textChars.Length * 6 * num];
		uv = new Vector2[textChars.Length * 4 * num];
		uv2 = new Vector2[textChars.Length * 4 * num];
		colors = new Color[textChars.Length * 4 * num];
		int num2 = 0;
		int num3 = 0;
		if (_privateProperties.enableShadow)
		{
			ResetHelperVariables();
			char[] array = textChars;
			foreach (char character in array)
			{
				CreateCharacter(character, num2, _privateProperties.shadowDistance, _privateProperties.shadowColor, _privateProperties.shadowColor);
				num2++;
			}
			SetAlignment(num3++);
		}
		if (_privateProperties.enableOutline)
		{
			ResetHelperVariables();
			char[] array2 = textChars;
			foreach (char character2 in array2)
			{
				CreateCharacter(character2, num2, Vector3.right * _privateProperties.outLineWidth, _privateProperties.outlineColor, _privateProperties.outlineColor);
				num2++;
			}
			SetAlignment(num3++);
			ResetHelperVariables();
			char[] array3 = textChars;
			foreach (char character3 in array3)
			{
				CreateCharacter(character3, num2, Vector3.left * _privateProperties.outLineWidth, _privateProperties.outlineColor, _privateProperties.outlineColor);
				num2++;
			}
			SetAlignment(num3++);
			ResetHelperVariables();
			char[] array4 = textChars;
			foreach (char character4 in array4)
			{
				CreateCharacter(character4, num2, Vector3.up * _privateProperties.outLineWidth, _privateProperties.outlineColor, _privateProperties.outlineColor);
				num2++;
			}
			SetAlignment(num3++);
			ResetHelperVariables();
			char[] array5 = textChars;
			foreach (char character5 in array5)
			{
				CreateCharacter(character5, num2, Vector3.down * _privateProperties.outLineWidth, _privateProperties.outlineColor, _privateProperties.outlineColor);
				num2++;
			}
			SetAlignment(num3++);
		}
		ResetHelperVariables();
		char[] array6 = textChars;
		foreach (char character6 in array6)
		{
			CreateCharacter(character6, num2, Vector3.zero, _privateProperties.fontColorTop, _privateProperties.fontColorBottom);
			num2++;
		}
		SetAlignment(num3++);
		if (textMesh != null)
		{
			textMesh.Clear(true);
			SetAnchor();
			textMesh.vertices = vertices;
			textMesh.uv = uv;
			textMesh.uv2 = uv2;
			if (_privateProperties.customFillMaterial != null && (_privateProperties.enableShadow || _privateProperties.enableOutline))
			{
				SetTrianglesForMultimesh();
			}
			else
			{
				textMesh.triangles = triangles;
			}
			textMesh.colors = colors;
		}
	}

	private void ResetHelperVariables()
	{
		lineBreakAccumulatedDistance.Clear();
		lineBreakCharCounter.Clear();
		currentLineBreak = 0;
		heightSum = 0f;
	}

	private void AnalizeText()
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int i = 0; i < textChars.Length; i++)
			{
				if (textChars[i] != '\\' || i + 1 >= textChars.Length || textChars[i + 1] != 'n')
				{
					continue;
				}
				char[] array = new char[textChars.Length - 1];
				int num = 0;
				for (int j = 0; j < textChars.Length; j++)
				{
					if (j == i)
					{
						array[num] = LINE_BREAK;
						num++;
						continue;
					}
					if (j == i + 1)
					{
						j++;
						if (j >= textChars.Length)
						{
							continue;
						}
					}
					array[num] = textChars[j];
					num++;
				}
				textChars = array;
				flag = true;
				break;
			}
		}
	}

	private void CreateCharacter(char _character, int _arrayPosition, Vector3 _offset, Color _colorTop, Color _colorBottom)
	{
		if (lineBreakAccumulatedDistance.Count == 0)
		{
			lineBreakAccumulatedDistance.Add(0f);
		}
		if (lineBreakCharCounter.Count == 0)
		{
			lineBreakCharCounter.Add(0);
		}
		CharacterInfo info = default(CharacterInfo);
		if (!_privateProperties.font.GetCharacterInfo(_character, out info, _privateProperties.fontSize))
		{
			lineBreakCharCounter.Add(lineBreakCharCounter[currentLineBreak]);
			lineBreakAccumulatedDistance.Add(0f);
			currentLineBreak++;
			return;
		}
		List<int> list;
		List<int> list2 = list = lineBreakCharCounter;
		int index;
		int index2 = index = currentLineBreak;
		index = list[index];
		list2[index2] = index + 1;
		float num = _privateProperties.size / (float)_privateProperties.fontSize;
		_offset *= _privateProperties.size * 0.1f;
		float num2 = info.vert.width * num;
		float num3 = info.vert.height * num;
		Vector2 vector = new Vector2(info.vert.x, info.vert.y) * num;
		if (_character != ' ')
		{
			heightSum += (info.vert.y + info.vert.height * 0.5f) * num;
		}
		Vector3 b = new Vector3(lineBreakAccumulatedDistance[currentLineBreak] * num, (0f - _privateProperties.size) * (float)currentLineBreak * _privateProperties.lineSpacing, 0f);
		if (info.flipped)
		{
			vertices[4 * _arrayPosition] = new Vector3(vector.x + num2, num3 + vector.y, 0f) + _offset + b;
			vertices[4 * _arrayPosition + 1] = new Vector3(vector.x, num3 + vector.y, 0f) + _offset + b;
			vertices[4 * _arrayPosition + 2] = new Vector3(vector.x, vector.y, 0f) + _offset + b;
			vertices[4 * _arrayPosition + 3] = new Vector3(vector.x + num2, vector.y, 0f) + _offset + b;
		}
		else
		{
			vertices[4 * _arrayPosition] = new Vector3(vector.x + num2, num3 + vector.y, 0f) + _offset + b;
			vertices[4 * _arrayPosition + 1] = new Vector3(vector.x, num3 + vector.y, 0f) + _offset + b;
			vertices[4 * _arrayPosition + 2] = new Vector3(vector.x, vector.y, 0f) + _offset + b;
			vertices[4 * _arrayPosition + 3] = new Vector3(vector.x + num2, vector.y, 0f) + _offset + b;
		}
		List<float> list3;
		List<float> list4 = list3 = lineBreakAccumulatedDistance;
		int index3 = index = currentLineBreak;
		float num4 = list3[index];
		list4[index3] = num4 + info.width;
		triangles[6 * _arrayPosition] = _arrayPosition * 4;
		triangles[6 * _arrayPosition + 1] = _arrayPosition * 4 + 1;
		triangles[6 * _arrayPosition + 2] = _arrayPosition * 4 + 2;
		triangles[6 * _arrayPosition + 3] = _arrayPosition * 4;
		triangles[6 * _arrayPosition + 4] = _arrayPosition * 4 + 2;
		triangles[6 * _arrayPosition + 5] = _arrayPosition * 4 + 3;
		if (info.flipped)
		{
			uv[4 * _arrayPosition] = new Vector2(info.uv.x, info.uv.y + info.uv.height);
			uv[4 * _arrayPosition + 1] = new Vector2(info.uv.x, info.uv.y);
			uv[4 * _arrayPosition + 2] = new Vector2(info.uv.x + info.uv.width, info.uv.y);
			uv[4 * _arrayPosition + 3] = new Vector2(info.uv.x + info.uv.width, info.uv.y + info.uv.height);
		}
		else
		{
			uv[4 * _arrayPosition] = new Vector2(info.uv.x + info.uv.width, info.uv.y);
			uv[4 * _arrayPosition + 1] = new Vector2(info.uv.x, info.uv.y);
			uv[4 * _arrayPosition + 2] = new Vector2(info.uv.x, info.uv.y + info.uv.height);
			uv[4 * _arrayPosition + 3] = new Vector2(info.uv.x + info.uv.width, info.uv.y + info.uv.height);
		}
		if (_privateProperties.customFillMaterial != null)
		{
			Vector2 b2 = new Vector2(_offset.x, _offset.y);
			Vector2 b3 = new Vector2(b.x, b.y);
			uv2[4 * _arrayPosition] = new Vector2(vector.x + num2, num3 + vector.y) + b2 + b3;
			uv2[4 * _arrayPosition + 1] = new Vector2(vector.x, num3 + vector.y) + b2 + b3;
			uv2[4 * _arrayPosition + 2] = new Vector2(vector.x, vector.y) + b2 + b3;
			uv2[4 * _arrayPosition + 3] = new Vector2(vector.x + num2, vector.y) + b2 + b3;
		}
		colors[4 * _arrayPosition] = _colorBottom;
		colors[4 * _arrayPosition + 1] = _colorBottom;
		colors[4 * _arrayPosition + 2] = _colorTop;
		colors[4 * _arrayPosition + 3] = _colorTop;
	}

	private void SetAnchor()
	{
		Vector2 zero = Vector2.zero;
		float num = 0f;
		for (int i = 0; i < lineBreakAccumulatedDistance.Count; i++)
		{
			if (lineBreakAccumulatedDistance[i] > num)
			{
				num = lineBreakAccumulatedDistance[i];
			}
		}
		switch (_privateProperties.textAnchor)
		{
		case TEXT_ANCHOR.UpperLeft:
		case TEXT_ANCHOR.MiddleLeft:
		case TEXT_ANCHOR.LowerLeft:
			switch (_privateProperties.textAlignment)
			{
			case TEXT_ALIGNMENT.left:
				zero.x = 0f;
				break;
			case TEXT_ALIGNMENT.right:
				zero.x = num * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			case TEXT_ALIGNMENT.center:
				zero.x += num * 0.5f * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			}
			break;
		case TEXT_ANCHOR.UpperRight:
		case TEXT_ANCHOR.MiddleRight:
		case TEXT_ANCHOR.LowerRight:
			switch (_privateProperties.textAlignment)
			{
			case TEXT_ALIGNMENT.left:
				zero.x -= num * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			case TEXT_ALIGNMENT.right:
				zero.x = 0f;
				break;
			case TEXT_ALIGNMENT.center:
				zero.x -= num * 0.5f * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			}
			break;
		case TEXT_ANCHOR.UpperCenter:
		case TEXT_ANCHOR.MiddleCenter:
		case TEXT_ANCHOR.LowerCenter:
			switch (_privateProperties.textAlignment)
			{
			case TEXT_ALIGNMENT.left:
				zero.x -= num * _privateProperties.size * 0.5f / (float)_privateProperties.fontSize;
				break;
			case TEXT_ALIGNMENT.right:
				zero.x = num * 0.5f * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			case TEXT_ALIGNMENT.center:
				zero.x = 0f;
				break;
			}
			break;
		}
		if (_privateProperties.textAnchor == TEXT_ANCHOR.UpperLeft || _privateProperties.textAnchor == TEXT_ANCHOR.UpperRight || _privateProperties.textAnchor == TEXT_ANCHOR.UpperCenter)
		{
			zero.y = (0f - heightSum) / (float)textChars.Length;
		}
		else if (_privateProperties.textAnchor == TEXT_ANCHOR.MiddleCenter || _privateProperties.textAnchor == TEXT_ANCHOR.MiddleLeft || _privateProperties.textAnchor == TEXT_ANCHOR.MiddleRight)
		{
			zero.y = 0f - heightSum / (float)textChars.Length + _privateProperties.size * (float)currentLineBreak * _privateProperties.lineSpacing * 0.5f;
		}
		else if (_privateProperties.textAnchor == TEXT_ANCHOR.LowerLeft || _privateProperties.textAnchor == TEXT_ANCHOR.LowerRight || _privateProperties.textAnchor == TEXT_ANCHOR.LowerCenter)
		{
			zero.y = (0f - heightSum) / (float)textChars.Length + _privateProperties.size * (float)currentLineBreak * _privateProperties.lineSpacing;
		}
		for (int j = 0; j < vertices.Length; j++)
		{
			vertices[j].x += zero.x;
			vertices[j].y += zero.y;
		}
	}

	private void SetAlignment(int _pass)
	{
		int num = _pass * textChars.Length * 4;
		float num2 = 0f;
		for (int i = 0; i < lineBreakCharCounter.Count; i++)
		{
			switch (_privateProperties.textAlignment)
			{
			case TEXT_ALIGNMENT.right:
				num2 = (0f - lineBreakAccumulatedDistance[i]) * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			case TEXT_ALIGNMENT.center:
				num2 = (0f - lineBreakAccumulatedDistance[i]) * 0.5f * _privateProperties.size / (float)_privateProperties.fontSize;
				break;
			}
			int num3 = (i != 0) ? (lineBreakCharCounter[i - 1] * 4) : 0;
			int num4 = lineBreakCharCounter[i] * 4 - 1;
			for (int j = num3 + i * 4 + num; j <= num4 + i * 4 + num; j++)
			{
				vertices[j].x += num2;
			}
		}
	}

	private void SetTrianglesForMultimesh()
	{
		int num = 0;
		if (_privateProperties.enableOutline && _privateProperties.enableShadow)
		{
			num = 5;
		}
		else if (_privateProperties.enableOutline)
		{
			num = 4;
		}
		else if (_privateProperties.enableShadow)
		{
			num = 1;
		}
		int num2 = num * 6 * textChars.Length;
		int[] array = new int[textChars.Length * 6];
		int num3 = 0;
		for (int i = num2; i < triangles.Length; i++)
		{
			array[num3] = triangles[i];
			num3++;
		}
		num3 = 0;
		int num4 = textChars.Length * num * 6;
		int[] array2 = new int[num4];
		for (int j = 0; j < num4; j++)
		{
			array2[num3] = triangles[j];
			num3++;
		}
		textMeshFilter.sharedMesh.subMeshCount = 2;
		textMeshFilter.sharedMesh.SetTriangles(array, 1);
		textMeshFilter.sharedMesh.SetTriangles(array2, 0);
	}

	private void FontTexureRebuild()
	{
		RefreshMesh(true);
	}

	private void OnDisable()
	{
		Font font = _privateProperties.font;
		font.textureRebuildCallback = (Font.FontTextureRebuildCallback)Delegate.Remove(font.textureRebuildCallback, new Font.FontTextureRebuildCallback(FontTexureRebuild));
	}

	public void RefreshMeshEditor()
	{
		CacheTextVars();
		UnityEngine.Object.DestroyImmediate(textMesh);
		textMesh = new Mesh();
		textMesh.name = GetInstanceID().ToString();
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null)
		{
			component.sharedMesh = textMesh;
			if (base.renderer.sharedMaterial == null)
			{
				base.renderer.sharedMaterial = _privateProperties.font.material;
			}
			RefreshMesh(true);
		}
	}

	public int GetVertexCount()
	{
		if (vertices != null)
		{
			return vertices.Length;
		}
		return 0;
	}

	private void LateUpdate()
	{
		if (isDirty)
		{
			isDirty = false;
			RefreshMesh(true);
		}
	}

	private void SetColor(Color _topColor, Color _bottomColor)
	{
		if (colors == null || textMesh == null)
		{
			return;
		}
		int initialVertexToColorize = GetInitialVertexToColorize(TEXT_COMPONENT.Main);
		int num = 0;
		for (int i = initialVertexToColorize; i < GetFinalVertexToColorize(TEXT_COMPONENT.Main); i++)
		{
			if (num == 0 || num == 1)
			{
				colors[i] = _bottomColor;
			}
			else
			{
				colors[i] = _topColor;
			}
			num++;
			if (num > 3)
			{
				num = 0;
			}
		}
		textMesh.colors = colors;
	}

	public void SetColor(Color _color)
	{
		if (colors != null && !(textMesh == null))
		{
			int initialVertexToColorize = GetInitialVertexToColorize(TEXT_COMPONENT.Main);
			for (int i = initialVertexToColorize; i < GetFinalVertexToColorize(TEXT_COMPONENT.Main); i++)
			{
				colors[i] = _color;
			}
			textMesh.colors = colors;
		}
	}

	private void SetShadowColor(Color _color)
	{
		if (colors != null && !(textMesh == null))
		{
			int initialVertexToColorize = GetInitialVertexToColorize(TEXT_COMPONENT.Shadow);
			for (int i = initialVertexToColorize; i < GetFinalVertexToColorize(TEXT_COMPONENT.Shadow); i++)
			{
				colors[i] = _color;
			}
			textMesh.colors = colors;
		}
	}

	private void SetOutlineColor(Color _color)
	{
		if (colors != null && !(textMesh == null))
		{
			int initialVertexToColorize = GetInitialVertexToColorize(TEXT_COMPONENT.Outline);
			for (int i = initialVertexToColorize; i < GetFinalVertexToColorize(TEXT_COMPONENT.Shadow); i++)
			{
				colors[i] = _color;
			}
			textMesh.colors = colors;
		}
	}

	private int GetInitialVertexToColorize(TEXT_COMPONENT _textComponent)
	{
		if (textChars == null)
		{
			textChars = _privateProperties.text.ToCharArray();
		}
		int num = 0;
		switch (_textComponent)
		{
		case TEXT_COMPONENT.Main:
			if (_privateProperties.enableShadow && _privateProperties.enableOutline)
			{
				num = 5;
			}
			else if (_privateProperties.enableOutline)
			{
				num = 4;
			}
			else if (_privateProperties.enableShadow)
			{
				num = 1;
			}
			break;
		case TEXT_COMPONENT.Shadow:
			num = 0;
			break;
		case TEXT_COMPONENT.Outline:
			num = 1;
			break;
		}
		return textChars.Length * 4 * num;
	}

	private int GetFinalVertexToColorize(TEXT_COMPONENT _textComponent)
	{
		if (textChars == null)
		{
			textChars = _privateProperties.text.ToCharArray();
		}
		int result = 0;
		int num = 0;
		switch (_textComponent)
		{
		case TEXT_COMPONENT.Main:
			if (_privateProperties.enableShadow && _privateProperties.enableOutline)
			{
				num = 6;
			}
			else if (_privateProperties.enableOutline)
			{
				num = 5;
			}
			else if (_privateProperties.enableShadow)
			{
				num = 2;
			}
			result = textChars.Length * 4 * num;
			break;
		case TEXT_COMPONENT.Shadow:
			result = textChars.Length * 4;
			break;
		case TEXT_COMPONENT.Outline:
			num = ((!_privateProperties.enableShadow) ? 1 : 2);
			result = textChars.Length * 4 * (num + 4);
			break;
		}
		return result;
	}

	private void ChangeFont()
	{
		if (!dontOverrideMaterials && _privateProperties.customFillMaterial == null)
		{
			textRenderer.sharedMaterial = _privateProperties.font.material;
		}
		isDirty = true;
	}
}
