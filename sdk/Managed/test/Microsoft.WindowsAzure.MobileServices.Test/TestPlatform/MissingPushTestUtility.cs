using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    class MissingPushTestUtility : IPushTestUtility
    {
        public string GetPushHandle()
        {
            throw new NotImplementedException();
        }

        public string GetUpdatedPushHandle()
        {
            throw new NotImplementedException();
        }

        public Registration GetTemplateRegistrationForToast()
        {
            throw new NotImplementedException();
        }

        public void ValidateTemplateRegistration(Registration registration)
        {
            throw new NotImplementedException();
        }

        public void ValidateTemplateRegistrationBeforeRegister(Registration registration)
        {
            throw new NotImplementedException();
        }

        public void ValidateTemplateRegistrationAfterRegister(Registration registration, string zumoInstallationId)
        {
            throw new NotImplementedException();
        }

        public Registration GetNewNativeRegistration(string deviceId, IEnumerable<string> tags)
        {
            throw new NotImplementedException();
        }

        public Registration GetNewTemplateRegistration(string deviceId, string bodyTemplate, string templateName)
        {
            throw new NotImplementedException();
        }


        public string GetListNativeRegistrationResponse()
        {
            throw new NotImplementedException();
        }

        public string GetListTemplateRegistrationResponse()
        {
            throw new NotImplementedException();
        }

        public string GetListMixedRegistrationResponse()
        {
            throw new NotImplementedException();
        }
    }
}
