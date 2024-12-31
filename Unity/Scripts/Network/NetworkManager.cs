using System.Collections;
using System.Collections.Generic;
using System.Net;

using UnityEngine;
using ServerCore;
using Clnt;
using Packet;
using Assets.Scripts.Network;
using Google.Protobuf;

public class NetworkManager : MonoBehaviour
{
    public static bool _pending = false;
    public static NetworkManager Instance { get { return _networkManager; } }
    private static NetworkManager _networkManager = new NetworkManager();
    public static ClntSession _session = new ClntSession();
    private void Awake()
    {
        Screen.SetResolution(640, 480, false);
        Application.runInBackground = true;
    }
    public void Send(IMessage pkt)
    {
        _session.Send(pkt);
    }
    void Start()
    {
        PacketManager.Instance.Init();

        IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress addr = entry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(addr, 7777);
        
        Connecter connecter = new Connecter();
        connecter.Connect(endPoint, () => { return _session; });
    }

    // Update is called once per frame
    void Update()
    {
        var queue = PacketQueue.PopAll();
        while (queue.Count > 0)
        {
            PacketQueueItem item = queue.Dequeue();
            item.Execute();
        }
    }
}
