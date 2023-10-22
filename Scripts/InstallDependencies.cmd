cd %~dp0
..\
cd Workload
cd Net6.0
dotnet workload install maui
cd ..
cd ..
dotnet workload install maui macos ios android maccatalyst wasm-tools 