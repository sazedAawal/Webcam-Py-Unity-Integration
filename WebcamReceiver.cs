using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class WebcamReceiver : MonoBehaviour
{
    public int listenPort = 5005; // Matches Python's SEND_PORT
    public RawImage displayUI;    // Drag a UI RawImage here to see your video

    private UdpClient receiveClient;
    private Thread receiveThread;
    private Texture2D videoTexture;
    
    private byte[] latestReceivedBytes;
    private bool isNewFrameAvailable = false;
    private object lockObject = new object();

    void Start()
    {
        // Allocate initial texture block (will resize automatically on LoadImage)
        videoTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);

        if (displayUI == null)
        {
            Debug.LogError("[WebcamReceiver] Please assign a UI RawImage component to display UI field.");
        }

        try
        {
            receiveClient = new UdpClient(listenPort);
            receiveThread = new Thread(new ThreadStart(ReceivePackets));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Debug.Log($"[WebcamReceiver] Listening for Python webcam on port {listenPort}...");
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebcamReceiver] Socket binding failed: {e.Message}");
        }
    }

    void Update()
    {
        lock (lockObject)
        {
            if (isNewFrameAvailable && latestReceivedBytes != null)
            {
                // Unpack the compressed JPG bytes straight into the GPU texture
                videoTexture.LoadImage(latestReceivedBytes);
                videoTexture.Apply();

                if (displayUI != null)
                {
                    displayUI.texture = videoTexture;
                }
                isNewFrameAvailable = false;
            }
        }
    }

    void ReceivePackets()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (receiveClient != null)
        {
            try
            {
                byte[] data = receiveClient.Receive(ref remoteEP);
                lock (lockObject)
                {
                    latestReceivedBytes = data;
                    isNewFrameAvailable = true;
                }
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    void OnDestroy()
    {
        if (receiveClient != null) { receiveClient.Close(); receiveClient = null; }
        if (receiveThread != null && receiveThread.IsAlive) { receiveThread.Abort(); }
    }
}