using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TicketFlow.BLL.Services.Interfaces;

namespace TicketFlow.BLL.Services.Classes
{
    public class PhoneVerificationService : IPhoneVerificationService
    {
        private readonly IHostEnvironment _env;
        public PhoneVerificationService(IConfiguration configuration, IHostEnvironment env)
        {
            _env = env;
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialPath = configuration["Firebase:ServiceAccountKeyPath"];
                if (!string.IsNullOrEmpty(credentialPath) && System.IO.File.Exists(credentialPath))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialPath)
                    });
                }
            }
        }

        public async Task<(bool IsValid, string? PhoneNumber)> VerifyFirebaseTokenAsync(string firebaseIdToken)
        {
            if (_env.IsDevelopment() && firebaseIdToken == "123456")
            {
                return (true, "+201234567890");
            }
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                    return (false, null);

                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(firebaseIdToken);

                if (decodedToken.Claims.TryGetValue("phone_number", out var phoneNumberObj) && phoneNumberObj is string phoneNumber)
                {
                    return (true, phoneNumber);
                }

                return (false, null);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}