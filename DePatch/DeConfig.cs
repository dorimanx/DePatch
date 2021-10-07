using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using DePatch.BlocksDisable;
using DePatch.ShipTools;
using Torch;

namespace DePatch
{
    public class DeConfig : ViewModel
    {
        private float _raycastLimit = 15000f;
        private float _timerMinDelay = 3f;
        private bool _disableTrigNow;
        private bool _disableAssemblerCoop;
        private bool _disableAssemblerLoop;
        private bool _disableProductionOnShip;
        private bool _disableNanoBotsOnShip;
        private bool _enabled;
        private bool _checkForUpdates;
        private bool _shipToolsEnabled;
        private bool _beaconAlert;
        private bool _removeMass;
        private List<ulong> _mods = new List<ulong>();
        private List<string> _ShipTools = new List<string>();
        private List<string> _beaconSubTypes = new List<string>();
        private string _redAlertText = "ВНИМАНИЕ!: эта сетка будет удалена при автоматической очистке! \n Чтобы избежать этого, исправьте следующее: \nWARNING: This grid will be deleted on automated cleanup! To avoid this, fix the following:";
        private string _withOutBeaconText = "  * На данном гриде не установлен блок маяка. Что бы его не удалило, установите МАЯК!.\nGrid does not have a beacon.  Build one to avoid deletion.\n";
        private string _withDefaultNameText = "  * Переименуйте Грид в панели инфо \"Наименование\"! В нем не должно быть содеражние имени \"Grid\" \nName your grid in the Control Panel Info Tab. It cannot have \"Grid\" in the name.\n";

        private bool _pveZoneEnabled;
        private float _pveX;
        private float _pveY;
        private float _pveZ;
        private float _pveZoneRadius = 500000f;
        private string _pveMessageEntered = "Your grid [{0}] entered to [PVE Zone]! All Weapons on grid cannot fire!";
        private string _pveMessageLeft = "Your grid [{0}] left from [PVE Zone]! All Weapons can fire now.";

        private bool _pveZoneEnabled2;
        private float _pveX2 = -427294.13f;
        private float _pveY2 = 1687093.89f;
        private float _pveZ2 = 2446336.21f;
        private float _pveZoneRadius2 = 500000f;
        private string _pveMessageEntered2 = "Your grid [{0}] entered to [PVE Zone 2]! All Weapons on grid cannot fire!";
        private string _pveMessageLeft2 = "Your grid [{0}] left from [PVE Zone 2]! All Weapons can fire now.";

        private bool _AllowToShootNPCinZone;
        private bool _DelayShootingOnBoot = true;
        private int _DelayShootingOnBootTime = 240;
        private bool _DrillTools = true;
        private int _drillUpdateRate = 150;
        private DrillingMode _parallelDrill;
        private SpeedingMode _speedingModeSelector;
        private bool _drillDisableRightClick;
        private bool _drillIgnoreSubtypes = true;
        private List<string> _DrillsSettings = new List<string>();
        private bool _protectVoxels;
        private bool _stopExplosion = true;
        private bool _protectGrid;
        private float _minProtectSpeed = 40f;
        private float _staticConvertSpeed = 70f;
        private int _maxProtectedSmallGridSize = 10000;
        private int _maxProtectedLargeGridSize = 10000;
        private int _maxGridSizeToConvert = 500;
        private float _damageToBlocksVoxel = 0.00f;
        private float _damageToBlocksRamming = 0.05f;
        private bool _convertToStatic;
        private bool _damageThreading;
        private bool _slowPbUpdateEnable;
        private int _slowPbUpdate1 = 2;
        private int _slowPbUpdate10 = 4;
        private int _slowPbUpdate100 = 2;
        private string _slowPbIgnored = "";
        private bool _enableBlockDisabler;
        private bool _allowFactions;
        private ObservableCollection<string> _targetedBlocks = new ObservableCollection<string>();
        private ObservableCollection<string> _exemptedFactions = new ObservableCollection<string>();
        private bool _enableGridMaxSpeedPurge;
        private float _largeGridMaxSpeedPurge = 500f;
        private float _smallGridMaxSpeedPurge = 500f;
        private bool _AdminGrid;
        private bool _LogTracker;
        private bool _ShieldsAntiHack;
        private bool _PistonInertiaTensor;
        private bool _NoEnemyPlayerLandingGearLocks;
        private bool _NanoBuildArea;
        private bool _NanoDrillArea;
        private bool _ValidationFailedSuspend = true;
        private bool _UpdateAfterSimulation100FIX = true;
        private bool _DenyPlacingBlocksOnEnemyGrid;
        private bool _FixExploits = true;

