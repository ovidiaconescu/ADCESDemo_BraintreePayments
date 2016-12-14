using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Braintree;

namespace WebApplicationBasic.Controllers
{
    public class HomeController : Controller
    {
        BraintreeGateway gateway = new BraintreeGateway
        {
            Environment = Braintree.Environment.SANDBOX,
            MerchantId = "...",
            PublicKey = "...",
            PrivateKey = "..."
        };

        public IActionResult Index()
        {
            var token = gateway.ClientToken.generate();
            Console.WriteLine(token);
            return View(model: token);
        }

        [HttpPost("Purchase")]
        public IActionResult Purchase([FromBody] NonceModel model)
        {
            var request = new TransactionRequest
            {
                Amount = 15.00M,
                PaymentMethodNonce = model.nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);

            Transaction transaction = result.Target;
            TransactionStatus status = transaction.Status;
            
            return Json(Url.Action("About", new { message = status.ToString() }));

        }

        public IActionResult About(string message = "")
        {
            ViewData["Message"] = message;
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "webhooks";
            return View(Program.messages);
        }

        public IActionResult Error()
        {
            return View();
        }
    }

    public class NonceModel
    {
        public string nonce {get;set;}
    }
}
