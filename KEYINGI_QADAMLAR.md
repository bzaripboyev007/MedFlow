# MedFlow — Nima qilish kerak (keyingi qadamlar)

**Sana:** 2026-09-24 (yangi sessiya, hujjat hozirgi holat asosida yozildi)
**Bu hujjat nima haqida:** avvalgi sessiyalarda `XATOLAR_TAHLILI.md` tartibida X-1…X-13 tuzatildi, sessiya qadam chegarasidan uzilib qoldi. Bu fayl **qolgan ishlar ro'yxati**: nimani siz (foydalanuvchi) qilishingiz kerak, nimani men (agent) qilishim kerak, va har bir qadamdan keyin qanday tekshirish kerak.

---

## 0. Hozirgi holat (tekshirilgan, dalili bilan)

| Tekshiruv | Natija |
|---|---|
| `dotnet build MedFlow.sln` | **0 error, 4 warning** (SDK 9.0.316) |
| Git holati | 21 ta o'zgargan manba fayl + 1 yangi fayl (`PhoneNumberNormalizer.cs`) |
| `git diff --stat` | 353 qator qo'shilgan, 64 qator o'chirilgan |
| Commit qilinganmi | **Yo'q** — o'zgarishlar hali `working tree`da |
| Qolgan warning | `NETSDK1138` (net6.0 EOL), `NU1903` (AutoMapper 12.0.1 zaifligi) |
| Tuzatilmagan topilma | **X-14** (framework/paket yangilash) |

Ya'ni: **kod tomonidagi xavfsizlik/oqim tuzatishlari tugagan, ochiq ish bitta — X-14.**

---

## 1. SIZ (foydalanuvchi) nima qilishingiz kerak

### 1.1. O'zgarishlarni ko'rib chiqib commit qilish

1. O'zgarishlarni ko'ring:
   ```
   git diff
   git status --short
   ```
2. Commit (tavsiya etilgan xabar):
   ```
   git add -A
   git commit -m "X-1..X-13 ni tuzatish: avtorizatsiya/IDOR, telefon normalizatsiyasi, validatsiya, tranzaksiya"
   ```
   > Diqqat: `bin/`, `obj/`, `build_out.log` va `.ourent/` commit'ga tushmasligi kerak — `.gitignore`ni tekshirish tavsiya etiladi.

### 1.2. Maxfiy sozlamalar joyidami (build ishlashi bulardan mustaqil, lekin ishga tushirish uchun shart)

`Program.cs` va `AddDataLayerRepository` konfiguratsiyadan quyidagilarni kutadi:

| Kalit | Nima uchun kerak | Qayerda saqlanadi |
|---|---|---|
| `Jwt:SigningKey` | Yo'q bo'lsa ilova **ishga tushmaydi** (`throw new InvalidOperationException`) | User Secrets (`UserSecretsId: 7dbb0fa5-ca97-4526-bcac-99c3c3a12dea`) yoki env |
| `Jwt:Issuer` | Token validatsiyasi (default `MedFlow`) | User Secrets / appsettings |
| PostgreSQL connection string | `Npgsql` EF provayderi | User Secrets / env |
| Telegram bot token (+ admin chat id) | `TelegramBotPollingService` | User Secrets / env |
| MinIO (endpoint/access/secret) | Retsept/`PrePrescription` fayl yuklash | User Secrets / env |

