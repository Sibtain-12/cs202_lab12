using System;

namespace OrderPipeline
{
    public class OrderEventArgs : EventArgs
    {
        public string CustomerName { get; }
        public string Product { get; }
        public int Quantity { get; }

        public OrderEventArgs(string customerName, string product, int quantity)
        {
            CustomerName = customerName;
            Product = product;
            Quantity = quantity;
        }
    }
}
