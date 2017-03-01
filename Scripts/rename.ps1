# Thanks a lot to Damian Hickey	
# http://dhickey.ie/post/2011/06/03/Rename-a-Visual-Studio-Project-using-PowerShell.aspx

    # designed to run from the src folder
    param(
        [string]$projectName=$(throw "projectName required."),
        [string]$newProjectName=$(throw "newProjectName required."),
		[string]$folder=$(throw "folder required.")
    )

    if(!(Test-Path $folder)){
        Write-Error "No folder '$folder' found"
        return
    }	
	
    if(!(Test-Path  $folder\$projectName)){
        Write-Error "No project folder '$folder\$projectName' found"
        return
    }
	
    if(!(Test-Path $folder\$projectName\$projectName.csproj)){
        Write-Error "No project '$folder\$projectName\$projectName.dll' found"
        return
    }
    
	if((Test-Path $folder\$newProjectName)){
        Write-Error "Project '$folder\$newProjectName' already exists"
        return
    }
	

     
    # project
    hg rename $folder\$projectName\$projectName.csproj $folder\$projectName\$newProjectName.csproj
     
    # folder
    hg rename $folder\$projectName $folder\$newProjectName
     
    # assembly title
    $assemblyInfoPath = "$folder\$newProjectName\Properties\AssemblyInfo.cs"
    (gc $assemblyInfoPath) -replace """$projectName""","""$newProjectName""" | sc $assemblyInfoPath

    # root namespace
    $projectFile = "$folder\$newProjectName\$newProjectName.csproj"
    (gc $projectFile) -replace "<RootNamespace>$projectName</RootNamespace>","<RootNamespace>$newProjectName</RootNamespace>" | sc $projectFile
     
    # assembly name
    (gc $projectFile) -replace "<AssemblyName>$projectName</AssemblyName>","<AssemblyName>$newProjectName</AssemblyName>" | sc $projectFile
     
    # other project references
    gci -Recurse -Include *.csproj |% { (gc $_) -replace "..\\$folder\\$projectName\\$projectName.csproj", "..\$folder\$newProjectName\$newProjectName.csproj" | sc $_ }
    gci -Recurse -Include *.csproj |% { (gc $_) -replace "<Name>$projectName</Name>", "<Name>$newProjectName</Name>" | sc $_ }
     
    # solution 
    gci -Recurse -Include *.sln |% { (gc $_) -replace "\""$projectName\""", """$newProjectName""" | sc $_ }
    gci -Recurse -Include *.sln |% { (gc $_) -replace "\""$folder\\$projectName\\$projectName.csproj\""", """$folder\$newProjectName\$newProjectName.csproj""" | sc $_ }
