using SQLite;

namespace PlasticQC.Models
{
    [Table("ProductStandards")]
    public class ProductStandard
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ProductId { get; set; }

        public string MachineNumber { get; set; }

        public int QuantityPerCycle { get; set; }

        public double StandardWeight { get; set; }
        public double WeightTolerancePlus { get; set; }
        public double WeightToleranceMinus { get; set; }

        public double StandardHeight { get; set; }
        public double HeightTolerancePlus { get; set; }
        public double HeightToleranceMinus { get; set; }

        public double StandardRimThickness { get; set; }
        public double RimThicknessTolerancePlus { get; set; }
        public double RimThicknessToleranceMinus { get; set; }

        public double StandardLoad { get; set; }
        public double LoadTolerancePlus { get; set; }
        public double LoadToleranceMinus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int CreatedById { get; set; }
    }
}