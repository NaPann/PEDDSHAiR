namespace Pedshair.Models
{
    //Omise
    public class OmiseEventData
    {
        public string id { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public List<WebhooksDeliveriesData> webhook_deliveries { get; set; }
        public MainData data { get; set; }
    }
    public class WebhooksDeliveriesData
    {
        public string id { get; set; }
        public string key { get; set; }
        public string error { get; set; }
        public string payload { get; set; }
    }
    public class MainData
    {
        public string id { get; set; }
        public int amount { get; set; }
    }

    public class PaymentData
    {
        public decimal amount { get; set; }
        public string   pay_type { get; set; }
    }

    public class PaymentPaySolutionData
    {
        public decimal total { get; set; }
        public string customeremail { get; set; }
        public string productdetail { get; set; }
        public long refno { get; set; }
        public int merchantid { get; set; }
        public string cc { get; set; }

        public string channel { get; set; }

        public string postbackurl { get; set; }
    }
}
