# MedFlow Backend — Xatolar va Zaifliklar Tahlili

**Sana:** 2026-08-09
**Usul:** Statik kod o'qish + `dotnet build MedFlow.sln` (baseline: 0 error, 4 warning)
**Qamrov:** `MedFlow/Controllers`, `MedFlow.Application/MedFlowServis`, `MedFlow.Infrastructure`, `MedFlow.DataLayer.Repository`, `MedFlow.Domain`

> Bu hujjat faqat tahlil — kod **o'zgartirilmagan**. Har bir topilma uchun fayl:satr dalili va tavsiya berilgan.

---

## 1. Umumiy xulosa

Loyiha arxitekturasi toza (qatlamli + Repository + UoW + StatusGeneric), lekin **ruxsat/egalik tekshiruvlari controllerlarda izchil emas** va bitta **aniq ishlamaydigan oqim** (OTP/Telegram telefon raqami) bor.

- Jami: **5 ta yuqori**, **4 ta o'rta**, **5 ta past** darajali topilma.
- Eng kritik: `Bookings` va `Prescriptions` endpointlaridagi **IDOR** (boshqa foydalanuvchi ma'lumotini o'qish/o'zgartirish), `PatientsController.CreateAsync`dagi **o'chirilgan avtorizatsiya**, va **telefon raqami normalizatsiyasi mos emasligi** sababli ro'yxatdan o'tish/parol tiklashning ishlamasligi.

---

## 2. Topilmalar jadvali

| # | Daraja | Soha | Fayl | Muammo |
|---|--------|------|------|--------|
| X-1 | 🔴 Yuqori | Avtorizatsiya | `MedFlow/Controllers/PatientsController.cs:22` | `[Authorize(Roles = RoleGroups.Staff)]` izohga olingan |
| X-2 | 🔴 Yuqori | Ma'lumot sizib chiqishi | `MedFlow/Controllers/BookingsController.cs:38-39` | Bemor **barcha** bronlarni ko'radi |
| X-3 | 🔴 Yuqori | IDOR | `MedFlow/Controllers/BookingsController.cs:43-58` | `GetById`/`UpdateStatus`da egalik/permission tekshiruvi yo'q |
| X-4 | 🔴 Yuqori | IDOR | `MedFlow/Controllers/PrescriptionsController.cs:53-57` | `GetById`da egalik tekshiruvi yo'q |
| X-5 | 🔴 Yuqori | Integratsiya | `OtpService.cs` vs `TelegramBotPollingService.cs` | Telefon raqam formati mos emas → OTP oqimi ishlamaydi |
| X-6 | 🟠 O'rta | IDOR | `MedFlow/Controllers/SessionsController.cs` | Mutatsiya endpointlarida hamshira egalik tekshiruvi yo'q |
| X-7 | 🟠 O'rta | 500 xatolik | `CourseService.cs:40` | `PreferredTimes` yuborilmasa `NullReferenceException` |
| X-8 | 🟠 O'rta | Biznes-logika | `NurseAvailabilityService.cs` | Mavjud `Bookings` hisobga olinmaydi → double booking |
| X-9 | 🟠 O'rta | Ma'lumot yaxlitligi | `TelegramBotPollingService.cs:126-141` | `ChatId` unique index buzilishi mumkin |
| X-10 | 🟡 Past | Xato xabari | `PaymentService.cs:95` | Xato xabarida `Payment` deb yozilgan, aslida `courseId` |
| X-11 | 🟡 Past | Tranzaksiya | `AuthService.RegisterPatientAsync` | 2 ta `SaveChanges` tranzaksiyasiz → orphan `Patient` |
| X-12 | 🟡 Past | Middleware | `ExceptionHandlingMiddleware.cs:28` | `Response.HasStarted` tekshirilmaydi |
| X-13 | 🟡 Past | Validatsiya | `BookingService.CreateAsync` | `BookingDate`/`BookingTime` formati tekshirilmaydi |
| X-14 | 🟡 Past | Barrel/Paket | `MedFlow.csproj`, `MedFlow.Application.csproj` | `net6.0` EOL + AutoMapper 12.0.1 zaifligi (NU1903) |

---

## 3. Batafsil topilmalar

### X-1 🔴 `PatientsController.CreateAsync` — avtorizatsiya o'chirilgan

**Dalil:**
```csharp
// MedFlow/Controllers/PatientsController.cs:21-23
[HttpPost]
//[Authorize(Roles = RoleGroups.Staff)]
public async Task<ActionResult<PatientDto>> CreateAsync([FromBody] CreatePatientRequest request, CancellationToken ct)
```

**Ta'sir:** Fallback policy faqat "autentifikatsiya qilingan"ni talab qiladi, shuning uchun **istalgan bemor akkaunti** yangi `Patient` yaratishi mumkin (hatto ixtiyoriy `ParentId` bilan — `PatientService.CreateAsync` faqat o'sha ota-ona mavjudligini tekshiradi, egalikni emas).

