using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Torch;

namespace DePatch
{
	public class DeConfig : ViewModel
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000025 RID: 37 RVA: 0x00002E53 File Offset: 0x00001053
		// (set) Token: 0x06000026 RID: 38 RVA: 0x00002E5B File Offset: 0x0000105B
		public bool DamageThreading
		{
			get
			{
				return this._damageThreading;
			}
			set
			{
				this.SetValue<bool>(ref this._damageThreading, value, "DamageThreading");
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002E6F File Offset: 0x0000106F
		// (set) Token: 0x06000028 RID: 40 RVA: 0x00002E77 File Offset: 0x00001077
		public long MaxProtectedLargeGridSize
		{
			get
			{
				return this._MaxProtectedLargeGridSize;
			}
			set
			{
				this.SetValue<long>(ref this._MaxProtectedLargeGridSize, value, "MaxProtectedLargeGridSize");
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000029 RID: 41 RVA: 0x00002E8B File Offset: 0x0000108B
		// (set) Token: 0x0600002A RID: 42 RVA: 0x00002E93 File Offset: 0x00001093
		public long MaxProtectedSmallGridSize
		{
			get
			{
				return this._MaxProtectedSmallGridSize;
			}
			set
			{
				this.SetValue<long>(ref this._MaxProtectedSmallGridSize, value, "MaxProtectedSmallGridSize");
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600002B RID: 43 RVA: 0x00002EA7 File Offset: 0x000010A7
		// (set) Token: 0x0600002C RID: 44 RVA: 0x00002EAF File Offset: 0x000010AF
		public float MinProtectSpeed
		{
			get
			{
				return this._MinProtectSpeed;
			}
			set
			{
				this.SetValue<float>(ref this._MinProtectSpeed, value, "MinProtectSpeed");
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600002D RID: 45 RVA: 0x00002EC3 File Offset: 0x000010C3
		// (set) Token: 0x0600002E RID: 46 RVA: 0x00002ECB File Offset: 0x000010CB
		public bool ProtectGrid
		{
			get
			{
				return this._ProtectGrid;
			}
			set
			{
				this.SetValue<bool>(ref this._ProtectGrid, value, "ProtectGrid");
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002EDF File Offset: 0x000010DF
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00002EE7 File Offset: 0x000010E7
		public bool ProtectVoxels
		{
			get
			{
				return this._ProtectVoxels;
			}
			set
			{
				this.SetValue<bool>(ref this._ProtectVoxels, value, "ProtectVoxels");
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002EFB File Offset: 0x000010FB
		// (set) Token: 0x06000032 RID: 50 RVA: 0x00002F03 File Offset: 0x00001103
		public List<string> DrillsSettings
		{
			get
			{
				return this._DrillsSettings;
			}
			set
			{
				this.SetValue<List<string>>(ref this._DrillsSettings, value, "DrillsSettings");
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00002F17 File Offset: 0x00001117
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00002F22 File Offset: 0x00001122
		public bool DrillIgnoreSubtypes
		{
			get
			{
				return !this._DrillIgnoreSubtypes;
			}
			set
			{
				this.SetValue<bool>(ref this._DrillIgnoreSubtypes, !value, "DrillIgnoreSubtypes");
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00002F39 File Offset: 0x00001139
		// (set) Token: 0x06000036 RID: 54 RVA: 0x00002F41 File Offset: 0x00001141
		public bool DrillStoneDumpRightClick
		{
			get
			{
				return this._DrillStoneDumpRightClick;
			}
			set
			{
				this.SetValue<bool>(ref this._DrillStoneDumpRightClick, value, "DrillStoneDumpRightClick");
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00002F55 File Offset: 0x00001155
		// (set) Token: 0x06000038 RID: 56 RVA: 0x00002F5D File Offset: 0x0000115D
		public bool DrillDisableRightClick
		{
			get
			{
				return this._DrillDisableRightClick;
			}
			set
			{
				this.SetValue<bool>(ref this._DrillDisableRightClick, value, "DrillDisableRightClick");
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00002F71 File Offset: 0x00001171
		// (set) Token: 0x0600003A RID: 58 RVA: 0x00002F79 File Offset: 0x00001179
		public DrillingMode ParallelDrill
		{
			get
			{
				return this._ParallelDrill;
			}
			set
			{
				this.SetValue<DrillingMode>(ref this._ParallelDrill, value, "ParallelDrill");
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002F8D File Offset: 0x0000118D
		// (set) Token: 0x0600003C RID: 60 RVA: 0x00002F95 File Offset: 0x00001195
		public int DrillUpdateRate
		{
			get
			{
				return this._DrillUpdateRate;
			}
			set
			{
				this.SetValue<int>(ref this._DrillUpdateRate, value, "DrillUpdateRate");
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002FA9 File Offset: 0x000011A9
		// (set) Token: 0x0600003E RID: 62 RVA: 0x00002FB1 File Offset: 0x000011B1
		public string PveMessageEntered
		{
			get
			{
				return this._PveMessageEntered;
			}
			set
			{
				this.SetValue<string>(ref this._PveMessageEntered, value, "PveMessageEntered");
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00002FC5 File Offset: 0x000011C5
		// (set) Token: 0x06000040 RID: 64 RVA: 0x00002FCD File Offset: 0x000011CD
		public string PveMessageLeft
		{
			get
			{
				return this._PveMessageLeft;
			}
			set
			{
				this.SetValue<string>(ref this._PveMessageLeft, value, "PveMessageLeft");
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00002FE1 File Offset: 0x000011E1
		// (set) Token: 0x06000042 RID: 66 RVA: 0x00002FE9 File Offset: 0x000011E9
		public bool PveZoneEnabled
		{
			get
			{
				return this._PveZoneEnabled;
			}
			set
			{
				this.SetValue<bool>(ref this._PveZoneEnabled, value, "PveZoneEnabled");
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000043 RID: 67 RVA: 0x00002FFD File Offset: 0x000011FD
		// (set) Token: 0x06000044 RID: 68 RVA: 0x00003005 File Offset: 0x00001205
		public float PveX
		{
			get
			{
				return this._PveX;
			}
			set
			{
				this.SetValue<float>(ref this._PveX, value, "PveX");
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000045 RID: 69 RVA: 0x00003019 File Offset: 0x00001219
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00003021 File Offset: 0x00001221
		public float PveY
		{
			get
			{
				return this._PveY;
			}
			set
			{
				this.SetValue<float>(ref this._PveY, value, "PveY");
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000047 RID: 71 RVA: 0x00003035 File Offset: 0x00001235
		// (set) Token: 0x06000048 RID: 72 RVA: 0x0000303D File Offset: 0x0000123D
		public float PveZ
		{
			get
			{
				return this._PveZ;
			}
			set
			{
				this.SetValue<float>(ref this._PveZ, value, "PveZ");
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00003051 File Offset: 0x00001251
		// (set) Token: 0x0600004A RID: 74 RVA: 0x00003059 File Offset: 0x00001259
		public float PveZoneRadius
		{
			get
			{
				return this._PveZoneRadius;
			}
			set
			{
				this.SetValue<float>(ref this._PveZoneRadius, value, "PveZoneRadius");
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600004B RID: 75 RVA: 0x0000306D File Offset: 0x0000126D
		// (set) Token: 0x0600004C RID: 76 RVA: 0x00003075 File Offset: 0x00001275
		public float RaycastLimit
		{
			get
			{
				return this._RaycastLimit;
			}
			set
			{
				this.SetValue<float>(ref this._RaycastLimit, value, "RaycastLimit");
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00003089 File Offset: 0x00001289
		// (set) Token: 0x0600004E RID: 78 RVA: 0x00003091 File Offset: 0x00001291
		public float TimerMinDelay
		{
			get
			{
				return this._TimerMinDelay;
			}
			set
			{
				this.SetValue<float>(ref this._TimerMinDelay, value, "TimerMinDelay");
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600004F RID: 79 RVA: 0x000030A5 File Offset: 0x000012A5
		// (set) Token: 0x06000050 RID: 80 RVA: 0x000030AD File Offset: 0x000012AD
		public bool DisableTrigNow
		{
			get
			{
				return this._DisableTrigNow;
			}
			set
			{
				this.SetValue<bool>(ref this._DisableTrigNow, value, "DisableTrigNow");
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000030C1 File Offset: 0x000012C1
		// (set) Token: 0x06000052 RID: 82 RVA: 0x000030C9 File Offset: 0x000012C9
		public bool Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				this.SetValue<bool>(ref this._Enabled, value, "Enabled");
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000053 RID: 83 RVA: 0x000030DD File Offset: 0x000012DD
		// (set) Token: 0x06000054 RID: 84 RVA: 0x000030E5 File Offset: 0x000012E5
		public bool BeaconAlert
		{
			get
			{
				return this._BeaconAlert;
			}
			set
			{
				this.SetValue<bool>(ref this._BeaconAlert, value, "BeaconAlert");
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000055 RID: 85 RVA: 0x000030F9 File Offset: 0x000012F9
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00003101 File Offset: 0x00001301
		public bool RemoveMass
		{
			get
			{
				return this._RemoveMass;
			}
			set
			{
				this.SetValue<bool>(ref this._RemoveMass, value, "RemoveMass");
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00003115 File Offset: 0x00001315
		// (set) Token: 0x06000058 RID: 88 RVA: 0x0000311D File Offset: 0x0000131D
		public List<ulong> Mods
		{
			get
			{
				return this._Mods;
			}
			set
			{
				this.SetValue<List<ulong>>(ref this._Mods, value, "Mods");
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00003131 File Offset: 0x00001331
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00003139 File Offset: 0x00001339
		public List<string> BeaconSubTypes
		{
			get
			{
				return this._BeaconSubTypes;
			}
			set
			{
				this.SetValue<List<string>>(ref this._BeaconSubTypes, value, "BeaconSubTypes");
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600005B RID: 91 RVA: 0x0000314D File Offset: 0x0000134D
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00003155 File Offset: 0x00001355
		public string RedAlertText
		{
			get
			{
				return this._RedAlertText;
			}
			set
			{
				this.SetValue<string>(ref this._RedAlertText, value, "RedAlertText");
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003169 File Offset: 0x00001369
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00003171 File Offset: 0x00001371
		public string WithOutBeaconText
		{
			get
			{
				return this._WithOutBeaconText;
			}
			set
			{
				this.SetValue<string>(ref this._WithOutBeaconText, value, "WithOutBeaconText");
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00003185 File Offset: 0x00001385
		// (set) Token: 0x06000060 RID: 96 RVA: 0x0000318D File Offset: 0x0000138D
		public string WithDefaultNameText
		{
			get
			{
				return this._WithDefaultNameText;
			}
			set
			{
				this.SetValue<string>(ref this._WithDefaultNameText, value, "WithDefaultNameText");
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000061 RID: 97 RVA: 0x000031A1 File Offset: 0x000013A1
		// (set) Token: 0x06000062 RID: 98 RVA: 0x000031A9 File Offset: 0x000013A9
		public bool ShipToolsEnabled
		{
			get
			{
				return this._ShipToolsEnabled;
			}
			set
			{
				this.SetValue<bool>(ref this._ShipToolsEnabled, value, "ShipToolsEnabled");
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000063 RID: 99 RVA: 0x000031BD File Offset: 0x000013BD
		// (set) Token: 0x06000064 RID: 100 RVA: 0x000031C5 File Offset: 0x000013C5
		public List<string> ShipTools
		{
			get
			{
				return this._ShipTools;
			}
			set
			{
				this.SetValue<List<string>>(ref this._ShipTools, value, "ShipTools");
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000065 RID: 101 RVA: 0x000031D9 File Offset: 0x000013D9
		// (set) Token: 0x06000066 RID: 102 RVA: 0x000031E1 File Offset: 0x000013E1
		public bool CheckForUpdates
		{
			get
			{
				return this._CheckForUpdates;
			}
			set
			{
				this.SetValue<bool>(ref this._CheckForUpdates, value, "CheckForUpdates");
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000067 RID: 103 RVA: 0x000031F5 File Offset: 0x000013F5
		// (set) Token: 0x06000068 RID: 104 RVA: 0x000031FD File Offset: 0x000013FD
		[XmlIgnore]
		public decimal GridColisionAverage
		{
			get
			{
				return this._gridColisionAverage;
			}
			set
			{
				this.SetValue<decimal>(delegate(decimal x)
				{
					this._gridColisionAverage = x;
				}, value, "GridColisionAverage");
			}
		}

		// Token: 0x0400001A RID: 26
		private float _RaycastLimit = 15000f;

		// Token: 0x0400001B RID: 27
		private float _TimerMinDelay = 3f;

		// Token: 0x0400001C RID: 28
		private bool _DisableTrigNow;

		// Token: 0x0400001D RID: 29
		private bool _Enabled;

		// Token: 0x0400001E RID: 30
		private bool _CheckForUpdates = false;

		// Token: 0x0400001F RID: 31
		private bool _ShipToolsEnabled;

		// Token: 0x04000020 RID: 32
		private bool _BeaconAlert;

		// Token: 0x04000021 RID: 33
		private bool _RemoveMass;

		// Token: 0x04000022 RID: 34
		private List<ulong> _Mods = new List<ulong>();

		// Token: 0x04000023 RID: 35
		private List<string> _ShipTools = new List<string>();

		// Token: 0x04000024 RID: 36
		private List<string> _BeaconSubTypes = new List<string>();

		// Token: 0x04000025 RID: 37
		private string _RedAlertText = "ВНИМАНИЕ!: эта сетка будет удалена при автоматической очистке! \n Чтобы избежать этого, исправьте следующее: \nWARNING: This grid will be deleted on automated cleanup! To avoid this, fix the following:";

		// Token: 0x04000026 RID: 38
		private string _WithOutBeaconText = "  * На данном гриде не установлен блок маяка. Что бы его не удалило, установите МАЯК!.\nGrid does not have a beacon.  Build one to avoid deletion.\n";

		// Token: 0x04000027 RID: 39
		private string _WithDefaultNameText = "  * Переименуйте Грид в панели инфо \"Наименование\"! В нем не должно быть содеражние имени \"Grid\" \nName your grid in the Control Panel Info Tab. It cannot have \"Grid\" in the name.\n";

		// Token: 0x04000028 RID: 40
		private bool _PveZoneEnabled;

		// Token: 0x04000029 RID: 41
		private float _PveX;

		// Token: 0x0400002A RID: 42
		private float _PveY;

		// Token: 0x0400002B RID: 43
		private float _PveZ;

		// Token: 0x0400002C RID: 44
		private float _PveZoneRadius = 500000f;

		// Token: 0x0400002D RID: 45
		private string _PveMessageEntered = "Your grid [{0}] entered to [PVE Zone]! All Weapons on grid are [disabled]";

		// Token: 0x0400002E RID: 46
		private string _PveMessageLeft = "Youк grid [{0}] left from [PVE Zone]!";

		// Token: 0x0400002F RID: 47
		private int _DrillUpdateRate = 90;

		// Token: 0x04000030 RID: 48
		private DrillingMode _ParallelDrill;

		// Token: 0x04000031 RID: 49
		private bool _DrillDisableRightClick;

		// Token: 0x04000032 RID: 50
		private bool _DrillStoneDumpRightClick;

		// Token: 0x04000033 RID: 51
		private bool _DrillIgnoreSubtypes = true;

		// Token: 0x04000034 RID: 52
		private List<string> _DrillsSettings = new List<string>();

		// Token: 0x04000035 RID: 53
		private bool _ProtectVoxels;

		// Token: 0x04000036 RID: 54
		private bool _ProtectGrid;

		// Token: 0x04000037 RID: 55
		private float _MinProtectSpeed = 40f;

		// Token: 0x04000038 RID: 56
		private long _MaxProtectedSmallGridSize = 6000L;

		// Token: 0x04000039 RID: 57
		private long _MaxProtectedLargeGridSize = 6000L;

		// Token: 0x0400003A RID: 58
		private bool _damageThreading = false;

		// Token: 0x0400003B RID: 59
		private decimal _gridColisionAverage;
	}
}
