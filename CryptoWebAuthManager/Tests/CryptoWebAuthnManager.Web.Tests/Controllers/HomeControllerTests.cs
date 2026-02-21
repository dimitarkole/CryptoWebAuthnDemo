using CryptoWebAuthnManager.Web.Controllers;
using MyTested.AspNetCore.Mvc;
using Xunit;

namespace CryptoWebAuthnManager.Web.Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public void IndexShouldReturnView()
        {
            MyController<HomeController>
                .Instance()
                .Calling(c => c.Index())
                .ShouldReturn()
                .View();
        }
    }
}
