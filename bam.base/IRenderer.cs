namespace Bam
{
    public interface IRenderer
    {
        string Render(object toRender);
        string Render(string templateName, object? toRender);
    }
}
