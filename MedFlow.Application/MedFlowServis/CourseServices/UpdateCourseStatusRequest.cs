using MedFlow.Domain.Enums;

namespace MedFlow.Application.MedFlowServis.CourseServices;

public class UpdateCourseStatusRequest
{
    public CourseStatus Status { get; set; }
}
