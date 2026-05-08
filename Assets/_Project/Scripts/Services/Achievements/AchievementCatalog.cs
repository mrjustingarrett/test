using System.Collections.Generic;

namespace LibraryGame.Services.Achievements
{
    /// <summary>Hardcoded achievement definitions for MVP.</summary>
    public static class AchievementCatalog
    {
        public sealed class Def
        {
            public string id;
            public string title;
            public string description;
            public int target;
            public int rewardSoft;
            public string rewardItemId;     // null = no item
            public TriggerKind trigger;
            public string filter;           // for genre matches etc.
        }

        public enum TriggerKind { BooksFinished, GenreFinished, ItemsPlaced }

        public static readonly List<Def> All = new()
        {
            new() { id="first_book",   title="First Page Turner", description="Finish your first book.",        target=1,  rewardSoft=25,  rewardItemId="oak_bookshelf",  trigger=TriggerKind.BooksFinished },
            new() { id="five_books",   title="Bookworm",          description="Finish 5 books.",                target=5,  rewardSoft=75,  rewardItemId="reading_chair",  trigger=TriggerKind.BooksFinished },
            new() { id="ten_books",    title="Voracious Reader",  description="Finish 10 books.",               target=10, rewardSoft=150, rewardItemId="fireplace",      trigger=TriggerKind.BooksFinished },
            new() { id="twenty_books", title="Library Legend",    description="Finish 20 books.",               target=20, rewardSoft=300, rewardItemId="globe_lamp",     trigger=TriggerKind.BooksFinished },
            new() { id="fantasy_fan",  title="Fantasy Fan",       description="Finish 3 fantasy books.",         target=3,  rewardSoft=60,  rewardItemId="wizard_hat",     trigger=TriggerKind.GenreFinished, filter="fantasy" },
            new() { id="decorator",    title="Interior Designer", description="Place 5 items in your room.",     target=5,  rewardSoft=40,  rewardItemId="painting_large", trigger=TriggerKind.ItemsPlaced },
        };

        public static Def Get(string id) => All.Find(x => x.id == id);
    }
}
