// File: Services/PptxExportService.cs
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;
using PresenterApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.Services
{
    public class PptxExportService
    {
        public async Task ExportToPptx(List<SlideDefinition> slideDefinitions, string filePath)
        {
            // Mở template từ MauiAsset
            using var templateStream = await FileSystem.OpenAppPackageFileAsync("template.pptx");
            using var memoryStream = new MemoryStream();
            await templateStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (PresentationDocument presentationDocument = PresentationDocument.Open(memoryStream, true))
            {
                PresentationPart presentationPart = presentationDocument.PresentationPart;

                // Lấy slide mẫu (slide đầu tiên trong template)
                SlidePart templateSlidePart = presentationPart.SlideParts.FirstOrDefault();
                if (templateSlidePart == null)
                {
                    // Xử lý lỗi nếu template không có slide nào
                    return;
                }

                // Tạo các slide mới bằng cách nhân bản slide mẫu
                foreach (var slideDef in slideDefinitions)
                {
                    // Nhân bản slide mẫu
                    SlidePart newSlidePart = CloneSlide(presentationPart, templateSlidePart);

                    // Điền nội dung vào slide mới
                    PopulateSlide(newSlidePart, slideDef);
                }

                // Xóa slide mẫu ban đầu
                var slideIdList = presentationPart.Presentation.SlideIdList;
                var slideIdToRemove = slideIdList.ChildElements.OfType<SlideId>().FirstOrDefault();
                if (slideIdToRemove != null)
                {
                    string relationshipId = slideIdToRemove.RelationshipId;
                    slideIdList.RemoveChild(slideIdToRemove);
                    presentationPart.DeletePart(relationshipId);
                }

                presentationPart.Presentation.Save();
            }

            // Lưu MemoryStream vào file cuối cùng
            memoryStream.Position = 0;
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await memoryStream.CopyToAsync(fileStream);
        }

        private static SlidePart CloneSlide(PresentationPart presentationPart, SlidePart templatePart)
        {
            var newSlidePart = presentationPart.AddNewPart<SlidePart>();
            newSlidePart.FeedData(templatePart.GetStream(FileMode.Open));
            newSlidePart.AddPart(templatePart.SlideLayoutPart);

            // Thêm slide mới vào danh sách slide của bài trình bày
            SlideIdList slideIdList = presentationPart.Presentation.SlideIdList;
            uint maxSlideId = slideIdList.ChildElements.OfType<SlideId>().Max(s => s.Id.Value);

            SlideId newSlideId = new SlideId
            {
                Id = maxSlideId + 1,
                RelationshipId = presentationPart.GetIdOfPart(newSlidePart)
            };
            slideIdList.Append(newSlideId);

            return newSlidePart;
        }

        private static void PopulateSlide(SlidePart slidePart, SlideDefinition slideDef)
        {
            if (slidePart?.Slide?.CommonSlideData?.ShapeTree == null)
            {
                return;
            }
            var shapeTree = slidePart.Slide.CommonSlideData.ShapeTree;

            // Tìm placeholder cho tiêu đề và nội dung
            var titleShape = shapeTree.Elements<Shape>().FirstOrDefault(s => IsPlaceholderOfType(s, PlaceholderValues.Title));
            var bodyShape = shapeTree.Elements<Shape>().FirstOrDefault(s => IsPlaceholderOfType(s, PlaceholderValues.Body));

            if (titleShape != null)
            {
                var textBody = titleShape.TextBody;
                textBody.Descendants<D.Text>().First().Text = slideDef.Title ?? string.Empty;
            }

            if (bodyShape != null)
            {
                var textBody = bodyShape.TextBody;
                // Xóa các paragraph cũ
                textBody.RemoveAllChildren<D.Paragraph>();

                foreach (var line in slideDef.Lines)
                {
                    var paragraph = new D.Paragraph();
                    var run = new D.Run();
                    var runProperties = new D.RunProperties { FontSize = slideDef.FontSize * 100, Bold = slideDef.IsBold };
                    var text = new D.Text { Text = line };
                    run.Append(runProperties);
                    run.Append(text);
                    paragraph.Append(run);
                    textBody.Append(paragraph);
                }
            }
        }

        private static bool IsPlaceholderOfType(Shape shape, PlaceholderValues type)
        {
            var placeholderShape = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.GetFirstChild<PlaceholderShape>();
            return placeholderShape?.Type != null && placeholderShape.Type.Value == type;
        }
    }
}