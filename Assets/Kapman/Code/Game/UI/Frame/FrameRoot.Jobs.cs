using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// BEFORE SHE SAILS, and why she may not (the critic's play of 8 Oct 2026, B1 and B4, taken up by the user the same
    /// day). Three things of the frame that hold a new player's thread:
    ///   - THE LIST under the buttons, from the first morning until she first casts off: the sail, the crew, a cargo, a
    ///     course, cast off. Each row says how it stands and has the button that does it or goes where it is done; a row
    ///     done is ticked. It is laid out in Game/UI/Frame.uxml ("jobs": the rows job-sail, job-crew, job-cargo,
    ///     job-course, job-cast-off, each with its labels ending -name and -note and a button of the class frame-job-do), and the
    ///     rules are FirstJobs.
    ///   - THE REASON under the buttons ("rack-why") whenever Cast off is grey in a port and the list is not there to
    ///     say it: the same words the button gives when the pointer rests on it.
    ///   - THE LANDING SLIP: when she makes fast after a passage, one framed line in the log (Landings; the words are
    ///     FrameGame.LandingWords), and on the first landing of a game begun through the opening a line from Ravn.
    /// </summary>
    public partial class FrameRoot
    {
        sealed class Job
        {
            public VisualElement row;
            public Label name, note;
            public Button button;
        }

        VisualElement jobs;
        Label rackWhy;
        Job jobSail, jobCrew, jobCargo, jobCourse, jobCastOff;
        float nextJobs;

        /// <summary>The list "Before she sails" is on the screen now.</summary>
        public bool JobsShown => jobs != null && jobs.resolvedStyle.display != DisplayStyle.None;

        partial void JobsBuild()
        {
            jobs = Part<VisualElement>("jobs");
            rackWhy = Part<Label>("rack-why");
            jobSail = JobOf("job-sail", () =>
            {
                var game = KapmanGame.Current;
                FirstJobs.Mend(game != null ? game.Day : 0.0, out string fact);
                if (!string.IsNullOrEmpty(fact))
                    FrameLog.Add(fact);
            });
            jobCrew = JobOf("job-crew", () => Do(FrameIcon.Kind.Alehouse));
            jobCargo = JobOf("job-cargo", () => Do(FrameIcon.Kind.Trade));
            jobCourse = JobOf("job-course", () => Do(FrameIcon.Kind.Voyage));
            jobCastOff = JobOf("job-cast-off", () => Do(FrameIcon.Kind.CastOff));
            jobs.style.display = DisplayStyle.None;
            Line(rackWhy, "");
            nextJobs = 0f;
        }

        Job JobOf(string rowName, Action does)
        {
            var row = Part<VisualElement>(rowName);
            var job = new Job
            {
                row = row, name = FrameParts.In<Label>(row, rowName + "-name"), note = FrameParts.In<Label>(row, rowName + "-note"),
                button = row.Q<Button>(className: "frame-job-do"),
            };
            if (job.button == null)
            {
                Debug.LogError("Kapman frame: the row \"" + rowName + "\" of the list Before she sails (Frame.uxml) has no button of the class frame-job-do.");
                job.button = new Button();
                job.button.AddToClassList("frame-k");
                job.button.AddToClassList("frame-job-do");
                row.Add(job.button);
            }
            job.button.clicked += () =>
            {
                does();
                nextJobs = 0f;
            };
            return job;
        }

        static void Set(Job job, bool done, string name, string note, string button, bool can = true, string tip = null)
        {
            job.row.EnableInClassList("done", done);
            job.name.text = name ?? "";
            Line(job.note, done ? "" : note);
            bool has = !done && !string.IsNullOrEmpty(button);
            job.button.style.display = has ? DisplayStyle.Flex : DisplayStyle.None;
            job.button.text = button ?? "";
            job.button.SetEnabled(can);
            job.button.tooltip = tip ?? "";
        }

        partial void JobsUpdate()
        {
            if (jobs == null)
                return;
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            // (the landing is looked for every frame, so its slip is of the moment she made fast at any speed of the clock)
            LandingLook(game, here);
            if (Time.realtimeSinceStartup < nextJobs)
                return;
            nextJobs = Time.realtimeSinceStartup + 0.2f;
            // (the list stands while she lies at the port of the jobs, Ravn talking or not)
            string lying = here != null ? here.id : Port.LyingId();
            bool listed = game != null && FirstJobs.Listed && lying != null && lying == FirstJobs.Port;
            jobs.style.display = listed ? DisplayStyle.Flex : DisplayStyle.None;
            if (listed)
                JobsFill(game);
            // the reason under the buttons: only where the list does not already say it
            string whyNot = "";
            if (!listed && here != null && game != null && !Showtime.Busy)
            {
                Look(FrameIcon.Kind.CastOff, out _, out string why, out bool can, out _);
                if (!can)
                    whyNot = why;
            }
            Line(rackWhy, whyNot);
        }

        void JobsFill(KapmanGame game)
        {
            // ---- the sail (the user's own words for it, 8 Oct 2026: "Sails tattered.... / Needs to be repaired." Then the
            // cost next to it: the cost is the button)
            if (!FirstJobs.Active || !FirstJobs.SailWorn)
                Set(jobSail, true, "Sails mended", null, null);
            else if (FirstJobs.Mending)
            {
                double hours = Math.Max(0.0, (FirstJobs.MendDoneAt - game.Day) * 24.0);
                Set(jobSail, false, "Sails ashore", hours >= 1.0 ? $"Being mended. About {Math.Ceiling(hours):0} {(Math.Ceiling(hours) == 1.0 ? "hour" : "hours")} yet. Let the clock run."
                                                                 : "Being mended. Nearly done.", null);
            }
            else
                Set(jobSail, false, "Sails tattered....", "Needs to be repaired.", $"{FirstJobs.MendCost:0} d", FrameGame.Silver >= FirstJobs.MendCost,
                    $"Have the sails repaired: {FirstJobs.MendCost:0} d, and six hours' work.");

            // ---- the crew
            int short_ = FirstJobs.HandsShort;
            string crew = $"Crew: {Math.Min(Voyage.Men, FirstJobs.CrewWanted)} of {FirstJobs.CrewWanted}";
            if (short_ <= 0)
                Set(jobCrew, true, crew, null, null);
            else
                // (no note: the count and the button say it, and the list is kept short)
                Set(jobCrew, false, crew, null, "Alehouse", true,
                    $"The men looking for a boat sit in the alehouse. The hiring silver is {FirstJobs.HandMoney:0} d a man.");

            // ---- a cargo: not asked of her, so it never stops her; Ravn's word stands here until something is bought
            if (FirstJobs.Bought)
                Set(jobCargo, true, "Cargo bought", null, null);
            else
            {
                var good = GameData.Good(FirstJobs.TipGood);
                var port = GameData.Port(FirstJobs.TipPort);
                Set(jobCargo, false, "A cargo", good != null && port != null ? $"Ravn's word: {good.name.ToLowerInvariant()}, for {port.name}." : "Buy what she will hold, or sail with what she has.",
                    "Trade", true, "The market of this port.");
            }

            // ---- a course
            if (Course.IsSet)
                Set(jobCourse, true, "Course: " + (GameData.Port(Course.Destination)?.name ?? Course.Destination), null, null);
            else
                Set(jobCourse, false, "No course set", null, "Voyage", true, "Pick a port and set its course.");

            // ---- cast off: what she still wants, or the button
            var wants = FirstJobs.Wanting(Course.IsSet);
            if (wants.Count > 0)
            {
                string list = wants.Count == 1 ? wants[0] : string.Join(", ", wants.GetRange(0, wants.Count - 1)) + " and " + wants[wants.Count - 1];
                Set(jobCastOff, false, "Cast off", "First " + list + ".", null);
            }
            else
            {
                Look(FrameIcon.Kind.CastOff, out _, out string why, out bool can, out _);
                Set(jobCastOff, false, "Cast off", can ? "She is ready." : why, "Cast off", can, why);
            }
            jobCastOff.row.EnableInClassList("ready", wants.Count == 0);
        }

        // ------------------------------------------------------------------ the landing slip

        void LandingLook(KapmanGame game, PortDef here)
        {
            if (game == null || game.ship == null || !Trade.Started || Showtime.Busy)
                return;
            // where she lies: the port whose business is open; with that shut but she still lying there (a card over the
            // port's menu, or lying to off the quay she has cast off from) it is as it was; under way it is nowhere
            string at = here != null ? here.id : Port.LyingId() != null ? Landings.Lying : "";
            var made = Landings.Look(at, game.Day, FrameGame.Silver, () => FrameGame.LandingFigures(here, game));
            if (made == null)
                return;
            FrameLog.Add(FrameGame.LandingWords(made), "slip");
            string ravn = FrameGame.RavnAtLanding(made, Landings.All.Count);
            if (ravn.Length > 0)
                FrameLog.Add(ravn);
        }
    }
}
