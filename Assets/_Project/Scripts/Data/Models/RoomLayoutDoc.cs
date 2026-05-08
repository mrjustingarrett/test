using System;
using System.Collections.Generic;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class RoomLayoutDoc
    {
        public string layoutId = "main";
        public string templateId = "default_room";
        public string name = "My Library";
        public bool isActive = true;
        public List<Placement> placements = new();
        public long updatedAtUnixMs;
        public int schemaVersion = 1;

        [Serializable]
        public sealed class Placement
        {
            public string itemId;
            public string variantId;
            public float posX;
            public float posY;
            public float posZ;
            public float rotationY;
            public int gridX;
            public int gridZ;
            public float scale = 1f;
        }
    }
}
