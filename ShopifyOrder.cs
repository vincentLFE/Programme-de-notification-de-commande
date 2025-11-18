using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShopifyNotifierWPF
{
    public class ShopifyOrder
    {
        public long id { get; set; }
        public Customer customer { get; set; }
        public string total_price { get; set; }
        public string created_at { get; set; }
        public string pickup_time { get; set; }
        public string note { get; set; }
        public ShippingAddress shipping_address { get; set; }
        public List<Product> line_items { get; set; }
    }

    public class Customer
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

    public class ShippingAddress
    {
        public string address1 { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
    }

    public class Product
    {
        [JsonPropertyName("title")]
        public string Name { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }
    }
}