        public bool DamageThreading
        {
            get => _damageThreading;
            set => SetValue(ref _damageThreading, value);
        }

        public int MaxProtectedLargeGridSize
        {
            get => _maxProtectedLargeGridSize;
            set => SetValue(ref _maxProtectedLargeGridSize, value);
        }

        public int MaxProtectedSmallGridSize
        {
            get => _maxProtectedSmallGridSize;
            set => SetValue(ref _maxProtectedSmallGridSize, value);
        }

        public float MinProtectSpeed
        {
            get => _minProtectSpeed;
            set => SetValue(ref _minProtectSpeed, value);
        }

        public bool ProtectGrid
        {
            get => _protectGrid;
            set => SetValue(ref _protectGrid, value);
        }

        public bool AdminGrid
        {
            get => _AdminGrid;
            set => SetValue(ref _AdminGrid, value);
        }

        public bool ProtectVoxels
        {
            get => _protectVoxels;
            set => SetValue(ref _protectVoxels, value);
        }

        public bool StopExplosion
        {
            get => _stopExplosion;
            set => SetValue(ref _stopExplosion, value);
        }

        public float DamageToBlocksVoxel
        {
            get => _damageToBlocksVoxel;
            set => SetValue(ref _damageToBlocksVoxel, value);
        }

        public float DamageToBlocksRamming
        {
            get => _damageToBlocksRamming;
            set => SetValue(ref _damageToBlocksRamming, value);
        }

        public bool ConvertToStatic
        {
            get => _convertToStatic;
            set => SetValue(ref _convertToStatic, value);
        }

        public int MaxGridSizeToConvert
        {
            get => _maxGridSizeToConvert;
            set => SetValue(ref _maxGridSizeToConvert, value);
        }

        public float StaticConvertSpeed
        {
            get => _staticConvertSpeed;
            set => SetValue(ref _staticConvertSpeed, value);
        }

        public bool DisableAssemblerCoop
        {
            get => _disableAssemblerCoop;
            set => SetValue(ref _disableAssemblerCoop, value);
        }

        public bool DisableAssemblerLoop
        {
            get => _disableAssemblerLoop;
            set => SetValue(ref _disableAssemblerLoop, value);
        }

        public bool DisableProductionOnShip
        {
            get => _disableProductionOnShip;
            set => SetValue(ref _disableProductionOnShip, value);
        }

        public bool DisableNanoBotsOnShip
        {
            get => _disableNanoBotsOnShip;
            set => SetValue(ref _disableNanoBotsOnShip, value);
        }

        public bool DrillTools
        {
            get => _DrillTools;
            set => SetValue(ref _DrillTools, value);
        }

        public List<string> DrillsSettings
        {
            get => _DrillsSettings;
            set => SetValue(ref _DrillsSettings, value);
        }

        public bool DrillIgnoreSubtypes
        {
            get => !_drillIgnoreSubtypes;
            set => SetValue(ref _drillIgnoreSubtypes, !value);
        }

        public bool DrillDisableRightClick
        {
            get => _drillDisableRightClick;
            set => SetValue(ref _drillDisableRightClick, value);
        }

        public DrillingMode ParallelDrill
        {
            get => _parallelDrill;
            set => SetValue(ref _parallelDrill, value);
        }

        public int DrillUpdateRate
        {
            get => _drillUpdateRate;
            set => SetValue(ref _drillUpdateRate, value);
        }

