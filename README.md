packrat
===
A flexible, free, open-source texture atlas creation and packaging tool.

## Features

- **Generate game-ready texture atlases** from multiple standalone source textures / tiles.
- **Generate atlas-friendly mipmaps** with periodic downsampling (no blurring between textures at higher mip levels).
- **Downsample with your choice** of bilinear, bicubic, nearest neighbor, etc.
- **Scriptable and shell-ready** for command-line builds and workflows.
- **Supports Windows, OS X, Linux** and other Mono-friendly platforms.
- **Engine- and subsystem- agnostic**; compatible with DirectX, OpenGL, Unity, Unreal, etc.

## Planned
- **Additional output formats** including DDS and TIF.
- **Exotic atlas variations** including 4-tap textures and mirrored atlases.
- **Texture adjustments** include padding and half-texel correction.
- **Non-regular textures** with different sizes.
- **Bin-packing** of irregular textures.
- **Better metadata support** including .TAI files.

## Use

From the shell:

```shell
packrat ATLAS FILES [OPTIONS]
```

Where `ATLAS` is the output atlas filename (without the extension), `FILES` is a wildcard or glob describing the texture files to pack, and `OPTIONS` describes any optional settings. For example:

```shell
# EXAMPLE 1
# Pack all PNGs in the /foo folder into a texture atlas
# 16 tiles across using default (bilinear) interpretation:
packrat myatlas foo/*.png -tx 16

# EXAMPLE 2
# Pack all JPEGs in the /foo folder into a texture atlas
# 32 tiles across using nearest-neighbor interpolation:
packrat myatlas foo/*.jpg -tx 32 -i nearestneighbor

# EXAMPLE 3
# As above, but enable silent mode (no console output):
packrat myatlas foo/*.png -s -tx 32 -i nearestneighbor
```

## License

Licensed under MIT, have fun. Copyright &copy; 2015-16 by [hacksalot](http://github.com/hacksalot).
