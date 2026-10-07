# The scale comparison page

The page for https://claude.ai/artifact/LeuyBNiqdnW95iv9H5NG53 (4 Oct 2026). Its data comes from `make_scale_maps.py`
(run.bat world\make_scale_maps.py, about 6 minutes) -> G:\kapman_unity\data\scale_maps.

To publish:
1. Copy maps.json beside index.html.
2. Base64 each .bin to NAME.b64.txt (artifacts serve text, not .bin): `base64 -w0 x.bin > x.b64.txt`.
3. Publish index.html with every file in `files`. To keep the same link, publish with the URL above as `url`.
