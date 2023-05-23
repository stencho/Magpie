# Magpie

<p>this is a several-years-long attempt to write a 3D game engine by uhh me</p>
<p>it's called magpie because I like magpies</p>

## random stuff it currently supports in one way or another
- 2D GJK collision detection
- 3D GJK swept collision detection with EPA
- Octree-based world space and mesh collision test partitioning
- Deferred dynamic lighting, currently supporting spot lights with shadows and point lights without
- An ambient lighting system that takes arbitrary gradients to determine how the lighting will change over time (basically a day/night cycle with arbitrary colours)
- Update loop in separate thread from drawing, with a fixed tick rate (and, in the future, interpolation in the rendering thread)
- Automatic rendering of object_info objects added to world map
- Easily useable global handling of loaded content through ContentHandler 
- Similar system for controls and control bindings
- Persistent game variable system, (currently) saving settings to a file simply called "gvars" in the same folder as the executable, these are easily set through, for example, gvars.set("vsync", true);
- 3D heightmaps with octree-based chunks built in
- Point clouds and particles
- 2D SDFs
- An entire window manager, as well as a WinForms-esque UI Forms system, this is not a bit
- the console is a fully featured C# REPL (it has full keyboard support, arrows/del/home/end, all those with ctrl, clipboard, etc)
- A massive library of 2D/3D drawing and math utility code outside of all of this
- there's so many projects in here, each written in a single manic sitting

##### The rest of the engine has a lot of goals to be met, including but not limited to:
- A walk system
- Reimplementation and improvement of the collision resolution system that currently lies dormant (I'm in the middle of upgrading my world octree code as I write this)
- Animations (!!)
- Inverse kinematics
- Room-based space partitioning and indoor/outdoor switching
- A lot of stuff to be actually finished lmao

##### The renderer is pretty close to done
- Cube-mesh-based deferred directional lighting
- CSMs based on the above
- An automatic render-to-texture setup implemented in the Camera class (this is one I had actually working in a much older version of this renderer)
- Planar reflections based on the above

<video src="https://cdn.discordapp.com/attachments/521011204685037598/1023693205243429095/wearetheuniverse.mp4" controls="controls" muted="muted" height=400></video>
<video src="https://cdn.discordapp.com/attachments/521011204685037598/1035661537089687572/finally_lighting2.mp4" controls="controls" muted="muted" height=400></video>