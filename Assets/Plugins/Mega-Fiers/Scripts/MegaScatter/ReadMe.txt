
MegaScatter
MegaScatter is a system that allows you to scatter multiple objects into regions defined by splines. You can ask the system to either scatter the selected objects inside splines, or scatter along splines. Each object can have a weight for how often it will appear as well as random factors for scaling, rotation etc. You can ask the system to either scatter the objects as new objects in the scene or combine the objects into single meshes for reduced draw calls and increased performance. There is also the option to have the objects be scattered at runtime this means you can greatly reduce the size of your game and reduce loading times.

Included with MegaScatter is a basic version of our spline system MegaShapes, this allows you to create and edit your rgion splines directly in Unity or make use of our exporters to import your created splines from 3DS Max or Maya. MegaShapes also allows you to turn splines into Meshes with various options such as filling the spline with a mesh or turning the spline into a tube mesh or even twisted tubes.

How to use:
From the game object menu goto the Create Other option then select the MegaScatter menu, from here you can choose which type of scatter object you would like to create, currently the options are:
Scatter Mesh - This will scatter selected objects inside the choosen splines combining the end result to a single mesh
Scatter Mesh Along - Same as Scatter Mesh but this time will scatter along a spline
Scatter Object - Same as Scatter Mesh but individual objects will be created in the scene instead of combined to a single mesh
Scatter Object Along - Same as above but objects will be scattered along the spline.

Once you have created your object you will need to pick the MegaShapes object in the scene that you wish to scatter your objects in or along. The shape can have multiple splines so for example if your games needed multiple areas of trees you can define a shape object with dozens of splines and the system will happily fill all those areas with your selected trees instead of having to create lots of Scatter Objects and select a spline for each.

When you have selected your shape you can start adding the GameObjects you want to scatter in that shape, it can be a single type of object or you can have as many different objects as you need, click the 'Add Mesh' button to add a new object to the scatter list. You will see the params for the object appear in the inspector 


Demo scene ideas
Small walled garden
Start with just dirt ground
areas of grass from filled spline shapes
borders
fill borders with flowers, rocks
path slaps scatter along splines
leaves under trees and on grass
wall could be done with spline to mesh
dandelions and daisys on grass

Other demo scene race track corner, marbles, gravel, weeds, plants, trees
war scene, bricks, rubble, weeds etc

v1.47
Fixed a bug where scatters could cause an exception is the number of splines in a shape is changed by another script.
Fixed the demo scene where some of the scatters would not work due to the above error.

v1.46
This updates means MegaScatter now uses the MegaShapes included with MegaFiers 2. This will stop errors when importing MegaScatter into a project which already includes MegaFiers 2 or MegaShapes.
This update is only needed if you need to use MegaScatter in a prject with MegaFiers 2 or MegaShapes in it.
This update is likely code breaking in a couple of places so please backup your project before using this update.
When installing MegaScatter in a project install MegaFiers 2 and or MegaShapes first as required, then install MegaScatter, and then dont import any asset that is marked as already in the project.

v1.45
Fixed a bug where some scatter types would not Update when the green update button in the inspector was clicked

v1.44
MegaScatter made fully Unity 2022 compatible

v1.43
Added a Spline Sub Div value to Scatter Mesh and Scatter Object, this allows more accurate scattering inside the curves of the spline, use 0 for simple straight lines between knots, higher values will do more accurate scattering to the spline shape
Fixed an issue where scattering over frames was not working for Scatter Mesh and Scatter Object layers.

v1.42
MegaScatter made fully Unity 2021 compatible

v1.41
MegaScatter made fully Unity 2019 compatible
MegaScatter made fully Unity 2020 compatible

v1.40
Fixed issue where demo scene on Unity caused an error, and updated code to stop it happennig again.

v1.39
MegaScatter made fully Unity 2018 compatible
MegaScatter made fully Unity 2018.2 compatible
Fixed an issue which stopped spline editing from working in Unity 2017.4

v1.38
Fixed an issue in Unity 2017.2 that stopped the spline knots from being moved.
MegaScatter made fully Unity 2017.2 compatible.
MegaScatter made fully Unity 2017.3 compatible.

v1.37
The Undo for MegaShapes now recognises adding and deleting of knots as well other the other options that change the spline such as Autocurve, scale etc.

v1.36
Fixed a bug where splines were being deleted if the 'Add' option was used when Loading a spl file.
MegaScatter made fully Unity 2017 compatible.

v1.35
Updated MegaUtils.cs so if MegaScatter and MegaFiers used together there wont be missing methods.

