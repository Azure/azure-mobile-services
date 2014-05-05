namespace Microsoft.WindowsAzure.MobileServices
{
    internal interface ILocalStorageManager
    {
        bool IsRefreshNeeded { get; }
        string ChannelUri { get; set; }

        StoredRegistrationEntry GetRegistration(string registrationName);

        void DeleteRegistrationByName(string registrationName);

        void DeleteRegistrationByRegistrationId(string registrationId);

        void UpdateRegistrationByName(string registrationName, string registrationId, string registrationChannelUri);

        void UpdateRegistrationByRegistrationId(string registrationId, string registrationName, string registrationChannelUri);

        void ClearRegistrations();

        void RefreshFinished(string refreshedChannelUri);
    }
}