## ADCESDemo_BraintreePayments
## Charging Real $Money$ Every Month

**demo live for another month on: http://adcesdemo2.azurewebsites.net/**

## Walkthrough

mkdir ADCES
cd ADCES
yo aspnet
--- Web Application Basic [without Membership and Authorization]
--- Bootstrap
cd "WebApplicationBasic"
dotnet restore
dotnet build 
dotnet run

—> SAFARI: go to http://localhost:5000
—> SAFARI: go to https://www.braintreepayments.com/sandbox
?create new account
?activate account

—> SAFARI: go to https://developers.braintreepayments.com/
how it works!
—> SAFARI: go to https://developers.braintreepayments.com/start/overview


**set up client**
https://developers.braintreepayments.com/start/hello-client/javascript/v3
go to hosted fields
https://developers.braintreepayments.com/guides/hosted-fields/overview/javascript/v3

**In VS Code**
Views / Home / Index.cshtml - remove the default carousel and div-row
add the html from the hosted fields integration 
add the CSS to the wwwroot/css/site.css
add the JS to the <script type="text/javascript"></script> on the same page
if the terminal was running, just hit refresh and see it working. 
Put some data in and see the default alert('Submit your nonce to your server here!');

---
**next part in C#, we're building our server side API**
https://developers.braintreepayments.com/start/hello-server/dotnet

in packages.json add the braintree nuget: "Braintree": "3.3.0"

in HomeController.cs change the index to:
(--watch the named parameter in the view--)

        public IActionResult Index()
        {
            var gateway = new BraintreeGateway
              {
                  Environment = Braintree.Environment.SANDBOX,
                  MerchantId = "...",
                  PublicKey = "...",
                  PrivateKey = "..."
              };
            
            var token = gateway.ClientToken.generate();
            Console.WriteLine(token);

            return View(model:token.ToString());
        }


--> Sandbox - account - myUser - scroll down - API Keys, Tokenization Keys, Encryption Keys

**On Index.cshtml add**
@model string
**and replace the authorization in our script**
var authorization = '@Model';

**change the end with this**
}, function (err, hostedFieldsInstance) {
    if (err) {
      console.error(err);
      return;
    }

    submit.removeAttribute('disabled');

    form.addEventListener('submit', function (event) {
      event.preventDefault();

      hostedFieldsInstance.tokenize(function (tokenizeErr, payload) {
        if (tokenizeErr) {
          console.error(tokenizeErr);
          return;
        }

        $.ajax({
              type: "POST",
              url: '@Url.Action("Purchase", "Home")',
              contentType: "application/json",
              data: JSON.stringify({ nonce: payload.nonce }),
              dataType: "json",
              success: function (data) {
                  window.location.href = data;
              }
          });
      });
    }, false);
  });
}
</script>


**final version of the HomeController.cs**
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
            return View(model:token);
        }


        [HttpPost("Purchase")]
        public IActionResult Purchase([FromBody] NonceModel model)
        {
            var request = new TransactionRequest
            {
                Amount = 10.00M,
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
——-

    public class NonceModel
    {
        public string nonce { get; set; }
    }

**Everything should work! We’re creating the webhooks.**

—————
https://developers.braintreepayments.com/guides/recurring-billing/overview

**Now onto the web hook**

**First we need a place to store messages, let’s hack a list in program.cs**
public static List<string> messages = new List<string>();

**Then retrieve the messages, in HomeController.cs**
public IActionResult Contact()
        {
            ViewData["Message"] = "Webhooks";
            return View(Program.messages);
        }


**And show them in the view Contact.cshtml**
@model List<string>

@{
    ViewData["Title"] = "Contact";
}
<h2>@ViewData["Title"].</h2>
<h3>@ViewData["Message"]</h3>

@foreach (var item in Model)
{
    <div>@item</div>
}


——-

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
		Program.messages.Add(“test”);
            return Ok();
        }
    }
}


**after testing in POSTMAN that the HttpPost works in the API, let’s link it to Braintree, push to Azure and restest**

        BraintreeGateway gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = "...",
                PublicKey = "...",
                PrivateKey = "..."
            };


        var webhook
            = gateway.WebhookNotification.Parse(Request.Form["bt_signature"], Request.Form["bt_payload"]);

        string message = $"Webhook Received [{webhook.Timestamp.Value}] | Kind: [{webhook.Kind}]";

        Program.messages.Add(message);

