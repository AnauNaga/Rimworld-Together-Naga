using Shared;

namespace GameServer
{
    public static class TransferManager
    {
        public static void ParseTransferPacket(ServerClient client, Packet packet)
        {
            TradeData tradeData = (TradeData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(tradeData.tradePacketType))
            {
                case (int)CommonEnumerators.TransferStepMode.TradeRequest:
                    TransferThings(client, tradeData);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeAccept:
                    //Nothing goes here
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReject:
                    RejectTransfer(client, packet);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReRequest:
                    TransferThingsRebound(client, packet);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReAccept:
                    AcceptReboundTransfer(client, packet);
                    break;

                case (int)CommonEnumerators.TransferStepMode.TradeReReject:
                    RejectReboundTransfer(client, packet);
                    break;
            }
        }

        public static void TransferThings(ServerClient client, TradeData tradeData)
        {
            if (!SettlementManager.CheckIfTileIsInUse(tradeData.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to send items to a settlement at tile {tradeData.toTile}, but no settlement could be found");
            else
            {
                SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(tradeData.toTile);

                if (!UserManager.CheckIfUserIsConnected(settlement.owner))
                {
                    if (int.Parse(tradeData.transferMode) == (int)CommonEnumerators.TransferMode.Pod) ResponseShortcutManager.SendUnavailablePacket(client);
                    else
                    {
                        tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                        client.listener.EnqueuePacket(rPacket);
                    }
                }

                else
                {
                    if (int.Parse(tradeData.transferMode) == (int)CommonEnumerators.TransferMode.Gift)
                    {
                        tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    else if (int.Parse(tradeData.transferMode) == (int)CommonEnumerators.TransferMode.Pod)
                    {
                        tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeAccept).ToString();
                        Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                        client.listener.EnqueuePacket(rPacket);
                    }

                    tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeRequest).ToString();
                    string[] contents2 = new string[] { Serializer.SerializeToString(tradeData) };
                    Packet rPacket2 = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                    UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket2);
                }
            }
        }

        public static void RejectTransfer(ServerClient client, Packet packet)
        {
            TradeData tradeData = (TradeData)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(tradeData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void TransferThingsRebound(ServerClient client, Packet packet)
        {
            TradeData tradeData = (TradeData)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(tradeData.toTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeReRequest).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void AcceptReboundTransfer(ServerClient client, Packet packet)
        {
            TradeData tradeData = (TradeData)Serializer.ConvertBytesToObject(packet.contents);
            
            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(tradeData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeReAccept).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }

        public static void RejectReboundTransfer(ServerClient client, Packet packet)
        {
            TradeData tradeData = (TradeData)Serializer.ConvertBytesToObject(packet.contents);

            SettlementFile settlement = SettlementManager.GetSettlementFileFromTile(tradeData.fromTile);
            if (!UserManager.CheckIfUserIsConnected(settlement.owner))
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.Recover).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                client.listener.EnqueuePacket(rPacket);
            }

            else
            {
                tradeData.tradePacketType = ((int)CommonEnumerators.TransferStepMode.TradeReReject).ToString();
                Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.TransferPacket), tradeData);
                UserManager.GetConnectedClientFromUsername(settlement.owner).listener.EnqueuePacket(rPacket);
            }
        }
    }
}
