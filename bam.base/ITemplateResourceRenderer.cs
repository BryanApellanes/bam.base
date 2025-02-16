namespace Bam
{
    public interface ITemplateResourceRenderer: ITemplateRenderer
    {
        void RenderResource(string templateName, object toRender, Stream output);
    }
}
