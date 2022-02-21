This currently only works on MacOS.

Following things need to be installed.

1. https://dotnet.microsoft.com/en-us/download
   download dotnet 6.0.200.
2. ios Workload
   In Administrative Console run folowwing program.
   dotnet workload install maui
3. Install Maui-check
   dotnet tool install -g Redth.Net.Maui.Check
4. Run Maui-check dont run dotnet and workload checks
   maui-check --fix --non-interactive --skip dotnetworkloads-6.0.100 --skip dotnet --skip dotnet-workload-dedup --skip dotnetsentinel
