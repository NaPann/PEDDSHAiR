namespace Pedshair.Models
{
    public class CommonModel
    {
        public int Id { get; set; }
        public string? ServiceType { get; set; }
        public string? PriceType { get; set; }
        public double Price { get; set; }
        public string? SocialType { get; set; }
        public string? SocialUsername { get; set; }
        public string? MessageType { get; set; }
        public string? Message { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string? CreatedBy { get; set; }
    }
}
