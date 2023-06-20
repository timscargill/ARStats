# ARStats: AR Session Measurement Application

ARStats is an AR app (Android or iOS) that records device tracking data and IMU sensor readings, environment characteristics, and user experience (e.g., virtual object stability) metrics. A full description of the app is available in [this technical report](https://arxiv.org/abs/2109.14757). ARStats was used to conduct quantitative measurements of virtual object drift on various AR platforms and devices in [Here To Stay: A Quantitative Comparison of Virtual Object Stability in Markerless Mobile AR](https://maria.gorlatova.com/wp-content/uploads/2022/03/HereToStay_CR.pdf), published in the IEEE/ACM Workshop on Cyber-Physical-Human System Design and Implementation, May 2022 (co-located with CPS-IoT Week 2022).

# ARStats Overview

ARStats measures virtual object (hologram) drift through the use of a real world reference point and virtual objects placed by the user. To perform a drift measurement, the user performs the following steps, illustrated in the below images: 

1) Place a real world reference point (e.g., a sticker approximately 1cm in diameter) and move the AR device to detect a virtual plane.
2) Use the virtual placement guidance to place the original virtual object (a red sphere) on the real reference point.
3) Perform a movement (e.g., walk away and return), and place the reference virtual object (a blue sphere) on the real reference point.

![ARStats screenshots](https://github.com/timscargill/ARStats/blob/main/ARStats.png?raw=true)

Throughout

Please see [Use Instructions](#use-instructions) for more detailed information on how to operate the ARStats app.

# Implementation Instructions

ARStats comes in the form of a Unity project folder, which can be built to Android or iOS devices; it is created using the Unity AR Foundation framework which supports both ARKit and ARCore. Some Unity AR Foundation sample prefabs and scripts are retained for functionality and experimenting with additional features.

Created using Unity 2020.1.16f1. Tested with the following configurations:
- Unity 2020.1.16f1, iOS 14.4, iPhone 11
- Unity 2020.1.16f1, iOS 15.1, iPhone 11
- Unity 2020.1.16f1, iOS 15.0, iPhone 13 Pro Max
- Unity 2020.2.2f1, Android 11, Samsung Galaxy Note 10+

1) Download .zip file and unzip in a convenient location (the Unity project folder is the folder 'ARPerformanceTestingTool-main'). 
2) In Unity Hub, under projects, click 'Add' and select the Unity project folder.
3) Open the Unity project, then open the 'PerformanceTesting' scene (Scenes>PerformanceTesting).
4) Navigate to File>Build Settings, select your Build platform, and click 'Switch Platform'.
5) Make sure only the 'PerformanceTesting' scene is selected in 'Scenes in Build'.
6) For Android, select your connected target device and click 'Build and Run'. For iOS, click 'Build' and follow the instructions for XCode below.

If building to iOS devices, an extra signing step in XCode is required:

7) Building the Unity project for iOS creates an XCode project folder, which you can choose the name and location of.
8) Open the project folder you just created in XCode (XCode 12 required to build for iOS 14).
9) Click 'Signing and Capabilities'. Check 'Automatically manage signing', select your development team, and choose a new bundle identifier.
10) Make sure your target iOS device is connected and selected at the top of the window, then click the play button to build and run.

# Use Instructions

'Map' Phase:
1) Move around to detect a plane (transparent grey object). 
2) The green circle shows you where the virtual object will be placed, located at the point a vector from the center of the screen first intersects a plane.

*During the mapping phase environment data (light and camera image) are sampled after 50 frames, and every 100 frames thereafter.*

'Place' Phase:

3) Press 'Place' to place the red sphere (target) virtual object at marked position in the real world. Press repeatedly to adjust its position if necessary. 
4) Press 'Measure' to confirm and capture the original position of the virtual object.
5) Perform some type of movement that may affect the accuracy of tracking (e.g. moving to a different viewing angle, placing the phone down).
6) Press 'Place' (repeatedly if necessary) to place the blue sphere (reference) virtual object, at the point in the real world where the red virtual object was originally placed.
7) Press 'Measure' to capture the position of the blue virtual object and write out all data.
8) Close the app. The above steps can be repeated to capture a new scene once the app has been closed and reopened.

# Output Files

The following output files are generated during the app use described above. Each scene is recorded in a sub-folder named with the timestamp (yyyy-MM-dd-HH-mm-ss) of when the app was opened.

**acc_map.txt**: accelerometer readings (one each frame) during 'map' stage.

**acc_place.txt**: accelerometer readings (one each frame) during 'place' stage.

**device_map.txt**: estimated device position in world space (one each frame) during 'map' stage.

**device_place.txt**: estimated device position in world space (one each frame) during 'place' stage.

**firstPlane_time.txt**: time until first plane was detected (hh:mm:ss:ms).

**fps_map.txt**: frames per second during 'map' stage.

**fps_place.txt**: frames per second during 'place' stage.

**gyro_map.txt**: gyroscope readings (one each frame) during 'map' stage.

**gyro_place.txt**: gyroscope readings (one each frame) during 'place' stage.

**initialization_time.txt**: time until device pose tracking first initializes after app open (hh:mm:ss:ms).

**light.txt**: brightness, color temperature for each environment data sample taken during the map stage (1 per line). Color temperature is currently only available for iOS.

**mapping_time.txt**: time spent by the user mapping the scene (hh:mm:ss:ms).

**placementPlane.txt**: properties of plane where virtual object was placed (1-orientation, 2-x and y extents, 3-distance from virtual object placement position to center of plane).

**planes.txt**: number of planes detected so far at environment data sample taken during the map stage (1 per line).

**pointcloud.txt**: pointcloud captured during the 'map' stage (1 3D point per line; x, y and z components).

**position0.png**: screenshot on first 'Measure' click (original target virtual object position).

**position0.txt**: details of the target virtual object placement (1-position in world space, 2-distance from camera to virtual object, 3-change in viewing angle around the y-axis from position0 (0), 4-magnitude of virtual object drift from position 0 (0).

**position1.png**: screenshot on second 'Measure' click (reference virtual object position).

**position1.txt**: details of the reference virtual object placement (1-position in world space, 2-distance from camera to virtual object, 3-change in viewing angle around the y-axis from position0, 4-magnitude of virtual object drift from position 0.

**relocalization_time.txt**: time for device pose tracking to be reestablished after an interruption or temporary loss of tracking (hh:mm:ss:ms).

**rgb#.png**: camera image at each environment data sample taken during 'map' stage.

**temp_map.txt**: device temperature readings (one each frame) during 'map' stage. Currently only available and generated for iOS.

**temp_place.txt**: device temperature readings (one each frame) during 'place' stage. Currently only available and generated for iOS.

**tracking_map.txt**: device pose tracking status (one each frame) during 'map' stage.

**tracking_place.txt**: device pose tracking status (one each frame) during 'place' stage.
