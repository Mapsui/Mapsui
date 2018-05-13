# Documentation

# Documentation setup
- We use docfx to generate the documentation. 
- There is a /docfx folder with a docfx project called Mapsui. 
- This projects documenation folder contains all the md files used to generate the 'documentation' tab in the site. This the the source of those files they should be edited there.
- In the docfx folder there is a script (generate-docs.cmd) that generates the documentation site (in /docfx/mapsui/_site) and copies it to the /docs folder.
- The Mapsui project on github is configures to automatically publish this docs folder to https://mapsui.github.io/mapsui
- A commit of an md file should trigger the build server. This should run the generate-docs.cmd. This should commit the generated site to the repo (this is not the case right now, 13 may 18). It when then show up on the website.

# Documentation Guidelines
- All md files should be in lower case (they are not right now).
- All md files should be directly in the root of /docfx. Hierachy is created with the toc.md in the documenation folder. By keeping the files itself in the root it is easier to change the hierarchy and easier to get contributers to follow the guidelines.
- All md files should start with a header one (#) and have only header one.
- All the headers in the toc should be equal to the header one in the file it points to.
- Mapsui is cased as Mapsui not MapsUI.

