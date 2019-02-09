using System.Text;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using Jamestel.WebsocketPlayer;
using Jamestel.WebsocketPlayer.WebServer;

public class Routes : MonoBehaviour {
	WebsocketPlayer websocket;

	// Register HTTP GET/POST web routes here
	void Start () {
		// Create object of WebsocketPlayer instance
		websocket = GetComponent<WebsocketPlayer>();

		// Hello world lambda syntax example
		Router.WebRoutes.Add(new WebRoute(
			"GET",
			"/hello_world",
			(HttpListenerRequest request, HttpListenerResponse response) =>
			{
				response.ContentType = "text/html";
				response.ContentEncoding = Encoding.UTF8;

				response.WriteContent(Encoding.UTF8.GetBytes("Hello World"));
			}
		));

		Router.WebRoutes.Add(new WebRoute(
			"GET",
			"/api/websocketlist",
			WebsocketList
		));
	}

	// Returns JS array of available websockets
	void WebsocketList(HttpListenerRequest request, HttpListenerResponse response)
	{
		// Start JS array syntax for web client 
		string content = "var cameraList = [";

		foreach (var path in websocket.GetCameraList())
		{
			// This will leave a trailing comma (this triggers OCD)
			content += ('"' + path + '"' + ",");
		}

		content += "];";

		response.ContentType = "text/javascript";
		response.ContentEncoding = Encoding.UTF8;

		response.WriteContent(Encoding.UTF8.GetBytes(content));
	}
}