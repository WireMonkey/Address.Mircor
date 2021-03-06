﻿namespace Microservices.Cass.Poco
{
    public class AddressReturn
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Zip4Code { get; set; }
        public string CassCode { get; set; }
        public bool ValidAddress { get; set; }
        public string errorMessage { get; set; }
    }
}
