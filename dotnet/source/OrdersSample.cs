using System;
using Google.Apis.ShoppingContent.v2_1;
using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2_1.Data;
using System.Collections.Generic;
using CommandLine;
using System.Data.SqlClient;
using System.Data;

namespace ShoppingSamples.Content
{
    /// <summary>
    /// A sample consumer that runs an example workflow for a single test order using the Orders
    /// service in the Content API for Shopping.
    ///
    /// <para>Since access to the Orders service is limited, this sample is written as a separate
    /// program from the rest. Also unlike the other samples, this sample uses the sandbox API
    /// endpoint instead of the normal endpoint for two reasons:</para>
    /// <list type="bullet">
    /// <item>
    /// <description>It provides access to the methods for creating and operating on test
    /// orders.</description>
    /// </item>
    /// <item>
    /// <description>It avoids accidentally mutating existing real orders.</description>
    /// </item></list>
    /// </summary>
    internal class OrdersSample : BaseContentSample
    {
        private Random prng; // Used for random order/tracking/shipment ID creation.
        private ulong nonce; // Nonce used for creating new operation IDs.

        public OrdersSample()
        {
            this.prng = new Random();
        }


        internal override void runCalls()
        {
            var merchantId = config.MerchantId.Value;

            #region create test order
            /* 
            string orderId;
            {
                var req = new OrdersCreateTestOrderRequest();
                req.TemplateName = "template1";
                var resp = sandboxService.Orders.Createtestorder(req, merchantId).Execute();
                orderId = resp.OrderId;
            }*/
            #endregion

            #region Get all Unacknowledged orders, then Acknowledge them (so we dont pull them again)
            // List all unacknowledged orders.  In normal usage, this is where we'd get new
            // order IDs to operate on.  We should see the new test order show up here.

            try
            {
                ListAllUnacknowledgedOrders(merchantId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            //This can stay commented out, i use it in "ListAllUnacknowledgedOrders" after i save the item to our DB
            //Acknowledge(merchantId, "TEST-4149-52-565222222");
            #endregion

            #region print specific order
            //var currentOrder = GetOrder(merchantId, "TEST-7467-39-2793");
            //PrintOrder(currentOrder);
            #endregion

            
            #region CancelLineItem
            try
            {
                {
                    /*
                    var currentOrder = GetOrder(merchantId, "TEST-5244-09-7823");
                    var req = new OrdersCancelLineItemRequest();
                    req.LineItemId = "3HAOTRA7HNCZEXP";
                    req.Quantity = 2;
                    req.Reason = "noInventory";
                    req.ReasonText = "Ran out of inventory while fulfilling request.";
                    req.OperationId = prng.Next().ToString();

                    CancelLineItem(merchantId, "TEST-5244-09-7823", req);
                    */
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            #endregion

            #region adding shipping number
            try
            {
                //var currentOrder = GetOrder(merchantId, "TEST-5328-88-5002");
                //PrintOrder(currentOrder);
                //var shipItem1Req = ShipAllLineItem(merchantId, currentOrder.Id, currentOrder.LineItems[1], "1Z2222222", "1Z2222222");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #endregion

            #region update order to status delivered
            try
            {
                //var currentOrder = GetOrder(merchantId, "TEST-5328-88-5002");
                //PrintOrder(currentOrder);
                //LineItemDelivered(merchantId, currentOrder.Id, "1Z2222222");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #endregion


            Console.ReadKey();
        }

        /// <summary>
        /// Retrieves a particular order given the (Google-specified) order ID.
        /// </summary>
        /// <returns>The order resource for the specified order.</returns>
        private Order GetOrder(ulong merchantId, string orderId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Getting Order {0}", orderId);
            Console.WriteLine("=================================================================");


            Order status = sandboxService.Orders.Get(merchantId, orderId).Execute();
            

            return status;
        }

        /// <summary>
        /// Retrieves a particular order given the merchant-specified order ID.
        /// </summary>
        /// <returns>The order resource for the specified order.</returns>
        private Order GetOrderByMerchantOrderId(ulong merchantId, string merchantOrderId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Getting Merchant Order {0}", merchantOrderId);
            Console.WriteLine("=================================================================");

            var resp =
                sandboxService.Orders.Getbymerchantorderid(merchantId, merchantOrderId).Execute();
            Console.WriteLine();

            return resp.Order;
        }

        /// <summary>
        /// Lists all the unacknowledged orders for the given merchant.
        /// </summary>
        private void ListAllUnacknowledgedOrders(ulong merchantId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Listing Unacknowledged Orders for Merchant {0}", merchantId);
            Console.WriteLine("=================================================================");

            // Retrieve orders list in pages and display data as we receive it.
            string pageToken = null;
            OrdersListResponse ordersResponse = null;

            do
            {
                OrdersResource.ListRequest ordersRequest = sandboxService.Orders.List(merchantId);
                ordersRequest.Acknowledged = false;
                ordersRequest.PageToken = pageToken;

                ordersResponse = ordersRequest.Execute();

                if (ordersResponse.Resources != null && ordersResponse.Resources.Count != 0)
                {
                    foreach (var order in ordersResponse.Resources)
                    {
                        PrintOrder(order);
                        SaveOrder(order);
                    }
                }
                else
                {
                    Console.WriteLine("No orders found.");
                }

                pageToken = ordersResponse.NextPageToken;
            } while (pageToken != null);
            Console.WriteLine();
        }

        // All (non-test) requests that change an order must have a unique operation
        // ID over the lifetime of the order to enable Google to detect and reject
        // duplicate requests.
        // Here, we just use a nonce and bump it each time, since we're not retrying failures
        // and sending them all sequentially.
        private string NewOperationId() {
            string str = nonce.ToString();
            nonce++;
            return str;
        }

        private void Acknowledge(ulong merchantId, string orderId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Acknowledging Order {0}", orderId);
            Console.WriteLine("=================================================================");

            var req = new OrdersAcknowledgeRequest();
            req.OperationId = NewOperationId();
            var resp = sandboxService.Orders.Acknowledge(req, merchantId, orderId).Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();
        }

        private void UpdateMerchantOrderId(ulong merchantId, string orderId, string merchantOrderId)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Updating Merchant Order ID to {0}", merchantOrderId);
            Console.WriteLine("=================================================================");

            var req = new OrdersUpdateMerchantOrderIdRequest();
            req.OperationId = NewOperationId();
            req.MerchantOrderId = merchantOrderId;
            var resp = sandboxService.Orders
                .Updatemerchantorderid(req, merchantId, orderId)
                .Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();
        }

        private void CancelLineItem(ulong merchantId, string orderId,
            OrdersCancelLineItemRequest req)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Canceling {0} of item {1}", req.Quantity, req.LineItemId);
            Console.WriteLine("=================================================================");

            var resp = sandboxService.Orders.Cancellineitem(req, merchantId, orderId).Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();
        }

        private OrdersShipLineItemsRequest ShipAllLineItem(ulong merchantId, string orderId,
            OrderLineItem item, string shipmentID, string TrackingID)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Shipping {0} of item {1}", item.QuantityPending, item.Id);
            Console.WriteLine("=================================================================");

            var itemShip = new OrderShipmentLineItemShipment();
            itemShip.LineItemId = item.Id;
            itemShip.Quantity = item.QuantityPending;

            var req = new OrdersShipLineItemsRequest();
            var shipmentInfo = new OrdersCustomBatchRequestEntryShipLineItemsShipmentInfo();
            shipmentInfo.Carrier = item.ShippingDetails.Method.Carrier;
            shipmentInfo.ShipmentId = shipmentID;
            shipmentInfo.TrackingId = TrackingID;
            req.ShipmentInfos = new List<OrdersCustomBatchRequestEntryShipLineItemsShipmentInfo>();
            req.ShipmentInfos.Add(shipmentInfo);
            req.LineItems = new List<OrderShipmentLineItemShipment>();
            req.LineItems.Add(itemShip);
            req.OperationId = prng.Next().ToString();

            var resp = sandboxService.Orders.Shiplineitems(req, merchantId, orderId).Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();

            // We return req here so that we have access to the randomly-generated IDs in the
            // main program.
            return req;
        }

        private void LineItemDelivered(ulong merchantId, string orderId,
            string shipmentID)
        {
           
            var req = new OrdersUpdateShipmentRequest();
            //req.Carrier = ship.Shipments[0].Carrier;
            //req.TrackingId = "1z";
            req.ShipmentId = shipmentID;
            req.Status = "delivered";
            req.OperationId = prng.Next().ToString();

            var resp = sandboxService.Orders.Updateshipment(req, merchantId, orderId).Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();
        }
        private void LineItemDelivered_OLD(ulong merchantId, string orderId,
            OrdersShipLineItemsRequest ship)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Delivered {0} of item {1}", ship.LineItems[0].Quantity,
                ship.LineItems[0].LineItemId);
            Console.WriteLine("=================================================================");

            var req = new OrdersUpdateShipmentRequest();
            req.Carrier = ship.ShipmentInfos[0].Carrier;
            req.TrackingId = ship.ShipmentInfos[0].TrackingId;
            req.ShipmentId = ship.ShipmentInfos[0].ShipmentId;
            req.Status = "delivered";
            req.OperationId = NewOperationId();

            var resp = sandboxService.Orders.Updateshipment(req, merchantId, orderId).Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();
        }
        private void LineItemReturned(ulong merchantId, string orderId,
            OrdersReturnRefundLineItemRequest req)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine("Returned {0} of item {1}", req.Quantity,
                req.LineItemId);
            Console.WriteLine("=================================================================");

            var resp = sandboxService.Orders.Returnrefundlineitem(req, merchantId, orderId).Execute();

            Console.WriteLine("Finished with status {0}.", resp.ExecutionStatus);
            Console.WriteLine();
        }

