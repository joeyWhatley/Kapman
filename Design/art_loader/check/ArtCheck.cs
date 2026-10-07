// Runs ArtFiles (the part of the loader that knows nothing of Unity) for real, on a folder of small files made in the
// temp folder. Not part of the game.   dotnet run --project Design/art_loader/check
using System;
using System.IO;
using Kapman.Game;

namespace ArtCheck
{
    static class Program
    {
        static int failed, passed;

        static void Check(bool ok, string what)
        {
            if (ok)
                passed++;
            else
                failed++;
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
        }

        /// <summary>A file that starts as a PNG of a size (its header alone is real) and ends with a word, so two files
        /// of the same size can differ.</summary>
        static string Png(string folder, string file, int w, int h, string word)
        {
            string path = Path.Combine(folder, file);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var head = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52,
                                    (byte)(w >> 24), (byte)(w >> 16), (byte)(w >> 8), (byte)w, (byte)(h >> 24), (byte)(h >> 16), (byte)(h >> 8), (byte)h };
            using (var f = new FileStream(path, FileMode.Create))
            {
                f.Write(head, 0, head.Length);
                var tail = System.Text.Encoding.ASCII.GetBytes(word);
                f.Write(tail, 0, tail.Length);
            }
            return path;
        }

        static bool Is(string got, string folder, string file) =>
            got != null && string.Equals(Path.GetFullPath(got), Path.GetFullPath(Path.Combine(folder, file)), StringComparison.OrdinalIgnoreCase);

