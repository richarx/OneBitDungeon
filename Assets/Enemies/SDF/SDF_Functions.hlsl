/*
If the shapes had different colors I think you'd have to do some Lerp between them depending on the distances.
But in this tutorial both shapes are same color, so the simple way would be to just Add relevant shapes together first 
(main with main, outline with outline and inline with inline)
Saturate the results and only after that apply colors
*/

void CircleSDF_float(float2 uv, float radius, out float distance) 
{
    distance = length(uv) - radius;
}

void RectangleSDF_float(float2 uv, float2 size, out float distance) 
{
    float2 d = abs(uv) - size;
    distance = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

void RoundedRectangleSDF_float(float2 uv, float2 size, float4 cornerRounding, float allCornersRounding, out float distance) 
{
    float2 centered = uv;
    float2 q = abs(centered);

    float top_mask = step(0.0, centered.y);
    float right_mask = step(0.0, centered.x);

    float left_side_rounding = lerp(cornerRounding.x, cornerRounding.y, top_mask);
    float right_side_rounding = lerp(cornerRounding.w, cornerRounding.z, top_mask);

    float r_individual = lerp(left_side_rounding, right_side_rounding, right_mask);

    float r = r_individual + allCornersRounding;

    float2 d = q - size + r;
    distance = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
}

void OutlineSDF_float(float distance, float thickness, out float outlineDistance) 
{
    outlineDistance = abs(distance) - thickness;
}

void InlineSDF_float(float distance, float thickness, out float inlineDistance) 
{
    inlineDistance = max(distance, -distance - thickness);
}

void SmoothUnion_float(float a, float b, float k, out float result)
{
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    result = lerp(b, a, h) - k * h * (1.0 - h);
}

void SmoothIntersection_float(float a, float b, float k, out float result)
{
    float h = clamp(0.5 + 0.5 * (a - b) / k, 0.0, 1.0);
    result = lerp(b, a, h) + k * h * (1.0 - h);
}

void SmoothDifference_float(float a, float b, float k, out float result)
{
    float h = clamp(0.5 + 0.5 * (a - (-b)) / k, 0.0, 1.0);
    result = lerp(-b, a, h) + k * h * (1.0 - h);
}