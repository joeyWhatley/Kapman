// PROJECT PATH: Assets/Kapman/Code/Game/Art/ArtFiles.cs
// (a draft of 7 Oct 2026, written outside Assets: Design/art_loader/PLAN.md says how to bring it in)
using System;
using System.Collections.Generic;
using System.IO;

namespace Kapman.Game
{
    /// <summary>
    /// WHERE A SLOT'S PICTURE IS ON DISK. Every picture of the game is a slot with a fixed file name under its kind
    /// ("portrait/person.dorestad.tollman.png": Art/art_list.csv). The user's finished pictures are in Art/final, the
    /// placeholders in Art/placeholder, both laid out the same way. This class only answers "which file": it reads no
    /// picture and knows nothing of Unity, so it can be tried without the editor.
    ///
    /// THE ORDER (ruling 13 of 7 Oct 2026, Design/DECISIONS.md). Asked for a slot and its stand-in (a person and their
    /// role, a card and its stage, a site and its kind), the answer is the first of these that is there:
    ///   1  the user's picture of the slot itself        Art/final/portrait/person.x.png
    ///   2  the user's picture of the stand-in           Art/final/portrait/role.tollman.png
    ///   3  the placeholder of the slot itself           Art/placeholder/portrait/person.x.png
    ///   4  the placeholder of the stand-in              Art/placeholder/portrait/role.tollman.png
    /// So a drawn stand-in covers everyone who has no drawn picture of their own, which is what the stand-ins are for
    /// (the Desktop folder's DRAW FIRST page, group 3). <see cref="StandInBeforePlaceholder"/> = false gives the other
    /// reading of the ruling (1, 3, 2, 4: a slot's own placeholder before any stand-in).
    /// </summary>
    public sealed class ArtFiles
    {
        /// <summary>The user's finished pictures (Art/final) and the placeholders (Art/placeholder). Either may be null
        /// or not there yet: Art/final is made by the first run of Bake/art/pull_art.py.</summary>
        public readonly string FinalFolder, PlaceholderFolder;

        /// <summary>The folder the user draws in (the Desktop folder "Kapman Art"), or null: not looked at. When set, a
        /// picture there counts as the user's before pull_art.py has brought it over, if it is the size of its
        /// placeholder and no longer the same file as its placeholder (the two checks pull_art.py makes).</summary>
        public string DrawFolder;

        /// <summary>True: a drawn stand-in comes before the slot's own placeholder (see the class's summary).</summary>
        public bool StandInBeforePlaceholder = true;

        readonly Dictionary<string, (long stamp, bool mine)> drawChecked = new Dictionary<string, (long, bool)>(StringComparer.OrdinalIgnoreCase);

        public ArtFiles(string finalFolder, string placeholderFolder)
        {
            FinalFolder = string.IsNullOrEmpty(finalFolder) ? null : finalFolder;
            PlaceholderFolder = string.IsNullOrEmpty(placeholderFolder) ? null : placeholderFolder;
        }

        /// <summary>A slot file is a kind, a slash and a name: no drive, no "..", nothing that leaves the art folders.</summary>
        public static bool Valid(string file) =>
            !string.IsNullOrEmpty(file) && file.IndexOf("..", StringComparison.Ordinal) < 0 && file.IndexOf(':') < 0 &&
            file[0] != '/' && file[0] != '\\';

        /// <summary>The user's own picture of a slot, or null: the drawing folder's (if it is looked at and the picture
        /// there is the user's), else Art/final's.</summary>
        public string Final(string file)
        {
            if (!Valid(file))
                return null;
            if (DrawFolder != null)
            {
                string d = Path.Combine(DrawFolder, file);
                if (Mine(d, file))
                    return d;
            }
            if (FinalFolder == null)
                return null;
            string p = Path.Combine(FinalFolder, file);
            return Exists(p) ? p : null;
        }

        /// <summary>The placeholder of a slot, or null.</summary>
        public string Placeholder(string file)
        {
            if (!Valid(file) || PlaceholderFolder == null)
                return null;
            string p = Path.Combine(PlaceholderFolder, file);
            return Exists(p) ? p : null;
        }

        /// <summary>Has the user drawn this slot?</summary>
        public bool Drawn(string file) => Final(file) != null;

        /// <summary>The file a slot is shown from: the user's picture, else the placeholder, else null.</summary>
        public string Find(string file) => Final(file) ?? Placeholder(file);