        private void PrintOrder(Order order)
        {
            Console.WriteLine();
            Console.WriteLine("Order {0}:", order.Id);
            Console.WriteLine("- Status: {0}", order.Status);
            Console.WriteLine("- Merchant: {0}", order.MerchantId);
            Console.WriteLine("- Merchant order ID: {0}", order.MerchantOrderId);
            Console.WriteLine("Ship Address: {0}", order.DeliveryDetails.Address.StreetAddress[0]);
            //Console.WriteLine("Ship Address2: {0}", order.DeliveryDetails.Address.StreetAddress[1]);
            //Console.WriteLine("Delivery Detail Address: {0}", order.DeliveryDetails.Address.StreetAddress[0]);
            Console.WriteLine("Ship City: {0}", order.DeliveryDetails.Address.Locality);
            Console.WriteLine("Ship State: {0}", order.DeliveryDetails.Address.Region);
            Console.WriteLine("Ship Zip: {0}", order.DeliveryDetails.Address.PostalCode);
            Console.WriteLine("Ship Country: {0}", order.DeliveryDetails.Address.Country);

            if (order.Customer != null)
            {
                Console.WriteLine("- Customer information:");
                Console.WriteLine("  - Full name: {0}", order.Customer.FullName);
                if (order.Customer.MarketingRightsInfo != null)
                {
                    Console.WriteLine("  - Email: {0}", order.Customer.MarketingRightsInfo.MarketingEmailAddress);
                }
            }
            Console.WriteLine("- Placed on date: {0}", order.PlacedDate);
            if (order.NetPriceAmount != null)
            {
                Console.WriteLine("- Net amount: {0} {1}", order.NetPriceAmount.Value,
                    order.NetPriceAmount.Currency);
            }
            Console.WriteLine("- Payment status: {0}", order.PaymentStatus);
            Console.WriteLine("- Acknowledged: {0}", order.Acknowledged == true ? "yes" : "no");
            if (order.LineItems != null && order.LineItems.Count > 0)
            {
                Console.WriteLine("- {0} line item(s):", order.LineItems.Count);
                foreach (var item in order.LineItems)
                {
                    PrintOrderLineItem(item);
                }
            }
            if (order.ShippingCost != null)
            {
                Console.WriteLine("- Shipping cost: {0} {1}", order.ShippingCost.Value,
                    order.ShippingCost.Currency);
            }
            if (order.ShippingCostTax != null)
            {
                Console.WriteLine("- Shipping cost tax: {0} {1}", order.ShippingCostTax.Value,
                    order.ShippingCostTax.Currency);
            }
            if (order.Shipments != null && order.Shipments.Count > 0)
            {
                Console.WriteLine("- {0} shipment(s):", order.Shipments.Count);
                foreach (var shipment in order.Shipments)
                {
                    Console.WriteLine("  Shipment {0}", shipment.Id);
                    Console.WriteLine("  - Creation date: {0}", shipment.CreationDate);
                    Console.WriteLine("  - Carrier: {0}", shipment.Carrier);
                    Console.WriteLine("  - Tracking ID: {0}", shipment.TrackingId);
                    if (shipment.LineItems != null && shipment.LineItems.Count > 0)
                    {
                        Console.WriteLine("- {0} line item(s):", shipment.LineItems.Count);
                        foreach (var item in shipment.LineItems) {
                            Console.WriteLine("  {0} of item {1}", item.Quantity, item.LineItemId);
                        }
                    }
                    if (shipment.DeliveryDate != null) {
                        Console.WriteLine("  - Delivery date: {0}", shipment.DeliveryDate);
                    }
                }
            }
        }

