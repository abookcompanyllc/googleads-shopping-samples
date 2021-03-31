using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.ShoppingContent.v2_1;
using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2_1.Data;
using CommandLine;

namespace GoogleShoppingApi
{
    class Program
    {
        static void Main(string[] args)
        {
            ListAllUnacknowledgedOrders(merchantId);
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
    }
}
