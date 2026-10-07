// 7 Oct 2026: THE CONTENT'S FILES. The writers filled Assets/Kapman/Game/Data with far more than the 22 tables KapmanGame
// holds: a places file for every port, the story tables (ships, upgrades, town acts, works, games, unlocks, the winter,
// the ages, sayings, settlements, wars, hinges, starts, endings, the storyteller), the gazetteer and the routes. Every
// loader of that content reads it through this one class, so there is one answer to "where is the data" (the project's
// folder in the editor and in the tests, StreamingAssets/Data in a built game: Editor/Game/ContentBuild.cs copies it
// there before a build), one rule for a file that is missing or half written (null, one warning, go on: never an error,
// never an exception), and one switch that makes every loader read again (Reset, and a test's SetRoot).
// The plan the loaders follow is Design/wiring_plan.md.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// Reads the content files by their path under the data folder ("places/dorestad.json", "story/ships_793.json"),
    /// always with forward slashes. Nothing here throws and nothing here logs an error: a file that is not there, cannot
    /// be read or does not parse gives null and ONE warning for the whole run, so twenty writers can be at work in the
    /// folder while the game runs. <see cref="Version"/> counts up on every <see cref="Reset"/> and <see cref="SetRoot"/>:
    /// a loader that keeps what it read (through <see cref="ContentCache{T}"/>) reads again when it has changed.
    /// </summary>
    public static class ContentFiles
    {
        /// <summary>The data folder inside the project's Assets folder (the editor and the tests read it off the disk).</summary>
        public const string ProjectFolder = "Kapman/Game/Data";
        /// <summary>The data folder inside a built game's StreamingAssets (ContentBuild copies the project's folder there).</summary>
        public const string BuiltFolder = "Data";

        static string root;                                               // null: the default
        static readonly HashSet<string> warned = new HashSet<string>(StringComparer.Ordinal);
        static readonly List<string> warnings = new List<string>();

        /// <summary>Counts up whenever the content must be read again (Reset, SetRoot, each Play without a domain reload).</summary>
        public static int Version { get; private set; } = 1;

        /// <summary>Where the content is when no test has pointed elsewhere: the project's data folder in the editor,
        /// StreamingAssets/Data in a built game. Forward slashes, no slash at the end.</summary>
        public static string DefaultRoot
        {
            get
            {
#if UNITY_EDITOR
                return Clean(Application.dataPath + "/" + ProjectFolder);
#else
                return Clean(Application.streamingAssetsPath + "/" + BuiltFolder);
#endif
            }
        }

        /// <summary>The folder every path is under (forward slashes, no slash at the end).</summary>
        public static string Root => root ?? DefaultRoot;

        /// <summary>False while a test has pointed the root elsewhere.</summary>
        public static bool RootIsDefault => root == null;

        /// <summary>Points the content at another folder (a test's own files); null or "" puts it back. Everything read
        /// before is forgotten (Reset).</summary>
        public static void SetRoot(string folder)
        {
            root = string.IsNullOrWhiteSpace(folder) ? null : Clean(Path.GetFullPath(folder));
            Reset();
        }

        /// <summary>Forgets what was warned of and tells every loader to read again (Version counts up). Called on each
        /// Play (so Play without a domain reload reads the files afresh) and by a test that has changed a file.</summary>
        public static void Reset()
        {
            Version++;
            warned.Clear();
            warnings.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            root = null;                                                  // (a test's root never leaks into a game)
            Reset();
        }

        // ------------------------------------------------------------------ paths

        /// <summary>The full path of a file or folder of the content (null, with a warning, for a path that leaves the
        /// data folder: one with ".." in it, or a rooted one). An empty path is the root itself.</summary>
        public static string PathOf(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Root;
            string rel = Slashes(relativePath).Trim('/');
            if (rel.Length == 0)
                return Root;
            bool outside;
            try
            {
                outside = Path.IsPathRooted(rel) || rel.Contains("..");
            }
            catch (Exception)
            {
                outside = true;                                           // (characters no path may hold)
            }
            if (outside)
            {
                Warn("path:" + rel, $"'{relativePath}' is not a path inside the data folder: not read");
                return null;
            }
            return Root + "/" + rel;
        }

        /// <summary>Is the file there? (No warning either way: ask this first for a file that may not be written yet.)</summary>
        public static bool Exists(string relativePath)
        {
            string path = PathOf(relativePath);
            try
            {
                return path != null && File.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Is the folder there? (No warning either way.)</summary>
        public static bool FolderExists(string folder)
        {
            string path = PathOf(folder);
            try
            {
                return path != null && Directory.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ------------------------------------------------------------------ reading

        /// <summary>A file's text ("story/ships_793.json"), or null with one warning when it is not there or cannot be
        /// read. A file another program is writing at this moment is still read (it is opened shared).</summary>
        public static string Read(string relativePath)
        {
            string path = PathOf(relativePath);
            if (path == null)
                return null;
            try
            {
                if (!File.Exists(path))
                {
                    Warn("missing:" + relativePath, $"no file {relativePath} (skipped)");
                    return null;
                }
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                    return reader.ReadToEnd();
            }
            catch (Exception e)
            {
                Warn("read:" + relativePath, $"{relativePath} could not be read ({e.Message}): skipped");
                return null;
            }
        }

        /// <summary>As <see cref="Read"/>, but always from the default root, wherever a test has pointed the root: for the
        /// shared tables a test's own folder does not have (the words of standing and feeling, the ports).</summary>
        public static string ReadDefault(string relativePath)
        {
            if (root == null)
                return Read(relativePath);
            string pointed = root;
            root = null;
            try
            {
                return Read(relativePath);
            }
            finally
            {
                root = pointed;
            }
        }

        /// <summary>
        /// The files of one folder that match a pattern ("places", "*.json"; "", "routes_*.json"; "encounters",
        /// "chain_*_793.json"), as paths under the data folder ("places/ahus.json"), in the names' order (ordinal, so the
        /// same on every machine). Not its subfolders. Unity's .meta files, hidden files and half-saved files (.tmp,
        /// .part, a name ending in ~) are never listed. A folder that is not there gives an empty list and one warning.
        /// </summary>
        public static string[] List(string folder, string pattern = "*.json")
        {
            string dir = PathOf(folder);
            if (dir == null)
                return Array.Empty<string>();
            string rel = string.IsNullOrWhiteSpace(folder) ? "" : Slashes(folder).Trim('/');
            if (rel.Length > 0)
                rel += "/";
            if (string.IsNullOrEmpty(pattern))
                pattern = "*";
            try
            {
                if (!Directory.Exists(dir))
                {
                    Warn("nofolder:" + rel, $"no folder {(rel.Length > 0 ? rel : "(the data folder itself)")}: nothing read from it");
                    return Array.Empty<string>();
                }
                // what follows the pattern's last star must end the name exactly ("*.json" never takes "x.json.meta" or "x.jsonl")
                string tail = pattern.IndexOf('?') >= 0 ? "" : pattern.Substring(pattern.LastIndexOf('*') + 1);
                var found = new List<string>();
                foreach (string path in Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    string name = Path.GetFileName(path);
                    if (name.Length == 0 || name[0] == '.' || name[name.Length - 1] == '~')
                        continue;
                    if (name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".part", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (tail.Length > 0 && !name.EndsWith(tail, StringComparison.OrdinalIgnoreCase))
                        continue;
                    found.Add(rel + name);
                }
                found.Sort(StringComparer.Ordinal);
                return found.ToArray();
            }
            catch (Exception e)
            {
                Warn("list:" + rel + pattern, $"the folder {rel} could not be listed ({e.Message}): nothing read from it");
                return Array.Empty<string>();
            }
        }

        // ------------------------------------------------------------------ parsing

        /// <summary>A file read and parsed with the project's Json reader (objects as Dictionary&lt;string, object&gt;, lists
        /// as List&lt;object&gt;, numbers as double), or null with one warning when it is missing, unreadable or does not
        /// parse (a half-written file).</summary>
        public static object Parse(string relativePath)
        {
            string text = Read(relativePath);
            return text == null ? null : ParseText(text, relativePath);
        }

        /// <summary>As <see cref="Parse"/>, for a file whose top level must be an object ({ ... }): null and one warning
        /// when it is anything else.</summary>
        public static Dictionary<string, object> ParseObject(string relativePath)
        {
            object v = Parse(relativePath);
            if (v == null)
                return null;
            if (v is Dictionary<string, object> d)
                return d;
            Warn("shape:" + relativePath, $"{relativePath} is not a JSON object at the top: skipped");
            return null;
        }

        /// <summary>Text parsed as JSON, or null with one warning (the name is only for the message and the once-only rule).</summary>
        public static object ParseText(string json, string name)
        {
            if (json == null)
                return null;
            try
            {
                object v = Json.Parse(json);
                if (v == null)
                    Warn("parse:" + name, $"{name} holds nothing: skipped");
                return v;
            }
            catch (Exception e)
            {
                Warn("parse:" + name, $"{name} does not parse ({e.Message}): skipped (it may be half written)");
                return null;
            }
        }

        /// <summary>Every file of a folder that parses to an object, with its path, in the names' order; the others are
        /// skipped with their one warning each.</summary>
        public static IEnumerable<KeyValuePair<string, Dictionary<string, object>>> ParseAll(string folder, string pattern = "*.json")
        {
            foreach (string file in List(folder, pattern))
            {
                var parsed = ParseObject(file);
                if (parsed != null)
                    yield return new KeyValuePair<string, Dictionary<string, object>>(file, parsed);
            }
        }

        // ------------------------------------------------------------------ warnings

        /// <summary>One warning for a key for the whole run ("Kapman content: ..."): a loader says a thing once however
        /// often it is asked. Never an error: an error fails the play tests, and content is allowed to be unfinished.</summary>
        public static void Warn(string key, string message)
        {
            if (!warned.Add(key ?? message ?? ""))
                return;
            string line = "Kapman content: " + message;
            warnings.Add(line);
            Debug.LogWarning(line);
        }

        /// <summary>Everything warned of since the last Reset, in order (for a content report, and for the tests).</summary>
        public static IReadOnlyList<string> Warnings => warnings;

        static string Slashes(string path) => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

        static string Clean(string path) => Slashes(path).TrimEnd('/');
    }

    /// <summary>
    /// What a loader keeps of the content it read: made on first use, and made again whenever <see cref="ContentFiles.Version"/>
    /// has changed (a new Play, a test's Reset or SetRoot). The loader's own function never has to guard itself: if it
    /// throws, there is one warning and the value is an empty T for this run, never null.
    /// <code>static readonly ContentCache&lt;PlaceBook&gt; cache = new ContentCache&lt;PlaceBook&gt;("the places", Load);
    /// public static PlaceBook Current =&gt; cache.Value;</code>
    /// </summary>
    public sealed class ContentCache<T> where T : class, new()
    {
        readonly string name;
        readonly Func<T> load;
        T value;
        int version;

        public ContentCache(string name, Func<T> load)
        {
            this.name = name ?? typeof(T).Name;
            this.load = load;
        }

        /// <summary>The loaded content (loaded now if it has not been, or if the content's version has moved on).</summary>
        public T Value
        {
            get
            {
                if (value != null && version == ContentFiles.Version)
                    return value;
                int reading = ContentFiles.Version;
                T made = null;
                try
                {
                    made = load != null ? load() : null;
                }
                catch (Exception e)
                {
                    ContentFiles.Warn("load:" + name, $"{name} could not be loaded ({e.GetType().Name}: {e.Message}): empty for this run");
                }
                value = made ?? new T();
                version = reading;
                return value;
            }
        }

        /// <summary>Has it been loaded for the content as it stands?</summary>
        public bool Loaded => value != null && version == ContentFiles.Version;

        /// <summary>Forgets it: the next use loads again.</summary>
        public void Clear() => value = null;
    }

    /// <summary>
    /// Reading values out of a parsed content file, forgivingly: the writers' files are drafts, so a field may be missing,
    /// a number written as text, one string where a list was meant. Beside <see cref="Json"/>'s own Str, Num, Bool, Obj
    /// and Arr; nothing here throws.
    /// </summary>
    public static class ContentJson
    {
        /// <summary>The objects of a list field ("ports", "ships", "acts"); anything in the list that is not an object is
        /// skipped. A missing field gives none.</summary>
        public static IEnumerable<Dictionary<string, object>> Rows(object o, string key)
        {
            var list = Json.Arr(o, key);
            if (list == null)
                yield break;
            foreach (object x in list)
                if (x is Dictionary<string, object> d)
                    yield return d;
        }

        /// <summary>The entries of an object used as a map ("kinds": { "ship": {...}, ... }); a missing field gives none.</summary>
        public static IEnumerable<KeyValuePair<string, object>> Pairs(object o, string key)
        {
            var d = Json.Obj(o, key);
            if (d == null)
                yield break;
            foreach (var kv in d)
                yield return kv;
        }

        /// <summary>A list of strings ("do", "built_at", "goods"), or one string taken as a list of one; never null. Things
        /// that are not strings are skipped.</summary>
        public static string[] Strings(object o, string key)
        {
            if (!(o is Dictionary<string, object> d) || !d.TryGetValue(key, out object v) || v == null)
                return Array.Empty<string>();
            if (v is string one)
                return one.Length > 0 ? new[] { one } : Array.Empty<string>();
            if (!(v is List<object> list))
                return Array.Empty<string>();
            var found = new List<string>(list.Count);
            foreach (object x in list)
                if (x is string s)
                    found.Add(s);
            return found.ToArray();
        }

        /// <summary>Is the field there at all (and not null)?</summary>
        public static bool Has(object o, string key) => o is Dictionary<string, object> d && d.TryGetValue(key, out object v) && v != null;

        /// <summary>A number, or text that is a number ("12"); the fallback for anything else.</summary>
        public static double Num(object o, string key, double fallback = 0.0)
        {
            if (!(o is Dictionary<string, object> d) || !d.TryGetValue(key, out object v))
                return fallback;
            if (v is double n)
                return n;
            if (v is string s && double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                return parsed;
            return fallback;
        }

        /// <summary>A whole number (a year, a count), rounded; the fallback when it is missing or not a number.</summary>
        public static int Int(object o, string key, int fallback = 0)
        {
            double n = Num(o, key, double.NaN);
            return double.IsNaN(n) || double.IsInfinity(n) ? fallback : (int)Math.Round(n);
        }

        /// <summary>Text: a string as it is, a number or true/false as its text; the fallback for a list, an object or nothing.</summary>
        public static string Text(object o, string key, string fallback = "")
        {
            if (!(o is Dictionary<string, object> d) || !d.TryGetValue(key, out object v) || v == null)
                return fallback;
            if (v is string s)
                return s;
            if (v is double n)
                return n.ToString("0.########", CultureInfo.InvariantCulture);
            if (v is bool b)
                return b ? "true" : "false";
            return fallback;
        }

        /// <summary>A pair written [low, high] ("units": [10, 60]), or one number taken as both. False when it is neither.</summary>
        public static bool Range(object o, string key, out double low, out double high)
        {
            low = high = 0.0;
            if (!(o is Dictionary<string, object> d) || !d.TryGetValue(key, out object v))
                return false;
            if (v is double one)
            {
                low = high = one;
                return true;
            }
            if (v is List<object> list && list.Count >= 2 && list[0] is double a && list[1] is double b)
            {
                low = Math.Min(a, b);
                high = Math.Max(a, b);
                return true;
            }
            return false;
        }

        /// <summary>
        /// A date as the story files write it (SCHEMAS.md rule 3): "793-06-21", a year alone as a number or as text
        /// (793, "863": month and day come back 0), or a day of any year ("03-25": the year comes back 0). False for
        /// nothing, "", or anything else.
        /// </summary>
        public static bool Date(object o, string key, out int year, out int month, out int day)
        {
            year = month = day = 0;
            if (!(o is Dictionary<string, object> d) || !d.TryGetValue(key, out object v) || v == null)
                return false;
            if (v is double n)
            {
                year = (int)Math.Round(n);
                return year != 0;
            }
            return v is string s && Date(s, out year, out month, out day);
        }

        /// <summary>As the other Date, from the text itself.</summary>
        public static bool Date(string text, out int year, out int month, out int day)
        {
            year = month = day = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;
            string[] parts = text.Trim().Split('-');
            var nums = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
                if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out nums[i]))
                    return false;
            switch (parts.Length)
            {
                case 1:
                    year = nums[0];
                    return year != 0;
                case 2:
                    month = nums[0];
                    day = nums[1];
                    return month >= 1 && month <= 12 && day >= 1 && day <= 31;
                case 3:
                    year = nums[0];
                    month = nums[1];
                    day = nums[2];
                    return month >= 1 && month <= 12 && day >= 1 && day <= 31;
                default:
                    return false;
            }
        }

        /// <summary>One number for a day, to compare two dates by (the chronicle's own: ChronicleEntry.Key).</summary>
        public static int DayKey(int year, int month, int day) => year * 400 + month * 32 + day;

        /// <summary>Is a thing with first and last years ("from", "to"; 0 for "before the game" or "no end", as in
        /// ports_793.json) there in a year?</summary>
        public static bool InYears(int from, int to, int year) => (from == 0 || from <= year) && (to == 0 || to >= year);
    }
}
