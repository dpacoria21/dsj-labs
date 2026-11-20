Shader "Lab08/Fish"
{
    Properties
    {
        _MainTex ("Color", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.25
        _SwimAmplitude ("Swim amplitude", Float) = 0.05
        _SwimSpeed ("Swim speed", Float) = 7.9
        _SwimFrequency ("Wave frequency", Float) = 2.27
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard vertex:Swim addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _BaseColor;
        half _Glossiness;
        float _SwimAmplitude, _SwimSpeed, _SwimFrequency;

        struct Input
        {
            float2 uv_MainTex;
        };

        void Swim(inout appdata_full v)
        {
            float phase = _Time.y * _SwimSpeed + v.vertex.z * _SwimFrequency;
            v.vertex.x += sin(phase) * _SwimAmplitude * saturate(abs(v.vertex.z));
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            output.Albedo = tex2D(_MainTex, input.uv_MainTex).rgb * _BaseColor.rgb;
            output.Smoothness = _Glossiness;
            output.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
