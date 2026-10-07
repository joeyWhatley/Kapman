using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.Networking;

namespace Kapman.Game
{
    /// <summary>
    /// THE GAME'S MUSIC, FOR NOW (the user, 7 Oct 2026: "add this song to start playing softly as soon as the game is
    /// launched"; of four tracks cut from an EP, "add them as OST for now"; and "play them whenever you think they
    /// should be played with appropriate breaks like minecraft"). Music files standing in a folder beside the game are
    /// played softly:
    ///   Music/title.mp3     from the moment the game is up, going round, through the title and the opening;
    ///   Music/ost/*.mp3     once the game proper has begun. (.ogg and .wav are taken too.)
    ///
    /// WHEN THE GAME'S TRACKS PLAY. One at a time, never cut short, and mostly there is none: after a track comes a
    /// long quiet, four to nine minutes by the wall clock, as in Minecraft. A moment of the game may bring one sooner,
    /// once the quiet has lasted a while: the first morning (the main one); casting off (a farewell); first light (a
    /// morning one). When the quiet simply runs out, the one chosen goes with where she is: at sea a long drifting
    /// one, in port any other. Which track is which is read from its file's name (a farewell, a light, a tape), so
    /// other files put there fall into the same places by their names, or are taken in turn. Never the same twice
    /// running when there is another.
    ///
    /// THESE FILES ARE NOT THE GAME'S. They are read from the disk as the game runs; the folder is in no commit and no
    /// build. What the user put there on 7 Oct 2026 are published recordings (Fan-Hsiu Kadesch, "Ewige Blumenkraft",
    /// 1993; Gilgareth, "Hinab in die Zwergenbinge", 2024): stand-ins for the user's own ear while the game is made,
    /// and not to be shipped without their owners' leave. The project's rule for sound that ships is public domain or
    /// CC0 recordings only. With no files there, nothing plays and nothing is said.
    ///
    /// It follows the menu's Sound setting, and makes no noise in a batch run.
    /// </summary>
    public sealed class GameMusic : MonoBehaviour
    {
        public static GameMusic Current { get; private set; }

        /// <summary>How loud the title's music and the game's play at the Sound setting "middling" (softly).</summary>
        public static float TitleLevel = 0.2f, OstLevel = 0.16f;
        /// <summary>Seconds the music takes to come up and to die away.</summary>
        public static float ComeUp = 3.5f, DieAway = 5f;
        /// <summary>The quiet after a track, least and most (seconds by the wall clock); and how long it must have been
        /// quiet before casting off, or first light, may bring a track.</summary>
        public static float QuietLeast = 240f, QuietMost = 540f, QuietBeforeFarewell = 90f, QuietBeforeMorning = 120f;

        /// <summary>The folder beside the game the music is looked for in.</summary>
        public static string Folder => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Music"));

        static readonly string[] Kinds = { ".mp3", ".ogg", ".wav" };
        // what a track is for, by words in its file's name
        static readonly string[] MainWords = { "single", "theme", "main" };
        static readonly string[] FarewellWords = { "verabschied", "abschied", "farewell", "leave", "depart" };
        static readonly string[] MorningWords = { "licht", "light", "dawn", "morning" };
        static readonly string[] SeaWords = { "sea", "scroll", "tape", "drift", "long" };

        AudioSource title, ost;
        float titleShare, ostShare;
        readonly List<string> tracks = new List<string>();
        readonly System.Random dice = new System.Random();              // (its own: the game's dice are not touched)
        string last = "";
        float quiet, quietRunsOut = 6f;
        bool loading, started, begun, wasMoored, mooredKnown;
        double wasHour = -1.0;

