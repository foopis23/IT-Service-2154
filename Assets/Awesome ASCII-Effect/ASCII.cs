using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ASCII : MonoBehaviour
{
	public static ASCII Instance { private set; get; }
	public enum ColorMode { ORIGINAL, DECIMATED, ONE_BIT, OVERLAY_ONLY }

	public bool useLegacyRendering = false;

	private Material mat;
	private int charWidth, charHeight;
	private char[][] mappedChars;
	private Texture2D charsGradient;

	private bool isOverlayDirty = false;
	private Vector2Int dirtyPos1;
	private Vector2Int dirtyPos2;
	private OverlayChar[][] overlay;
	private Texture2D overlayTexture;
	private RenderTexture overlayRT;
	
	public int largeScreenHeight;
	public int mediumScreenHeight;
	public int smallScreenHeight;
	public int extraSmallScreenHeight;

	public bool pixelate = true;
	[Range(0, 1)]
	public float tranparency = 0;
	public Color background = Color.black;
	[Tooltip("Only in ONE_BIT mode!")]
	public Color foreground = Color.white;
	[Tooltip("Must be 10 chars or will be truncated/padded.\n(Sort by brightness ascending!)")]
	public string lookupChars;
	public uint columns = 120, rows = 120;
	public ColorMode colorMode = ColorMode.ORIGINAL;
	[Tooltip("Must be white on black baground")]
	public Texture2D bitmapFont;
	[Tooltip("Must be an exact representation of the chars in the bitmap font texture (including blank lines).")]
	[TextArea]
	public string characterMapping;

	private void OnEnable()
	{
		Instance = this;
		Setup();
	}

	private void Setup()
	{
		if (Screen.height >= 1080)
		{
			rows = (uint) largeScreenHeight;
			columns = (uint) (largeScreenHeight * Screen.width / Screen.height);
		}
		else if (Screen.height >= 720)
		{
			rows = (uint) mediumScreenHeight;
			columns = (uint) (mediumScreenHeight * Screen.width / Screen.height);
		}
		else if (Screen.height >= 600)
		{
			rows = (uint) smallScreenHeight;
			columns = (uint) (smallScreenHeight * Screen.width / Screen.height);
		}
		else
		{
			rows = (uint) extraSmallScreenHeight;
			columns = (uint) (extraSmallScreenHeight * Screen.width / Screen.height);
		}
		
		mat = new Material(Shader.Find("Hidden/ASCII"));
		dirtyPos1 = new Vector2Int();
		dirtyPos2 = new Vector2Int();

		if (lookupChars == null || lookupChars.Length == 0)
		{
			Debug.LogError("No lookup chars provided");
			enabled = false;
			return;
		}

		while (lookupChars.Length < 10)
		{
			lookupChars += lookupChars.ToCharArray()[lookupChars.Length - 1];
		}

		computeCharacterMapping();

		charsGradient = new Texture2D(charWidth * 10, charHeight);
		charsGradient.filterMode = FilterMode.Point;

		for (int i = 0; i < 10; i++)
		{
			char c = lookupChars.ToCharArray()[i];
			Vector2 charPosition = lookupCharPosition(c);
			int startX = (int)charPosition.x * charWidth;
			int startY = (int)charPosition.y * charHeight;

			for (int x = 0; x < charWidth; x++)
			{
				for (int y = 0; y < charHeight; y++)
				{
					Color col = bitmapFont.GetPixel(startX + x, bitmapFont.height - (startY + y));
					charsGradient.SetPixel(i * charWidth + x, charHeight - y, col);
				}
			}
		}
		charsGradient.Apply();
		mat.SetTexture("_BitmapFont", charsGradient);
		ClearOverlay();
		RenderOverlay();
	}

	private void Start()
	{
	}

	private struct OverlayChar
	{
		public char c;
		public Color foreground, background;
		public OverlayChar(char c, Color foreground)
		{
			this.c = c;
			this.foreground = foreground;
			background = ASCII.Instance.background;
		}
		public OverlayChar(char c, Color foreground, Color background)
		{
			this.c = c;
			this.foreground = foreground;
			this.background = background;
		}
	}
	private OverlayChar Whitespace = new OverlayChar(' ', Color.white, new Color(0, 0, 0, 0));

	public void ClearOverlay()
	{
		overlay = new OverlayChar[rows][];
		for (int row = 0; row < rows; row++)
		{
			overlay[row] = new OverlayChar[columns];
			for (int column = 0; column < columns; column++)
			{
				overlay[row][column] = Whitespace;
			}
		}
		setRegionDirty(0, 0, (int)columns, (int)rows);
	}

	public void PutChar(char c, Color foreground, int x, int y)
	{
		PutChar(c, foreground, background, x, y);
	}

	public void PutChar(char c, Color foreground, Color background, int x, int y)
	{
		overlay[y][x] = new OverlayChar(c, foreground, background);
		setRegionDirty(x, y, 1, 1);
	}

	public void PutString(string s, Color foreground, int x, int y)
	{
		PutString(s, foreground, background, x, y);
	}

	public void PutString(string s, Color foreground, Color background, int x, int y)
	{
		char[] chars = s.ToCharArray();
		for (int i = 0; x + i < columns && i < chars.Length; i++)
		{
			PutChar(chars[i], foreground, background, x + i, y);
		}
	}

	public void ClearArea(int x, int y, int width, int height)
	{
		for (int i = 0; i + y < rows && i < height; i++)
		{
			for (int j = 0; j + x < columns && j < width; j++)
			{
				overlay[y + i][x + j] = Whitespace;
			}
		}
		setRegionDirty(x, y, width, height);
	}

	public void DrawBox(char border, Color foreground, Color background, int x, int y, int width, int height)
	{
		OverlayChar cBorder = new OverlayChar(border, foreground, background);
		OverlayChar cClear = new OverlayChar(' ', foreground, background);

		if (x + width > columns || y + height > rows)
		{
			throw new UnityException("Box does not fit screen!");
		}

		for (int row = 0; row < height; row++)
		{
			for (int column = 0; column < width; column++)
			{
				overlay[y + row][x + column] = column == 0 || row == 0 || column == (width - 1) || row == (height - 1) ? cBorder : cClear;
			}
		}
		setRegionDirty(x, y, width, height);
	}

	private void LegacyRenderRegion(int x, int y, int width, int height)
	{
		var regionX = charWidth * x;
		var regionY = charHeight * y;
		var regionWidth = charWidth * width;
		var regionHeight = charHeight * height;

		// clear current region 
		for (var px=0; px < regionWidth; px++) {
			for (var py=0; py < regionHeight; py++) {
				overlayTexture.SetPixel(regionX + px, regionY + py, new Color(0, 0, 0, 0));
			}
		}

		// loop through region array and generate region texture
		for (var row = 0; row < height; row++)
		{
			for (var column = 0; column < width; column++)
			{
				// figure out what char we are trying to draw
				Vector2 charPosition = lookupCharPosition(overlay[y + row][x + column].c);

				// loop through screen pixels and draw the appropriate pixel from the bitmap font texture
				for (var charX = 0; charX < charWidth; charX++)
				{
					for (var charY = 0; charY < charHeight; charY++)
					{
						var bitmapFontColor = bitmapFont.GetPixel((int)charPosition.x * charWidth + charX, bitmapFont.height - ((int)charPosition.y * charHeight + charY));
						var isCharPixel = bitmapFontColor.r > 0.5f;
						Color pixelColor;
						pixelColor = isCharPixel ? overlay[row + y][column + x].foreground : overlay[row + y][column + x].background;
						overlayTexture.SetPixel(regionX + column * charWidth + charX, (overlayTexture.height - regionY - regionHeight) +
							(regionHeight - (row * charHeight + charY)), pixelColor);
					}
				}
			}
		}

		//update the region texture
		overlayTexture.Apply();

		//copy the region to the overlay and set the overlay in the shader
		mat.SetTexture("_Overlay", overlayTexture);
	}

	private void RenderRegion(int x, int y, int width, int height)
	{
		//create new texture that is size of screen region
		Texture2D regionTexture = new Texture2D((int)(charWidth * width), (int)(charHeight * height));
		regionTexture.filterMode = FilterMode.Point;
		regionTexture.wrapModeV = TextureWrapMode.Clamp;

		// make texture completely transparent
		for (int px=0; px < regionTexture.width; px++) {
			for (int py=0; py < regionTexture.height; py++) {
				regionTexture.SetPixel(px, py, new Color(0, 0, 0, 0));
			}
		}

		// loop through region array and generate region texture
		for (int row = 0; row < height; row++)
		{
			for (int column = 0; column < width; column++)
			{
				// figure out what char we are trying to draw
				Vector2 charPosition = lookupCharPosition(overlay[y + row][x + column].c);

				// loop through screen pixels and draw the appropriate pixel from the bitmap font texture
				for (int charX = 0; charX < charWidth; charX++)
				{
					for (int charY = 0; charY < charHeight; charY++)
					{
						Color bitmapFontColor = bitmapFont.GetPixel((int)charPosition.x * charWidth + charX, bitmapFont.height - ((int)charPosition.y * charHeight + charY));
						bool isCharPixel = bitmapFontColor.r > 0.5f;
						Color pixelColor;
						if (isCharPixel)
						{
							pixelColor = overlay[row + y][column + x].foreground;
						}
						else
						{
							pixelColor = overlay[row + y][column + x].background;
						}
						regionTexture.SetPixel(column * charWidth + charX, regionTexture.height - (row * charHeight + charY), pixelColor);
					}
				}
			}
		}

		//update the region texture
		regionTexture.Apply();

		//copy the region to the overlay and set the overlay in the shader
		Graphics.CopyTexture(regionTexture, 0, 0, 0, 0, regionTexture.width, regionTexture.height, overlayTexture, 0, 0, (x * charWidth), (overlayTexture.height - y * charHeight) - regionTexture.height);
		mat.SetTexture("_Overlay", overlayTexture);
	}

	private void RenderOverlay()
	{
		overlayTexture = new Texture2D((int)(charWidth * columns), (int)(charHeight * rows));
		overlayTexture.filterMode = FilterMode.Point;
		overlayTexture.wrapModeV = TextureWrapMode.Clamp;

		for (int row = 0; row < rows; row++)
		{
			for (int column = 0; column < columns; column++)
			{
				Vector2 charPosition = lookupCharPosition(overlay[row][column].c);

				for (int x = 0; x < charWidth; x++)
				{
					for (int y = 0; y < charHeight; y++)
					{
						Color bitmapFontColor = bitmapFont.GetPixel((int)charPosition.x * charWidth + x, bitmapFont.height - ((int)charPosition.y * charHeight + y));
						bool isCharPixel = bitmapFontColor.r > 0.5f;
						Color pixelColor;
						if (isCharPixel)
						{
							pixelColor = overlay[row][column].foreground;
						}
						else
						{
							pixelColor = overlay[row][column].background;
						}
						overlayTexture.SetPixel(column * charWidth + x, overlayTexture.height - (row * charHeight + y), pixelColor);
					}
				}
			}
		}

		overlayTexture.Apply();
		mat.SetTexture("_Overlay", overlayTexture);
	}

	private Vector2 lookupCharPosition(char c)
	{
		for (int row = 0; row < mappedChars.Length; row++)
		{
			for (int column = 0; column < mappedChars[row].Length; column++)
			{
				if (c == mappedChars[row][column])
				{
					return new Vector2(column, row);
				}
			}
		}
		throw new UnityException("Char \\" + (int)c + " not availiable in bitmapFont or not mapped!");
	}

	private void computeCharacterMapping()
	{
		string[] lines = characterMapping.Split('\n');
		mappedChars = new char[lines.Length][];
		for (int i = 0; i < lines.Length; i++)
		{
			mappedChars[i] = lines[i].ToCharArray();
		}
		charHeight = bitmapFont.height / mappedChars.Length;
		charWidth = bitmapFont.width / mappedChars[0].Length;
	}

	private void setRegionDirty(int x, int y, int width, int height)
	{
		if (isOverlayDirty)
		{
			if (x < dirtyPos1.x)
			{
				dirtyPos1.x = x;
			}

			if (y < dirtyPos1.y)
			{
				dirtyPos1.y = y;
			}

			if (x + width > dirtyPos2.x)
			{
				dirtyPos2.x = x + width;
			}

			if (y + height > dirtyPos2.y)
			{
				dirtyPos2.y = y + height;
			}
		}
		else
		{
			dirtyPos1.x = x;
			dirtyPos1.y = y;
			dirtyPos2.x = x + width;
			dirtyPos2.y = y + height;

		}

		isOverlayDirty = true;
	}

	private void Update()
	{
		mat.SetInt("colorMode", colorMode == ColorMode.ORIGINAL ? 0 : colorMode == ColorMode.DECIMATED ? 1 : 2);
		mat.SetInt("columns", (int)columns);
		mat.SetInt("rows", (int)rows);
		mat.SetColor("background", background);
		mat.SetColor("foreground", foreground);
		mat.SetFloat("mix", tranparency);
		mat.SetInt("overlayOnly", colorMode == ColorMode.OVERLAY_ONLY ? 1 : 0);
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (isOverlayDirty)
		{
			isOverlayDirty = false;

			if (SystemInfo.copyTextureSupport == CopyTextureSupport.None || useLegacyRendering)
			{
				LegacyRenderRegion(dirtyPos1.x, dirtyPos1.y, dirtyPos2.x - dirtyPos1.x, dirtyPos2.y - dirtyPos1.y);
			}
			else
			{
				RenderRegion(dirtyPos1.x, dirtyPos1.y, dirtyPos2.x - dirtyPos1.x, dirtyPos2.y - dirtyPos1.y);
			}
		}

		if (!pixelate)
		{
			if (colorMode == ColorMode.OVERLAY_ONLY)
			{
				mat.SetTexture("_MainTexFull", src);
			}
			Graphics.Blit(src, dest, mat);
		}
		else
		{
			RenderTexture ds = RenderTexture.GetTemporary((int)columns, (int)rows);
			ds.filterMode = FilterMode.Point;
			Graphics.Blit(src, ds);
			RenderTexture tmp = RenderTexture.GetTemporary(src.width, src.height);
			Graphics.Blit(ds, tmp);
			RenderTexture.ReleaseTemporary(ds);
			Graphics.Blit(tmp, dest, mat);
			if (colorMode == ColorMode.OVERLAY_ONLY)
			{
				mat.SetTexture("_MainTexFull", tmp);
			}
			RenderTexture.ReleaseTemporary(tmp);
		}
	}
}
