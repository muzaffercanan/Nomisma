# Nomisma

## Teknik Gereksinimler

Nomisma, bireysel musterilerin kredi basvurularini, taksit planlarini ve geri odemelerini dijital ortamda yonetmek icin hazirlanan full-stack bankacilik case-study uygulamasidir.

- Backend: C# / .NET 8
- Frontend: React + TypeScript
- Database: Microsoft SQL Server
- Mimari: Clean Architecture
- Auth: JWT, `Admin` ve `Customer` rolleri

## Kurulum

Backend:

```bash
dotnet tool restore
dotnet restore
dotnet ef database update --project src/Nomisma.Infrastructure --startup-project src/Nomisma.Api
dotnet run --project src/Nomisma.Api
```

Frontend:

```bash
cd client
npm install
npm run dev
```

Varsayilan API adresi `https://localhost:7176`. Farkli bir port kullaniliyorsa client icin `VITE_API_URL` verilebilir.

```bash
VITE_API_URL=https://localhost:7176 npm run dev
```

## Local SQL Server

Varsayilan connection string:

```json
"Server=(localdb)\\mssqllocaldb;Database=NomismaDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Local SQL Server instance farkliysa `src/Nomisma.Api/appsettings.Development.json` veya environment variable ile `ConnectionStrings__NomismaDb` degistirilebilir.

## Demo Hesaplari

- Admin: `admin@nomisma.local` / `Admin123!`
- Musteri: `customer@nomisma.local` / `Customer123!`

Development ortaminda uygulama acilirken migration uygulanir ve seed data olusturulur.

## Finansal Kurallar

Kredi hesaplama basit kar modeliyle yapilir:

```text
totalProfit = principal * profitRate / 100
totalDebt = principal + totalProfit
```

Toplam borc vade ay sayisina esit bolunur. Kurus yuvarlama farki son taksite yansitilir; bu sayede taksit toplamlari toplam borcla birebir eslesir.

Odeme kuralı: v1'de odeme tutari tam taksit tutariyla eslesmelidir.

Gerekce: Case-study v1 modeli her `Payment` kaydini tek bir `Installment` kaydina baglar. Kismi odeme veya fazla odeme desteklenirse kalan bakiye, mahsuplasma, iade ve muhasebe kurallari icin ayri domain modeli gerekir. Finansal tutarliligi sade ve denetlenebilir tutmak icin v1 kapsaminda yalnizca tam taksit kapama desteklenir.

## Mock Entegrasyonlar

Kredi skoru:

- `MockCreditScoreService` musteri bilgisine gore deterministik skor uretir.
- Skor `650` altindaysa kredi olusturma reddedilir.
- `risk` iceren email veya `0` ile biten kimlik numarasi dusuk skor dondurur.

Odeme gateway:

- `MockPaymentGateway` odeme kaydi olusmadan once cagrilir.
- `4111111111111111` gibi gecerli test kartlari basarili doner.
- `4000000000000002` ve `0000000000000000` reddedilir.
- Gateway basarisizsa `Payment` kaydi olusmaz ve taksit durumu degismez.

## API Ozeti

- `POST /api/auth/login`
- `GET/POST/PUT/DELETE /api/customers`
- `GET /api/customers/{id}`
- `GET /api/customers/{id}/summary`
- `GET /api/customers/me/summary`
- `GET/POST/PUT /api/loans`
- `GET /api/loans/{id}`
- `GET /api/loans/{id}/installments`
- `GET/PUT /api/installments/{id}`
- `GET/POST /api/payments`
- `GET /api/payments/{id}`

## Dogrulama

```bash
dotnet test
cd client
npm run build
```

