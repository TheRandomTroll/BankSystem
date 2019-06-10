using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();

            this.ConfigureDatabase();
        }

        private void ConfigureDatabase()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = ".\\SQLEXPRESS",
                InitialCatalog = "master",
                UserID = "SA",
            };

            builder["Trusted_Connection"] = true;

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction = null;

                try
                {
                    command.CommandText = "IF NOT EXISTS(SELECT * FROM sys.databases WHERE Name = 'BankSystem') CREATE DATABASE BankSystem";
                    command.ExecuteNonQuery();

                    transaction = connection.BeginTransaction("SampleTransaction");
                    command.Transaction = transaction;

                    StringBuilder sb = new StringBuilder("USE BankSystem;");
                    sb.Append("CREATE TABLE Banks(");
                    sb.Append(" Id INT IDENTITY(1000,1) NOT NULL PRIMARY KEY, ");
                    sb.Append(" Name NVARCHAR(50), ");
                    sb.Append(" Address NVARCHAR(50), ");
                    sb.Append(" City NVARCHAR(30)");
                    sb.Append("); ");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    sb.Clear();
                    sb.Append("CREATE TABLE Users(");
                    sb.Append(" Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, ");
                    sb.Append(" Username NVARCHAR(50), ");
                    sb.Append(" Email NVARCHAR(50),");
                    sb.Append(" Password NVARCHAR(1000), ");
                    sb.Append("); ");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    sb.Clear();
                    sb.Append("CREATE TABLE AccountTypes(");
                    sb.Append(" Id INT IDENTITY(100,1) NOT NULL PRIMARY KEY, ");
                    sb.Append(" Description NVARCHAR(50), ");
                    sb.Append(" InterestRate NUMERIC(4,2), ");
                    sb.Append("); ");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    sb.Clear();
                    sb.Append("CREATE TABLE Accounts(");
                    sb.Append(" Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, ");
                    sb.Append(" BankId INT FOREIGN KEY REFERENCES Banks(Id), ");
                    sb.Append(" UserId INT FOREIGN KEY REFERENCES Users(Id), ");
                    sb.Append(" AccountTypeId INT FOREIGN KEY REFERENCES AccountTypes(Id), ");
                    sb.Append(" Balance NUMERIC(10,2)");
                    sb.Append("); ");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    sb.Clear();
                    sb.Append("CREATE TABLE Transactions(");
                    sb.Append(" Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, ");
                    sb.Append(" AccountId INT FOREIGN KEY REFERENCES Accounts(Id), ");
                    sb.Append(" EventTime DATETIME,");
                    sb.Append(" TransactionType NVARCHAR(15),");
                    sb.Append(" Amount NUMERIC(10,2)");
                    sb.Append("); ");

                    command.CommandText = sb.ToString();
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
        }
    }
}