**Tavsiya:** `[Authorize(Policy = PermissionCode.PatientEdit)]` (yoki `[Authorize(Roles = RoleGroups.Staff)]`) qaytarish. Agar bemor o'z farzandini yaratishi kerak bo'lsa — `ParentId` ni tokendagi `linkedPatientId` bilan majburan bog'lash (foydalanuvchi yuborgan qiymatga ishonmaslik).

---

### X-2 🔴 `BookingsController.List` — bemor barcha bronlarni ko'radi

**Dalil:**
```csharp
// MedFlow/Controllers/BookingsController.cs:37-40
[HttpGet]
public async Task<ActionResult<List<BookingDto>>> List([FromQuery] int? nurseId, CancellationToken ct)
{
    var effectiveNurseId = User.IsNurse() ? User.GetLinkedNurseId() : nurseId;
```

**Ta'sir:** Hamshira uchun filtr to'g'ri ishlaydi, lekin **bemor** uchun `effectiveNurseId = nurseId` (query'dan). `nurseId` berilmasa `ListAsync(null)` **hamma** bronlarni qaytaradi — va `BookingDto`da `FullName`, `Phone`, `Address`, `PrescriptionUrl` bor. Bu boshqa bemorlarning shaxsiy ma'lumotlarining sizib chiqishi.

**Tavsiya:** `User.IsPatient()` holatida 403 (`Forbid()`) qaytarish yoki bemorni umuman bu endpointga qo'ymaslik.

---

### X-3 🔴 `BookingsController` — `GetById` va `UpdateStatus`da ruxsat yo'q

**Dalil:**
```csharp
// MedFlow/Controllers/BookingsController.cs:42-58
[HttpGet("{id:long}")]
public async Task<ActionResult<BookingDto>> GetById(long id, CancellationToken ct) { ... }

[HttpPatch("{id:long}")]
public async Task<ActionResult<BookingDto>> UpdateStatus(long id, [FromBody] UpdateBookingStatusRequest request, ...)
```

**Ta'sir:**
- `GetById` — har qanday autentifikatsiya qilingan foydalanuvchi ID bo'yicha (ketma-ket oshirib) barcha bronlarni o'qiy oladi.
- `UpdateStatus` — `Booking.UpdateStatus` uchun permission ham, rol tekshiruvi ham yo'q; **bemor** boshqa bronning holatini `Kelgan`/`Yakunlandi`ga o'zgartira oladi (spec: "Hamshira bron holatini o'zgartiradi").
- `Bookings` uchun `PermissionCode` umuman yo'q (`PermissionCode.cs`da `Booking.*` yo'q) — ya'ni hozircha permission infratuzilmasi bu domenni qamramagan.

**Tavsiya:** `Booking.UpdateStatus` kabi kod qo'shib, `[Authorize(Policy = ...)]` qo'llash; hamshira uchun `booking.NurseId == User.GetLinkedNurseId()` egalik tekshiruvini qo'shish; `GetById`ni autentifikatsiyalangan egalari bilan cheklash.

