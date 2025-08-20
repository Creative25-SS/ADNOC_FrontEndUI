Shader "Custom/AudioWaveformCircle"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0.5, 0.3, 1.0, 1.0)
        _GlowColor1 ("Glow Color 1", Color) = (1.0, 1.0, 1.0, 1.0)
        _GlowColor2 ("Glow Color 2", Color) = (0.5, 0.8, 1.0, 1.0)
        _Thickness ("Line Thickness", Range(0.001, 0.1)) = 0.02
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 2.0
        _SecondGlowOffset ("Second Glow Offset", Range(0.0, 0.1)) = 0.03
        _AnimationSpeed ("Animation Speed", Range(0.1, 10.0)) = 2.0
        _CurrentAmplitude ("Current Amplitude", Range(0.0, 1.0)) = 0.5
        _BaseRadius ("Base Radius", Range(0.1, 0.5)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
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
            
            float4 _MainColor;
            float4 _GlowColor1;
            float4 _GlowColor2;
            float _Thickness;
            float _GlowIntensity;
            float _SecondGlowOffset;
            float _AnimationSpeed;
            float _CurrentAmplitude;
            float _BaseRadius;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float getRadius(float angle, float time, float amplitude)
            {
                float radius = _BaseRadius;
                
                // Wave animation scaled by amplitude
                radius += sin(angle * 3.0 + time) * 0.02 * amplitude;
                radius += sin(angle * 5.0 - time * 1.3) * 0.015 * amplitude;
                radius += sin(angle * 7.0 + time * 0.7) * 0.01 * amplitude;
                radius += sin(angle * 4.0 + time * _AnimationSpeed) * 0.05 * amplitude;
                
                return radius;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = i.uv - 0.5;
                float angle = atan2(center.y, center.x);
                float dist = length(center);
                float time = _Time.y;
                
                // Get target radius at this angle
                float targetRadius = getRadius(angle, time, _CurrentAmplitude);
                
                // Main line
                float lineDist = abs(dist - targetRadius);
                float lineVar = 1.0 - smoothstep(0.0, _Thickness, lineDist);
                
                // First glow
                float glow1 = exp(-lineDist * 10.0) * _GlowIntensity;
                
                // Second glow line
                float secondRadius = getRadius(angle, time * 0.8, _CurrentAmplitude);
                float secondLineDist = abs(dist - (secondRadius + _SecondGlowOffset));
                float line2 = 1.0 - smoothstep(0.0, _Thickness * 0.8, secondLineDist);
                float glow2 = exp(-secondLineDist * 15.0) * _GlowIntensity * 0.5;
                
                // Combine colors
                float3 color = _MainColor.rgb * lineVar;
                color += _GlowColor1.rgb * glow1 * 0.5;
                color += _GlowColor2.rgb * (line2 + glow2);
                
                float alpha = saturate(lineVar + glow1 + line2 + glow2) * _MainColor.a;
                
                return float4(color, alpha);
            }
            ENDCG
        }
    }
}