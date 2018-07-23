using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microservices.Cass.Business;
using Microservices.Cass.Poco;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microservices.Cass.Controllers
{
    [Produces("application/json")]
    [Route("api/Address")]
    public class AddressController : Controller
    {
        public static IConfiguration Configuration { get; set; }

        public AddressController()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        /// <summary>
        /// Process a sinlge address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CheckValidAddress(string address1, string address2,string city, string state, string zip)
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
        /// Process many addresses in parallel
        /// </summary>
        /// <param name="addresssList"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CheckManyAddresses([FromBody]List<AddressInfo> addresssList)
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
        [HttpOptions]
        public IActionResult GetEndPointDetails()
        {
            var squigleOpen = "{";
            var squigleClose = "}";
            var addressReturns = new List<AddressReturn>
            {
                new AddressReturn(),
                new AddressReturn()
            };
            var addressList = new List<AddressInfo>
            {
                new AddressInfo(),
                new AddressInfo()
            };

            var returnJson = JsonConvert.DeserializeObject(
                $"[{squigleOpen}\"Verb\":\"GET\",\"Info\":\"Processes single Address\",\"Input\":\"?address1=&address2&city=&state=&zip=\",\"Return\":{JsonConvert.SerializeObject(new AddressReturn())}{squigleClose},{squigleOpen}\"Verb\":\"POST\",\"Info\":\"Processes Multiple Address\",\"Input\":{JsonConvert.SerializeObject(addressList)},\"Return\":{JsonConvert.SerializeObject(addressReturns)}{squigleClose}]");
            return StatusCode(200,returnJson);
        }
    }
}