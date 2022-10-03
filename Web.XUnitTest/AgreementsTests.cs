using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Web.API.Controllers;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Concrete;
using Web.Services.Interfaces;
using Xunit;

namespace Web.XUnitTest
{
    public class AgreementsTests
    {
        private readonly AgreementsController _controller;
        private readonly IAgreementsService _service;
        private readonly IGenericRepository<Agreement> _agreementsRepo;

        public AgreementsTests()
        {
        }

        private static AgreementsFilterVM filters = new AgreementsFilterVM
        {
            AgentId = 0,
            OperatorId = 0,
            SearchString = "",
            ServiceId = 0,
            NotConfirmed = false,

        };

        [Fact]
        public void GetAgreements_ResultIsBaseResponse()
        {

            var result = _controller.GetAgreements(filters);
            Assert.IsType<BaseResponse>(result.Body);
        }

        [Fact]
        public void GetAgreements_ReturnList()
        {
            
            var result = _controller.GetAgreements(filters);
            Assert.IsType<List<AgreementVM>>(result.Body);

        }

       

    }
    //public class ShoppingCartControllerTest
    //{
    //    private readonly ShoppingCartController _controller;
    //    private readonly IShoppingCartService _service;

    //    public ShoppingCartControllerTest()
    //    {
    //        _service = new ShoppingCartServiceFake();
    //        _controller = new ShoppingCartController(_service);
    //    }

    //    [Fact]
    //    public void Get_WhenCalled_ReturnsOkResult()
    //    {
    //        // Act
    //        var okResult = _controller.Get();

    //        // Assert
    //        Assert.IsType<OkObjectResult>(okResult as OkObjectResult);
    //    }

    //    [Fact]
    //    public void Get_WhenCalled_ReturnsAllItems()
    //    {
    //        // Act
    //        var okResult = _controller.Get() as OkObjectResult;

    //        // Assert
    //        var items = Assert.IsType<List<ShoppingItem>>(okResult.Value);
    //        Assert.Equal(3, items.Count);
    //    }
    //}
    //[Fact]
    //public void GetById_UnknownGuidPassed_ReturnsNotFoundResult()
    //{
    //    // Act
    //    var notFoundResult = _controller.Get(Guid.NewGuid());
    //    // Assert
    //    Assert.IsType<NotFoundResult>(notFoundResult);
    //}
    //[Fact]
    //public void GetById_ExistingGuidPassed_ReturnsOkResult()
    //{
    //    // Arrange
    //    var testGuid = new Guid("ab2bd817-98cd-4cf3-a80a-53ea0cd9c200");
    //    // Act
    //    var okResult = _controller.Get(testGuid);
    //    // Assert
    //    Assert.IsType<OkObjectResult>(okResult as OkObjectResult);
    //}
    //[Fact]
    //public void GetById_ExistingGuidPassed_ReturnsRightItem()
    //{
    //    // Arrange
    //    var testGuid = new Guid("ab2bd817-98cd-4cf3-a80a-53ea0cd9c200");
    //    // Act
    //    var okResult = _controller.Get(testGuid) as OkObjectResult;
    //    // Assert
    //    Assert.IsType<ShoppingItem>(okResult.Value);
    //    Assert.Equal(testGuid, (okResult.Value as ShoppingItem).Id);
    //}
}
