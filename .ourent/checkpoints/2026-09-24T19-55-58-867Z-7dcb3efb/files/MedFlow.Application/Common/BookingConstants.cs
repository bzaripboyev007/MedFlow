namespace MedFlow.Application.Common;

public static class BookingConstants
{
    /// <summary>Uyga chaqirish uchun yo'l haqi (frontend'dagi HOME_VISIT_FEE bilan bir xil bo'lishi shart).</summary>
    public const decimal HomeVisitFee = 25000m;

    /// <summary>Hamshiraga narx ko'rsatilmaganda ishlatiladigan standart tashrif narxi.</summary>
    public const decimal DefaultNurseVisitPrice = 50000m;

    public const string PatientTypeAdult = "adult";
    public const string PatientTypeChild = "child";

    public const string ServiceTypeHome = "home";
    public const string ServiceTypeClinic = "clinic";

    public const string StatusNew = "Yangi";

    /// <summary>Bron o'tishi mumkin bo'lgan holatlar.</summary>
    public static readonly string[] AllowedStatuses =
    {
        StatusNew, "Kelgan", "Jarayonda", "Yakunlandi", "Bekor qilindi"
    };
}
