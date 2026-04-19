# 🛡️ Sentinel — Auth Server

**.NET 8 Clean Architecture tabanlı, production-ready kimlik doğrulama sunucusu.**

---

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Mimari](#mimari)
- [Özellikler](#özellikler)
- [Teknoloji Stack](#teknoloji-stack)
- [Kurulum](#kurulum)
- [API Endpointleri](#api-endpointleri)
- [Konfigürasyon](#konfigürasyon)
- [Servisler](#servisler)

---

## Genel Bakış

Sentinel, mikroservis mimarisinde merkezi kimlik doğrulama sunucusu olarak tasarlanmıştır. JWT tabanlı access token ve refresh token üretir, Redis ile token blacklist ve brute-force koruması sağlar, RabbitMQ ile kullanıcı kayıt eventlerini yayınlar ve ELK stack ile merkezi loglama yapar.

---

## Mimari

```
Sentinel/
├── Sentinel.Domain          → Entity'ler, domain kuralları
├── Sentinel.Application     → CQRS (MediatR), Interface'ler, Handler'lar, Exception'lar
├── Sentinel.Infrastructure  → JWT, Redis, RabbitMQ, PasswordHasher
├── Sentinel.Persistence     → EF Core, PostgreSQL, Repository'ler
└── Sentinel.API             → Controller'lar, Middleware'ler, Program.cs
```

**Katman bağımlılıkları:**
```
API → Application ← Infrastructure
API → Persistence
Domain ← Application ← Infrastructure
Domain ← Persistence
```

---

## Özellikler

### 🔐 Kimlik Doğrulama
- Kullanıcı kaydı (Register)
- Email + şifre ile giriş (Login)
- JWT Access Token üretimi (`jti` claim dahil)
- Refresh Token üretimi ve rotasyonu
- Logout (Token blacklist)

### 🔴 Redis Entegrasyonu
- **Token Blacklist:** Logout sonrası access token geçersiz kılınır, token'ın kalan süresi kadar Redis'te tutulur
- **Brute-force Koruması:** 5 başarısız giriş denemesinde hesap 15 dakika bloke edilir

### 🐇 RabbitMQ Entegrasyonu
- Kayıt sonrası `UserRegisteredEvent` publish edilir
- `user.registered` queue'su üzerinden consume edilir
- Exchange: `sentinel.events` (Topic)

### 📋 Merkezi Loglama
- Serilog ile structured logging
- Elasticsearch'e otomatik index
- Kibana üzerinden görüntüleme
- Log formatı: `sentinel-logs-{yyyy.MM.dd}`

### 🛡️ Güvenlik
- BCrypt şifre hash'leme
- JWT `jti` claim ile token tekil kimliği
- Global Exception Middleware (400 / 401 / 422 / 500)
- JWT Blacklist Middleware
- Rate Limiting (login endpoint: 10 istek/dakika)

---

## Teknoloji Stack

| Katman | Teknoloji |
|--------|-----------|
| Framework | .NET 8 |
| ORM | Entity Framework Core 8 |
| Veritabanı | PostgreSQL 16 |
| Cache | Redis 7 |
| Message Broker | RabbitMQ 3 |
| Loglama | Serilog + Elasticsearch 8 + Kibana 8 |
| CQRS | MediatR 12 |
| Validasyon | FluentValidation 11 |
| Container | Docker + Docker Compose |

---

## Kurulum

### Gereksinimler
- Docker
- Docker Compose

### Adımlar

```bash
# Repoyu klonla
git clone <repo-url>
cd Sentinel

# Tüm servisleri ayağa kaldır
docker-compose up -d
```

### Servis URL'leri

| Servis | URL |
|--------|-----|
| Sentinel API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| RabbitMQ Management | http://localhost:15672 |
| Kibana | http://localhost:5601 |
| Elasticsearch | http://localhost:9200 |

### RabbitMQ Credentials
- Username: `sentinel_user`
- Password: `sentinel_pass`

---

## API Endpointleri

### `POST /api/auth/register`
Yeni kullanıcı kaydı oluşturur.

**Request:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Response:** `200 OK` — Oluşturulan kullanıcının `Guid` ID'si

---

### `POST /api/auth/login`
Email ve şifre ile giriş yapar, token çifti döner.

**Request:**
```json
{
  "email": "john@example.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "base64string..."
}
```

---

### `POST /api/auth/refresh`
Refresh token ile yeni token çifti üretir. Eski refresh token revoke edilir.

**Request:**
```json
{
  "refreshToken": "base64string..."
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "newbase64string..."
}
```

---

### `POST /api/auth/logout`
Mevcut access token'ı blacklist'e ekler.

**Headers:** `Authorization: Bearer <token>`

**Response:** `204 No Content`

---

### `GET /api/auth/me`
Token sahibinin bilgilerini döner.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "userId": "guid",
  "email": "john@example.com",
  "username": "johndoe"
}
```

---

## Konfigürasyon

`appsettings.json` veya environment variable'lar üzerinden yapılandırılır.

| Key | Açıklama | Örnek |
|-----|----------|-------|
| `Jwt__Key` | JWT imzalama anahtarı | `SuperSecretKey...` |
| `Jwt__Issuer` | Token üreticisi | `Sentinel` |
| `Jwt__Audience` | Token hedef kitlesi | `SentinelUsers` |
| `Jwt__AccessTokenExpirationHours` | Access token süresi | `1` |
| `Jwt__RefreshTokenExpirationDays` | Refresh token süresi | `7` |
| `ConnectionStrings__PostgreSqlConnection` | PostgreSQL bağlantı string'i | `Host=...` |
| `Redis__ConnectionString` | Redis bağlantı string'i | `localhost:6379` |
| `RabbitMQ__Host` | RabbitMQ host | `localhost` |
| `RabbitMQ__Port` | RabbitMQ port | `5672` |
| `RabbitMQ__Username` | RabbitMQ kullanıcı adı | `guest` |
| `RabbitMQ__Password` | RabbitMQ şifresi | `guest` |
| `Serilog__ElasticsearchUrl` | Elasticsearch URL | `http://localhost:9200` |

---

## Servisler

### Docker Compose Servisleri

| Servis | Image | Port |
|--------|-------|------|
| sentinel-api | custom build | 8080 |
| sentinel-postgresql | postgres:16-alpine | 5432 |
| sentinel-redis | redis:7-alpine | 6379 |
| sentinel-rabbitmq | rabbitmq:3-management-alpine | 5672, 15672 |
| sentinel-elasticsearch | elasticsearch:8.13.0 | 9200 |
| sentinel-kibana | kibana:8.13.0 | 5601 |

### Servisleri Durdurma

```bash
docker-compose down
```

### Servisleri Durdurma ve Volume'ları Silme

```bash
docker-compose down -v
```