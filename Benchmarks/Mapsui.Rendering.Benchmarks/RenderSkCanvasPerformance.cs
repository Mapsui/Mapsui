using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using ExCSS;
using SkiaSharp;

namespace Mapsui.Rendering.Benchmarks;

[SimpleJob(RunStrategy.Throughput)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class RenderSkCanvasPerformance : IDisposable
{           
    private readonly SKPaint _paint;
    private readonly SKPath _path;
    private readonly SKImageInfo _info;
    private readonly SKBitmap _bitmap;
    private readonly SKCanvas _canvas;
    private readonly SKPath _pathOffCanvas;
    private readonly SKMatrix _rotationMatrix;
    private readonly SKMatrix _reverseRotationMatrix;
    private readonly SKImageInfo _infoTransformed;
    private readonly SKBitmap _bitmapTransformed;
    private readonly SKCanvas _canvasTransformed;
    private readonly SKMatrix _scaleMatrix;
    private readonly SKMatrix _translationMatrix;
    private readonly SKMatrix _translationScaleMatrix;
    private readonly SKPath _pathLessPoints;
    private bool _disposed;

    public RenderSkCanvasPerformance()
    {
        _info = new SKImageInfo(1000, 1000)
        {
            AlphaType = SKAlphaType.Premul
        };

        _bitmap = new SKBitmap(_info);
        _canvas = new SKCanvas(_bitmap);
        
        _infoTransformed = new SKImageInfo(1000, 1000)
        {
            AlphaType = SKAlphaType.Premul
        };

        _bitmapTransformed = new SKBitmap(_infoTransformed);
        _canvasTransformed = new SKCanvas(_bitmapTransformed);

        _paint = new SKPaint
        {
            Color = SKColors.Green,
        };

        _path = new SKPath();
        for (int i = 0; i < 1000; i++)
        {
            _path.MoveTo(i, i);
        }
        
        _pathOffCanvas = new SKPath();
        for (int i = -5000; i < 6000; i++)
        {
            _pathOffCanvas.MoveTo(i, i);
        }
        
        _pathLessPoints = new SKPath();
        for (int i = 0; i < 1000; i += 10)
        {
            _pathLessPoints.MoveTo(i, i);
        }

        _scaleMatrix = SKMatrix.CreateScale(10, 10);
        _translationMatrix = SKMatrix.CreateTranslation(1000, 1000);
        _translationScaleMatrix = SKMatrix.CreateScaleTranslation(10, 10, 100, 100);
        _rotationMatrix = SKMatrix.CreateRotation(175);
        _canvasTransformed.SetMatrix(_rotationMatrix);
        _reverseRotationMatrix = _rotationMatrix.Invert();
    }

    [Benchmark]
    public void RenderDirect()
    {
        _canvas.DrawPath(_path, _paint);
    }
    
    [Benchmark]
    public void RenderDirectLessPoints()
    {
        _canvas.DrawPath(_pathLessPoints, _paint);
    }
    
    [Benchmark]
    public void RenderRotationCanvasEveryTime()
    {
        _canvas.SetMatrix(_rotationMatrix);
        _canvas.DrawPath(_path, _paint);
        _canvas.ResetMatrix();
    }
    
    [Benchmark]
    public void RenderScaleCanvasEveryTime()
    {
        _canvas.SetMatrix(_scaleMatrix);
        _canvas.DrawPath(_path, _paint);
        _canvas.ResetMatrix();
    }
    
    [Benchmark]
    public void RenderTranslationCanvasEveryTime()
    {
        _canvas.SetMatrix(_translationMatrix);
        _canvas.DrawPath(_path, _paint);
        _canvas.ResetMatrix();
    }
    
    [Benchmark]
    public void RenderTranslationScaleCanvasEveryTime()
    {
        _canvas.SetMatrix(_translationScaleMatrix);
        _canvas.DrawPath(_path, _paint);
        _canvas.ResetMatrix();
    }
    
    [Benchmark]
    public void RenderRotationCanvasOnce()
    {
        _canvasTransformed.DrawPath(_path, _paint);
    }
    
    [Benchmark]
    public void RenderRotationPath()
    {
        _path.Transform(_rotationMatrix);
        _canvas.DrawPath(_path, _paint);
        _path.Transform(_reverseRotationMatrix);
    }
    
    [Benchmark]
    public void RenderOffCanvas()
    {
        _canvas.DrawPath(_pathOffCanvas, _paint);
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _paint.Dispose();
        _path.Dispose();
        _bitmap.Dispose();
        _canvas.Dispose();
        _pathOffCanvas.Dispose();
        _bitmapTransformed.Dispose();
        _canvasTransformed.Dispose();
        _pathLessPoints.Dispose();

        _disposed = true;
    }
}
