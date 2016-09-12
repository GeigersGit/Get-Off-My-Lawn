VERSION 4.6 - changes against versions 3.5/4.0/4.5:

since 4.5:
- Added placeholder player character sprite to replace drawing a dynamic square.
- Changed limit of max physics objects to 5k (managed to hit max at 4k).
- Fixed undesired effect of pixels sticking to edges of terrain (like top empty area).

since 4.0/3.5:
- Added temporary fix for edge crash, by making borders solid.
- Added a temporary follow camera test script. Camera will now follow (although jittery) the players position.
- New level from Cortex Command, "Rich Lands" (a portion of it anyway), at 2048 x 1024 in size.
- Changed limit of max physics objects to 4k.
- Changed from checking texture2d's width and height to cached version. 
- Fixed double removal of ID's problem.
- Changed player drawing to use isPixelSolid() method instead of performing the a check within its own method.

Original Credits for the inspiration and original Java/Processing source project belongs to "Jared Counts" at:
http://gamedevelopment.tutsplus.com/tutorials/coding-destructible-pixel-terrain--gamedev-45

HOW TO SETUP:
- You need to setup the "GAME" window to be of matching dimensions in pixels in size to the demo scenes background!
- The "RenderQuad" should remain the dimensions of the demo terrain to keep things matching!

THINGS TO NOTE:
- Changing destruction resolution below 2 can and will cause errors, for now use 2 or higher.
- Shooting may possibly still crash the editor/player, but has not done this in testing!
- New bugs may be hidden in there, thanks to recent changes not being well tested yet.
- If you can fix something, or create something cool or interesting out of it, BRING IT BACK TO the original thread 
and SHARE! :)

THE ORIGINAL FORUM POST: http://forum.unity3d.com/threads/198919-Destructible-Pixel-Terrain-Open-Source

If you use this in a project commercial or otherwise please include credit to both the original author Jared Counts 
for creating it to begin with, and MD_Reptile with the help of mgear and others of the Unity Community for converting it 
to a Unity Project!