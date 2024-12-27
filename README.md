# 멀티플레이 몬스터 배틀 게임 - 개인 프로젝트

## 프로젝트 개요
- 포켓몬스터에서 영감을 받은 멀티플레이 게임 제작
- 개발 기간: 2024.06 ~ 2024.10
- 개발 인원: 1명
---

##  시연 영상

...

---

## 사용 기술 스택  
- **언어**: C#
- **DB 연동**: MySQL
- **프로토콜**: Protocol Buffers - Protobuf
- **프로파일링**: Visual Studio Performance Profiler
- **게임엔진**: Unity

---

## 시스템 다이어그램  
![서버아키텍처](https://github.com/user-attachments/assets/874e03b1-7064-4471-aac4-7838fbbd7479)

---
## 주요 개발 내용
1. **네트워크 I/O 비동기 작업**  
   - 클라이언트-서버 간 비동기 방식 데이터 처리 ( SendAsync, RecvAsync ... )
      <details>
      <summary>상세 보기</summary>
          
     - 메인 스레드 작업과 별도로 네트워크 I/O 요청 작업을 수행하기 위해 SendAsync와 ReceiveAsync 같은 비동기 함수를 사용했다.
         
            // ReceiveAsync 함수 사용 부분
            void RegisterRecv()
            {
                _recvBuffer.Clean();
                _recvEvent.SetBuffer(_recvBuffer._writeSegment);
                try
                {
                    bool pending = _sock.ReceiveAsync(_recvEvent);
                    if(pending == false)
                    {
                        OnRecvCompleted(null,_recvEvent);
                    }
                }
                catch(Exception e) { }
            }
            void OnRecvCompleted(object obj,SocketAsyncEventArgs e)
            {
                if(e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    if (_recvBuffer.OnWrite(_recvEvent.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }
                    int readBytes = OnRecv(_recvBuffer._dataSegment);
                    if (_recvBuffer.OnRead(readBytes) == false)
                    {
                        Disconnect();
                        return;
                    }
                    RegisterRecv();
                }
                else
                {
                    Disconnect();
                }
            }
     
     - 데이터 수신 시 할당받은 메모리를 재사용하기 위한 RecvBuffer 클래스를 만들어 메모리 효율을 높였다.
   
            public class RecvBuffer
            {
             public byte[] _buf;
             public int _bufSize;
         
             int _readPos = 0;
             int _writePos = 0;
         
             public int _freeSize { get {return _bufSize - _writePos; } }
             public int _dataSize { get { return _writePos - _readPos; } }
             public ArraySegment<byte> _readSegment { get { return new ArraySegment<byte>(_buf, _readPos, _dataSize); } }
             public ArraySegment<byte> _writeSegment { get { return new ArraySegment<byte>(_buf, _writePos, _freeSize); } }
         
             public RecvBuffer(int bufSize = 4096)
             {
                 _bufSize = bufSize;
                 _buf = new byte[bufSize];
             }
             public void ResetBuf()
             {
                 if(_readPos == _writePos)
                 {
                     _readPos = _writePos = 0;
                 }
                 else
                 {
                     int dataSize = _dataSize;
                     Array.Copy(_buf, _readPos, _buf, 0, dataSize);
                     _readPos = 0;
                     _writePos = dataSize;
                 }
             }
             public bool OnWrite(int num)
             {
                 if (num > _freeSize)
                     return false;
                 _writePos += num;
                 return true;
             }
             public bool OnRead(int num)
             {
                 if (num > _dataSize)
                     return false;
                 _readPos += num;
                 return true;
             }
         
      </details>

2. **Protobuf 기반 직렬화 작업**  
   
      - 구글의 데이터 직렬화 방식인 Protobuf을 사용하여 데이터 직렬화 / 역직렬화 작업을 했다.
      
      - 바이너리 저장 형식으로 네트워크 전송에 효율적이다.
  
      - 패킷에 번호를 부여하기 위한 열거형과 다양한 데이터 타입을 사용할 수 있어서 채택했다.
        
3. **패킷 코드 자동화**  
   - 패킷을 받은 후 각각의 패킷 핸들러까지 연동되는 부분을 자동화했다.
   - 최초 수작업으로 기본 틀을 만든 후 패킷마다 반복되는 부분을 고려했다.
      <details>
         
      - .proto 파일을 파싱 하여 클래스 파일을 만들었다.
        
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
     - 클래스를 자동 생성 후 배치 파일을 이용하여 각각 필요한 위치에 이동시켰다.
       
               @echo off
               if errorlevel 1 pause
               protoc.exe -I=./ --csharp_out=./ ./myProto.proto
               
               @echo on
               start ../../../PacketTool/bin/Debug/net7.0/PacketTool.exe ./myProto.proto
               @echo off
               xcopy /y "ServerPacketManager.cs" "../../../Server/Packet"
               xcopy /y "ClntPacketManager.cs" "../../../Clnt/Packet"
               xcopy /y "ClntPacketManager.cs" "../../../../../unity/clnt/derekopP1/Assets/Scripts/Network/Packet"
               xcopy /y "MyProto.cs" "../../../Server/Packet"
               xcopy /y "MyProto.cs" "../../../Clnt/Packet"
               xcopy /y "MyProto.cs" "../../../../../unity/clnt/derekopP1/Assets/Scripts/Network/Packet"\
               
               pause
          
      <summary>상세보기</summary>
      </details>

5. **DB 및 데이터 관리**  
   - 자주 변하지 않는 데이터들( 몬스터 데이터 )은 정적 데이터 클래스로 관리하는 방식에서 외부 XML 파일로 관리하는 방식으로 변경했다. 비개발직군( 기획자 )도 데이터 수치를 변경하기 쉽고 자동화 방식으로 유지 보수에 더 적합하다.
   - 설정 파일을 사용하여 데이터베이스 접속 데이터를 관리했고 코드가 간결해졌다.
      <details>
      <summary>개선된 코드 보기</summary>
      개선 전
         
           public DbConnector(string driver = "mysql odbc 8.4 unicode driver", string server = "localhost",string database = "derekopserver",string user = "root",string passwrod = "0000")
           {
               StringBuilder sb = new StringBuilder();
               sb.AppendFormat("driver={{{0}}};server={1};database={2};user={3};password={4};", driver, server, database, user, passwrod);
               string op = sb.ToString();
               _connection = new OdbcConnection(op);
               _connection.Open();
   
               _command = _connection.CreateCommand();
           }
      
      개선 이후
      
            public DbConnector()
            {
                string op = ConfigurationManager.ConnectionStrings["DBconnect"].ConnectionString;
                _connection = new OdbcConnection(op);
                _connection.Open();
            
                _command = _connection.CreateCommand();
            }
      
      </details>
   - 변경이 잦은 데이터들은 대량의 수정사항을 관리하기 위해 MySql으로 관리한다.
      <details>
      <summary>유저 정보 불러오는 부분</summary>
         
               public static void C_SignIn_Handler(Session s, IMessage pkt)
               {
                   ServerSession session = s as ServerSession;
                   C_SignIn packet = pkt as C_SignIn;
               
                   DbConnector con = DbPool.Instance.Pop();
               
                   con._command.CommandText = $"select * from user_login where username = \"{packet.Id}\"";
                   ushort state = 1;
                   using(OdbcDataReader reader =  con._command.ExecuteReader())
                   {
                       while(reader.Read())
                       {
                           string password =  reader.GetString(reader.GetOrdinal("password"));
                           if (password == packet.Password)
                           {
                               state = 0;
                               session._dataBaseId = reader.GetInt32(reader.GetOrdinal("userid"));
                           }
                       }
                   }
                   S_SignIn replyPkt = new S_SignIn();
                   replyPkt.State = state;
                   session.Send(replyPkt);
               
                   if (state != 0)
                       return;
                   GameRoom room = RoomManager.Instance.GetRoom(1);
               
                   Player player = new Player();
                   player._session = session;
                   session._player = player;
               
                   using(OdbcCommand potionCommand = con._connection.CreateCommand())
                   {
                       potionCommand.CommandText = $"select * from items where ownerid = {session._dataBaseId}";
                       using(OdbcDataReader potionReader = potionCommand.ExecuteReader())
                       {
                           bool exist = false;
                           while(potionReader.Read())
                           {
                               exist = true;
                               session._player._hpPotionCnt = potionReader.GetInt32(potionReader.GetOrdinal("hppotion"));
                           }
                           if(exist == false)
                           {
                               session._player._hpPotionCnt = 3;
                               using(OdbcCommand command = con._connection.CreateCommand())
                               {
                                   command.CommandText = $"insert into items (ownerid,hppotion) values({session._dataBaseId},3)";
                                   command.ExecuteNonQuery();
                               }
                           }
                       }
                   }
               
                   con._command.CommandText = $"select * from pets where ownerid = {session._dataBaseId}";
                   using(OdbcDataReader reader = con._command.ExecuteReader())
                   {
                       int cnt = 0;
                       while (reader.Read())
                       {
                           cnt++;
                           MyMonster m = new MyMonster(0);
                           m._cp.Hp = reader.GetInt32(reader.GetOrdinal("Hp"));
                           m._cp.MaxHp = reader.GetInt32(reader.GetOrdinal("maxHp"));
                           m._cp.HpIncrease = reader.GetInt32(reader.GetOrdinal("HpIncrease"));
                           m._cp.Damage = reader.GetInt32(reader.GetOrdinal("Damage"));
                           m._cp.DamageIncrease = reader.GetInt32(reader.GetOrdinal("DamageIncrease"));
                           m._cp.MonNum = reader.GetInt32(reader.GetOrdinal("MonNum"));
                           m._cp.Exp = reader.GetInt32(reader.GetOrdinal("Exp"));
                           m._cp.MaxExp = reader.GetInt32(reader.GetOrdinal("MaxExp"));
                           m._cp.RewardExp = reader.GetInt32(reader.GetOrdinal("RewardExp"));
                           m._cp.Level = reader.GetInt32(reader.GetOrdinal("Level"));
               
                           player.AddMon(m);
                           if (player._mainMon == null)
                               player._mainMon = m;
                       }
                       if (cnt == 0)
                       {
                           MyMonster m = new MyMonster(1);
                           
                           using(OdbcCommand command = con._connection.CreateCommand())
                           {
                               command.CommandText = $"insert into pets (ownerid,maxhp,hp,hpIncrease,damage,damageIncrease,monNum,level,exp,maxExp,rewardExp) values({session._dataBaseId},{m._cp.MaxHp},{m._cp.Hp},{m._cp.HpIncrease},{m._cp.Damage},{m._cp.DamageIncrease},{m._cp.MonNum},{m._cp.Level},{m._cp.Exp},{m._cp.MaxExp},{m._cp.RewardExp})";
                               command.ExecuteNonQuery();
                           }
                           player.AddMon(m);
                           player._mainMon = m;
                       }
                   }
               
                   DbPool.Instance.Push(con);
                   room.Push(room.Enter, player);
            }
      </details>
      
6. **게임 컨텐츠 작업**  
   - 캐릭터 및 몬스터 이동 동기화
   - 배틀 구현 ( 턴제 + 명령 기반 전투 )
   - 채팅 시스템
7. **멀티스레드 환경에서의 서버 성능 테스트 및 최적화**  
   - 대규모 트래픽을 수용하기 위한 부하 테스트를 진행했다.
      - 목표 : 평균 100ms 이하의 응답 시간으로 최대 사용자 수용
      - 시나리오 : 각 유저가 100ms마다 이동 요청을 보내고 서버가 이를 처리하여 모든 클라이언트에 브로드캐스트하는 시뮬레이션
      - 서버 사양 : intel i5 - 10400F CPU @ 2.9GHz ( 6코어 12스레드 ) , 16GB 메모리
        
   - 첫 번째 방법 - 기본 JobQueue를 이용한 처리 방식
      - 다수의 스레드들이 네트워크 요청을 비동기 방식으로 받은 후 공용자원인 작업큐에 저장 이후 1개의 스레드가 전담하여 작업들을 수행한다.
        <details>
        <summary>작업 큐 코드 보기</summary>
           
            // Job 클래스
            public abstract class IJob
            {
                public abstract void Execute();
            }
            
            public class Job : IJob
            {
                Action _action;
                public Job(Action action)
                {
                    _action = action;
                }
                public override void Execute()
                {
                    _action();
                }
            }
            public class Job<T1> : IJob
            {
                Action<T1> _action;
                T1 _t1;
                public Job(Action<T1> action,T1 t1)
                {
                    _action = action;
                    _t1 = t1;
                }
                public override void Execute()
                {
                    _action(_t1);
                }
            }
        
            // 잡큐
            public class JobQueue
            {
                object _lock = new object();
                Queue<IJob> _queue = new Queue<IJob>();
                bool _isProcess = false; // 전담하여 작업 수행하는 스레드가 있는지??
        
                public void Push(Action _action) { Push(new Job(_action)); }
                public void Push<T1>(Action<T1> _action,T1 _t1) { Push(new Job<T1>(_action,_t1)); }
        
                public void Push(IJob job)
                {
                    bool isProcess = false;
                    lock(_lock)
                    {
                        _queue.Enqueue(job);
                        if(_isProcess == false)
                        {
                            isProcess = _isProcess = true;
                        }
                    }
                    if(isProcess)
                    {
                        ProcessJob();
                    }
                }
                public void ProcessJob()
                {
                    lock(_lock)
                    {
                        while(_queue.Count > 0)
                        {
                            IJob job = _queue.Dequeue();
                            job.Execute();
                        }
                        _isProcess = false;
                    }
                }
            }
        </details>
     테스트 결과
      - 첫 번째 방법은 70명까지 안정적인 수용이 가능했다.
  
      *서버 실행 시간 1분
      | 유저 수   | 평균 응답시간| 최대 응답시간 |
      |-----------|--------------|---------------|
      | 50        | 24ms         | 109ms         |
      | 70        |  51ms        | 594ms         |
      | 80        |  8941ms      | 36041ms       |
     
     
    - 두 번째 방법 - Lock 범위를 줄인 JobQueue
      - JObQueue 를 개선할 방법을 생각하다 작업을 넣고 빼는 작업과 작업을 진행하는 부분을 나눴다. lock의 범위를 줄여서 경합을 줄이면 성능 향상이 될 것이라고 생각했다.
      - 공용 작업 큐와 작업 전담 스레드의 개인 작업큐를 만들어 작업을 이동 후 수행했다.
      <details>
      <summary>변경 코드 보기</summary>
   
            Queue<IJob> _PrivateQueue = new Queue<IJob>(); // 새로운 개인 작업큐
            public void ProcessJob()
            {
                while(true)
                {
                     //작업을 개인 큐로 이동
                    lock(_lock)
                    {
                        while(_queue.Count > 0)
                        {
                            _PrivateQueue.Enqueue(_queue.Dequeue());
                        }
                    }
      
                      //작업 수행
                    foreach (var job in _PrivateQueue)
                    {
                        job.Execute();
                    }
                    _PrivateQueue.Clear();
      
                     // 작업을 진행하는 동안 새로운 작업이 있는지 확인
                    lock(_lock)
                    {
                        if (_queue.Count == 0)
                        {
                            _isProcess = false;
                            return;
                        }
                    }
                }
            }
      </details>

      테스트 결과
      - 첫 번째 방법과 유사한 성능
        
       *서버 실행 시간 1분
      | 유저 수   | 평균 응답시간| 최대 응답시간 |
      |-----------|--------------|---------------|
      | 50        | 21ms         | 63ms         |
      | 70        |  39ms        | 531ms         |
      | 80        |  3129ms      | 5594ms       |
      

   - 세 번째 방법
      - 두 번째 방식에서 경합을 줄이는 대신 lock 사용횟수가 늘어서 성능차이가 없었다고 판단하여 lockFree 구조를 이용해보기로 했다.
      - CAS를 이용한 LockFree 구조를 참고했다.
      <details>
      <summary>락프리스택 코드</summary>
         
            public class LockFreeData<T>
            {
                public T data;
                public LockFreeData<T> next = null;
            }
            public class LockFreeStack<T>
            {
                LockFreeData<T> _header;
                public void Push(T t)
                {
                    LockFreeData<T> newHeader = new LockFreeData<T>();
                    newHeader.data = t;
                    newHeader.next = _header;
            
                    while(true)
                    {
                        LockFreeData<T> oldHeader = Interlocked.CompareExchange<LockFreeData<T>>(ref _header, newHeader, newHeader.next);
                        if(oldHeader == newHeader.next)
                        {
                            break;
                        }
                        else
                        {
                            newHeader.next = _header;
                            Thread.Sleep(0);
                        }
                    }
                }
                public LockFreeData<T> Pop()
                {
                    LockFreeData<T> popHeader = _header;
                    while (popHeader != null)
                    {
                        LockFreeData<T> next = popHeader.next;
                        LockFreeData<T> oldHeader = Interlocked.CompareExchange<LockFreeData<T>>(ref _header, next, popHeader);
                        if( oldHeader == popHeader)
                        {
                            return popHeader;
                        }
                        else
                        {
                            popHeader = _header;
                            Thread.Sleep(0);
                        }
                    }
                    return null;
                }
            }
      </details>
  
       테스트 결과
      - 가장 최악이다 유저 수 10명까지 원활하다
      - 락프리 구조는 경합이 많아질수록 효율이 떨어지며 테스트에서 문제를 확인했다.
      
      *서버 실행 시간 1분
      | 유저 수   | 평균 응답시간| 최대 응답시간 |
      |-----------|--------------|-------------|
      | 10        | 15ms         | 47ms        |
      | 20        |  18423ms     | 61406ms     |

        
      

   - 네 번째 방법
      - JobQueue를 개선하기 어려워 다른 방법을 찾았다
      - Broadcast 작업을 100ms 단위로 모아 한 번에 처리하도록 변경했고 이를 통해 Send 호출 횟수를 줄이고 성능 향상을 기대했다.
     
      부하 결과
      - 평균 응답시간이 100ms 목표일 때 400명까지 안정적으로 수용이 가능하다
      - X 만큼의 시간 동안 모을 경우 X가 커질 경우 평균 응답시간이 늘지만 더 많은 유저 수에 대응 가능했고 X가 작아질수록 평균 응답시간은 줄지만 안정성은 줄었다. 
        
      *서버 실행 시간 1분
      | 유저 수   | 평균 응답시간| 최대 응답시간 |
      |-----------|--------------|-------------|
      | 50        | 33ms         | 47ms        |
      | 100       | 53ms         | 141ms      |
      | 150      | 73ms         | 157ms        |
      | 300       | 84ms         | 219ms      |
      | 400     | 101ms         | 265ms      |
     | 500     | 123ms         | 313ms      |
        
---


## 느낀 점  

락프리는 만능이 아니라 경합이 많을 때는 오히려 성능이 저하된다 이는 빈번한 CAS 경합에서 나오는 비용이 더 크기 때문이다.

서버에서 Send 나 Recv 같은 네트워크 I/O 작업은 생각보다 비용이 큰 작업이다. I/O 작업을 줄이거나 최적화할 수 있는 방법을 찾아야 한다

잘 정의된 네이밍 규칙은 중요하다. 팀 혹은 과거의 나와 협업할 때 오류를 줄이고 유지 보수에 도움이 된다.

반복적인 작업을 자동화하는 방법은 오류를 줄이고 개발 시간을 크게 절약할 수 있다.



---