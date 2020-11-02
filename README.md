# PixelGraph [![Actions Status](https://github.com/null511/MC-PBR-Pipeline/workflows/Release/badge.svg)](https://github.com/null511/MC-PBR-Pipeline/actions)

PixelGraph (formerly MCPBRP) is an application for publishing Minecraft resource packs, with special tooling for post-processing PBR materials. Automate the workload of modifying and distributing your resource packs through simple property files. Supports a WPF-based desktop application as well as a command-line version for remote/server usage. Written in .NET Core; supports Windows, Linux, Mac. Docker ready.

<img src="https://github.com/null511/MC-PBR-Pipeline/raw/master/media/LAB11.png" alt="PBR Workflow" />

 - **Simplify your workflow** by adjusting text instead of pixels. Getting image-based material values just right can be tedious, time consuming, and destructive.

 - **Preserve Quality** by adjusting material values through text rather than altering the original image data. Repeatedly scaling the integer-based channels of your image slowly destroys quality. Save the gradients!

 - **Support more users** by publishing multiple packs with varying quality. The resolution and included textures can be altered using either the command-line or Publishing Profiles to create multiple distributions.

 - **Automate** Normal & AO generation, resizing, and channel-swapping so that you can spend more time designing and less time repeating yourself.
 
### Normal-Map Generation

<img src="https://github.com/null511/MC-PBR-Pipeline/raw/master/media/NormalGeneration.png" alt="Normal-Map from Height-Map" height="140px"/>

Allows normal-map textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Strength, blur, and wrapping can be managed using the textures matching pbr-properties file.
 
### Occlusion-Map Generation

<img src="https://github.com/null511/MC-PBR-Pipeline/raw/master/media/OcclusionGeneration.png" alt="Occlusion-Map from Height-Map" height="140px"/>

Allows ambient-occlusion textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Quality, Z-scale, step-count, and wrapping can be managed using the textures matching pbr-properties file.

## Installation

For manual installation, download the latest standalone executable from the [Releases](https://github.com/null511/MC-PBR-Pipeline/releases) page. For automated usage see [Docker Usage](https://github.com/null511/MC-PBR-Pipeline/wiki/Installation#docker). Visit the [wiki](https://github.com/null511/MC-PBR-Pipeline/wiki/Installation) for more information.

## Usage

Pack property files describe how the resource pack should be published, as well as an global properties affecting all textures. Multiple pack property files can be defined allowing packs with different descriptions, tags, resolution, channel-mappings, and more. Pre-defined input/output encoding have also been provided, including `lab-1.1` and `lab-1.3`.

```ini
# ~/pack.properties
input.format = default
output.format = lab-1.3
```

Each item-texture requires a matching `*.pbr.properties` file to enable filtering. For more details, see the [Wiki](https://github.com/null511/MC-PBR-Pipeline/wiki/File-Loading).
```ini
# ~/assets/minecraft/textures/block/lantern.pbr.properties
smooth.scale = 1.2
metal.scale = 0.8
emissive.scale = 0.2
```

## Sample Repository

Coming Soon! I am _not_ a texture artist and need a good set of example content for a proper sample. If you own content you'd like to submit please reach out to me.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
