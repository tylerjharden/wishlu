using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTap.API;
using TwoTap.API.Impl;
using TwoTap.API.Models;
using TwoTap.Core;

namespace Squid.Products.TwoTap
{
    public static class Client
    {
        private static ITwoTapClient _client;
        private const string publicKey = "147b1eed7550448a6d435c98975322";
        private const string privateKey = "3a89a336d7f7e1f63ab8809db94b5e9e4762ff5e546a67c524";

        static Client()
        {
            _client = new TwoTapClient(publicKey, privateKey, TwoTapClient.eMode.LIVE);
        }

        public static AddToCartResponse AddToCart(Guid userId, List<string> products)
        {
            AddToCartRequest request = new AddToCartRequest();
            request.UserToken = userId.ToString();
            request.UserId = userId.ToString();
            request.Products = products;
            
            var response = _client.AddToCart(request);

            return response;
        }

        public static PurchaseConfirmResponse Confirm(string purchaseId)
        {
            PurchaseConfirmRequest request = new PurchaseConfirmRequest();
            request.PurchaseId = purchaseId;
            
            var response = _client.PurchaseConfirm(request);

            return response;
        }

        public static AddToCartStatusResponse GetCartStatus(string cartId)
        {
            AddToCartStatusRequest request = new AddToCartStatusRequest();
            request.CartId = cartId;
            
            var response = _client.AddToCartStatus(request);
            
            return response;
        }

        public static AddToCartStatusResponse GetCartStatus(Guid userId, String cartId)
        {
            AddToCartStatusRequest request = new AddToCartStatusRequest();
            request.CartId = cartId;
            request.UserToken = userId.ToString();
            request.UserId = userId.ToString();

            var response = _client.AddToCartStatus(request);

            return response;
        }

        public static PurchaseEstimatesResponse GetCartEstimates(string cartId, string siteId, string productId, string zip, string city, string state, string productUrl)
        {
            PurchaseEstimatesRequest request = new PurchaseEstimatesRequest();
            request.CartId = cartId;

            request.FieldsInput = new Dictionary<string, PurchaseEstimatesRequest.FieldInputData>();

            PurchaseEstimatesRequest.FieldInputData data = new PurchaseEstimatesRequest.FieldInputData();
            data.AddToCart = new Dictionary<string, Dictionary<string, string>>();
            data.AddToCart.Add(productId, new Dictionary<string, string>());

            data.NoauthCheckout = new Dictionary<string, string>();
            data.NoauthCheckout.Add("shipping_zip", zip);
            data.NoauthCheckout.Add("shipping_city", city);
            data.NoauthCheckout.Add("shipping_state", state);
                        
            request.FieldsInput.Add(siteId, data);

            //request.Products = new List<string>();
            //request.Products.Add(productUrl);

            var response = _client.PurchaseEstimates(request);

            return response;
        }

        public static PurchaseEstimatesResponse GetProductEstimates(List<string> products)
        {
            PurchaseEstimatesRequest request = new PurchaseEstimatesRequest();
            request.Products = products;

            var response = _client.PurchaseEstimates(request);

            return response;
        }
    }
}
