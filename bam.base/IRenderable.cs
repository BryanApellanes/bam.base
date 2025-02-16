namespace Bam
{
    public interface IRenderable
    {
        string Render();
        void Render(Stream output);
        void Render(ITemplateRenderer renderer);
        void Render(ITemplateRenderer renderer, string templateName, Stream output);
    }
}
