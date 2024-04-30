using System;

namespace Shared
{
    [Serializable]

    public class ItemData
    {
        public string defName;
        public string materialDefName;
        public int quantity;
        public string quality;

        public bool isMinified;
        public int hitpoints;

        public string[] position;
        public int rotation;

        public float growthTicks;

        //Art

        public bool hasArt;

        public string authorName;

        public string title;

        public string taleDefName;

        public string artTale;
    }
}