v1.34
Added option to the texture scatter layers to use the alpha channel in the mask texture to control scatter density.
Changes made for the latest Unity 5.6 obsolete methods for the final release of Unity 5.6

v1.33
Fixed a problem with the Position handles when editing splines.
Splines now default to Free Handle mode for editing.

v1.32
MegaScatter is fully 5.6 compatible.
Added new improved spline smoothing, you can now choose between old and new smoothing modes in the Shape Inspector. New method will produce a perfectly smooth spline through all the control points.
Added option to auto smooth the spline when you drag a knot, this will recalc the knot control points if you have changed any.
Rewritten the spline editing bit, making it easier to select and move spline knots and handles.
Move gizmos are now only shown for the currently selected knot.
To edit a spline knot you just need to click the green sphere for the point you want to edit, that will then select that point.
Option to freeze y movement when editing spline points, useful if using the free move handle type.
No need to click and drag mid point gizmos to add a new point you can now just click the circle to add a new knot.
Move handles now point along the spline.
Added extra circle gizmo at end of open spline for easier creation of splines by easily allowing addition of knot on the end.
Add Undo system for spline editing, you can now undo any changes made to a splines knots by clicking the Undo Edit Spline button.
Added option to set the max Undos that can be stored.
Fixed bug that caused splines to get corrupted if you tried to use the Spline animation builder on a spline with an imported animation.
Added the missing SetHandles method to the MegaShapes API.

v1.31
Fixed error when target platform is Windows App.

v1.30
MegaScatter now fully Unity 5.5 compatible.

v1.29
Added new option to 'build on start' for Scatter Mesh, Scatter Object, Scatter Mesh Texture and Scatter Object Texture so it can be spread over multiple frames, if the option is selected the scatter will be done by a CoRoutine and the amount scattered per frame is set by the 'Num Per Frame' option.

v1.28
Fixed error with Spline Fill mesh when being used as a collider with zero thickness.
MegaShapes fully compatible with Unity 5.3 and 5.4
Fixed a problem in Unity 5.x where zero length tris would cause an error and stop the collider being built.
Fixed exception in inspector if you happen to create a spline with no knots in it.
Added option to turn of the autosmooth when a new knot is added to the spline.
Also added the AutoCurve option when the Add Knot button is clicked

v1.27
MegaScatter made fully compatible with Unity 5.3

v1.26
Changes made to remove obsolete methods for Unity 5.1 and 5.2

v1.25
Fixed MegaShapeLightMapWindow problem when doing an app build.
You can now build lightmap data for MegaShape standard meshes.
Changed OSM importer to use ulong for ids instead of int so can now handle complex OSM files.
Fixed Shape labels being displayed if behind the camera.
Added a beta OSM data importer. Click the Assets/Import OSM option.
Added FindNearestPointXZ and FindNearestPointXZWorld methods to MegaShape API.

v1.24
Fixed bug in the Scatter Obj Texture layer which stopped Speedtree prefabs from being scattered.

v1.23
MegaScatter Fully Unity 5 compatible.
Fixed exception if trying to build a mesh with a shape with no splines.
Fixed exception if trying to interpolate along a shape with no splines.
Fixed exception if trying to interpolate a spline with no knots.
Autocurve fixed so the last knots handles on an open spline are correct.
Fixed some potential errors in the constant speed interpolation.
Smooth value is now a slider and shows results in realtime for easier use.

v1.22
The align object option is now copied when a layer is copied.
Added new option to the Uniform scaling option, you can now choose how the uniform scaling is applied and to which axis so XYZ, XY, XZ, YZ with the other axis having its own scale.

v1.21
Fixed offsets in Scatter Object Along not being applied correctly along the length of the spline.
Fixed offsets in Scatter Mesh Along not being applied correctly along the length of the spline.

v1.20
Further small changes to make compatible with Unity 5.0
Made changes for latest Unity 5 beta to fix any import warnings.
Fixed exception when adding a new curve to a shape.
The Clone Spline layer now works with the spline twist values.
Fixed exception when adding a new curve to a shape.
Added option to Spline Tube Mesh to flip normals for inside tubes.
Added option to Spline Box Mesh to flip normals for inside box tubes.
Added option to Spline Ribbon Mesh to flip normals.

