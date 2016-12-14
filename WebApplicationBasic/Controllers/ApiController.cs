using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Braintree;

namespace WebApplicationBasic.Controllers
{
    public class ApiController : Controller
    {
        [HttpPost]
        public IActionResult Hooked()
        {
             BraintreeGateway gateway = new BraintreeGateway
                {
                    Environment = Braintree.Environment.SANDBOX,
                    MerchantId = "",
                    PublicKey = "",
                    PrivateKey = ""
                };


            var webhook
                = gateway.WebhookNotification.Parse(Request.Form["bt_signature"], Request.Form["bt_payload"]);

            string message = $"Webhook Received [{webhook.Timestamp.Value}] | Kind: [{webhook.Kind}]";

            Program.messages.Add(message);

            return Ok();
        }
    }
}