---

### X-4 🔴 `PrescriptionsController.GetById` — egalik tekshiruvi yo'q (IDOR)

**Dalil:**
```csharp
// MedFlow/Controllers/PrescriptionsController.cs:52-57
[HttpGet("{id:long}")]
public async Task<ActionResult<PrescriptionDto>> GetById(long id, CancellationToken ct)
{
    var result = await _prescriptions.GetByIdAsync(id, ct);
```

**Ta'sir:** `Upload` (satr 26) va `ListByPatient` (satr 70) `IsSelfOrChildAsync` bilan himoyalangan, lekin **`GetById` himoyalanmagan**. `PrescriptionDto`da `FileUrl` bor — bemor boshqa bemorning retsept faylini ochib oladi.

**Tavsiya:** `Upload`dagi bilan bir xil tekshiruvni qo'shish (avval retseptni yuklab, `PatientId` ni `IsSelfOrChildAsync` orqali tekshirish), yoki servis qatlamida egalikni tekshirish.

---

### X-5 🔴 Telefon raqam normalizatsiyasi mos emas — OTP oqimi ishlamaydi

**Dalil:**
```csharp
// MedFlow.Infrastructure/Telegram/TelegramBotPollingService.cs:162-166
private static string NormalizePhone(string phone)
{
    var trimmed = phone.Trim();
    return trimmed.StartsWith('+') ? trimmed : $"+{trimmed}";   // -> "+998901234567"
}
```
vs.
```csharp
// MedFlow.Application/MedFlowServis/OtpServices/Concrete/OtpService.cs:23
var phoneNumber = request.PhoneNumber.Trim();                   // -> "998901234567"
var link = await _uow.TelegramLinks.Query().FirstOrDefaultAsync(l => l.PhoneNumber == phoneNumber, ct);
```

Xuddi shu `Trim()`-only yondashuv: `OtpService.cs:74`, `OtpService.cs:115`, `AuthService.cs:33, 91, 115, 208`.

**Ta'sir:** `TELEGRAM_LINKS.PHONE_NUMBER` **doim `+` bilan** saqlanadi, OTP/registratsiya esa **`+`siz** qidiradi. Agar klient raqamni `+`siz yuborsa (odatiy holat, chunki bot `+` qo'shadi) — `SendOtpAsync` linkni topmaydi va "Bu raqam Telegram botga ulanmagan..." xatosini qaytaradi. Natijada:
- `Auth/SendOtp` ishlamaydi,
- `RegisterPatientAsync` va `ResetPasswordAsync` (`IsRecentlyVerifiedAsync`) hech qachon `true` bo'lmaydi.

**Tavsiya:** yagona `PhoneNumberNormalizer.ToE164(string)` yordamchisini yaratib (bo'sh joy/`-`/`(`/`)` tozalash, `+` qo'shish, `998` prefiksini tekshirish) **ham yozishda, ham qidiruvda** ishlatish. Migratsiya bilan mavjud `PHONE_NUMBER` qiymatlarini ham bir xil formatga keltirish.

---

### X-6 🟠 `SessionsController` — mutatsiyalarda egalik tekshiruvi yo'q

**Dalil:** `MedFlow/Controllers/SessionsController.cs` — `Arrive`, `Complete`, `MarkMissed`, `IssueMedicine` faqat policy bilan himoyalangan. `RolePermissions.StaffCodes` esa **ikkala hamshira roliga ham bir xil** barcha kodlarni beradi.

**Ta'sir:** Hamshira A hamshira B ning seansini "Kelgan" deb belgilashi, yakunlashi yoki dorini berilgan deb yozishi mumkin. `CoursesController` esa `GetById`/`GetSessions`da `CanAccessCourseAsync` bilan egalikni tekshiradi — ya'ni yondashuv nomuvofiq. `SessionsController.GetById` ham umuman egalikni tekshirmaydi.

