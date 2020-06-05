# Unit Tests for Rendering

Testing the renderers in done in two ways.

1. Unit tests that compare the current output to previous output.
2. Visual inspection.

Unit tests are useful during refactoring when you expect no changes. Visual tests are useful when you are working on a change in the rendering output.

## 1. Unit tests 
The generated images are written to:

    {test project folder}\bin\Debug\Resources\Images\Generated\

Those will be compared to the original images in:

    {test project folder}\bin\Debug\Resources\Images\Original\

If after code changes there are expected changes in the generated files they should be committed to git so they need to be copied to:

    {test project folder}\Resources\Images\Original\

## 2. Visual inspection
In the WPF sample there is an option to select the list of unit tests. Both WPF and Skia renderers can be selected. We assume the skia renderer on other platforms are identical.

