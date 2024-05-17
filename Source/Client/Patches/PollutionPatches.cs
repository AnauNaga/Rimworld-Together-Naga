using HarmonyLib;
using Verse;
using Shared;

namespace GameClient.Patches
{
    [HarmonyPatch(typeof(WorldPollutionUtility), "PolluteWorldAtTile")]
    public class PollutionPatches
    {
        [HarmonyPrefix]
        public static bool Prefix(int __root, float __pollutionAmount) 
        {
            WorldUpdateData worldupdateData = new WorldUpdateData();
            worldupdateData.tile = __root;
            worldupdateData.pollutionAmount = __pollutionAmount;
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.WorldUpdatePacket),worldupdateData);
            Network.listener.EnqueuePacket(packet);
            return true;
        }
    }
}
