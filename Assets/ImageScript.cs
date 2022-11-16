using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ImageScript : MonoBehaviour
{
    private Texture2D _texture;

    private RectTransform _rectTransform;

    private int width = 0;

    private int height = 0;

    private Texture2D _srcTexture;

    private RawImage _rawImage;

    private int imageCount = 0;

    public Text textField;

    private TcpClient _tcpClient;

    private int tcpPort = 13000;
    private byte[] buffer;
    private NetworkStream _tcpStream;
    
    // Start is called before the first frame update
    void Start() {
        _rawImage = GetComponent<RawImage>();
        _rectTransform = _rawImage.GetComponent<RectTransform>();
        width = (int)_rectTransform.rect.width;
        height = (int)_rectTransform.rect.height;

        _srcTexture = _rawImage.texture as Texture2D;
        _texture = new Texture2D(28, 28);
        _texture.filterMode = FilterMode.Point;
        
        for (int i = 0; i < 28; i++) {
            for (int k = 0; k < 28; k++) {
                _texture.SetPixel(i, k, Color.black);
            }
        }

        _texture.Apply();
        
        _rawImage.texture = _texture;
        
        _testPixelData = new float[10000, 784];
        GetDataMNIST(@"H:\dev\Operation-Terminator\Resources\mnist_test.csv");
        for (int i = 0; i < 28; i++) {
            for (int k = 0; k < 28; k++) {
                var index = i * 28 + k;
                var color = new Color(255.0f, 255.0f, 255.0f, _testPixelData[0, index]);
                
                Console.WriteLine(color);
                _texture.SetPixel(i, k, color);
                //_texture.
            }
        }

        _texture.Apply();
        
        
        SetupTcpClient();
    }

    void SetupTcpClient() {
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");

        _tcpClient = new TcpClient();
        _tcpClient.Connect(localAddr, tcpPort);
        if (!_tcpClient.Connected) {
            return;
        }
        
        Debug.Log("Tcp client connected!");
        buffer = new byte[1024];
        _tcpStream = _tcpClient.GetStream();

    }

    int GetTerminatorImageGuess(int imageIndex) {
        
        _tcpStream.Write(BitConverter.GetBytes(imageIndex), 0, 4);
        _tcpStream.Read(buffer, 0, buffer.Length);
        int guess = BitConverter.ToInt32(buffer, 0);
        return guess;
    }

    // Update is called once per frame
    void FixedUpdate() {
        imageCount++;
        int fps = 50;
        int updateAfterSeconds = 3;
        
        if (imageCount % (fps * updateAfterSeconds) == 0) {
            
            
            var imageIndex = Mathf.FloorToInt(imageCount / 60);
            int guess = GetTerminatorImageGuess(imageIndex);
            for (int i = 0; i < 28; i++) {
                for (int k = 0; k < 28; k++) {
                    var index = i * 28 + k;
                    var color = new Color(255.0f, 255.0f, 255.0f, _testPixelData[imageIndex, index]);
                
                    Console.WriteLine(color);
                    _texture.SetPixel(i, k, color);
                }
            }
            _texture.Apply();
            if (textField != null) {
                textField.text = "Terminator guess: " + guess;
            }
        }

    }

    private float[,] _testPixelData;
    
    void GetDataMNIST(string path) {
        //float[,] pixelData = new float[60000, 784];
        //int[] labels = new int[60000];
        float[,] testPixelData = new float[10000, 784];
        using (var reader = new StreamReader(@"H:\dev\Operation-Terminator\Resources\mnist_test.csv")) {
               
            int i = 0;
            reader.ReadLine();
            while (!reader.EndOfStream && i < 10000) {
                var line = reader.ReadLine();
                var values = line.Split(',');
                var label = int.Parse(values[0]);
                for (int k = 1; k < values.Length; k++) {
                    _testPixelData[i, k - 1] = int.Parse(values[k]);
                }

                i++;
            }
        }
    }
}
