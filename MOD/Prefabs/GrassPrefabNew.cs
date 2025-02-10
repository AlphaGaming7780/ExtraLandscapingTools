using Colossal.IO.AssetDatabase;
using Colossal;
using Game.Prefabs;
using System;
using UnityEngine;

namespace ExtraLandscapingTools.MOD.Prefabs
{
    [ComponentMenu("ELT/", new Type[]
    {

    })]
    public class GrassPrefabNew : GrassPrefabBase // Why ObjectPrefab ? Because otherwise Brush Render system is doing shit
    {
        public bool DebugDistantGrass = false;
        public bool DebugGrassLOD = false;
        public Texture Grass_BaseColorMap; // = ((TextureAsset)new AssetReference<TextureAsset>(new Colossal.Hash128("52155cd1d35ff71419438c9add4a463c"))).Load();
        public Vector4 Grass_ColorRandom1;
        public Vector4 Grass_ColorRandom2;
        public float Grass_Coverage;
        public bool Grass_Enabled;
        public Vector2 Grass_FlipBookSize;
        public float Grass_LOD0CullDistance;
        public float Grass_LOD0ParticleCount;
        public float Grass_LOD1CullDistance;
        public float Grass_LOD1ParticleCount;
        public float Grass_LOD2CullDistance;
        public float Grass_LOD2ParticleCount;
        public float Grass_NoiseScale;
        public Texture Grass_NormalMap;
        public float Grass_NormalScale;
        public Vector2 Grass_QuadSize;
        public float Grass_ScaleMultiplier;
        public float Grass_ScaleRandom;
        public int Grass_SplatIndex1;
        public int Grass_SplatIndex2;
        public int Grass_SplatIndex3;
        public int Grass_SplatIndex4;
        public int Grass_SplatIndex5;
        public int Grass_SplatIndex6;
        public int Grass_SplatIndex7;
        public int Grass_SplatIndex8;
        public int Grass_SplatIndex9;
        public int Grass_SplatIndex10;
        public Vector2 Grass_YAngleRange;

        public float Heightmap_SamplingScale;
        public float HeightmapYScale;
        public bool IndexOverride;

        public bool Scatter_Enabled;
        public float Scatter_DensityMultiplier;
        public float Scatter_DistanceRangeMultiplier;
        public Vector4 Scatter_LODValues;
        public Mesh Scatter_Mesh1;
        public Mesh Scatter_Mesh2;
        public float Scatter_MeshScale;
        public float Scatter_MeshScaleMultiplier;
        public float Scatter_MeshScaleRandom;
        public float Scatter_MeshWeight;
        public int Scatter_ParticleCount;
        public int Scatter_Splatndex1;
        public int Scatter_Splatndex2;
        public int Scatter_Splatndex3;
        public int Scatter_Splatndex4;
        public int Scatter_Splatndex5;
        public int Scatter_Splatndex6;
        public int Scatter_Splatndex7;
        public int Scatter_Splatndex8;
        public int Scatter_Splatndex9;
        public int Scatter_Splatndex10;

        public float Splatmap_SamplingScale;
        public float Splatmap_WeightBlendStrength;

        public Vector3 VolumeScale;
    }
}
