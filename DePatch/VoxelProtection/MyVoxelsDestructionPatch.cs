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
        		voxelMaterial = (MyVoxelMaterialDefinition) null;
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
            MyVoxelsDestructionPatch.ComputeShapeBounds.Invoke(null, new object[6]
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
      		if ((MyStorageData) MyVoxelsDestructionPatch.Cache.GetValue(null) == null)
            {
                MyVoxelsDestructionPatch.Cache.SetValue(null, new MyStorageData());
            } ((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).Resize(vector3I, vector3I2);

            MyVoxelRequestFlags myVoxelRequestFlags = (skipCache ? ((MyVoxelRequestFlags)0) : MyVoxelRequestFlags.AdviseCache) | (flag2 ? MyVoxelRequestFlags.ConsiderContent : ((MyVoxelRequestFlags)0));
            voxelMap.Storage.ReadRange((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null), flag2 ? MyStorageDataTypeFlags.ContentAndMaterial : MyStorageDataTypeFlags.Content, 0, vector3I, vector3I2, ref myVoxelRequestFlags);
            if (!flag1)
            {
                Vector3I vector3I4 = (vector3I + vector3I2) / 2;
                voxelMaterial = voxelMap.Storage.GetMaterialAt(ref vector3I4);
            }
            else
            {
                Vector3I vector3I3 = ((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue((object)null)).Size3D / 2;
                voxelMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition(((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).Material(ref vector3I3));
            }
            MyVoxelMaterialDefinition key = (MyVoxelMaterialDefinition) null;
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
						int linearIdx = ((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).ComputeLinear(ref vector3I6);
						byte b = ((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).Content(linearIdx);
						if (b != (byte) 0)
						{
							Vector3D vector3D = (vector3I5 - voxelMap.StorageMin) * 1.0;
							float volume = shape.GetVolume(ref vector3D);
							if ((double) volume != 0.0)
							{
								int num3 = (int)((double) volume * byte.MaxValue);
								int num4 = Math.Max((int)b - num3, 0);
								int num5 = (int)b - num4;
								if ((int)(b / 10) != num4 / 10)
								{
									if (!onlyCheck && !onlyApplyMaterial)
										((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).Content(linearIdx, (byte)num4);

									num1 += (int)b;
									num2 += num5;
									byte b2 = ((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).Material(linearIdx);
									if (num4 == 0)
										((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null)).Material(linearIdx, byte.MaxValue);

									if (b2 != byte.MaxValue)
									{
										if (flag2)
											key = MyDefinitionManager.Static.GetVoxelMaterialDefinition(b2);

										if (exactCutOutMaterials != null)
										{
                                            exactCutOutMaterials.TryGetValue(key, out int num6);
                                            num6 += MyFakes.ENABLE_REMOVED_VOXEL_CONTENT_HACK ? (int) (num5 * 3.9f) : num5;
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
				shape.SendDrillCutOutRequest(voxelMap, applyDamageMaterial);

			if (num2 > 0 && !onlyCheck)
			{
				MyVoxelsDestructionPatch.RemoveSmallVoxelsUsingChachedVoxels1.Invoke(null, new object[0]);
				MyStorageDataTypeFlags dataToWrite = MyStorageDataTypeFlags.ContentAndMaterial;
				if (MyFakes.LOG_NAVMESH_GENERATION && MyAIComponent.Static.Pathfinding != null)
					MyAIComponent.Static.Pathfinding.GetPathfindingLog().LogStorageWrite(voxelMap, (MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null), dataToWrite, vector3I, vector3I2);

				voxelMap.Storage.WriteRange((MyStorageData)MyVoxelsDestructionPatch.Cache.GetValue(null), dataToWrite, vector3I, vector3I2, false, skipCache);
			}
			voxelsCountInPercent = (num1 > 0f) ? (num2 / (float)num1) : 0f;
			if (num2 > 0)
			{
				BoundingBoxD cutOutBox = shape.GetWorldBoundaries();
                MySandboxGame.Static.Invoke(delegate ()
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
	}
}
