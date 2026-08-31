namespace AgriBuy.Models.Enums
{
    public enum PaymentMethodType
    {
        COD = 1,
        GCash = 2,
        InstaPay = 3,
        Card = 4,
        BankTransfer = 5
    }

    public static class PaymentMethodExtensions
    {
        public static string GetDisplayName(this PaymentMethodType paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodType.COD => "Cash on Delivery",
                PaymentMethodType.GCash => "GCash",
                PaymentMethodType.InstaPay => "InstaPay",
                PaymentMethodType.Card => "Credit/Debit Card",
                PaymentMethodType.BankTransfer => "Bank Transfer",
                _ => "Unknown"
            };
        }

        public static string GetIcon(this PaymentMethodType paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodType.COD => "💵",
                PaymentMethodType.GCash => "📱",
                PaymentMethodType.InstaPay => "⚡",
                PaymentMethodType.Card => "💳",
                PaymentMethodType.BankTransfer => "🏦",
                _ => "❓"
            };
        }

        public static bool RequiresPaymentGateway(this PaymentMethodType paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodType.GCash or PaymentMethodType.InstaPay or PaymentMethodType.Card => true,
                _ => false
            };
        }

        public static bool IsManualVerification(this PaymentMethodType paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethodType.BankTransfer => true,
                _ => false
            };
        }
    }
}