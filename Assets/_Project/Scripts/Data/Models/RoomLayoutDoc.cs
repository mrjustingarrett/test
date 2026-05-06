using System;
using System.Collections.Generic;

namespace LibraryGame.Data.Models
{
    [Serializable]
    public sealed class RoomLayoutDoc
    {
        public string layoutId;
        public string templateId;
        public string name;
        public bool isActive;
        public List<Placement> placements = new();
        public DateTime updatedAt;
        public int schemaVersion = 1;

        [Serializable]
        public sealed class Placement
        {
            public string itemId;
            public string variantId;
            public Vector3 position;
            public float rotationY;
            public GridCell gridCell;
            public float scale = 1f;
        }

        [Serializable]
        public struct Vector3
        {
            public float x, y, z;
        }

        [Serializable]
        public struct GridCell
        {
            public int x, z;
        }
    }
}
