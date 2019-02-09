using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Jamestel.WebsocketPlayer
{
	public class WebsocketPlayer : MonoBehaviour
	{
		public int SocketServicePort = 80;
		public string httpRoot = "/WebPlayerTemplates";
		public bool openAtLaunch = true;
		private HttpServer httpsv;

		// Use this for initialization
		void Start()
		{
			bool successLaunch = false;

			while (successLaunch == false)
			{
				try
				{
					// TO DO: make seperate class as singleton

					// Create new instance of HttpServer
					httpsv = new HttpServer(SocketServicePort);

					// Set document root path, relative to projects/assets/..
					httpsv.DocumentRootPath = Application.dataPath + httpRoot;

					httpsv.Start();
					successLaunch = true;

				}
				catch (InvalidOperationException e)
				{
					// Check if port is not in use, if not next available port
					if (e.InnerException != null & e.InnerException.GetType().Name == "SocketException")
					{
						Debug.LogError("Port " + SocketServicePort + " is already in use");

						SocketServicePort++;
						httpsv = null;
					}
				}
			}

			this.LaunchPage();

			// Set the HTTP GET/Post request event.
			httpsv.OnGet += (sender, e) => {
				new WebServer.WebServerController(sender, e);
			};

			if (httpsv.IsListening)
			{
				Debug.Log("Listning on port " + httpsv.Port + ", and providing WebSocket/REST services");
			}
		}

		void LaunchPage()
		{
			if (openAtLaunch)
			{
				Application.OpenURL("http://" + System.Net.Dns.GetHostName() + ":" + SocketServicePort);
			}
		}

		void OnApplicationQuit()
		{
			httpsv.Stop();
		}

		public void CreateSocketService(string serviceName)
		{

			httpsv.AddWebSocketService<WebServer.CameraFrame> ("/" + serviceName);
			Debug.Log("Added " + serviceName + " as websocket service");
		}

		public void SendFrame(string serviceName, byte[] frameData)
		{
			httpsv.WebSocketServices["/" + serviceName].Sessions.Broadcast(frameData);
		}

		public IEnumerable<string> GetCameraList()
		{
			return httpsv.WebSocketServices.Paths;
		}
	}
}