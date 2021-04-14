using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ESA_AuthenicationB2C.Controllers
{
    public class InvitationController : Controller
    {
        private readonly IConfiguration _configuration;

        public InvitationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Separate SignUp handler for invitation links sent by email
        [AllowAnonymous]
        public IActionResult Redeem(string id_token_hint)
        {
            var invite_auth = new AuthenticationProperties { RedirectUri = "/" };
            invite_auth.Items.Add("id_token_hint", id_token_hint);

            string invite_policy = _configuration.GetSection("AzureAdB2C")["InvitationPolicyId"];

            return this.Challenge(invite_auth, invite_policy);
        }
    }
}