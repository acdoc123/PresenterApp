// File: Views/ContentBlockTemplateSelector.cs
using PresenterApp.Models;

namespace PresenterApp.Views
{
    public class ContentBlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NamedTextBlockTemplate { get; set; }
        public DataTemplate ImageBlockTemplate { get; set; }
        public DataTemplate PdfBlockTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not FlexibleContentBlock block)
            {
                return null;
            }

            switch (block.Type)
            {
                case ContentBlockType.Image:
                    return ImageBlockTemplate;
                case ContentBlockType.Pdf:
                    return PdfBlockTemplate;
                case ContentBlockType.NamedText:
                default:
                    return NamedTextBlockTemplate;
            }
        }
    }
}