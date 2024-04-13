# Documentation

## Documentation setup
- We use mkdocs to generate the general documentation.
- This 'docs/general' folder contains all the md files used to generate the documenation website. This is the source of those files, they should be edited there.
- We use docfx to generate the api documentation. 
- The './docs/api/docfx.json' file defines how the api docs are built. You can test it locally by running `dockfx docfx.json --serve`
- In the powershell script ./Scripts/BuildDocumentationWebSite.ps1 generates the documentation web site, which is generated in the /docfx/mapsui/_site folder, and copies it to the /docs folder.
- The Mapsui project on github is configured to automatically publish this docs folder to https://mapsui.github.io/Mapsui
- A commit of an md file should trigger the build server. This should run the build-site.cmd. This should commit the generated site to the repo. It will when then show up on the website. We should have two separate build configurations one for the docs which ignores the project and one for the project which ignores the docs.

## Documentation guidelines
- All md files should be in lower case.
- All md files should be directly in the root of folder. Hierachy is created by indenting page references in the toc.md in the documenation folder. By keeping the files itself in the root it is easier to change the hierarchy later on - you do not need to move the files as well - and easier to get contributers to follow the guidelines.
- All md files should start with a header one (#) and should have no other header one in that file.
- All the headers in the toc should be equal to the header one in the file it points to.
- Mapsui is cased as Mapsui not MapsUI.
- We should iteratively improve the documentation. If questions are asked in the issues we should search for the answer in the documentation. Update the documentation when it is missing and answer the issue with a url to the documentation.
- Writing documentation is not only useful to inform the user but also as a sanity check for the developer. If what you have to tell becomes complicated and hard to grasp this could mean the software is not well designed. Writing documentation early should be used as a part of the software development process.
