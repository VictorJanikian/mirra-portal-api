﻿namespace Mirra_Portal_API.Services.Interfaces
{
    public interface IEmailService
    {
        public Task SendActivationCode(string recipientEmail, string code);
        public Task ActivateEmail(string email, string code);
    }
}
