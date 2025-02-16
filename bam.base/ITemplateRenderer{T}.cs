namespace Bam
{
    public interface ITemplateRenderer<T>: ITemplateRenderer, IRenderer<T>
    {
        string Render(string templateName, T? toRender);
        void Render(T? toRender, Stream output);
        void Render(string templateName, T? toRender, Stream output);
    }
}
