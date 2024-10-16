﻿using System.IO;

namespace Bam
{
    public interface ITemplateRenderer : IRenderer
    {
        string Render(string templateName, object? toRender);
        void Render(object? toRender, Stream output);
        void Render(string templateName, object? toRender, Stream output);
    }
}