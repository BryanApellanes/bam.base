namespace Bam
{
    public interface ITemplateRenderer : IRenderer
    {
        new string Render(string templateName, object? toRender);
        void Render(object? toRender, Stream output);
        void Render(string templateName, object? toRender, Stream output);
    }
}