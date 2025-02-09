using Colossal.Mathematics;
using Colossal.Serialization.Entities;
using ExtraLandscapingTools.MOD.Prefabs;
using ExtraLandscapingTools.Prefabs;
using Game;
using Game.Audio;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace ExtraLandscapingTools.Systems.Tools
{
    public partial class GrassToolSystem : ToolBaseSystem
    {

        public enum State
        {
            Default,
            Adding,
            Removing,
            SetDefault,
        }

        private AudioManager m_AudioManager;
        private AudioSource m_AudioSource;
        private ToolOutputBarrier m_ToolOutputBarrier;
        private EntityQuery m_VisibleQuery;
        private EntityQuery m_TempQuery;
        private EntityQuery m_SoundQuery;
        private EntityQuery m_BrushQuery;
        private EntityQuery m_DefinitionQuery;
        private ControlPoint m_RaycastPoint;
        private ControlPoint m_StartPoint;
        private State m_State;
        private bool m_TargetSet;
        private float3 m_TargetPosition;
        private float3 m_ApplyPosition;

        private ComponentTypeHandle<Brush> m_BrushComponentTypeHandle;

        public override string toolID => "Grass Tool";

        public GrassPrefab prefab { get; private set; }

        public override PrefabBase GetPrefab()
        {
            return prefab;
        }

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            GrassPrefab grassPrefab = prefab as GrassPrefab;
            if (grassPrefab != null)
            {
                this.SetPrefab(grassPrefab);
                return true;
            }
            return false;
        }

        public void SetPrefab(GrassPrefab value)
        {
            this.prefab = value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            this.m_BrushComponentTypeHandle = SystemAPI.GetComponentTypeHandle<Brush>();
            this.m_AudioManager = base.World.GetOrCreateSystemManaged<AudioManager>();
            this.m_ToolOutputBarrier = base.World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            this.m_DefinitionQuery = base.GetDefinitionQuery();
            this.m_BrushQuery = base.GetBrushQuery();
            this.m_VisibleQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<Brush>(),
                ComponentType.Exclude<Hidden>(),
                ComponentType.Exclude<Deleted>()
            });
            this.m_TempQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<Brush>(),
                ComponentType.ReadOnly<Temp>()
            });
            this.m_SoundQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ToolUXSoundSettingsData>()
            });

            base.brushSize = 100f;
            base.brushAngle = 0f;
            base.brushStrength = 0.5f;

            // Fix for the Grass prefab, because it has to be a child of ObjectPrefab because BrushRenderSystem as weird code and use a null material otherwise.
            ToolSystem toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            ObjectToolSystem objectToolSystem = World.GetOrCreateSystemManaged<ObjectToolSystem>();
            int index  = toolSystem.tools.IndexOf(objectToolSystem);
            toolSystem.tools.Remove(this);
            toolSystem.tools.Insert(index, this);

        }

        protected override void OnGameLoaded(Context serializationContext)
        {
            base.OnGameLoaded(serializationContext);
            base.brushType = FindDefaultBrush(this.m_BrushQuery);
            base.brushSize = 100f;
            base.brushAngle = 0f;
            base.brushStrength = 0.5f;
        }
        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            this.m_RaycastPoint = default(ControlPoint);
            this.m_StartPoint = default(ControlPoint);
            this.m_State = GrassToolSystem.State.Default;
            base.applyAction.enabled = true;
            base.secondaryApplyAction.enabled = true;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            if (this.prefab != null && base.brushType != null)
            {
                this.m_ToolRaycastSystem.typeMask = TypeMask.Terrain;
                this.m_ToolRaycastSystem.raycastFlags |= RaycastFlags.Outside;
                return;
            }
            this.m_ToolRaycastSystem.typeMask = TypeMask.None;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (base.brushType == null)
            {
                base.brushType = base.FindDefaultBrush(this.m_BrushQuery);
            }
            base.requireNet = (Layer.Road | Layer.TrainTrack | Layer.Pathway | Layer.TramTrack | Layer.SubwayTrack | Layer.PublicTransportRoad);
            base.requirePipelines = true;
            if (this.m_FocusChanged)
            {
                return inputDeps;
            }
            if (this.prefab != null && base.brushType != null && this.m_HasFocus)
            {
                base.UpdateInfoview(this.m_PrefabSystem.GetEntity(this.prefab));
                this.GetAvailableSnapMask(out this.m_SnapOnMask, out this.m_SnapOffMask);
                if (this.m_State != GrassToolSystem.State.Default && !base.applyAction.enabled)
                {
                    this.m_State = GrassToolSystem.State.Default;
                }
                if ((this.m_ToolRaycastSystem.raycastFlags & (RaycastFlags.DebugDisable | RaycastFlags.UIDisable)) == (RaycastFlags)0U)
                {
                    if (this.m_State != GrassToolSystem.State.Default)
                    {
                        if (base.applyAction.WasPressedThisFrame() || base.applyAction.WasReleasedThisFrame())
                        {
                            return this.Apply(inputDeps, false);
                        }
                        if (base.secondaryApplyAction.WasPressedThisFrame() || base.secondaryApplyAction.WasReleasedThisFrame())
                        {
                            return this.Cancel(inputDeps, false);
                        }
                        return this.Update(inputDeps);
                    }
                    else
                    {
                        if (base.secondaryApplyAction.WasPressedThisFrame())
                        {
                            return this.Cancel(inputDeps, base.secondaryApplyAction.WasReleasedThisFrame());
                        }
                        if (base.applyAction.WasPressedThisFrame())
                        {
                            return this.Apply(inputDeps, base.applyAction.WasReleasedThisFrame());
                        }
                        return this.Update(inputDeps);
                    }
                }
            }
            else
            {
                base.UpdateInfoview(Entity.Null);
            }
            if (this.m_State != GrassToolSystem.State.Default && (base.applyAction.WasReleasedThisFrame() || base.secondaryApplyAction.WasReleasedThisFrame() || !this.m_HasFocus))
            {
                this.m_StartPoint = default(ControlPoint);
                this.m_State = GrassToolSystem.State.Default;
            }
            return this.Clear(inputDeps);
        }

        private JobHandle Update(JobHandle inputDeps)
        {
            ControlPoint controlPoint;
            if (this.GetRaycastResult(out controlPoint))
            {
                if (this.m_State != GrassToolSystem.State.Default)
                {
                    base.applyMode = this.GetAllowApply() ? ApplyMode.Apply : ApplyMode.Clear;
                    this.m_StartPoint = this.m_RaycastPoint;
                    this.m_RaycastPoint = controlPoint;
                    return this.UpdateDefinitions(inputDeps);
                }
                if (!this.m_RaycastPoint.Equals(controlPoint))
                {
                    base.applyMode = ApplyMode.Clear;
                    this.m_StartPoint = controlPoint;
                    this.m_RaycastPoint = controlPoint;
                    return this.UpdateDefinitions(inputDeps);
                }
                if (this.HaveBrushSettingsChanged())
                {
                    base.applyMode = ApplyMode.Clear;
                    return this.UpdateDefinitions(inputDeps);
                }
                base.applyMode = ApplyMode.None;
                return inputDeps;
            }
            else
            {
                if (this.m_RaycastPoint.Equals(default(ControlPoint)))
                {
                    base.applyMode = ApplyMode.None;
                    return inputDeps;
                }
                if (this.m_State != GrassToolSystem.State.Default)
                {
                    base.applyMode = (this.GetAllowApply() ? ApplyMode.Apply : ApplyMode.Clear);
                    this.m_StartPoint = this.m_RaycastPoint;
                    this.m_RaycastPoint = default(ControlPoint);
                }
                else
                {
                    base.applyMode = ApplyMode.Clear;
                    this.m_StartPoint = default(ControlPoint);
                    this.m_RaycastPoint = default(ControlPoint);
                }
                return this.UpdateDefinitions(inputDeps);
            }
        }

        private bool HaveBrushSettingsChanged()
        {
            bool result;
            using (NativeArray<ArchetypeChunk> nativeArray = this.m_VisibleQuery.ToArchetypeChunkArray(Allocator.TempJob))
            {
                m_BrushComponentTypeHandle.Update(ref base.CheckedStateRef);
                ComponentTypeHandle<Brush> brushComponentTypeHandle = m_BrushComponentTypeHandle;
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    NativeArray<Brush> nativeArray2 = nativeArray[i].GetNativeArray<Brush>(ref brushComponentTypeHandle);
                    for (int j = 0; j < nativeArray2.Length; j++)
                    {
                        if (!nativeArray2[j].m_Size.Equals(base.brushSize))
                        {
                            return true;
                        }
                    }
                }
                result = false;
            }
            return result;
        }

        private JobHandle Apply(JobHandle inputDeps, bool singleFrameOnly = false)
        {
            if (this.m_State == GrassToolSystem.State.Default)
            {
                base.applyMode = this.GetAllowApply() ? ApplyMode.Apply : ApplyMode.Clear;
                if (!singleFrameOnly)
                {
                    this.m_StartPoint = this.m_RaycastPoint;
                    this.m_State = GrassToolSystem.State.Adding;
                }
                if (this.m_AudioSource == null && !this.m_ToolSystem.actionMode.IsEditor())
                {
                    this.m_AudioSource = this.m_AudioManager.PlayExclusiveUISound(this.m_SoundQuery.GetSingleton<ToolUXSoundSettingsData>().m_TerraformSound);
                }
                this.GetRaycastResult(out this.m_RaycastPoint);
                this.m_ApplyPosition = this.m_RaycastPoint.m_HitPosition;
                return this.UpdateDefinitions(inputDeps);
            }
            if (this.m_State == GrassToolSystem.State.Adding)
            {
                base.applyMode = (this.GetAllowApply() ? ApplyMode.Apply : ApplyMode.Clear);
                this.m_StartPoint = default(ControlPoint);
                this.m_State = GrassToolSystem.State.Default;
                this.GetRaycastResult(out this.m_RaycastPoint);
                this.SetDisableFX();
                return this.UpdateDefinitions(inputDeps);
            }
            base.applyMode = ApplyMode.Clear;
            this.m_StartPoint = default(ControlPoint);
            this.m_State = GrassToolSystem.State.Default;
            this.GetRaycastResult(out this.m_RaycastPoint);
            this.SetDisableFX();
            return this.UpdateDefinitions(inputDeps);
        }

        private JobHandle Cancel(JobHandle inputDeps, bool singleFrameOnly = false)
        {
            if (this.m_State == GrassToolSystem.State.Default)
            {
                base.applyMode = this.GetAllowApply() ? ApplyMode.Apply : ApplyMode.Clear;
                if (!singleFrameOnly)
                {
                    this.m_StartPoint = this.m_RaycastPoint;
                    this.m_State = GrassToolSystem.State.Removing;
                }
                if (this.m_AudioSource == null && !this.m_ToolSystem.actionMode.IsEditor())
                {
                    this.m_AudioSource = this.m_AudioManager.PlayExclusiveUISound(this.m_SoundQuery.GetSingleton<ToolUXSoundSettingsData>().m_TerraformSound);
                }
                this.GetRaycastResult(out this.m_RaycastPoint);
                this.m_TargetSet = true;
                this.m_TargetPosition = this.m_RaycastPoint.m_HitPosition;
                inputDeps = base.InvertBrushes(this.m_TempQuery, inputDeps);
                return this.UpdateDefinitions(inputDeps);
            }
            if (this.m_State == GrassToolSystem.State.Removing)
            {
                base.applyMode = this.GetAllowApply() ? ApplyMode.Apply : ApplyMode.Clear;
                this.m_StartPoint = default(ControlPoint);
                this.m_State = GrassToolSystem.State.Default;
                this.GetRaycastResult(out this.m_RaycastPoint);
                this.SetDisableFX();
                return this.UpdateDefinitions(inputDeps);
            }
            base.applyMode = ApplyMode.Clear;
            this.m_StartPoint = default(ControlPoint);
            this.m_State = GrassToolSystem.State.Default;
            this.GetRaycastResult(out this.m_RaycastPoint);
            this.SetDisableFX();
            return this.UpdateDefinitions(inputDeps);
        }

        private JobHandle Clear(JobHandle inputDeps)
        {
            base.applyMode = ApplyMode.Clear;
            this.SetDisableFX();
            return inputDeps;
        }

        private void SetDisableFX()
        {
            if (this.m_AudioSource != null)
            {
                this.m_AudioManager.StopExclusiveUISound(this.m_AudioSource);
                this.m_AudioSource = null;
            }
        }

        private JobHandle UpdateDefinitions(JobHandle inputDeps)
        {
            JobHandle jobHandle = base.DestroyDefinitions(this.m_DefinitionQuery, this.m_ToolOutputBarrier, inputDeps);
            if (this.prefab != null && base.brushType != null)
            {
                GrassToolSystem.CreateDefinitionsJob jobData = default(GrassToolSystem.CreateDefinitionsJob);
                jobData.m_Prefab = this.m_PrefabSystem.GetEntity(this.prefab);
                jobData.m_Brush = this.m_PrefabSystem.GetEntity(base.brushType);
                jobData.m_Size = base.brushSize;
                jobData.m_Angle = math.radians(base.brushAngle);
                jobData.m_Strength = base.brushStrength;
                jobData.m_Time = UnityEngine.Time.deltaTime;
                jobData.m_StartPoint = this.m_StartPoint;
                jobData.m_EndPoint = this.m_RaycastPoint;
                jobData.m_Target = (this.m_TargetSet ? this.m_TargetPosition : this.m_RaycastPoint.m_HitPosition);
                jobData.m_ApplyStart = this.m_ApplyPosition;
                jobData.m_State = this.m_State;
                jobData.m_CommandBuffer = this.m_ToolOutputBarrier.CreateCommandBuffer();
                JobHandle jobHandle2 = jobData.Schedule(inputDeps);
                this.m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle2);
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobHandle2);
                if (base.applyMode == ApplyMode.Apply)
                {
                    base.EnsureCachedBrushData();
                }
            }
            return jobHandle;
        }


