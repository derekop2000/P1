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