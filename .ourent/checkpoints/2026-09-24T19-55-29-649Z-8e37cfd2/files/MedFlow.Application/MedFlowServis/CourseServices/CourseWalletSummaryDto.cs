using MedFlow.Application.MedFlowServis.WalletServices;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class CourseWalletSummaryDto
{
    public List<WalletEntryDto> Entries { get; set; } = null!;
}
