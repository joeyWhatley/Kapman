# -*- coding: utf-8 -*-
"""Compiles Kapman's own C# WITHOUT starting Unity, so an engineer who may not run Unity can still see his compile errors.

Unity leaves the exact compiler arguments of its last compile in Library/Bee/artifacts/<x>.dag/<Assembly>.rsp. This reads
them, swaps the list of source files for every .cs that is in the assembly's folder NOW (so new files are in), and runs
the same Roslyn compiler Unity ships, writing the results to a temp folder (nothing in the project is touched). The
assemblies are compiled in the order they depend on each other, each against the fresh copy of the ones before it:

    Kapman.Core  ->  Kapman.Game  ->  Kapman.Graphs  ->  Kapman.Editor  ->  Kapman.Tests.EditMode  ->  Kapman.Tests.PlayMode

Run it (about 20 seconds):
    "C:\\Program Files\\QGIS 3.44.12\\bin\\python-qgis-ltr.bat" G:\\kapman_unity\\Kapman2\\Bake\\content\\compile_check.py
    ... compile_check.py Kapman.Game            (stop after that assembly)

It prints each assembly's errors (error CS....) and ends with "COMPILES" or "DOES NOT COMPILE". Warnings are not shown.
It is a check, not the truth: it needs one earlier Unity compile of the project (the .rsp files), it does not run a
test, and Unity itself still has the last word. It is safe to run while Unity is running. Written 7 Oct 2026.
"""
import glob
import io
import os
import subprocess
import sys
import tempfile

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
UNITY_DATA = r"C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Data"

# assembly name, its folder under the project (every .cs in it, and below it, is its source)
ASSEMBLIES = [
    ("Kapman.Core", "Assets/Kapman/Code/Core"),
    ("Kapman.Game", "Assets/Kapman/Code/Game"),
    ("Kapman.Graphs", "Assets/Kapman/Code/Graphs"),
    ("Kapman.Editor", "Assets/Kapman/Editor"),
    ("Kapman.Tests.EditMode", "Assets/Kapman/Tests/EditMode"),
    ("Kapman.Tests.PlayMode", "Assets/Kapman/Tests/PlayMode"),
]


def newest(pattern):
    found = glob.glob(pattern)
    return max(found, key=os.path.getmtime) if found else None


def sources(folder):
    out = []
    for base, dirs, files in os.walk(os.path.join(ROOT, folder)):
        dirs[:] = [d for d in dirs if not d.startswith(".") and not d.endswith("~")]
        for f in sorted(files):
            if f.endswith(".cs"):
                out.append(os.path.relpath(os.path.join(base, f), ROOT).replace(os.sep, "/"))
    return sorted(out)


def main():
    stop_after = sys.argv[1] if len(sys.argv) > 1 else None
    dotnet = os.path.join(UNITY_DATA, "NetCoreRuntime", "dotnet.exe")
    csc = newest(os.path.join(UNITY_DATA, "DotNetSdk", "sdk", "*", "Roslyn", "bincore", "csc.dll"))
    if not os.path.exists(dotnet) or not csc:
        print("The compiler Unity ships was not found under " + UNITY_DATA)
        return 2
    out_dir = os.path.join(tempfile.gettempdir(), "kapman_compile_check")
    os.makedirs(out_dir, exist_ok=True)
    made = {}      # assembly name -> the dll made here
    failed = False
    for name, folder in ASSEMBLIES:
        rsp = newest(os.path.join(ROOT, "Library", "Bee", "artifacts", "*.dag", name + ".rsp"))
        if not rsp:
            print("%-24s skipped: Unity has not compiled it yet (no %s.rsp)" % (name, name))
            continue
        args = []
        for line in io.open(rsp, encoding="utf-8-sig").read().splitlines():
            s = line.strip()
            if not s:
                continue
            if s.startswith("-out:") or s.startswith("-refout:") or s.startswith("/out:") or s.startswith("/refout:"):
                continue
            if s.startswith('"') and s.rstrip('"').endswith(".cs"):
                continue                                           # the old list of sources
            if s.startswith("-r:") or s.startswith("/r:") or s.startswith("-reference:"):
                # a Kapman assembly compiled a moment ago: use that copy, not Unity's older one
                ref = s.split(":", 1)[1].strip('"').replace("\\", "/")
                ref_name = os.path.basename(ref)
                for done in made:
                    if ref_name in (done + ".ref.dll", done + ".dll"):
                        s = '-r:"%s"' % made[done].replace("\\", "/")
            args.append(s)
        src = sources(folder)
        dll = os.path.join(out_dir, name + ".dll")
        args.append('-out:"%s"' % dll.replace("\\", "/"))
        args.extend('"%s"' % f for f in src)
        new_rsp = os.path.join(out_dir, name + ".rsp")
        io.open(new_rsp, "w", encoding="utf-8").write("\n".join(args) + "\n")
        run = subprocess.run([dotnet, "exec", csc, "/noconfig", "@" + new_rsp], cwd=ROOT, stdout=subprocess.PIPE,
                             stderr=subprocess.STDOUT)
        text = run.stdout.decode("utf-8", "replace")
        errors = sorted(set(l.strip() for l in text.splitlines() if ": error " in l or l.startswith("error ")))
        if run.returncode == 0 and not errors:
            print("%-24s ok   (%d files)" % (name, len(src)))
            made[name] = dll
        else:
            failed = True
            print("%-24s %d ERROR(S)" % (name, len(errors)))
            for e in errors[:60]:
                print("    " + e)
            if not errors:
                print("    " + text.strip()[:2000])
            # what depends on it cannot be judged: stop here
            break
        if stop_after == name:
            break
    print("DOES NOT COMPILE" if failed else "COMPILES")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
