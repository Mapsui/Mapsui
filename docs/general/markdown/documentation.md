---
title: Documentation
description: How the Mapsui documentation is structured, built with MkDocs and DocFX, and automatically published to mapsui.com via GitHub Actions.
---

# Documentation

## Documentation setup
- We use *mkdocs* to generate the **general documentation**. The main configuration is in `./docs/general/mkdocs.yml`. You can test it locally by running `mkdocs serve` in that folder, which serves on http://127.0.0.1:9756. The port is set with `dev_addr` in `mkdocs.yml` instead of the mkdocs default of 8000, which often collides with another local server. The content is in the .md files in the `./docs/general/markdown` folder. To update the documentation those files need to be edited. The documentation web site has a link back to these files on github to make it easy to update them. The general docs are published to the `./docs/general/_site` folder.
- We use *docfx* to generate the **api documentation**. The main configuration is in `./docs/api/docfx.json`. You can test it locally by running `docfx --serve --port 9757` in that folder. Without `--port` docfx serves on 8080, which often collides with another local server. The api docs are published to the `./docs/api/_site` folder.
- The powershell script `./Scripts/BuildDocumentationWebSite.ps1` runs both mkdocs and docfx and copies both to the `./website` folder.
- The Mapsui project on GitHub is configured to publish the `./website` folder to GitHub Pages at https://mapsui.com.
- A documentation pull request merged into `main` triggers the `dotnet-docs.yml` GitHub Action. It builds the v5 documentation and samples, adds the frozen v4 website, and deploys one complete GitHub Pages artifact. Direct pushes do not deploy the website.
- The v4 website is not rebuilt. It is the immutable release asset described by `.github/pages-v4-snapshot.json`. `Scripts/AddFrozenV4Website.ps1` verifies the asset checksum before adding it to `./website/v4`.

## Documentation setup guidelines
- All md files should be in lower case.
- All md files should be directly in the `./docs/general/markdown` folder, not in a subdirectory. Hierarchy is created by indenting page references in the toc.md in the documentation folder. By keeping the files in the root it is easier to change the hierarchy later on - you do not need to move the files as well - and it is easier to stick to the guidelines.
- All md files should start with a header one (#) and should have no other header one in the file.
- All the headers in the toc should be equal to the header one in the file it points to.

## Documentation guidelines
- Mapsui is cased as Mapsui not MapsUI.
- We should iteratively improve the documentation. If questions are asked in the issues we should search for the answer in the documentation. When something is missing in the documentation update the documentation and answer the question with a url to the documentation.
- Writing documentation is not only useful to inform the user but also as a sanity check for the developer. If what you have to tell becomes complicated and hard to grasp this could mean the software is not well designed. Writing documentation early should be used as a part of the software development process.
