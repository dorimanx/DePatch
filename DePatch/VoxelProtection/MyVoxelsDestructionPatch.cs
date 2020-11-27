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
        private static MethodInfo RemoveSmallVoxelsUsingChachedVoxels = typeof(MyVoxelGenerator).GetMethod("RemoveSmallVoxelsUsingChachedVoxels", BindingFlags.Static | BindingFlags.NonPublic);

        private static FieldInfo m_cache = typeof(MyVoxelGenerator).GetField("m_cache", BindingFlags.Static | BindingFlags.NonPublic);

        private static MethodInfo ComputeShapeBounds = typeof(MyVoxelGenerator).GetMethod("ComputeShapeBounds", BindingFlags.Static | BindingFlags.NonPublic);

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
            MyVoxelBase voxelMap2 = voxelMap;
            if ((!MySession.Static.EnableVoxelDestruction && !applyDamageMaterial) || voxelMap2 == null || voxelMap2.Storage == null || shape == null)
            {
                voxelsCountInPercent = 0f;
                voxelMaterial = null;
                return false;
            }
            int num = 0;
            int num2 = 0;
            bool flag = exactCutOutMaterials != null;
            MatrixD transformation = shape.Transformation;
            MatrixD transformation2 = transformation * voxelMap2.PositionComp.WorldMatrixInvScaled;
            transformation2.Translation += voxelMap2.SizeInMetresHalf;
            shape.Transformation = transformation2;
            BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
            Vector3I minCorner = default(Vector3I);
            Vector3I maxCorner = default(Vector3I);
            ComputeShapeBounds.Invoke(null, new object[6]
            {
                 voxelMap2,
                 worldBoundaries,
                 Vector3.Zero,
                 voxelMap2.Storage.Size,
                 minCorner,
                 maxCorner
            });
            bool flag2 = exactCutOutMaterials != null || applyDamageMaterial;
            Vector3I voxelCoord = minCorner - 1;
            Vector3I voxelCoord2 = maxCorner + 1;
            voxelMap2.Storage.ClampVoxelCoord(ref voxelCoord);
            voxelMap2.Storage.ClampVoxelCoord(ref voxelCoord2);
            if ((MyStorageData)m_cache.GetValue(null) == null)
            {
                m_cache.SetValue(null, new MyStorageData());
            }
            ((MyStorageData)m_cache.GetValue(null)).Resize(voxelCoord, voxelCoord2);
            MyVoxelRequestFlags requestFlags = ((!skipCache) ? MyVoxelRequestFlags.AdviseCache : 0) | (flag2 ? MyVoxelRequestFlags.ConsiderContent : 0);
            voxelMap2.Storage.ReadRange((MyStorageData)m_cache.GetValue(null), (!flag2) ? MyStorageDataTypeFlags.Content : MyStorageDataTypeFlags.ContentAndMaterial, 0, voxelCoord, voxelCoord2, ref requestFlags);
            if (flag)
            {
                Vector3I p = ((MyStorageData)m_cache.GetValue(null)).Size3D / 2;
                voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(((MyStorageData)m_cache.GetValue(null)).Material(ref p));
            }
            else
            {
                Vector3I voxelCoords = (voxelCoord + voxelCoord2) / 2;
                voxelMaterial = voxelMap2.Storage.GetMaterialAt(ref voxelCoords);
            }
            MyVoxelMaterialDefinition key = null;
            Vector3I vector3I = default;
            vector3I.X = minCorner.X;
            while (vector3I.X <= maxCorner.X)
            {
                vector3I.Y = minCorner.Y;
                while (vector3I.Y <= maxCorner.Y)
                {
                    vector3I.Z = minCorner.Z;
                    while (vector3I.Z <= maxCorner.Z)
                    {
                        Vector3I p2 = vector3I - voxelCoord;
                        int linearIdx = ((MyStorageData)m_cache.GetValue(null)).ComputeLinear(ref p2);
                        byte b = ((MyStorageData)m_cache.GetValue(null)).Content(linearIdx);
                        if (b != 0)
                        {
                            Vector3D voxelPosition = (vector3I - voxelMap2.StorageMin) * 1.0;
                            float volume = shape.GetVolume(ref voxelPosition);
                            if (volume != 0f)
                            {
                                int num3 = (int)(volume * 255f);
                                int num4 = Math.Max(b - num3, 0);
                                int num5 = b - num4;
                                if ((int)b / 10 != num4 / 10)
                                {
                                    if (!onlyCheck && !onlyApplyMaterial)
                                    {
                                        ((MyStorageData)m_cache.GetValue(null)).Content(linearIdx, (byte)num4);
                                    }
                                    num += b;
                                    num2 += num5;
                                    byte b2 = ((MyStorageData)m_cache.GetValue(null)).Material(linearIdx);
                                    if (num4 == 0)
                                    {
                                        ((MyStorageData)m_cache.GetValue(null)).Material(linearIdx, byte.MaxValue);
                                    }
                                    if (b2 != byte.MaxValue)
                                    {
                                        if (flag2)
                                        {
                                            key = MyDefinitionManager.Static.GetVoxelMaterialDefinition(b2);
                                        }
                                        if (exactCutOutMaterials != null)
                                        {
                                            exactCutOutMaterials.TryGetValue(key, out var value);
                                            value = (exactCutOutMaterials[key] = value + (MyFakes.ENABLE_REMOVED_VOXEL_CONTENT_HACK ? ((int)((float)num5 * 3.9f)) : num5));
                                        }
                                    }
                                }
                            }
                        }
                        vector3I.Z++;
                    }
                    vector3I.Y++;
                }
                vector3I.X++;
            }
            if (num2 > 0 && updateSync && Sync.IsServer && !onlyCheck)
            {
                shape.SendDrillCutOutRequest(voxelMap2, applyDamageMaterial);
            }
            if (num2 > 0 && !onlyCheck)
            {
                RemoveSmallVoxelsUsingChachedVoxels.Invoke(null, new object[0]);
                MyStorageDataTypeFlags dataToWrite = MyStorageDataTypeFlags.ContentAndMaterial;
                if (MyFakes.LOG_NAVMESH_GENERATION && MyAIComponent.Static.Pathfinding != null)
                {
                    MyAIComponent.Static.Pathfinding.GetPathfindingLog().LogStorageWrite(voxelMap2, (MyStorageData)m_cache.GetValue(null), dataToWrite, voxelCoord, voxelCoord2);
                }
                voxelMap2.Storage.WriteRange((MyStorageData)m_cache.GetValue(null), dataToWrite, voxelCoord, voxelCoord2, notify: false, skipCache);
            }
            voxelsCountInPercent = (((float)num > 0f) ? ((float)num2 / (float)num) : 0f);
            if (num2 > 0)
            {
                BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                MySandboxGame.Static.Invoke(delegate
                {
                    if (voxelMap2.Storage != null)
                    {
                        voxelMap2.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.ContentAndMaterial);
                        MyVoxelGenerator.NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, voxelMap2, ref cutOutBox);
                    }
                }, "CutOutShapeWithProperties notify");
            }
            shape.Transformation = transformation;
            return false;
        }
    }
}
