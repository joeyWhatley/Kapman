using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.EditorTools;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The content's files (Content/ContentFiles.cs; Design/wiring_plan.md): every loader of the new data reads through
    /// this, so its rules are tested once, on a folder of the test's own. A file comes back as text or parsed; a file that
    /// is missing, half written or of the wrong shape gives null and ONE warning however often it is asked; a folder lists
    /// its files in order without Unity's .meta files or half-saved ones; the root can be pointed elsewhere and back; and
    /// Reset makes a loader that kept its data (ContentCache) read again. Also the forgiving readers of ContentJson, and
    /// the copy a build takes of the data folder (Editor/Game/ContentBuild.cs).
    /// </summary>
    public class ContentFilesTests
    {
        string dir;

        void Put(string relative, string text)
        {
            string path = Path.Combine(dir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }

        [SetUp]
        public void MakeAFolder()
        {
            dir = Path.Combine(Path.GetTempPath(), "kapman_content_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            Put("places/b_port.json", "{ \"about\": \"a test\", \"ports\": [ { \"port\": \"b_port\", \"sites\": [] } ] }");
            Put("places/a_port.json", "{ \"ports\": [ { \"port\": \"a_port\" } ] }");
            Put("story/ships_793.json", "{ \"ships\": [ { \"id\": \"gislinge\", \"cost_d\": 600 } ] }");
            ContentFiles.SetRoot(dir);
        }

        [TearDown]
        public void PutTheRootBack()
        {
            ContentFiles.SetRoot(null);
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception)
            {
                // (a temp folder left behind harms nothing)
            }
        }

        [Test]
        public void TheDefaultRootIsTheProjectsDataFolder()
        {
            ContentFiles.SetRoot(null);
            Assert.IsTrue(ContentFiles.RootIsDefault);
            StringAssert.EndsWith("Assets/" + ContentFiles.ProjectFolder, ContentFiles.Root);
            Assert.IsFalse(ContentFiles.Root.Contains(Path.DirectorySeparatorChar.ToString()) && Path.DirectorySeparatorChar != '/',
                           "the root is written with forward slashes");
            Assert.IsTrue(ContentFiles.Exists("ports_793.json"), "the ports table is in the project's data folder");
            var ports = ContentFiles.ParseObject("ports_793.json");
            Assert.IsNotNull(ports, "ports_793.json parses");
            Assert.Greater(ContentJson.Rows(ports, "ports").Count(), 10, "and holds the ports");
            Assert.IsTrue(ContentFiles.FolderExists("places"));
            Assert.Greater(ContentFiles.List("places").Length, 10, "a places file for each port");
        }

        [Test]
        public void TheRootCanBePointedElsewhereAndBack()
        {
            Assert.IsFalse(ContentFiles.RootIsDefault);
            Assert.AreEqual(dir.Replace(Path.DirectorySeparatorChar, '/').TrimEnd('/'), ContentFiles.Root);
            Assert.IsTrue(ContentFiles.Exists("places/b_port.json"));
            Assert.IsFalse(ContentFiles.Exists("ports_793.json"), "the test's folder has no ports table");
            int before = ContentFiles.Version;
            ContentFiles.SetRoot(null);
            Assert.IsTrue(ContentFiles.RootIsDefault);
            Assert.AreNotEqual(before, ContentFiles.Version, "pointing the root elsewhere tells the loaders to read again");
            Assert.IsTrue(ContentFiles.Exists("ports_793.json"));
        }

        [Test]
        public void ReadGivesTheTextAndNullWithOneWarningForAMissingFile()
        {
            StringAssert.Contains("b_port", ContentFiles.Read("places/b_port.json"));
            StringAssert.Contains("b_port", ContentFiles.Read("places\\b_port.json"), "either slash");
            Assert.AreEqual(0, ContentFiles.Warnings.Count);
            Assert.IsNull(ContentFiles.Read("places/nowhere.json"));
            Assert.AreEqual(1, ContentFiles.Warnings.Count, "one warning");
            Assert.IsNull(ContentFiles.Read("places/nowhere.json"));
            Assert.IsNull(ContentFiles.Parse("places/nowhere.json"));
            Assert.AreEqual(1, ContentFiles.Warnings.Count, "and only one, however often it is asked");
            StringAssert.Contains("places/nowhere.json", ContentFiles.Warnings[0]);
            ContentFiles.Reset();
            Assert.AreEqual(0, ContentFiles.Warnings.Count, "a new run says it again");
        }

        [Test]
        public void AFileBeingWrittenIsStillRead()
        {
            string path = Path.Combine(dir, "places", "b_port.json");
            using (var writer = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                string text = ContentFiles.Read("places/b_port.json");
                Assert.IsNotNull(text, "a writer holding the file open does not stop the game reading it");
                StringAssert.Contains("b_port", text);
            }
        }

        [Test]
        public void APathOutsideTheFolderIsNotRead()
        {
            Assert.IsNull(ContentFiles.Read("../outside.json"));
            Assert.IsNull(ContentFiles.Read("places/../../outside.json"));
            Assert.IsNull(ContentFiles.Read(Path.Combine(dir, "places", "b_port.json")), "a full path is refused too");
            Assert.IsFalse(ContentFiles.Exists("../outside.json"));
            Assert.AreEqual(0, ContentFiles.List("..").Length);
            Assert.AreEqual(ContentFiles.Root, ContentFiles.PathOf(""));
            Assert.AreEqual(ContentFiles.Root + "/places/b_port.json", ContentFiles.PathOf("/places/b_port.json"));
        }

        [Test]
        public void ListGivesAFoldersFilesInOrderWithoutMetaOrHalfSavedOnes()
        {
            Put("places/b_port.json.meta", "fileFormatVersion: 2");
            Put("places/c_port.json.tmp", "{");
            Put("places/d_port.json~", "{");
            Put("places/.hidden.json", "{}");
            Put("places/notes.txt", "notes");
            Put("places/deeper/e_port.json", "{}");
            Put("routes_water_793.json", "{}");
            Put("routes_land_793.json", "{}");
            Put("gazetteer_793.json", "{}");
            Put("encounters/chain_black_stones_793.json", "{}");
            Put("encounters/harbour_793.json", "{}");

            CollectionAssert.AreEqual(new[] { "places/a_port.json", "places/b_port.json" }, ContentFiles.List("places"));
            CollectionAssert.AreEqual(new[] { "places/a_port.json", "places/b_port.json" }, ContentFiles.List("places/", "*.json"));
            CollectionAssert.AreEqual(new[] { "places/notes.txt" }, ContentFiles.List("places", "*.txt"));
            CollectionAssert.AreEqual(new[] { "routes_land_793.json", "routes_water_793.json" }, ContentFiles.List("", "routes_*.json"));
            CollectionAssert.AreEqual(new[] { "gazetteer_793.json", "routes_land_793.json", "routes_water_793.json" }, ContentFiles.List(null));
            CollectionAssert.AreEqual(new[] { "encounters/chain_black_stones_793.json" }, ContentFiles.List("encounters", "chain_*_793.json"));
            Assert.AreEqual(0, ContentFiles.Warnings.Count);

            Assert.AreEqual(0, ContentFiles.List("story/wars").Length, "a folder not written yet lists nothing");
            Assert.AreEqual(0, ContentFiles.List("story/wars").Length);
            Assert.AreEqual(1, ContentFiles.Warnings.Count, "with one warning");
            Assert.IsFalse(ContentFiles.FolderExists("story/wars"));
            Assert.IsTrue(ContentFiles.FolderExists("story"));
        }

        [Test]
        public void ParseGivesNullAndOneWarningForAHalfWrittenFile()
        {
            Put("places/half.json", "{ \"ports\": [ { \"port\": \"half\", \"summary\": \"The writer stopped in the middle of a sen");
            Put("places/empty.json", "");
            Put("places/list.json", "[ 1, 2, 3 ]");
            Put("places/cut_escape.json", "{ \"a\": \"x\\");

            var good = ContentFiles.ParseObject("places/b_port.json");
            Assert.IsNotNull(good);
            Assert.AreEqual("a test", Json.Str(good, "about"));
            Assert.AreEqual(0, ContentFiles.Warnings.Count);

            Assert.IsNull(ContentFiles.Parse("places/half.json"));
            Assert.IsNull(ContentFiles.ParseObject("places/half.json"));
            Assert.AreEqual(1, ContentFiles.Warnings.Count, "one warning for the half-written file, however often it is asked");
            StringAssert.Contains("places/half.json", ContentFiles.Warnings[0]);

            Assert.IsNull(ContentFiles.Parse("places/empty.json"));
            Assert.IsNull(ContentFiles.Parse("places/cut_escape.json"), "cut off inside an escape: no exception");
            Assert.IsNotNull(ContentFiles.Parse("places/list.json"), "a list parses");
            Assert.IsNull(ContentFiles.ParseObject("places/list.json"), "but is not an object");
            Assert.AreEqual(4, ContentFiles.Warnings.Count);

            Assert.IsNull(ContentFiles.ParseText(null, "nothing"));
            Assert.IsNotNull(ContentFiles.ParseText("{ \"a\": 1 }", "text"));
        }

        [Test]
        public void ParseAllSkipsTheFilesThatDoNotParse()
        {
            Put("places/half.json", "{ \"ports\": [");
            var read = ContentFiles.ParseAll("places").ToList();
            CollectionAssert.AreEqual(new[] { "places/a_port.json", "places/b_port.json" }, read.Select(kv => kv.Key).ToArray());
            Assert.AreEqual("b_port", Json.Str(ContentJson.Rows(read[1].Value, "ports").First(), "port"));
            Assert.AreEqual(1, ContentFiles.Warnings.Count);
            Assert.AreEqual(0, ContentFiles.ParseAll("story/hinges").Count(), "a folder not there: none, and no exception");
        }

        sealed class Book
        {
            public int ships;
        }

        [Test]
        public void ACacheLoadsOnceAndAgainAfterAReset()
        {
            int loads = 0;
            var cache = new ContentCache<Book>("the test's ships", () =>
            {
                loads++;
                var book = new Book();
                foreach (var ship in ContentJson.Rows(ContentFiles.ParseObject("story/ships_793.json"), "ships"))
                    if (ship != null)
                        book.ships++;
                return book;
            });
            Assert.IsFalse(cache.Loaded);
            Assert.AreEqual(1, cache.Value.ships);
            Assert.AreSame(cache.Value, cache.Value);
            Assert.AreEqual(1, loads, "read once");
            Assert.IsTrue(cache.Loaded);

            Put("story/ships_793.json", "{ \"ships\": [ { \"id\": \"gislinge\" }, { \"id\": \"knarr\" } ] }");
            Assert.AreEqual(1, cache.Value.ships, "a changed file is not seen until the content is reset");
            ContentFiles.Reset();
            Assert.IsFalse(cache.Loaded);
            Assert.AreEqual(2, cache.Value.ships);
            Assert.AreEqual(2, loads);

            File.Delete(Path.Combine(dir, "story", "ships_793.json"));
            ContentFiles.SetRoot(dir);
            Assert.AreEqual(0, cache.Value.ships, "the file gone: an empty book, not null");
            Assert.AreEqual(3, loads);
            cache.Clear();
            Assert.AreEqual(0, cache.Value.ships);
            Assert.AreEqual(4, loads);
        }

        [Test]
        public void ACacheWhoseLoaderFailsGivesAnEmptyValue()
        {
            int loads = 0;
            var throwing = new ContentCache<Book>("a loader that throws", () =>
            {
                loads++;
                throw new InvalidOperationException("a fault in the loader");
            });
            Assert.IsNotNull(throwing.Value);
            Assert.AreEqual(0, throwing.Value.ships);
            Assert.AreSame(throwing.Value, throwing.Value);
            Assert.AreEqual(1, loads, "not tried again every time it is asked");
            Assert.AreEqual(1, ContentFiles.Warnings.Count);
            StringAssert.Contains("a loader that throws", ContentFiles.Warnings[0]);

            var nothing = new ContentCache<Book>("a loader that gives nothing", () => null);
            Assert.IsNotNull(nothing.Value);
        }

        [Test]
        public void TheForgivingReadersTakeWhatIsThere()
        {
            var o = ContentFiles.ParseText(
                "{ \"ships\": [ { \"id\": \"a\" }, 7, \"x\", { \"id\": \"b\" } ], \"kinds\": { \"ship\": { \"prefix\": \"unlock_ship_\" }, \"plot\": 1 }," +
                " \"do\": [ \"act:load\", 3, \"market\" ], \"one\": \"act:repair\", \"empty\": \"\", \"cost_d\": \"600\", \"days\": 45.4, \"word\": \"many\"," +
                " \"units\": [ 60, 10 ], \"heat\": 2, \"owned\": true, \"nothing\": null," +
                " \"from\": \"850-07-01\", \"year\": 863, \"year_text\": \"863\", \"counted_on\": \"03-25\", \"bad\": \"soon\", \"bad2\": \"850-13-01\" }", "test");
            Assert.IsNotNull(o);

            CollectionAssert.AreEqual(new[] { "a", "b" }, ContentJson.Rows(o, "ships").Select(r => Json.Str(r, "id")).ToArray());
            Assert.AreEqual(0, ContentJson.Rows(o, "nowhere").Count());
            Assert.AreEqual(0, ContentJson.Rows(null, "ships").Count());
            CollectionAssert.AreEqual(new[] { "ship", "plot" }, ContentJson.Pairs(o, "kinds").Select(kv => kv.Key).ToArray());
            Assert.AreEqual(0, ContentJson.Pairs(o, "ships").Count());

            CollectionAssert.AreEqual(new[] { "act:load", "market" }, ContentJson.Strings(o, "do"));
            CollectionAssert.AreEqual(new[] { "act:repair" }, ContentJson.Strings(o, "one"));
            Assert.AreEqual(0, ContentJson.Strings(o, "empty").Length);
            Assert.AreEqual(0, ContentJson.Strings(o, "nowhere").Length);
            Assert.AreEqual(0, ContentJson.Strings(o, "heat").Length);

            Assert.IsTrue(ContentJson.Has(o, "do"));
            Assert.IsFalse(ContentJson.Has(o, "nothing"));
            Assert.IsFalse(ContentJson.Has(o, "nowhere"));

            Assert.AreEqual(600.0, ContentJson.Num(o, "cost_d"), "a number written as text");
            Assert.AreEqual(600, ContentJson.Int(o, "cost_d"));
            Assert.AreEqual(45, ContentJson.Int(o, "days"));
            Assert.AreEqual(-1, ContentJson.Int(o, "word", -1));
            Assert.AreEqual(5, ContentJson.Int(o, "nowhere", 5));
            Assert.AreEqual("600", ContentJson.Text(o, "cost_d"));
            Assert.AreEqual("2", ContentJson.Text(o, "heat"));
            Assert.AreEqual("true", ContentJson.Text(o, "owned"));
            Assert.AreEqual("none", ContentJson.Text(o, "do", "none"));

            Assert.IsTrue(ContentJson.Range(o, "units", out double low, out double high));
            Assert.AreEqual(10.0, low);
            Assert.AreEqual(60.0, high);
            Assert.IsTrue(ContentJson.Range(o, "heat", out low, out high));
            Assert.AreEqual(2.0, low);
            Assert.AreEqual(2.0, high);
            Assert.IsFalse(ContentJson.Range(o, "word", out low, out high));

            Assert.IsTrue(ContentJson.Date(o, "from", out int y, out int m, out int d));
            Assert.AreEqual((850, 7, 1), (y, m, d));
            Assert.IsTrue(ContentJson.Date(o, "year", out y, out m, out d));
            Assert.AreEqual((863, 0, 0), (y, m, d));
            Assert.IsTrue(ContentJson.Date(o, "year_text", out y, out m, out d));
            Assert.AreEqual((863, 0, 0), (y, m, d));
            Assert.IsTrue(ContentJson.Date(o, "counted_on", out y, out m, out d));
            Assert.AreEqual((0, 3, 25), (y, m, d));
            Assert.IsFalse(ContentJson.Date(o, "bad", out y, out m, out d));
            Assert.IsFalse(ContentJson.Date(o, "bad2", out y, out m, out d));
            Assert.IsFalse(ContentJson.Date(o, "empty", out y, out m, out d));
            Assert.IsFalse(ContentJson.Date(o, "nowhere", out y, out m, out d));
            Assert.Less(ContentJson.DayKey(850, 6, 30), ContentJson.DayKey(850, 7, 1));
            Assert.Less(ContentJson.DayKey(850, 12, 31), ContentJson.DayKey(851, 1, 1));

            Assert.IsTrue(ContentJson.InYears(0, 0, 793));
            Assert.IsTrue(ContentJson.InYears(793, 863, 863));
            Assert.IsFalse(ContentJson.InYears(793, 863, 864));
            Assert.IsFalse(ContentJson.InYears(900, 0, 899));
        }

        [Test]
        public void ABuildTakesACopyOfTheDataWithoutTheMetaFiles()
        {
            string copy = Path.Combine(dir, "copy");
            int n = ContentBuild.CopyTo(copy);
            Assert.Greater(n, 50, "the data folder's files");
            Assert.IsTrue(File.Exists(Path.Combine(copy, "ports_793.json")));
            Assert.IsTrue(File.Exists(Path.Combine(copy, "encounters", "harbour_793.json")), "with its subfolders");
            var copied = Directory.GetFiles(copy, "*", SearchOption.AllDirectories);
            Assert.AreEqual(n, copied.Length, "it says how many");
            Assert.IsFalse(copied.Any(f => f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)), "no .meta files");

            // and the game reads the copy as it reads the project's folder (what a built game does from StreamingAssets/Data)
            ContentFiles.SetRoot(copy);
            Assert.IsNotNull(ContentFiles.ParseObject("ports_793.json"));
            Assert.Greater(ContentFiles.List("encounters").Length, 3);
        }
    }
}
