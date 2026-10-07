using System.IO;
using Kapman.Encounters;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The encounter tables as the tests read them: straight off the disk, loaded once and checked.</summary>
    public static class EncounterData
    {
        const string Data = "Assets/Kapman/Game/Data/";
        static EncounterBook book;

        static string Read(string file)
        {
            string path = Data + file;
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        /// <summary>Every card file in encounters/, the roles, names, words, writing and the cross-check tables.</summary>
        public static EncounterSources Sources()
        {
            var src = new EncounterSources
            {
                roles = Read("encounters/npc_roles_793.json"),
                names = Read("encounters/names_placeholder.json"),
                reputation = Read("reputation_793.json"),
                knowing = Read("knowing_793.json"),
                writing = Read("writing.json"),
                goods = Read("goods_793.json"),
                ports = Read("ports_793.json"),
                traffic = Read("traffic_793.json"),
            };
            foreach (var file in Directory.GetFiles(Data + "encounters", "*.json"))
            {
                string name = Path.GetFileName(file);
                if (name == "npc_roles_793.json" || name == "names_placeholder.json")
                    continue;
                src.cards.Add(("encounters/" + name, File.ReadAllText(file)));
            }
            return src;
        }

        /// <summary>The loaded book; a test fails here with the loader's faults if the tables do not check.</summary>
        public static EncounterBook Book()
        {
            if (book != null)
                return book;
            var b = EncounterLoader.Load(Sources());
            Assert.IsTrue(b.Ok, "the encounter tables have faults:\n" + string.Join("\n", b.errors));
            return book = b;
        }

        /// <summary>A book of its own with only the named cards (a fresh load each time, so a test may change it): for the
        /// tests of one card alone, however many cards the game has by now.</summary>
        public static EncounterBook BookOf(params string[] ids)
        {
            var b = EncounterLoader.Load(Sources());
            Assert.IsTrue(b.Ok, "the encounter tables have faults: " + string.Join("; ", b.errors));
            b.cards.RemoveAll(c => System.Array.IndexOf(ids, c.id) < 0);
            Assert.AreEqual(ids.Length, b.cards.Count, "every named card is in the book");
            return b;
        }
    }
}
