Shader "Lines/WireFrame" {

	Properties { _Color ("Main Color", Color) = (1,1,1,0.5) } 
		
	SubShader {

		Offset -1,-1

		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
		
		Pass { 
		
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			Lighting Off
			Fog { Mode Off }
			Color[_Color]
			
		}
		
	}
	
}