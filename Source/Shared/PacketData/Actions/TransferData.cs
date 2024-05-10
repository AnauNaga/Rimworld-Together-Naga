using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators

namespace Shared
{
    [Serializable]
    public class TradeData
    {
        public TradePacketType tradePacketType;

        public string transferMode;

        public int fromTile;

        public int toTile;

        public List<byte[]> humanDatas = new List<byte[]>();

        public List<byte[]> animalDatas = new List<byte[]>();

        public List<byte[]> itemDatas = new List<byte[]>();
    }
}
