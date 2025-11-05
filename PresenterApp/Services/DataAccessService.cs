// File: Services/DataAccessService.cs
using PresenterApp.Models;
using SQLite;
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

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Library.db3"); // Đổi tên DB
            _database = new SQLiteAsyncConnection(databasePath);

            // Tạo các bảng mới
            await _database.CreateTableAsync<BookType>();
            await _database.CreateTableAsync<Book>();
            await _database.CreateTableAsync<Tag>();
            await _database.CreateTableAsync<BookTag>();
            await _database.CreateTableAsync<AttributeDefinition>();
            await _database.CreateTableAsync<ContentEntry>();
            await _database.CreateTableAsync<AttributeValue>();
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
    }
}