Tekshirish:
```
dotnet user-secrets list --project MedFlow/MedFlow.csproj
```
(Kamida `Jwt:SigningKey`, connection string va Telegram tokeni bo'lishi kerak.)

### 1.3. Baza holati (X-5, X-11 tuzatishlari uchun)

- **Yangi migratsiya shart emas.** Sxema o'zgarmagan: X-5 yechimi `PhoneNumberNormalizer.StorageForms()` orqali bazadagi **ham `+998…`, ham `998…` ko'rinishlarini** qidiradi (`PhoneNumberNormalizer.cs:36-45`). Shu sabab eski yozuvlarni migratsiya qilish ixtiyoriy (tavsiya: kelajakda bir ko'rinishga keltirish).
- Faqat mavjud migratsiyalar bazaga qo'llanganiga ishonch hosil qiling:
  ```
  dotnet ef database update --project MedFlow.DataLayer.Repository --startup-project MedFlow
  ```
  (Oxirgi migratsiya: `20260914130105_AddBookingsAndNursePricePerVisit`.)

### 1.4. Qo'lda tekshirish — 5 ta oqim (avtomatik test yo'q, shuning uchun bu majburiy)

| # | Nima qilinadi | Kutilgan natija | Tegishli topilma |
|---|---|---|---|
| 1 | Botda raqam ulash → `POST /api/Auth/SendOtp` (raqamni `+`li va `+`siz yuborib) → `VerifyOtp` → `RegisterPatient` | OTP yuboriladi, ro'yxatdan o'tish o'tadi (ilgari **umuman ishlamasdi**) | X-5 |
| 2 | Bemor tokeni bilan `GET /api/Bookings` va begona `GET /api/Bookings/{id}` | `403 Forbidden` (ma'lumot sizib chiqmaydi) | X-2, X-3 |
| 3 | Bemor tokeni bilan begona `PATCH /api/Bookings/{id}` | `403` | X-3 |
| 4 | Bemor tokeni bilan begona `GET /api/Prescriptions/{id}` | `403` (fayl URL berilmaydi) | X-4 |
| 5 | Hamshira A tokeni bilan hamshira B seansini `Arrive/Complete/MarkMissed/IssueMedicine` | `403` | X-6 |
| 6 | Bir vaqtga bir hamshira uchun ikkinchi bron yaratish (`POST /api/Bookings`), hamda `BookingDate="2026-13-45"` | `400` + aniq xato xabari (500 emas) | X-8, X-13 |
| 7 | `POST /api/Courses` JSON'ida `preferredTimes` umuman bo'lmasa | `400` (500 emas) | X-7 |

> Hamma endpointlar `FallbackPolicy` tufayli token talab qiladi; `AuthController` ochiq (`[AllowAnonymous]`).

### 1.5. Ixtiyoriy, lekin kuchli tavsiya

- **Test loyihasi qo'shish** (`MedFlow.Tests`, xUnit): hozircha avtomatik test yo'q, har bir tuzatish faqat qo'lda tekshiriladi. Eng arzon regressiya — X-1…X-6 uchun `403` testlari.
- CI (GitHub Actions) qo'shib `dotnet build` + testlarni avtomatik yuritish.

---

## 2. MEN (agent) nima qilishim kerak — qolgan yagona ish: X-14

### 2.1. Muammo (dalil)

- `NETSDK1138`: 4 ta `.csproj`da `net6.0` — qo'llab-quvvatlanmaydi (EOL), xavfsizlik yangilanishlari kelmaydi.
- `NU1903`: `AutoMapper 12.0.1` — yuqori darajali ma'lum zaiflik (GHSA-rvv3-g6hj-g44x).
- **Diqqat:** `AutoMapper 12.0.1` yonida `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1` bor; 13.x versiyasida bu paket **olib tashlangan** — DI ro'yxatga olish kodi o'zgarishi shart (breaking change). Shuning uchun bu ish alohida, ehtiyotkor bajariladi.

### 2.2. Qaror kerak (mendan oldin siz tasdiqlashingiz kerak)

1. Maqsad framework: **`net8.0` (LTS, barqaror, tavsiya)** yoki `net9.0` (mashinada SDK 9.0.316 bor)?
2. AutoMapper'ni **13.0.1** ga ko'tarish (breaking fix talab qiladi) yoki **12.0.1 da qoldirib** faqat frameworkni ko'tarish (zaiflik ochiq qoladi)?

### 2.3. Taklif etilayotgan tartib (har qadamdan keyin `dotnet build MedFlow.sln`)

1. `MedFlow.Domain`, `MedFlow.Application`, `MedFlow.DataLayer.Repository`, `MedFlow.Infrastructure`, `MedFlow` — 5 ta `.csproj`da `<TargetFramework>net8.0</TargetFramework>`.
2. Paketlarni moslashtirish:
   - `Microsoft.AspNetCore.Authentication.JwtBearer` 6.0.36 → 8.0.x
   - `Microsoft.EntityFrameworkCore(.Design)` 7.0.20 → 8.0.x
   - `Npgsql.EntityFrameworkCore.PostgreSQL` 7.0.18 → 8.0.x
   - `Microsoft.Extensions.*` 7.0.x → 8.0.x
   - `Swashbuckle.AspNetCore`, `Minio`, `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt` — net8 bilan mos versiyalarga.
3. Build → xatoliklarni tuzatish (odatda EF 8 da `DateOnly`/`TimeOnly` xaritalash va `Include` semantikasi tekshiriladi — loyihada `DateOnly` ko'p ishlatilgan, `DateOnlyTypeConverter`/`DateOnlyJsonConverter` mavjud).
4. **AutoMapper 13.0.1** (agar tasdiqlansa): `AutoMapper.Extensions.Microsoft.DependencyInjection` paketini olib tashlash, `AddAutoMapper` chaqiruvini yangi ko'rinishga o'tkazish:
   ```csharp
   builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(SomeProfile).Assembly));
   ```
   va `MedFlow.Application/DependencyInjection` (yoki qayerda ro'yxatga olinayotgan bo'lsa) ni moslash.
5. Build → 0 error va `NETSDK1138`/`NU1903` warninglar **yo'qolganini** tekshirish.
6. `XATOLAR_TAHLILI.md` 6-bo'limidagi X-14 qatorini ✅ ga o'zgartirish + shu faylga qisqa "X-14 bajarildi" izohi.

### 2.4. Men taklif qiladigan qo'shimcha (ixtiyoriy) ishlar

- `MedFlow.Tests` (xUnit + `WebApplicationFactory`) — X-1…X-6 uchun `403`, X-7/X-13 uchun `400` testlari.
- `dotnet list package --vulnerable --include-transitive` ni CI'ga qo'shish (kelgusi zaifliklarni avtomatik ushlash).

---

## 3. Muhim eslatmalar

- Tuzatishlar **mavjud oqimlarni o'zgartirdi** (ayniqsa: bemor endi `Bookings/List`ni ko'rmaydi). Agar frontend bemor uchun "mening bronlarim" ro'yxatiga tayansa — unga alohida, egalik tekshiruvli endpoint kerak bo'ladi (men buni qo'sha olaman).
- **Saqlangan fayl muhim:** `XATOLAR_TAHLILI.md` — topilmalar va tuzatishlar tarixi; `KEYINGI_QADAMLAR.md` (shu fayl) — navbatdagi amallar ro'yxati.
- Har bir qadamdan keyin tekshiruv buyrug'i bir xil: `dotnet build MedFlow.sln` (kutilgan: 0 error).

---

## 4. Bir qatorga qisqa javob ("nima qilishim kerak?")

1. `dotnet build MedFlow.sln` ni o'zingizda bir marta yuritib 0 error ekaniga ishonch hosil qilish.
2. User Secrets'da `Jwt:SigningKey` + connection string + Telegram token + MinIO borligini tekshirish.
3. `dotnet ef database update` (migratsiya shart emas, lekin bazani yangilab qo'yish).
4. 1.4-jadvaldagi 7 ta oqimni qo'lda sinash.
5. O'zgarishlarni commit qilish.
6. Menga X-14 bo'yicha qarorni aytish (`net8.0` yoki `net9.0`; AutoMapper 13 ga o'tamizmi) — shundan keyin X-14 ni tuzatib, build bilan tasdiqlab beraman.
