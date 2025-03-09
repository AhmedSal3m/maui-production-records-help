using PlasticQC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlasticQC.Services
{
    public class ProductService
    {
        private readonly DatabaseService _databaseService;

        public ProductService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _databaseService.GetProductsAsync();
        }

        public async Task<Product> GetProductAsync(int id)
        {
            return await _databaseService.GetProductAsync(id);
        }

        public async Task<List<ProductStandard>> GetProductStandardsAsync(int productId)
        {
            return await _databaseService.GetProductStandardsAsync(productId);
        }

        public async Task<ProductStandard> GetProductStandardAsync(int id)
        {
            return await _databaseService.GetProductStandardAsync(id);
        }

        public async Task<int> SaveProductAsync(Product product)
        {
            return await _databaseService.SaveProductAsync(product);
        }

        public async Task<int> SaveProductStandardAsync(ProductStandard standard)
        {
            return await _databaseService.SaveProductStandardAsync(standard);
        }

        public async Task<bool> DeleteProductAsync(Product product)
        {
            try
            {
                // Get all standards for this product
                var standards = await _databaseService.GetProductStandardsAsync(product.Id);

                // Delete all standards first
                foreach (var standard in standards)
                {
                    await _databaseService.DeleteProductStandardAsync(standard);
                }

                // Then delete the product
                await _databaseService.DeleteProductAsync(product);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductStandardAsync(ProductStandard standard)
        {
            try
            {
                await _databaseService.DeleteProductStandardAsync(standard);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}