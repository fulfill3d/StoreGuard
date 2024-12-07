using UnityEngine;
using System.IO;

public class CameraFrameCapture : MonoBehaviour
{
    public int targetFrameRate = 2; // Frames per second
    public string savePath = "CapturedFrames"; // Directory to save frames
    public int frameWidth = 320; // Desired frame width
    public int frameHeight = 180; // Desired frame height

    private float _captureInterval;
    private float _nextCaptureTime;
    private int _frameCount;

    private Camera _captureCamera;
    private RenderTexture _renderTexture;
    private Texture2D _reusableTexture;

    private void Start()
    {
        // Automatically assign the camera from the GameObject this script is attached to
        _captureCamera = GetComponent<Camera>();
        if (_captureCamera == null)
        {
            Debug.LogError("CameraFrameCaptureOptimized: No Camera component found on this GameObject!");
            enabled = false;
            return;
        }

        _captureInterval = 1f / targetFrameRate;
        _nextCaptureTime = Time.time;

        // Ensure the save directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // Set up a smaller RenderTexture for reduced frame size
        _renderTexture = new RenderTexture(frameWidth, frameHeight, 24, RenderTextureFormat.ARGB32);
        _captureCamera.targetTexture = _renderTexture;

        // Create a reusable Texture2D matching the RenderTexture's size
        _reusableTexture = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if (Time.time >= _nextCaptureTime)
        {
            CaptureFrame();
            _nextCaptureTime += _captureInterval;
        }
    }

    private void CaptureFrame()
    {
        var currentRT = RenderTexture.active;
        RenderTexture.active = _renderTexture;

        // Render the camera's view to the RenderTexture
        _captureCamera.Render();

        // Read the pixels from the RenderTexture into the reusable Texture2D
        _reusableTexture.ReadPixels(new Rect(0, 0, frameWidth, frameHeight), 0, 0);
        _reusableTexture.Apply();

        // Encode the frame to PNG format and save it with the camera's name
        var cameraName = _captureCamera.name;
        var filePath = Path.Combine(savePath, $"camera_{cameraName}_frame_{_frameCount:D9}.png");
        var bytes = _reusableTexture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        _frameCount++;
        RenderTexture.active = currentRT;
    }

    private void OnDestroy()
    {
        // Clean up allocated resources
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _captureCamera.targetTexture = null;
        }

        if (_reusableTexture != null)
        {
            Destroy(_reusableTexture);
        }
    }
}
