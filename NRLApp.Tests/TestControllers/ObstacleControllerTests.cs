using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using NRLApp.Controllers;
using NRLApp.Models;
using NRLApp.Models.Obstacles;

namespace NRLApp.Tests.TestControllers
{
	public class ObstacleControllerTests
	{
		private static ObstacleController CreateController(
			bool isAdmin = false,
			string? drawStateJson = null)
		{
			// Mock IConfiguration
			var configMock = new Mock<IConfiguration>();

			var controller = new ObstacleController(configMock.Object);

			// Fake HttpContext + bruker
			var httpContext = new DefaultHttpContext();
			var identity = new ClaimsIdentity();

			if (isAdmin)
			{
				identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
			}

			identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "user-1"));
			httpContext.User = new ClaimsPrincipal(identity);

			// TempData
			var tempDataProvider = new Mock<ITempDataProvider>();
			var tempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = httpContext
			};
			controller.TempData = tempData;

			// Sett [TempData]-property direkte (DrawJson)
			if (drawStateJson != null)
			{
				controller.DrawJson = drawStateJson;
			}

			return controller;
		}

		// ---------------------------------------------------------------------
		// 1) AREA GET
		// ---------------------------------------------------------------------

		[Fact]
		public void Area_Get_Admin_IsRedirectedToAdminUsers()
		{
			var controller = CreateController(isAdmin: true);

			var result = controller.Area();

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("Users", redirect.ActionName);
			Assert.Equal("Admin", redirect.ControllerName);
		}

		[Fact]
		public void Area_Get_NonAdmin_ReturnsView()
		{
			var controller = CreateController(isAdmin: false);

			var result = controller.Area();

			Assert.IsType<ViewResult>(result);
		}

		// ---------------------------------------------------------------------
		// 2) AREA POST
		// ---------------------------------------------------------------------

		[Fact]
		public void Area_Post_NoGeoJson_SetsError_AndRedirectsBackToArea()
		{
			var controller = CreateController();

			var result = controller.Area("");

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(ObstacleController.Area), redirect.ActionName);

			Assert.True(controller.TempData.ContainsKey("Error"));
		}

		[Fact]
		public void Area_Post_WithGeoJson_SavesToDrawJson_AndRedirectsToMeta()
		{
			var controller = CreateController();
			var json = """{"type":"Point"}""";

			var result = controller.Area(json);

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(ObstacleController.Meta), redirect.ActionName);

			Assert.NotNull(controller.DrawJson);
		}

		// ---------------------------------------------------------------------
		// 3) META GET
		// ---------------------------------------------------------------------

		[Fact]
		public void Meta_Get_WithoutGeoJson_RedirectsToArea()
		{
			var controller = CreateController(drawStateJson: null);

			var result = controller.Meta();

			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal(nameof(ObstacleController.Area), redirect.ActionName);
		}

		[Fact]
		public void Meta_Get_WithGeoJson_ReturnsViewWithEmptyViewModel()
		{
			// Mimic hva SaveDrawState lagrer: en DrawState med GeoJson
			var drawState = new DrawState
			{
				GeoJson = """{"type":"Polygon"}"""
			};
			var drawStateJson = JsonSerializer.Serialize(drawState);

			var controller = CreateController(drawStateJson: drawStateJson);

			var result = controller.Meta();

			var view = Assert.IsType<ViewResult>(result);
			Assert.IsType<ObstacleMetaVm>(view.Model);
		}

		// ---------------------------------------------------------------------
		// 4) THANKS VIEW
		// ---------------------------------------------------------------------

		[Fact]
		public void Thanks_SetsDraftFlagAndReturnsView()
		{
			var controller = CreateController();

			var result = controller.Thanks(draft: true);

			var view = Assert.IsType<ViewResult>(result);
			Assert.True((bool)controller.ViewBag.Draft);
		}

		// ---------------------------------------------------------------------
		// 5) EDIT (POST), valideringslogikk uten DB
		// ---------------------------------------------------------------------

		[Fact]
		public async Task Edit_Post_EmptyName_ReturnsViewWithModelError()
		{
			// Arrange
			var controller = CreateController();
			var vm = new ObstacleEditVm
			{
				Id = 1,
				ObstacleName = "",   // ugyldig
				HeightValue = 10,
				HeightUnit = "m",
				Description = "Test",
				SaveAsDraft = false
			};

			// Act
			var result = await controller.Edit(vm);

			// Assert
			var view = Assert.IsType<ViewResult>(result);
			var returnedVm = Assert.IsType<ObstacleEditVm>(view.Model);
			Assert.Same(vm, returnedVm);

			Assert.False(controller.ModelState.IsValid);
			Assert.True(controller.ModelState.ContainsKey(nameof(ObstacleEditVm.ObstacleName)));
		}

		[Fact]
		public async Task Edit_Post_NegativeHeight_ReturnsViewWithModelError()
		{
			// Arrange
			var controller = CreateController();
			var vm = new ObstacleEditVm
			{
				Id = 1,
				ObstacleName = "Tower",
				HeightValue = -1,    // ugyldig
				HeightUnit = "m",
				Description = "Test",
				SaveAsDraft = false
			};

			// Act
			var result = await controller.Edit(vm);

			// Assert
			var view = Assert.IsType<ViewResult>(result);
			Assert.False(controller.ModelState.IsValid);
			Assert.True(controller.ModelState.ContainsKey(nameof(ObstacleEditVm.HeightValue)));
		}

		[Fact]
		public async Task Edit_Post_HeightOver300_ReturnsViewWithModelError()
		{
			// Arrange
			var controller = CreateController();
			var vm = new ObstacleEditVm
			{
				Id = 1,
				ObstacleName = "Crazy tall mast",
				HeightValue = 400,   // for høyt
				HeightUnit = "m",
				Description = "Test",
				SaveAsDraft = false
			};

			// Act
			var result = await controller.Edit(vm);

			// Assert
			var view = Assert.IsType<ViewResult>(result);
			var returnedVm = Assert.IsType<ObstacleEditVm>(view.Model);
			Assert.Same(vm, returnedVm);

			Assert.False(controller.ModelState.IsValid);
			// Valideringen legges på HeightValue
			Assert.True(controller.ModelState.ContainsKey(nameof(ObstacleEditVm.HeightValue)));
		}
	}
}