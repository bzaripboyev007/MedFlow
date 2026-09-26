namespace MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;

public interface INurseAvailabilityService
{
    /// <summary>Hamshira berilgan barcha vaqtlarda bo'shmi (mavjud band seanslar/bronlar bilan to'qnashmaydimi).</summary>
    Task<bool> IsAvailableForAllAsync(int nurseId, IEnumerable<DateTimeOffset> times, CancellationToken ct = default);

    /// <summary>
    /// Hamshiraning bir kunda band bo'lgan vaqt oynalari (seanslar + faol bronlar).
    /// <c>NursesController.GetAvailability</c> va <c>NurseService.GetAvailabilityAsync</c> bir xil
    /// mantiq bilan ishlashi uchun yagona manba shu metod (X-8 ning controller oqimi).
    /// </summary>
    Task<List<(DateTimeOffset From, DateTimeOffset To)>> GetBusySlotsAsync(int nurseId, DateTime date, CancellationToken ct = default);
}
