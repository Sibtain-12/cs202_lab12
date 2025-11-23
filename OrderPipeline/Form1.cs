using System;
using System.Windows.Forms;

namespace OrderPipeline
{
    public partial class Form1 : Form
    {
        // Events for Task 1
        public event EventHandler<OrderEventArgs> OrderCreated;
        public event EventHandler<OrderEventArgs> OrderRejected;
        public event EventHandler<OrderEventArgs> OrderConfirmed;

        // Task 2 event
        public event EventHandler<ShipEventArgs> OrderShipped;

        // Track whether last order was confirmed
        private bool lastOrderConfirmed = false;

        public Form1()
        {
            InitializeComponent();

            // Populate product list if not set in designer
            if (cmbProduct.Items.Count == 0)
            {
                cmbProduct.Items.AddRange(new object[] { "Laptop", "Mouse", "Keyboard" });
                cmbProduct.SelectedIndex = 0;
            }

            // Subscribe Task 1 handlers
            OrderCreated += ValidateOrder;
            OrderCreated += DisplayOrderInfo;

            OrderRejected += ShowRejection;
            OrderConfirmed += ShowConfirmation;

            // Subscribe Task 2 base handler
            OrderShipped += ShowDispatch;

            // Publishers
            btnProcessOrder.Click += btnProcessOrder_Click;
            btnShipOrder.Click += btnShipOrder_Click;
        }

        // ----- Task 1: publisher -----
        private void btnProcessOrder_Click(object sender, EventArgs e)
        {
            string customer = txtCustomerName.Text.Trim();
            string product = cmbProduct.SelectedItem?.ToString() ?? string.Empty;
            int quantity = (int)numQuantity.Value;

            var args = new OrderEventArgs(customer, product, quantity);

            // Reset flag; will be set true only after confirmation
            lastOrderConfirmed = false;

            OrderCreated?.Invoke(this, args);
        }

        // ----- Task 1: subscribers -----

        // 1) ValidateOrder: checks quantity and chains rejection/confirmation
        private void ValidateOrder(object sender, OrderEventArgs e)
        {
            if (e.Quantity > 0)
            {
                lblStatus.Text = "Validated";
                // Chain OrderConfirmed if valid
                OrderConfirmed?.Invoke(this, e);
            }
            else
            {
                // Invalid order -> fire OrderRejected
                OrderRejected?.Invoke(this, e);
            }
        }

        // 2) DisplayOrderInfo: shows summary in MessageBox
        private void DisplayOrderInfo(object sender, OrderEventArgs e)
        {
            string msg =
                $"Customer: {e.CustomerName}\nProduct: {e.Product}\nQuantity: {e.Quantity}";
            MessageBox.Show(msg, "Order Summary",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 3) ShowRejection: handles OrderRejected
        private void ShowRejection(object sender, OrderEventArgs e)
        {
            lastOrderConfirmed = false;
            lblStatus.Text = "Order Invalid \u2013 Please retry";
        }

        // 4) ShowConfirmation: handles OrderConfirmed
        private void ShowConfirmation(object sender, OrderEventArgs e)
        {
            lastOrderConfirmed = true;
            lblStatus.Text =
                $"Order Processed Successfully for {e.CustomerName}";
        }
        // ----- Task 2: publisher for shipping -----
        private void btnShipOrder_Click(object sender, EventArgs e)
        {
            // Event should fire only if previous order was confirmed
            if (!lastOrderConfirmed)
            {
                MessageBox.Show("Cannot ship: last order is not confirmed.",
                    "OrderPipeline",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string product = cmbProduct.SelectedItem?.ToString() ?? "Unknown";
            bool express = chkExpress.Checked;

            // Dynamic subscriber management for NotifyCourier
            OrderShipped -= NotifyCourier;        // avoid duplicates
            if (express)
            {
                OrderShipped += NotifyCourier;    // filter: only when Express checked
            }

            var shipArgs = new ShipEventArgs(product, express);

            OrderShipped?.Invoke(this, shipArgs);
        }

        // Subscriber 1: always present
        private void ShowDispatch(object sender, ShipEventArgs e)
        {
            lblStatus.Text = $"Product dispatched: {e.Product}";
        }

        // Subscriber 2: added/removed dynamically based on chkExpress
        private void NotifyCourier(object sender, ShipEventArgs e)
        {
            if (e.Express)
            {
                MessageBox.Show("Express delivery initiated!",
                    "Courier",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
