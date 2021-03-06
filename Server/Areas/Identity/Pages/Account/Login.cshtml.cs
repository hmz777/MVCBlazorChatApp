using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCBlazorChatApp.Server.Helpers.StatusMessages;
using MVCBlazorChatApp.Server.Models;
using MVCBlazorChatApp.Shared.Models;

namespace MVCBlazorChatApp.Server.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public LoginDto LoginDto { get; set; } = new LoginDto();

        public void OnGet()
        {
            ViewData["Account"] = true;
        }

        public async Task<IActionResult> OnPost(LoginDto loginDto, string returnUrl = null)
        {
            //Short circuit the process temporarily
            return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Accounts feature is not available yet." });

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Invalid login." });

            string UsernameOrEmail = null;
            if (loginDto.UsernameOrEmail.Contains('@'))
            {
                try
                {
                    _ = new MailAddress(UsernameOrEmail);

                    ApplicationUser user = await userManager.FindByEmailAsync(UsernameOrEmail);

                    if (user != null)
                    {
                        //Email
                        UsernameOrEmail = user.UserName;
                    }
                }
                catch
                {
                    return new BadRequestObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Invalid email." });
                }
            }
            else
            {
                //Username
                UsernameOrEmail = loginDto.UsernameOrEmail;
            }

            var signInResult = await signInManager.PasswordSignInAsync(UsernameOrEmail, loginDto.Password, loginDto.RememberMe, true);

            if (signInResult.Succeeded)
            {
                returnUrl = (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl)) ? Url.PageLink("/Index") : returnUrl;

                return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Success, Message = "Sign in successful.", Link = returnUrl });
            }
            else if (signInResult.IsNotAllowed)
            {
                TempData.Put<string>("Username", UsernameOrEmail);
                return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Sign in not allowed, account needs activation.", Link = Url.PageLink("ConfirmAccount") });
            }
            else if (signInResult.IsLockedOut)
            {
                return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Too many sign in attempts, user locked out." });
            }
            else
            {
                return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Invalid username or password." });
            }
        }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Username or email")]
        public string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
