using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ya.D.Models;
using Ya.D.Services.HTTP;

namespace Ya.D.Services
{
    public class DataProvider
    {
        private static readonly Lazy<DataProvider> _self = new Lazy<DataProvider>(() => new DataProvider());

        private readonly DiskItemComparer _comparer = new DiskItemComparer();
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private readonly ConcurrentQueue<ActionItem> _queue = new ConcurrentQueue<ActionItem>();
        private readonly LocalContext _localContext;

        internal class ActionItem
        {
            public bool ToRemove { get; set; }
            public DiskItem Item { get; set; }
        }

        private DataProvider()
        {
            _localContext = new LocalContext();
        }

        public static DataProvider Instance => _self.Value;

        public async Task<List<DiskItem>> GetItemsAsync(Func<DiskItem, bool> condition, List<DiskItem> actual, int limit = 0, int offset = 0, bool ignoreActual = false)
        {
            List<DiskItem> result;
            if (limit > 0 && offset == 0)
            {
                result = _localContext.Items.Where(condition).Take(limit).ToList();
            }
            else if (limit > 0 && offset > 0)
            {
                result = _localContext.Items.Where(condition).Skip(limit * offset).Take(limit).ToList();
            }
            else
            {
                result = _localContext.Items.Where(condition).ToList();
            }

            if (ignoreActual)
            {
                return result;
            }

            var onlyNew = result.Count == 0;
            var newItems = actual.Except(result, _comparer).ToList();
            foreach (var item in newItems)
            {
                var newItem = await _localContext.Items.AddAsync(item);
                result.Add(newItem.Entity);
            }
            if (onlyNew)
            {
                await Save();
                return result;
            }

            var toRemove = result.Except(actual, _comparer).ToList();
            foreach (var item in toRemove)
            {
                _localContext.ItemsInPlaylist.RemoveRange(_localContext.ItemsInPlaylist.Where(i => i.ItemID == item.ID));
                var removedItem = _localContext.Items.Remove(item);
                result.Remove(removedItem.Entity);
            }
            var updated = new List<DiskItem>();
            foreach (var item in actual)
            {
                var inDB = result.FirstOrDefault(i => i.Path == item.Path);
                if (inDB == null)
                {
                    continue;
                }

                if (inDB.PreviewImage == null && item.PreviewImage != null)
                {
                    inDB = _localContext.Items.Update(inDB).Entity;
                }

                updated.Add(inDB);
            }
            result = updated;
            await Save();
            return result;
        }

        public async Task<DiskItem> CreateItemAsync(DiskItem diskItem)
        {
            if (diskItem.ID != 0)
            {
                return diskItem;
            }

            var result = await _localContext.Items.AddAsync(diskItem);
            await Save();
            return result.Entity;
        }

        public async Task<DiskItem> UpdateItemAsync(DiskItem diskItem)
        {
            if (diskItem.ID == 0)
            {
                return null;
            }

            var result = _localContext.Items.Update(diskItem);
            await Save();
            return result.Entity;
        }

        public async Task UpdateItemsAsync(List<DiskItem> diskItems)
        {
            if (diskItems == null || diskItems.Count == 0)
            {
                return;
            }

            foreach (var item in diskItems)
            {
                var got = _localContext.Items.SingleOrDefault(i => i.ID == item.ID);
                if (got != null)
                {
                    got.CopyFromOther(item);
                    _localContext.Items.Update(got);
                    await Save();
                }
                else
                {
                    await _localContext.AddAsync(item);
                }
            }
        }

        public void Close()
        {
            _localContext.Dispose();
            _source.Cancel();
        }

        public async Task ResetAsync()
        {
            var items = _localContext.Items.Where(i => i.PreviewImage != null || i.BigPreviewImage != null);
            foreach (var item in items)
            {
                item.PreviewImage = null;
                item.BigPreviewImage = null;
            }
            _localContext.UpdateRange(items);
            await Save();
        }

        #region Playlists

        public async Task<PlayList> GetPlaylistByIDAsync(uint ID)
        {
            PlayList result;
            result = await _localContext.PlayLists
                    .Where(i => i.ID == ID)
                    .Include(i => i.Type)
                    .FirstOrDefaultAsync();
            if (result != null)
            {
                result.DiskItems = await _localContext.Items
                        .Include(i => i.PlayLists)
                        .Where(i => i.PlayLists.FirstOrDefault(p => p.PlayListID == ID) != null)
                        .ToListAsync();
            }
            return result;
        }

        public List<PlayList> GetPlaylists(bool withItems = false)
        {
            List<PlayList> result;
            using (var context = new LocalContext())
            {
                result = context.PlayLists
                        .Include(i => i.Type)
                        .ToList();
                if (withItems)
                {
                    foreach (var playList in result)
                    {
                        playList.DiskItems = context.Items
                            .Include(i => i.PlayLists)
                            .Where(i => i.PlayLists.FirstOrDefault(p => p.PlayListID == playList.ID) != null)
                            .ToList();
                    }
                }
            }
            return result;
        }

        public async Task<PlayList> AddPlayListAsync(PlayList playList)
        {
            PlayList result;
            var entry = await _localContext.PlayLists.AddAsync(playList);
            await Save();
            result = entry.Entity;
            return result;
        }

        public async Task AddToPlayListAsync(PlayList playList, DiskItem diskItem)
        {
            var item = new ItemList() { ItemID = diskItem.ID, PlayListID = playList.ID };
            await _localContext.ItemsInPlaylist.AddAsync(item);
            await Save();
        }

        public async Task RemoveFromPlayListAsync(PlayList playList, DiskItem diskItem)
        {
            var item = await _localContext.ItemsInPlaylist.FirstOrDefaultAsync(i => i.ItemID == diskItem.ID && i.PlayListID == playList.ID);
            if (item != null)
            {
                _localContext.ItemsInPlaylist.Remove(item);
                await Save();
            }
        }

        #endregion

        #region 

        public async Task<PlayListType> AddPlayListTypeAsync(PlayListType playListType)
        {
            var entry = await _localContext.AddAsync(playListType);
            return entry.Entity;
        }

        public List<PlayListType> GetPlayListTypesAsync(Func<PlayListType, bool> expression = null)
        {
            var result = _localContext.PlayListTypes.ToList();
            if (expression != null)
            {
                return result.Where(expression).ToList();
            }

            return result;
        }

        #endregion

        private async Task Save([CallerMemberName]string caller = "")
        {
            try
            {
                await _localContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await FlourService.I.PushAnalytics("db.error",
                    JsonConvert.SerializeObject(new
                    {
                        calledBy = caller,
                        error = ex.Message,
                    }
                ));
            }
        }
    }
}
