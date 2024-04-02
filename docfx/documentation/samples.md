# Samples

After going through the guickstart you should look into the samples. All samples work the same on all platforms. You can view online samples presented in Blazor [here](https://mapsui.com/samples/). Each sample has an accompaning 'source code' tab you can use to build your own version. 

## Download samples if you have to
Currently the source code of the samples does not always contain all needed code. In that case you may need to [clone the project](https://github.com/mapsui/mapsui) and open the .slnf of your favorite UI framework. Then set Mapsui.Samples.'YourFavorityUIFramework'.csproj as startup project. If you run any of the samplea you will see there is a way to selected a category and within that category several specific samples. All these samples correspond to a specific sample class that is derived from ISample (or IMapControlSample). The most easy way to find them is to search for the name of the sample that is displayed including quotes. For instance, searching for `"Points"` will lead you to the file PointsSample.cs. 
