using Colossal.Entities;
using Game.Rendering;
using Game.Simulation;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using UnityEngine;
using Colossal.IO.AssetDatabase;
using Game.Areas;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Tools;
using Unity.Entities;
using ExtraLandscapingTools.Prefabs;
using Unity.Collections;

using Unity.Mathematics;
using Game.Prefabs;
using Colossal.Mathematics;
using Unity.Jobs;
using System.Diagnostics.SymbolStore;
using Colossal.UI.Binding;
using Color = UnityEngine.Color;
using ExtraLib.Helpers;
using Colossal.PSI.Environment;
using System.IO;
using Unity.Burst;

namespace ExtraLandscapingTools.Systems
{
    public partial class VegetationRenderSystem : GameSystemBase
    {

        private TerrainSystem m_TerrainSystem;
        private TerrainMaterialSystem m_TerrainMaterialSystem;
        private CameraUpdateSystem m_CameraUpdateSystem;
        private PrefabSystem m_PrefabSystem;

        private static VisualEffectAsset s_FoliageVFXAsset;
        private VisualEffect m_FoliageVFX;

        private Texture2D m_UserPaintedGrassTexture;

        private EntityQuery m_BrushQuery;

        protected override void OnCreate()
        {
            m_CameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
            m_TerrainSystem = World.GetOrCreateSystemManaged<TerrainSystem>();
            m_TerrainMaterialSystem = World.GetOrCreateSystemManaged<TerrainMaterialSystem>();
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            VegetationRenderSystem.s_FoliageVFXAsset = Resources.Load<VisualEffectAsset>("Vegetation/FoliageVFX");

            m_UserPaintedGrassTexture = new Texture2D(TerrainSystem.kDefaultHeightmapWidth, TerrainSystem.kDefaultHeightmapHeight, TextureFormat.RG16, false, true)
            {
                name = "UserPaintedGrassTexture",
                hideFlags = HideFlags.HideAndDontSave,
            };

            Color[] pixels = Enumerable.Repeat(Color.black, m_UserPaintedGrassTexture.width * m_UserPaintedGrassTexture.height).ToArray();
            m_UserPaintedGrassTexture.SetPixels(pixels);
            m_UserPaintedGrassTexture.Apply();

            m_BrushQuery = GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<Brush>(),
                ComponentType.Exclude<Hidden>(),
                ComponentType.Exclude<Deleted>()
            });

        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            CoreUtils.Destroy(this.m_FoliageVFX);
        }

        protected override void OnUpdate()
        {
            if (this.m_CameraUpdateSystem.activeViewer != null)
            {
                
                NativeArray<Entity> entities = m_BrushQuery.ToEntityArray(Allocator.Temp);

                if(TryGetValideBrushEntity(entities, out Entity brushEntity, out Brush brush, out GrassData grassData) && grassData.m_State != Tools.GrassToolSystem.State.Default)
                {

                    if (!m_PrefabSystem.TryGetPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(brushEntity).m_Prefab, out PrefabBase prefab)) 
                    {
                        ELT.Logger.Warn("Isn't a prefab");
                        return;
                    }
                    
                    if( prefab is not BrushPrefab brushPrefab)
                    {
                        ELT.Logger.Warn("Isn't a brush prefab");
                        return;
                    }

                    if(m_UserPaintedGrassTexture == null)
                    {
                        ELT.Logger.Warn("m_UserPaintedGrassTexture is null");
                        return;
                    }

                    Texture2D texture = brushPrefab.m_Texture;
                    Bounds2 brushArea = ToolUtils.GetBounds(brush);
                    TerrainHeightData terrainHeightData = m_TerrainSystem.GetHeightData();
                    TextureStruct<ColorA8> brushTexture = GetTextureFromTexture2D<ColorA8>(texture);
                    TextureStruct<ColorRG8> userPaintedGrassTexture = GetTextureFromTexture2D<ColorRG8>(m_UserPaintedGrassTexture);

                    UpdateUserPaintedGrass updateUserPaintedGrass = new UpdateUserPaintedGrass()
                    {
                        brushTexture = brushTexture,
                        userPaintedGrassTexture = userPaintedGrassTexture,
                        grassData = grassData,
                        brushArea = brushArea,
                        terrainHeightData = terrainHeightData,
                        brush = brush,
                    };

                    JobHandle jobHandle = updateUserPaintedGrass.Schedule(Dependency);

                    jobHandle.Complete();
                    //m_UserPaintedGrassTexture.SetPixels(userPaintedGrassTexture.data.ToArray());
                    m_UserPaintedGrassTexture.Apply(false);
                    //TextureHelper.SaveTextureAsPNG(m_UserPaintedGrassTexture, Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(ExtraLandscapingTools), "TEST.png" ));
                    //brushTexture.Dispose();
                    //userPaintedGrassTexture.Dispose();


                }

                this.CreateDynamicVFXIfNeeded();
                this.UpdateEffect();
            }
        }

        private void UpdateEffect()
        {
            Bounds terrainBounds = this.m_TerrainSystem.GetTerrainBounds();
            this.m_FoliageVFX.SetVector3("TerrainBounds_center", terrainBounds.center);
            this.m_FoliageVFX.SetVector3("TerrainBounds_size", terrainBounds.size);
            this.m_FoliageVFX.SetTexture("Terrain HeightMap", this.m_TerrainSystem.heightmap);
            //this.m_FoliageVFX.SetTexture("Terrain SplatMap", this.m_TerrainMaterialSystem.splatmap);
            this.m_FoliageVFX.SetTexture("Terrain SplatMap", m_UserPaintedGrassTexture);
            Vector4 globalVector = Shader.GetGlobalVector("colossal_TerrainScale");
            Vector4 globalVector2 = Shader.GetGlobalVector("colossal_TerrainOffset");
            this.m_FoliageVFX.SetVector4("Terrain Offset Scale", new Vector4(globalVector.x, globalVector.z, globalVector2.x, globalVector2.z));
            this.m_FoliageVFX.SetVector3("CameraPosition", this.m_CameraUpdateSystem.position);
            this.m_FoliageVFX.SetVector3("CameraDirection", this.m_CameraUpdateSystem.direction);
            //Texture2D tex = TextureHelper.GetTexture2DFromTexture(this.m_TerrainMaterialSystem.splatmap, TextureFormat.RG16);
            //if (tex != null) TextureHelper.SaveTextureAsPNG(tex, Path.Combine(EDT.ResourcesIcons, "TEST.PNG"));
        }

        private void CreateDynamicVFXIfNeeded()
        {
            if (VegetationRenderSystem.s_FoliageVFXAsset != null && this.m_FoliageVFX == null)
            {
                COSystemBase.baseLog.DebugFormat("Creating FoliageVFX", Array.Empty<object>());
                this.m_FoliageVFX = new GameObject("FoliageVFX").AddComponent<VisualEffect>();
                this.m_FoliageVFX.visualEffectAsset = VegetationRenderSystem.s_FoliageVFXAsset;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnityEngine.Object.Destroy(m_UserPaintedGrassTexture);
        }

        private bool TryGetValideBrushEntity(NativeArray<Entity> entities, out Entity brushEntity, out Brush brush, out GrassData grassData )
        {   
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                Brush tBrush = EntityManager.GetComponentData<Brush>(entity);
                if(EntityManager.TryGetComponent<GrassData>(tBrush.m_Tool, out grassData))
                {
                    brushEntity = entity;
                    brush = tBrush;
                    return true;
                }
            }

            brushEntity = Entity.Null;
            brush = default(Brush);
            grassData = default(GrassData);
            return false;

        }

        private TextureStruct<T> GetTextureFromTexture2D<T>(Texture2D texture) where T : struct
        {
            TextureStruct<T> textureStruct = default;
            textureStruct.data = texture.GetPixelData<T>(0);
            textureStruct.width = texture.width;
            textureStruct.height = texture.height;
            return textureStruct;
        }

        private struct ColorRG8
        {
            public byte r, g;
        }

        private struct ColorA8
        {
            public byte a;
        }

        private struct TextureStruct<T> where T : struct
        {
            public NativeArray<T> data;
            public int width;
            public int height;

            public void SetValue(int x, int y, T value)
            {
                if (y < 0 || y > height - 1 || x < 0 || x > width - 1)
                {
                    throw new IndexOutOfRangeException($"Input x : {x}, max width {width} | Input y : {y}, max width {height}");
                }

                int val = y * width + x;

                if (val >= data.Length)
                {
                    throw new IndexOutOfRangeException($"data.Length : {data.Length} | Calculate : {val}");
                }

                data[val] = value;
            }

            public T GetValue(int x, int y)
            {
                if (y < 0 || y > height - 1 || x < 0 || x > width - 1)
                {
                    throw new IndexOutOfRangeException($"Input x : {x}, max width {width} | Input y : {y}, max width {height}");
                }

                int val = y * width + x;

                if (val >= data.Length)
                {
                    throw new IndexOutOfRangeException($"data.Length : {data.Length} | Calculate : {val}");
                }

                return data[val];
            }

            public void SetValue(int2 int2, T value)
            {
                SetValue(int2.x, int2.y, value);
            }

            public T GetValue(int2 int2)
            {
                return GetValue(int2.x, int2.y);
            }

            public float2 Size()
            {
                return new float2(width, height);
            }

            public void Dispose()
            {
                data.Dispose();
            }

        }

