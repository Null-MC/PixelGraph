# MC-PBR-Pipeline [![Actions Status](https://github.com/null511/MC-PBR-Pipeline/workflows/Release/badge.svg)](https://github.com/null511/MC-PBR-Pipeline/actions)

MC-PBRP is a command-line application for publishing Minecraft resource packs, with special tooling for post-processing PBR materials. Automate the workload of modifying and distributing your resource packs through simple properties files. Written in .NET Core; supports Windows, Linux, Mac. Docker ready.

 - **Simplify your workflow** by adjusting text instead of pixels. Getting image-based material values just right can be tedious, time consuming, and destructive.

 - **Preserve Quality** by adjusting material values through text rather than altering the original image data. Repeatedly scaling the integer-based channels of your image slowly destroys quality. Save the gradients!

 - **Support more users** by publishing multiple packs with varying quality. The resolution and included textures can be altered using either the command-line or Publishing Profiles to create multiple distributions.

 - **Automate** normal-map creation, resizing, and channel-swapping so that you can spend more time designing and less time repeating yourself.
 
### Normal-Map Generation

<img src="https://github.com/null511/MC-PBR-Pipeline/raw/master/media/NormalGeneration.png" alt="Normal-Map from Height-Map" height="140px"/>

Allows normal-map textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Strength, blur, and wrapping can be managed using the textures matching pbr-properties file.
 
### Occlusion-Map Generation

<img src="https://github.com/null511/MC-PBR-Pipeline/raw/master/media/OcclusionGeneration.png" alt="Occlusion-Map from Height-Map" height="140px"/>

Allows ambient-occlusion textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Quality, Z-scale, step-count, and wrapping can be managed using the textures matching pbr-properties file.

## Installation

See the [wiki](https://github.com/null511/MC-PBR-Pipeline/wiki/Installation) for more information.

## Usage

Pack property files are text documents in the root of the source project that describe how the resource pack should be published. Each profile allows several options to be configured for the generated resource pack; this includes description, tags, resolution, channel-mappings, and more. The following pack ensures all textures are 128x* (preserving aspect) and remaps the material channels of the specular map from the source `{smooth2, metal, emissive}` to the `{smooth, metal, porosity, emissive}` encoding expected by the SEUS renewed shader.

```ini
# ~/pack.SEUS-PBR-x128.properties
texture.size = 128

# remap specular materials
porosity.value = 64
specular.input.r = metal
specular.input.g = rough
specular.input.b = emissive
output.specular.r = smooth2
output.specular.g = metal
output.specular.b = porosity-sss
output.specular.a = emissive
```

The additional publishing profile below is used to publish a non-PBR version of the resource pack, that is downsized to 32x\*.
```ini
# ~/pack.vanilla-x32.properties

# resize textures down to 32x*
texture.size = 32

# exclude pbr materials
output.albedo = true
output.normal = false
output.specular = false
```

Each item-texture requires a matching `*.pbr.properties` file to enable filtering. For more details, see the [Wiki](https://github.com/null511/MC-PBR-Pipeline/wiki/File-Loading).
```ini
# ~/assets/minecraft/textures/block/lantern.pbr.properties
smooth.scale = 1.2
metal.scale = 0.8
emissive.scale = 0.2
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
