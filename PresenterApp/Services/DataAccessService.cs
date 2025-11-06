// File: Services/DataAccessService.cs
using PresenterApp.Models;
using SQLite;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PresenterApp.Services
{
    public class DataAccessService
    {
        private SQLiteAsyncConnection? _database;

        public DataAccessService()
        {
        }

        private async Task Init()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Library.db3");
            _database = new SQLiteAsyncConnection(databasePath);

            // Tạo các bảng mới
            await _database.CreateTableAsync<BookType>();
            await _database.CreateTableAsync<Book>();
            await _database.CreateTableAsync<Tag>();
            await _database.CreateTableAsync<BookTag>();
            await _database.CreateTableAsync<AttributeDefinition>();
            await _database.CreateTableAsync<ContentEntry>();
            await _database.CreateTableAsync<AttributeValue>();
            await _database.CreateTableAsync<ContentEntryTag>();

            await _database.CreateTableAsync<PresentationTheme>();
            await _database.CreateTableAsync<PresentationStructure>();
            await _database.CreateTableAsync<PresentationComponent>();
        }

        // --- BookType Methods ---
        public async Task<List<BookType>> GetBookTypesAsync()
        {
            await Init();
            return await _database.Table<BookType>().ToListAsync();
        }

        public async Task<int> SaveBookTypeAsync(BookType bookType)
        {
            await Init();
            if (bookType.Id != 0)
                return await _database.UpdateAsync(bookType);
            else
                return await _database.InsertAsync(bookType);
        }

        public async Task<int> DeleteBookTypeAsync(BookType bookType)
        {
            await Init();
            // TODO: Cần xóa các sách, thuộc tính... liên quan
            return await _database.DeleteAsync(bookType);
        }

        // --- Tag Methods ---
        public async Task<List<Tag>> GetTagsAsync()
        {
            await Init();
            return await _database.Table<Tag>().ToListAsync();
        }

        public async Task<int> SaveTagAsync(Tag tag)
        {
            await Init();
            if (tag.Id != 0)
                return await _database.UpdateAsync(tag);
            else
                return await _database.InsertAsync(tag);
        }

        public async Task<int> DeleteTagAsync(Tag tag)
        {
            await Init();
            return await _database.DeleteAsync(tag);
        }

        // --- Book Methods ---
        public async Task<List<Book>> GetBooksAsync()
        {
            await Init();
            return await _database.Table<Book>().ToListAsync();
        }

        public async Task<Book> GetBookAsync(int id)
        {
            await Init();
            return await _database.Table<Book>().Where(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveBookAsync(Book book)
        {
            await Init();
            if (book.Id != 0)
                return await _database.UpdateAsync(book);
            else
                return await _database.InsertAsync(book);
        }
        public async Task<int> DeleteBookAsync(Book book)
        {
            await Init();
            // TODO: Cần xóa các mục liên quan (BookTag, ContentEntry, AttributeValue...)
            return await _database.DeleteAsync(book);
        }


        // --- BookTag (M-M) Methods ---
        public async Task<List<BookTag>> GetTagsForBookAsync(int bookId)
        {
            await Init();
            return await _database.Table<BookTag>().Where(bt => bt.BookId == bookId).ToListAsync();
        }

        public async Task SetTagsForBookAsync(int bookId, List<Tag> tags)
        {
            await Init();
            // Xóa các tag cũ
            await _database.Table<BookTag>().DeleteAsync(bt => bt.BookId == bookId);
            // Thêm tag mới
            var newBookTags = tags.Select(t => new BookTag { BookId = bookId, TagId = t.Id });
            await _database.InsertAllAsync(newBookTags);
        }
        public async Task<List<ContentEntryTag>> GetTagsForContentEntryAsync(int contentEntryId)
        {
            await Init();
            return await _database.Table<ContentEntryTag>().Where(cet => cet.ContentEntryId == contentEntryId).ToListAsync();
        }
        public async Task SetTagsForContentEntryAsync(int contentEntryId, List<Tag> tags)
        {
            await Init();
            // Xóa các tag cũ
            await _database.Table<ContentEntryTag>().DeleteAsync(cet => cet.ContentEntryId == contentEntryId);
            // Thêm tag mới
            var newContentEntryTags = tags.Select(t => new ContentEntryTag { ContentEntryId = contentEntryId, TagId = t.Id });
            await _database.InsertAllAsync(newContentEntryTags);
        }

        // --- AttributeDefinition Methods ---
        public async Task<List<AttributeDefinition>> GetAttributesForBookTypeAsync(int bookTypeId)
        {
            await Init();
            return await _database.Table<AttributeDefinition>().Where(ad => ad.BookTypeId == bookTypeId).ToListAsync();
        }

        public async Task<List<AttributeDefinition>> GetAttributesForBookAsync(int bookId)
        {
            await Init();
            return await _database.Table<AttributeDefinition>().Where(ad => ad.BookId == bookId).ToListAsync();
        }
        public async Task<List<AttributeDefinition>> GetAllAttributesForBookAsync(int bookId)
        {
            await Init();
            var book = await GetBookAsync(bookId);
            if (book == null) return new List<AttributeDefinition>();

            var commonAttrs = await GetAttributesForBookTypeAsync(book.BookTypeId);
            var privateAttrs = await GetAttributesForBookAsync(bookId);

            return commonAttrs.Concat(privateAttrs).ToList();
        }
        public async Task<int> SaveAttributeDefinitionAsync(AttributeDefinition attribute)
        {
            await Init();
            if (attribute.Id != 0)
                return await _database.UpdateAsync(attribute);
            else
                return await _database.InsertAsync(attribute);
        }
        public async Task<int> DeleteAttributeDefinitionAsync(AttributeDefinition attribute)
        {
            await Init();
            return await _database.DeleteAsync(attribute);
        }

        // --- ContentEntry & AttributeValue Methods ---
        public async Task<List<ContentEntry>> GetContentEntriesAsync(int bookId)
        {
            await Init();
            return await _database.Table<ContentEntry>().Where(ce => ce.BookId == bookId).ToListAsync();
        }
        public async Task<List<ContentEntry>> SearchContentEntriesAsync(string searchText, bool isExactSearch,
            int? bookTypeId, int? bookId, int? attributeId, List<int> tagIds)
        {
            await Init();

            var query = new StringBuilder("SELECT DISTINCT T1.* FROM ContentEntry AS T1");
            var args = new List<object>();

            query.Append(" INNER JOIN Book AS T2 ON T1.BookId = T2.Id");
            // Join với ContentEntryTag nếu cần lọc theo Tag
            if (tagIds != null && tagIds.Any())
            {
                query.Append(" INNER JOIN ContentEntryTags AS T4 ON T1.Id = T4.ContentEntryId");
            }

            // Chỉ join Bảng Giá trị (AttributeValue) nếu *bắt buộc* (lọc theo ID hoặc tìm kiếm chính xác)
            if (attributeId.HasValue || (isExactSearch && !string.IsNullOrWhiteSpace(searchText)))
            {
                query.Append(" INNER JOIN AttributeValue AS T3 ON T1.Id = T3.ContentEntryId");
            }

            var whereClauses = new List<string>();

            if (bookTypeId.HasValue)
            {
                whereClauses.Add("T2.BookTypeId = ?");
                args.Add(bookTypeId.Value);
            }

            if (bookId.HasValue)
            {
                whereClauses.Add("T1.BookId = ?");
                args.Add(bookId.Value);
            }

            if (attributeId.HasValue)
            {
                whereClauses.Add("T3.AttributeDefinitionId = ?");
                args.Add(attributeId.Value);
            }

            // 1. TÌM KIẾM CHÍNH XÁC: Dùng SQL LIKE
            if (isExactSearch && !string.IsNullOrWhiteSpace(searchText))
            {
                whereClauses.Add("T3.Value LIKE ?");
                args.Add($"%{searchText}%");
            }
            // --- Lọc theo Tag (khớp BẤT KỲ tag nào) ---
            if (tagIds != null && tagIds.Any())
            {
                // Tạo danh sách tham số (?, ?, ?)
                var tagParams = string.Join(",", Enumerable.Repeat("?", tagIds.Count));
                whereClauses.Add($"T4.TagId IN ({tagParams})");
                args.AddRange(tagIds.Cast<object>());
            }

            if (whereClauses.Any())
            {
                query.Append(" WHERE " + string.Join(" AND ", whereClauses));
            }

            query.Append(" ORDER BY T1.DateAdded DESC");

            // Lấy danh sách ứng viên từ DB
            var candidateEntries = await _database.QueryAsync<ContentEntry>(query.ToString(), args.ToArray());

            // 2. TÌM KIẾM KHÔNG CHÍNH XÁC (FUZZY): Lọc thêm bằng C#
            if (!isExactSearch && !string.IsNullOrWhiteSpace(searchText))
            {
                var normalizedSearchText = SearchHelper.NormalizeString(searchText);
                var finalResults = new List<ContentEntry>();

                foreach (var entry in candidateEntries)
                {
                    // Lấy tất cả giá trị thuộc tính của mục nội dung này
                    var values = await GetAttributeValuesAsync(entry.Id);

                    // Nếu lọc theo thuộc tính cụ thể, chỉ tìm trong thuộc tính đó
                    if (attributeId.HasValue)
                    {
                        values = values.Where(v => v.AttributeDefinitionId == attributeId.Value).ToList();
                    }

                    foreach (var value in values)
                    {
                        var normalizedValue = SearchHelper.NormalizeString(value.Value);
                        if (normalizedValue.Contains(normalizedSearchText))
                        {
                            finalResults.Add(entry);
                            break; // Đã tìm thấy, chuyển sang entry tiếp theo
                        }
                    }
                }
                return finalResults; // Trả về danh sách đã lọc bằng C#
            }

            return candidateEntries; // Trả về danh sách lọc bằng SQL (tìm kiếm chính xác hoặc không tìm kiếm)
        }
        public async Task<int> SaveContentEntryAsync(ContentEntry entry)
        {
            await Init();
            if (entry.Id != 0)
                return await _database.UpdateAsync(entry);
            else
                return await _database.InsertAsync(entry);
        }

        public async Task<int> DeleteContentEntryAsync(ContentEntry entry)
        {
            await Init();
            // Cũng nên xóa các AttributeValue liên quan
            await _database.Table<AttributeValue>().DeleteAsync(val => val.ContentEntryId == entry.Id);
            return await _database.DeleteAsync(entry);
        }

        public async Task<List<AttributeValue>> GetAttributeValuesAsync(int contentEntryId)
        {
            await Init();
            return await _database.Table<AttributeValue>().Where(val => val.ContentEntryId == contentEntryId).ToListAsync();
        }

        public async Task<AttributeValue?> GetAttributeValueAsync(int contentEntryId, int attributeDefinitionId)
        {
            await Init();
            return await _database.Table<AttributeValue>()
                .Where(v => v.ContentEntryId == contentEntryId && v.AttributeDefinitionId == attributeDefinitionId)
                .FirstOrDefaultAsync();
        }

        public async Task SaveAttributeValueAsync(AttributeValue value)
        {
            await Init();
            // Tìm giá trị hiện có
            var existing = await _database.Table<AttributeValue>()
                .Where(v => v.ContentEntryId == value.ContentEntryId && v.AttributeDefinitionId == value.AttributeDefinitionId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Value = value.Value;
                await _database.UpdateAsync(existing);
            }
            else
            {
                await _database.InsertAsync(value);
            }
        }
        // --- PresentationTheme Methods ---
        public async Task<List<PresentationTheme>> GetPresentationThemesAsync()
        {
            await Init();
            return await _database.Table<PresentationTheme>().ToListAsync();
        }

        public async Task<int> SavePresentationThemeAsync(PresentationTheme theme)
        {
            await Init();
            if (theme.Id != 0)
                return await _database.UpdateAsync(theme);
            else
                return await _database.InsertAsync(theme);
        }

        public async Task<int> DeletePresentationThemeAsync(PresentationTheme theme)
        {
            await Init();
            return await _database.DeleteAsync(theme);
        }

        // --- PresentationStructure Methods ---
        public async Task<List<PresentationStructure>> GetPresentationStructuresAsync()
        {
            await Init();
            return await _database.Table<PresentationStructure>().ToListAsync();
        }

        public async Task<int> SavePresentationStructureAsync(PresentationStructure structure)
        {
            await Init();
            if (structure.Id != 0)
                return await _database.UpdateAsync(structure);
            else
                return await _database.InsertAsync(structure);
        }

        public async Task<int> DeletePresentationStructureAsync(PresentationStructure structure)
        {
            await Init();
            // Cũng xóa các thành phần con
            await _database.Table<PresentationComponent>().DeleteAsync(c => c.StructureId == structure.Id);
            return await _database.DeleteAsync(structure);
        }

        // --- PresentationComponent Methods ---
        public async Task<List<PresentationComponent>> GetComponentsForStructureAsync(int structureId)
        {
            await Init();
            return await _database.Table<PresentationComponent>()
                                .Where(c => c.StructureId == structureId)
                                .OrderBy(c => c.DisplayOrder)
                                .ToListAsync();
        }

        public async Task<int> SaveComponentAsync(PresentationComponent component)
        {
            await Init();
            if (component.Id != 0)
                return await _database.UpdateAsync(component);
            else
                return await _database.InsertAsync(component);
        }

        public async Task<int> DeleteComponentAsync(PresentationComponent component)
        {
            await Init();
            return await _database.DeleteAsync(component);
        }

        // Dùng để lưu lại thứ tự và các thay đổi
        public async Task SaveComponentsForStructureAsync(int structureId, List<PresentationComponent> components)
        {
            await Init();
            // Xóa tất cả component cũ
            await _database.Table<PresentationComponent>().DeleteAsync(c => c.StructureId == structureId);

            // Thêm lại với thứ tự mới
            int order = 0;
            foreach (var comp in components)
            {
                comp.Id = 0; // Đảm bảo tạo mới
                comp.StructureId = structureId;
                comp.DisplayOrder = order++;
            }
            await _database.InsertAllAsync(components);
        }
    }
}