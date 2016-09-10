# ForceDirectedNodeGraph3DUnity
A 3D Force Directed Node Graph Implementation in Unity3D

This is a hobby project to attempt to create a Force Directed Node Graph in 3D which renders and updates in realtime. Its primary intent and goal is to visualize computer networks. It uses the Gameengine Unity3d and available physics engines to draw the Graph.
https://vimeo.com/181982717

Unity3D was chosen because of the available physics engines offering all needed force functionality and because of its extensibility, moving towards more detailed graphics in later releases. Currently the graph can render around 100 to 300 nodes linked in realtime, depending on the physic engine and the hardware being run on.

I started out with this to learn OOP, C# and Unity3D and it was a great learning experience so far. The initial concept is based upon the great Unity3D Network Visualization Tutorial from Jason Graves, which was a great resource to get up to speed quickly: http://collaboradev.com/2014/03/12/visualizing-3d-network-topologies-using-unity/. Jason's node/link concept was used as building blocks for the graph. Also the example input file "layout.xml" is his work. Thanks, Jason.

The current version v0.02 uses the BulletUnity Plugin for Unity3D, authored by Ian Deane and Andres Tracks. It makes the open source physics engine Bulletphysics available in Unity3D, which offers great flexibility. Thanks for the release and your help, Ian. BulletUnity can be found here:
http://forum.unity3d.com/threads/released-bullet-physics-for-unity.408154/
http://www.digitalopus.ca/site/bullet-physics-in-unity-3d/
https://www.assetstore.unity3d.com/#!/content/62991
https://github.com/Phong13/BulletSharpUnity3d

Beware of my code, it is more than a little rough around the edges. The current version is in the state of a working draft.

Licensing:
- If not mentioned otherwise, the code is this release is GPLv3
- Included is a slighty modified version of BulletUnity Plugin (Examples removed, BCamera included), refer to mentioned BulletUnity Github repository for its license
- Layout.xml from Jason Graves is GPLv3, as mentioned in his tutorial (see link above)
- ProgressBar Unity3D Plugin from Eri is used as progress bar (loading files). More info and licensing are available here:
      https://www.assetstore.unity3d.com/en/#!/content/30891
      https://eri-st.eu/

Changelog:
v0.01; 14.08.2016; PhysX only. Unreleased.
v0.02; 08.09.2016; BulletUnity only. (this release)
v0.03; xx.09.2016; Code Rework, BulletUnity and PhysX in the same codebase. In the works.

Plans:
- Code rework, quality improvements
- Include PhysX on same codebase
- Improve rendering speed.
- Lots more.