**Tavsiya:** `SessionsController`ga `CanAccessCourseAsync`ga o'xshash egalik tekshiruvini qo'shish (sessiya → kurs → `PrimaryNurseId` / `PatientId`).

---

### X-7 🟠 `CourseService.CreateAsync` — `PreferredTimes` null bo'lsa 500

**Dalil:**
```csharp
// MedFlow.Application/MedFlowServis/CourseServices/CreateCourseRequest.cs
public List<string> PreferredTimes { get; set; } = null!;

// MedFlow.Application/MedFlowServis/CourseServices/Concrete/CourseService.cs:40
if (request.PreferredTimes.Count != request.TimesPerDay)
```

**Ta'sir:** JSON'da `preferredTimes` umuman bo'lmasa `System.Text.Json` uni `null` qoldiradi → `Count` da `NullReferenceException` → `500` (to'g'ri javob `400`) va `ExceptionHandlingMiddleware` orqali umumiy xatolik.

**Tavsiya:** `request.PreferredTimes is null || request.PreferredTimes.Count != request.TimesPerDay` formasida tekshirish (yoki `[Required]` + model validation).

---

### X-8 🟠 `NurseAvailabilityService` — bronlarni hisobga olmaydi

**Dalil:** `MedFlow.Application/MedFlowServis/NurseAvailabilityServices/Concrete/NurseAvailabilityService.cs:17-21` faqat `_uow.Sessions` ni so'raydi; `Bookings` jadvaliga murojaat yo'q.

**Ta'sir:** `NursesController.GetAvailability` hamshirani "bo'sh" deb ko'rsatishi mumkin, holbuki o'sha vaqtda `BOOKINGS`da band (xususan uyga chaqirish). Bir vaqtga ikki mijoz yoziladi.

**Tavsiya:** mavjud `Bookings` (`BookingDate`+`BookingTime`, `Status` faol holatlarda) ni ham band vaqtlar ro'yxatiga qo'shish; `BookingService.CreateAsync`da saqlashdan oldin availability tekshiruvini o'tkazish.

---

### X-9 🟠 `TelegramBotPollingService.LinkPhoneAsync` — unique index to'qnashuvi

**Dalil:**
```csharp
// MedFlow.Infrastructure/Telegram/TelegramBotPollingService.cs:126-134
var existing = await uow.TelegramLinks.Query()
    .FirstOrDefaultAsync(l => l.PhoneNumber == phoneNumber || l.ChatId == chatId, ct);
if (existing is not null) { existing.PhoneNumber = phoneNumber; existing.ChatId = chatId; ... }
```
`TelegramLinkConfiguration.cs:11-12`: `HasIndex(l => l.PhoneNumber).IsUnique()` va `HasIndex(l => l.ChatId).IsUnique()`.

**Ta'sir:** Agar `chatId` X allaqachon A raqamiga bog'langan bo'lsa, keyin X dan B raqami ulashilsa va B raqami boshqa qatorda mavjud bo'lsa — `OR` so'rovi B qatorini qaytaradi va uning `ChatId` ini X ga o'zgartirish **unique constraint** buzilishiga olib keladi (`DbUpdateException` → polling sikli 5s kutib qayta urinadi, lekin xato takrorlanadi). Bir xil `chatId`ni ikki qatorga yozish ham mumkin emas.

**Tavsiya:** telefon va chat bo'yicha alohida qidirish, konfliktni eksplitsit hal qilish (masalan, eski bog'lanishni ajratish/o'chirish), `DbUpdateException`ni ushlab foydalanuvchiga tushunarli xabar yuborish.

---

### X-10 🟡 `PaymentService.GetByCourseAsync` — xato xabaridagi ID

