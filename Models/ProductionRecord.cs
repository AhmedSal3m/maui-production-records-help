using SQLite;

namespace PlasticQC.Models
{
    [Table("ProductionRecords")]
    public class ProductionRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ProductId { get; set; }

        [Indexed]
        public int StandardId { get; set; }

        public string MachineNumber { get; set; }

        public int QuantityMeasured { get; set; }

        public DateTime RecordDate { get; set; } = DateTime.Now;

        public int CreatedById { get; set; }

        [Ignore]
        public List<MeasurementEntry> Measurements { get; set; } = new List<MeasurementEntry>();

        [Ignore]
        public string CreatedByName { get; set; }

        [Ignore]
        public string ProductName { get; set; }

        [Ignore]
        public string ProductNumber { get; set; }
    }
}
