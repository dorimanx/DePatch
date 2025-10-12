using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Torch;
using Torch.Managers.PatchManager;
using VRage.Game.Entity;
using VRageMath;
using Torch.API.Managers;
using DePatch.CoolDown;
using Sandbox.ModAPI;
using Sandbox.Game.Entities.Planet;
using VRage.Game;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using VRage.GameServices;
using VRage.Game.ModAPI;

namespace DePatch.PVEZONE
{
    internal static class PVE
    {
        public static readonly Logger Log = LogManager.GetLogger("PVE ZONE");

        public static List<long> EntitiesInZone = new List<long>();
        public static List<long> EntitiesInZone2 = new List<long>();
        public static List<MyPlanet> ServerPlanets = new List<MyPlanet>();
        public static Dictionary<MyPlayer, string> PlanetScanner = new Dictionary<MyPlayer, string>();
        public static Dictionary<string, string> PlayerPlanet = new Dictionary<string, string>();
        private static int TickRadar = 1;
        public const float EARTH_GRAVITY = 9.806652f;
        private static int RandomTimer = 1;

        public static BoundingSphereD PVESphere;
        public static BoundingSphereD PVESphere2;

        public static BoundingSphereD PVPAlien;
        public static BoundingSphereD PVPBarsoom;
        public static BoundingSphereD PVPCaldera;
        public static BoundingSphereD PVPDune;
        public static BoundingSphereD PVPEnigma;
        public static BoundingSphereD PVPEuropa;
        public static BoundingSphereD PVPFaraday;
        public static BoundingSphereD PVPGalatea;
        public static BoundingSphereD PVPGea_Desert;
        public static BoundingSphereD PVICE_Planet;
        public static BoundingSphereD PVPKaelnium;
        public static BoundingSphereD PVPNadrit;
        public static BoundingSphereD PVPOctave;
        public static BoundingSphereD PVPPertam;
        public static BoundingSphereD PVPSaturn;
        public static BoundingSphereD PVPSiberia;
        public static BoundingSphereD PVPStyx;
        public static BoundingSphereD PVPTerra;
        public static BoundingSphereD PVPTitan;
        public static BoundingSphereD PVPTriton;
        public static BoundingSphereD PVPValhalla;

        public static void Init(DePatchPlugin plugin, PatchContext ctx)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            PVESphere = new BoundingSphereD(new Vector3D(plugin.Config.PveX, plugin.Config.PveY, plugin.Config.PveZ), plugin.Config.PveZoneRadius);
            PVESphere2 = new BoundingSphereD(new Vector3D(plugin.Config.PveX2, plugin.Config.PveY2, plugin.Config.PveZ2), plugin.Config.PveZoneRadius2);

            ServerPlanets = MyPlanets.GetPlanets();

