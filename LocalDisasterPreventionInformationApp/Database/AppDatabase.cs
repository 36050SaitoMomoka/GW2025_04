using LocalDisasterPreventionInformationApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Database {
    public class AppDatabase {
        private readonly SQLiteAsyncConnection _db;

        public AppDatabase(string dbPath) {
            _db = new SQLiteAsyncConnection(dbPath);

            //テーブル作成
            _db.CreateTableAsync<Product>().Wait();
            _db.CreateTableAsync<Stock>().Wait();
            _db.CreateTableAsync<Shelter>().Wait();
        }

        //===================
        //  商品テーブル
        //===================
        public async Task<int> AddProductIfNotExistsAsync(string productName) {
            // 同じ名前の商品があるか確認
            var existing = await _db.Table<Product>()
                                    .Where(x => x.Name == productName)
                                    .FirstOrDefaultAsync();

            //既に登録済み
            if (existing != null) {
                return 0;
            }

            // 新規追加
            var newProduct = new Product { Name = productName };
            return await _db.InsertAsync(newProduct);
        }

        public Task<List<Product>> GetProductsAsync()
            => _db.Table<Product>().ToListAsync();

        public Task<int> DeleteProductAsync(Product item)
            => _db.DeleteAsync(item);

        //===================
        //  在庫テーブル
        //===================
        public async Task<int> AddOrUpdateStockAsync(Stock stock) {
            // 同じ商品IDかつ同じ消費期限の在庫を探す
            var existing = await _db.Table<Stock>()
                                    .Where(x => x.ProductId == stock.ProductId &&
                                                x.ExpirationDate == stock.ExpirationDate)
                                    .FirstOrDefaultAsync();

            //同じ商品IDかつ同じ消費期限の場合
            if (existing != null) {
                existing.Quantity += stock.Quantity;
                return await _db.UpdateAsync(existing);
            }

            // 新規追加
            return await _db.InsertAsync(stock);
        }

        public Task<List<Stock>> GetStocksAsync()
            => _db.Table<Stock>().ToListAsync();

        public Task<int> DeleteStockAsync(Stock item)
            => _db.DeleteAsync(item);

        //===========================
        //  全国避難所情報テーブル
        //===========================
        public Task<List<Shelter>> GetSheltersAsync()
            => _db.Table<Shelter>().ToListAsync();

        public Task<Shelter> GetShelterAsync(string id)
            => _db.Table<Shelter>().Where(x => x.ShelterId == id).FirstOrDefaultAsync();

        public Task<int> SaveShelterAsync(Shelter item)
            => _db.InsertOrReplaceAsync(item);

        public Task<int> DeleteShelterAsync(Shelter item)
            => _db.DeleteAsync(item);
    }
}
