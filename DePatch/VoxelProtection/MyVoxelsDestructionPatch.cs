using System;
using System.Collections.Generic;
using System.Reflection;
using Sandbox;
using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.Engine.Voxels;
using Sandbox.Game.AI;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Voxels;
using VRageMath;

namespace DePatch
{
    internal class MyVoxelsDestructionPatch
    {
        private static readonly MethodInfo methodInfo = typeof(MyVoxelGenerator).GetMethod(nameof(RemoveSmallVoxelsUsingChachedVoxels1), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo fieldInfo = typeof(MyVoxelGenerator).GetField(nameof(Cache), BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ComputeShapeBounds = typeof(MyVoxelGenerator).GetMethod(nameof(ComputeShapeBounds), BindingFlags.Static | BindingFlags.NonPublic);

        public static MethodInfo RemoveSmallVoxelsUsingChachedVoxels1 { get; set; } = methodInfo;
        public static FieldInfo Cache { get; set; } = fieldInfo;

        private static bool Prefix(
          MyVoxelBase voxelMap,
          MyShape shape,
          out float voxelsCountInPercent,
          out MyVoxelMaterialDefinition voxelMaterial,
          Dictionary<MyVoxelMaterialDefinition, int> exactCutOutMaterials,
          bool updateSync,
          bool onlyCheck,
          bool applyDamageMaterial,
          bool onlyApplyMaterial,
          bool skipCache)
        {
            if ((!MySession.Static.EnableVoxelDestruction && !applyDamageMaterial) || voxelMap == null || voxelMap.Storage == null || shape == null)
            {
                voxelsCountInPercent = 0.0f;
                voxelMaterial = (MyVoxelMaterialDefinition)null;
                return false;
            }
            int num1 = 0;
            int num2 = 0;
            bool flag1 = exactCutOutMaterials != null;
            MatrixD transformation = shape.Transformation;
            MatrixD transformation2 = transformation * voxelMap.PositionComp.WorldMatrixInvScaled;
            transformation2.Translation += voxelMap.SizeInMetresHalf;
            shape.Transformation = transformation2;
            BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
            Vector3I minCorner = new Vector3I();
            Vector3I maxCorner = new Vector3I();
            ComputeShapeBounds.Invoke(null, new object[6]
            {
                 voxelMap,
                 worldBoundaries,
                 Vector3.Zero,
                 voxelMap.Storage.Size,
                 minCorner,
                 maxCorner
            });
            bool flag2 = exactCutOutMaterials != null || applyDamageMaterial;
            Vector3I vector3I = minCorner - 1;
            Vector3I vector3I2 = maxCorner + 1;
            voxelMap.Storage.ClampVoxelCoord(ref vector3I, 1);
            voxelMap.Storage.ClampVoxelCoord(ref vector3I2, 1);
            if ((MyStorageData)Cache.GetValue(null) == null)
            {
                Cache.SetValue(null, new MyStorageData());
            } ((MyStorageData)Cache.GetValue(null)).Resize(vector3I, vector3I2);

            MyVoxelRequestFlags myVoxelRequestFlags = (skipCache ? 0 : MyVoxelRequestFlags.AdviseCache) | (flag2 ? MyVoxelRequestFlags.ConsiderContent : 0);
            voxelMap.Storage.ReadRange((MyStorageData)Cache.GetValue(null), flag2 ? MyStorageDataTypeFlags.ContentAndMaterial : MyStorageDataTypeFlags.Content, 0, vector3I, vector3I2, ref myVoxelRequestFlags);
            if (!flag1)
            {
                Vector3I vector3I4 = (vector3I + vector3I2) / 2;
                voxelMaterial = voxelMap.Storage.GetMaterialAt(ref vector3I4);
            }
            else
            {
                Vector3I vector3I3 = ((MyStorageData)Cache.GetValue(null)).Size3D / 2;
                voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(((MyStorageData)Cache.GetValue(null)).Material(ref vector3I3));
            }

            Vector3I vector3I5;
            vector3I5.X = minCorner.X;
            while (vector3I5.X <= maxCorner.X)
            {
                vector3I5.Y = minCorner.Y;
                while (vector3I5.Y <= maxCorner.Y)
                {
                    vector3I5.Z = minCorner.Z;
                    while (vector3I5.Z <= maxCorner.Z)
                    {
                        Vector3I vector3I6 = vector3I5 - vector3I;
                        if (((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) != 0)
                        {
                            Vector3D vector3D = (vector3I5 - voxelMap.StorageMin) * 1.0;
                            if ((double)shape.GetVolume(ref vector3D) != 0.0 && ((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) / 10 != Math.Max(((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - (shape.GetVolume(ref vector3D) * byte.MaxValue), 0) / 10)
                            {
                                if (!onlyCheck && !onlyApplyMaterial)
                                {
                                    ((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6),
                                        (byte)Math.Max(((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - (shape.GetVolume(ref vector3D) * byte.MaxValue), 0));
                                }

                                num1 += ((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6));
                                num2 += ((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - Math.Max(((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - (int)(shape.GetVolume(ref vector3D) * byte.MaxValue), 0);
                                if (Math.Max(((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - (shape.GetVolume(ref vector3D) * byte.MaxValue), 0) == 0)
                                {
                                    ((MyStorageData)Cache.GetValue(null)).Material(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6), byte.MaxValue);
                                }

                                if (((MyStorageData)Cache.GetValue(null)).Material(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) != byte.MaxValue)
                                {
                                    MyVoxelMaterialDefinition key = null;
                                    if (flag2)
                                        key = MyDefinitionManager.Static.GetVoxelMaterialDefinition(((MyStorageData)Cache.GetValue(null)).Material(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)));

                                    if (exactCutOutMaterials != null)
                                    {
                                        exactCutOutMaterials.TryGetValue(key, out int num6);
                                        num6 += MyFakes.ENABLE_REMOVED_VOXEL_CONTENT_HACK ? (int)((((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - Math.Max(((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - (shape.GetVolume(ref vector3D) * byte.MaxValue), 0)) * 3.9f) : ((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - Math.Max(((MyStorageData)Cache.GetValue(null)).Content(((MyStorageData)Cache.GetValue(null)).ComputeLinear(ref vector3I6)) - (int)(shape.GetVolume(ref vector3D) * byte.MaxValue), 0);
                                        exactCutOutMaterials[key] = num6;
                                    }
                                }
                            }
                        }
                        vector3I5.Z++;
                    }
                    vector3I5.Y++;
                }
                vector3I5.X++;
            }
            if (num2 > 0 && updateSync && Sync.IsServer && !onlyCheck)
            {
                shape.SendDrillCutOutRequest(voxelMap, applyDamageMaterial);
            }

            if (num2 > 0 && !onlyCheck)
            {
                RemoveSmallVoxelsUsingChachedVoxels1.Invoke(null, new object[0]);
                if (MyFakes.LOG_NAVMESH_GENERATION && MyAIComponent.Static.Pathfinding != null)
                {
                    MyAIComponent.Static.Pathfinding.GetPathfindingLog().LogStorageWrite(voxelMap,
                        (MyStorageData)Cache.GetValue(null), MyStorageDataTypeFlags.ContentAndMaterial,
                        vector3I, vector3I2);
                }

                voxelMap.Storage.WriteRange((MyStorageData)Cache.GetValue(null), MyStorageDataTypeFlags.ContentAndMaterial, vector3I, vector3I2, false, skipCache);
            }
            voxelsCountInPercent = (num1 > 0f) ? (num2 / (float)num1) : 0f;
            if (num2 > 0)
            {
                MySandboxGame.Static.Invoke(delegate ()
                {
                    if (voxelMap.Storage != null)
                    {
                        voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.ContentAndMaterial);
                        BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                        MyVoxelGenerator.NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, voxelMap, ref cutOutBox);
                    }
                }, "CutOutShapeWithProperties notify");
            }
            shape.Transformation = transformation;
            return false;
        }
    }
}
