@echo off
rem ============================================================================================
rem  KAPMAN BAKE - runs one bake script with QGIS's own Python (it has GDAL, OGR and numpy).
rem  Usage, from this folder:   run.bat layer1_land\bake_land.py
rem ============================================================================================
call "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" %*
