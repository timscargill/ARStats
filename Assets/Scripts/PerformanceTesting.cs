using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation.Samples;

[RequireComponent(typeof(ARPointCloudManager))]
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARRaycastManager))]

public class PerformanceTesting : MonoBehaviour
{
    [SerializeField]
    ARCameraManager m_CameraManager;
    [SerializeField]
    ARSessionOrigin m_ARSessionOrigin;
    [SerializeField]
    ARPointCloud m_ARPointCloud;
    [SerializeField]
    ARPointCloudManager m_ARPointCloudManager;
    [SerializeField]
    ARPlaneManager m_ARPlaneManager;

    //camera
    public GameObject _camera;
    public ARCameraManager cameraManager
    {
        get { return m_CameraManager; }
        set { m_CameraManager = value; }
    }

    //timestamp of start for folder creation
    private string timestamp = "";

    //app mode: map mode (0) or place mode(1)
    private int app_mode = 0;
    //characterize and frame counts
    private int characterize_count = 0;
    private int measure_count = -1;
    private int placed_ref_count = 0;
    private int frame_count = 0;
    private int characterize_frame_count = 0;

    //variables to hold fps and device temp data
    private string fpsData_map = "";
    private string fpsData_place = "";
#if UNITY_IOS
    private string tempSample = "";
    private string tempData_map = "";
    private string tempData_place = "";
#endif

    //for thermal state (iOS)
    public ThermalStateForIOS thermalStateForIOS
    {
        get => m_ThermalStateForIOS;
        set => m_ThermalStateForIOS = value;
    }

    [SerializeField]
    ThermalStateForIOS m_ThermalStateForIOS;

    //variables to hold device pose and IMU data
    private string deviceData_map = "";
    private string deviceData_place = "";
    private string accData_map = "";
    private string accData_place = "";
    private string gyroData_map = "";
    private string gyroData_place = "";

    //variables to hold tracking status data
    private string trackingSample = "";
    private string trackingData_map = "";
    private string trackingData_place = "";

    //mapping timing
    private string mappingData = "";
    private Stopwatch stopwatch_map = new Stopwatch();
    //initialization timing
    private bool initializing = false;
    private string initializationData = "";
    private Stopwatch stopwatch_initialization = new Stopwatch();
    //relocalizalization timing
    private bool relocalizing = false;
    private string relocalizationData = "";
    private Stopwatch stopwatch_relocalization = new Stopwatch();
    //first plane detection
    private bool firstPlane = false;
    private string firstPlaneData = "";
    private Stopwatch stopwatch_plane = new Stopwatch();

    //light estimation data
    public float? brightness { get; private set; }
    public float? colorTemperature { get; private set; }
    private float current_brightness = 0.0f;
    private float current_colortemp = 0.0f;
    private string lightData = "";

    //camera image data
    private int image_width;
    private int image_height;
    private static List<Texture2D> rgb_textures = new List<Texture2D>();

    //plane data
    private string planeData = "";
    //alignment of plane placed on
    private string placePlaneAlign = "";
    //size of plane placed on
    private Vector2 placePlaneSize;
    //center of plane placed on
    private Vector3 placePlaneCenter;

    //point cloud data
    private string pointcloudData = "";

    //list of vectors to calculate viewing angles
    private List<Vector2> position_xz = new List<Vector2>();
    //list of vectors to hold hologram positions
    private static List<Vector3> position = new List<Vector3>();

    //hologram prefabs
    public GameObject _hologram;
    public GameObject _reference;
    public GameObject spawnedObject { get; private set; }
    public static GameObject spawnedRef { get; private set; }
    //placement guidance circle
    public GameObject _placement;
    public GameObject spawnedPlace { get; private set; }
    private bool guidance = false;
    
    //tracking status
    NotTrackingReason m_CurrentReason;
    bool m_SessionTracking;


    void Awake()
    {
        m_ARSessionOrigin = GetComponent<ARSessionOrigin>();
        m_ARPointCloudManager = GetComponent<ARPointCloudManager>();
        m_ARPlaneManager = GetComponent<ARPlaneManager>();
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }


