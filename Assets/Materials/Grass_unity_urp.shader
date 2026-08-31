Shader "Kaleidos/Grass 2D URP" {
    Properties {
        _Zoom ("Zoom", Float) = 1
        _Pan ("Pan", Vector) = (0,0,0,0)
        _Rotation ("Rotation", Float) = 0
        _Tint ("Tint", Color) = (1,1,1,1)
        _Speed ("Speed", Float) = 1
        _CellSize ("Pixel Size", Float) = 7.0
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

#define time uTime
static const float noiseScale = 8.0;
#define screenSize float2(640.0, 360.0)
#define sampleOffset (float2)(0.0)

static const float octaves = 6.0;
static const float gain = 0.4;

#define morphPhase (uTime*0.0)
#define scrollSpeed float2(0.05, 0.05)
static const float2 loopD = float2(0.0, 0.0);
static const float loopZ = 0.0;

static const float4 PALETTE[110] = {
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.211246,0.234375,0.179443,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.225432,0.265625,0.170166,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.296053,0.375,0.1875,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.324733,0.421875,0.191162,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0),
  float4(0.366468,0.484375,0.204346,1.0)
};
float4 paletteLookup(float x){
  int i = clamp(int(clamp(x,0.0,1.0)*256.0),0,255);
  return PALETTE[clamp(int(float(i)/255.0*110.0),0,109)];
}

#define MAX_OCTAVES 10

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

float basisSample(float3 p, float2 d, float pz) {

    return vnoise3(p) * 2.0 - 1.0;
}

float fbm3(float3 p) {
    float v = 0.0, a = 0.5, norm = 0.0;
    float2  d  = loopD;
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

float2 warpUV(float2 uv) {
    return uv;
}

float2 rotateUV(float2 uv) {
    return uv;
}

float fieldN(float2 uv, float mzExtra) {
    float mz = morphPhase + mzExtra;

    float2 nsv = (float2)(noiseScale);
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