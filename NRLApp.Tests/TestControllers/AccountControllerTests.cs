using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using NRLApp.Controllers;
using NRLApp.Models.Account;

// Alias for å unngå konflikt mellom MVC sin SignInResult og Identity sin
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace NRLApp.Tests
{
	public class AccountControllerTests
	{
		private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
		private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
		private readonly AccountController _controller;

		public AccountControllerTests()
		{
			// UserManager<IdentityUser>-mock
			_userManagerMock = new Mock<UserManager<IdentityUser>>(
				Mock.Of<IUserStore<IdentityUser>>(),
				null,   // IOptions<IdentityOptions>
				null,   // IPasswordHasher<IdentityUser>
				null,   // IEnumerable<IUserValidator<IdentityUser>>
				null,   // IEnumerable<IPasswordValidator<IdentityUser>>
				null,   // ILookupNormalizer
				null,   // IdentityErrorDescriber
				null,   // IServiceProvider
				null    // ILogger<UserManager<IdentityUser>>
			);

			// SignInManager<IdentityUser>-mock
			_signInManagerMock = new Mock<SignInManager<IdentityUser>>(
				_userManagerMock.Object,
				Mock.Of<IHttpContextAccessor>(),
				Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
				null,   // IOptions<IdentityOptions>
				null,   // ILogger<SignInManager<IdentityUser>>
				null,   // IAuthenticationSchemeProvider
				null    // IUserConfirmation<IdentityUser>
			);

			// Kontroller vi tester
			_controller = new AccountController(
				_userManagerMock.Object,
				_signInManagerMock.Object
			);
		}

		[Fact]
		public void Login_Get_Returns_View_With_LoginViewModel()
		{
			// Act
			var result = _controller.Login();

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.IsType<LoginViewModel>(viewResult.Model);
		}

		[Fact]
		public async Task Login_Post_Success_Redirects_To_Area_Obstacle()
		{
			// Arrange
			var model = new LoginViewModel
			{
				Email = "test@test.com",
				Password = "Password1!",
				RememberMe = false
			};

			var user = new IdentityUser
			{
				UserName = model.Email,
				Email = model.Email
			};

			_signInManagerMock
				.Setup(x => x.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					true))
				.ReturnsAsync(SignInResult.Success);

			_userManagerMock
				.Setup(x => x.FindByNameAsync(model.Email))
				.ReturnsAsync(user);

			// Ingen av rollene Admin/Approver
			_userManagerMock
				.Setup(x => x.IsInRoleAsync(user, "Admin"))
				.ReturnsAsync(false);
			_userManagerMock
				.Setup(x => x.IsInRoleAsync(user, "Approver"))
				.ReturnsAsync(false);

			// Act
			var result = await _controller.Login(model) as RedirectToActionResult;

			// Assert
			Assert.NotNull(result);
			Assert.Equal("Area", result!.ActionName);
			Assert.Equal("Obstacle", result.ControllerName);
		}

		[Fact]
		public async Task Login_Post_Failed_Returns_View_With_ModelError()
		{
			// Arrange
			var model = new LoginViewModel
			{
				Email = "test@test.com",
				Password = "WrongPassword",
				RememberMe = false
			};

			_signInManagerMock
				.Setup(x => x.PasswordSignInAsync(
					model.Email,
					model.Password,
					model.RememberMe,
					true))
				.ReturnsAsync(SignInResult.Failed);

			// Act
			var result = await _controller.Login(model);

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Same(model, viewResult.Model);
			Assert.False(_controller.ModelState.IsValid); // forventer feilmelding
		}

		[Fact]
		public async Task Logout_Post_Signs_Out_And_Redirects_To_Login()
		{
			// Arrange
			_signInManagerMock
				.Setup(x => x.SignOutAsync())
				.Returns(Task.CompletedTask)
				.Verifiable();

			// Act
			var result = await _controller.Logout() as RedirectToActionResult;

			// Assert
			_signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);

			Assert.NotNull(result);
			Assert.Equal("Login", result!.ActionName);
			Assert.Equal("Account", result.ControllerName);
		}
	}
}
