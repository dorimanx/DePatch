using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
		// Token: 0x06000091 RID: 145 RVA: 0x00003FAC File Offset: 0x000021AC
		private unsafe static bool Prefix(MyVoxelBase voxelMap, MyShape shape, out float voxelsCountInPercent, out MyVoxelMaterialDefinition voxelMaterial, Dictionary<MyVoxelMaterialDefinition, int> exactCutOutMaterials, bool updateSync, bool onlyCheck, bool applyDamageMaterial, bool onlyApplyMaterial, bool skipCache)
		{
			if ((!MySession.Static.EnableVoxelDestruction && !applyDamageMaterial) || voxelMap == null || voxelMap.Storage == null || shape == null)
			{
				voxelsCountInPercent = 0f;
				voxelMaterial = null;
				return false;
			}
			int num = 0;
			int num2 = 0;
			bool flag = exactCutOutMaterials != null;
			MatrixD transformation = shape.Transformation;
			MatrixD transformation2 = transformation * voxelMap.PositionComp.WorldMatrixInvScaled;
			transformation2.Translation += voxelMap.SizeInMetresHalf;
			shape.Transformation = transformation2;
			BoundingBoxD worldBoundaries = shape.GetWorldBoundaries();
			Vector3I minCorner = default(Vector3I);
			Vector3I maxCorner = default(Vector3I);
			MyVoxelsDestructionPatch.ComputeShapeBounds.Invoke(null, new object[]
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
			if ((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null) == null)
			{
				MyVoxelsDestructionPatch.m_cache.SetValue(null, new MyStorageData());
			}
			((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Resize(vector3I, vector3I2);
			MyVoxelRequestFlags myVoxelRequestFlags = (skipCache ? ((MyVoxelRequestFlags)0) : MyVoxelRequestFlags.AdviseCache) | (flag2 ? MyVoxelRequestFlags.ConsiderContent : ((MyVoxelRequestFlags)0));
			voxelMap.Storage.ReadRange((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null), flag2 ? MyStorageDataTypeFlags.ContentAndMaterial : MyStorageDataTypeFlags.Content, 0, vector3I, vector3I2, ref myVoxelRequestFlags);
			if (flag)
			{
				Vector3I vector3I3 = ((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Size3D / 2;
				voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Material(ref vector3I3));
			}
			else
			{
				Vector3I vector3I4 = (vector3I + vector3I2) / 2;
				voxelMaterial = voxelMap.Storage.GetMaterialAt(ref vector3I4);
			}
			MyVoxelMaterialDefinition key = null;
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
						int linearIdx = ((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).ComputeLinear(ref vector3I6);
						byte b = ((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Content(linearIdx);
						if (b != 0)
						{
							Vector3D vector3D = (vector3I5 - voxelMap.StorageMin) * 1.0;
							float volume = shape.GetVolume(ref vector3D);
							if (volume != 0f)
							{
								int num3 = (int)(volume * 255f);
								int num4 = Math.Max((int)b - num3, 0);
								int num5 = (int)b - num4;
								if ((int)(b / 10) != num4 / 10)
								{
									if (!onlyCheck && !onlyApplyMaterial)
									{
										((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Content(linearIdx, (byte)num4);
									}
									num += (int)b;
									num2 += num5;
									byte b2 = ((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Material(linearIdx);
									if (num4 == 0)
									{
										((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null)).Material(linearIdx, byte.MaxValue);
									}
									if (b2 != 255)
									{
										if (flag2)
										{
											key = MyDefinitionManager.Static.GetVoxelMaterialDefinition(b2);
										}
										if (exactCutOutMaterials != null)
										{
											int num6;
											exactCutOutMaterials.TryGetValue(key, out num6);
											num6 += (MyFakes.ENABLE_REMOVED_VOXEL_CONTENT_HACK ? ((int)((float)num5 * 3.9f)) : num5);
											exactCutOutMaterials[key] = num6;
										}
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
				MyVoxelsDestructionPatch.RemoveSmallVoxelsUsingChachedVoxels.Invoke(null, new object[0]);
				MyStorageDataTypeFlags dataToWrite = MyStorageDataTypeFlags.ContentAndMaterial;
				if (MyFakes.LOG_NAVMESH_GENERATION && MyAIComponent.Static.Pathfinding != null)
				{
					MyAIComponent.Static.Pathfinding.GetPathfindingLog().LogStorageWrite(voxelMap, (MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null), dataToWrite, vector3I, vector3I2);
				}
				voxelMap.Storage.WriteRange((MyStorageData)MyVoxelsDestructionPatch.m_cache.GetValue(null), dataToWrite, vector3I, vector3I2, false, skipCache);
			}
			voxelsCountInPercent = (((float)num > 0f) ? ((float)num2 / (float)num) : 0f);
			if (num2 > 0)
			{
				BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
				MySandboxGame.Static.Invoke(delegate()
				{
					if (voxelMap.Storage != null)
					{
						voxelMap.Storage.NotifyChanged(minCorner, maxCorner, MyStorageDataTypeFlags.ContentAndMaterial);
						MyVoxelGenerator.NotifyVoxelChanged(MyVoxelBase.OperationType.Cut, voxelMap, ref cutOutBox);
					}
				}, "CutOutShapeWithProperties notify");
			}
			shape.Transformation = transformation;
			return false;
		}

		// Token: 0x0400004F RID: 79
		private static MethodInfo RemoveSmallVoxelsUsingChachedVoxels = typeof(MyVoxelGenerator).GetMethod("RemoveSmallVoxelsUsingChachedVoxels", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04000050 RID: 80
		private static FieldInfo m_cache = typeof(MyVoxelGenerator).GetField("m_cache", BindingFlags.Static | BindingFlags.NonPublic);

		// Token: 0x04000051 RID: 81
		private static MethodInfo ComputeShapeBounds = typeof(MyVoxelGenerator).GetMethod("ComputeShapeBounds", BindingFlags.Static | BindingFlags.NonPublic);
	}
}
