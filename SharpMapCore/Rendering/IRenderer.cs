
namespace SharpMap.Rendering
{
    public interface IRenderer
    {
        void Render(IView view, LayerCollection layers);
    }
}
