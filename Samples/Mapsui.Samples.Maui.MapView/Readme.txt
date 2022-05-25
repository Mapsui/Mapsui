This currently only works on Windows Visual Studio 2022 17.1.

Following things need to be installed.

1. https://dotnet.microsoft.com/en-us/download
   download dotnet 6.0.200.
2. ios Workload
   In Administrative Console run folowwing program.
   dotnet workload install maui
3. Install Single-project MSIX Packaging Tools for VS 2022
   https://marketplace.visualstudio.com/items?itemName=ProjectReunion.MicrosoftSingleProjectMSIXPackagingToolsDev17

How to install a specific dotnet Workload:
  1. download the Manifest with nuget
     https://www.nuget.org/packages/Microsoft.NET.Sdk.Maui.Manifest-6.0.200/6.0.200-preview.13.2865
  2. install the manifest into the sdk-manifest folder
     C:\Program Files\dotnet\sdk-manifests\6.0.200\microsoft.net.sdk.maui
  3. Install maui
     dotnet workload install maui --skip-manifest-update 