# MedFlow Backend - Loyiha Analizi

**Analiz sanasi:** 2026-08-09
**Analiz qiluvchi:** Claude Code

---

## 1. UMUMIY MA'LUMOT

| Parametr | Qiymat |
|----------|--------|
| **Loyiha nomi** | MedFlow Backend |
| **Texnologiya** | .NET 6.0, C# |
| **Arxitektura** | Monolit, qatlamli (Layered) arxitektura + Repository Pattern |
| **Konteyner** | Yo'q (Dockerfile/docker-compose topilmadi) |
| **Ma'lumotlar bazasi** | PostgreSQL (yagona) |
| **Message Broker** | Yo'q |
| **Fayl xotira** | MinIO (S3-mos object storage, lokal standalone server) |

---

## 2. LOYIHA STRUKTURASI

```
MedFlow/
├── MedFlow/                        # API host — Controllers, Program.cs, Middleware
├── MedFlow.Domain/                 # Entity'lar, enum'lar (hech narsaga bog'liq emas)
├── MedFlow.Application/            # Servislar, DTO'lar, interfeyslar, Permission tizimi
├── MedFlow.DataLayer.Repository/   # EF Core DbContext, migratsiyalar, repository'lar
├── MedFlow.Infrastructure/         # JWT/BCrypt auth, MinIO fayl xotira, notification
├── tools/minio/                    # Lokal MinIO server binary'lari (.csproj'ga tegishli emas)
├── .config/dotnet-tools.json       # dotnet-ef lokal tool
└── MedFlow.sln
```

MedFlow bitta solution, 5 ta loyiha.

---

## 3. MIKROSERVISLAR RO'YXATI

