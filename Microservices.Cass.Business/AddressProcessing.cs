using System.Linq;
using Microservices.Cass.Poco;
using SmartyStreets;
using SmartyStreets.USStreetApi;

namespace Microservices.Cass.Business
{
    public class AddressProcessing
    {
        private readonly string _username;
        private readonly string _pwd;

        public AddressProcessing(string userName, string password)
        {
            _username = userName;
            _pwd = password;
        }

        public AddressReturn ProcessAddress(AddressInfo address)
        {
            var client = new ClientBuilder(_username, _pwd).BuildUsStreetApiClient();
            var returnAddress = new AddressReturn();
            var lookup = new Lookup
            {
                Street = address.Address1,
                Street2 = address.Address2,
                City = address.City,
                State = address.City,
                ZipCode = address.Zip
            };

            client.Send(lookup);

            var result = lookup.Result;
            //Return true if first returned address matches supplied address
            if (result.Count <= 0)
            {
                returnAddress.ValidAddress = false;
            }
            else
            {
                var rAddress = result.FirstOrDefault();
                returnAddress.Address1 = address.Address1;
                returnAddress.Address2 = address.Address2;
                returnAddress.City = rAddress.Components.CityName;
                returnAddress.State = rAddress.Components.State;
                returnAddress.Zip = rAddress.Components.ZipCode;
                returnAddress.Zip4Code = rAddress.Components.Plus4Code;
                returnAddress.CassCode = rAddress.Metadata.CarrierRoute;
                returnAddress.ValidAddress = true;
            }

            return returnAddress;
        }
    }
}
