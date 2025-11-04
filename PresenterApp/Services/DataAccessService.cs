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
            // Constructor để trống, việc khởi tạo sẽ được thực hiện trong Init()
        }

        private async Task Init()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Songs.db3");
            _database = new SQLiteAsyncConnection(databasePath);
            await _database.CreateTableAsync<Song>();
        }

        public async Task<List<Song>> GetSongsAsync()
        {
            await Init();
            return await _database.Table<Song>().OrderByDescending(s => s.LastModified).ToListAsync();
        }

        public async Task<Song> GetSongAsync(int id)
        {
            await Init();
            return await _database.Table<Song>().Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveSongAsync(Song song)
        {
            await Init();
            song.LastModified = DateTime.UtcNow;
            if (song.Id != 0)
            {
                return await _database.UpdateAsync(song);
            }
            else
            {
                return await _database.InsertAsync(song);
            }
        }

        public async Task<int> DeleteSongAsync(Song song)
        {
            await Init();
            return await _database.DeleteAsync(song);
        }
    }
}