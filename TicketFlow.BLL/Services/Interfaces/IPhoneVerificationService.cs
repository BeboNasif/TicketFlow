using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketFlow.BLL.Services.Interfaces
{
    public interface IPhoneVerificationService
    {
        Task<(bool IsValid, string? PhoneNumber)> VerifyFirebaseTokenAsync(string firebaseIdToken);
    }
}