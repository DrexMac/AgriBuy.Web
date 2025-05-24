using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Services
{
    public abstract class BaseService
    {
        protected readonly IMapper Mapper;
        protected readonly IConfiguration Configuration;
        protected readonly ILogger<BaseService> Logger;

        public BaseService(IConfiguration configuration, ILogger<BaseService> logger, IMapper mapper)
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
        }
    }
}
