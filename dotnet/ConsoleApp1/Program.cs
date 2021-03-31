using System;
using Google.Apis.ShoppingContent.v2_1;
using Google.Apis.Services;
using Google.Apis.ShoppingContent.v2_1.Data;
using System.Collections.Generic;
using CommandLine;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            static void ListAllUnacknowledgedOrders(ulong merchantId)
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
                            Console.WriteLine("Here.");
                            //PrintOrder(order);
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
}