        static int Main()
        {
            string root = Path.Combine(Path.GetTempPath(), "kapman_artcheck_" + Guid.NewGuid().ToString("N"));
            string final = Path.Combine(root, "final"), holder = Path.Combine(root, "placeholder"), desk = Path.Combine(root, "desk");
            Directory.CreateDirectory(holder);
            const string own = "portrait/person.testport.tollman.png", role = "portrait/role.tollman.png";
            try
            {
                var files = new ArtFiles(final, holder);
                Console.WriteLine("THE ORDER (a drawn stand-in before a slot's own placeholder)");
                Check(files.Find(new[] { own, role }, false, out bool drawn) == null && !drawn, "nothing there: none");
                Png(holder, role, 64, 80, "role placeholder");
                Check(Is(files.Find(new[] { own, role }, false, out drawn), holder, role) && !drawn, "only the role's placeholder: the role's placeholder");
                Png(holder, own, 64, 80, "own placeholder");
                Check(Is(files.Find(new[] { own, role }, false, out drawn), holder, own) && !drawn, "the person's own placeholder before the role's");
                Png(final, role, 64, 80, "role final");
                Check(Is(files.Find(new[] { own, role }, false, out drawn), final, role) && drawn, "the user's stand-in before the person's own placeholder");
                Png(final, own, 64, 80, "own final");
                Check(Is(files.Find(new[] { own, role }, false, out drawn), final, own) && drawn, "the user's own picture first of all");
                Check(Is(files.Find(own), final, own), "a slot alone: final over the placeholder of the same name");
                Check(files.Drawn(own) && !files.Drawn("portrait/role.smith.png"), "drawn, and not drawn");
                Check(files.Find(new[] { "portrait/role.smith.png" }, true, out drawn) == null, "the user's only: a slot with a placeholder alone gives none");
                Check(Is(files.Find(new[] { null, "", role }, false, out drawn), final, role), "empty names are passed over");

                Console.WriteLine("THE OTHER READING (a slot's own placeholder before any stand-in)");
                File.Delete(Path.Combine(final, own));
                files.StandInBeforePlaceholder = false;
                Check(Is(files.Find(new[] { own, role }, false, out drawn), holder, own) && !drawn, "own placeholder, though the stand-in is drawn");
                Check(Is(files.Find(new[] { "portrait/person.nobody.png", role }, false, out drawn), final, role) && drawn, "no slot of their own: the drawn stand-in");
                files.StandInBeforePlaceholder = true;

                Console.WriteLine("NOTHING LEAVES THE ART FOLDERS");
                Check(!ArtFiles.Valid("../x.png") && !ArtFiles.Valid("C:/x.png") && !ArtFiles.Valid("/x.png") && !ArtFiles.Valid("") && ArtFiles.Valid("good/good.wax.png"), "odd names are refused");
                Check(files.Find("../placeholder/" + role) == null, "a name with .. finds nothing");

                Console.WriteLine("CHANGES ON DISK (this is what the game's once-a-second look relies on)");
                string p = Path.Combine(holder, own);
                long s1 = ArtFiles.Stamp(p);
                Check(s1 != 0 && ArtFiles.Stamp(p) == s1, "a file left alone keeps its stamp");
                File.SetLastWriteTimeUtc(p, File.GetLastWriteTimeUtc(p).AddSeconds(2));
                Check(ArtFiles.Stamp(p) != s1, "written again (a later time): the stamp moves");
                long s2 = ArtFiles.Stamp(p);
                var when = File.GetLastWriteTimeUtc(p);
                File.AppendAllText(p, "more");
                File.SetLastWriteTimeUtc(p, when);
                Check(ArtFiles.Stamp(p) != s2, "written again at the very same time, but longer: the stamp moves");
                Check(ArtFiles.Stamp(Path.Combine(holder, "portrait/none.png")) == 0, "a file that is not there: 0");
                string dir = Path.Combine(final, "good");
                Check(ArtFiles.FolderStamp(dir) == 0, "a folder that is not there: 0");
                Png(final, "good/good.wax.png", 32, 32, "wax");
                long d1 = ArtFiles.FolderStamp(dir);
                Check(d1 != 0, "the folder is made: its stamp is no longer 0");
                System.Threading.Thread.Sleep(1100);
                Png(final, "good/good.amber.png", 32, 32, "amber");
                long d2 = ArtFiles.FolderStamp(dir);
                Check(d2 != d1, "a picture added: the folder's stamp moves");
                System.Threading.Thread.Sleep(1100);
                File.Delete(Path.Combine(dir, "good.amber.png"));
                Check(ArtFiles.FolderStamp(dir) != d2, "a picture taken away: the folder's stamp moves");
                var watched = new System.Collections.Generic.List<string>();
                files.Folders(own, watched);
                Check(watched.Count == 2, "a slot is watched in two folders (final and placeholder)");

                Console.WriteLine("THE DRAWING FOLDER");
                Png(holder, "hud/hud.fn_button.png", 36, 24, "the placeholder");
                Directory.CreateDirectory(Path.Combine(desk, "hud"));
                File.Copy(Path.Combine(holder, "hud/hud.fn_button.png"), Path.Combine(desk, "hud/hud.fn_button.png"));
                files.DrawFolder = desk;
                Check(!files.Drawn("hud/hud.fn_button.png"), "still the placeholder: not the user's");
                Png(desk, "hud/hud.fn_button.png", 30, 24, "drawn, but another size");
                File.SetLastWriteTimeUtc(Path.Combine(desk, "hud/hud.fn_button.png"), DateTime.UtcNow.AddSeconds(5));
                Check(!files.Drawn("hud/hud.fn_button.png"), "another size than the slot's: not taken");
                string mine = Png(desk, "hud/hud.fn_button.png", 36, 24, "drawn by hand");
                File.SetLastWriteTimeUtc(mine, DateTime.UtcNow.AddSeconds(10));
                Check(files.Drawn("hud/hud.fn_button.png") && Is(files.Final("hud/hud.fn_button.png"), desk, "hud/hud.fn_button.png"), "the slot's size, no longer the placeholder: the user's");
                watched.Clear();
                files.Folders("hud/hud.fn_button.png", watched);
                Check(watched.Count == 3, "and the drawing folder is watched too");
                Check(ArtFiles.PngSize(mine, out int w, out int h) && w == 36 && h == 24, "a PNG's size from its header");
            }
            finally
            {
                try
                {
                    Directory.Delete(root, true);
                }
                catch (IOException)
                {
                }
            }
            Console.WriteLine();
            Console.WriteLine(failed == 0 ? "ALL " + passed + " CHECKS PASSED" : failed + " OF " + (passed + failed) + " CHECKS FAILED");
            return failed == 0 ? 0 : 1;
        }
    }
}
