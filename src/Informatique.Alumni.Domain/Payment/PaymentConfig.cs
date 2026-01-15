using System;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Payment;

public class PaymentConfig : Entity<Guid>
{
    public PaymentGatewayType GatewayType { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string EncryptedApiKey { get; set; } = string.Empty;
    public string EncryptedSecret { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    private PaymentConfig() { }

    public PaymentConfig(Guid id, PaymentGatewayType gatewayType, string providerName) : base(id)
    {
        GatewayType = gatewayType;
        ProviderName = providerName;
        IsActive = true;
    }
    
    public void SetCredentials(string encryptedApiKey, string encryptedSecret)
    {
        EncryptedApiKey = encryptedApiKey;
        EncryptedSecret = encryptedSecret;
    }
}
