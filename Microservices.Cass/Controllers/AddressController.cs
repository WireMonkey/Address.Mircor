﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microservices.Cass.Business;
using Microservices.Cass.Poco;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Veritas.Logging.Aop;

namespace Microservices.Cass.Controllers
{
    [Produces("application/json")]
    [Route("api/Address")]
    public class AddressController : Controller
    {
        public static IConfiguration Configuration { get; set; }
        private readonly ILogger<AddressController> _logger;
        private object description;

        public AddressController(ILogger<AddressController> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            description = new List<object>
            {
                new
                {
                    Verb = "GET",
                    Info = "Processes single address.",
                    Input = "?address1=&address2&city=&state=&zip=",
                    Returns = new AddressReturn()
                },
                new
                {
                    Verb = "POST",
                    Info = "Processes multiple addresses.",
                    Input = new List<AddressInfo>
                    {
                        new AddressInfo(),
                        new AddressInfo()

                    },
                    Returns = new List<AddressReturn>
                    {
                        new AddressReturn(),
                        new AddressReturn()
                    }
                }
            };

        }

        /// <summary>
        /// Process a sinlge address
        /// </summary>
        /// <param name="address1">address1</param>
        /// <param name="address2">address2</param>
        /// <param name="city">city</param>
        /// <param name="state">state</param>
        /// <param name="zip">zip</param>
        /// <returns>{"address1":null,"address2":null,"address3":null,"city":null,"state":null,"zip":null,"zip4Code":null,"cassCode":null,"validAddress":false,"errorMessage":null}</returns>
        [LoggingAspect]
        [HttpGet]
        public ActionResult<AddressReturn> CheckValidAddress(string address1, string address2, string city, string state, string zip)
        {
            try
            {
                var address = new AddressInfo
                {
                    Address1 = address1,
                    Address2 = address2,
                    City = city,
                    State = state,
                    Zip = zip
                };

                var addressProcessing = new AddressProcessing(Configuration["Smarty:userName"], Configuration["Smarty:password"]);
                return StatusCode(200, addressProcessing.ProcessAddress(address));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Process many addresses in parallel. Returns a list of addresses
        /// </summary>
        /// <param name="addresssList">[{"address1":null,"address2":null,"address3":null,"city":null,"state":null,"zip":null}]</param>
        /// <returns>[{"address1":null,"address2":null,"address3":null,"city":null,"state":null,"zip":null,"zip4Code":null,"cassCode":null,"validAddress":false,"errorMessage":null}]</returns>
        [LoggingAspect]
        [HttpPost]
        public ActionResult<List<AddressReturn>> CheckManyAddresses([FromBody]List<AddressInfo> addresssList)
        {
            try
            {
                var addressProcessing = new AddressProcessing(Configuration["Smarty:userName"], Configuration["Smarty:password"]);
                var returnList = new ConcurrentBag<AddressReturn>();
                Parallel.ForEach(addresssList, (address) =>
                {
                    var addAddress = new AddressReturn();
                    try
                    {
                        addAddress = addressProcessing.ProcessAddress(address);
                    }
                    catch (Exception e)
                    {
                        addAddress.ValidAddress = false;
                        addAddress.errorMessage = e.Message;
                    }
                    returnList.Add(addAddress);
                });

                return StatusCode(200, returnList);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Get description of the end points
        /// </summary>
        /// <returns></returns>
        [LoggingAspect]
        [HttpOptions]
        public IActionResult GetEndPointDetails()
        {
            return StatusCode(200, description);
        }

        /// <summary>
        /// Get description of the end points
        /// </summary>
        /// <returns></returns>
        [LoggingAspect]
        [HttpGet]
        [Route("Help")]
        public IActionResult HelpPage()
        {
            return StatusCode(200, description);
        }
    }
}