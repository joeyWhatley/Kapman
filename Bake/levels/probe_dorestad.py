from osgeo import gdal, osr, ogr
gdal.UseExceptions()
src = osr.SpatialReference(); src.ImportFromEPSG(4326); src.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
rd = osr.SpatialReference(); rd.ImportFromEPSG(28992); rd.SetAxisMappingStrategy(osr.OAMS_TRADITIONAL_GIS_ORDER)
t = osr.CoordinateTransformation(src, rd)
x, y, _ = t.TransformPoint(5.343, 51.972)
print("Dorestad RD", round(x), round(y))
ds = ogr.Open(r"G:\kapman_unity\data\work\realground\cache\nl_vos800_rd.gpkg")
lyr = ds.GetLayer(0)
print("layer", lyr.GetName(), [lyr.GetLayerDefn().GetFieldDefn(i).GetName() for i in range(lyr.GetLayerDefn().GetFieldCount())])
lyr.SetSpatialFilterRect(x - 3000, y - 3000, x + 3000, y + 3000)
seen = {}
for f in lyr:
    k = tuple(str(f.GetField(i))[:30] for i in range(min(4, f.GetFieldCount())))
    seen[k] = seen.get(k, 0) + f.GetGeometryRef().Intersection(ogr.CreateGeometryFromWkt("POLYGON((%f %f,%f %f,%f %f,%f %f,%f %f))" % (x-3000,y-3000,x+3000,y-3000,x+3000,y+3000,x-3000,y+3000,x-3000,y-3000))).GetArea() / 1e4
for k, v in sorted(seen.items(), key=lambda kv: -kv[1]):
    print(" %8.1f ha" % v, k)
