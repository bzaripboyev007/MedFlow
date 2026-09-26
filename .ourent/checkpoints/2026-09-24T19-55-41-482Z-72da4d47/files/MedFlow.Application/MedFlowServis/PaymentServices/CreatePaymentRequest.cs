using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.PaymentServices;

public class CreatePaymentRequest
{
    public PaymentMethod Method { get; set; }
}
