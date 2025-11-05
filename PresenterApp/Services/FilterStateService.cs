// File: Services/FilterStateService.cs
using PresenterApp.Models;
using System.Collections.Generic;

namespace PresenterApp.Services
{
    /// <summary>
    /// Dịch vụ Singleton để giữ trạng thái của bộ lọc Tags trên trang chủ.
    /// </summary>
    public class FilterStateService
    {
        public List<Tag> SelectedFilterTags { get; set; } = new List<Tag>();
    }
}