using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.Mvc.Extensions.Users
{
    public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public EmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<EmailConfirmationTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<TUser>> logger) : base(dataProtectionProvider, options, logger)
        {

        }
    }

    //.NET Core 2.2
    //public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    //{
    //    public EmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
    //        IOptions<EmailConfirmationTokenProviderOptions> options) : base(dataProtectionProvider, options)
    //    {

    //    }
    //}

    public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {

    }
}
