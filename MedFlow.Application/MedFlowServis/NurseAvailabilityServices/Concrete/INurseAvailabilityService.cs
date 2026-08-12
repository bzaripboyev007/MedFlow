namespace MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;

public interface INurseAvailabilityService
{
    /// <summary>Hamshira berilgan barcha vaqtlarda bo'shmi (mavjud band seanslar bilan to'qnashmaydimi).</summary>
    Task<bool> IsAvailableForAllAsync(int nurseId, IEnumerable<DateTimeOffset> times, CancellationToken ct = default);
}
