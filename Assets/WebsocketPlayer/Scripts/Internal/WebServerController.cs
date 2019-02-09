using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Jamestel.WebsocketPlayer.WebServer
{
	public class WebServerController
	{
		private HttpListenerRequest request;
		private HttpListenerResponse response;
		private HttpRequestEventArgs httpEvent;
		public WebServerController(object sender, HttpRequestEventArgs e)
		{
			request = e.Request;
			response = e.Response;

			httpEvent = e;

			// Find matching function if file is not found
			if (!funcServ())
			{
				fileServ();
			}
		}

		private bool fileServ()
		{
			string path = request.RawUrl;
			byte[] contents;

			if (path == "/")
			{
				path += "index.html";
			}

			if (!httpEvent.TryReadFile(path, out contents))
			{
				response.StatusCode = (int)HttpStatusCode.NotFound;
				return false;
			}

			// Temporary
			if (path.EndsWith("html"))
			{
				response.ContentType = "text/html";
			}

			else if (path.EndsWith("js"))
			{
				response.ContentType = "text/javascript";
			}

			else if (path.EndsWith("css"))
			{
				response.ContentType = "text/css";
			}

			else if (path.EndsWith("png"))
			{
				response.ContentType = "image/png";
			}

			else if (path.EndsWith("jpg"))
			{
				response.ContentType = "image/jpeg";
			}

			else {
				response.ContentType = "text/plain";
			}

			response.ContentEncoding = Encoding.UTF8;
			response.WriteContent(contents);

			return true;
		}

		private bool funcServ()
		{
			WebRoute route = Router.WebRoutes.Find(request.HttpMethod, request.RawUrl);

			if (route != null)
			{
				try
				{
					route.webFunc(request, response);
				} catch (Exception e)
				{
					Debug.LogError(e);
					response.StatusCode = (int)HttpStatusCode.InternalServerError;
				}
				return true;
			}
			return false;
		}
	}
}