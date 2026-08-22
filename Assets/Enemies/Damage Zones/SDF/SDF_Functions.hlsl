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

float ConeDistanceToSegment(float2 toCheck, float2 start, float2 end)
{
    float2 segment = end - start;
    float segmentLengthSquared = dot(segment, segment);
    float t = segmentLengthSquared <= 0.0000001 ? 0.0 : saturate(dot( toCheck - start, segment) / segmentLengthSquared);
    return length( toCheck - (start + segment * t));
}

void ConeSDF_float(float2 uv, float radius, float halfAngle, out float distance)
{
    float mpi = 3.14159265359;
    float EPSILON = 0.00001;

    radius = max(radius, 0.0);
    halfAngle = clamp(halfAngle, 0.0, mpi);

    float pointLength = length(uv);
    if (halfAngle >= mpi - EPSILON)
    {
        distance = pointLength - radius;
        return;
    }

    float sine = sin(halfAngle);
    float cosine = cos(halfAngle);
    float2 leftEdge = float2(-sine, cosine) * radius;
    float2 rightEdge = float2(sine, cosine) * radius;

    float boundaryDistance = min(
        ConeDistanceToSegment(uv, float2(0.0, 0.0), leftEdge),
        ConeDistanceToSegment(uv, float2(0.0, 0.0), rightEdge));

    float signedAngle = atan2(uv.x, uv.y);
    if (pointLength > EPSILON && abs(signedAngle) <= halfAngle)
        boundaryDistance = min(boundaryDistance, abs(pointLength - radius));

    bool inside = pointLength <= radius && (pointLength <= EPSILON || abs(signedAngle) <= halfAngle);
    distance = inside ? -boundaryDistance : boundaryDistance;
}

// Returns a binary mask for the portion of a cone filled from its apex toward
// its outer arc. The cone forward axis is local +Y, like ConeSDF_float.
//
// Shader Graph inputs:
//   uv         : the same centred UV fed to ConeSDF_float
//   radius     : _Radius
//   halfAngle  : _HalfAngle, in radians
//   fillAmount : [0, 1], where 0 is empty and 1 is fully filled
void ConeRadialFill_float(float2 uv, float radius, float halfAngle, float fillAmount, out float filled)
{
    float EPSILON = 0.00001;

    radius = max(radius, 0.0);
    halfAngle = clamp(halfAngle, 0.0, PI);
    fillAmount = saturate(fillAmount);

    float radialDistance = length(uv);
    float radialMask = step(radialDistance, radius * fillAmount);

    // A 360-degree cone is a disk: no angular clipping is needed.
    float signedAngle = atan2(uv.x, uv.y);
    float angularMask = halfAngle >= PI - EPSILON
        ? 1.0
        : step(abs(signedAngle), halfAngle);

    // Keep an empty cone truly empty, including at its apex.
    filled = step(EPSILON, fillAmount) * radialMask * angularMask;
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