v1.19
Add beta option to Scatter Object Along layer to have objects align with the spline direction. Extra rotations will then be added to that.
Add beta option to Scatter Mesh Along layer to have objects align with the spline direction. Extra rotations will then be added to that.
Imported SXL splines will now no longer change values to centre the spline.
Imported SVG splines will now no longer change values to centre the spline.
Added Centre Shape button to Shapes Inspector to allow you move the pivot to the centre of all the points.
Added new InterpCurve3D method which will return the postion, twist and also rotation quaternion for a point on a spline.
Autocurve now does the first and last handles on open splines.
Added 'Update on Drag' option to MegaShape inspector, if checked spline meshes will update as you drag, off then they will update when dragging stops.
Fixed inspector for spline animations so buttons aren't hidden.
Added SVG option to export MegaShape splines to SVG files.

v1.18
Added option to the scatter surfaces to enable the collider before the scatter.
Added option to the scatter surfaces for all layers to disable the collider after the scatter.

v1.17
Increased the raycast range from 1000 to 10000 so making it possible to scatter over a larger vertical range.
MegaScatter now imports into Unity 5.x with no autochanges required.

v1.16
Updated the MegaShapes version to the latest, which will stop any errors if used alongside the full version of MegaShapes or MegaFiers
Added KML import option to MegaShapes.

v1.15
Option added to recalc static batching after a scatter, which means in projects that use the scatter at runtime option batching will now work better and reduce draw calls even further.

v1.14
Added option to be able to copy and paste layer data for quicker setup. Click the Copy button to save the layer, then a Paste button will be available.

v1.13
Fixed warnings about Obsolete methods in Unity 4.5 and 4.6

v1.12
Resubmission due to last update changes not being applied to package.

v1.11
Fixed bug when object being scattered did not have a renderer, so things like LOD groups work correctly now.
Fixed bug where the remove objects wouldnt work with some object heirachies

v1.10
Fixed exception in texture scatter modes when using a texture collider and texture slot is empty.
If using a scale texture in texture scatter it now will use the uv coords from the texture collider correctly.
Added color variation texture option to texture scatters.

v1.09
Fixed a bug where constant speed interpolation was not being used for open splines.

v1.08
Added the ability to create animated splines inside Unity, you can now add keyframes for splines and have them played back for you.

v1.07
Fixed a bug where objects with disabled colliders would be disbaled after a scatter
Added option to include children of Ignore objects in the ingnore list
Improved the inspector for Ignore objects

v1.06
Warnings about GetChildCount fixed.
Slight change for Unity 4.x for enabling objects after scatter.

v1.05
New feature, support for proxy collision meshes in Mesh Scatters so can have much simpler mesh colliders
Added FailCount values to inspector so you have control over how long scatter will try to find places before it quits

v1.04
Fixed bug with colliders for scatter meshes being used by lower layers
Option to have no colliders added to scatter mesh objects
Option to disable colliders during scatter for Object scatters, stops things piling up
Added missing color variations option to Scatter Object Texture

v1.02
Improved the surface offset method so works correctly in all cases
Added option to generate mesh colliders
Added option to generate mesh tangents
Fixed bug that could caused aligned surface objects to be incorrectly aligned
Added support for generating lighmapping uvs for mesh based scatters.
Fixed example scene scatter along object not having correct values.

V1.0
First release

TODO: save positions etc for fast build at start
TODO: spline contain method could be improved to make more accurate
TODO: random for each axis of non uniform scale, same for rot and offset
TODO: Move all features to various scatter classes
TODO: see if code can be shared a bit better
TODO: Option to add in objects with mouse, 
TODO: texture scale should have option for what to scale, xyz check boxes
TODO: display mesh inf things like radius for overlap, heights and slope info
TODO: move or override any non meshinf values
TODO: Brook scene with rocks, grass flowers, trees
TODO: Conform option for mesh scatters
TODO: show realtime positon and radius of objects, so basic position and show
TODO: density mode show area and how many will be created
TODO: Hide objects button
TODO: multiple shapes
TODO: threaded build so can build over time
TODO: color from texture (only object or if not using color for waving)
TODO: every vertex
TODO: every face centre
TODO: every midpoint
TODO: option in existing scatters to look at a texture object
TODO: test on 4.2 and 4.3
TODO: submit
TODO: Lod system
TODO: Build proper objects in max with painted v cols for shader
TODO: Change way objects are added to the end result, ie random to pick object then add that, as opposed
to filling up wth first object then no room for others
TODO: Split build over multiple frames, do as coroutine and have a thread option
TODO: Should be able to cope with submeshes uv1/2, color etc, and what if we are mesh based but obj has multiple children
TODO: Method to add objects for building in script
TODO: Fill mesh with objects, fill outside of mesh, ie leaves on tree, fill either bound box, sphere or mesh
TODO: System where we add a mesh to the system, ie a leaf falls and when it hits the ground its added
TODO: When I do the add a mesh system do it so it can grow in, or fade even. So either keep replacing at end with scaled version or use a child object and expand it in, mmm actually that could be dealt with outside
TODO: Do a version that uses a collider the provide area
TODO: Overlap of any mesh or internally, or externally etc, different options
DONE: display gizmo option
TODO: Have a version where we keep track of each item added, option to add colldier or trigger for each then can remove or replace item