            foreach (var Planet in ServerPlanets)
            {
                if (Planet == null)
                    continue;

                if (Planet.Name == "Saturn")
                {
                    PVPSaturn = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 60000);
                }
                else if (Planet.Name == "PVP_Terra")
                {
                    PVPTerra = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Enigma_FreeLancers")
                {
                    PVPEnigma = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Pirates_Home")
                {
                    PVPKaelnium = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Siberia_FreeLancers")
                {
                    PVPSiberia = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Styx")
                {
                    PVPStyx = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Triton_Traders")
                {
                    PVPTriton = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Faradium_Planet")
                {
                    PVPFaraday = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Octave")
                {
                    PVPOctave = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Dune")
                {
                    PVPDune = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Titan_Pirates")
                {
                    PVPTitan = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_GalateaMoon")
                {
                    PVPGalatea = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_IcePlanet")
                {
                    PVICE_Planet = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Valhalla")
                {
                    PVPValhalla = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Barsoom")
                {
                    PVPBarsoom = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Gea_Desert_Traders")
                {
                    PVPGea_Desert = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Alien_Pirates")
                {
                    PVPAlien = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Europa_Traders")
                {
                    PVPEuropa = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Nadrid")
                {
                    PVPNadrit = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Caldera_Pirates")
                {
                    PVPCaldera = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
                else if (Planet.Name == "PVP_Pertam_Desert_Empire")
                {
                    PVPPertam = new BoundingSphereD(new Vector3D(Planet.PositionComp.GetPosition()), Planet.Provider.Radius + 40000);
                }
            }

            DamageHandler.Patch(ctx);
            Log.Info("Initing PVE ZONE... Complete!");
        }

        public static void SendMessage(string message, ulong targetSteamId = 0, Color color = default)
        {
            ChatMessageCustomData customData = new ChatMessageCustomData
            {
                AuthorName = "PVP Radar",
                SenderId = targetSteamId,
                TextColor = new Color?(color == default ? Color.Gold : color)
            };

            MyMultiplayer.Static.SendChatMessage(message, ChatChannel.Global, 0, new ChatMessageCustomData?(customData));

#pragma warning disable 618
            //TorchBase.Instance.CurrentSession.Managers.GetManager<IChatManagerServer>()
            //    ?.SendMessageAsOther("PVP Radar", message, color == default ? Color.Gold : color, targetSteamId);
#pragma warning restore 618
        }

        public static void AlertInPVPZone()
        {
            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return;

            if (!DePatchPlugin.Instance.Config.PVPRadar)
                return;

            if (++TickRadar > 60)
            {
                // loop for 120 sec and send PVP ping to chat.
                _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopRadarRequestID, null, out var remainingSecondsToNextRadarPing);

                if (remainingSecondsToNextRadarPing < 1)
                {
                    int LoopCooldown = 120 * 1000;
                    // arm new timer.
                    if (RandomTimer == 1)
                    {
                        LoopCooldown = 120 * 1000;
                        RandomTimer = 2;
                    }
                    else if (RandomTimer == 2)
                    {
                        LoopCooldown = 30 * 1000;
                        RandomTimer = 3;
                    }
                    else if (RandomTimer == 3)
                    {
                        LoopCooldown = 60 * 1000;
                        RandomTimer = 4;
                    }
                    else if (RandomTimer == 4)
                    {
                        LoopCooldown = 90 * 1000;
                        RandomTimer = 1;
                    }

                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopRadarRequestID, null, LoopCooldown);

                    var OnlinePlayersList = MySession.Static.Players.GetOnlinePlayers().ToList();

                    foreach (var Player in OnlinePlayersList)
                        CheckEntityInPVPZone(Player);
                }
                TickRadar = 1;
            }
        }

        public static void CheckEntityInPVPZone(MyPlayer myPlayer)
        {
            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return;

            if (myPlayer == null || myPlayer.Character == null)
                return;

            if (myPlayer != default)
            {
                try
                {
                    PlanetScanner.Clear();
                    bool SmallShipPlayer = false;

                    var Entity = myPlayer.Character.Parent.GetTopMostParent();

                    if (Entity is MyCubeGrid PlayerShip)
                    {
                        List<IMyCubeGrid> attachedList = new List<IMyCubeGrid>();
                        List<VRage.ModAPI.IMyEntity> entList = new List<VRage.ModAPI.IMyEntity>();
                        List<VRage.ModAPI.IMyEntity> GridsEntList = new List<VRage.ModAPI.IMyEntity>();
                        double radius = 2000;

                        MyAPIGateway.GridGroups.GetGroup(PlayerShip, GridLinkTypeEnum.Physical, attachedList);
                        BoundingSphereD sphere = new BoundingSphereD(PlayerShip.PositionComp.GetPosition(), radius);
                        entList = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);

                        foreach (VRage.ModAPI.IMyEntity ent in entList)
                        {
                            if (ent is IMyCubeGrid GridInRange)
                            {
                                // Check if grid is projected
                                MyCubeGrid myEnt = GridInRange as MyCubeGrid;
                                if (myEnt.IsPreview)
                                    continue;

                                // Skip own MainGrid
                                if (attachedList.Contains(GridInRange))
                                    continue;

                                GridsEntList.Add(GridInRange);
                            }
                        }

                        if (attachedList.Count == 1)
                        {
                            if (GridsEntList.Count == 0)
                            {
                                if (!PlayerShip.IsPreview && PlayerShip.GridSizeEnum == MyCubeSize.Small && PlayerShip.BlocksCount <= 1000 ||
                                    !PlayerShip.IsPreview && PlayerShip.GridSizeEnum == MyCubeSize.Large && PlayerShip.BlocksCount <= 300)
                                    SmallShipPlayer = true;
                            }
                        }
                        else
                        {
                            attachedList.SortNoAlloc((x, y) =>
                            {
                                var GridX = (MyCubeGrid)x;
                                var GridY = (MyCubeGrid)y;

                                return GridX.BlocksCount.CompareTo(GridY.BlocksCount);
                            });
                            attachedList.Reverse();
                            attachedList.SortNoAlloc((x, y) => x.GridSizeEnum.CompareTo(y.GridSizeEnum));

                            foreach (var Grid in attachedList)
                            {
                                var PlayerGrid = (MyCubeGrid)Grid;

                                if (!PlayerGrid.IsPreview && PlayerGrid.GridSizeEnum == MyCubeSize.Small && PlayerGrid.BlocksCount > 1000 ||
                                    !PlayerGrid.IsPreview && PlayerGrid.GridSizeEnum == MyCubeSize.Large && PlayerGrid.BlocksCount > 300)
                                {
                                    SmallShipPlayer = false;
                                    break;
                                }

                                if (!PlayerGrid.IsPreview && PlayerGrid.GridSizeEnum == MyCubeSize.Small && PlayerGrid.BlocksCount <= 1000 ||
                                    !PlayerGrid.IsPreview && PlayerGrid.GridSizeEnum == MyCubeSize.Large && PlayerGrid.BlocksCount <= 300)
                                {
                                    SmallShipPlayer = true;
                                    continue;
                                }
                            }

                            if (GridsEntList.Count > 0)
                                SmallShipPlayer = false;
                        }
                    }

                    if (PVPAlien.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Alien");
                    }
                    if (PVPBarsoom.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Barsoom");
                    }
                    if (PVPCaldera.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Caldera");
                    }
                    if (PVPDune.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Dune");
                    }
                    if (PVPEnigma.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Enigma");
                    }
                    if (PVPEuropa.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Europa");
                    }
                    if (PVPFaraday.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Faraday");
                    }
                    if (PVPGalatea.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Galatea");
                    }
                    if (PVPGea_Desert.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Gea Desert");
                    }
                    if (PVICE_Planet.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "ICE Planet");
                    }
                    if (PVPKaelnium.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Kaelnium");
                    }
                    if (PVPNadrit.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Nadrit");
                    }
                    if (PVPOctave.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Octave");
                    }
                    if (PVPPertam.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Pertam");
                    }
                    if (PVPSaturn.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Saturn");
                    }
                    if (PVPSiberia.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Siberia");
                    }
                    if (PVPStyx.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Styx");
                    }
                    if (PVPTerra.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Terra");
                    }
                    if (PVPTitan.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Titan");
                    }
                    if (PVPTriton.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Triton");
                    }
                    if (PVPValhalla.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                    {
                        PlanetScanner.Add(myPlayer, "Valhalla");
                    }

                    if (PlanetScanner.Count == 0)
                    {
                        if (PlayerPlanet.ContainsKey(myPlayer.DisplayName))
                        {
                            PlayerPlanet.TryGetValue(myPlayer.DisplayName, out string Planet);

                            if (SmallShipPlayer)
                                SendMessage($"Little ship left PVP planet/Маленький корабль покинул PVP планету {Planet}->");
                            else
                                SendMessage($"'{myPlayer.DisplayName}' in big ship left PVP planet {Planet}->\n Игрок на большом корабле покинул PVP планету {Planet}->");

                            PlayerPlanet.Remove(myPlayer.DisplayName);
                        }
                        return;
                    }

                    foreach (var Player in PlanetScanner)
                    {
                        if (Player.Key == null)
                            continue;

                        var gravityVectorTemp = 0.0f;
                        Vector3D position = Player.Key.GetPosition();
                        var gravityVector = MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out gravityVectorTemp);
                        var PlayerGravityCalc = gravityVector.Length() / EARTH_GRAVITY;
                        var PlanetGravity = 0.15f;

                        if (!PlayerPlanet.ContainsKey(Player.Key.DisplayName))
                        {
                            if (!MySession.Static.IsUserAdmin(Player.Key.Id.SteamId))
                            {
                                var PlayerChar = Player.Key.Character;
                                MyPlanet closestPlanet = null;

                                if (PlayerChar != null) 
                                    closestPlanet = MyPlanets.Static.GetClosestPlanet(PlayerChar.PositionComp.GetPosition());

                                if (closestPlanet != null)
                                {
                                    var PlanetObject = closestPlanet.GetObjectBuilder();
                                    MyObjectBuilder_Planet ThisPlanetConfig = (MyObjectBuilder_Planet)PlanetObject;
                                    float PlanetGrav = ThisPlanetConfig.SurfaceGravity;

                                    if (PlayerGravityCalc >= PlanetGrav || PlayerGravityCalc >= PlanetGrav - 0.15f)
                                    {
                                        if (!SmallShipPlayer)
                                            SendMessage($"'{Player.Key.DisplayName}' in big ship arrived to PVP planet ->{Player.Value}\nИгрок на большом корабле прибыл на PVP планету ->{Player.Value}");
                                        else
                                            SendMessage($"Little ship arrived to PVP planet/Маленький корабль прибыл на PVP планету");

                                        PlayerPlanet.Add(Player.Key.DisplayName, Player.Value);
                                    }
                                }
                                else
                                {
                                    if (PlayerGravityCalc >= PlanetGravity)
                                    {
                                        if (!SmallShipPlayer)
                                            SendMessage($"'{Player.Key.DisplayName}' in big ship arrived to PVP planet ->{Player.Value}\nИгрок на большом корабле прибыл на PVP планету ->{Player.Value}");
                                        else
                                            SendMessage($"Little ship arrived to PVP planet/Маленький корабль прибыл на PVP планету");

                                        PlayerPlanet.Add(Player.Key.DisplayName, Player.Value);
                                    }
                                }
                            }
                        }
                        else
                        {
                            PlayerPlanet.TryGetValue(Player.Key.DisplayName, out string PlayerNameOnPlanet);
                            PlanetScanner.TryGetValue(Player.Key, out string PlayerNameDetectedPlanet);

                            if (PlayerNameOnPlanet == PlayerNameDetectedPlanet)
                                return;
                            else
                            {
                                if (SmallShipPlayer)
                                    SendMessage($"Little ship left PVP planet/Маленький корабль покинул PVP планету {PlayerNameOnPlanet}->");
                                else
                                    SendMessage($"'{Player.Key.DisplayName}' in big ship left PVP planet {PlayerNameOnPlanet}->\nИгрок на большом корабле покинул PVP планету {PlayerNameOnPlanet}->");
                                PlayerPlanet.Remove(Player.Key.DisplayName);

                                if (!SmallShipPlayer)
                                    SendMessage($"'{Player.Key.DisplayName}' in big ship arrived to PVP planet ->{Player.Value}\nИгрок на большом корабле прибыл на PVP планету ->{Player.Value}");
                                else
                                    SendMessage($"Little ship arrived to PVP planet/Маленький корабль прибыл на PVP планету");

                                PlayerPlanet.Add(Player.Key.DisplayName, PlayerNameDetectedPlanet);
                            }
                        }
                    }
                }
                catch
                {
                    // not in PVP zones.
                    return;
                }
            }
        }

        public static bool CheckEntityInZone(object obj, ref bool __result)
        {
            var zone1 = false;
            var zone2 = false;
            __result = false;

            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return __result;

            if (obj is MyPlayer myPlayer)
            {
                if (myPlayer == null)
                    return __result;

                if (myPlayer != default)
                {
                    try
                    {
                        if (PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone1 = true;
                        if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && PVESphere2.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                            zone2 = true;
                    }
                    catch
                    {
                        // FALSE. not in PVE zones.
                        return __result;
                    }

                    if (zone1 || zone2)
                    {
                        __result = true;
                        return __result;
                    }

                    // FALSE. not in PVE zones.
                    return __result;
                }
            }
            else if (obj is MyEntity entity)
            {
                if (EntitiesInZone.Contains(entity.EntityId))
                    zone1 = true;
                if (DePatchPlugin.Instance.Config.PveZoneEnabled2 && EntitiesInZone2.Contains(entity.EntityId))
                    zone2 = true;

                if (zone1 || zone2)
                {
                    __result = true;
                    return __result;
                }

                // FALSE. not in PVE zones.
                return __result;
            }

            // FALSE. not in PVE zones.
            return __result;
        }

        public static bool CheckEntityInZone(MyCubeGrid grid)
        {
            bool res = false;
            return CheckEntityInZone(grid, ref res);
        }

        internal static MyPlayer FindOnlineOwner(MyCubeGrid grid)
        {
            var controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
            if (controllingPlayer != null)
                return controllingPlayer;

            var dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);

            if (grid.BigOwners.Count() > 0)
            {
                var owner = grid.BigOwners.FirstOrDefault();
                if (dictionary.ContainsKey(owner))
                    return dictionary[owner];
            }
            return null;
        }
    }
}
