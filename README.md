# MC-PBR-Pipeline

MC-PBRP is a command-line application written in .NET for publishing Minecraft resource packs. It aims to simplify the workload of creating and distributing these packs by partially automating the creation of PBR textures, and allowing multiple values to be tuned using JSON-based mappings.

 - **Simplify** your workflow by adjusting text instead of pixels. Getting your material values just right can be tedious, and opening your image editor is often time consuming.

 - **Preserve Quality** by Adjusting material values through text (JSON) rather than altering the original image data. Repeatedly scaling the integer-based channels of your image slowly destroys quality. Save the gradients!

 - **Support** more users by publishing multiple packs with varying quality. The resolution and included textures can be altered using either the command-line or Publishing Profiles to create multiple distributions.

```json
// ~/pack.vanilla-x32.json
{
	// resize textures down to 32x*
	"texture-size": 32,

	// exclude pbr materials
	"include-normals": false,
	"include-specular": false,
}

// ~/pack.SEUS-PBR-x128.json
{
	// resize textures down to 128x*
	"texture-size": 128,

	// remap specular materials
	"specular-in": {
		"red": "rough",
		"green": "metal",
		"blue": "emissive"
	},
	"specular-out": {
		"red": "metal",
		"green": "smooth",
		"blue": "emissive"
	}
}
```

```json
// ~/assets/minecraft/textures/block/lantern.pbr
{
 	"specular": {
 		"metal": 0.8,
 		"smooth": 1.6,
 		"emissive": 0,
 	}
 }
```

## Installation

[TODO]

## Usage

[TODO]

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[GNU GPLv3](https://choosealicense.com/licenses/gpl-3.0/)
