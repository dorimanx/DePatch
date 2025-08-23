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

namespace DePatch.PVEZONE
{
    internal static class PVE
    {
        public static readonly Logger Log = LogManager.GetLogger("PVE ZONE");

        public static List<long> EntitiesInZone = new List<long>();
        public static List<long> EntitiesInZone2 = new List<long>();
        public static Dictionary<MyPlayer, string> PlanetScanner = new Dictionary<MyPlayer, string>();
        public static Dictionary<string, string> PlayerPlanet = new Dictionary<string, string>();
        private static int TickRadar = 1;
        public const float EARTH_GRAVITY = 9.806652f;

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

            PVPAlien = new BoundingSphereD(new Vector3D(5217892.6931551006, 3586960.28777134, 24330814.626217317), 160000);
            PVPBarsoom = new BoundingSphereD(new Vector3D(-4487399.1877444545, 9924822.4033073764, -2047436.5433490407), 160000);
            PVPCaldera = new BoundingSphereD(new Vector3D(4423873.9707971066, 3447997.6324882084, 24413094.900133859), 140000);
            PVPDune = new BoundingSphereD(new Vector3D(24588174.166927494, -5100911.1766861351, 5471442.0334872119), 140000);
            PVPEnigma = new BoundingSphereD(new Vector3D(48699129.693050854, 21283754.009524003, 7541964.1015719185), 140000);
            PVPEuropa = new BoundingSphereD(new Vector3D(41137970.008399256, -28924780.373912029, 9528057.5614499226), 130000);
            PVPFaraday = new BoundingSphereD(new Vector3D(3217874.6904728841, 4437198.9240576252, -980661.716303604), 160000);
            PVPGalatea = new BoundingSphereD(new Vector3D(-4839914.8461530115, 9004595.1545535717, -4206229.5598180983), 160000);
            PVPGea_Desert = new BoundingSphereD(new Vector3D(40830069.103547491, -29610859.878449582, 9242811.8954619151), 140000);
            PVICE_Planet = new BoundingSphereD(new Vector3D(478468.08345142053, -886115.97622446285, -5028714.145565345), 140000);
            PVPKaelnium = new BoundingSphereD(new Vector3D(4650948.8135129521, 3881444.7555122869, 24243070.801727097), 130000);
            PVPNadrit = new BoundingSphereD(new Vector3D(2825437.2118936856, 4448763.2586455392, -872866.88817172288), 140000);
            PVPOctave = new BoundingSphereD(new Vector3D(3989238.5409811782, 4793593.0767463958, -3009762.234743767), 160000);
            PVPPertam = new BoundingSphereD(new Vector3D(1699744.1421741291, 1402014.6837624309, -41540.831866881112), 140000);
            PVPSaturn = new BoundingSphereD(new Vector3D(-5075830.4638930392, 9433381.5605102256, -2903876.8130593989), 400000);
            PVPSiberia = new BoundingSphereD(new Vector3D(49550725.786055669, 20648033.55527636, 7093292.6885949271), 160000);
            PVPStyx = new BoundingSphereD(new Vector3D(-4960556.841415735, 9194154.8499497287, -4227386.9396974072), 160000);
            PVPTerra = new BoundingSphereD(new Vector3D(49174670.003773756, 20716503.526764885, 7667419.7965744119), 160000);
            PVPTitan = new BoundingSphereD(new Vector3D(4736792.882427997, 3720394.0052326508, 24437549.840063781), 140000);
            PVPTriton = new BoundingSphereD(new Vector3D(41919144.845246159, -28762305.595447615, 10201790.434855942), 160000);
            PVPValhalla = new BoundingSphereD(new Vector3D(25532376.310819753, 1877057.4612285423, 4580741.4395682905), 160000);

            DamageHandler.Patch(ctx);
            Log.Info("Initing PVE ZONE... Complete!");
        }

        public static void SendMessage(string message, ulong targetSteamId = 0, Color color = default)
        {
#pragma warning disable 618
            TorchBase.Instance.CurrentSession.Managers.GetManager<IChatManagerServer>()
#pragma warning restore 618
                ?.SendMessageAsOther("PVP Radar", message, color == default ? Color.Gold : color, targetSteamId);
        }

        public static void AlertInPVPZone()
        {
            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return;

            if (!DePatchPlugin.Instance.Config.PVPRadar)
                return;

            if (++TickRadar > 60)
            {
                // loop for 180 sec and send PVP ping to chat.
                _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopRadarRequestID, null, out var remainingSecondsToNextRadarPing);

                if (remainingSecondsToNextRadarPing < 1)
                {
                    // arm new timer.
                    int LoopCooldown = 60 * 1000;
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

            if (myPlayer == null)
                return;

            if (myPlayer != default)
            {
                try
                {
                    PlanetScanner.Clear();

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
                            SendMessage($"{myPlayer.DisplayName} left planet/покинул планету {Planet}");
                            PlayerPlanet.Remove(myPlayer.DisplayName);
                        }
                        return;
                    }

                    foreach (var Player in PlanetScanner)
                    {
                        var gravityVectorTemp = 0.0f;
                        Vector3D position = Player.Key.GetPosition();
                        var gravityVector = MyAPIGateway.Physics.CalculateNaturalGravityAt(position, out gravityVectorTemp);
                        var GridGravityCalc = gravityVector.Length() / EARTH_GRAVITY;

                        if (!PlayerPlanet.ContainsKey(Player.Key.DisplayName))
                        {
                            if (!MySession.Static.IsUserAdmin(myPlayer.Id.SteamId))
                            {
                                if (GridGravityCalc > 0.4f)
                                {
                                    SendMessage($"{Player.Key.DisplayName} arrived to planet/прибыл на планету {Player.Value}");
                                    PlayerPlanet.Add(Player.Key.DisplayName, Player.Value);
                                }
                            }
                        }
                        else
                        {
                            PlayerPlanet.TryGetValue(myPlayer.DisplayName, out string PlayerNameOnPlanet);
                            PlanetScanner.TryGetValue(myPlayer, out string PlayerNameDetectedPlanet);

                            if (PlayerNameOnPlanet == PlayerNameDetectedPlanet)
                                return;
                            else
                            {
                                SendMessage($"{Player.Key.DisplayName} left planet/покинул планету {PlayerNameOnPlanet}");
                                SendMessage($"{Player.Key.DisplayName} arrived to planet/прибыл на планету {Player.Value}");
                                PlayerPlanet.Remove(myPlayer.DisplayName);
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
