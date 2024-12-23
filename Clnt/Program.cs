using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using Clnt;

using Google.Protobuf;

using Packet;

using ServerCore;

class smanager
{
    public static object _lock= new object();
    public static List<ClntSession> sessions = new List<ClntSession>();
    public static ClntSession GetS()
    {
        ClntSession s = new ClntSession();
        lock(_lock)
        {
            sessions.Add(s);
        }
        return s;
    }
    public static void OutS(ClntSession s)
    {
        lock(_lock)
        {
            sessions.Remove(s);
        }
    }
}


public class Program
{
    static void Main(string[] args)
    {
        PacketManager.Instance.Init();

        string hostName = Dns.GetHostName();
        IPHostEntry entry = Dns.GetHostEntry(hostName);
        IPAddress addr = entry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(addr, 7777);

        Connecter con = new Connecter();

        

        //con.Connect(endPoint, () => { return new ClntSession(); },2000);
        con.Connect(endPoint, () => { return smanager.GetS(); },1000);

        while (true)
        {
            Random r = new Random();
            lock (smanager._lock)
            {
                foreach (ClntSession c in smanager.sessions)
                {
                    C_Chat pkt = new C_Chat();
                    pkt.Str = $"hello server i am {r.Next()}";
                    c.Send(pkt);
                }
            }
            Thread.Sleep(100);
        }
    }
}
