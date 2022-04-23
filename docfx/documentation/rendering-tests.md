# Rendering Tests

Mapsui has a way of testing rendering where a Map state is rendered to a bitmap. This bitmap is then compared to the original images which are stored as resource in the repository. This is thus a kind of regresssion test (1). If there are differences between these images the test will fail. If this is the case the developer needs to visually inspect the generated images (2). If the generated image is as expected and the original is not then the original needs to be overwritten. 

1. Regression tests 
2. Visual inspection.

The regression tests are useful during refactoring when you expect no changes. When you are working on changes in the rendered output the regression tests will fail but the visual inspection of the output is still useful to check if this is as intended.

## 1. Regression tests 

The generated images are written to:

    {test project folder}\bin\Debug\net6.0\Resources\Images\Generated

Those will be compared to the original images that were deployed in the build located here:

    {test project folder}\bin\Debug\net6.0\Resources\Images\Original\

If after code changes there are expected changes in the generated files they should be committed to git so they need to be copied to:

    {test project folder}\Resources\Images\Original\
    
This can be done with a script: ```scripts\test-image-copier.cmd```. 

## 2. Visual inspection

There tests can be inspected in two ways. 
1. In the WPF sample there is a 'Tests' category, that shows an interactive version of the test sample.
2. In the output folder (see above) the generated images can be viewed. Currently this folder looks like this:

![image](https://user-images.githubusercontent.com/963462/139462183-cf8126ba-8dc5-4c17-b107-11752196dd19.png)



