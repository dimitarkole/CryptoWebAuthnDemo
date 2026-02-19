namespace CryptoWebAuthnManager.Web.Areas.Administration.Controllers
{
    using CryptoWebAuthnManager.Common;
    using CryptoWebAuthnManager.Web.Controllers;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
    [Area("Administration")]
    public class AdministrationController : BaseController
    {
    }
}
