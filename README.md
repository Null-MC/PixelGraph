# <img src="https://raw.githubusercontent.com/null511/PixelGraph-Release/master/media/icon.png" height="28"/>&nbsp; PixelGraph&nbsp; [![Actions Status](https://github.com/null511/PixelGraph/workflows/BuildTest/badge.svg)](https://github.com/null511/PixelGraph/actions) [![Actions Status](https://github.com/null511/PixelGraph-Release/workflows/Release/badge.svg)](https://github.com/null511/PixelGraph-Release/actions)

PixelGraph is an application for publishing Minecraft resource packs, specially tooled for PBR materials. It allows you to work in a "raw" texture space and automates publishing to one or more encodings, rather than trying to directly encode your textures as design-time. Yaml configuration files can also be used to apply final adjustments to your compiled textures. A cross-platform command-line version is also available, allowing you to completely automating your publishing process from your remote content repository.

> :warning: **PixelGraph is now licensed for commercial usage!** For authors with a combined total income over $30/month from Pixel-Graph published projects, please see [Licensing](https://github.com/null511/PixelGraph-Release/wiki/Licensing). For personal projects or authors not meeting the license criteria, usage is completely free.
----
<img src="https://github.com/null511/PixelGraph-Release/raw/main/media/UI.png" alt="User Interface" />

 - **Simplify your workflow** by adjusting text instead of pixels. Getting image-based material values just right can be tedious, time consuming, and destructive.

 - **Preserve Quality** by adjusting material values through text rather than altering the original image data. Repeatedly scaling the integer-based channels of your image slowly destroys quality. Save the gradients!

 - **Support more users** by publishing multiple packs with varying quality. The resolution and included textures can be altered using either the command-line or Publishing Profiles to create multiple distributions.

 - **Automate** Normal & AO generation, resizing, and channel-swapping so that you can spend more time designing and less time repeating yourself.

<img src="https://github.com/null511/PixelGraph-Release/raw/main/media/LAB11.png" alt="PBR Workflow" />

### Normal-Map Generation

Allows normal-map textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Strength, blur, and wrapping can be managed using the textures matching pbr-properties file.

<img src="https://github.com/null511/PixelGraph-Release/raw/main/media/NormalGeneration.png" alt="Normal-Map from Height-Map" height="180px"/>
 
### Occlusion-Map Generation

Allows ambient-occlusion textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Quality, Z-scale, step-count, and wrapping can be managed using the materials properties.

<img src="https://github.com/null511/PixelGraph-Release/raw/main/media/OcclusionGeneration.png" alt="Occlusion-Map from Height-Map" height="180px"/>

## Installation

For manual installation, download the latest standalone executable from the [Releases](https://github.com/null511/PixelGraph-Release/releases) page. For automated usage see [Docker Usage](https://github.com/null511/PixelGraph-Release/wiki/Installation#docker). Visit the [wiki](https://github.com/null511/PixelGraph-Release/wiki/Installation) for more information.

## Manual Usage

A single Pack-Input file lives in the root of the workspace (`~/input.yml`) which specifies the default formatting of all content. The example below uses a 'raw' encoding which manages each channel in a different texture; The 'height' channel has been set to inverted.

```yml
# ~/input.yml
format: raw
height:
  invert: true
```

One or more Pack-Profiles are used to describe a publishing routine; they also live in the project root and should match the naming convention `~/<name>.pack.yml`. Each profile can specify pack details, encoding, format, resizing, etc; this allows a single set of content to be published for multiple resolutions and encodings, ie `pbr-lab1.3-64x` or `default-128x`

```yml
# ~/pbr-lab13-x64.yml
output:
  format: default
texture-scale: 0.5
```

Material files are used to desribe a collection of textures that compose a single game "item". For more details, see the [Wiki](https://github.com/null511/PixelGraph-Release/wiki/File-Loading).
```yml
# ~/assets/minecraft/textures/block/lantern.mat.yml
smooth:
  scale: 1.2
metal:
  scale: 0.8
emissive:
  scale: 0.2
```

## Sample Repository

[Oversized](https://github.com/null511/MCRP-Oversized) - A high-resolution texture pack made primarily using stock textures and CTM.

[Textureless](https://github.com/null511/MCRP-Textureless) - A low-resolution pack using solid colors, material values, and alpha-masking to provide a smooth visual base for shader testing. Also good for toon/outline style.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Support
Ask about PixelGraph in the [ShaderLabs Discord](https://discord.gg/PG9RmWTBU9) under `#pixelgraph`.
