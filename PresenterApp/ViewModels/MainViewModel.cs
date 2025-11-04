// File: ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace PresenterApp.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        ObservableCollection<BookType> bookTypes = new();

        [ObservableProperty]
        ObservableCollection<Book> books = new();

        [ObservableProperty]
        ObservableCollection<Tag> tags = new();

        [ObservableProperty]
        bool isNavMenuVisible = true; // Bắt đầu với menu mở

        [ObservableProperty]
        BaseViewModel? currentEditViewModel; // ViewModel cho cột 2

        // Cần inject các ViewModel con qua DI
        // (Đây là một cấu trúc DI đơn giản hóa)
        public MainViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            Title = "Quản lý Sách";
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                BookTypes.Clear();
                var bts = await _dataAccess.GetBookTypesAsync();
                foreach (var bt in bts) BookTypes.Add(bt);

                Books.Clear();
                var bks = await _dataAccess.GetBooksAsync();
                foreach (var bk in bks) Books.Add(bk);

                Tags.Clear();
                var tgs = await _dataAccess.GetTagsAsync();
                foreach (var tg in tgs) Tags.Add(tg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi không tải được dữ liệu: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void ToggleNavMenu() => IsNavMenuVisible = !IsNavMenuVisible;

        [RelayCommand]
        void ShowEditPage(object item)
        {
            // Logic để hiển thị trang chỉnh sửa ở Cột 2
            // Trong một ứng dụng thực tế, bạn sẽ dùng DI để tạo các VM này
            Debug.WriteLine($"Hiển thị trang chỉnh sửa cho: {item.GetType().Name}");
            // if (item is BookType bt)
            //    CurrentEditViewModel = new EditBookTypeViewModel(bt, _dataAccess);
            // else if (item is Book b)
            //    CurrentEditViewModel = new EditBookViewModel(b, _dataAccess, ...);
            // else if (item is Tag t)
            //    CurrentEditViewModel = new EditTagViewModel(t, _dataAccess);
        }

        [RelayCommand]
        void AddNew(string type)
        {
            // Logic để hiển thị trang thêm mới ở Cột 2
            Debug.WriteLine($"Thêm mới: {type}");
            // if (type == "BookType")
            //    CurrentEditViewModel = new EditBookTypeViewModel(new BookType(), _dataAccess);
            // else if (type == "Book")
            //    CurrentEditViewModel = new EditBookViewModel(new Book(), _dataAccess, ...);
            // else if (type == "Tag")
            //    CurrentEditViewModel = new EditTagViewModel(new Tag(), _dataAccess);
        }
    }
}