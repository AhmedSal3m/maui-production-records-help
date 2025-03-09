using SQLite;

namespace PlasticQC.Models
{
    [Table("MeasurementEntries")]
    public class MeasurementEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int RecordId { get; set; }

        public int ItemNumber { get; set; }

        public bool VisualLookOk { get; set; }

        public double Weight { get; set; }
        public bool WeightInSpec { get; set; }

        public bool HeightOk { get; set; }

        public double RimThickness { get; set; }
        public bool RimThicknessInSpec { get; set; }

        public double Load { get; set; }
        public bool LoadInSpec { get; set; }
    }
}