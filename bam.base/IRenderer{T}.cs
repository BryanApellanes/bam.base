namespace Bam
{
    public interface IRenderer<T> : IRenderer
    {
        string Render(T toRender);
    }
}
