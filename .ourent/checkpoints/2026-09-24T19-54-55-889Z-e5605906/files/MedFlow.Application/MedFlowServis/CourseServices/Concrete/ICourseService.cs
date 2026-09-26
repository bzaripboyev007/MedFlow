using MedFlow.Domain.Enums;
using StatusGeneric;

namespace MedFlow.Application.MedFlowServis.CourseServices.Concrete;

public interface ICourseService : IStatusGeneric
{
    Task<CourseDto?> CreateAsync(CreateCourseRequest request, CancellationToken ct = default);
    Task<CourseDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<CourseSummaryDto>> ListAsync(long? patientId, int? nurseId, CourseStatus? status, CancellationToken ct = default);
    Task<CourseDto?> UpdateStatusAsync(long id, CourseStatus status, CancellationToken ct = default);
    Task<CourseWalletSummaryDto?> GetWalletSummaryAsync(long courseId, CancellationToken ct = default);
}
