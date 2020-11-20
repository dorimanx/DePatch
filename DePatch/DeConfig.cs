using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Torch;
using Torch.Views;

namespace DePatch
{
    public class DeConfig : ViewModel
    {
        private float _RaycastLimit = 15000f;
        private float _TimerMinDelay = 3f;
        private bool _DisableTrigNow;
        private bool _DisableAssemblerCoop;
        private bool _DisableAssemblerLoop;
        private bool _Enabled;
        private bool _CheckForUpdates = false;
        private bool _ShipToolsEnabled;
        private bool _BeaconAlert;
        private bool _RemoveMass;
        private List<ulong> _Mods = new List<ulong>();
        private List<string> _ShipTools = new List<string>();
        private List<string> _BeaconSubTypes = new List<string>();
        private string _RedAlertText = "ВНИМАНИЕ!: эта сетка будет удалена при автоматической очистке! \n Чтобы избежать этого, исправьте следующее: \nWARNING: This grid will be deleted on automated cleanup! To avoid this, fix the following:";
        private string _WithOutBeaconText = "  * На данном гриде не установлен блок маяка. Что бы его не удалило, установите МАЯК!.\nGrid does not have a beacon.  Build one to avoid deletion.\n";
        private string _WithDefaultNameText = "  * Переименуйте Грид в панели инфо \"Наименование\"! В нем не должно быть содеражние имени \"Grid\" \nName your grid in the Control Panel Info Tab. It cannot have \"Grid\" in the name.\n";
        private bool _PveZoneEnabled;
        private float _PveX;
        private float _PveY;
        private float _PveZ;
        private float _PveZoneRadius = 500000f;
        private string _PveMessageEntered = "Your grid [{0}] entered to [PVE Zone]! All Weapons on grid cannot fire!";
        private string _PveMessageLeft = "Youк grid [{0}] left from [PVE Zone]! All Weapons can fire now.";
        private int _DrillUpdateRate = 90;
        private DrillingMode _ParallelDrill;
        private bool _DrillDisableRightClick;
        private bool _DrillStoneDumpRightClick;
        private bool _DrillIgnoreSubtypes = true;
        private List<string> _DrillsSettings = new List<string>();
        private bool _ProtectVoxels;
        private bool _stopExplosion = true;
        private bool _ProtectGrid;
        private float _MinProtectSpeed = 40f;
        private long _MaxProtectedSmallGridSize = 10000L;
        private long _MaxProtectedLargeGridSize = 10000L;
        private bool _damageThreading;
        private Decimal _gridColisionAverage;

        [Display(Name = "DamageThreading", Description = "Right now it's not working! just keep it OFF")]
        public bool DamageThreading
        {
            get => _damageThreading;
            set => SetValue(ref _damageThreading, value, "DamageThreading");
        }

        public long MaxProtectedLargeGridSize
        {
            get => _MaxProtectedLargeGridSize;
            set => SetValue(ref _MaxProtectedLargeGridSize, value, "MaxProtectedLargeGridSize");
        }

        public long MaxProtectedSmallGridSize
        {
            get => _MaxProtectedSmallGridSize;
            set => SetValue(ref _MaxProtectedSmallGridSize, value, "MaxProtectedSmallGridSize");
        }

        public float MinProtectSpeed
        {
            get => _MinProtectSpeed;
            set => SetValue(ref _MinProtectSpeed, value, "MinProtectSpeed");
        }

        [Display(Name = "ProtectGrid", Description = "Prevents damage to grids from voxels or ramming by other grids")]
        public bool ProtectGrid
        {
            get => _ProtectGrid;
            set => SetValue(ref _ProtectGrid, value, "ProtectGrid");
        }

        [Display(Name = "ProtectVoxels", Description = "Prevents damage to voxels from impacts with grids")]
        public bool ProtectVoxels
        {
            get => _ProtectVoxels;
            set => SetValue(ref _ProtectVoxels, value, "ProtectVoxels");
        }

        [Display(Name = "Stop Explosion", Description = "Prevents damage to voxels from missile and warhead explosions")]
        public bool StopExplosion
        {
            get => _stopExplosion;
            set => SetValue(ref _stopExplosion, value, "StopExplosion");
        }

        public bool DisableAssemblerCoop
        {
            get => _DisableAssemblerCoop;
            set => SetValue(ref _DisableAssemblerCoop, value, "DisableAssemblerCoop");
        }
        public bool DisableAssemblerLoop
        {
            get => _DisableAssemblerLoop;
            set => SetValue(ref _DisableAssemblerLoop, value, "DisableAssemblerLoop");
        }