DONE: Web pages
DONE: demo scene
DONE: Scale with distance from spline up to a limit
DONE: have height values per mesh so will automatically use other meshes based on height
DONE: Get size of mesh adding, and keep track on objs pos to seperate items
DONE: Adjust area value as large trees dont get included at the moment
DONE: offset for raycast adjust, so can sink objs or raise them
DONE: slope option for object
DONE: min scale value below which mesh not added
DONE: Texture based should have option for an object to be used
DONE: Scaling texture, gray scale
DONE: multiple colors for texture ones, instead of range
DONE: All rays should be the same collider
DONE: Random color range for object
DONE: show how many objects placed
DONE: show vertex count
DONE: scale from texture
DONE: vertex chaos
DONE: Color range for texture scatters
DONE: 4 raycasts to make sure all mesh is on object
DONE: check box on ignore objects to disable
DONE: take scaling to account for nooverlap
DONE: pick rgb channel for layer
DONE: select splines to use
DONE: option for density or actual count
DONE: select objects to not scatter on
DONE: vertex count limit beofre making new object (instead of 65535)
DONE: Scatter to a texture on an object, rgb value defines object to position, or red value defines object, green scale, blue density?
DONE: change mesh builder to handle going over 65535 verts
DONE: scale based on distance from line, so for filling inverse scale for along further = smaller
DONE: gap value to keep things set distance apart
DONE: Raycast collision for height
DONE: Add in col changer if vertex colours involved, and uv changer
DONE: color option should be a meshinf param
DONE: Force count of objects to appear
DONE: move common values to MegaScatter
DONE: split classes out to own files
DONE: Curve for coloring
DONE: global scale for scatter object that effects all layers
DONE: currentedit move to main class
DONE: check on counts so doesnt lock up with low scale values etc
DONE: maxcount value to avoid long delays
DONE: Mark objects as static
DONE: get shared params into base class
DONE: Build on start option
DONE: show enabled in layer list
DONE: change default dist crv to linear
DONE: Add name to layer
DONE: Change inspector to select layer and then show values for that
DONE: Object based one instead of building meshes
DONE: Along a spline system, pretty much Loft I guess
DONE: No overlap option
DONE: Snap angles, so could say 360 but snap to 90 for buildings
DONE: Rename to MegaScatterMesh
DONE: uniform scaling
DONE: Bounds on mesh and do simple circle check to make sure we dont overlap
DONE: Per mesh say if it should raycast etc, also if should align to hit normal, and have layer
DONE: Option to only place if collision happens
DONE: Dont need box in meshinf anymore
DONE: Overlap should be for whole system, can always add another scatter to an object (need to be careful of scatter name for destroy)
DONE: Do object scatter along a line
DONE: Move raycast params to meshinf
DONE: color amount value
DONE: power for vertical color
DONE: Shader to animate plants
DONE: Option to color verts on height for shader interaction
DONE: Enable value for each mesh
DONE: Add a remove objects button to clear generated objects
DONE: Scatter along spline with distance from value
DONE: Transform normals so dont need recalc
DONE: remove mesh from meshinf use obj only
DONE: Start end curve numbers so can do parts of shape
DONE: Could have snapping on x y
DONE: Density should work as how many per metre square, or should take size of bounds and a density of 1 is 1 unity per size etc
DONE: Snap should be per mesh, also add a vert offset
DONE: Seed should be in meshinf, so can adjust mesh level without messing up other levels, ie stretch grass
DONE: Count for whole area, so need area of closed spline
DONE: Detect if spline inside another ie O only do edge
DONE: Need a seed
DONE: Object based system instead of build mesh
DONE: Version where it builds objects, should provide GameObjects to system

IDEA: compound areas, say area under trees, keep adding to over time for leaf build up
IDEA: overlay or decal in skid marks based on a track shape and direction curve, with a density curve - would allow build up over time
IDEA: if we use this for marbles, have it so we can add to the end so can build up over time