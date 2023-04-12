namespace GeekShopping.Web.Models.Cart
{
    public class CartDetailViewModel
    {
        public long Id { get; set; }
        public long CartHeaderId { get; set; }
        public CartHeaderViewModel CartHeader { get; set; }
        public long ProductId { get; set; }
        public ProductVO Product { get; set; }
        public int Count { get; set; }
    }
}