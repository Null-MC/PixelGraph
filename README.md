# MC-PBR-Pipeline

MC-PBRP is a command-line application for publishing Minecraft resource packs, with special tooling for PBR materials. Automate the workload of modifying and distributing your resource packs through simple JSON configuration files.

 - **Simplify** your workflow by adjusting text instead of pixels. Getting image-based material values just right can be tedious, time consuming, and destructive.

 - **Preserve Quality** by adjusting material values through text rather than altering the original image data. Repeatedly scaling the integer-based channels of your image slowly destroys quality. Save the gradients!

 - **Support** more users by publishing multiple packs with varying quality. The resolution and included textures can be altered using either the command-line or Publishing Profiles to create multiple distributions.

Written in .NET Core; supports Windows, Linux, Mac.

## Installation

**NOTE:** _MC-PBRP is still a pre-release application in early development! The initial release will be coming soon though, so please check back soon!_

*Manual*
Download the latest [release](https://github.com/null511/MC-PBR-Pipeline/releases). For best results, add to your system PATH variable.

*Automated*
- Chocolatey: TODO
- Docker: example coming soon.

## Usage

Publishing Profiles are JSON documents in the root of the source project that describe how the resource pack should be published. Each profile allows several options to be configured for the generated resource pack; this includes description, tags, resolution, channel-mappings, and more. The following pack ensures all textures are 128x* (preserving aspect) and remaps the material channels of the specular map from the source `{rough, metal, emissive}` to the `{metal, smooth, emissive}` encoding expected by the SEUS renewed shader.

```
// ~/pack.SEUS-PBR-x128.json
{
	// resize textures down to 128x*
	"texture-size": 128,

	// remap specular materials
	"specular-in": {
		"red": "metal",
		"green": "rough",
		"blue": "emissive"
	},
	"specular-out": {
		"red": "smooth",
		"green": "metal",
		"blue": "emissive"
	}
}
```

The additional publishing profile below is used to publish a non-PBR version of the resource pack, that is downsized to 32x\*.
```
// ~/pack.vanilla-x32.json
{
	// resize textures down to 32x*
	"texture-size": 32,

	// exclude pbr materials
	"include-normals": false,
	"include-specular": false,
}
```

Each item-texture requires a matching `*.pbr` file to enable filtering. For more details, see the [Wiki](/wiki/File-Loading).
```
// ~/assets/minecraft/textures/block/lantern.pbr
{
 	"specular": {
 		"metal": 0.8,
 		"smooth": 1.6,
 		"emissive": 0,
 	}
 }
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
