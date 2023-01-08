# Disable Central Package Management
$Packages = (Get-Content -raw -path $PSScriptRoot\..\Directory.Packages.props -Encoding UTF8)
$fileNames = Get-ChildItem -Path $PSScriptRoot\.. -Recurse -Include *.csproj,Directory.Build.props

foreach ($file in $fileNames) {
    $fileContent = (Get-Content -path $file -Encoding UTF8)
    # Set Version in files
    $Packages | Select-Xml -XPath "//PackageVersion" | foreach {  
        $include=$_.node.Include
        $version=$_.node.Version

        # Write-Host $include $version
        # Write-Host "Include=""$include"""
        # Write-Host "Include=""$include"" Version=""$version"""
        # (Get-Content -path $PSScriptRoot\..\Directory.Build.props -Raw) -replace "Include=""$include""" ,"Include=""$include"" Version=""$version""" | Set-Content -Path $PSScriptRoot\..\Directory.Build.props
      
        $fileContent = $fileContent -replace "Include=""$include"" Version=""$version""", "Include=""$include"""
    }

    # Normalize to one Cariage Return at the end
    $fileContent = $fileContent -replace "</Project>`r`n`r`n", "</Project>`r`n"

    Set-Content -Path $file $fileContent -Encoding UTF8
}

$Packages = $Packages -replace 'false','true' 
# Normalize to one Cariage Return at the end
$Packages = $Packages -replace "</Project>`r`n", "</Project>"

Set-Content -Path $PSScriptRoot\..\Directory.Packages.props $Packages -Encoding UTF8