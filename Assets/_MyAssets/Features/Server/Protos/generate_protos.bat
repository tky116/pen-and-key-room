@echo off
setlocal

set PROTOC=..\..\..\..\..\Packages\Grpc.Tools.2.68.1\tools\windows_x64\protoc.exe
set PLUGIN=..\..\..\..\..\Packages\Grpc.Tools.2.68.1\tools\windows_x64\grpc_csharp_plugin.exe
set PROTO_PATH=.
set OUTPUT_PATH=..\Generated

if not exist "..\Generated" mkdir "..\Generated"

"%PROTOC%" --proto_path="%PROTO_PATH%" --csharp_out="%OUTPUT_PATH%" --grpc_out="%OUTPUT_PATH%" --plugin=protoc-gen-grpc="%PLUGIN%" drawing.proto

echo Protocol buffer files generated successfully.
pause