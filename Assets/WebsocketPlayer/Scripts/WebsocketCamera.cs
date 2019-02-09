using UnityEngine;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Jamestel.WebsocketPlayer.CameraControl
{
	public class WebsocketCamera : MonoBehaviour {
		public GameObject WebsocketPlayer;
		public string serviceName = "";
		public int width = 1280;
		public int height = 720;
		public float fps = 24f;
		[Range(1, 100)]
		public long quality = 75L;
		public float avgFPS = 0;
		private Camera rendCamera;
		private RenderTexture renderTexture;
		private Texture2D texture;
		private Rect rect;
		private byte[] rawTextureData;
		private WebsocketPlayer websocket;
	
		void Start()
		{
			renderTexture = new RenderTexture(width, height, 24);
	
			rendCamera = this.GetComponent<Camera>();
	
			texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			rect = new Rect(0, 0, width, height);
	
			// Get script WebSocketVideo from GameObject WebSocketVideo
			websocket = WebsocketPlayer.GetComponent<WebsocketPlayer>();
			
			// Create name for socket service if null
			if (string.IsNullOrWhiteSpace(serviceName))
			{
				serviceName = name;
			}
	
			serviceName = serviceName.Replace(" ", "_");
			websocket.CreateSocketService(serviceName);
	
			Thread thread = new Thread(() => frameLoop(width, height));
	
			thread.IsBackground = true;
			thread.SetApartmentState(System.Threading.ApartmentState.STA);
			
			thread.Start();
		}
		
		void Update()
		{
			rendCamera.targetTexture = renderTexture;
			RenderTexture.active = renderTexture;
	
			rendCamera.Render();

			texture.ReadPixels(rect, 0, 0);

			// Clear render texture as to not block display rendering
			RenderTexture.active = null;
			rendCamera.targetTexture = null;
		}

		void OnPreRender()
		{
			rawTextureData = texture.GetRawTextureData();
		}
	
		// Frame processing loop
		void frameLoop(int width, int height)
		{
			Stopwatch fpsTimer;
			Stopwatch fpsCountTimer;
			float mspf = 1000 / fps;
			int frames = 0;
			ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
	
			fpsCountTimer = Stopwatch.StartNew();
	
			while (true)
			{
				if (rawTextureData != null)
				{
					fpsTimer = Stopwatch.StartNew();
	
					byte[] _rawTextureData = rawTextureData;
					byte[] bitmapRender = new byte[_rawTextureData.Length];
	
					// Rearrange _rawTextureData from bottom-up big-endian to top-down small-endian
					Parallel.For(0, height, row =>
					{
						for (int col = 0; col < width; col++) 
						{
							for (int i = 0; i < 4; i++) 
							{
								bitmapRender[row * 4 * width + 4 * col + i] = _rawTextureData[(height - 1 - row) * 4 * width + 4 * col + (3 - i)];
							}
						}
					});

					// Load bitmap from byte bitmapRender
					Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
					var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					System.Runtime.InteropServices.Marshal.Copy(bitmapRender, 0, data.Scan0, bitmapRender.Length);
					bitmap.UnlockBits(data);
	
					// jpeg compression quality - realtime change
					EncoderParameters jpegParams = new EncoderParameters(1);
					EncoderParameter jpegParam = new EncoderParameter(Encoder.Quality, quality);
					jpegParams.Param[0] = jpegParam;
	
					using (MemoryStream finalFrameRender = new MemoryStream()) 
					{
						bitmap.Save(finalFrameRender, jpegCodec, jpegParams);
	
						// Send frame for websocket delivery
						websocket.SendFrame(serviceName, finalFrameRender.ToArray());
	
						// FPS counter
						frames++;
					}
	
					// Average FPS
					avgFPS = frames / (fpsCountTimer.ElapsedMilliseconds / 1000f);
	
					// Timestep - wait to meet our FPS target
					if (fpsTimer.ElapsedMilliseconds < mspf)
					{
						Thread.Sleep((int)(mspf - fpsTimer.ElapsedMilliseconds));
					}
				}
			}
		}
	
		// Get ImageCodecInfo mime type required for bitmap.save overloaded method
		private ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}
	}
}
