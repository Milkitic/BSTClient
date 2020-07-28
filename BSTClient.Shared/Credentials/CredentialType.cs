namespace BSTClient.Shared.Credentials
{
    public enum CredentialType
    {
        Generic = 1,
        DomainPassword,
        DomainCertificate,
        DomainVisiblePassword,
        GenericCertificate,
        DomainExtended,
        Maximum,
        MaximumEx = Maximum + 1000,
    }
}
