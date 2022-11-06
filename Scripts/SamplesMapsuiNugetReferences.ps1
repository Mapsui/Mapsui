$Packages = (Get-Content -raw -path $PSScriptRoot\..\Directory.Packages.props -Encoding UTF8)
$fileNames = Get-ChildItem -Path $PSScriptRoot\..\Samples, $PSScriptRoot\..\Tests -Recurse -Include *.csproj

foreach ($file in $fileNames) {
    $fileContent = (Get-Content -raw -path $file -Encoding UTF8)
    # Set Version in files
    $Packages | Select-Xml -XPath "//PackageVersion" | ForEach-Object {  
        if ($_.node.Include.StartsWith("Mapsui")) {                    
            $include=$_.node.Include
            $project = $include + ".csproj"
            $includeui = $include -replace "Mapsui.", "Mapsui.UI."
            $projectui = $includeui + ".csproj"
        
            # <ProjectReference .... />     
            $fileContent = $fileContent -replace "<ProjectReference Include=`"`[\.\\a-zA-Z]*$project`"` />", "<PackageReference Include=""$include"" />"
            $fileContent = $fileContent -replace "<ProjectReference Include=`"`[\.\\a-zA-Z]*$projectui`"` />", "<PackageReference Include=""$include"" />"       

            # <ProjectReference .... >...</ProjectReference'
            $fileContent = $fileContent -replace "<ProjectReference Include=`"`[\.\\a-zA-Z]*$project`"`>[`r`n <>{}/\.\-a-zA-Z0-9]*</ProjectReference>", "<PackageReference Include=""$include"" />"
            $fileContent = $fileContent -replace "<ProjectReference Include=`"`[\.\\a-zA-Z]*$projectui`"`>[`r`n <>{}/\.\-a-zA-Z0-9]*</ProjectReference>", "<PackageReference Include=""$include"" />"       
        }
    }
       
    # Normalize to no Cariage Return at the End
    $fileContent = $fileContent -replace "</Project>`r`n", "</Project>"
    Set-Content -Path $file $fileContent -Encoding UTF8
}