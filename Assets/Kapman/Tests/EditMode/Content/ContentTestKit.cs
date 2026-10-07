using System;
using System.IO;
using Kapman.Encounters;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// What every test of the content needs (Design/wiring_plan.md 1.3), written once: a folder of the test's own to
    /// point the content at, with fixture files in it; the cards' engine with the game's real tables of roles, names and
    /// words and NO cards (so a card file a writer is in the middle of cannot fail a test of something else), moored at a
    /// port or at sea, with the fake game that writes down every call; and the ports and goods tables for a test that
    /// asks GameData. Call <see cref="Done"/> in [TearDown].
    /// </summary>
    public static class ContentTestKit
    {
        /// <summary>A new empty folder of the test's own (under the machine's temp folder).</summary>
        public static string TempFolder()
        {
            string dir = Path.Combine(Path.GetTempPath(), "kapman_content_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>Writes a fixture file ("story/ships_793.json") into a folder, making its subfolders.</summary>
        public static void Put(string folder, string relativePath, string text)
        {
            string path = Path.Combine(folder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }

        /// <summary>A new folder with the content pointed at it (ContentFiles.SetRoot). Returns the folder.</summary>
        public static string UseFolder()
        {
            string dir = TempFolder();
            ContentFiles.SetRoot(dir);
            return dir;
        }

        /// <summary>For [TearDown]: no test's director, flags or root are left behind, and the folder (if one is given)
        /// is deleted.</summary>
        public static void Done(string folder = null)
        {
            ContentRules.Use(null);
            ContentFlags.Use(null);
            ContentFiles.SetRoot(null);
            if (string.IsNullOrEmpty(folder))
                return;
            try
            {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
            catch (Exception)
            {
                // (a temp folder left behind harms nothing)
            }
        }

        /// <summary>The game's real tables of roles, names and words, read from the project's folder wherever the content
        /// is pointed, and no cards: add a test's own cards to <c>cards</c>.</summary>
        public static EncounterSources Tables()
        {
            return new EncounterSources
            {
                roles = ContentFiles.ReadDefault("encounters/npc_roles_793.json"),
                names = ContentFiles.ReadDefault("encounters/names_placeholder.json"),
                reputation = ContentFiles.ReadDefault("reputation_793.json"),
                knowing = ContentFiles.ReadDefault("knowing_793.json"),
                writing = "{ \"entries\": [] }",
            };
        }

        /// <summary>The cards' engine with those tables and no cards.</summary>
        public static EncounterBook EmptyBook()
        {
            var book = EncounterLoader.Load(Tables());
            Assert.IsTrue(book.Ok, "the tables of roles, names and words have faults:\n" + string.Join("\n", book.errors));
            return book;
        }

        /// <summary>
        /// A director moored at a port (every port here is of the realm "franks" with "frisian" people), handed to
        /// ContentRules.Use, with the fake game (100 d of silver; every call written down in game.calls). It is ten in
        /// the morning, the crew is four, and the dice always win (set d.roll to lose). Any other number a condition
        /// reads is 0 until the test sets it: d.vars.nums["hold.lead"] = 2. "silver" follows game.silver.
        /// </summary>
        public static (EncounterDirector d, FakeGame game) AtPort(string port = "dorestad", double day = 10.0, int year = 793, int month = 6)
        {
            var (d, game) = Director(day, year, month);
            d.BeginCall(port);
            return (d, game);
        }

        /// <summary>As <see cref="AtPort"/>, at sea: no call in progress, nobody there.</summary>
        public static (EncounterDirector d, FakeGame game) AtSea(double day = 10.0, int year = 793, int month = 6) => Director(day, year, month);

        static (EncounterDirector d, FakeGame game) Director(double day, int year, int month)
        {
            var game = new FakeGame();
            var d = new EncounterDirector(EmptyBook(), 793, game);
            d.portInfo = port => string.IsNullOrEmpty(port) ? ("", "") : ("franks", "frisian");
            d.roll = () => 0.0;
            d.vars.nums["hour"] = 10.0;
            d.vars.nums["crew"] = 4.0;
            d.vars.numSource = name => name == "silver" ? game.silver : 0.0;
            d.SetTime(day, year, month, (long)Math.Floor(day * 24.0) + 10);
            ContentRules.Use(d);
            return (d, game);
        }

        /// <summary>The ports table into GameData, from the project's folder (for a test that asks GameData.Port).</summary>
        public static void LoadPorts()
        {
            string text = ContentFiles.ReadDefault("ports_793.json");
            Assert.IsNotNull(text, "ports_793.json is in the project's data folder");
            GameData.LoadPorts(new TextAsset(text));
        }

        /// <summary>The goods table into GameData, from the project's folder.</summary>
        public static void LoadGoods()
        {
            string text = ContentFiles.ReadDefault("goods_793.json");
            Assert.IsNotNull(text, "goods_793.json is in the project's data folder");
            GameData.LoadGoods(new TextAsset(text));
        }
    }
}
