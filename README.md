# AR Performance Testing Tool

Unity project folder for an AR application (iOS or Android) to record device tracking data and IMU sensor readings, environment characteristics, and user experience metrics.

Built using the Unity AR Foundation framework supporting both ARKit and ARCore. Some Unity AR Foundation sample prefabs and scripts retained for functionality and experimenting with additional features.

Created using Unity 2020.1.16f1. Tested with the following configurations:
- Unity 2020.1.16f1, iOS 14.4, iPhone 11
- Unity 2020.2.2f1, Android 11, Samsung Galaxy Note 10+ 

# Build Instructions (Unity for iOS or Android)

1) Download .zip file and unzip in a convenient location (the Unity project folder is the folder 'ARPerformanceTestingTool-main'). 
2) In Unity Hub, under projects, click 'Add' and select the Unity project folder.
3) Open the Unity project, then open the 'PerformanceTesting' scene (Scenes>PerformanceTesting).
4) Navigate to File>Build Settings, select your Build platform, and click 'Switch Platform'.
5) Make sure only the 'PerformanceTesting' scene is selected in 'Scenes in Build'.
6) For Android, select your connected target device and click 'Build and Run'. For iOS, click 'Build' and follow the instructions for XCode below.

# Build Instructions (XCode for iOS)

1) Building the Unity project for iOS creates an XCode project folder, which you can choose the name and location of.
2) Open the project folder you just created in XCode (XCode 12 required to build for iOS 14).
3) Click 'Signing and Capabilities'. Check 'Automatically manage signing', select your development team, and choose a new bundle identifier.
4) Make sure your target iOS device is connected and selected at the top of the window, then click the play button to build and run.

# Use Instructions

'Map' Phase:
1) Move around to detect a plane (transparent grey object). 
2) The green circle shows you where the hologram will be placed, located at the point a vector from the center of the screen first intersects a plane.

*During the mapping phase environment data (light and camera image) are sampled after 50 frames, and every 100 frames thereafter.*

'Place' Phase:

3) Press 'Place' to place the red sphere (target) hologram at marked position in the real world. Press repeatedly to adjust its position if necessary. 
4) Press 'Measure' to confirm and capture the original position of the hologram.
5) Perform some type of movement that may affect the accuracy of tracking (e.g. moving to a different viewing angle, placing the phone down).
6) Press 'Place' (repeatedly if necessary) to place the blue sphere (reference) hologram, at the point in the real world where the red hologram was originally placed.
7) Press 'Measure' to capture the position of the blue hologram and write out all data.
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

**placementPlane.txt**: properties of plane where hologram was placed (1-orientation, 2-x and y extents, 3-distance from hologram placement position to center of plane).

**planes.txt**: number of planes detected so far at environment data sample taken during the map stage (1 per line).

**pointcloud.txt**: pointcloud captured during the 'map' stage (1 3D point per line; x, y and z components).

**position0.png**: screenshot on first 'Measure' click (original target hologram position).

**position0.txt**: details of the target hologram placement (1-position in world space, 2-distance from camera to hologram, 3-change in viewing angle around the y-axis from position0 (0), 4-magnitude of hologram drift from position 0 (0).

**position1.png**: screenshot on second 'Measure' click (reference hologram position).

**position1.txt**: details of the reference hologram placement (1-position in world space, 2-distance from camera to hologram, 3-change in viewing angle around the y-axis from position0, 4-magnitude of hologram drift from position 0.

**relocalization_time.txt**: time for device pose tracking to be reestablished after an interruption or temporary loss of tracking (hh:mm:ss:ms).

**rgb#.png**: camera image at each environment data sample taken during 'map' stage.

**temp_map.txt**: device temperature readings (one each frame) during 'map' stage. Currently only available and generated for iOS.

**temp_place.txt**: device temperature readings (one each frame) during 'place' stage. Currently only available and generated for iOS.

**tracking_map.txt**: device pose tracking status (one each frame) during 'map' stage.

**tracking_place.txt**: device pose tracking status (one each frame) during 'place' stage.