#if RELEASE
	[BurstCompile]
#endif
        [BurstCompile]
        private struct UpdateUserPaintedGrass : IJob
        {
            [ReadOnly]
            public TextureStruct<ColorA8> brushTexture;

            public TextureStruct<ColorRG8> userPaintedGrassTexture;

            [ReadOnly]
            public Brush brush;

            [ReadOnly]
            public Bounds2 brushArea;

            [ReadOnly]
            public GrassData grassData;

            [ReadOnly]
            public TerrainHeightData terrainHeightData;

            public void Execute()
            {

                float3 brushAreaMinFloat3 = new float3(brushArea.min.x, 0, brushArea.min.y);
                float3 brushAreaMaxFloat3 = new float3(brushArea.max.x, 0, brushArea.max.y);

                float3 brushAreaMinToHeightMapFloat3 = TerrainUtils.ToHeightmapSpace(ref terrainHeightData, brushAreaMinFloat3);
                float3 brushAreaMaxToHeightMapFloat3 = TerrainUtils.ToHeightmapSpace(ref terrainHeightData, brushAreaMaxFloat3);

                Bounds2 brushHeightMapArea = new Bounds2(brushAreaMinToHeightMapFloat3.xz, brushAreaMaxToHeightMapFloat3.xz);

                float2 brushHeightMapAreaSize = brushHeightMapArea.Size();
                float2 areaToTextureScale = brushTexture.Size() / brushHeightMapAreaSize;

                for (int x = 0; x < brushHeightMapAreaSize.x; x++)
                {
                    for (int y = 0; y < brushHeightMapAreaSize.y; y++)
                    {
                        int2 brushTexturePos = new int2( (int)Math.Round(x * areaToTextureScale.x) , (int)Math.Round(y * areaToTextureScale.y));
                        int2 userTexturePos = new int2((int)Math.Round(brushHeightMapArea.min.x) + x, (int)Math.Round(brushHeightMapArea.min.y) + y);
                        ColorRG8 color = userPaintedGrassTexture.GetValue(userTexturePos);

                        int val;
                        switch(grassData.m_State)
                        {
                            case Tools.GrassToolSystem.State.Adding:
                                //val = (int)Math.Round(color.g + brushTexture.GetValue(brushTexturePos).a / 255 * brush.m_Strength);
                                //color.g = val > 255 ? (byte)255 : (byte)val;
                                color.g = brushTexture.GetValue(brushTexturePos).a > 200 ? (byte)255 : color.g;
                                break;
                            case Tools.GrassToolSystem.State.Removing:
                                //color.g = color.g - brushTexture.GetValue(brushTexturePos).a * brush.m_Strength;
                                //val = (int)Math.Round(color.g - brushTexture.GetValue(brushTexturePos).a / 255 * brush.m_Strength);
                                //color.g = val < 0 ? (byte)0 : (byte)val;
                                color.g = brushTexture.GetValue(brushTexturePos).a > 200 ? (byte)0 : color.g;
                                break;
                        }
                        
                        userPaintedGrassTexture.SetValue(userTexturePos, color);
                    }
                }

            }

        }

    }
}
