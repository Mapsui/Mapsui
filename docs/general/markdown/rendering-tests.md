# Rendering Tests

Mapsui has rendering tests where images are generated and compared to reference images that are stored as file in the repository. They are regression tests but executed by our unit test framework. The images are compared pixel by pixel. If there are too many differences the test will fail. 

### Fix the code or update the image?
If a test fails the developer needs to decide whether to:
- Accept the changes because the generated image is as intended.
- Change the code because the generated image is not as intended.

To make this decision the developer needs to visually inspect the generated images. This can be done in two ways.

#### Inspect the sample in the app
Every rendering test that can fail corresponds a sample. You can view any sample by running one of the sample apps (we have one for each platform). Even if you have not seen the reference image you can often spot a mistake by looking at the sample, it can be an obvious bug.  

You need to know the name and the category of the sample. For this you need to look at the error of the failed test. Which could be:

```
Failed TestSampleAsync(Mapsui.Tests.Common.Maps.LineSample) [4 s]
```
In this case the sample is in the file `LineSample.cs`. If you open that file you will see it is named `Line` and in category `Tests`.

#### Compare the generated images

The generated images are written to:

    {Mapsui.sln folder}\Tests\Mapsui.Rendering.Skia.Tests\bin\Debug\net9.0\Resources\Images\GeneratedRegression

You will need to compare those to the the original ones that have been copied to this folder as part of the build:

    {Mapsui.sln folder}\Tests\Mapsui.Rendering.Skia.Tests\bin\Debug\net9.0\Resources\Images\OriginalRegression

Such a folder would look like this:

![image](https://user-images.githubusercontent.com/963462/139462183-cf8126ba-8dc5-4c17-b107-11752196dd19.png)



### Update the image
If after code changes there are expected changes in the generated files they should be committed to git so they need to be copied to:

    {Mapsui.sln folder}\Tests\Mapsui.Rendering.Skia.Tests\Resources\Images\OriginalRegression
    
My way of working is like this. I copy all original files over the original files with this command:
```ps
PS> .\Scripts\CopyGeneratedImagesOverOriginalImages.ps1
```
Then there will be many git changes because smaller differences have been accepted by the tests in the past. You need to revert all files that did not cause a test to fail. This is because we want to reduce the number of changes in our git history, especialy if they are binary files.
