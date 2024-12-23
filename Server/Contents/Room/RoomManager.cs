using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Contents.Room
{
    public class RoomManager
    {
        private static RoomManager _roomManager = new RoomManager();
        public static RoomManager Instance { get {  return _roomManager; } }
        Dictionary<int,GameRoom> _rooms = new Dictionary<int,GameRoom>();
        public static int _roomSize =0;
        object _lock = new object();
        public void Init()
        {
            _roomSize = Directory.GetFiles("../../../../Common/Map").Length;
            _rooms.Clear();
            for(int roomId=1; roomId <= _roomSize; roomId++)
            {
                GameRoom room = new GameRoom();
                room.Init(roomId);
                _rooms.Add(roomId, room);
            }
        }
        public GameRoom GetRoom(int mapId)
        {
            GameRoom room = null;
            lock(_lock)
            {
                _rooms.TryGetValue(mapId, out room);
            }
            return room;
        }
        public void RoomUpdate()
        {
            foreach(GameRoom room in _rooms.Values)
            {
                room.Push(room.Update);
            }
        }
    }
}