        public string PveMessageEntered
        {
            get => _pveMessageEntered;
            set => SetValue(ref _pveMessageEntered, value);
        }

        public string PveMessageLeft
        {
            get => _pveMessageLeft;
            set => SetValue(ref _pveMessageLeft, value);
        }

        public bool PveZoneEnabled
        {
            get => _pveZoneEnabled;
            set => SetValue(ref _pveZoneEnabled, value);
        }

        public float PveX
        {
            get => _pveX;
            set => SetValue(ref _pveX, value);
        }

        public float PveY
        {
            get => _pveY;
            set => SetValue(ref _pveY, value);
        }

        public float PveZ
        {
            get => _pveZ;
            set => SetValue(ref _pveZ, value);
        }

        public float PveZoneRadius
        {
            get => _pveZoneRadius;
            set => SetValue(ref _pveZoneRadius, value);
        }

        public bool DelayShootingOnBoot
        {
            get => _DelayShootingOnBoot;
            set => SetValue(ref _DelayShootingOnBoot, value);
        }

        public int DelayShootingOnBootTime
        {
            get => _DelayShootingOnBootTime;
            set => SetValue(ref _DelayShootingOnBootTime, value);
        }

        public float RaycastLimit
        {
            get => _raycastLimit;
            set => SetValue(ref _raycastLimit, value);
        }

        public float TimerMinDelay
        {
            get => _timerMinDelay;
            set => SetValue(ref _timerMinDelay, value);
        }

        public bool DisableTrigNow
        {
            get => _disableTrigNow;
            set => SetValue(ref _disableTrigNow, value);
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetValue(ref _enabled, value);
        }

        public bool BeaconAlert
        {
            get => _beaconAlert;
            set => SetValue(ref _beaconAlert, value);
        }

        public bool RemoveMass
        {
            get => _removeMass;
            set => SetValue(ref _removeMass, value);
        }

        public List<ulong> Mods
        {
            get => _mods;
            set => SetValue(ref _mods, value);
        }

        public List<string> BeaconSubTypes
        {
            get => _beaconSubTypes;
            set => SetValue(ref _beaconSubTypes, value);
        }

        public string RedAlertText
        {
            get => _redAlertText;
            set => SetValue(ref _redAlertText, value);
        }

        public string WithOutBeaconText
        {
            get => _withOutBeaconText;
            set => SetValue(ref _withOutBeaconText, value);
        }

        public string WithDefaultNameText
        {
            get => _withDefaultNameText;
            set => SetValue(ref _withDefaultNameText, value);
        }

        public bool ShipToolsEnabled
        {
            get => _shipToolsEnabled;
            set => SetValue(ref _shipToolsEnabled, value);
        }

        public List<string> ShipTools
        {
            get => _ShipTools;
            set => SetValue(ref _ShipTools, value);
        }

        public bool CheckForUpdates
        {
            get => _checkForUpdates;
            set => SetValue(ref _checkForUpdates, value);
        }

        public bool SlowPbEnabled
        {
            get => _slowPbUpdateEnable;
            set => SetValue(ref _slowPbUpdateEnable, value);
        }

        public int SlowPbUpdate1
        {
            get => _slowPbUpdate1;
            set => SetValue(ref _slowPbUpdate1, Math.Max(1, value));
        }

        public int SlowPbUpdate10
        {
            get => _slowPbUpdate10;
            set => SetValue(ref _slowPbUpdate10, Math.Max(1, value));
        }

        public int SlowPbUpdate100
        {
            get => _slowPbUpdate100;
            set => SetValue(ref _slowPbUpdate100, Math.Max(1, value));
        }

        public string SlowPbIgnored
        {
            get => _slowPbIgnored;
            set => SetValue(ref _slowPbIgnored, value);
        }

        public string PveMessageEntered2
        {
            get => _pveMessageEntered2;
            set => SetValue(ref _pveMessageEntered2, value);
        }

        public string PveMessageLeft2
        {
            get => _pveMessageLeft2;
            set => SetValue(ref _pveMessageLeft2, value);
        }

