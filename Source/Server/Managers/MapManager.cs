﻿using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class MapManager
    {
        //Variables

        public readonly static string fileExtension = ".mpmap";

        public static void SaveUserMap(ServerClient client, Packet packet)
        {
            MapFileData mapFileData = (MapFileData)Serializer.ConvertBytesToObject(packet.contents);
            mapFileData.mapOwner = client.username;

            byte[] compressedMapBytes = Serializer.ConvertObjectToBytes(mapFileData);
            File.WriteAllBytes(Path.Combine(Master.mapsPath, mapFileData.mapTile + fileExtension), compressedMapBytes);

            Logger.Message($"[Save map] > {client.username} > {mapFileData.mapTile}");
        }

        public static void DeleteMap(MapFileData mapFile)
        {
            if (mapFile == null) return;

            File.Delete(Path.Combine(Master.mapsPath, mapFile.mapTile + fileExtension));

            Logger.Warning($"[Remove map] > {mapFile.mapTile}");
        }

        public static MapFileData[] GetAllMapFiles()
        {
            List<MapFileData> mapDatas = new List<MapFileData>();

            string[] maps = Directory.GetFiles(Master.mapsPath);
            foreach (string map in maps)
            {
                if (!map.EndsWith(fileExtension)) continue;
                byte[] decompressedBytes = File.ReadAllBytes(map);

                MapFileData newMap = (MapFileData)Serializer.ConvertBytesToObject(decompressedBytes);
                mapDatas.Add(newMap);
            }

            return mapDatas.ToArray();
        }

        public static bool CheckIfMapExists(string mapTileToCheck)
        {
            MapFileData[] maps = GetAllMapFiles();
            foreach (MapFileData map in maps)
            {
                if (map.mapTile == mapTileToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static MapFileData[] GetAllMapsFromUsername(string username)
        {
            List<MapFileData> userMaps = new List<MapFileData>();

            SettlementFile[] userSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in userSettlements)
            {
                MapFileData mapFile = GetUserMapFromTile(settlementFile.tile);
                userMaps.Add(mapFile);
            }

            return userMaps.ToArray();
        }

        public static MapFileData GetUserMapFromTile(string mapTileToGet)
        {
            MapFileData[] mapFiles = GetAllMapFiles();

            foreach (MapFileData mapFile in mapFiles)
            {
                if (mapFile.mapTile == mapTileToGet) return mapFile;
            }

            return null;
        }
    }
}