        private void SaveOrder(Order order)
        {
            var merchantId = config.MerchantId.Value;

            if (order.LineItems != null && order.LineItems.Count > 0)
            {
                //Console.WriteLine("- {0} line item(s):", order.LineItems.Count);
                foreach (var item in order.LineItems)
                {
                    PrintOrderLineItem(item);

                    insertOrderInfo(item, order);
                    Acknowledge(merchantId, order.Id);
                    
                }
            }
           
        }


        private void PrintIfNonzero(long? l, string s)
        {
            if(l.HasValue && l.Value > 0) {
                Console.WriteLine("  - {0}: {1}", s, l.Value);
            }
        }

        private void PrintOrderLineItem(OrderLineItem item)
        {
            Console.WriteLine("  Line item: {0}", item.Id);
            Console.WriteLine("  - Product: {0} ({1})", item.Product.Id, item.Product.Title);
            Console.WriteLine("  - Price: {0} {1}", item.Price.Value, item.Price.Currency);
            Console.WriteLine("  - Tax: {0} {1}", item.Tax.Value, item.Tax.Currency);
            if (item.ShippingDetails != null)
            {
                Console.WriteLine("  - Ship by date: {0}", item.ShippingDetails.ShipByDate);
                Console.WriteLine("  - Deliver by date: {0}", item.ShippingDetails.DeliverByDate);
                Console.WriteLine("  - Deliver via {0} {1} ({2} - {3} days)",
                    item.ShippingDetails.Method.Carrier, item.ShippingDetails.Method.MethodName,
                    item.ShippingDetails.Method.MinDaysInTransit,
                    item.ShippingDetails.Method.MaxDaysInTransit);
            }
            if (item.ReturnInfo != null && item.ReturnInfo.IsReturnable == true)
            {
                Console.WriteLine("  - Item is returnable.");
                Console.WriteLine("    - Days to return: {0}", item.ReturnInfo.DaysToReturn);
                Console.WriteLine("    - Return policy is at {0}.", item.ReturnInfo.PolicyUrl);
            }
            else
            {
                Console.WriteLine("  - Item is not returnable.");
            }
            PrintIfNonzero(item.QuantityOrdered, "Quantity Ordered");
            PrintIfNonzero(item.QuantityPending, "Quantity Pending");
            PrintIfNonzero(item.QuantityCanceled, "Quantity Canceled");
            PrintIfNonzero(item.QuantityShipped, "Quantity Shipped");
            PrintIfNonzero(item.QuantityDelivered, "Quantity Delivered");
            PrintIfNonzero(item.QuantityReturned, "Quantity Returned");
            if (item.Cancellations != null && item.Cancellations.Count > 0)
            {
                Console.WriteLine("  - {0} cancellations(s):", item.Cancellations.Count);
                foreach (var cancel in item.Cancellations)
                {
                    Console.WriteLine("    Cancellation:");
                    if (cancel.Actor != null)
                    {
                        Console.WriteLine("    - Actor: {0}", cancel.Actor);
                    }
                    Console.WriteLine("    - Creation date: {0}", cancel.CreationDate);
                    Console.WriteLine("    - Quantity: {0}", cancel.Quantity);
                    Console.WriteLine("    - Reason: {0}", cancel.Reason);
                    Console.WriteLine("    - Reason text: {0}", cancel.ReasonText);
                }
            }
            if (item.Returns != null && item.Returns.Count > 0)
            {
                Console.WriteLine("  - {0} return(s):", item.Returns.Count);
                foreach (var ret in item.Returns)
                {
                    Console.WriteLine("    Return:");
                    if (ret.Actor != null)
                    {
                        Console.WriteLine("    - Actor: {0}", ret.Actor);
                    }
                    Console.WriteLine("    - Creation date: {0}", ret.CreationDate);
                    Console.WriteLine("    - Quantity: {0}", ret.Quantity);
                    Console.WriteLine("    - Reason: {0}", ret.Reason);
                    Console.WriteLine("    - Reason text: {0}", ret.ReasonText);
                }
            }
        }

