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
        }

        public async Task InitializeAsync() {
            //テーブル作成
            await _db.CreateTableAsync<User>();
            await _db.CreateTableAsync<UserAddress>();
            await _db.CreateTableAsync<Product>();
            await _db.CreateTableAsync<Stock>();
            await _db.CreateTableAsync<Shelter>();
            //テーブル削除するときはフルパスで指定
        }

        //=====================
        //  ユーザ情報テーブル
        //=====================
        public async Task<int> SaveUserAsync(User user) {
            // 既存ユーザーを取得
            var existing = await _db.Table<User>().FirstOrDefaultAsync();

            if (existing != null) {
                // 主キーを維持して更新
                user.UserId = existing.UserId;
                return await _db.UpdateAsync(user);
            }

            // 新規追加
            return await _db.InsertAsync(user);
        }

        //ユーザ情報取得
        public Task<User> GetUserAsync()
            => _db.Table<User>().FirstOrDefaultAsync();

        //=======================
        //  ユーザ住所テーブル
        //=======================

        public async Task<int> AddAddressIfNotExistsAsync(UserAddress address) {
            // 同じ住所があるか確認
            var existing = await _db.Table<UserAddress>()
                                    .Where(x => x.Address == address.Address)
                                    .FirstOrDefaultAsync();

            // 既に登録済みなら追加しない
            if (existing != null)
                return 0;

            // 新規追加
            return await _db.InsertAsync(address);
        }

        // 住所一覧を取得
        public Task<List<UserAddress>> GetAddressesAsync()
            => _db.Table<UserAddress>().ToListAsync();

        // 住所を削除
        public Task<int> DeleteAddressAsync(UserAddress item)
            => _db.DeleteAsync(item);

        // 住所を更新
        public Task<int> UpdateAddressAsync(UserAddress item)
            => _db.UpdateAsync(item);

        // 住所を1件取得
        public Task<UserAddress> GetAddressAsync(int id)
            => _db.Table<UserAddress>()
                  .Where(x => x.AddressId == id)
                  .FirstOrDefaultAsync();

        //=====================
        //  商品テーブル
        //=====================
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

        //商品一覧を取得
        public Task<List<Product>> GetProductsAsync()
            => _db.Table<Product>().ToListAsync();

        //レコードを削除
        public Task<int> DeleteProductAsync(Product item)
            => _db.DeleteAsync(item);

        //=====================
        //  在庫テーブル
        //=====================
        //商品登録または数量増やす
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

        //在庫減らす
        public async Task<int> ReduceStockAsync(int productId, DateTime expirationDate, int amount) {
            // 同じ商品ID＋同じ消費期限の在庫を探す
            var existing = await _db.Table<Stock>()
                                    .Where(x => x.ProductId == productId &&
                                                x.ExpirationDate == expirationDate)
                                    .FirstOrDefaultAsync();

            if (existing == null)
                return 0; // 在庫が存在しない

            // 数量を減らす
            existing.Quantity -= amount;

            // 数量が0以下なら削除する運用も自然
            if (existing.Quantity <= 0) {
                existing.Quantity = 0;
                return await _db.UpdateAsync(existing);
            }

            // 更新
            return await _db.UpdateAsync(existing);
        }

        //在庫一覧を取得
        public Task<List<Stock>> GetStocksAsync()
            => _db.Table<Stock>().ToListAsync();

        //レコードを削除
        public Task<int> DeleteStockAsync(Stock item)
            => _db.DeleteAsync(item);

        //===========================
        //  全国避難所情報テーブル
        //===========================
        public Task<List<Shelter>> GetSheltersAsync()
            => _db.Table<Shelter>().ToListAsync();

        public Task<Shelter> GetShelterAsync(string id)
            => _db.Table<Shelter>().Where(x => x.ShelterId == id).FirstOrDefaultAsync();

        public async Task<int> SaveShelterAsync(Shelter item) {
            var existing = await GetShelterAsync(item.ShelterId);
            if (existing != null)
                return 0;

            return await _db.InsertAsync(item);
        }

        public Task<int> DeleteShelterAsync(Shelter item)
            => _db.DeleteAsync(item);
    }
}
