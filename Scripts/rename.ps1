# Thanks a lot to Damien Hickey	
# http://dhickey.ie/post/2011/06/03/Rename-a-Visual-Studio-Project-using-PowerShell.aspx

    # designed to run from the src folder
    param(
        [string]$projectName=$(throw "projectName required."),
        [string]$newProjectName=$(throw "newProjectName required.")
    )
     
    if(!(Test-Path $projectName)){
        Write-Error "No project folder '$projectName' found"
        return
    }
     
    if(!(Test-Path $projectName\$projectName.csproj)){
        Write-Error "No project '$projectName\$projectName.dll' found"
        return
    }
     
    if((Test-Path $newProjectName)){
        Write-Error "Project '$newProjectName' already exists"
        return
    }
     
    # project
    hg rename $projectName\$projectName.csproj $projectName\$newProjectName.csproj
     
    # folder
    hg rename $projectName $newProjectName
     
    # assembly title
    $assemblyInfoPath = "$newProjectName\Properties\AssemblyInfo.cs"
    (gc $assemblyInfoPath) -replace """$projectName""","""$newProjectName""" | sc $assemblyInfoPath
     
    # root namespace
    $projectFile = "$newProjectName\$newProjectName.csproj"
    (gc $projectFile) -replace "<RootNamespace>$projectName</RootNamespace>","<RootNamespace>$newProjectName</RootNamespace>" | sc $projectFile
     
    # assembly name
    (gc $projectFile) -replace "<AssemblyName>$projectName</AssemblyName>","<AssemblyName>$newProjectName</AssemblyName>" | sc $projectFile
     
    # other project references
    gci -Recurse -Include *.csproj |% { (gc $_) -replace "..\\$projectName\\$projectName.csproj", "..\$newProjectName\$newProjectName.csproj" | sc $_ }
    gci -Recurse -Include *.csproj |% { (gc $_) -replace "<Name>$projectName</Name>", "<Name>$newProjectName</Name>" | sc $_ }
     
    # solution 
    gci -Recurse -Include *.sln |% { (gc $_) -replace "\""$projectName\""", """$newProjectName""" | sc $_ }
    gci -Recurse -Include *.sln |% { (gc $_) -replace "\""$projectName\\$projectName.csproj\""", """$newProjectName\$newProjectName.csproj""" | sc $_ }
