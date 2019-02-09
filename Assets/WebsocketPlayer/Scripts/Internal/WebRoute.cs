using System;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace Jamestel.WebsocketPlayer.WebServer
{
	public class WebRoute
	{
		public delegate void webFuncObj(HttpListenerRequest request, HttpListenerResponse response);
		public string httpMethod;
		public string path;
		public webFuncObj webFunc;

		public WebRoute(string _httpMethod, string _path, webFuncObj f)
		{
			httpMethod = _httpMethod;
			path = _path;
			webFunc = f;
		}
	}
}