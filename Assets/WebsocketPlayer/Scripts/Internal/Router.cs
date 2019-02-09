using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace Jamestel.WebsocketPlayer.WebServer
{
	public sealed class Router
	{
		private static Router instance = null;
		private static readonly object padlock = new object();
		private static ArrayList webRoutesList = new ArrayList();

		public static Router WebRoutes
		{
			get
			{
				lock (padlock)
				{
					if (instance == null)
					{
						instance = new Router();
					}
					return instance;
				}
			}
		}

		public void Add(WebRoute route)
		{
			webRoutesList.Add(route);
			Debug.Log("Added " + route.path + " as web route");
		}

		public WebRoute Find(string httpMethod, string path)
		{
			foreach (WebRoute route in webRoutesList)
			{
				if (httpMethod.Equals(route.httpMethod, StringComparison.InvariantCultureIgnoreCase) && path.Equals(route.path, StringComparison.InvariantCultureIgnoreCase))
				{
					return route;
				}
			}
			return null;
		}
	}
}