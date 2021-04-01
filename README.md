# AR Performance Testing Tool

Unity project folder for an AR application (iOS or Android) to record device tracking data and IMU sensor readings, environment characteristics, and user experience metrics.

Built using the Unity AR Foundation framework. Some Unity AR Foundation sample prefabs and scripts retained for functionality and experimenting with additional features.

Created using Unity 2020.1.16f1. Tested with the following configurations:
- Unity 2020.1.16f1, iOS 14.3, iPhone 11
- Unity 20220.2..., Android 11, Samsung Galaxy Note 10+ 

# Build Instructions (Unity for Android or iOS)

1) Download .zip file and unzip in a convenient location (the Unity project folder is the sub-folder 'ARPerformanceTestingTool-main') 
2) In Unity Hub, under projects, click 'Add' and select the Unity project folder
3) Open the Unity project, and make sure the 'PerformanceTesting' scene (Scenes>SceneIt>PerformanceTesting) is open (should open on startup)
4) Navigate to File>Build Settings, select your Build platform, and click 'Switch Platform'.
5) Make sure only the 'SceneIt>PerformanceTesting' scene is selected in 'Scenes in Build'.
6) For Android, select your connected target device and click 'Build and Run'. For iOS, click 'Build' and follow the instructions for XCode below.

# Build Instructions (XCode for iOS)

1) Building the Unity project for iOS creates an XCode project folder, which you can choose the name and location of. A sample built XCode project folder is provided here: 'ARPerformanceTestingTool (iOS)'.
2) Open the project folder in XCode (XCode 12 required to build for iOS 14).
3) Click 'Signing and Capabilities'. Check 'Automatically manage signing', select your development team, and choose a new bundle identifier.
4) Make sure your target iOS device is connected and selected at the top of the window, then click the play button to build and run.

# Use Instructions

1)

# Output Files

