Shader "Lab08/Arena Ground"
{
    Properties
    {
        _Floor ("Floor", Color) = (0.38, 0.46, 0.45, 1)
        _Lines ("Grid", Color) = (0.31, 0.39, 0.39, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert
        #pragma target 3.0

        fixed4 _Floor, _Lines;

        struct Input
        {
            float3 worldPos;
        };

        void surf(Input input, inout SurfaceOutput output)
        {
            float2 cell = input.worldPos.xz * 0.5;
            float2 distanceToLine = abs(frac(cell - 0.5) - 0.5);
            float2 width = max(fwidth(cell), 0.0001);
            float2 blend = saturate(distanceToLine / width);
            output.Albedo = lerp(_Lines.rgb, _Floor.rgb, min(blend.x, blend.y));
            output.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
