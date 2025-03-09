using SQLite;
using PlasticQC.Models;
using PlasticQC.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;

namespace PlasticQC.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _initialized = false;

        public DatabaseService()
        {
            Debug.WriteLine("DatabaseService constructor called");
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "plasticqc.db3");
                Debug.WriteLine($"Database path: {dbPath}");
                _database = new SQLiteAsyncConnection(dbPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DatabaseService constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task InitializeAsync()
        {
            Debug.WriteLine("InitializeAsync called");

            if (_initialized)
                return;

            try
            {
                // Create tables if they don't exist
                await _database.CreateTablesAsync(CreateFlags.None,
                    typeof(User),
                    typeof(Product),
                    typeof(ProductStandard),
                    typeof(ProductionRecord),
                    typeof(MeasurementEntry)).ConfigureAwait(false);

                // Check if admin user exists, if not create default admin
                var adminUser = await _database.Table<User>()
                    .Where(u => u.Username == "admin")
                    .FirstOrDefaultAsync();

                if (adminUser == null)
                {
                    Debug.WriteLine("Creating default admin user");
                    await _database.InsertAsync(new User
                    {
                        Username = "admin",
                        Password = "password", // In production, this should be hashed
                        FullName = "Administrator",
                        IsAdmin = true,
                        CreatedAt = DateTime.Now
                    });
                }

                _initialized = true;
                Debug.WriteLine("Database initialization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // User methods
        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<User>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetUsersAsync: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User> GetUserAsync(int id)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<User>()
                    .Where(u => u.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetUserAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<User>()
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetUserByUsernameAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveUserAsync(User user)
        {
            try
            {
                await InitializeAsync();
                if (user.Id != 0)
                    return await _database.UpdateAsync(user);
                else
                    return await _database.InsertAsync(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveUserAsync: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> DeleteUserAsync(User user)
        {
            try
            {
                await InitializeAsync();
                return await _database.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteUserAsync: {ex.Message}");
                return -1;
            }
        }

        // Product methods
        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<Product>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductsAsync: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<Product> GetProductAsync(int id)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<Product>()
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveProductAsync(Product product)
        {
            try
            {
                await InitializeAsync();
                if (product.Id != 0)
                    return await _database.UpdateAsync(product);
                else
                    return await _database.InsertAsync(product);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveProductAsync: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> DeleteProductAsync(Product product)
        {
            try
            {
                await InitializeAsync();
                return await _database.DeleteAsync(product);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteProductAsync: {ex.Message}");
                return -1;
            }
        }

        // ProductStandard methods
        public async Task<List<ProductStandard>> GetProductStandardsAsync(int productId)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<ProductStandard>()
                    .Where(ps => ps.ProductId == productId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductStandardsAsync: {ex.Message}");
                return new List<ProductStandard>();
            }
        }

        public async Task<ProductStandard> GetProductStandardAsync(int id)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<ProductStandard>()
                    .Where(ps => ps.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductStandardAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveProductStandardAsync(ProductStandard standard)
        {
            try
            {
                await InitializeAsync();
                if (standard.Id != 0)
                    return await _database.UpdateAsync(standard);
                else
                    return await _database.InsertAsync(standard);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveProductStandardAsync: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> DeleteProductStandardAsync(ProductStandard standard)
        {
            try
            {
                await InitializeAsync();
                return await _database.DeleteAsync(standard);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteProductStandardAsync: {ex.Message}");
                return -1;
            }
        }

        // ProductionRecord methods
        public async Task<List<ProductionRecord>> GetProductionRecordsAsync()
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<ProductionRecord>().ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductionRecordsAsync: {ex.Message}");
                return new List<ProductionRecord>();
            }
        }

        public async Task<List<ProductionRecord>> GetProductionRecordsByProductAsync(int productId)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<ProductionRecord>()
                    .Where(pr => pr.ProductId == productId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductionRecordsByProductAsync: {ex.Message}");
                return new List<ProductionRecord>();
            }
        }

        public async Task<List<ProductionRecord>> GetProductionRecordsByUserAsync(int userId)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<ProductionRecord>()
                    .Where(pr => pr.CreatedById == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductionRecordsByUserAsync: {ex.Message}");
                return new List<ProductionRecord>();
            }
        }

        public async Task<ProductionRecord> GetProductionRecordAsync(int id)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<ProductionRecord>()
                    .Where(pr => pr.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetProductionRecordAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<int> SaveProductionRecordAsync(ProductionRecord record, List<MeasurementEntry> measurements)
        {
            Debug.WriteLine($"SaveProductionRecordAsync called for {measurements.Count} measurements");

            await InitializeAsync();

            try
            {
                int recordId;

                if (record.Id != 0)
                {
                    Debug.WriteLine($"Updating existing record: {record.Id}");
                    await _database.UpdateAsync(record);
                    recordId = record.Id;

                    // Delete existing measurements for this record
                    var deleteResult = await _database.Table<MeasurementEntry>()
                        .Where(m => m.RecordId == recordId)
                        .DeleteAsync();
                    Debug.WriteLine($"Deleted {deleteResult} existing measurements");
                }
                else
                {
                    Debug.WriteLine("Inserting new record");
                    // Force date to current if not set
                    if (record.RecordDate == default)
                        record.RecordDate = DateTime.Now;

                    recordId = await _database.InsertAsync(record);
                    Debug.WriteLine($"New record ID: {recordId}");
                }

                // Set the record ID for all measurements
                int measurementsAdded = 0;
                foreach (var measurement in measurements)
                {
                    measurement.RecordId = recordId;
                    var measurementId = await _database.InsertAsync(measurement);
                    Debug.WriteLine($"Added measurement {++measurementsAdded} with ID: {measurementId}");
                }

                Debug.WriteLine($"Successfully saved record {recordId} with {measurementsAdded} measurements");
                return recordId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in SaveProductionRecordAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let the caller handle it
            }
        }

        public async Task<List<MeasurementEntry>> GetMeasurementEntriesAsync(int recordId)
        {
            try
            {
                await InitializeAsync();
                return await _database.Table<MeasurementEntry>()
                    .Where(m => m.RecordId == recordId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetMeasurementEntriesAsync: {ex.Message}");
                return new List<MeasurementEntry>();
            }
        }

        public async Task<int> DeleteProductionRecordAsync(ProductionRecord record)
        {
            try
            {
                await InitializeAsync();
                // First delete all related measurement entries
                await _database.Table<MeasurementEntry>()
                    .Where(m => m.RecordId == record.Id)
                    .DeleteAsync();

                return await _database.DeleteAsync(record);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteProductionRecordAsync: {ex.Message}");
                return -1;
            }
        }
    }
}