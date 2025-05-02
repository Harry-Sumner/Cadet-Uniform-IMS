namespace Cadet_Uniform_IMS.Data
{
    public class StockReportItem
    {
        public string UniformName { get; set; } = "";
        public string UniformType { get; set; } = "";
        public int Quantity { get; set; }
        public int Available { get; set; }
        public string AttributeName { get; set; } = "";
        public string Size { get; set; } = "";
    }

    public class PendingReportItem
    {
        public string UniformName { get; set; } = "";
        public string UniformType { get; set; } = "";
        public int Quantity { get; set; }
        public string AttributeName { get; set; } = "";
        public string Size { get; set; } = "";
        public string Username { get; set; } = "";
    }

    public class ReturnReportItem
    {
        public string UniformName { get; set; } = "";
        public string UniformType { get; set; } = "";
        public int Quantity { get; set; }
        public string AttributeName { get; set; } = "";
        public string Size { get; set; } = "";
    }
}