    void OnEnable()
    {
        //for tracking state
        ARSession.stateChanged += ARSessionOnstateChanged;

        //for light estimation and camera frames
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived += OnCameraFrameReceived;
        }

        //for point cloud
        GetComponent<ARPointCloudManager>().pointCloudsChanged += OnPointCloudsChanged;

        //for thermal state (iOS)
#if UNITY_IOS
        ThermalStateForIOS.ThermalState thermalState = m_ThermalStateForIOS.currentThermalState;
        m_ThermalStateForIOS.stateChanged += OnThermalStateChanged;
#endif
    }


    void OnDisable()
    {
        //for tracking state
        ARSession.stateChanged -= ARSessionOnstateChanged;

        //for light estimation and camera frames
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived -= OnCameraFrameReceived;
        }

        //for point cloud
        GetComponent<ARPointCloudManager>().pointCloudsChanged -= OnPointCloudsChanged;

        //for thermal state (iOS)
#if UNITY_IOS
        m_ThermalStateForIOS.stateChanged -= OnThermalStateChanged;
#endif
    }


    void Start()
    {
        //Needed to access gyroscope data on Android
#if UNITY_ANDROID
        Input.gyro.enabled = true;
#endif

        //start timestamp for scene folder creation
        timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        //Start mapping and firstPlane timers
        stopwatch_map.Start();
        stopwatch_plane.Start(); 
    }


    void Update()
    {
        frame_count += 1;
        characterize_frame_count += 1;

        //Gather FPS and Temp Data
        float fpsSample = 1 / Time.unscaledDeltaTime;
        //for thermal state (iOS)
#if UNITY_IOS
        string tempSample = m_ThermalStateForIOS.currentThermalState.ToString();
#endif
        //Gather Device Position and IMU Data
        string deviceSample = _camera.transform.position.ToString("F4");
        string accSample = Input.acceleration.ToString("F4");
        string gyroSample = Input.gyro.rotationRate.ToString("F4");

        //Format Device Position and IMU Data
        deviceSample = deviceSample.Replace("(", "");
        deviceSample = deviceSample.Replace(")", "");
        deviceSample = deviceSample.Replace(",", "");
        accSample = accSample.Replace("(", "");
        accSample = accSample.Replace(")", "");
        accSample = accSample.Replace(",", "");
        gyroSample = gyroSample.Replace("(", "");
        gyroSample = gyroSample.Replace(")", "");
        gyroSample = gyroSample.Replace(",", "");

        //Gather tracking status
        if (!m_SessionTracking)
        {
            if ((frame_count == 1) && (initializing == false))
            {
                stopwatch_initialization.Start();
                initializing = true;
            }
            else if ((initializing == false) && (relocalizing == false))
            {
                stopwatch_relocalization.Start();
                relocalizing = true;
            }


            m_CurrentReason = ARSession.notTrackingReason;
            switch (m_CurrentReason)
            {
                case NotTrackingReason.Initializing:
                    trackingSample = "0 - Initializing";
                    break;
                case NotTrackingReason.Relocalizing:
                    trackingSample = "0 - Relocalizing";
                    break;
                case NotTrackingReason.ExcessiveMotion:
                    trackingSample = "0 - ExcessiveMotion";
                    break;
                case NotTrackingReason.InsufficientLight:
                    trackingSample = "0 - InsufficientLight";
                    break;
                case NotTrackingReason.InsufficientFeatures:
                    trackingSample = "0 - InsufficientFeatures";
                    break;
                case NotTrackingReason.Unsupported:
                    trackingSample = "0 - Unsupported";
                    break;
                case NotTrackingReason.None:
                    trackingSample = "0 - None";
                    break;
            }
        }
        else
        {
            trackingSample = "1";
            if (initializing == true)
            {
                initializing = false;
                stopwatch_initialization.Stop();
                string initialization_time = stopwatch_initialization.Elapsed.ToString();
                initializationData = initialization_time;
            }
            if (relocalizing == true)
            {
                relocalizing = false;
                stopwatch_relocalization.Stop();
                string relocalization_time = stopwatch_relocalization.Elapsed.ToString();
                relocalizationData += relocalization_time + "\r\n";
                stopwatch_relocalization.Reset();
            }
        }

        //Log collected device data
        if (app_mode == 0)
        {
            fpsData_map += fpsSample.ToString("F2") + "\r\n";
#if UNITY_IOS
            tempData_map += tempSample + "\r\n";
#endif
            deviceData_map += deviceSample + "\r\n";
            accData_map += accSample + "\r\n";
            gyroData_map += gyroSample + "\r\n";
            trackingData_map += trackingSample + "\r\n";
        }
        else
        {
            fpsData_place += fpsSample.ToString("F2") + "\r\n";
#if UNITY_IOS
            tempData_place += tempSample + "\r\n";
#endif
            deviceData_place += deviceSample + "\r\n";
            accData_place += accSample + "\r\n";
            gyroData_place += gyroSample + "\r\n";
            trackingData_place += trackingSample + "\r\n";
        }

        //Log when first plane detected
        if (firstPlane == false)
        {
            int planesDetected = 0;
            foreach (ARPlane plane in m_ARPlaneManager.trackables)
            {
                planesDetected += 1;
            }
            if (planesDetected > 0)
            {
                stopwatch_plane.Stop();
                string firstPlane_time = stopwatch_plane.Elapsed.ToString();
                firstPlaneData = firstPlane_time;
                firstPlane = true;
            }
        }

        //Handle automatic characterization while in mapping mode
        if (app_mode == 0)
        {
            //first scene characterization after 50 frames
            if (characterize_count == 0)
            {
                if (characterize_frame_count == 50)
                {
                    characterize_frame_count = 0;
                    characterize_count = characterize_count + 1;
                    StartCoroutine(Characterize());
                }
            }
            //subsequent scene characterization every 100 frames
            else
            {
                if (characterize_frame_count == 100)
                {
                    characterize_frame_count = 0;
                    characterize_count = characterize_count + 1;
                    StartCoroutine(Characterize());
                }
            }
        }

        //Handle Placement Guidance (if a plane is detected and tracking is running) 
        if ((firstPlane == true) && (trackingSample == "1"))
        {
            var ray = new Ray(_camera.transform.position, _camera.transform.forward);

            if (m_RaycastManager.Raycast(ray, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                //if guidance isn't on yet, instantiate guidance circle
                if (guidance == false)
                {
                    guidance = true;
                    var hitPose = s_Hits[0].pose;
                    spawnedPlace = Instantiate(_placement, hitPose.position, hitPose.rotation);
                }
                //else change position of guidance circle
                else
                {
                    var hitPose = s_Hits[0].pose;
                    spawnedPlace.transform.position = hitPose.position;
                    spawnedPlace.transform.rotation = hitPose.rotation;
                }
            }
        }
    }

    //Handle temperature status
#if UNITY_IOS
    void OnThermalStateChanged(ThermalStateForIOS.ThermalStateChange thermalStateChangeArgs)
    {
        tempSample = thermalStateChangeArgs.currentThermalState.ToString();
    }
#endif

    //Handle tracking status
    void ARSessionOnstateChanged(ARSessionStateChangedEventArgs obj)
    {
        m_SessionTracking = obj.state == ARSessionState.SessionTracking ? true : false;
    }


    //Handle light estimation and camera image data
    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (app_mode == 0)
        {
            //light estimation
            if (eventArgs.lightEstimation.averageBrightness.HasValue)
            {
                brightness = eventArgs.lightEstimation.averageBrightness.Value;
                current_brightness = brightness.Value;
            }

            if (eventArgs.lightEstimation.averageColorTemperature.HasValue)
            {
                colorTemperature = eventArgs.lightEstimation.averageColorTemperature.Value;
                current_colortemp = colorTemperature.Value;
            }

            if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                return;
            }

            // Choose an RGBA format.
            // See XRCameraImage.FormatSupported for a complete list of supported formats.
            var format = TextureFormat.RGBA32;
            //var format = TextureFormat.Alpha8;

            image_width = image.width;
            image_height = image.height;

            if (m_CameraTexture == null || m_CameraTexture.width != image_width || m_CameraTexture.height != image_height)
            {
                m_CameraTexture = new Texture2D(image_width, image_height, format, false);
            }

            // Convert the image to format, flipping the image across the Y axis.
            // We can also get a sub rectangle, but we'll get the full image here.
            var conversionParams = new XRCpuImage.ConversionParams(image, format, XRCpuImage.Transformation.MirrorY);

            // Texture2D allows us write directly to the raw texture data
            // This allows us to do the conversion in-place without making any copies.
            var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
            try
            {
                image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
            }
            finally
            {
                // We must dispose of the XRCpuImage after we're finished
                // with it to avoid leaking native resources.
                image.Dispose();
            }

            // Apply the updated texture data to our texture
            m_CameraTexture.Apply();
        }
    }


    //Handle point cloud data
    void OnPointCloudsChanged(ARPointCloudChangedEventArgs eventArgs)
    {
        if (app_mode == 0)
        {
            foreach (var pointCloud in eventArgs.updated)
            {
                if (!pointCloud.positions.HasValue)
                    return;

                var positions = pointCloud.positions.Value;

                // Store all the positions over time associated with their unique identifiers
                if (pointCloud.identifiers.HasValue)
                {
                    var identifiers = pointCloud.identifiers.Value;
                    for (int i = 0; i < positions.Length; ++i)
                    {
                        m_Points[identifiers[i]] = positions[i];
                    }
                }
            }
        }
    }


    IEnumerator Characterize()
    {
        //record latest light estimation values
        lightData += current_brightness.ToString("F4") + "," + current_colortemp.ToString("F4") + "\r\n";

        //record latest plane data
        int planeCount = 0;
        var planeManager = GetComponent<ARPlaneManager>();
        foreach (ARPlane plane in planeManager.trackables)
        {
            planeCount += 1;
        }
        planeData += planeCount.ToString("D") + "\r\n";

        //record latest camera image
        Texture2D _rgbTexture = new Texture2D(image_width, image_height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(m_CameraTexture, _rgbTexture);
        rgb_textures.Add(_rgbTexture);

        yield return null;
    }


    public void HandlePlaceClick()
    {
        //on first click switch to place mode, write out data
        if (app_mode == 0)
        {
            app_mode = 1;

            //stop mapping timing
            stopwatch_map.Stop();
            string mapping_time = stopwatch_map.Elapsed.ToString();
            mappingData = mapping_time;
        }

        //place spheres on plane and get positions
        var ray = new Ray(_camera.transform.position, _camera.transform.forward);

        if (m_RaycastManager.Raycast(ray, s_Hits, TrackableType.PlaneWithinPolygon))
        {        
            var hitPose = s_Hits[0].pose;

            if ((measure_count == -1) || (measure_count == 0))
            {
                //get placement plane info
                TrackableId planeHit_ID = s_Hits[0].trackableId;
                ARPlane m_ARPlaneHit = m_ARPlaneManager.GetPlane(planeHit_ID);
                placePlaneAlign = m_ARPlaneHit.alignment.ToString();
                placePlaneSize = m_ARPlaneHit.extents;
                placePlaneCenter = m_ARPlaneHit.center;

                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(_hologram, hitPose.position, hitPose.rotation);
                    position.Add(hitPose.position);
                    position_xz.Add(position[0] - _camera.transform.position);
                    measure_count = 0;
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                    position[0] = hitPose.position;
                    position_xz[0] = position[0] - _camera.transform.position;
                }
            }
            else if (measure_count > 0)
            {
                if (spawnedRef == null)
                {
                    //if want to use more than 1 drift measurement position, add more empty vectors to list
                    spawnedRef = Instantiate(_reference, hitPose.position, hitPose.rotation);
                    position.Add(new Vector3(0.0f, 0.0f, 0.0f)); //add position 1
                    position_xz.Add(new Vector2(0.0f, 0.0f)); //add position 1
                }
                else
                {
                    spawnedRef.transform.position = hitPose.position;
                }
                placed_ref_count = measure_count;
                position[placed_ref_count] = hitPose.position;
            }
        }
    }


    public void HandleMeasureClick()
    {
        //create folder for app
        Directory.CreateDirectory(Application.persistentDataPath + "/" + timestamp);

        //calculate distance to hologram
        float dist = Vector3.Distance(_camera.transform.position, position[measure_count]);
        //calculate angle of rotation around y (vertical) axis from original viewing position
        position_xz[placed_ref_count] = new Vector2(position[placed_ref_count].x, position[placed_ref_count].z) - new Vector2(_camera.transform.position.x, _camera.transform.position.z);
        float angle = Vector2.Angle(position_xz[0], position_xz[placed_ref_count]);
        //calculate drift
        float drift = Vector3.Distance(position[0], position[measure_count]);
        //write out position data
        string fileString_measure = "/position" + measure_count + ".txt";
        string filepath_measure = Application.persistentDataPath + "/" + timestamp + fileString_measure;
        using (StreamWriter sw = new StreamWriter(filepath_measure))
        {
            sw.WriteLine(position[0].ToString("F4"));
            sw.WriteLine(dist.ToString("F4"));
            sw.WriteLine(angle.ToString("F4"));
            sw.WriteLine(drift.ToString("F4"));
        }

        //write out placement data
        float planeCenterDist = Vector3.Distance(position[0], placePlaneCenter);
        string fileString_placementPlane = "/placementPlane.txt";
        string filepath_placementPlane = Application.persistentDataPath + "/" + timestamp + fileString_placementPlane;
        using (StreamWriter sw = new StreamWriter(filepath_placementPlane))
        {
            sw.WriteLine(placePlaneAlign);
            sw.WriteLine(placePlaneSize.x.ToString("F4") + " " + placePlaneSize.y.ToString("F4"));
            sw.WriteLine(planeCenterDist.ToString("F4"));
        }

        //capture screenshot
        string screenshotString = "/" + timestamp + "/position" + measure_count + ".png";
        ScreenCapture.CaptureScreenshot(screenshotString);

        //on final time (currently set to position 1) end experiment, write out necessary data
        if (measure_count == 1)
        {
            //write light data
            string filepath_light = Application.persistentDataPath + "/" + timestamp + "/light.txt";
            using (StreamWriter sw = new StreamWriter(filepath_light))
            {
                sw.WriteLine(lightData);
            }

            //write planes data
            string filepath_planes = Application.persistentDataPath + "/" + timestamp + "/planes.txt";
            using (StreamWriter sw = new StreamWriter(filepath_planes))
            {
                sw.WriteLine(planeData);
            }

            pointcloudData = "";
            foreach (var point in m_Points)
            {
                pointcloudData += point.Value.ToString("F4") + "\r\n";
            }
            pointcloudData = pointcloudData.Replace("(", "");
            pointcloudData = pointcloudData.Replace(")", "");
            pointcloudData = pointcloudData.Replace(",", "");

            //write pointcloud data
            string filepath_pc = Application.persistentDataPath + "/" + timestamp + "/pointcloud.txt";
            using (StreamWriter sw = new StreamWriter(filepath_pc))
            {
                sw.WriteLine(pointcloudData);
            }

            string filepath_fpsM = Application.persistentDataPath + "/" + timestamp + "/fps_map.txt";
            using (StreamWriter sw = new StreamWriter(filepath_fpsM))
            {
                sw.WriteLine(fpsData_map);
            }
            string filepath_fpsP = Application.persistentDataPath + "/" + timestamp + "/fps_place.txt";
            using (StreamWriter sw = new StreamWriter(filepath_fpsP))
            {
                sw.WriteLine(fpsData_place);
            }

#if UNITY_IOS
            string filepath_tempM = Application.persistentDataPath + "/" + timestamp + "/temp_map.txt";
            using (StreamWriter sw = new StreamWriter(filepath_tempM))
            {
                sw.WriteLine(tempData_map);
            }
            string filepath_tempP = Application.persistentDataPath + "/" + timestamp + "/temp_place.txt";
            using (StreamWriter sw = new StreamWriter(filepath_tempP))
            {
                sw.WriteLine(tempData_place);
            }
#endif

            //write device position and IMU data
            string filepath_deviceM = Application.persistentDataPath + "/" + timestamp + "/device_map.txt";
            using (StreamWriter sw = new StreamWriter(filepath_deviceM))
            {
                sw.WriteLine(deviceData_map);
            }
            string filepath_deviceP = Application.persistentDataPath + "/" + timestamp + "/device_place.txt";
            using (StreamWriter sw = new StreamWriter(filepath_deviceP))
            {
                sw.WriteLine(deviceData_place);
            }

            string filepath_accM = Application.persistentDataPath + "/" + timestamp + "/acc_map.txt";
            using (StreamWriter sw = new StreamWriter(filepath_accM))
            {
                sw.WriteLine(accData_map);
            }
            string filepath_accP = Application.persistentDataPath + "/" + timestamp + "/acc_place.txt";
            using (StreamWriter sw = new StreamWriter(filepath_accP))
            {
                sw.WriteLine(accData_place);
            }

            string filepath_gyroM = Application.persistentDataPath + "/" + timestamp + "/gyro_map.txt";
            using (StreamWriter sw = new StreamWriter(filepath_gyroM))
            {
                sw.WriteLine(gyroData_map);
            }
            string filepath_gyroP = Application.persistentDataPath + "/" + timestamp + "/gyro_place.txt";
            using (StreamWriter sw = new StreamWriter(filepath_gyroP))
            {
                sw.WriteLine(gyroData_place);
            }

            //write out tracking data
            string filepath_trackingM = Application.persistentDataPath + "/" + timestamp + "/tracking_map.txt";
            using (StreamWriter sw = new StreamWriter(filepath_trackingM))
            {
                sw.WriteLine(trackingData_map);
            }
            string filepath_trackingP = Application.persistentDataPath + "/" + timestamp + "/tracking_place.txt";
            using (StreamWriter sw = new StreamWriter(filepath_trackingP))
            {
                sw.WriteLine(trackingData_place);
            }

            //write out timing data
            string filepath_map = Application.persistentDataPath + "/" + timestamp + "/mapping_time.txt";
            using (StreamWriter sw = new StreamWriter(filepath_map))
            {
                sw.WriteLine(mappingData);
            }
            string filepath_initialize = Application.persistentDataPath + "/" + timestamp + "/initialization_time.txt";
            using (StreamWriter sw = new StreamWriter(filepath_initialize))
            {
                sw.WriteLine(initializationData);
            }
            string filepath_relocalize = Application.persistentDataPath + "/" + timestamp + "/relocalization_time.txt";
            using (StreamWriter sw = new StreamWriter(filepath_relocalize))
            {
                sw.WriteLine(relocalizationData);
            }
            string filepath_firstPlane = Application.persistentDataPath + "/" + timestamp + "/firstPlane_time.txt";
            using (StreamWriter sw = new StreamWriter(filepath_firstPlane))
            {
                sw.WriteLine(firstPlaneData);
            }

            //write out rgb camera data
            for (var i = 0; i < rgb_textures.Count; i++)
            {
                var bytes = rgb_textures[i].EncodeToPNG();
                string filepath_rgb = Application.persistentDataPath + "/" + timestamp + "/rgb" + (i+1) + ".png";
                File.WriteAllBytes(filepath_rgb, bytes);
            }
        }
        measure_count = measure_count + 1;
    }

    public Texture2D m_CameraTexture;

    Dictionary<ulong, Vector3> m_Points = new Dictionary<ulong, Vector3>();

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}
