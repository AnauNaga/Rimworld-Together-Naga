﻿using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    [Serializable]
    public class MarketFile
    {
        public List<ItemData> MarketStock = new List<ItemData>();
    }
}