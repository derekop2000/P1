using System;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;

using Google.Protobuf;

using Packet;

using Server;
using Server.Contents.GameData;
using Server.Contents.Room;
using Server.DB;
using Server.Session;

using ServerCore;

public class Program
{
    static Listener _listener = new Listener();
    static void Main(string[] args)
    {
        GameDataManager.Init();
        PacketManager.Instance.Init();
        RoomManager.Instance.Init();

        string hostName = Dns.GetHostName();
        IPHostEntry entry = Dns.GetHostEntry(hostName);
        IPAddress addr = entry.AddressList[0];
        IPEndPoint point = new IPEndPoint(addr, 7777);
        _listener.Init(point, () => { return new ServerSession(); });
        while (true)
        {
            RoomManager.Instance.RoomUpdate();
            Thread.Sleep(100);
        }

        DbPool.Instance.Dispose();
    }
}
