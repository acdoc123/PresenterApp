// File: Views/AttributeTemplateSelector.cs
using PresenterApp.Models;
using PresenterApp.ViewModels;

namespace PresenterApp.Views
{
    public class AttributeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate TextAreaTemplate { get; set; }
        public DataTemplate NumberTemplate { get; set; }
        // Các template khác cho Image, Pdf... có thể được thêm vào sau

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var attributeVM = (DynamicAttributeViewModel)item;

            switch (attributeVM.FieldType)
            {
                case FieldType.TextArea:
                    return TextAreaTemplate;
                case FieldType.Number:
                    return NumberTemplate;
                case FieldType.Text:
                default:
                    return TextTemplate;
            }
        }
    }
}