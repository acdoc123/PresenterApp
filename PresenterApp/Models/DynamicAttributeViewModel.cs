// File: Models/DynamicAttributeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Models;

namespace PresenterApp.ViewModels
{
    // Đây là ViewModel cho một thuộc tính động trên giao diện
    public partial class DynamicAttributeViewModel : ObservableObject
    {
        public AttributeDefinition Definition { get; set; }
        public AttributeValue Value { get; set; }

        public string DisplayName => Definition.Name;
        public FieldType FieldType => Definition.Type;

        // Đây là thuộc tính chúng ta bind (liên kết) với Entry/Editor
        public string StringValue
        {
            get => Value.Value;
            set => SetProperty(Value.Value, value, Value, (v, val) => v.Value = val);
        }

        public DynamicAttributeViewModel(AttributeDefinition definition, AttributeValue value)
        {
            Definition = definition;
            Value = value;
        }
    }
}