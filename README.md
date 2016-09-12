# ForceDirectedNodeGraph3DUnity
A 3D Force Directed Node Graph Implementation in Unity3D

This is a hobby project to attempt to create a Force Directed Node Graph in 3D which renders and updates in realtime. Its primary intent and goal is to visualize computer networks. It uses the Gameengine Unity3d and available physics engines to draw the Graph.

[![Vimeo 3D Force Directed Node Graph with BulletUnity](https://i.vimeocdn.com/video/590726525_590x332.jpg)](https://vimeo.com/181982717 "3D Force Directed Node Graph with BulletUnity")

https://vimeo.com/181982717

Unity3D was chosen because of the available physics engines offering all needed force functionality and because of its extensibility, moving towards more detailed graphics in later releases. Currently the graph can render around 100 to 300 linked nodes  in realtime, depending on the physic engine and the hardware being run on. As this node limit is not enough for most networks, this needs to be improved in the future.

I started out with this to learn OOP, C# and Unity3D and it was a great learning experience so far. The initial concept is based upon the great Unity3D Network Visualization Tutorial from Jason Graves, which is/was a great resource to get up to speed quickly: http://collaboradev.com/2014/03/12/visualizing-3d-network-topologies-using-unity/. Jason's node/link concept was used as building blocks for the graph. Also the example input file "layout.xml" is his work, as well as CameraControlZeroG.cs. Thanks, Jason.

The current version v0.02 uses the BulletUnity Plugin for Unity3D, authored by Ian Deane and Andres Tracks. It makes the open source physics engine Bulletphysics available in Unity3D, which offers great flexibility. Thanks for the release and your help, Ian. BulletUnity can be found here:
- http://forum.unity3d.com/threads/released-bullet-physics-for-unity.408154/
- http://www.digitalopus.ca/site/bullet-physics-in-unity-3d/
- https://www.assetstore.unity3d.com/#!/content/62991
- https://github.com/Phong13/BulletSharpUnity3d

<br>
Usage:
- w, a, s, d: Move
- Ctrl, Space: Move up/down
- Mousewheel: Increase/decrease move speed. Current move speed printed in status text bottom left.

<br>
Platforms:
- Only tested on Win 10
- Should work on other platforms

<br>
Beware of the code, still learning. The current version is in the state of a working draft.

<br>
Licensing:
- Please be aware that the different parts of the code are using different licenses.
- If not mentioned otherwise, the code is released is GPLv3.
- Included is a slighty modified version of BulletUnity Plugin (Examples removed, BCamera included), refer to mentioned BulletUnity Github repository for its license.
- Layout.xml and CameraControlZeroG.cs from Jason Graves are GPLv3, as mentioned in his tutorial (see link above).
- ProgressBar Unity3D Plugin from Eri is used as progress bar (on loading files). More info and licensing are available here:
      https://www.assetstore.unity3d.com/en/#!/content/30891
      https://eri-st.eu/

<br>
Changelog:
- v0.01; 14.08.2016; PhysX only. Unreleased.
- v0.02; 08.09.2016; BulletUnity only.
- v0.03; 11.09.2016;
	- Readded PhysX engine on same codebase. Now BulletUnity or Physics engine can be chosen. Change with bool in GameController
	- Code Rework, splitted into separate classes per topic
- v0.04; 13.09.2016;
	- Node did get base class
	- Moved global Bullet ApplyGravity() back locally in NodeBullet
	- Graph default values for Bullet. Still needs some tweaking

<br>
Next versions:
- Remove Bugs
- Make it more OO
- Display used Engine in Editor
- Have better default settings for both engines
- Make main settings available in runtime GUI
- Sphere and / or flat graph projection
- Save/load of transforms
- Inputreader: Make Nodelimit an allstatic choosable on fileload

- Make it buildable

<br>
Future plans:
- Implement custom force graph algo without physics engine.
- Improve rendering speed.
- Make prettier. 
- More Input readers.
- more. much more. after christmas.
