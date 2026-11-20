<!-- ─────────────────────────────────────────────────────────────────────────────
     DigiKalaPriceAlert · README (English) · @mmd-yousefiasl
     Persian version: README.fa.md
     ───────────────────────────────────────────────────────────────────────────── -->

<img width="100%" src="https://capsule-render.vercel.app/api?type=waving&color=0:512BD4,1:8E7CF3&height=200&section=header&text=DIGIKALA%20PRICE%20ALERT&fontSize=38&fontColor=ffffff&desc=Real-Time%20Price%20Tracking%20%C2%B7%20Telegram%20and%20SMS%20Alerts%20%C2%B7%20.NET%2010&descSize=16&descAlignY=72&animation=fadeIn" alt="DigiKalaPriceAlert — Real-time DigiKala price monitoring" />

<div align="center">

<b>English</b> | <a href="README.fa.md">فارسی</a>

<a href="https://github.com/mmd-yousefiasl/DigiKalaPriceAlert">
  <img src="https://readme-typing-svg.demolab.com?font=Fira+Code&weight=600&size=18&pause=1200&color=A855F7&center=true&vCenter=true&width=640&height=44&lines=Track+any+DigiKala+product+%E2%80%94+sellers%2C+colors%2C+buy+box;Price+drops%3F+Stock+changes%3F+You%27ll+know+first.;Telegram+and+SMS+alerts+the+moment+the+market+moves;Event-driven+%C2%B7+Clean+Architecture+%C2%B7+.NET+10" alt="Track any DigiKala product — sellers, colors, buy box" />
</a>

<br>

<img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 10" />&nbsp;
<img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC" />&nbsp;
<img src="https://img.shields.io/badge/C%23-512BD4?style=flat-square" alt="C#" />&nbsp;
<img src="https://img.shields.io/badge/PostgreSQL-4169E1?style=flat-square&logo=postgresql&logoColor=white" alt="PostgreSQL" />&nbsp;
<img src="https://img.shields.io/badge/EF%20Core-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="EF Core" />

</div>

<img width="100%" src="https://capsule-render.vercel.app/api?type=rect&color=0:512BD4,1:8E7CF3&height=2&width=1000" alt="" />

<div align="center">
  <img src="https://img.shields.io/badge/OVERVIEW-2F1C6B?style=for-the-badge" alt="Overview" />
</div>

