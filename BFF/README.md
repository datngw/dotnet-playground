# Backend for Frontend (BFF) Pattern — Learn by Doing

## Table of Contents

1. [What is BFF?](#1-what-is-bff)
2. [The Problem BFF Solves](#2-the-problem-bff-solves)
3. [Project Architecture Overview](#3-project-architecture-overview)
4. [How to Run](#4-how-to-run)
5. [Concept 1: API Aggregation — Combining Multiple APIs into One](#5-concept-1-api-aggregation--combining-multiple-apis-into-one)
6. [Concept 2: Data Shape Transformation — Reshaping Data for the Frontend](#6-concept-2-data-shape-transformation--reshaping-data-for-the-frontend)
7. [Concept 3: Enrichment — Augmenting Data from Multiple Sources](#7-concept-3-enrichment--augmenting-data-from-multiple-sources)
8. [Concept 4: Graceful Degradation — Partial Responses on Failure](#8-concept-4-graceful-degradation--partial-responses-on-failure)
9. [Concept 5: Resilience — Retry, Circuit Breaker, Timeout](#9-concept-5-resilience--retry-circuit-breaker-timeout)
10. [Concept 6: Caching — Caching at the BFF Layer](#10-concept-6-caching--caching-at-the-bff-layer)
11. [Concept 7: Rate Limiting — Protecting Backend Services](#11-concept-7-rate-limiting--protecting-backend-services)
12. [Concept 8: Multiple BFFs — One Backend per Frontend](#12-concept-8-multiple-bffs--one-backend-per-frontend)
13. [Web BFF vs Mobile BFF — Side by Side](#13-web-bff-vs-mobile-bff--side-by-side)
14. [When to Use BFF? When NOT to?](#14-when-to-use-bff-when-not-to)
15. [Request Flow: How a Request Travels Through the BFF](#15-request-flow-how-a-request-travels-through-the-bff)
16. [Suggested Reading Order](#16-suggested-reading-order)

---

## 1. What is BFF?

**BFF (Backend for Frontend)** is a pattern where you create a **dedicated backend service for each type of frontend**.

Instead of having the frontend call multiple microservices directly, you place an intermediary layer — the BFF — in between. Each BFF is **purpose-built** for a specific client type (Web, Mobile, Desktop, etc.).

```
 ┌──────────┐     ┌──────────┐     ┌──────────┐
 │  Web App │     │ iOS App  │     │ Android  │
 └────┬─────┘     └────┬─────┘     └────┬─────┘
      │                │                │
      ▼                ▼                ▼
┌──────────┐    ┌───────────┐    ┌───────────┐
│ BFF.Web  │    │ BFF.Mobile│    │ BFF.Mobile│
│ (port    │    │ (port     │    │ (port     │
│  6000)   │    │  6001)    │    │  6001)    │
└──┬───┬───┘    └──┬───┬───┘    └──┬───┬───┘
   │   │           │   │           │   │
   ▼   ▼           ▼   ▼           ▼   ▼
 ┌─────────────────────────────────────────┐
 │        Microservices (Backend)          │
 │  ┌───────────┐┌──────────┐┌──────────┐ │
 │  │UserService││OrderSvc  ││ProductSvc│ │
 │  │  :5001    ││  :5002   ││  :5003   │ │
 │  └───────────┘└──────────┘└──────────┘ │
 └─────────────────────────────────────────┘
```

---

## 2. The Problem BFF Solves

### Without BFF — Frontend Calls Microservices Directly

```
Frontend needs to render a Dashboard for user #1:

Step 1: GET http://user-service:5001/api/users/1          → User info
Step 2: GET http://order-service:5002/api/orders/user/1   → Orders
Step 3: GET http://product-service:5003/api/products       → Recommended products
```

**5 Problems:**

| # | Problem | Details |
|---|---------|---------|
| 1 | **Multiple round trips** | 3 HTTP requests = 3 network hops = slow |
| 2 | **Frontend must know every URL** | Frontend hard-codes URLs of 3 services |
| 3 | **Inconsistent error handling** | Each service returns errors differently, frontend must handle each |
| 4 | **Client-side data merging** | Frontend must merge data from 3 sources itself |
| 5 | **No cross-cutting concerns** | No central place for caching, rate limiting, logging |

### With BFF — Frontend Calls a Single Endpoint

```
Frontend only needs:

GET http://bff-web:6000/api/dashboard/1   → All data, pre-aggregated!
```

**The BFF handles everything:**
- Calls 3 services **in parallel** using `Task.WhenAll`
- **Aggregates** data into a response shape tailored for the frontend
- **Handles errors centrally** — if 1 service dies, still returns partial data
- **Caches** responses to reduce backend load
- **Retries + Circuit Breaker** automatically on transient failures

---

## 3. Project Architecture Overview

```
BFF/
├── BFF.slnx
└── src/
    ├── Shared/                          # Shared library (DTOs)
    │   ├── DTOs/
    │   │   ├── UserDto.cs               # User data received from UserService
    │   │   ├── OrderDto.cs              # Order data received from OrderService
    │   │   └── ProductDto.cs            # Product data received from ProductService
    │   └── Exceptions/
    │       └── DownstreamServiceException.cs
    │
    ├── BackendServices/                 # 3 mock microservices
    │   ├── UserService/  (port 5001)
    │   │   ├── Models/User.cs
    │   │   ├── Data/UserStore.cs        # In-memory data (5 users)
    │   │   └── Program.cs               # Minimal API: GET /api/users, GET /api/users/{id}
    │   │
    │   ├── OrderService/ (port 5002)
    │   │   ├── Models/Order.cs
    │   │   ├── Data/OrderStore.cs       # In-memory data (8 orders)
    │   │   └── Program.cs               # Minimal API: GET /api/orders, GET /api/orders/user/{userId}
    │   │
    │   └── ProductService/ (port 5003)
    │       ├── Models/Product.cs
    │       ├── Data/ProductStore.cs     # In-memory data (8 products)
    │       └── Program.cs               # Minimal API: GET /api/products, GET /api/products/batch
    │
    ├── BFF.Web/                         # BFF for Web Frontend (port 6000)
    │   ├── Controllers/
    │   │   ├── DashboardController.cs      # GET /api/dashboard/{userId}
    │   │   ├── UserProfileController.cs    # GET /api/userprofile/{userId}
    │   │   └── OrderDetailController.cs    # GET /api/orderdetail/{orderId}
    │   ├── Services/
    │   │   ├── Interfaces/
    │   │   │   ├── IUserService.cs
    │   │   │   ├── IOrderService.cs
    │   │   │   └── IProductService.cs
    │   │   ├── UserBffService.cs           # Calls UserService via HttpClient
    │   │   ├── OrderBffService.cs          # Calls OrderService via HttpClient
    │   │   └── ProductBffService.cs        # Calls ProductService via HttpClient
    │   ├── Aggregators/
    │   │   ├── IDashboardAggregator.cs
    │   │   └── DashboardAggregator.cs      # ⭐ MOST IMPORTANT FILE
    │   ├── Models/Responses/
    │   │   ├── DashboardResponse.cs        # Response shape for web dashboard
    │   │   ├── UserProfileResponse.cs      # Response shape for profile page
    │   │   └── OrderDetailResponse.cs      # Response shape for order detail (enriched)
    │   ├── Mapping/
    │   │   └── ResponseMapper.cs           # Transforms DTOs → Response models
    │   ├── Middleware/
    │   │   ├── RequestLoggingMiddleware.cs # Logs timing for each request
    │   │   └── ExceptionHandlingMiddleware.cs  # Centralized error catching
    │   └── Program.cs                      # Composition root — wires everything together
    │
    └── BFF.Mobile/                      # BFF for Mobile Frontend (port 6001)
        ├── Endpoints/
        │   ├── DashboardEndpoints.cs       # Compact dashboard for mobile
        │   └── OrderEndpoints.cs           # Simplified order view for mobile
        └── Program.cs                      # Minimal API setup
```

---

## 4. How to Run

Open **5 terminals** and start each service:

```bash
# Terminal 1: User Microservice
dotnet run --project src/BackendServices/UserService
# → http://localhost:5001/swagger

# Terminal 2: Order Microservice
dotnet run --project src/BackendServices/OrderService
# → http://localhost:5002/swagger

# Terminal 3: Product Microservice
dotnet run --project src/BackendServices/ProductService
# → http://localhost:5003/swagger

# Terminal 4: BFF.Web
dotnet run --project src/BFF.Web
# → http://localhost:6000/swagger

# Terminal 5: BFF.Mobile
dotnet run --project src/BFF.Mobile
# → http://localhost:6001/swagger
```

### Quick Test

```bash
# Call BFF.Web — 1 request instead of 3
curl http://localhost:6000/api/dashboard/1

# Call BFF.Mobile — smaller response
curl http://localhost:6001/api/mobile/dashboard/1

# Compare: call each downstream service directly
curl http://localhost:5001/api/users/1
curl http://localhost:5002/api/orders/user/1
curl http://localhost:5003/api/products
```

---

## 5. Concept 1: API Aggregation — Combining Multiple APIs into One

> **File:** `src/BFF.Web/Aggregators/DashboardAggregator.cs`

### The Problem

A dashboard needs: user info + recent orders + product recommendations.
Without a BFF, the frontend must make **3 separate API calls**.

### The Solution: `Task.WhenAll` — Parallel Calls

```csharp
// DashboardAggregator.cs — GetDashboardAsync()

// Initialize 3 tasks AT THE SAME TIME — no await yet
var userTask = _userService.GetUserAsync(userId);
var ordersTask = _orderService.GetOrdersByUserAsync(userId);
var productsTask = _productService.GetAllProductsAsync();

// Each task is an HTTP call to a different service
// They run IN PARALLEL, not sequentially
UserDto? user = null;
List<OrderDto> orders = [];
List<ProductDto> products = [];

try { user = await userTask; }
catch (Exception ex) { /* UserService failed → user = null */ }

try { orders = await ordersTask; }
catch (Exception ex) { /* OrderService failed → orders = [] */ }

try { products = await productsTask; }
catch (Exception ex) { /* ProductService failed → products = [] */ }

// Combine 3 data sources into 1 response
var response = _mapper.ToDashboardResponse(user, orders, products);
```

### Why Not Sequential?

```
Sequential:  [User 200ms] → [Order 150ms] → [Product 100ms] = 450ms total
Parallel:    [User 200ms]
             [Order 150ms]  } → max(200, 150, 100) = 200ms
             [Product 100ms]
```

**Parallel is ~2x faster** in this scenario.

---

## 6. Concept 2: Data Shape Transformation — Reshaping Data for the Frontend

> **File:** `src/BFF.Web/Mapping/ResponseMapper.cs`

### The Problem

Each microservice returns data in its own **domain model**. But the frontend needs data shaped for a **UI view** — a completely different structure.

```
UserService returns:     { id, username, email, firstName, lastName, createdAt, role, isActive }
OrderService returns:    { id, userId, items: [{ productId, quantity, unitPrice }], status, createdAt, totalAmount }
ProductService returns:  { id, name, description, price, category, stockQuantity, imageUrl, isActive }
```

The dashboard needs: **user name**, **5 most recent orders**, **3 product recommendations**, **total amount spent**.

There's no field called "total amount spent" — it must be **computed** from orders.

### The Solution: ResponseMapper

```csharp
// ResponseMapper.cs — ToDashboardResponse()

public DashboardResponse ToDashboardResponse(
    UserDto? user,
    List<OrderDto> orders,
    List<ProductDto> products)
{
    return new DashboardResponse
    {
        // FLATTEN: Combine firstName + lastName into a single "Name" field
        User = new UserInfo
        {
            Name = $"{user?.FirstName} {user?.LastName}",
            Email = user?.Email ?? string.Empty,
            MemberSince = user?.CreatedAt ?? DateTime.MinValue
        },

        // TRANSFORM: Take 5 most recent orders, keep only needed fields
        RecentOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new OrderSummary
            {
                OrderId = o.Id,
                Date = o.CreatedAt,
                Total = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.Items.Sum(i => i.Quantity)  // COMPUTED field
            }),

        // SELECT: Take only 3 products, keep only name + price + image
        RecommendedProducts = products
            .Take(3)
            .Select(p => new ProductRecommendation
            {
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ImageUrl
            }),

        // COMPUTED: Frontend doesn't need to calculate this itself
        TotalOrders = orders.Count,
        TotalSpent = orders
            .Where(o => o.Status != "Cancelled")  // Exclude cancelled orders
            .Sum(o => o.TotalAmount)
    };
}
```

### Key Takeaway: The BFF Acts as a "Data Contract" Between Frontend and Backend

The frontend **never knows** about the domain models of UserService or OrderService.
It only knows the response shapes the BFF defines — `DashboardResponse`, `UserProfileResponse`, etc.

If the backend changes a field name? → Only `ResponseMapper` needs updating. The frontend stays the same.

---

## 7. Concept 3: Enrichment — Augmenting Data from Multiple Sources

> **File:** `src/BFF.Web/Controllers/OrderDetailController.cs` + `DashboardAggregator.GetOrderDetailAsync()`

### The Problem

OrderService only stores `ProductId` in each order item. It **doesn't know** the product name or image — that data belongs to ProductService.

```json
// OrderService returns:
{
  "items": [
    { "productId": 1, "quantity": 1, "unitPrice": 199.99 },
    { "productId": 4, "quantity": 1, "unitPrice": 59.99 }
  ]
}
```

The frontend needs: **product name** + **product image** — not just an ID.

### The Solution: BFF Enriches by Calling an Additional Service

```csharp
// DashboardAggregator.cs — GetOrderDetailAsync()

public async Task<OrderDetailResponse?> GetOrderDetailAsync(int orderId)
{
    // 1. Get the order from OrderService
    var order = await _orderService.GetOrderAsync(orderId);
    if (order is null) return null;

    // 2. Collect the ProductIds that need enriching
    var productIds = order.Items.Select(i => i.ProductId).Distinct().ToArray();
    // → [1, 4]

    // 3. Batch fetch from ProductService
    var products = await _productService.GetProductsByIdsAsync(productIds);

    // 4. Map — merge order items with product details
    return _mapper.ToOrderDetailResponse(order, products);
}
```

Result:

```json
{
  "orderId": 1,
  "items": [
    {
      "productId": 1,
      "productName": "Wireless Headphones",      // ← From ProductService!
      "productImage": "/images/headphones.jpg",   // ← From ProductService!
      "quantity": 1,
      "unitPrice": 199.99,
      "lineTotal": 199.99
    },
    {
      "productId": 4,
      "productName": "Mouse Pad",                // ← From ProductService!
      "productImage": "/images/mousepad.jpg",     // ← From ProductService!
      "quantity": 1,
      "unitPrice": 59.99,
      "lineTotal": 59.99
    }
  ]
}
```

### Key Takeaway: Each Service Only Knows Its Own Domain

- OrderService only knows: productId, quantity, price
- ProductService only knows: name, description, imageUrl
- **The BFF** is the only layer that "knows everything" and combines it all

This is why the BFF exists — **no single service has enough data** to render a complete page.

---

## 8. Concept 4: Graceful Degradation — Partial Responses on Failure

> **File:** `src/BFF.Web/Aggregators/DashboardAggregator.cs` — `GetDashboardAsync()`

### The Problem

If ProductService goes down, should the dashboard go down with it?

**NO.** The user should still see their name and orders. Only the product recommendations would be missing.

### The Solution: Separate Try-Catch for Each Service Call

```csharp
// DashboardAggregator.cs

UserDto? user = null;
List<OrderDto> orders = [];
List<ProductDto> products = [];

try { user = await userTask; }
catch (Exception ex) { _logger.LogWarning(ex, "UserService failed"); }

try { orders = await ordersTask; }
catch (Exception ex) { _logger.LogWarning(ex, "OrderService failed"); }

try { products = await productsTask; }
catch (Exception ex) { _logger.LogWarning(ex, "ProductService failed"); }

// Still build a response — with whatever data we managed to get
var response = _mapper.ToDashboardResponse(user, orders, products);
```

### Result When ProductService Is Down

```json
{
  "user": { "name": "John Doe", "email": "john@example.com" },
  "recentOrders": [ ... ],
  "recommendedProducts": [],         // ← Empty, not a crash
  "totalOrders": 3,
  "totalSpent": 384.94
}
```

### Key Takeaway: The Frontend Never Sees Backend Errors

If the frontend calls ProductService directly and it dies → **blank page**.
With the BFF → frontend gets a normal response (missing one part) → **UX is fine**.

---

## 9. Concept 5: Resilience — Retry, Circuit Breaker, Timeout

> **File:** `src/BFF.Web/Program.cs` — `AddStandardResilienceHandler()`

### The Problem

Microservices can have transient failures: network glitches, service restarts, overload.
If the BFF tries once and fails → it just fails → not good enough.

### The Solution: Resilience Pipeline (via Microsoft.Extensions.Http.Resilience)

```csharp
// Program.cs

builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
})
.AddStandardResilienceHandler();
// ↑ Automatically adds 3 policies:
```

`AddStandardResilienceHandler()` adds automatically:

```
Request to UserService
         │
         ▼
   ┌─────────────┐
   │ Total Timeout│  ← 30s total for the entire request (including retries)
   └──────┬──────┘
          ▼
   ┌─────────────┐
   │    Retry     │  ← Up to 3 retries, exponential backoff
   │  (3 times)  │     Attempt 1: ~0.5s, Attempt 2: ~1s, Attempt 3: ~2s
   └──────┬──────┘
          ▼
   ┌──────────────┐
   │    Circuit    │  ← If 50% of requests fail within 30s
   │   Breaker     │     → Open circuit, block requests for 30s
   └──────┬───────┘
          ▼
   ┌─────────────┐
   │   Attempt    │  ← 10s timeout per individual attempt
   │   Timeout    │
   └──────┬──────┘
          ▼
      Call UserService
```

### Circuit Breaker Explained

```
Closed (normal)           → Requests pass through normally
        │
        │ High failure rate (>50% in 10 requests / 30s)
        ▼
Open (tripped)            → Block ALL requests, return error immediately
        │                   (no downstream call, saves resources)
        │ After 30s
        ▼
Half-Open                 → Let 1 trial request through
        │
   Success → Closed       │  Failure → Open
```

### Experiment

1. Start all services
2. Call `GET /api/dashboard/1` → works normally
3. **Stop ProductService** (Ctrl+C on terminal 3)
4. Call again → still works (graceful degradation) + logs show 3 retries
5. Restart ProductService → call again → back to normal

---

## 10. Concept 6: Caching — Caching at the BFF Layer

> **File:** `src/BFF.Web/Aggregators/DashboardAggregator.cs`

### The Problem

The dashboard loads every time a user opens the page. If 100 users load it in 30s → **300 HTTP calls** to backend (100 × 3 services).

Most of this data doesn't change for several seconds. Why call again?

### The Solution: IMemoryCache with Different TTLs

```csharp
// DashboardAggregator.cs

public async Task<DashboardResponse> GetDashboardAsync(int userId)
{
    var cacheKey = $"dashboard:{userId}";

    // Check cache first
    if (_cache.TryGetValue(cacheKey, out DashboardResponse? cached))
    {
        _logger.LogInformation("Dashboard cache hit for user {UserId}", userId);
        return cached!;  // ← Return immediately, NO downstream calls
    }

    // Cache miss → call 3 services, aggregate data
    var response = _mapper.ToDashboardResponse(user, orders, products);

    // Store in cache with an appropriate TTL
    _cache.Set(cacheKey, response, TimeSpan.FromSeconds(30));
    return response;
}
```

### TTL Strategy

| Data | TTL | Reason |
|------|-----|--------|
| Dashboard | 30 seconds | Acceptable staleness, significant load reduction |
| User Profile | 5 minutes | User info rarely changes |
| Product List | 2 minutes | Products change infrequently |
| Order Detail | **No cache** | Must always be fresh — user just placed an order |

### Experiment

1. Call `GET /api/dashboard/1` for the first time → logs show 3 downstream calls
2. Call again immediately → logs show "Dashboard cache hit" + **no downstream calls**
3. Wait 30s → call again → cache expired → downstream calls resume

---

## 11. Concept 7: Rate Limiting — Protecting Backend Services

> **File:** `src/BFF.Web/Program.cs` — `AddRateLimiter()`

### The Problem

If a frontend has a bug and sends 1000 requests/second to the BFF → BFF forwards all of them to the backend → **backend dies**.

### The Solution

```csharp
// Program.cs

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("BffRateLimit", opt =>
    {
        opt.PermitLimit = 100;                  // Max 100 requests
        opt.Window = TimeSpan.FromSeconds(60);  // Per 60 seconds
    });
    options.RejectionStatusCode = 429;          // Over limit → 429 Too Many Requests
});
```

### Experiment

```bash
# Send 110 rapid requests
for i in $(seq 1 110); do curl -s -o /dev/null -w "%{http_code}\n" http://localhost:6000/api/dashboard/1; done

# Result:
# 200 (×100)
# 429 (×10)  ← Requests rejected
```

---

## 12. Concept 8: Multiple BFFs — One Backend per Frontend

> **Files:** `src/BFF.Web/` vs `src/BFF.Mobile/`

### The Problem

Web and Mobile have **different needs**:

| | Web | Mobile |
|---|-----|--------|
| Screen | Large → more data | Small → less data |
| Connection | Stable WiFi | Flaky 4G |
| Battery | Not a concern | Needs optimization |
| Bandwidth | Abundant | Must minimize |

**Same dashboard concept**, but Web needs more data than Mobile.

### BFF.Web Response (`GET /api/dashboard/1`)

```json
{
  "user": { "name": "John Doe", "email": "john@example.com", "memberSince": "2024-01-15" },
  "recentOrders": [
    { "orderId": 3, "date": "2024-12-20", "total": 89.97, "status": "Processing", "itemCount": 3 },
    { "orderId": 2, "date": "2024-11-15", "total": 34.99, "status": "Shipped", "itemCount": 1 },
    { "orderId": 1, "date": "2024-10-01", "total": 259.98, "status": "Delivered", "itemCount": 2 }
  ],
  "recommendedProducts": [
    { "name": "Wireless Headphones", "price": 199.99, "imageUrl": "/images/headphones.jpg" },
    { "name": "Mechanical Keyboard", "price": 449.99, "imageUrl": "/images/keyboard.jpg" },
    { "name": "Programming Book", "price": 29.99, "imageUrl": "/images/book.jpg" }
  ],
  "totalOrders": 3,
  "totalSpent": 384.94
}
```

### BFF.Mobile Response (`GET /api/mobile/dashboard/1`)

```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "recentOrders": [
    { "id": 3, "total": 89.97, "status": "Processing", "date": "2024-12-20" },
    { "id": 2, "total": 34.99, "status": "Shipped", "date": "2024-11-15" },
    { "id": 1, "total": 259.98, "status": "Delivered", "date": "2024-10-01" }
  ],
  "totalOrders": 3
}
```

### Differences

| | BFF.Web | BFF.Mobile |
|---|---------|------------|
| Recent orders count | 5 | 3 |
| Product recommendations | Yes (3 products) | **No** |
| User format | Nested object | Flat |
| TotalSpent | Yes | **No** |
| Cache TTL | 30 seconds | **5 minutes** (more aggressive) |
| API style | Controller-based | Minimal API (lighter) |

---

## 13. Web BFF vs Mobile BFF — Side by Side

### Same Downstream Services, Different Aggregation

```
                UserService    OrderService    ProductService
                     │              │                │
                     ▼              ▼                ▼
               ┌─────────────────────────────────────────┐
               │           Shared DTOs (BFF.Shared)       │
               └─────────┬──────────────────┬────────────┘
                         │                  │
            ┌────────────▼──────┐  ┌────────▼───────────┐
            │     BFF.Web       │  │    BFF.Mobile       │
            │                   │  │                      │
            │ DashboardController│  │ DashboardEndpoints   │
            │ - 5 orders        │  │ - 3 orders           │
            │ - 3 products      │  │ - no products        │
            │ - full stats      │  │ - simple stats       │
            │ - cache 30s       │  │ - cache 5min         │
            └────────┬──────────┘  └─────────┬────────────┘
                     │                        │
              Web Frontend              Mobile Frontend
```

### Key Insight

**BFF is NOT an API Gateway.** An API Gateway forwards the same requests to all clients. A BFF **produces different responses** for each client type.

---

## 14. When to Use BFF? When NOT to?

### Use BFF When

- You have **multiple frontend types** (Web + Mobile + Desktop) with different needs
- The frontend needs to call **3+ APIs** to render a single page
- The backend consists of **multiple microservices** that need aggregation
- You need centralized **cross-cutting concerns** (auth, caching, rate limiting, logging)
- Backend services are owned by **other teams** and their APIs can't be changed

### Don't Use BFF When

- You only have **one frontend** → no need for multiple BFFs
- The backend is a **single monolith** → aggregation is unnecessary
- The frontend only calls **1-2 APIs** → overhead isn't justified
- The team is **too small** → maintaining a BFF adds operational cost

---

## 15. Request Flow: How a Request Travels Through the BFF

```
User opens Dashboard in Web App
         │
         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Frontend: fetch('http://localhost:6000/api/dashboard/1')        │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────────┐
│ BFF.Web (port 6000)                                              │
│                                                                  │
│  1. Rate Liminter          → Check: quota remaining? (100/min)  │
│  2. RequestLogging         → Log: "→ GET /api/dashboard/1"      │
│  3. ExceptionHandling      → Setup try-catch wrapper             │
│  4. DashboardController    → Receive request, call Aggregator    │
│  5. DashboardAggregator:                                         │
│     a. Check cache         → "dashboard:1" → MISS                │
│     b. Create 3 Tasks      → userTask, ordersTask, productsTask  │
│     c. Parallel execution  → Run 3 HTTP calls simultaneously     │
│        │                                                         │
│        ├→ HttpClient("UserService")                              │
│        │  ├── Retry (max 3)                                      │
│        │  ├── Circuit Breaker                                    │
│        │  └── GET http://localhost:5001/api/users/1               │
│        │              │                                          │
│        │              ▼                                          │
│        │     ┌──────────────────┐                                │
│        │     │ UserService :5001│ → { id: 1, name: "John" ... }  │
│        │     └──────────────────┘                                │
│        │                                                         │
│        ├→ HttpClient("OrderService")                             │
│        │  └── GET http://localhost:5002/api/orders/user/1         │
│        │              │                                          │
│        │              ▼                                          │
│        │     ┌───────────────────┐                               │
│        │     │ OrderService :5002│ → [{ id: 1, items: [...] }]   │
│        │     └───────────────────┘                               │
│        │                                                         │
│        └→ HttpClient("ProductService")                           │
│           └── GET http://localhost:5003/api/products              │
│                     │                                            │
│                     ▼                                            │
│          ┌─────────────────────┐                                 │
│          │ ProductService :5003│ → [{ id: 1, name: "Headphones"}]│
│          └─────────────────────┘                                 │
│                                                                  │
│     d. ResponseMapper       → Transform 3 DTOs into 1 Dashboard │
│     e. Cache.Set            → Store "dashboard:1" for 30s       │
│                                                                  │
│  6. Return Ok(response)     → 200 OK                             │
│  7. RequestLogging          → Log: "← GET /api/dashboard/1 → 200 (152ms)"│
└──────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ Frontend receives:                                               │
│ {                                                                │
│   "user": { "name": "John Doe", ... },                          │
│   "recentOrders": [...],                                         │
│   "recommendedProducts": [...],                                  │
│   "totalOrders": 3,                                              │
│   "totalSpent": 384.94                                           │
│ }                                                                │
│                                                                  │
│ → 1 single request, data ready to render!                        │
└─────────────────────────────────────────────────────────────────┘
```

---

## 16. Suggested Reading Order

Read files in this order to build understanding progressively:

| Order | File | What You'll Learn |
|-------|------|-------------------|
| 1 | `src/BFF.Web/Program.cs` | Composition root — where everything is wired together |
| 2 | `src/BFF.Web/Services/UserBffService.cs` | HttpClient Factory pattern — calling a downstream service |
| 3 | `src/BFF.Web/Aggregators/DashboardAggregator.cs` | **Aggregation + graceful degradation + caching** — the heart of the BFF |
| 4 | `src/BFF.Web/Mapping/ResponseMapper.cs` | Data transformation — reshaping for the frontend |
| 5 | `src/BFF.Web/Controllers/DashboardController.cs` | Thin controller — logic lives in the aggregator |
| 6 | `src/BFF.Web/Controllers/OrderDetailController.cs` | Enrichment pattern — enriching orders with product data |
| 7 | `src/BFF.Web/Middleware/ExceptionHandlingMiddleware.cs` | Centralized error handling |
| 8 | `src/BFF.Web/Middleware/RequestLoggingMiddleware.cs` | Request timing logs |
| 9 | `src/BFF.Mobile/Endpoints/DashboardEndpoints.cs` | Mobile BFF — same concept, smaller response |
| 10 | `src/BackendServices/UserService/Program.cs` | Downstream service — simple Minimal API |

---

## Summary

```
┌──────────────────────────────────────────────────────────────┐
│                    BFF Pattern Checklist                      │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ✓ Aggregation    — 1 request instead of N requests          │
│  ✓ Transformation — Data shape tailored for each client      │
│  ✓ Enrichment     — Combine data from multiple services      │
│  ✓ Degradation    — Partial response when a service fails    │
│  ✓ Resilience     — Retry + Circuit Breaker + Timeout        │
│  ✓ Caching        — IMemoryCache with appropriate TTLs       │
│  ✓ Rate Limiting  — Protect backend from overload            │
│  ✓ Multi-BFF      — Web BFF ≠ Mobile BFF                    │
│                                                              │
│  Principle: Frontend only talks to the BFF.                  │
│  The BFF talks to all microservices.                         │
│  Each frontend type gets its own BFF.                         │
└──────────────────────────────────────────────────────────────┘
```
