Shader "Kaleidos/Moon 2D URP" {
    Properties {
        _Zoom ("Zoom", Float) = 1
        _Pan ("Pan", Vector) = (0,0,0,0)
        _Rotation ("Rotation", Float) = 0
        _Tint ("Tint", Color) = (1,1,1,1)
        _Speed ("Speed", Float) = 1
        _CellSize ("Pixel Size", Float) = 3.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _Zoom;
            float4 _Pan;
            float _Rotation;
            float4 _Tint;
            float _Speed;
            float _CellSize;

            #define uTime (_Time.y * _Speed)
            #define uResolution float2(640.0, 360.0)
            #define cellSize _CellSize

float glslMod(float a, float b){ return a - b * floor(a / b); }

#define time uTime
static const float noiseScale = 8.0;
#define screenSize float2(640.0, 360.0)
#define sampleOffset (float2)(0.0)
#define renderSize float2(640.0, 360.0)

static const float octaves = 4.0;
static const float gain = 1.05;

#define morphPhase (uTime*0.08)
static const float morphDesync = 0.0;
#define scrollSpeed float2(0.08, 0.08)
static const float loopOn = 0.0;
static const float2 loopD = float2(0.0, 0.0);
static const float loopAxis = 0.0;
static const float loopZ = 0.0;
static const float tileOn = 0.0;
static const float2 tileP = float2(0.0, 0.0);
static const float2 tileScale = float2(0.0, 0.0);
static const float warpStrength = 0.45;
static const float rippleFreq = 34.0;
static const float pinchStrength = 0.0;
#define warpPhase (uTime*0.15)

static const float4 PALETTE[110] = {
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.3125,0.130005,0.0878906,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.90625,0.273468,0.127441,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.953125,0.614319,0.178711,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(0.827451,0.74902,0.121569,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(1.0,1.0,1.0,1.0),
  float4(0.990826,0.990826,0.990826,1.0),
  float4(1.0,1.0,1.0,1.0)
};
float4 paletteLookup(float x){
  int i = clamp(int(clamp(x,0.0,1.0)*256.0),0,255);
  return PALETTE[clamp(int(float(i)/255.0*110.0),0,109)];
}

#define TAU 6.28318530718
#define MAX_OCTAVES 10
#define MORPH_DESYNC_AMP   1.6
#define MORPH_DESYNC_SCALE 2.5

float2 frameC(float2 uv) {
    float2 r = renderSize;
    return (uv - 0.5) * screenSize / r.y;
}
float2 frameUV(float2 c) {
    float2 r = renderSize;
    return 0.5 + c * r.y / screenSize;
}

float hash3(float3 p) {
    p = frac(p * float3(127.1, 311.7, 74.7));
    p += dot(p, p.yxz + 19.19);
    return frac((p.x + p.y) * p.z);
}

float vnoise3(float3 p) {
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * (3.0 - 2.0 * f);
    return lerp(
        lerp(lerp(hash3(i + float3(0,0,0)), hash3(i + float3(1,0,0)), u.x),
            lerp(hash3(i + float3(0,1,0)), hash3(i + float3(1,1,0)), u.x), u.y),
        lerp(lerp(hash3(i + float3(0,0,1)), hash3(i + float3(1,0,1)), u.x),
            lerp(hash3(i + float3(0,1,1)), hash3(i + float3(1,1,1)), u.x), u.y),
        u.z
    );
}

float3 wrapLat(float3 i, float2 d, float pz) {
    if (pz > 0.5) i.z = glslMod(i.z, pz);
    if (tileOn > 0.5) {
        i.xy = glslMod(i.xy, d);
        return i;
    }
    if (d.x != 0.0 || d.y != 0.0) {
        float n = (loopAxis < 0.5) ? floor(i.x / d.x) : floor(i.y / d.y);
        i.xy -= n * d;
    }
    return i;
}

float vnoise3L(float3 p, float2 d, float pz) {
    float3 i = floor(p);
    float3 f = frac(p);
    float3 u = f * f * (3.0 - 2.0 * f);
    return lerp(
        lerp(lerp(hash3(wrapLat(i + float3(0,0,0), d, pz)), hash3(wrapLat(i + float3(1,0,0), d, pz)), u.x),
            lerp(hash3(wrapLat(i + float3(0,1,0), d, pz)), hash3(wrapLat(i + float3(1,1,0), d, pz)), u.x), u.y),
        lerp(lerp(hash3(wrapLat(i + float3(0,0,1), d, pz)), hash3(wrapLat(i + float3(1,0,1), d, pz)), u.x),
            lerp(hash3(wrapLat(i + float3(0,1,1), d, pz)), hash3(wrapLat(i + float3(1,1,1), d, pz)), u.x), u.y),
        u.z
    );
}

float basisSample(float3 p, float2 d, float pz) {
    if (loopOn > 0.5 || tileOn > 0.5) {
        
        return vnoise3L(p, d, pz) * 2.0 - 1.0;
    }
    
    return vnoise3(p) * 2.0 - 1.0;
}

float fbm3(float3 p) {
    float v = 0.0, a = 0.5, norm = 0.0;
    float2  d  = (tileOn > 0.5) ? tileP : loopD;
    float pz = loopZ;
    int oct = int(octaves + 0.5);
    for (int i = 0; i < MAX_OCTAVES; i++) {
        if (i >= oct) break;
        float sn = basisSample(p, d, pz);
        float t;
        {
            t = sn * 0.5 + 0.5;
        }
        v    += a * t;
        norm += a;
        p     = p * 2.0 + float3(5.3, 1.7, 3.1);
        d    *= 2.0;
        pz   *= 2.0;
        a    *= gain;
    }
    return v / norm;
}

#define RIPPLE_AMP  0.05
#define PINCH_R     0.8
#define PINCH_K     0.9
#define PINCH_PULSE 0.5

float whash2(float2 p) {
    p = frac(p * float2(127.1, 311.7));
    p += dot(p, p.yx + 19.19);
    return frac((p.x + p.y) * 43.32);
}
float wnoise2(float2 p) {
    float2 i = floor(p), f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(whash2(i + float2(0.0, 0.0)), whash2(i + float2(1.0, 0.0)), u.x),
               lerp(whash2(i + float2(0.0, 1.0)), whash2(i + float2(1.0, 1.0)), u.x), u.y);
}

float2 warpUV(float2 uv) {
    
    {
        float2  c   = frameC(uv);
        float r   = length(c);
        float2  dir = r > 1e-4 ? c / r : (float2)(0.0);
        c += dir * warpStrength * RIPPLE_AMP * sin(r * rippleFreq - warpPhase * TAU);
        return frameUV(c);
    }
    float2  c   = frameC(uv);
    float r   = length(c);
    if (r > 1e-5) {
        float k  = pinchStrength * (1.0 + PINCH_PULSE * sin(warpPhase * TAU));
        float rn = min(r / PINCH_R, 1.0);
        float factor;
        if (k >= 0.0) {
            factor = pow(rn, k * PINCH_K);
        } else {
            factor = 1.0 + (-k) * PINCH_K * (1.0 - rn * rn); 
        }
        c *= factor;
    }
    return frameUV(c);
}

float2 rotateUV(float2 uv) {
    return uv;
}

float fieldN(float2 uv, float mzExtra) {
    float mz = morphPhase + mzExtra;

    if (morphDesync > 0.5 && tileOn < 0.5) mz += MORPH_DESYNC_AMP * (wnoise2(uv * MORPH_DESYNC_SCALE) - 0.5);

    float2 nsv = (tileOn > 0.5) ? tileScale : (float2)(noiseScale);
    return fbm3(float3(uv * nsv + time * scrollSpeed, mz));
}

float4 shade(float2 screen_coords) {
    float2 block = floor((screen_coords + sampleOffset) / cellSize) * cellSize;
    float2 uv0   = warpUV(block / screenSize);
    float mzAdd = 0.0, valAdd = 0.0;
    float2 uvDisp = (float2)(0.0);

    float n = fieldN(rotateUV(uv0 + uvDisp), mzAdd);
    n = clamp(n + valAdd, 0.0, 1.0);
    return paletteLookup(n);
}


            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings vert(Attributes IN){
                Varyings o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = IN.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target {
                float2 px = 1.0 / fwidth(i.uv);
                float ar = px.x / px.y;
                float2 uv = i.uv - 0.5;
                if (ar > 1.77778) uv.x *= ar / 1.77778; else uv.y *= 1.77778 / ar;
                float s = sin(_Rotation), cs = cos(_Rotation);
                uv = mul(uv, float2x2(cs, -s, s, cs));
                uv = uv / _Zoom + _Pan.xy;
                float4 c = shade((uv + 0.5) * float2(640.0, 360.0));
                return float4(c.rgb * c.a, 1.0) * _Tint;
            }
            ENDHLSL
        }
    }
}