        /// <summary>
        /// The file to show for a slot and its stand-ins, in the order of the class's summary; null with none.
        /// `files`: the slot's own file first, then its stand-ins (null and "" entries are passed over).
        /// `finalOnly`: only the user's own pictures count (the frame's pieces: FrameSkin).
        /// `drawn`: the answer is a picture of the user's.
        /// </summary>
        public string Find(IReadOnlyList<string> files, bool finalOnly, out bool drawn)
        {
            drawn = false;
            if (files == null)
                return null;
            string p;
            if (StandInBeforePlaceholder || finalOnly)
            {
                for (int i = 0; i < files.Count; i++)
                    if ((p = Final(files[i])) != null)
                    {
                        drawn = true;
                        return p;
                    }
                if (finalOnly)
                    return null;
                for (int i = 0; i < files.Count; i++)
                    if ((p = Placeholder(files[i])) != null)
                        return p;
                return null;
            }
            for (int i = 0; i < files.Count; i++)
            {
                if ((p = Final(files[i])) != null)
                {
                    drawn = true;
                    return p;
                }
                if ((p = Placeholder(files[i])) != null)
                    return p;
            }
            return null;
        }

        /// <summary>The folders a slot's picture could come from (to watch for a picture being added or taken away).</summary>
        public void Folders(string file, ICollection<string> into)
        {
            if (!Valid(file) || into == null)
                return;
            foreach (string root in new[] { DrawFolder, FinalFolder, PlaceholderFolder })
            {
                if (root == null)
                    continue;
                string dir = Path.GetDirectoryName(Path.Combine(root, file));
                if (!string.IsNullOrEmpty(dir) && !into.Contains(dir))
                    into.Add(dir);
            }
        }

        // ------------------------------------------------------------------ what has changed on disk

        /// <summary>A number that changes when a file is written again (its time of writing and its length); 0 when it
        /// is not there or cannot be looked at.</summary>
        public static long Stamp(string path)
        {
            try
            {
                var info = new FileInfo(path);
                return info.Exists ? unchecked(info.LastWriteTimeUtc.Ticks * 31 + info.Length) : 0L;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
            {
                return 0L;
            }
        }

        /// <summary>A number that changes when a file is added to a folder, taken from it or renamed in it (the folder's
        /// own time of writing); 0 when the folder is not there.</summary>
        public static long FolderStamp(string folder)
        {
            try
            {
                var info = new DirectoryInfo(folder);
                return info.Exists ? info.LastWriteTimeUtc.Ticks : 0L;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
            {
                return 0L;
            }
        }

        static bool Exists(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
            {
                return false;
            }
        }

        // ------------------------------------------------------------------ the drawing folder

        /// <summary>Is the picture in the drawing folder the user's own? It must have a placeholder of the same name, be
        /// of the placeholder's size, and not be the placeholder itself. Looked at once for each time it is written.</summary>
        bool Mine(string path, string file)
        {
            long stamp = Stamp(path);
            if (stamp == 0L)
                return false;
            if (drawChecked.TryGetValue(path, out var known) && known.stamp == stamp)
                return known.mine;
            bool mine = false;
            try
            {
                string holder = PlaceholderFolder != null ? Path.Combine(PlaceholderFolder, file) : null;
                if (holder != null && File.Exists(holder) && PngSize(path, out int w, out int h) && PngSize(holder, out int pw, out int ph) && w == pw && h == ph)
                    mine = !SameBytes(path, holder);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                return false;                                              // (half saved: look again next time, remember nothing)
            }
            drawChecked[path] = (stamp, mine);
            return mine;
        }

        /// <summary>A PNG's width and height from its own header (false: not a PNG).</summary>
        public static bool PngSize(string path, out int width, out int height)
        {
            width = height = 0;
            var head = new byte[24];
            using (var f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                if (f.Read(head, 0, 24) < 24)
                    return false;
            if (head[0] != 0x89 || head[1] != 'P' || head[2] != 'N' || head[3] != 'G' || head[12] != 'I' || head[13] != 'H' || head[14] != 'D' || head[15] != 'R')
                return false;
            width = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
            height = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
            return width > 0 && height > 0;
        }

        static bool SameBytes(string a, string b)
        {
            if (new FileInfo(a).Length != new FileInfo(b).Length)
                return false;
            byte[] x = File.ReadAllBytes(a), y = File.ReadAllBytes(b);
            if (x.Length != y.Length)
                return false;
            for (int i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;
            return true;
        }
    }
}
