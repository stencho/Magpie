#wtf is this
Magpie is an in-progress game engine, intended for very specific use in slower-paced first/third person shooters and "walking simulators"

Instead of the usual method of building maps by adding terrain and prefab objects with collision on them then just using the physics engine to push actors around, Magpie is designed to use large planes and heightfields for floors and not take object collisions into consideration for movement at all. All movement blockers will be in the form of invisible walls and shapes. Movement on these planes and heightfields will be effectively 2D, and very solid. 

I am aiming for a movement-feel similar to Resident Evil 4/5- no inherent ability to jump, and somewhat arbitrary world collisions, shapes drawn on the ground around objects which actors shall not pass and stuff like that.

Rendering should be relatively simple, no PBRs or anything, possibly not even speculars or bump maps, but with a heavy focus on dynamic lighting. CSMs might be used, but for the most part, it'll be regular old shadow maps.