Shader "Hidden/ASCII"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTexFull ("Texture (Original Size)", 2D) = "white" {}
		// Only 10 chars for the different levels of brightness
		_BitmapFont ("Bitmap Font", 2D) = "white" {}
		_Overlay ("Overlay", 2D) = "black" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			uniform sampler2D _MainTex;
			uniform sampler2D _MainTexFull;
			uniform sampler2D _BitmapFont;
			uniform sampler2D _Overlay;

			uniform int colorMode;
			uniform int columns, rows;
			uniform fixed3 background, foreground;
			uniform float mix = 0;
			uniform bool overlayOnly;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 colOverlay = tex2D(_Overlay, i.uv);
				if(overlayOnly){
					fixed4 col = tex2D(_MainTexFull, i.uv);
					return lerp(col, colOverlay, colOverlay.a);
				}				

				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col2 = col;

				fixed avg = floor((col.r + col.g + col.b) / 3 * 10) / 10;
				int char = avg * 10 - 0.1;
				
				if(colorMode == 1){
					col.rgb = floor(col.rgb * 10) / 10;
				}
				else if(colorMode == 2){
					col.rgb = 1;
				}

				int column = i.uv.x * columns;
				float startX = (i.uv.x * columns - column) / 10;
				fixed4 colChar = tex2D(_BitmapFont, float2(startX + float(char) / 10, i.uv.y * rows));

				if(colChar.r != 1 && colChar.g != 1 && colChar.b != 1){
					col.rgb = background;
				}
				else if(colorMode == 2){
					col.rgb = foreground;
				}

				col = lerp(col, col2, mix);	// ASCII vs. original
				col = lerp(col, colOverlay, colOverlay.a); // Text overlay

				return col;
			}
			ENDCG
		}
	}
}
