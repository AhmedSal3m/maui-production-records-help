using PlasticQC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace PlasticQC.Services
{
    public class RecordService
    {
        private readonly DatabaseService _databaseService;

        public RecordService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ProductionRecord>> GetAllRecordsAsync()
        {
            return await _databaseService.GetProductionRecordsAsync();
        }

        public async Task<ProductionRecord> GetRecordAsync(int id)
        {
            return await _databaseService.GetProductionRecordAsync(id);
        }

        public async Task<List<ProductionRecord>> GetRecordsByProductAsync(int productId)
        {
            return await _databaseService.GetProductionRecordsByProductAsync(productId);
        }

        public async Task<List<ProductionRecord>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var allRecords = await _databaseService.GetProductionRecordsAsync();
            return allRecords.Where(r => r.RecordDate.Date >= startDate.Date && r.RecordDate.Date <= endDate.Date).ToList();
        }

        public async Task<List<MeasurementEntry>> GetMeasurementsForRecordAsync(int recordId)
        {
            return await _databaseService.GetMeasurementEntriesAsync(recordId);
        }

        public async Task<int> SaveRecordAsync(ProductionRecord record, List<MeasurementEntry> measurements)
        {
            return await _databaseService.SaveProductionRecordAsync(record, measurements);
        }

        public async Task<bool> DeleteRecordAsync(ProductionRecord record)
        {
            try
            {
                await _databaseService.DeleteProductionRecordAsync(record);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ProductionRecordSummary> GetRecordSummaryAsync(int recordId)
        {
            var record = await _databaseService.GetProductionRecordAsync(recordId);
            if (record == null)
                return null;

            var measurements = await _databaseService.GetMeasurementEntriesAsync(recordId);
            var product = await _databaseService.GetProductAsync(record.ProductId);
            var standard = await _databaseService.GetProductStandardAsync(record.StandardId);
            var user = await _databaseService.GetUserAsync(record.CreatedById);

            return new ProductionRecordSummary
            {
                RecordId = recordId,
                ProductName = product?.Name,
                ProductNumber = product?.ProductNumber,
                MachineNumber = record.MachineNumber,
                RecordDate = record.RecordDate,
                CreatedBy = user?.FullName,
                TotalMeasurements = measurements.Count,
                WeightOutOfSpec = measurements.Count(m => !m.WeightInSpec),
                HeightOutOfSpec = measurements.Count(m => !m.HeightOk),
                RimThicknessOutOfSpec = measurements.Count(m => !m.RimThicknessInSpec),
                LoadOutOfSpec = measurements.Count(m => !m.LoadInSpec),
                VisualIssues = measurements.Count(m => !m.VisualLookOk)
            };
        }
    }

    public class ProductionRecordSummary
    {
        public int RecordId { get; set; }
        public string ProductName { get; set; }
        public string ProductNumber { get; set; }
        public string MachineNumber { get; set; }
        public DateTime RecordDate { get; set; }
        public string CreatedBy { get; set; }
        public int TotalMeasurements { get; set; }
        public int WeightOutOfSpec { get; set; }
        public int HeightOutOfSpec { get; set; }
        public int RimThicknessOutOfSpec { get; set; }
        public int LoadOutOfSpec { get; set; }
        public int VisualIssues { get; set; }

        public bool HasIssues => WeightOutOfSpec > 0 || HeightOutOfSpec > 0 || RimThicknessOutOfSpec > 0 || LoadOutOfSpec > 0 || VisualIssues > 0;
    }
}