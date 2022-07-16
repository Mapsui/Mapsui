# Disable Central Package Management
(Get-Content -path $PSScriptRoot\..\Directory.Packages.props -Raw) -replace 'false','true'  | Set-Content -Path $PSScriptRoot\..\Directory.Packages.props

$fileNames = Get-ChildItem -Path $PSScriptRoot\.. -Recurse -Include *.csproj,Directory.Build.props

foreach ($file in $fileNames) {
    $fileContent = (Get-Content -path $file -Encoding UTF8BOM)
    # Set Version in files
    (Get-Content -path $PSScriptRoot\..\Directory.Packages.props -Raw) | Select-Xml -XPath "/Project/ItemGroup/PackageVersion" | foreach {  
        $include=$_.node.Include
        $version=$_.node.Version

        # Write-Host $include $version
        # Write-Host "Include=""$include"""
        # Write-Host "Include=""$include"" Version=""$version"""
        # (Get-Content -path $PSScriptRoot\..\Directory.Build.props -Raw) -replace "Include=""$include""" ,"Include=""$include"" Version=""$version""" | Set-Content -Path $PSScriptRoot\..\Directory.Build.props
      
        $fileContent = $fileContent -replace "Include=""$include"" Version=""$version""", "Include=""$include"""
    }

    Set-Content -Path $file $fileContent -Encoding UTF8BOM
}