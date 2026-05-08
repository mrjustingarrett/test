using System.Collections.Generic;
using UnityEngine;

namespace LibraryGame.Services.Inventory
{
    /// <summary>
    /// Static catalog of all furniture items. Hardcoded for the MVP — at M8
    /// these become ScriptableObject assets driven by Addressables/RemoteConfig.
    /// </summary>
    public static class FurnitureCatalog
    {
        public enum Shape { Box, Shelf, Chair, Lamp, Tree, Rug, Painting }

        public sealed class Item
        {
            public string id;
            public string displayName;
            public Shape shape;
            public Color color;
            public Vector3 size;        // bounding size in meters (for visual mesh)
            public int priceSoft;       // 0 = not for sale (achievement reward only)
            public bool seasonal;       // shows in shop "Seasonal" section
        }

        public static readonly List<Item> All = new()
        {
            new() { id="starter_shelf",   displayName="Pine Bookshelf",      shape=Shape.Shelf,    color=new(0.62f,0.45f,0.30f), size=new(1.0f,1.8f,0.4f), priceSoft=0 },
            new() { id="oak_bookshelf",   displayName="Oak Bookshelf",       shape=Shape.Shelf,    color=new(0.50f,0.35f,0.20f), size=new(1.2f,2.0f,0.4f), priceSoft=80 },
            new() { id="reading_chair",   displayName="Reading Chair",       shape=Shape.Chair,    color=new(0.62f,0.30f,0.30f), size=new(0.8f,1.0f,0.8f), priceSoft=120 },
            new() { id="cozy_armchair",   displayName="Cozy Armchair",       shape=Shape.Chair,    color=new(0.32f,0.45f,0.35f), size=new(0.9f,1.0f,0.9f), priceSoft=160 },
            new() { id="floor_lamp",      displayName="Floor Lamp",          shape=Shape.Lamp,     color=new(0.95f,0.85f,0.55f), size=new(0.3f,1.6f,0.3f), priceSoft=60 },
            new() { id="rug_red",         displayName="Red Rug",             shape=Shape.Rug,      color=new(0.62f,0.22f,0.22f), size=new(2.4f,0.04f,1.6f), priceSoft=70 },
            new() { id="rug_blue",        displayName="Blue Rug",            shape=Shape.Rug,      color=new(0.20f,0.30f,0.55f), size=new(2.4f,0.04f,1.6f), priceSoft=70 },
            new() { id="painting_small",  displayName="Small Painting",      shape=Shape.Painting, color=new(0.90f,0.70f,0.40f), size=new(0.6f,0.8f,0.05f), priceSoft=40 },
            new() { id="painting_large",  displayName="Large Painting",      shape=Shape.Painting, color=new(0.30f,0.55f,0.65f), size=new(1.2f,0.9f,0.05f), priceSoft=110 },
            new() { id="end_table",       displayName="End Table",           shape=Shape.Box,      color=new(0.45f,0.30f,0.20f), size=new(0.5f,0.5f,0.5f), priceSoft=45 },
            new() { id="coffee_table",    displayName="Coffee Table",        shape=Shape.Box,      color=new(0.40f,0.28f,0.18f), size=new(1.2f,0.45f,0.6f), priceSoft=85 },
            new() { id="desk",            displayName="Writing Desk",        shape=Shape.Box,      color=new(0.55f,0.40f,0.25f), size=new(1.3f,0.75f,0.6f), priceSoft=140 },
            new() { id="globe",           displayName="Antique Globe",       shape=Shape.Lamp,     color=new(0.40f,0.55f,0.65f), size=new(0.4f,0.6f,0.4f), priceSoft=200 },
            new() { id="potted_fern",     displayName="Potted Fern",         shape=Shape.Tree,     color=new(0.30f,0.55f,0.30f), size=new(0.5f,0.9f,0.5f), priceSoft=55 },
            new() { id="big_plant",       displayName="Tall Houseplant",     shape=Shape.Tree,     color=new(0.28f,0.50f,0.32f), size=new(0.8f,1.7f,0.8f), priceSoft=130 },
            new() { id="fireplace",       displayName="Stone Fireplace",     shape=Shape.Box,      color=new(0.55f,0.55f,0.58f), size=new(1.6f,1.6f,0.6f), priceSoft=0 },
            new() { id="xmas_tree",       displayName="Christmas Tree",      shape=Shape.Tree,     color=new(0.20f,0.45f,0.25f), size=new(1.0f,2.2f,1.0f), priceSoft=0, seasonal=true },
            new() { id="pumpkin",         displayName="Carved Pumpkin",      shape=Shape.Lamp,     color=new(0.95f,0.50f,0.10f), size=new(0.4f,0.4f,0.4f), priceSoft=40, seasonal=true },
            new() { id="wizard_hat",      displayName="Wizard Hat (Decor)",  shape=Shape.Lamp,     color=new(0.30f,0.20f,0.55f), size=new(0.4f,0.6f,0.4f), priceSoft=0 },
            new() { id="globe_lamp",      displayName="Globe Lamp",          shape=Shape.Lamp,     color=new(0.95f,0.92f,0.70f), size=new(0.35f,0.55f,0.35f), priceSoft=180 },
        };

        public static Item Get(string id) => All.Find(x => x.id == id);
    }
}
