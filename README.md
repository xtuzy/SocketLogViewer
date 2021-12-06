# SocketLogViewer

![app](https://github.com/xtuzy/SocketLogViewer/blob/master/Resources/app.png)

When use ReloadPreview, if you use multiple platform, if you not at debug , you can use this accept debug message.

It use 399 port, when you run it, you can see it ip and port, so you just need use sock client send message.

Such as:
```
var client = new MessageClientApp("192.168.0.108",399);
client.SendMessage(sender+"~iOS" +$"~{DateTime.Now}"+ "\n");//~ split Log,Platform,Time
```

**Notice**, if you use vpn..., maybe will get mistake, you can see Setting->Network->Properties, find IP, compare it with the LogViewer Title show, if they are not equal, please restart you compute let they equal.