        /// <summary>The track of the game's music playing now ("" for none), for whoever wants to say it.</summary>
        public string Playing { get; private set; } = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Current = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // AN OBJECT OF THE SCENE (KAPMAN, step 1; the user, 7 Oct 2026: "make it like people usually make it, not just all
        // in script, build it physically"). It stands in the Game scene under SYSTEMS, to be seen and set in the
        // Inspector. Where a scene has none (a test's own scene) it still makes itself, as it always did.
        static void First()
        {
            if (Current != null || Application.isBatchMode || !Directory.Exists(Folder))
                return;
            new GameObject("Game music").AddComponent<GameMusic>();
        }

        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);                                      // (the scene came again for a new game: the one that plays stays)
                return;
            }
            if (Application.isBatchMode || !Directory.Exists(Folder))
            {
                enabled = false;                                          // (no music beside the game, or a run without a screen: silent)
                return;
            }
            Current = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);                                // (a new game loads the scene again: the music goes on)
            title = Source();
            ost = Source();
            string dir = Path.Combine(Folder, "ost");
            if (Directory.Exists(dir))
                foreach (string file in Directory.GetFiles(dir))
                    if (Array.IndexOf(Kinds, Path.GetExtension(file).ToLowerInvariant()) >= 0)
                        tracks.Add(file);
            tracks.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (string kind in Kinds)
            {
                string file = Path.Combine(Folder, "title" + kind);
                if (File.Exists(file))
                {
                    StartCoroutine(Load(file, title, true, null));
                    break;
                }
            }
        }

        AudioSource Source()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.ignoreListenerPause = true;
            s.volume = 0f;
            return s;
        }

        IEnumerator Load(string file, AudioSource into, bool loop, Action done)
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            var kind = ext == ".ogg" ? AudioType.OGGVORBIS : ext == ".wav" ? AudioType.WAV : AudioType.MPEG;
            using (var req = UnityWebRequestMultimedia.GetAudioClip(new Uri(file).AbsoluteUri, kind))
            {
                // (played from the disk as it goes: a long track is not unpacked whole into memory)
                if (req.downloadHandler is DownloadHandlerAudioClip handler)
                    handler.streamAudio = true;
                yield return req.SendWebRequest();
                AudioClip clip = null;
                if (req.result == UnityWebRequest.Result.Success)
                    clip = DownloadHandlerAudioClip.GetContent(req);
                if (clip == null)
                    Debug.LogWarning("Kapman music: " + file + " could not be read (" + req.error + ")");
                else
                {
                    clip.name = Path.GetFileNameWithoutExtension(file) + " (from the Music folder; not shipped)";
                    if (into.clip != null)
                        Destroy(into.clip);
                    into.clip = clip;
                    into.loop = loop;
                    into.Play();
                }
            }
            done?.Invoke();
        }

        /// <summary>The game proper is on: not the title, not the opening.</summary>
        static bool InGame
        {
            get
            {
                var menu = MainMenu.Current;
                return menu != null && !menu.OnTitle && !Showtime.BeforeTheGame;
            }
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool game = InGame;
            float setting = ChartSound.SettingFactor;

            // the title's music: up while the title and the opening are on, away when the game begins
            titleShare = Mathf.MoveTowards(titleShare, game ? 0f : 1f, dt / Mathf.Max(0.05f, game ? DieAway : ComeUp));
            if (title.clip != null)
            {
                title.volume = Mathf.Clamp01(TitleLevel * setting * titleShare * titleShare);
                if (game && titleShare <= 0f && title.isPlaying)
                    title.Pause();
                else if (!game && !title.isPlaying)
                    title.UnPause();
            }

            // the game's music
            ostShare = Mathf.MoveTowards(ostShare, game ? 1f : 0f, dt / Mathf.Max(0.05f, game ? ComeUp : DieAway));
            ost.volume = Mathf.Clamp01(OstLevel * setting * ostShare * ostShare);
            if (!game)
            {
                if (ostShare <= 0f && ost.isPlaying)
                    ost.Pause();
                mooredKnown = false;
                return;
            }
            if (ost.clip != null && !ost.isPlaying && started && ost.time > 0f && ost.time < ost.clip.length - 0.5f)
                ost.UnPause();                                           // (back from the title: on from where it was)
            Moments(out bool castOff, out bool firstLight, out bool atSea);
            if (tracks.Count == 0 || loading || ost.isPlaying)
                return;
            if (started)
            {
                // (the track has run out: its long quiet begins)
                started = false;
                Playing = "";
                quiet = 0f;
                quietRunsOut = QuietLeast + (float)dice.NextDouble() * Mathf.Max(0f, QuietMost - QuietLeast);
            }
            quiet += dt;
            string file = null;
            if (!begun)
            {
                if (quiet >= quietRunsOut)
                    file = Pick(MainWords, null, true);                   // (the first morning: the main one)
            }
            else if (castOff && quiet >= QuietBeforeFarewell)
                file = Pick(FarewellWords, null, false);
            else if (firstLight && quiet >= QuietBeforeMorning)
                file = Pick(MorningWords, null, false);
            else if (quiet >= quietRunsOut)
                file = atSea ? Pick(SeaWords, null, true) : Pick(null, SeaWords, true);
            if (file == null)
                return;
            begun = true;
            last = file;
            loading = true;
            StartCoroutine(Load(file, ost, false, () =>
            {
                loading = false;
                started = ost.clip != null && ost.isPlaying;
                Playing = started ? Path.GetFileNameWithoutExtension(file) : "";
                if (!started)
                {
                    quiet = 0f;
                    quietRunsOut = 20f;
                }
            }));
        }

        /// <summary>What has just happened in the game that a track may answer: she has cast off; it is first light.</summary>
        void Moments(out bool castOff, out bool firstLight, out bool atSea)
        {
            bool moored = Port.LyingId() != null;
            castOff = mooredKnown && wasMoored && !moored;
            wasMoored = moored;
            mooredKnown = true;
            atSea = !moored;
            firstLight = false;
            var clock = LevelClock.Current;
            if (clock != null)
            {
                double hour = clock.HourOfDay;
                firstLight = wasHour >= 0.0 && wasHour < 5.5 && hour >= 5.5 && hour < 9.0;
                wasHour = hour;
            }
        }

        static bool Has(string file, string[] words)
        {
            if (words == null)
                return false;
            string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
            foreach (string w in words)
                if (name.Contains(w))
                    return true;
            return false;
        }

        /// <summary>A track whose name has one of `with` (null: any) and none of `without`; not the last one played when
        /// there is another. `orAny`: when none is named so, any other track will do; else none is played.</summary>
        string Pick(string[] with, string[] without, bool orAny)
        {
            var named = new List<string>();
            var others = new List<string>();
            foreach (string t in tracks)
            {
                if (t == last && tracks.Count > 1)
                    continue;
                if ((with == null || Has(t, with)) && !Has(t, without))
                    named.Add(t);
                else
                    others.Add(t);
            }
            if (named.Count > 0)
                return named[dice.Next(named.Count)];
            return orAny && others.Count > 0 ? others[dice.Next(others.Count)] : null;
        }
    }
}
