using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.MobileServices.Management
{
    public class CreateMobileServiceResult
    {
        /// <summary>
        /// The overall state of the mobile service application. Anything other than "Healthy" indicates failure.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The state of the mobile service component.
        /// </summary>
        public string MobileServiceState { get; set; }

        /// <summary>
        /// The name of the SQL database.
        /// </summary>
        public string SqlDbName { get; set; }

        /// <summary>
        /// The state of the SQL database component.
        /// </summary>
        public string SqlDbState { get; set; }

        /// <summary>
        /// The name of the SQL server.
        /// </summary>
        public string SqlServerName { get; set; }

        /// <summary>
        /// The state of the SQL server component
        /// </summary>
        public string SqlServerState { get; set; }

        /// <summary>
        /// Full response from Windows Azure, including any failure details. 
        /// </summary>
        public XElement Details { get; set; }

        public CreateMobileServiceResult() 
        {
            // empty
        }

        /// <summary>
        /// Returns true if the mobile service had been successfuly created and is healthy. 
        /// </summary>
        /// <returns></returns>
        public bool IsSuccess() 
        {
            return this.State == "Healthy";
        }
    }
}