#if RELEASE
	[BurstCompile]
#endif
        private struct CreateDefinitionsJob : IJob
        {
            public void Execute()
            {
                if (this.m_EndPoint.Equals(default(ControlPoint)))
                {
                    return;
                }
                CreationDefinition creationDefinition = default(CreationDefinition);
                creationDefinition.m_Prefab = this.m_Brush;
                BrushDefinition brushDefinition = default(BrushDefinition);
                brushDefinition.m_Tool = this.m_Prefab;
                if (this.m_StartPoint.Equals(default(ControlPoint)))
                {
                    brushDefinition.m_Line = new Line3.Segment(this.m_EndPoint.m_Position, this.m_EndPoint.m_Position);
                }
                else
                {
                    brushDefinition.m_Line = new Line3.Segment(this.m_StartPoint.m_Position, this.m_EndPoint.m_Position);
                }
                brushDefinition.m_Size = this.m_Size;
                brushDefinition.m_Angle = this.m_Angle;
                brushDefinition.m_Strength = this.m_Strength;
                brushDefinition.m_Time = this.m_Time;
                brushDefinition.m_Target = this.m_Target;
                brushDefinition.m_Start = this.m_ApplyStart;

                GrassData grassBrushData = default;
                grassBrushData.m_State = m_State;
                this.m_CommandBuffer.AddComponent<GrassData>(m_Prefab, grassBrushData);

                Entity e = this.m_CommandBuffer.CreateEntity();
                this.m_CommandBuffer.AddComponent<CreationDefinition>(e, creationDefinition);
                this.m_CommandBuffer.AddComponent<BrushDefinition>(e, brushDefinition);
                this.m_CommandBuffer.AddComponent<Updated>(e, default(Updated));
            }

            [ReadOnly]
            public Entity m_Prefab;

            [ReadOnly]
            public Entity m_Brush;

            [ReadOnly]
            public float m_Size;

            [ReadOnly]
            public float m_Angle;

            [ReadOnly]
            public float m_Strength;

            [ReadOnly]
            public float m_Time;

            [ReadOnly]
            public float3 m_Target;

            [ReadOnly]
            public float3 m_ApplyStart;

            [ReadOnly]
            public ControlPoint m_StartPoint;

            [ReadOnly]
            public ControlPoint m_EndPoint;

            [ReadOnly]
            public State m_State;

            public EntityCommandBuffer m_CommandBuffer;
        }

    }
}
