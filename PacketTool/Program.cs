using System;
using System.Security.Cryptography.X509Certificates;

using PacketTool;

public class Program
{
    static void Main(string[] args)
    {
        string path = "../../../../Common/protoc-3.12.3-win64/bin/myProto.proto";
        if(args.Length > 0 )
            path = args[0];
        string[] texts = File.ReadAllLines(path);
        string ClntRegister = "";
        string ServerRegister = "";
        int lineIndex = 0;
        foreach( string text in texts )
        {
            if(text.Contains("message"))
            {
                string packetName = text.Split(' ')[1];
                if(!(packetName.StartsWith("S_") || packetName.StartsWith("s_") || packetName.StartsWith("C_") || packetName.StartsWith("c_")))
                {
                    continue;
                }
                string packetName2 = ChangeName(packetName);
                if (packetName.StartsWith("s_") || packetName.StartsWith("S_"))
                {
                    ClntRegister += string.Format(StringFrame._packetFrame, packetName2.Replace("_",string.Empty), packetName);
                    ClntRegister += "\n";
                }
                else
                {
                    ServerRegister += string.Format(StringFrame._packetFrame, packetName2.Replace("_", string.Empty), packetName);
                    ServerRegister += "\n";
                }
            }
        }
        string _serverPacketManager = string.Format(StringFrame._mainFrame, ServerRegister);
        string _clntPacketManager = string.Format(StringFrame._mainFrame, ClntRegister);
        File.WriteAllText("ServerPacketManager.cs", _serverPacketManager);
        File.WriteAllText("ClntPacketManager.cs", _clntPacketManager);
    }
    public static string ChangeName(string name)
    {
        string[] strings = name.Split('_');
        string ret = "";
        foreach(string s in strings)
        {
            ret+= (s.Substring(0, 1).ToUpper()+s.Substring(1).ToLower())+"_";
        }
        ret = ret.Remove(ret.Length-1);
        return ret;
    }
}
