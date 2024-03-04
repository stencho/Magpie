# Magpie

this is a several-years-long single-idiot attempt to write a 3D game engine. it's going alright<br>
it's called magpie because I like magpies and naming projects after birds<br>

here are some images of the nonsense it's capable of<br>
I have no way to explain the use of tump programmer art. tumpl<br>

![](img/scr133235758403855399.png)![](img/scr133114626191656635.png)
![](img/scr133044561005967787.jpg)![](img/scr132966595510168487.jpg)
![](img/scr133091138553824388.png)![](img/scr133215412581394526.png)
![](img/scr133086050450791444.png)![](img/scr133085982162092423.png)

<p>tirmp</p>


### random stuff it currently supports in one way or another
- 2D GJK collision detection
- 3D GJK swept collision detection (broken) with EPA (working)
- Octree-based world space and mesh collision test partitioning
- Deferred dynamic lighting, currently supporting spot lights with shadows and point lights without
- An ambient lighting system that takes a colour gradient to determine how the lighting will change over time (basically a day/night cycle with arbitrary lighting colours and brightnesses)
- Update loop in separate thread from drawing, with a fixed tick rate (and, in the future, interpolation in the rendering thread)
- Easily useable global handling of shared content through ContentHandler ('ContentHandler.resources["OnePXWhite"].value_tx' will get the texture "OnePXWhite", which is a 1x1 white square created by the ContentHandler on load)
- Similar system for controls and control bindings, also threaded (to be supplemented with a RawInput-based setup (and rewritten to utilize it to the fullest) in the near future)
- Persistent game variable system, (currently) saving settings to a file simply called "gvars" in the same folder as the executable, these are easily set through, for example, gvars.set("vsync", true);
- Automatic, clean handling of things like changing graphics modes through the gvar system (running 'gvars.set("resolution", new XYPair(720,1280))' will immediately switch to 720x1280, a portrait resolution, without issue) 
- Internal resolution scaling, also handled through gvars
- 3D heightmaps with octree-based chunks built in
- Point clouds/particles
- 2D SDFs with support for things like scrolling textured, tinted outlines
- An entire window manager, as well as a WinForms-esque UI Forms system. *this is not a bit*
- the console is a very usable cs-script C# REPL (with full keyboard and mouse support, arrows/del/home/end, clipboard, undo/redo, etc, as well as a console colour system )
- A massive library of 2D/3D drawing and math utility code outside of all of this
- there's so many projects in here, many written in a single manic sitting

this is mostly a dead project, however:

##### The engine has a lot of goals yet to be met, including:
- A walk system
- Reimplementation and improvement of the collision solver which currently lies dormant (I'm "in the middle" of upgrading the world octree code)
- Animations (!!)
- Inverse kinematics
- Room-based space partitioning and indoor/outdoor switching
- A lot of stuff to be actually finished lmao

##### The renderer is pretty close to done
- Cube-mesh-based deferred directional lighting
- CSMs based on the above
- Partial transparencies, with dynamic lighting support. probably approached via rendering everything but transparencies, followed by transparent objects front to back, using forward rendering instead of deferred for their lighting
- An automatic render-to-texture setup implemented in the Camera class (this is one I had actually working in a much older version of this renderer)
- Planar reflections based on the above