**N/A** — MedFlow mikroservis emas, yagona monolit API. Uzasbo2'dagi 33 ta mustaqil servis o'rniga, barcha domenlar (bemor, hamshira, kurs, seans, dorixona, to'lov) bitta `MedFlow` host loyihasi ichida, faqat controller/servis darajasida ajratilgan.

---

## 4. KUTUBXONALAR TUZILISHI

Alohida `libs/` daraxti yo'q — umumiy kod loyihaning o'zi ichida 3 ta qatlamga taqsimlangan.

### 4.1 MedFlow.Domain — Entity'lar (12 ta)

Patient, Nurse, Prescription, TreatmentCourse, Session, Medicine, PatientMedicineWallet, MedicineIssue, ServiceType, Payment, User, RefreshToken.

`Enums.cs`: NurseType, Gender, LocationType, CourseStatus, SessionStatus, WalletSource, IssueSource, AssignmentMode, PaymentMethod, UserRole.

### 4.2 MedFlow.Application — `MedFlowServis/` (12 ta servis papkasi)

PatientServices, NurseServices, NurseAvailabilityServices, NurseAssignmentServices, PrescriptionServices, CourseServices, SessionServices, PharmacyServices, WalletServices, ServiceTypeServices, PaymentServices, AuthServices — har birida DTO fayllari + `Concrete/` (interfeys+impl) + `QueryObjects/` (hozircha bo'sh).

### 4.3 MedFlow.Infrastructure

- `Auth/` — JwtTokenService, BCryptPasswordHasher
- `FileStorage/` — MinioFileStorageService (faol), LocalDiskFileStorageService (DI'ga ulanmagan, dev fallback)
- `Notifications/` — LoggingNotificationSender

**Tashqi integratsiya kutubxonalari yo'q** — MedFlow davlat/bank tizimlariga ulanmaydi.

---

## 5. SERVIS ARXITEKTURASI

MedFlow bitta solution ichida qatlam bo'yicha ajratilgan:

```
MedFlow.Application/MedFlowServis/<Domen>Services/
├── <Entity>Dto.cs, Create<X>Request.cs, ...   # DTO/request — bitta klass = bitta fayl
├── <Entity>DtoConfig.cs                        # AutoMapper Profile (entity -> DTO)
└── Concrete/
    ├── I<Domen>Service.cs
    └── <Domen>Service.cs                       # StatusGenericHandler'dan meros
```

### Dizayn Pattern'lar:

1. **Repository Pattern** — har entity uchun ingichka (thin) interfeys+implementatsiya, generic `IRepository<T>` emas
2. **Unit of Work** — `IUnitOfWork` orqali barcha repository'lar bitta joydan
3. **Dependency Injection** — har qatlam o'z `DependencyInjection.cs`iga ega (`AddApplication`, `AddDataLayerRepository`, `AddInfrastructure`)
4. **DTO Pattern** + **AutoMapper Profile** — qo'lda `ToDto()` yo'q
5. **StatusGeneric** — exception tashlash o'rniga `AddError`/`IsValid` natija qaytarish

---

## 6. MA'LUMOTLAR BAZASI

### 6.1 PostgreSQL (yagona)

```
Host=localhost;Port=5432;Database=medflow_dev;Username=medflow;Password=***
```

Dev connection string `appsettings.Development.json`da; production sirlar (JWT signing key, MinIO credentials) .NET user-secrets orqali (`MedFlow.csproj`dagi `UserSecretsId`). MedFlow butunlay PostgreSQL'da.

### 6.2 Schema-per-domen

| Schema | Entity'lar |
|--------|-----------|
| `IDM` | User, RefreshToken |
| `ADM` | Patient |
| `HRM` | Nurse |
| `CLINIC` | Prescription, TreatmentCourse, Session |
| `PHARMACY` | Medicine, PatientMedicineWallet, MedicineIssue |
| `PAYMENT` | ServiceType, Payment |

Jadval/ustun nomlari `[Table]`/`[Column]` atributlari orqali `SCREAMING_SNAKE_CASE` shaklida (fluent config emas).

### 6.3 Entity Framework Core

- `DbContext`: `MedFlowDbContext` (`MedFlow.DataLayer.Repository/Persistence/`)
- Code-First Migrations — hozircha 1 ta: `20260807153148_InitialSchemaV2`
- `dotnet-ef` mahalliy tool sifatida o'rnatilgan (`.config/dotnet-tools.json`, versiya 7.0.20); `--project MedFlow.DataLayer.Repository --startup-project MedFlow` ikkalasi ham shart

---

## 7. KAFKA INTEGRATSIYA

**N/A** — MedFlow'da message broker yo'q. Barcha amallar sinxron HTTP so'rov/javob orqali bajariladi. Davriy ishlar uchun alohida consumer/producer infratuzilmasi o'rniga oddiy in-process `BackgroundService` ishlatiladi (`MissedSessionSweepService` — har 15 daqiqada muddati o'tgan seanslarni `Missed`ga o'tkazadi).

---

## 8. API TUZILISHI

### Controller Namunasi:

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class PatientsController : ApiControllerBase
{
    private readonly IPatientService _patients;

    [Authorize(Policy = PermissionCode.PatientEdit)]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PatientDto>> UpdateAsync(long id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var result = await _patients.UpdateAsync(id, request, ct);
        return FromStatus(result, _patients);
    }
}
```

### API Xususiyatlari:

- **Swagger/OpenAPI** — Swashbuckle 6.5.0, Bearer JWT security scheme
- **Routing** — `[controller]/[action]` (RPC-uslub)
- **JSON Serialization** — `System.Text.Json` (Newtonsoft emas), custom `DateOnly`/`DateTime` converterlar
- **Error Handling** — `StatusGeneric` + `ApiControllerBase.FromStatus()` → `400 ValidationProblem`; `ExceptionHandlingMiddleware` faqat kutilmagan `500` uchun
- **Response Compression / Metrics (Prometheus)** — yo'q

Controllerlar (10 ta): AuthController, PatientsController, NursesController, PrescriptionsController, CoursesController, SessionsController, MedicinesController, PatientWalletController, ServiceTypesController, PaymentsController

---

## 9. XAVFSIZLIK

### Permission-Based Access Control:

```csharp
// Kutubxona: MedFlow.Application.Common.Permissions
// Attribute: [Authorize(Policy = PermissionCode.***)]

[Authorize(Policy = PermissionCode.NurseCreate)]   // Yaratish
[Authorize(Policy = PermissionCode.PatientEdit)]   // Tahrirlash
```

Policy'lar statik `RolePermissions` (rol → kodlar xaritasi) orqali `Program.cs`da dinamik ro'yxatdan o'tadi.

Resurs egaligini tekshirish (masalan, bemor faqat o'zi/farzandi ma'lumotini ko'ra olishi) alohida — permission-kod emas, controllerlarda inline (`IsSelfOrChildAsync` + `Forbid()`).

### Autentifikatsiya:

- JWT Bearer (`System.IdentityModel.Tokens.Jwt`) + BCrypt parol hash
- **Basic Authentication** — yo'q (Uzasbo2'dagi `treas-internal` kabi realm'lar yo'q)

---

## 10. KONFIGURATSIYA FAYLLARI

### Muhitlar:

| Fayl | Muhit |
|------|-------|
| `appsettings.json` | Base (deyarli bo'sh — faqat `Logging`/`AllowedHosts`) |
| `appsettings.Development.json` | Development (connection string shu yerda) |

### Asosiy Konfiguratsiya Bo'limlari (appsettings + user-secrets birgalikda):

```json
{
  "ConnectionStrings": { "Default": "..." },
  "Jwt": { "SigningKey": "...", "Issuer": "MedFlow" },
  "Minio": { "Endpoint": "...", "AccessKey": "...", "SecretKey": "...", "BucketName": "...", "UseSsl": false },
  "Logging": { "LogLevel": {...} },
  "AllowedHosts": "*"
}
```

## 11. MONITORING VA LOGGING

| Texnologiya | Holati |
|-------------|--------|
| Standart ASP.NET Core `ILogger` | Bor (`ExceptionHandlingMiddleware`, `MissedSessionSweepService` va h.k.) |
| Serilog | Yo'q |
| Prometheus | Yo'q |
| Jaeger / distributed tracing | Yo'q |
| ELK Stack | Yo'q |
| Health Checks | Yo'q |

---

## 12. TASHQI INTEGRATSIYALAR

**N/A** — MedFlow tashqi davlat/bank/soliq tizimlariga ulanmaydi. Yagona tashqi bog'lanish — MinIO (fayl xotira, o'zining lokal infratuzilmasi).

---

## 13. HOZIRGI HOLAT / OXIRGI YIRIK O'ZGARISH

Git repozitoriysi hali ishga tushirilmagan (`.git` topilmadi) — branch/commit tarixi yo'q.

**Oxirgi yirik arxitektura o'zgarishi** (Uzasbo2 uslubiga moslashtirish maqsadida bajarilgan):

1. `MedFlow.DataLayer.Repository` alohida loyihaga ajratildi (DbContext + migratsiyalar + repository'lar Infrastructure'dan ko'chdi)
2. Xatolik boshqaruvi `throw new DomainException/NotFoundException`dan `StatusGeneric` (`AddError`/`IsValid`) ga o'tkazildi
3. Avtorizatsiya `[Authorize(Roles=...)]`dan `[Authorize(Policy = PermissionCode.*)]`ga o'tkazildi
4. Fayl xotira `LocalDiskFileStorageService`dan `MinioFileStorageService`ga o'tkazildi

**Maqsad:** biznes logikasi/validatsiya qoidalarini o'zgartirmasdan, faqat qatlam va xatolik/avtorizatsiya/fayl-xotira uslubini Uzasbo2'ga moslashtirish. Barcha 4 o'zgarish live HTTP testlar orqali tekshirilgan (eski va yangi xatti-harakat bir xil ekanligi tasdiqlangan).

---

## 14. MUHIM FAYLLAR YO'LLARI

| Tavsif | Yo'l |
|--------|------|
| Solution | `MedFlow.sln` |
| API host / entry point | `MedFlow/Program.cs` |
| Controllerlar | `MedFlow/Controllers/` |
| Entity'lar | `MedFlow.Domain/Entities/` |
| Servislar | `MedFlow.Application/MedFlowServis/` |
| Permission tizimi | `MedFlow.Application/Common/Permissions/` |
| DbContext + migratsiyalar | `MedFlow.DataLayer.Repository/Persistence/` |
| Repository'lar | `MedFlow.DataLayer.Repository/Repositories/` |
| Auth / MinIO | `MedFlow.Infrastructure/` |
| EF Core tool config | `.config/dotnet-tools.json` |
| Loyiha yo'l-yo'rig'i (Claude uchun) | `CLAUDE.md` |

---

## 15. ARXITEKTURA KUCHLI TOMONLARI

1. **Aniq qatlamli ajratish** — Domain → Application → DataLayer.Repository/Infrastructure → API, bog'liqlik yo'nalishi bir tomonlama (Application hech qachon tashqi qatlamlarga bog'lanmaydi)
2. **StatusGeneric** — exception-asoslangan boshqaruv oqimisiz, oldindan bashorat qilinadigan xatolik natijalari
3. **Permission-kod avtorizatsiya** — deklarativ, markazlashgan (`RolePermissions` bitta joyda)
4. **Konsistent servis/DTO konvensiyasi** — barcha 12 ta servis bir xil papka shakli va nomlashga amal qiladi
5. **AutoMapper Profile'lar** — entity→DTO mapping markazlashgan, qo'lda `ToDto()` takrorlanmaydi
6. **Schema-per-domen DB tuzilishi** — jadval egaligini aniq ko'rsatadi
7. **Real object storage (MinIO)** — fayl saqlashda disk-lock/scaling muammolaridan xoli

---

## 16. YAXSHILASH TAVSIYALARI

1. **Test loyihasi yo'q** — hech bo'lmasa servis qatlami (StatusGeneric natijalari) uchun unit test loyihasi qo'shish tavsiya etiladi
2. **Git repozitoriysi hali yo'q** — versiyalashni boshlash (tarix, branch, code review imkoniyati uchun)
3. **Docker yo'q** — hech bo'lmasa dev uchun `docker-compose` (Postgres + MinIO) qo'shish; hozir ikkalasi ham qo'lda ishga tushiriladi
4. **Health Checks yo'q** — DB va MinIO ulanishini tekshiruvchi `/health` endpoint qo'shish
5. **Faqat Development muhiti sozlangan** — production uchun `appsettings.Production.json` va sirlarni boshqarish strategiyasi (masalan Azure Key Vault) kerak bo'ladi
6. **appsettings.Development.json'da parol ochiq matnda** — hatto dev muhitida ham, connection string parolini user-secrets'ga to'liq ko'chirish tavsiya etiladi

---

*Bu analiz Claude Code tomonidan avtomatik yaratilgan.*