        public bool PveZoneEnabled2
        {
            get => _pveZoneEnabled2;
            set => SetValue(ref _pveZoneEnabled2, value);
        }

        public float PveX2
        {
            get => _pveX2;
            set => SetValue(ref _pveX2, value);
        }

        public float PveY2
        {
            get => _pveY2;
            set => SetValue(ref _pveY2, value);
        }

        public float PveZ2
        {
            get => _pveZ2;
            set => SetValue(ref _pveZ2, value);
        }

        public float PveZoneRadius2
        {
            get => _pveZoneRadius2;
            set => SetValue(ref _pveZoneRadius2, value);
        }

        public bool AllowToShootNPCinZone
        {
            get => _AllowToShootNPCinZone;
            set => SetValue(ref _AllowToShootNPCinZone, value);
        }

        public bool EnableBlockDisabler
        {
            get => _enableBlockDisabler;
            set => SetValue(ref _enableBlockDisabler, value);
        }

        public bool AllowFactions
        {
            get => _allowFactions;
            set => SetValue(ref _allowFactions, value);
        }

        [XmlArray("TargetedBlocks")]
        [XmlArrayItem("TargetedBlocks", ElementName = "Block Types/Subtypes")]
        public ObservableCollection<string> TargetedBlocks
        {
            get => _targetedBlocks;
            set => SetValue(ref _targetedBlocks, value);
        }

        [XmlArray("ExemptedFactions")]
        [XmlArrayItem("ExemptedFactions", ElementName = "Faction Tags")]
        public ObservableCollection<string> ExemptedFactions
        {
            get => _exemptedFactions;
            set => SetValue(ref _exemptedFactions, value);
        }

        public bool EnableGridMaxSpeedPurge
        {
            get => _enableGridMaxSpeedPurge;
            set => SetValue(ref _enableGridMaxSpeedPurge, value);
        }

        public float LargeGridMaxSpeedPurge
        {
            get => _largeGridMaxSpeedPurge;
            set => SetValue(ref _largeGridMaxSpeedPurge, value);
        }

        public float SmallGridMaxSpeedPurge
        {
            get => _smallGridMaxSpeedPurge;
            set => SetValue(ref _smallGridMaxSpeedPurge, value);
        }

        public SpeedingMode SpeedingModeSelector
        {
            get => _speedingModeSelector;
            set => SetValue(ref _speedingModeSelector, value);
        }

        public bool LogTracker
        {
            get => _LogTracker;
            set => SetValue(ref _LogTracker, value);
        }

        public bool ShieldsAntiHack
        {
            get => _ShieldsAntiHack;
            set => SetValue(ref _ShieldsAntiHack, value);
        }

        public bool PistonInertiaTensor
        {
            get => _PistonInertiaTensor;
            set => SetValue(ref _PistonInertiaTensor, value);
        }

        public bool NoEnemyPlayerLandingGearLocks
        {
            get => _NoEnemyPlayerLandingGearLocks;
            set => SetValue(ref _NoEnemyPlayerLandingGearLocks, value);
        }

        public bool NanoBuildArea
        {
            get => _NanoBuildArea;
            set => SetValue(ref _NanoBuildArea, value);
        }

        public bool NanoDrillArea
        {
            get => _NanoDrillArea;
            set => SetValue(ref _NanoDrillArea, value);
        }

        public bool ValidationFailedSuspend
        {
            get => _ValidationFailedSuspend;
            set => SetValue(ref _ValidationFailedSuspend, value);
        }

        public bool UpdateAfterSimulation100FIX
        {
            get => _UpdateAfterSimulation100FIX;
            set => SetValue(ref _UpdateAfterSimulation100FIX, value);
        }

        public bool DenyPlacingBlocksOnEnemyGrid
        {
            get => _DenyPlacingBlocksOnEnemyGrid;
            set => SetValue(ref _DenyPlacingBlocksOnEnemyGrid, value);
        }

        public bool FixExploits
        {
            get => _FixExploits;
            set => SetValue(ref _FixExploits, value);
        }
    }
}
