namespace LibraryGame.Core
{
    public static class Constants
    {
        public const string AppName = "Library Game";

        public static class Scenes
        {
            public const string Boot = "00_Boot";
            public const string Auth = "01_Auth";
            public const string MainHub = "02_MainHub";
            public const string Loading = "03_Loading";
        }

        public static class Firestore
        {
            public const string Users = "users";
            public const string Books = "books";
            public const string Reviews = "reviews";
            public const string OwnedItems = "ownedItems";
            public const string RoomLayouts = "roomLayouts";
            public const string Achievements = "achievements";
            public const string SeasonPassProgress = "seasonPassProgress";
            public const string Catalog = "catalog";
            public const string BookCache = "bookCache";
        }
    }
}
