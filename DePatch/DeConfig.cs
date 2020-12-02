using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Torch;
using Torch.Collections;
using Torch.Views;

namespace DePatch
{
    public class DisplayTab : Torch.Views.DisplayAttribute
    {
        public string Tab = "";
        public bool LiveUpdate = false;
    }

    public class DeConfig : ViewModel
    {
        private float _RaycastLimit = 15000f;
        private float _TimerMinDelay = 3f;
        private bool _DisableTrigNow;
        private bool _DisableAssemblerCoop;
        private bool _DisableAssemblerLoop;
        private bool _DisableProductionOnShip;
        private bool _DisableNanoBotsOnShip;
        private bool _Enabled;
        private bool _CheckForUpdates;
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
        private string _PveMessageLeft = "Your grid [{0}] left from [PVE Zone]! All Weapons can fire now.";
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
        private float _DamgeToBlocksVoxel = 0f;
        private float _DamgeToBlocksRamming = 200f;
        private long _MaxBlocksDoDamage = 50L;
        private bool _damageThreading;
        private decimal _gridColisionAverage;
        private bool _slowPBUpdateEnable;
        private int _slowPBUpdate1 = 2;
        private int _slowPBUpdate10 = 4;
        private int _slowPBUpdate100 = 2;
        private string _slowPBIgnored = "";
        private bool _enableBlockDisabler;
        private bool _AllowFactions;
        private MtObservableList<string> _targetedBlocks = new MtObservableList<string>();
        private MtObservableList<string> _exemptedFactions = new MtObservableList<string>();

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

        public bool ProtectGrid
        {
            get => _ProtectGrid;
            set => SetValue(ref _ProtectGrid, value, "ProtectGrid");
        }

        public bool ProtectVoxels
        {
            get => _ProtectVoxels;
            set => SetValue(ref _ProtectVoxels, value, "ProtectVoxels");
        }

        public bool StopExplosion
        {
            get => _stopExplosion;
            set => SetValue(ref _stopExplosion, value, "StopExplosion");
        }

        public float DamgeToBlocksVoxel
        {
            get => _DamgeToBlocksVoxel;
            set => SetValue(ref _DamgeToBlocksVoxel, value, "DamgeToBlocksVoxel");
        }

        public float DamgeToBlocksRamming
        {
            get => _DamgeToBlocksRamming;
            set => SetValue(ref _DamgeToBlocksRamming, value, "DamgeToBlocksRamming");
        }

        public long MaxBlocksDoDamage
        {
            get => _MaxBlocksDoDamage;
            set => SetValue(ref _MaxBlocksDoDamage, value, "MaxBlocksDoDamage");
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

        public bool DisableProductionOnShip
        {
            get => _DisableProductionOnShip;
            set => SetValue(ref _DisableProductionOnShip, value, "DisableProductionOnShip");
        }

        public bool DisableNanoBotsOnShip
        {
            get => _DisableNanoBotsOnShip;
            set => SetValue(ref _DisableNanoBotsOnShip, value, "DisableNanoBotsOnShip");
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

        public bool CheckForUpdates
        {
            get => _CheckForUpdates;
            set => SetValue(ref _CheckForUpdates, value, "CheckForUpdates");
        }

        [XmlIgnore]
        public decimal GridColisionAverage
        {
            get => _gridColisionAverage;
            set => SetValue(x => _gridColisionAverage = x, value, "GridColisionAverage");
        }

        public bool SlowPBEnabled
        {
            get => _slowPBUpdateEnable;
            set => SetValue(ref _slowPBUpdateEnable, value, "SlowPBEnabled");
        }

        public int SlowPBUpdate1
        {
            get => _slowPBUpdate1;
            set => SetValue(ref _slowPBUpdate1, Math.Max(1, value), "SlowPBUpdate1");
        }

        public int SlowPBUpdate10
        {
            get => _slowPBUpdate10;
            set => SetValue(ref _slowPBUpdate10, Math.Max(1, value), "SlowPBUpdate10");
        }

        public int SlowPBUpdate100
        {
            get => _slowPBUpdate100;
            set => SetValue(ref _slowPBUpdate100, Math.Max(1, value), "SlowPBUpdate100");
        }

        public string SlowPBIgnored
        {
            get => _slowPBIgnored;
            set => SetValue(ref _slowPBIgnored, value, "SlowPBIgnored");
        }

        public bool EnableBlockDisabler
        {
            get => _enableBlockDisabler;
            set => SetValue(ref _enableBlockDisabler, value, "EnableBlockDisabler");
        }

        public bool AllowFactions
        {
            get => _AllowFactions;
            set => SetValue(ref _AllowFactions, value, "AllowFactions");
        }

        [XmlIgnore]
        public MtObservableList<string> TargetedBlocks
        {
            get
            {
                return _targetedBlocks;
            }
            set => SetValue(ref _targetedBlocks, value, "TargetedBlocks");
        }

        [XmlArray("TargetedBlocks")]
        [XmlArrayItem("TargetedBlocks", ElementName = "Block Types/Subtypes")]
        public string[] TargetedBlocksSerial
        {
            get => TargetedBlocks.ToArray();
            set
            {
                TargetedBlocks.Clear();
                if (value != null)
                {
                    foreach (string i in value)
                    {
                        TargetedBlocks.Add(i);
                    }
                }
            }
        }

        [XmlIgnore]
        public MtObservableList<string> ExemptedFactions
        {
            get
            {
                return _exemptedFactions;
            }
            set => SetValue(ref _exemptedFactions, value, "ExemptedFactions");
        }

        [XmlArray("ExemptedFactions")]
        [XmlArrayItem("ExemptedFactions", ElementName = "Faction Tags")]
        public string[] ExemptedFactionsSerial
        {
            get => ExemptedFactions.ToArray();
            set
            {
                ExemptedFactions.Clear();
                if (value != null)
                {
                    foreach (string i in value)
                    {
                        ExemptedFactions.Add(i);
                    }
                }
            }
        }
    }
}