        public List<string> DrillsSettings
        {
            get => _DrillsSettings;
            set => SetValue(ref _DrillsSettings, value, "DrillsSettings");
        }

        public bool DrillIgnoreSubtypes
        {
            get => !_DrillIgnoreSubtypes;
            set => SetValue(ref _DrillIgnoreSubtypes, !value, "DrillIgnoreSubtypes");
        }

        public bool DrillStoneDumpRightClick
        {
            get => _DrillStoneDumpRightClick;
            set => SetValue(ref _DrillStoneDumpRightClick, value, "DrillStoneDumpRightClick");
        }

        public bool DrillDisableRightClick
        {
            get => _DrillDisableRightClick;
            set => SetValue(ref _DrillDisableRightClick, value, "DrillDisableRightClick");
        }

        public DrillingMode ParallelDrill
        {
            get => _ParallelDrill;
            set => SetValue(ref _ParallelDrill, value, "ParallelDrill");
        }

        public int DrillUpdateRate
        {
            get => _DrillUpdateRate;
            set => SetValue(ref _DrillUpdateRate, value, "DrillUpdateRate");
        }

        public string PveMessageEntered
        {
            get => _PveMessageEntered;
            set => SetValue(ref _PveMessageEntered, value, "PveMessageEntered");
        }

        public string PveMessageLeft
        {
            get => _PveMessageLeft;
            set => SetValue(ref _PveMessageLeft, value, "PveMessageLeft");
        }

        public bool PveZoneEnabled
        {
            get => _PveZoneEnabled;
            set => SetValue(ref _PveZoneEnabled, value, "PveZoneEnabled");
        }

        public float PveX
        {
            get => _PveX;
            set => SetValue(ref _PveX, value, "PveX");
        }

        public float PveY
        {
            get => _PveY;
            set => SetValue(ref _PveY, value, "PveY");
        }

        public float PveZ
        {
            get => _PveZ;
            set => SetValue(ref _PveZ, value, "PveZ");
        }

        public float PveZoneRadius
        {
            get => _PveZoneRadius;
            set => SetValue(ref _PveZoneRadius, value, "PveZoneRadius");
        }

        public float RaycastLimit
        {
            get => _RaycastLimit;
            set => SetValue(ref _RaycastLimit, value, "RaycastLimit");
        }

        public float TimerMinDelay
        {
            get => _TimerMinDelay;
            set => SetValue(ref _TimerMinDelay, value, "TimerMinDelay");
        }

        public bool DisableTrigNow
        {
            get => _DisableTrigNow;
            set => SetValue(ref _DisableTrigNow, value, "DisableTrigNow");
        }

        public bool Enabled
        {
            get => _Enabled;
            set => SetValue(ref _Enabled, value, "Enabled");
        }

        public bool BeaconAlert
        {
            get => _BeaconAlert;
            set => SetValue(ref _BeaconAlert, value, "BeaconAlert");
        }

        public bool RemoveMass
        {
            get => _RemoveMass;
            set => SetValue(ref _RemoveMass, value, "RemoveMass");
        }

        public List<ulong> Mods
        {
            get => _Mods;
            set => SetValue(ref _Mods, value, "Mods");
        }

        public List<string> BeaconSubTypes
        {
            get => _BeaconSubTypes;
            set => SetValue(ref _BeaconSubTypes, value, "BeaconSubTypes");
        }

        public string RedAlertText
        {
            get => _RedAlertText;
            set => SetValue(ref _RedAlertText, value, "RedAlertText");
        }

        public string WithOutBeaconText
        {
            get => _WithOutBeaconText;
            set => SetValue(ref _WithOutBeaconText, value, "WithOutBeaconText");
        }

        public string WithDefaultNameText
        {
            get => _WithDefaultNameText;
            set => SetValue(ref _WithDefaultNameText, value, "WithDefaultNameText");
        }

        public bool ShipToolsEnabled
        {
            get => _ShipToolsEnabled;
            set => SetValue(ref _ShipToolsEnabled, value, "ShipToolsEnabled");
        }

        public List<string> ShipTools
        {
            get => _ShipTools;
            set => SetValue(ref _ShipTools, value, "ShipTools");
        }

        [Display(Name = "CheckForUpdates", Description = "Right now not working! just keep it OFF")]
        public bool CheckForUpdates
        {
            get => _CheckForUpdates;
            set => SetValue(ref _CheckForUpdates, value, "CheckForUpdates");
        }

        [XmlIgnore]
        public Decimal GridColisionAverage
        {
            get => _gridColisionAverage;
            set => SetValue((Action<Decimal>)(x => _gridColisionAverage = x), value, "GridColisionAverage");
        }
    }
}
