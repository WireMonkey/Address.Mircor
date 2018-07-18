using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microservices.Cass.Business;
using Microservices.Cass.Poco;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Microservices.Cass.Controllers
{
    [Produces("application/json")]
    [Route("api/Address")]
    public class AddressController : Controller
    {
        /// <summary>
        /// Process a sinlge address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult CheckValidAddress(AddressInfo address)
        {
            try
            {
                var addressProcessing = new AddressProcessing("","");
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
        public IActionResult CheckManyAddresses(List<AddressInfo> addresssList)
        {
            try
            {
                var addressProcessing = new AddressProcessing("", "");
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

        [HttpOptions]
        public IActionResult GetEndPointDetails()
        {
            return StatusCode(200,
                value: $"Use GET for processing single address. Use POST for processing many addresses. GET takes {JsonConvert.SerializeObject(new AddressInfo())}. Returns {JsonConvert.SerializeObject(new AddressReturn())}. POST takes {JsonConvert.SerializeObject(new List<AddressInfo>())}. Returns {JsonConvert.SerializeObject(new List<AddressReturn>())}.");
    }
    }
}