DigiKalaPriceAlert is a full monitoring platform for [DigiKala](https://www.digikala.com) — Iran's largest online marketplace. Users paste a product URL, choose the sellers and colors they care about, and get notified the moment the market moves: price changes, sellers going in or out of stock, and buy box winners changing hands. Alerts arrive through a Telegram bot and patterned SMS, with the full change history on the site.

Beyond the tracking side, the platform talks to the **DigiKala Marketplace API** on behalf of sellers — managing variants, stock and prices — and ships a complete commercial layer: OTP login, subscription plans, online payments, a seller profit calculator, and a full admin panel with CMS features.

<div align="center">
  <img src="https://img.shields.io/badge/HOW_IT_WORKS-2F1C6B?style=for-the-badge" alt="How It Works" />
</div>

The heart of the system is an event-driven monitoring pipeline:

```
PriceMonitorWorker  (BackgroundService · every 15 minutes)
        │
        ▼
Load tracked products ─── only products watched by at least one user
        │
        ▼
Scrape DigiKala API ──── sequential · random 0.8–1.5s delay
        │                 early abort when rate-limited
        ▼
Reconcile scraped market vs database
        │   price change · stock out / back in stock
        │   buy box won / lost · title & image updates
        ▼
Persist per product ──── failure isolation: one bad product
        │                 never poisons the rest of the batch
        ▼
Publish ProductVariantMarketStatusUpdatedEvent (MediatR)
        │                 one event per affected color group
        ▼
MarketAlertService
        ├──► Telegram bot — one rich message per watcher,
        │    with a deep link to their own product page
        ├──► SMS — bounded in-process queue → SmsIr worker
        └──► Site — tracking rows flagged unread in the user panel
```

**What makes this careful rather than naive:**

- **Per-seller, per-color granularity** — variants are flattened into seller rows (`ProductVariantSeller`), so the system tracks *every* seller of *every* color variant: price, rating, activity and buy box status — not a single price per product.
- **Rate-limit awareness** — requests are spaced with a random delay, and the cycle stops early the moment the API signals `RateLimited`, instead of hammering the endpoint.
- **Color groups as the unit of change** — DigiKala's variant id is unique per color × seller combination, so changes are consolidated per color group: one change record, one alert wave per group.

<div align="center">
  <img src="https://img.shields.io/badge/FEATURES-2F1C6B?style=for-the-badge" alt="Features" />
</div>

**For shoppers & watchers**

- **Track any product by URL** — paste a DigiKala link (or product id), preview it in a search modal, and pick what to follow
- **Buy box tracking** — know who wins and loses the buy box as sellers shift prices, via a swappable `IBuyBoxStrategy`
- **Telegram alerts** — richly formatted change reports with a deep link straight to the user's own tracking page
- **SMS alerts** — patterned SMS through SmsIr, dispatched by a dedicated queue consumer worker
- **Unread inbox on the site** — every change flags the affected tracking rows, so nothing is missed in the user panel

**For sellers — DigiKala Marketplace API**

- **Seller catalog & variants** — list seller products and variants directly through the marketplace API
- **Stock & price management** — read and update variant stock and selling prices, including batch updates
- **Pricing intelligence** — winning price, price boundaries, and current promotions per variant
- **Order verification with auto-pricing plans** — order flows understand automated repricing plans

**Platform**

- **OTP mobile login** — phone-number login with verification codes and a resending flow
- **Subscriptions & payments** — plans with features and durations, orders, and online payment with a request/verify gateway flow
- **Full admin panel** — users, roles & permissions, orders, payments, SMS panel, balance transactions, and more
- **CMS built in** — blog with categories, FAQs, tickets with attachments, newsletters, site notifications, sitemap and social links
- **Profit calculator** — a free public tool that estimates net seller profit (commission, VAT, shipping, returns)
- **Operational visibility** — Serilog logging with `Error`/`Fatal` events forwarded, batched, to a private Telegram channel

<div align="center">
  <img src="https://img.shields.io/badge/ARCHITECTURE-2F1C6B?style=for-the-badge" alt="Architecture" />
</div>

The solution follows **Clean Architecture** across four projects, each with a single responsibility:

```
DigiKalaPriceAlert.slnx
├── Domain        # Entities, enums, domain events, shared kernel — pure C#
├── Application   # Services + interfaces, DTOs, MediatR handlers, tools
├── Data          # EF Core DbContext — PostgreSQL via Npgsql
└── Web           # ASP.NET Core MVC host: site, areas, background services
```

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities (`Product`, `ProductVariantSeller`, orders, subscription plans, users), enums, and domain events such as `ProductVariantMarketStatusUpdatedEvent` — plus the shared kernel (`Result`, error definitions, site tools). No EF Core or ASP.NET dependencies. |
| **Application** | 35+ application services behind interfaces, DTOs, MediatR handlers (market alerts, notifications, cache invalidation), helpers, and validation attributes. |
| **Data** | EF Core `DbContext` with the Npgsql provider, retry-enabled execution strategy, and entity configurations. |
| **Web** | Public site + `Admin` and `UserPanel` areas (Razor views), three background services, typed HTTP clients, Serilog pipeline, response compression and bundling. |

**Patterns worth noting**

- **Result pattern** — services return `Result` / `Result<T>` with typed errors (e.g. `DigiKalaErrors.RateLimited`); no exceptions as control flow
- **Event-driven alerting** — the reconciler persists first, publishes second; `MarketStatusAlertHandler` and friends do the notifying
- **Strategy pattern** — `IBuyBoxStrategy` decides buy box winners per color group (`HighestRankBuyBoxStrategy`)
- **Bounded in-process queues** — SMS and Telegram updates flow through bounded channels consumed by workers, so bursts are absorbed rather than dropped
- **Typed HTTP clients** — the DigiKala API client registers custom headers, gzip/brotli decompression and a 15-second timeout centrally in DI

<div align="center">
  <img src="https://img.shields.io/badge/BACKGROUND_SERVICES-2F1C6B?style=for-the-badge" alt="Background Services" />
</div>

| Worker | Cadence | Responsibility |
|--------|---------|----------------|
| `PriceMonitorWorker` | every 15 minutes | scrapes and reconciles all tracked products, publishes change events |
| `TelegramUpdateWorker` | continuous | dequeues Telegram updates and processes each in its own DI scope |
| `SmsConsumerWorker` | continuous | dequeues SMS items and sends patterned messages through SmsIr |

<div align="center">
  <img src="https://img.shields.io/badge/TECH_STACK-2F1C6B?style=for-the-badge" alt="Tech Stack" />
</div>

| Category | Technologies |
|----------|--------------|
| **Runtime / Language** | .NET 10, C# |
| **Framework** | ASP.NET Core MVC (public site, Admin & UserPanel areas) |
| **ORM** | Entity Framework Core 10 |
| **Database** | PostgreSQL (Npgsql provider, retry-on-failure) |
| **Messaging** | MediatR — domain events, notifications, cache invalidation |
| **Caching** | EasyCaching (in-memory) |
| **Notifications** | Telegram Bot API · SmsIr (SMS) · MailKit (email) |
| **Logging** | Serilog + custom Telegram error sink |
| **Integrations** | DigiKala public API · DigiKala Marketplace API |
| **Frontend** | Razor views, HTML, CSS, JavaScript, BundlerMinifier |

<div align="center">
  <img src="https://img.shields.io/badge/KEY_ENGINEERING_DECISIONS-2F1C6B?style=for-the-badge" alt="Key Engineering Decisions" />
</div>

**Rate-limit aware scraping.** DigiKala's public API is not an officially open one. The monitor scrapes sequentially with a random 0.8–1.5s delay between products, and the whole cycle aborts early the moment the API signals a rate limit — politeness is built into the loop, not bolted on.

**Failure isolation per product.** A failed save (constraint conflict, bad data) reverts only that product's pending changes and continues with the next — one poisoned product can never break the rest of the batch or the following retry.

**Persist first, publish after.** Change events reach MediatR only after their database state is committed, so an alert can never reference data that isn't actually there.

**Deactivated sellers stay visible.** Stocked-out (soft-deleted) sellers keep being scraped so they can reactivate later; reconciliation matches on the (product, variant, seller) key, so a returning seller revives its row instead of duplicating it.

**An error sink that never throws.** Serilog `Error`/`Fatal` events are queued through a bounded channel and flushed in batches to a private Telegram channel — kept safely below Telegram's 4096-character limit, and a failure inside the sink can never cause another error.

<div align="center">
  <img src="https://img.shields.io/badge/GETTING_STARTED-2F1C6B?style=for-the-badge" alt="Getting Started" />
</div>

**Prerequisites**

- .NET 10 SDK
- PostgreSQL
- `dotnet-ef` CLI tool (there is a tool manifest in `Web/dotnet-tools.json`)

**Run it**

```
git clone https://github.com/mmd-yousefiasl/DigiKalaPriceAlert.git
cd DigiKalaPriceAlert
dotnet tool restore
dotnet restore
dotnet ef database update --project Data --startup-project Web
dotnet run --project Web
```

**Configure**

The app reads its configuration from `appsettings.json` / user secrets. Before running, fill in:

- the PostgreSQL connection string
- Telegram bot token (bot + private error-report channel)
- SmsIr API key and SMS templates
- payment gateway credentials (merchant pin, request/callback/verify URLs)
- `DkMarketplace` base URL and `SiteTools` values (site URL, support email, monitor batch settings)

<div align="center">
  <img src="https://img.shields.io/badge/CONTACT-2F1C6B?style=for-the-badge" alt="Contact" />
</div>

<p align="center">
  <b>Mohammad Yousefi</b> — Backend Developer (.NET / ASP.NET Core)
</p>

<div align="center">

<a href="mailto:mmdyousefi95@gmail.com">
  <img src="https://img.shields.io/badge/Email-mmdyousefi95%40gmail.com-512BD4?style=for-the-badge&logo=gmail&logoColor=white" alt="Email" />
</a>
<a href="https://github.com/mmd-yousefiasl">
  <img src="https://img.shields.io/badge/GitHub-%40mmd--yousefiasl-512BD4?style=for-the-badge&logo=github&logoColor=white" alt="GitHub" />
</a>
<a href="https://t.me/mmd_u_a">
  <img src="https://img.shields.io/badge/Telegram-%40mmd__u__a-512BD4?style=for-the-badge&logo=telegram&logoColor=white" alt="Telegram" />
</a>

</div>

<br>

<img width="100%" src="https://capsule-render.vercel.app/api?type=waving&color=0:512BD4,1:8E7CF3&height=140&section=footer" alt="" />
