# WebsocketPlayer

![websocketplayer_controllergun](https://user-images.githubusercontent.com/7420764/52525340-1add8900-2c76-11e9-8071-56c42bffc6be.gif)

WebsocketPlayer is a Unity plugin designed to expose cameras and internal C# functions via both HTTP and Websockets, perfect for the creation of web based local Unity client(s).

## Features
- Subsecond video streaming
- Multiple cameras supported
- Per camera quality, resolution and FPS controls
- Realtime camera FPS counter
- Extendable HTTP router class
- HTML demo with router example
- Windows, Mac, Linux support (Mono)

## Install

### Dependencies
- [WebsocketSharp](https://github.com/sta/websocket-sharp "WebsocketSharp")

WebsocketPlayer requires the `System.Drawing` library, but Unity by default does not support it. 

```
%UnityFolder%\Editor\Data\Mono\lib\mono\2.0
```

You can manually copy the library to `Assets/Packages` from the above path so your project can reference it.

#### Windows Specific
You can use the included NuGet package config or the following command.

```
Install-Package WebSocketSharp-netstandard -Version 1.0.1
```

Remove the .NET versions that you do not wish to target, check your Unity Mono `Api Compatibility Level` in `PlayerSettings/Configuration`.

## Usage

### Quick Start
Simply import the latest version from [releases](https://github.com/jamestel/WebsocketPlayer/releases "releases") and place the `WebsocketPlayerController` prefrab into your project.

Add the `WebsocketCamera` to any camera object that you wish to be accessible via websocket.

Press play.

### WebsocketPlayerController Prefab

#### HTTP Root
By default, the `WebPlayerTemplates` directory is used as the web server root. This Unity designated folder ignores HTML/JS specifically for web use.

#### Router.cs
`Router.cs` derives in the `Example Scene` included in the plugin. For future updates it is best to not directly edit this, but rather recreate the router outside of the example.

Functions and inline code executed by a web route will exist in the web server thread. The Unity API is not thread-safe, thus API calls will display in the console as an error.
There are multiple solutions, such as using instances defined from `Start()`, dispatched IEnumerator functions and using nonserialized variables. 
- https://stackoverflow.com/a/54184457
- https://github.com/PimDeWitte/UnityMainThreadDispatcher
- https://docs.unity3d.com/2017.3/Documentation/ScriptReference/NonSerializable.html

```csharp
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;
using Jamestel.WebsocketPlayer.WebServer;

public class Routes 
{
    Router.WebRoutes.Add(new WebRoute(
        "GET",                // Type of request, GET or POST supported
        "/test/helloworld",   // Path to the function ex: http://localhost/test/helloworld
        FunctionToExecute     // Function to be executed, lambda syntax also supported
    ));
    
    // WebSocketSharp listener & request are expected arguments
    void FunctionToExecute(HttpListenerRequest request, HttpListenerResponse response)
    {
        string content = "Hello World";

        // Define content type as HTML and encoding UTF-8
        response.ContentType = "text/html";
        response.ContentEncoding = Encoding.UTF8;

        // Write response, get bytes of string for final byte array
        response.WriteContent(Encoding.UTF8.GetBytes(content));
    }
}
```

## Feature Roadmap
- [ ] TurboJPEG integration
- [ ] Wildcard support for web routes
- [ ] Camera texture performance
- [ ] Bidirectional websocket communication
- [ ] Better error handling
- [ ] Websocket audio?

