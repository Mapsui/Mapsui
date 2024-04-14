# Documentation

## Documentation setup
- We use *mkdocs* to generate the **general documentation**. The main configuration is in `./docs/general/mkdocs.yml`. You can test it locally by running `mkdocs serve` in that folder. The content is in the .md files in the `./docs/general/markdown` folder. To update the documentation those files need to be edited. The documentation web site has a link back to these files on github to make it easy to update them. The general docs are published to the `./docs/general/_site` folder.
- We use *docfx* to generate the **api documentation**. The main configuration is in `./docs/api/docfx.json`. You can test it locally by running `docfx --serve` in that folder. The api docs are published to the `./docs/api/_site` folder.
- The powershell script `./Scripts/BuildDocumentationWebSite.ps1` runs both mkdocs and docfx and copies both to the `./website` folder.
- The Mapsui project on github is configured to automatically publish the `./website` folder to github pages: https://mapsui.github.io/Mapsui, configured for the mapsui.com domain: https://mapsui.com.
- A pushed commit of a markdown file triggers the `dotnet-docs.yml` github action. It will run the scripts and automatically publish the new version to the website. 

## Documentation setup guidelines
- All md files should be in lower case.
- All md files should be directly in the `./docs/general/markdown` folder, not in a subdirectory. Hierachy is created by indenting page references in the toc.md in the documenation folder. By keeping the files in the root it is easier to change the hierarchy later on - you do not need to move the files as well - and it is easier to stick to the guidelines.
- All md files should start with a header one (#) and should have no other header one in the file.
- All the headers in the toc should be equal to the header one in the file it points to.

## Documentation guidelines
- Mapsui is cased as Mapsui not MapsUI.
- We should iteratively improve the documentation. If questions are asked in the issues we should search for the answer in the documentation. When something is missing in the documentation update the documentation and answer the question with a url to the documentation.
- Writing documentation is not only useful to inform the user but also as a sanity check for the developer. If what you have to tell becomes complicated and hard to grasp this could mean the software is not well designed. Writing documentation early should be used as a part of the software development process.
