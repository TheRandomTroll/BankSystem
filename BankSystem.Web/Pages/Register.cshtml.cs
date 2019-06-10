using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

namespace BankSystem.Web.Pages
{
    public class RegisterModel : PageModel
    {
        public class InputModel
        {
            [Required]
            public string Username { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
            if (this.ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(UtilityMethods.GenerateConnectionString()))
                {
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        command.CommandText = "USE BankSystem; INSERT INTO Users(Username, Email, Password) VALUES(@Username, @Email, @Password)";
                        command.Transaction = transaction;

                        command.Parameters.Add("@Username", SqlDbType.NVarChar);
                        command.Parameters["@Username"].Value = Input.Username;
                        command.Parameters.Add("@Email", SqlDbType.NVarChar);
                        command.Parameters["@Email"].Value = Input.Email;
                        command.Parameters.Add("@Password", SqlDbType.NVarChar);
                        command.Parameters["@Password"].Value = Input.Password;

                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                        Console.WriteLine("  Message: {0}", ex.Message);
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                            Console.WriteLine("  Message: {0}", ex2.Message);
                        }
                    }
                    connection.Close();
                }

                return LocalRedirect("/");
            }

            return this.Page();
        }
    }
}