        private static void insertOrderInfo(OrderLineItem item, Order order)
        {


            SqlConnection objConn = new SqlConnection();
            SqlCommand objCmd = new SqlCommand();

            try
            {
                objConn.ConnectionString = "Data Source=cat_db1; Initial Catalog=abookprod; Persist Security Info=True; User ID=ssis; Password=passw0rd; application name=eCampus";
                objConn.Open();

                objCmd.Connection = objConn;
                objCmd.CommandText = "uspIns_tblGoogleOrderDump";
                objCmd.CommandType = CommandType.StoredProcedure;
                objCmd.CommandTimeout = 100;

                objCmd.Parameters.Add("@order_Id", SqlDbType.VarChar, 100).Value = order.Id;
                objCmd.Parameters.Add("@order_Status", SqlDbType.VarChar, 100).Value = order.Status;
                objCmd.Parameters.Add("@order_MerchantId", SqlDbType.VarChar, 100).Value = order.MerchantId;
                objCmd.Parameters.Add("@order_MerchantOrderId", SqlDbType.VarChar, 100).Value = order.MerchantOrderId;
                objCmd.Parameters.Add("@order_Customer_FullName", SqlDbType.VarChar, 100).Value = order.Customer.FullName;
                objCmd.Parameters.Add("@order_PlacedDate", SqlDbType.VarChar, 100).Value = order.PlacedDate;
                objCmd.Parameters.Add("@order_NetPriceAmount_Value", SqlDbType.VarChar, 100).Value = order.NetPriceAmount.Value;
                objCmd.Parameters.Add("@order_NetPriceAmount_Currency", SqlDbType.VarChar, 100).Value = order.NetPriceAmount.Currency;
                objCmd.Parameters.Add("@order_PaymentStatus", SqlDbType.VarChar, 100).Value = order.PaymentStatus;
                objCmd.Parameters.Add("@order_ShippingCost_Value", SqlDbType.VarChar, 100).Value = order.ShippingCost.Value;
                objCmd.Parameters.Add("@order_ShippingCost_Currency", SqlDbType.VarChar, 100).Value = order.ShippingCost.Currency;
                objCmd.Parameters.Add("@order_ShippingCostTax_Value", SqlDbType.VarChar, 100).Value = order.ShippingCostTax.Value;
                objCmd.Parameters.Add("@order_ShippingCostTax_Currency", SqlDbType.VarChar, 100).Value = order.ShippingCostTax.Currency;
                objCmd.Parameters.Add("@item_Id", SqlDbType.VarChar, 100).Value = item.Id;
                objCmd.Parameters.Add("@item_Product_Id", SqlDbType.VarChar, 100).Value = item.Product.Id;
                objCmd.Parameters.Add("@item_Product_Title", SqlDbType.VarChar, 100).Value = item.Product.Title;
                objCmd.Parameters.Add("@item_Price_Value", SqlDbType.VarChar, 100).Value = item.Price.Value;
                objCmd.Parameters.Add("@item_Price_Currency", SqlDbType.VarChar, 100).Value = item.Price.Currency;
                objCmd.Parameters.Add("@item_Tax_Value", SqlDbType.VarChar, 100).Value = item.Tax.Value;
                objCmd.Parameters.Add("@item_Tax_Currency", SqlDbType.VarChar, 100).Value = item.Tax.Currency;
                objCmd.Parameters.Add("@item_ShippingDetails_ShipByDate", SqlDbType.VarChar, 100).Value = item.ShippingDetails.ShipByDate;
                objCmd.Parameters.Add("@item_ShippingDetails_DeliverByDate", SqlDbType.VarChar, 100).Value = item.ShippingDetails.DeliverByDate;
                objCmd.Parameters.Add("@item_ShippingDetails_Method_Carrier", SqlDbType.VarChar, 100).Value = item.ShippingDetails.Method.Carrier;
                objCmd.Parameters.Add("@item_ShippingDetails_Method_MethodName", SqlDbType.VarChar, 100).Value = item.ShippingDetails.Method.MethodName;
                objCmd.Parameters.Add("@item_ShippingDetails_Method_MinDaysInTransit", SqlDbType.VarChar, 100).Value = item.ShippingDetails.Method.MinDaysInTransit;
                objCmd.Parameters.Add("@item_ShippingDetails_Method_MaxDaysInTransit", SqlDbType.VarChar, 100).Value = item.ShippingDetails.Method.MaxDaysInTransit;
                objCmd.Parameters.Add("@item_QuantityOrdered", SqlDbType.VarChar, 100).Value = item.QuantityOrdered;
                objCmd.Parameters.Add("@Shipping_Address", SqlDbType.VarChar, 100).Value = order.BillingAddress.StreetAddress[0];
                objCmd.Parameters.Add("@Shipping_Address2", SqlDbType.VarChar, 100).Value = order.BillingAddress.StreetAddress[1];
                objCmd.Parameters.Add("@Shipping_City", SqlDbType.VarChar, 100).Value = order.BillingAddress.Locality;
                objCmd.Parameters.Add("@Shipping_State", SqlDbType.VarChar, 100).Value = order.BillingAddress.Region;
                objCmd.Parameters.Add("@Shipping_Zip", SqlDbType.VarChar, 100).Value = order.BillingAddress.PostalCode;
                objCmd.Parameters.Add("@Shipping_Country", SqlDbType.VarChar, 100).Value = order.BillingAddress.Country;



                objCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //sendEmail(" Error in insertOrderInfo()\r\n\r\n" + ex.ToString(), ConfigurationManager.AppSettings["EmailFrom"].ToString(), ConfigurationManager.AppSettings["EmailTo"].ToString(), "Error in insertOrderInfo()");
            }
            finally
            {
                if (objConn != null)
                    objConn.Close();

                objCmd.Dispose();
                objConn.Dispose();

                objCmd = null;
                objConn = null;
            }

        }
    }
}

