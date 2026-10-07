#!/usr/bin/env bash
# Compiles the art loader's drafts WITHOUT Unity, with the compiler that ships with the editor and the references and
# defines of the project's own response files (Library/Bee/artifacts/1900b0aE.dag/Kapman.*.rsp). Everything it writes
# goes to the temp folder; nothing in the project is touched.
#
#   bash Design/art_loader/check/compile_check.sh          (Git Bash)
#
# What it compiles, in order:
#   Kapman.Core and Kapman.Runtime from the sources on disk (so the check matches the project as it is now, not as
#     Unity last compiled it)
#   0      Kapman.Game as on disk + ArtFiles.cs, ArtSlots.cs, FrameSkin.cs, nothing else changed
#   1..6   the same after each step of tools/make_hooks.py (FrameArt.cs left out from step 1; FrameSkin.cs in at step 6)
#   the editor step (ArtBuild.cs) and the two test files, against the step 6 assembly
HERE="$(cd "$(dirname "$0")" && pwd -W 2>/dev/null || pwd)"
P="$(cd "$HERE/../../.." && pwd -W 2>/dev/null || pwd)"
D="$P/Design/art_loader/draft"
R="$P/Library/Bee/artifacts/1900b0aE.dag"
T="$(cd "${TMPDIR:-${TEMP:-/tmp}}" && pwd -W 2>/dev/null || pwd)/kapman_art_compile"
ED="C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Data"
mkdir -p "$T"

PY() { cmd //c "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" "$@"; }
CSC() { "$ED/DotNetSdk/dotnet.exe" "$ED/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll" "$@"; }
# a response file's references and defines, with the project's own assemblies taken from the copies built here
BASE() {
  grep -v '^-out:\|^-refout:\|^"Assets\|^-analyzer\|^/additionalfile\|^/debug' "$1" |
    sed "s|^-r:\"Library/Bee/artifacts/1900b0aE.dag/Kapman.Core.ref.dll\"|-r:\"$T/Kapman.Core.dll\"|
         s|^-r:\"Library/Bee/artifacts/1900b0aE.dag/Kapman.Runtime.ref.dll\"|-r:\"$T/Kapman.Runtime.dll\"|
         s|^-r:\"Library/Bee/artifacts/1900b0aE.dag/Kapman.Game.ref.dll\"|-r:\"$T/step6/Kapman.Game.dll\"|
         s|^-r:\"Library/|-r:\"$P/Library/|; s|^-r:\"Packages/|-r:\"$P/Packages/|"
}
SAY() { echo "$1: exit $2, errors $(grep -c 'error CS' "$3")"; grep 'error CS' "$3" | head -5; }

cd "$T" || exit 1
{ BASE "$R/Kapman.Core.rsp"; echo "-out:\"$T/Kapman.Core.dll\""; find "$P/Assets/Kapman/Code/Core" -name "*.cs" | sed 's|.*|"&"|'; } > core.rsp
CSC @core.rsp > core.txt 2>&1; SAY "Kapman.Core, as on disk" $? core.txt
{ BASE "$R/Kapman.Runtime.rsp"; echo "-out:\"$T/Kapman.Runtime.dll\""; find "$P/Assets/Kapman/Code/Runtime" -name "*.cs" | sed 's|.*|"&"|'; } > runtime.rsp
CSC @runtime.rsp > runtime.txt 2>&1; SAY "Kapman.Runtime, as on disk" $? runtime.txt

find "$P/Assets/Kapman/Code/Game" -name "*.cs" > gamefiles.txt
echo "Kapman.Game: $(wc -l < gamefiles.txt) files on disk"
{ BASE "$R/Kapman.Game.rsp"; echo "-out:\"$T/Kapman.Game.dll\""; sed 's|.*|"&"|' gamefiles.txt
  echo "\"$D/ArtFiles.cs\""; echo "\"$D/ArtSlots.cs\""; echo "\"$D/FrameSkin.cs\""; } > step0.rsp
CSC @step0.rsp > step0.txt 2>&1; SAY "0  the game + the three drafts, nothing hooked" $? step0.txt

for N in 1 2 3 4 5 6; do
  PY "$D/tools/make_hooks.py" "$T/step$N" --upto $N > "hooks$N.txt" 2>&1
  grep 'NOT FOUND' -A1 "hooks$N.txt"
  { BASE "$R/Kapman.Game.rsp"; echo "-out:\"$T/step$N/Kapman.Game.dll\""
    while read -r f; do
      rel="${f#$P/Assets/Kapman/Code/Game/}"
      if [ "$rel" = "UI/Frame/FrameArt.cs" ]; then continue; fi
      if [ -f "$T/step$N/$rel" ]; then echo "\"$T/step$N/$rel\""; else echo "\"$f\""; fi
    done < gamefiles.txt
    echo "\"$D/ArtFiles.cs\""; echo "\"$D/ArtSlots.cs\""
    if [ "$N" = 6 ]; then echo "\"$D/FrameSkin.cs\""; fi; } > "step$N.rsp"
  CSC @"step$N.rsp" > "step$N.txt" 2>&1; SAY "$N  the game with steps 1 to $N hooked in" $? "step$N.txt"
done

{ BASE "$R/Kapman.Editor.rsp"; echo "-out:\"$T/ArtBuild.dll\""; echo "\"$D/ArtBuild.cs\""; } > editor.rsp
CSC @editor.rsp > editor.txt 2>&1; SAY "the editor step (ArtBuild.cs)" $? editor.txt
{ BASE "$R/Kapman.Tests.EditMode.rsp"; echo "-out:\"$T/ArtTests.dll\""; echo "\"$D/ArtSlotsTests.cs\""; echo "\"$D/FrameSkinTests.cs\""; } > tests.rsp
CSC @tests.rsp > tests.txt 2>&1; SAY "the tests (ArtSlotsTests.cs, FrameSkinTests.cs)" $? tests.txt
echo "written to $T"
