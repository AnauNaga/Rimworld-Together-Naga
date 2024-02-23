﻿using Shared;

namespace GameServer
{
    public static class SaveManager
    {
        public static void ReceiveSavePartFromClient(ServerClient client, Packet packet)
        {
            string baseClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsavetemp");

            FileTransferJSON fileTransferJSON = (FileTransferJSON)Serializer.ConvertBytesToObject(packet.contents);

            if (client.listener.downloadManager == null)
            {
                client.listener.downloadManager = new DownloadManager();
                client.listener.downloadManager.PrepareDownload(tempClientSavePath, fileTransferJSON.fileParts);
            }

            client.listener.downloadManager.WriteFilePart(fileTransferJSON.fileBytes);

            if (fileTransferJSON.isLastPart)
            {
                client.listener.downloadManager.FinishFileWrite();
                client.listener.downloadManager = null;

                byte[] saveBytes = File.ReadAllBytes(tempClientSavePath);
                byte[] compressedSave = GZip.Compress(saveBytes);

                File.WriteAllBytes(baseClientSavePath, compressedSave);
                File.Delete(tempClientSavePath);

                OnUserSave(client, fileTransferJSON);
            }

            else
            {
                Packet rPacket = Packet.CreatePacketFromJSON("RequestSavePartPacket");
                client.listener.dataQueue.Enqueue(rPacket);
            }
        }

        public static void SendSavePartToClient(ServerClient client)
        {
            string baseClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsave");
            string tempClientSavePath = Path.Combine(Program.savesPath, client.username + ".mpsavetemp");

            if (client.listener.uploadManager == null)
            {
                Logger.WriteToConsole($"[Load save] > {client.username} | {client.SavedIP}");

                byte[] decompressedSave = GZip.Decompress(File.ReadAllBytes(baseClientSavePath));
                File.WriteAllBytes(tempClientSavePath, decompressedSave);

                client.listener.uploadManager = new UploadManager();
                client.listener.uploadManager.PrepareUpload(tempClientSavePath);
            }

            FileTransferJSON fileTransferJSON = new FileTransferJSON();
            fileTransferJSON.fileSize = client.listener.uploadManager.fileSize;
            fileTransferJSON.fileParts = client.listener.uploadManager.fileParts;
            fileTransferJSON.fileBytes = client.listener.uploadManager.ReadFilePart();
            fileTransferJSON.isLastPart = client.listener.uploadManager.isLastPart;

            Packet packet = Packet.CreatePacketFromJSON("ReceiveSavePartPacket", fileTransferJSON);
            client.listener.dataQueue.Enqueue(packet);

            if (client.listener.uploadManager.isLastPart)
            {
                File.Delete(tempClientSavePath);
                client.listener.uploadManager = null;
            }
        }

        private static void OnUserSave(ServerClient client, FileTransferJSON fileTransferJSON)
        {
            if (fileTransferJSON.additionalInstructions == ((int)CommonEnumerators.SaveMode.Disconnect).ToString())
            {
                CommandManager.SendDisconnectCommand(client);

                client.listener.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > To menu");
            }

            else if (fileTransferJSON.additionalInstructions == ((int)CommonEnumerators.SaveMode.Quit).ToString())
            {
                CommandManager.SendQuitCommand(client);

                client.listener.disconnectFlag = true;

                Logger.WriteToConsole($"[Save game] > {client.username} > To desktop");
            }

            else Logger.WriteToConsole($"[Save game] > {client.username} > Autosave");
        }

        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach(string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == client.username)
                {
                    return true;
                }
            }

            return false;
        }

        public static byte[] GetUserSaveFromUsername(string username)
        {
            string[] saves = Directory.GetFiles(Program.savesPath);
            foreach (string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == username)
                {
                    return File.ReadAllBytes(save);
                }
            }

            return null;
        }

        public static void ResetClientSave(ServerClient client)
        {
            if (!CheckIfUserHasSave(client)) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                client.listener.disconnectFlag = true;

                string[] saves = Directory.GetFiles(Program.savesPath);

                string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == client.username);
                if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

                Logger.WriteToConsole($"[Delete save] > {client.username}", Logger.LogMode.Warning);

                MapFileJSON[] userMaps = MapManager.GetAllMapsFromUsername(client.username);
                foreach (MapFileJSON map in userMaps) MapManager.DeleteMap(map);

                SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(client.username);
                foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

                SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(client.username);
                foreach (SettlementFile settlementFile in playerSettlements)
                {
                    SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                    settlementDetailsJSON.tile = settlementFile.tile;
                    settlementDetailsJSON.owner = settlementFile.owner;

                    SettlementManager.RemoveSettlement(client, settlementDetailsJSON);
                }
            }
        }

        public static void DeletePlayerDetails(string username)
        {
            ServerClient connectedUser = UserManager.GetConnectedClientFromUsername(username);
            if (connectedUser != null) connectedUser.listener.disconnectFlag = true;

            string[] saves = Directory.GetFiles(Program.savesPath);
            string toDelete = saves.ToList().Find(x => Path.GetFileNameWithoutExtension(x) == username);
            if (!string.IsNullOrWhiteSpace(toDelete)) File.Delete(toDelete);

            MapFileJSON[] userMaps = MapManager.GetAllMapsFromUsername(username);
            foreach (MapFileJSON map in userMaps) MapManager.DeleteMap(map);

            SiteFile[] playerSites = SiteManager.GetAllSitesFromUsername(username);
            foreach (SiteFile site in playerSites) SiteManager.DestroySiteFromFile(site);

            SettlementFile[] playerSettlements = SettlementManager.GetAllSettlementsFromUsername(username);
            foreach (SettlementFile settlementFile in playerSettlements)
            {
                SettlementDetailsJSON settlementDetailsJSON = new SettlementDetailsJSON();
                settlementDetailsJSON.tile = settlementFile.tile;
                settlementDetailsJSON.owner = settlementFile.owner;

                SettlementManager.RemoveSettlement(null, settlementDetailsJSON, false);
            }

            Logger.WriteToConsole($"[Deleted player details] > {username}", Logger.LogMode.Warning);
        }
    }
}