**Dalil:**
```csharp
// MedFlow.Application/MedFlowServis/PaymentServices/Concrete/PaymentService.cs:93-96
var payment = await _uow.Payments.Query().FirstOrDefaultAsync(p => p.CourseId == courseId, ct);
if (payment is null)
{
    AddError($"{nameof(Payment)} (Id={courseId}) topilmadi.");
```
**Ta'sir:** Xabar `Payment` haqida, lekin `courseId` ko'rsatilgan — chalkashtiradi. **Tavsiya:** `Payment (CourseId={courseId}) topilmadi.` yoki `nameof(TreatmentCourse)`.

---

### X-11 🟡 `AuthService.RegisterPatientAsync` — tranzaksiyasiz ikki yozuv

**Dalil:** `AuthService.cs:52-77` — avval `Patient` saqlanadi (`SaveChangesAsync`), keyin `User` saqlanadi (yana `SaveChangesAsync`). Oraliqda xatolik bo'lsa `Patient` "orphan" qoladi.

**Tavsiya:** ikkalasini bitta `SaveChangesAsync` chaqiruviga birlashtirish (FK `LinkedPatientId` uchun Patient `Id` kerak — EF navigatsiya orqali bog'lash) yoki `IUnitOfWork`/`DbContext` tranzaksiyasidan foydalanish.

---

### X-12 🟡 `ExceptionHandlingMiddleware` — `HasStarted` tekshiruvi yo'q

**Dalil:** `MedFlow/Middleware/ExceptionHandlingMiddleware.cs:28-34` — javob boshlanib bo'lgan bo'lsa ham `StatusCode`/`ContentType` o'zgartiriladi va matn yoziladi.

**Ta'sir:** Streaming/serializatsiya paytida yuzaga kelgan xatolikda `InvalidOperationException` ("response headers already sent") yashirincha yuzaga kelishi mumkin.

**Tavsiya:** `if (context.Response.HasStarted) { _logger... ; return; }` qo'shish (yoki `IExceptionHandler`/`ProblemDetails`ga o'tish).

---

### X-13 🟡 `BookingService.CreateAsync` — sana/vaqt formati tekshirilmaydi

**Dalil:** `BookingService.cs:86-87` — faqat `string.IsNullOrWhiteSpace` tekshiriladi, keyin qiymatlar `nvarchar(10)`/`nvarchar(5)` ustunlarga `Trim()` qilinadi (`BookingConfiguration`, `BOOKING_DATE`/`BOOKING_TIME`).

**Ta'sir:** `"2026-13-45"` yoki juda uzun string DB'ga tushishi yoki `maxLength` tufayli xatolik berishi mumkin; `NurseAvailabilityService` bilan solishtirishda ham buziladi.

**Tavsiya:** `DateOnly.TryParseExact("yyyy-MM-dd")` va `TimeOnly.TryParseExact("HH:mm")` bilan validatsiya qilib, normallashtirilgan qiymatni saqlash.

---

### X-14 🟡 Muhit/paket holati (build warninglaridan)

- `NETSDK1138`: `net6.0` qo'llab-quvvatlanmaydi (EOL). Yangilash tavsiya etiladi.
- `NU1903`: `AutoMapper 12.0.1` — ma'lum yuqori darajali zaiflik (GHSA-rvv3-g6hj-g44x). 13.x/15.x ga ko'tarish.

---

## 4. Tavsiya etilgan tuzatish tartibi

1. **X-1, X-2, X-3, X-4, X-6** — avtorizatsiya/IDOR (xavfsizlik, eng katta ta'sir).
2. **X-5** — OTP/registratsiya oqimining butunlay ishlamasligi.
3. **X-7** — 500 → 400 (barqarorlik).
4. **X-8, X-9, X-11** — ma'lumot yaxlitligi.
5. **X-10, X-12, X-13, X-14** — tozalash va muvofiqlik.

## 5. Izohlar

- `bin/`, `obj/` dagi DLL'lar tahlil qilinmagan (faqat manba kod).
- Avtomatik test loyihasi yo'q — tuzatishlardan keyin qo'lda/HTTP orqali tekshirish tavsiya etiladi.
