using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Context
{
    public class ControllerServicesContext
    {
        public IWebHostEnvironment HostingEnvironment { get; }
        public IMapper Mapper { get; }
        public IEmailService EmailService { get; }
        public LinkGenerator LinkGenerator { get; }
        public AppSettings AppSettings { get; }

        public ControllerServicesContext(IWebHostEnvironment hostingEnvironment, IMapper mapper, IEmailService emailService, LinkGenerator linkGenerator, AppSettings appSettings)
        {
            HostingEnvironment = hostingEnvironment;
            Mapper = mapper;
            EmailService = emailService;
            LinkGenerator = linkGenerator;
            AppSettings = appSettings;
        }
    }
}
