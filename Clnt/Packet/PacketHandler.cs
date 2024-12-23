using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clnt;

using Google.Protobuf;

using ServerCore;

namespace Packet
{
    public class PacketHandler
    {
        public static void S_Chat_Handler(Session s, IMessage pkt)
        {
            ClntSession session = s as ClntSession;
            S_Chat packet = pkt as S_Chat;
            Console.WriteLine(packet.Str);
        }

        internal static void S_Attack_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_DeSpawn_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_Enter_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_Fight_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_Leave_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_MonList_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_Move_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_SignIn_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_SignUp_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }

        internal static void S_Spawn_Handler(Session session, IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
