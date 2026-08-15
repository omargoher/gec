# Category Service — Technical Documentation

> **Project:** GEC (E-Commerce Platform)
> **Layer:** `GEC.ApplicationCore` (Service) · `GEC.Infrastructure` (Repository) · `GEC.API` (Controller)
> **Last updated:** 2026-08-17

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture & Folder Structure](#2-architecture--folder-structure)
3. [Data Model](#3-data-model)
4. [Repository Layer — Method by Method](#4-repository-layer--method-by-method)
5. [Service Layer — Method by Method](#5-service-layer--method-by-method)
6. [Controller / API Layer](#6-controller--api-layer)
7. [Business Behavior & Rules](#7-business-behavior--rules)
8. [All Possible Cases / Scenarios](#8-all-possible-cases--scenarios)
9. [Decisions & Trade-offs](#9-decisions--trade-offs)
10. [Examples](#10-examples)

---

## 1. Overview

### What This Service Does

The **Category Service** is responsible for the full lifecycle management of product categories in the GEC e-commerce platform. It provides:

- **Hierarchical category organization** — categories are arranged in a self-referencing tree (up to 7 levels deep) that mirrors how a user would browse a product catalogue (e.g., _Electronics → Smartphones → Android Phones_).
- **Public read access** — browsing the category tree and retrieving individual category details (with breadcrumbs and direct sub-categories) for display on a storefront.
- **Admin write access** — creating, updating, and deleting categories, including managing parent-child relationships and active/inactive state.
- **Slug management** — unique, URL-safe slugs for SEO-friendly category URLs, with a dedicated availability-check endpoint.
- **Breadcrumb generation** — the ordered list of ancestor categories from root to the current category, used for navigation UI.

### Bounded Context & Dependencies

| Dependency | Direction | Nature |
|---|---|---|
| **Product Service** _(not yet implemented)_ | Upstream | Categories will be linked to products via `CategoryId` on the product entity. Deletion protection for categories with products is noted as a TODO. |
| **ASP.NET Identity** | Sideways | Authentication/authorization only — the Category Service does not read or write identity data. |
| **`ICacheService`** | Infrastructure | A memory-cache abstraction exists in the system but is **not currently used** by the Category Service. |
| **`IUnitOfWork`** | Infrastructure | All database access is mediated through the Unit of Work pattern. |

The Category Service has **no runtime dependency** on other domain services (Address, Profile, Auth). It is a standalone bounded context within the application layer.

---

## 2. Architecture & Folder Structure

### Folder Map

```
GEC.Domain/
└── Entities/
    ├── BaseEntity.cs              # Abstract base: Id, CreatedAt, UpdatedAt
    └── Category.cs                # Category entity (self-referencing tree)

GEC.ApplicationCore/
├── Interfaces/
│   ├── Repositories/
│   │   ├── IBaseRepository.cs     # Generic CRUD contract
│   │   └── ICategoryRepository.cs # Category-specific query contract
│   ├── Persistence/
│   │   └── IUnitOfWork.cs         # Transaction + repository access contract
│   └── Services/
│       └── ICategoryService.cs    # Category business logic contract
├── DTOs/
│   └── Categories/
│       ├── BreadcrumbDto.cs
│       ├── CategoryAdminTreeNodeDto.cs
│       ├── CategoryDto.cs                         # Lightweight flat record
│       ├── CategoryFilterParams.cs                # For future paginated list
│       ├── CategoryResponse.cs                    # Full public response shape
│       ├── CategoryTreeNodeDto.cs
│       ├── CheckSlugAvailabilityRequest.cs
│       ├── CheckSlugAvailabilityRequestValidator.cs
│       ├── CreateCategoryRequest.cs
│       ├── CreateCategoryRequestValidator.cs
│       ├── GetBreadcrumbsRequest.cs
│       ├── GetBreadcrumbsRequestValidator.cs
│       ├── SubCategoryDto.cs
│       ├── UpdateCategoryRequest.cs
│       └── UpdateCategoryRequestValidator.cs
├── Enums/
│   ├── CategorySearchBy.cs        # Enum: Name | Slug | Description | All
│   ├── CategorySortBy.cs          # Enum: DisplayOrder | Name | CreatedAt | ProductCount
│   └── SortDirection.cs
├── Exceptions/
│   ├── ConflictException.cs       # HTTP 409
│   ├── NotFoundException.cs       # HTTP 404
│   └── ...                        # Other domain exceptions
└── Services/
    └── CategoryService.cs         # Business logic implementation

GEC.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs    # EF Core DbContext (PostgreSQL + snake_case)
│   └── Configurations/
│       └── CategoryEntityTypeConfiguration.cs   # Fluent API table/column config
├── Repositories/
│   ├── BaseRepository.cs          # Generic EF CRUD
│   └── CategoryRepository.cs      # Category-specific queries
└── UnitOfWork/
    └── UnitOfWork.cs              # Aggregates all repositories + transaction management

GEC.API/
└── Controllers/
    └── CategoryController.cs      # REST endpoints (10 endpoints)
```

### Layer Responsibilities

| Layer | File(s) | Responsibility |
|---|---|---|
| **Domain** | `Category.cs`, `BaseEntity.cs` | Pure data model. No logic, no infrastructure. Defines what a Category _is_. |
| **Application Core** | `ICategoryService`, `CategoryService`, DTOs, Validators | Orchestrates business rules. Depends only on interfaces, never on infrastructure types. All business invariants (depth, circularity, active/inactive cascade) live here. |
| **Infrastructure** | `CategoryRepository`, `ApplicationDbContext`, `UnitOfWork` | Translates domain operations into database queries (EF Core + PostgreSQL). Implements interfaces defined in the Application Core. |
| **API** | `CategoryController` | HTTP boundary — deserializes HTTP requests, delegates to the service, serializes HTTP responses. Authorization enforcement via `[Authorize]` attributes. |

### Why This Layering Exists

- **Separation of Concerns:** Each layer has a single reason to change. Switching from PostgreSQL to another database requires changes only in `Infrastructure`, not in `ApplicationCore`.
- **Testability:** `CategoryService` can be unit-tested against a mock `IUnitOfWork` without a real database. The controller can be tested against a mock `ICategoryService`.
- **Dependency Inversion:** High-level modules (`CategoryService`) depend on abstractions (`ICategoryRepository`), not concretions. This prevents tight coupling.
- **Explicit Transaction Boundary:** The `IUnitOfWork` pattern ensures that multiple repository operations can be wrapped in a single database transaction, preventing partial writes.

### Dependency Injection

```csharp
// GEC.ApplicationCore/DependencyInjection.cs
services.AddScoped<ICategoryService, CategoryService>();

// GEC.Infrastructure/DependencyInjection.cs
services.AddScoped<IUnitOfWork, UnitOfWork>();
// UnitOfWork internally instantiates CategoryRepository
```

Both `ICategoryService` and `IUnitOfWork` are registered as **Scoped** (one instance per HTTP request), which is the correct lifetime for EF Core DbContext-backed services.

---

## 3. Data Model

### Entity: `Category`

**Inherits:** `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`)

**Database table:** `categories` (PostgreSQL snake_case convention)

#### Column Reference

| Property | Column | CLR Type | DB Type | Nullable | Default | Max Length | Constraints |
|---|---|---|---|---|---|---|---|
| `Id` | `id` | `Guid` | `uuid` | NOT NULL | `Guid.NewGuid()` | — | **PRIMARY KEY** |
| `ParentId` | `parent_id` | `Guid?` | `uuid` | NULL | `null` | — | **FK → categories.id** (Restrict on delete) |
| `Name` | `name` | `string` | `varchar(100)` | NOT NULL | — | 100 | Required |
| `Slug` | `slug` | `string` | `varchar(100)` | NOT NULL | — | 100 | Required, **UNIQUE INDEX** |
| `Description` | `description` | `string?` | `varchar(255)` | NULL | `null` | 255 | — |
| `IsActive` | `is_active` | `bool` | `boolean` | NOT NULL | `true` | — | Required |
| `Icon` | `icon` | `string?` | `text` | NULL | `null` | 100 (service layer) | Lucide icon name, e.g. `"smartphone"` |
| `ImageUrl` | `image_url` | `string?` | `text` | NULL | `null` | 2048 (service layer) | Must be a valid absolute URL when provided |
| `CreatedAt` | `created_at` | `DateTime` | `timestamptz` | NOT NULL | Set by `DbContext.SaveChangesAsync` | — | Auto-managed |
| `UpdatedAt` | `updated_at` | `DateTime?` | `timestamptz` | NULL | Set on modification | — | Auto-managed |

> ⚠️ **Needs clarification:** `Icon` and `ImageUrl` have no `HasMaxLength` defined in the EF Fluent configuration (`CategoryEntityTypeConfiguration`), only in the FluentValidation validators. This means the database column is `text` (unlimited), but the application enforces the 100/2048 character limits. These constraints should be aligned with `HasMaxLength` in the EF configuration for database-level safety.

> ⚠️ **Needs clarification:** `Description` is defined as `HasMaxLength(255)` in the EF configuration, but the validator allows up to `1000` characters. The database will reject any description longer than 255 characters with a database-level constraint violation error rather than a clean validation error. This is an inconsistency between the infrastructure configuration and the application validation.

#### Indexes

| Index | Columns | Unique | Purpose |
|---|---|---|---|
| Primary Key | `id` | Yes | Row identity |
| `ix_categories_slug` | `slug` | Yes | Enforces global slug uniqueness at the database level; supports fast slug lookups |

#### Relationships

| Relationship | Type | FK | Cascade |
|---|---|---|---|
| `Category` → `Category` (parent) | Many-to-One | `parent_id → id` | **Restrict** — the database will prevent deletion of a parent that has children |

The `Restrict` delete behavior is the correct choice here because:
1. The service layer already enforces the "no children" rule before attempting a delete.
2. The database-level `Restrict` acts as a safety net for any direct DB operations or future code paths that bypass the service.

#### Navigation Properties

| Property | Type | Description |
|---|---|---|
| `Parent` | `Category?` | Reference to the parent category. `null` for root categories. |
| `Children` | `ICollection<Category>` | Direct children of this category. Initialized as an empty `List<Category>`. |

#### Business Rationale for Constraints

- **Slug uniqueness (global):** The slug is used as a URL segment (e.g., `/categories/smartphones`). It must be globally unique to guarantee that every category has a distinct, unambiguous URL. A per-parent uniqueness model was not chosen because slugs appear in flat URL paths, not hierarchical ones.
- **`Restrict` on delete:** Prevents orphaned children (categories whose parent no longer exists), which would corrupt the tree structure.
- **`IsActive` default `true`:** A newly created category is immediately visible. Admins must explicitly disable a category, which is the less common operation.
- **`Icon` as string (not enum/FK):** Icon names are sourced from the Lucide icon library. Storing them as strings avoids a rigid enum that would require code changes for every new icon and allows the frontend to render icons without backend schema changes.

---

## 4. Repository Layer — Method by Method

All repository methods are accessed via `IUnitOfWork.Category`. The `CategoryRepository` inherits from `BaseRepository<Category>`, which provides the four generic CRUD operations described first.

### Inherited from `BaseRepository<Category>`

---

#### `GetByIdAsync`

```csharp
Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
```

**Purpose:** Generic primary-key lookup using EF Core's `FindAsync`, which checks the local change tracker cache before hitting the database.

**Query logic:** `DbSet<T>.FindAsync(id)` — uses EF's identity map. No navigation properties are eagerly loaded.

**Performance:** Fast for tracked entities (cache hit). Does not load children or parent. Used in `UpdateCategoryAsync` when a quick existence/status check of the parent is needed.

---

#### `Add`

```csharp
void Add(Category entity)
```

**Purpose:** Registers a new `Category` entity into the EF change tracker in the `Added` state. The actual `INSERT` is deferred until `SaveChangesAsync` is called.

**Note:** This method is synchronous because EF's change tracking is an in-memory operation.

---

#### `Update`

```csharp
void Update(Category entity)
```

**Purpose:** Marks an entity as `Modified` in the change tracker, causing EF to emit an `UPDATE` statement for all columns on `SaveChangesAsync`.

**Note:** Since the entity is already retrieved from the database within the same DbContext lifetime (same scoped request), the entity is already tracked. Calling `Update` is technically redundant in these scenarios but is retained for explicitness.

---

#### `Remove`

```csharp
void Remove(Category entity)
```

**Purpose:** Marks an entity for deletion in the change tracker. The `DELETE` statement is issued on `SaveChangesAsync`. The database-level `Restrict` FK constraint provides a second line of defense.

---

### Category-Specific Methods

---

#### `GetByIdWithChildrenAsync`

```csharp
Task<Category?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
```

**Purpose:** Retrieves a category **with all of its direct children** (no active filter). Used by mutation operations (`UpdateCategoryAsync`, `DeleteCategoryAsync`) that need to inspect or enforce rules against the full child collection.

**Query logic:**
```sql
SELECT c.*, ch.*
FROM categories c
LEFT JOIN categories ch ON ch.parent_id = c.id
WHERE c.id = @id
```
(EF translates the `Include(c => c.Children)` to a JOIN or split query.)

**Why include all children (not just active)?** The `UpdateCategoryAsync` must check whether *any* active child exists when attempting to deactivate. `DeleteCategoryAsync` must block deletion if *any* child exists (active or inactive). Loading only active children would create a silent data integrity gap.

**Performance:** Single round-trip with a JOIN. Not paginated — category children counts are bounded by the business model (limited depth and fan-out in a product catalogue). No N+1 risk. The entity **is tracked** by the change tracker (no `AsNoTracking`), which is required for subsequent mutation.

---

#### `GetByIdWithActiveChildrenAsync`

```csharp
Task<Category?> GetByIdWithActiveChildrenAsync(Guid id, CancellationToken cancellationToken = default)
```

**Purpose:** Retrieves a single **active** category with only its **active** direct children, for public storefront display.

**Query logic:**
```sql
SELECT c.*, ch.*
FROM categories c
LEFT JOIN categories ch ON ch.parent_id = c.id AND ch.is_active = true
WHERE c.id = @id AND c.is_active = true
```

**Why filter active-only?** Storefront customers must never see inactive categories or inactive sub-categories, as these represent catalogue items that are either discontinued or not yet published.

**Performance:** Uses `AsNoTracking()` — the result is read-only, so EF does not need to track the entity in the change tracker, reducing memory overhead and improving performance.

---

#### `GetBySlugWithActiveChildrenAsync`

```csharp
Task<Category?> GetBySlugWithActiveChildrenAsync(string slug, CancellationToken cancellationToken = default)
```

**Purpose:** Identical behavior to `GetByIdWithActiveChildrenAsync` but looked up by `slug` instead of `id`. Supports slug-based URL routing (`/categories/smartphones`).

**Query logic:**
```sql
SELECT c.*, ch.*
FROM categories c
LEFT JOIN categories ch ON ch.parent_id = c.id AND ch.is_active = true
WHERE c.slug = @slug AND c.is_active = true
```

**Performance:** The `slug` column has a unique index. This query is index-bound (fast). Uses `AsNoTracking()`.

---

#### `GetAllAsync`

```csharp
Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
```

**Purpose:** Loads the **entire category table** as a flat list of lightweight `CategoryDto` records into application memory. Used by the service layer for:
1. Breadcrumb generation (ancestor traversal).
2. Parent validation (circularity check, depth calculation).

**Query logic:**
```sql
SELECT id, parent_id, name, slug, is_active FROM categories
```
(Projection to `CategoryDto` — only required columns are fetched.)

**Performance considerations:**
- This is a **full-table scan** on every call. It scales linearly with the number of categories.
- The service layer then builds an in-memory dictionary (`O(n)`) for `O(1)` per-node lookups during ancestor traversal.
- For typical e-commerce catalogues (hundreds to a few thousand categories), this is efficient. For catalogues with tens of thousands of categories, a dedicated recursive CTE query or caching strategy would be warranted.
- ⚠️ **No caching is currently applied.** Each call to `GetBreadcrumbsBySlugAsync` and `ValidateParentCategoryAsync` triggers a fresh database round-trip loading all categories. If these methods are called multiple times within a single request (e.g., in `CreateCategoryAsync`, breadcrumbs are fetched after create), multiple full-table loads occur.
- Uses `AsNoTracking()`.

---

#### `GetFlatAdminTreeNodesAsync`

```csharp
Task<List<CategoryAdminTreeNodeDto>> GetFlatAdminTreeNodesAsync(CancellationToken cancellationToken = default)
```

**Purpose:** Loads **all categories** (active and inactive) as a flat list of `CategoryAdminTreeNodeDto` records. The service layer assembles these into a nested tree in memory.

**Query logic:**
```sql
SELECT id, parent_id, name, slug, icon, image_url, is_active FROM categories
```

**Why return a flat list instead of a nested result?** SQL is inherently a flat, tabular format. Building the nested tree in C# via a dictionary lookup is `O(n)` and avoids complex recursive CTEs, which are harder to maintain and less portable. This is the correct approach for the expected data volumes.

**Performance:** Full table scan with projection. `AsNoTracking()`.

---

#### `GetFlatActiveTreeNodesAsync`

```csharp
Task<List<CategoryTreeNodeDto>> GetFlatActiveTreeNodesAsync(CancellationToken cancellationToken = default)
```

**Purpose:** Identical to `GetFlatAdminTreeNodesAsync` but filters to **active categories only** and returns the slimmer `CategoryTreeNodeDto` (no `IsActive`, no `ImageUrl`).

**Query logic:**
```sql
SELECT id, parent_id, name, slug, icon FROM categories WHERE is_active = true
```

**Performance:** The `is_active` column currently does not have a dedicated index. For large catalogues, an index on `is_active` (or a partial index `WHERE is_active = true`) would improve this query. `AsNoTracking()`.

---

#### `SlugExistAsync`

```csharp
Task<bool> SlugExistAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
```

**Purpose:** Checks whether a slug is already in use by any category other than the optionally excluded one. The `excludeId` parameter supports the "edit category" scenario, where a category should be able to keep its own current slug without triggering a conflict.

**Query logic:**
```sql
SELECT EXISTS (
  SELECT 1 FROM categories
  WHERE slug = @slug
  AND (@excludeId IS NULL OR id != @excludeId)
)
```

**Performance:** The `slug` unique index makes this an index seek — `O(log n)` at the database level. Highly efficient.

---

## 5. Service Layer — Method by Method

`CategoryService` implements `ICategoryService` and is injected with `IUnitOfWork`. It is registered as **Scoped**.

---

### `CreateCategoryAsync`

```csharp
Task<CategoryResponse> CreateCategoryAsync(
    CreateCategoryRequest request,
    CancellationToken cancellationToken = default)
```

**Purpose:** Creates a new category, enforcing all business invariants before persisting.

**Step-by-step logic:**

1. Normalize the slug to lowercase (`ToLowerInvariant()`).
2. **Slug uniqueness check:** Call `SlugExistAsync(slug, null)`. If the slug already exists, throw `ConflictException("Category slug")` → HTTP 409.
3. **Parent validation (if `ParentId` is provided):** Call `ValidateParentCategoryAsync(null, request.ParentId.Value, request.IsActive)`, which performs:
   - Existence check on the parent.
   - Active/inactive compatibility check.
   - Circular reference check (N/A on create, since the new category has no ID yet; `currentCategoryId` is passed as `null`).
   - Depth constraint: ensures `parentDepth + 1 + 0 (no subtree yet) <= 7`.
4. Construct a new `Category` entity from the request.
5. Call `_unitOfWork.Category.Add(category)` — registers the entity with the change tracker.
6. Call `_unitOfWork.SaveChangesAsync()` — executes the `INSERT`.
7. Fetch breadcrumbs for the newly created category by calling `GetBreadcrumbsBySlugAsync`.
8. Return a fully populated `CategoryResponse`.

**Business rules enforced:**
- Slug must be globally unique.
- If a parent is specified, it must exist.
- An active category cannot be placed under an inactive parent.
- The resulting depth after creation must not exceed 7.

**Exceptions thrown:**

| Exception | Condition | HTTP Status |
|---|---|---|
| `ConflictException` | Slug already exists | 409 |
| `NotFoundException` | `ParentId` provided but parent not found | 404 |
| `InvalidOperationException` | Active category assigned to inactive parent | 400 (mapped by exception handler) |
| `InvalidOperationException` | Depth would exceed 7 | 400 |
| DB Unique Constraint Violation | Two concurrent requests with the same slug race past the check | 500 ⚠️ (see note) |

> ⚠️ **Known gap (noted in code comment):** There is a TOCTOU (time-of-check / time-of-use) race condition between the slug uniqueness check and the `INSERT`. If two concurrent requests submit the same slug simultaneously, both may pass the `SlugExistAsync` check before either has committed to the database. The database's unique index will reject the second `INSERT` with a constraint violation exception, but this exception is currently **not caught and translated** into a clean `ConflictException`. It will propagate as an unhandled 500.

**Side effects:**
- One `INSERT` into the `categories` table.
- No cache invalidation (no cache is currently used by this service).
- No domain events published.

**Atomicity:** The entire operation — slug check, add entity, save — is **not wrapped in an explicit transaction**. The `SaveChangesAsync` call is atomic at the EF level (single INSERT). The slug uniqueness check and the INSERT are not in the same DB transaction, creating the race window described above.

---

### `UpdateCategoryAsync`

```csharp
Task UpdateCategoryAsync(
    Guid categoryId,
    UpdateCategoryRequest request,
    CancellationToken cancellationToken = default)
```

**Purpose:** Partially updates an existing category. All fields in `UpdateCategoryRequest` are nullable — `null` means "do not change this field."

**Step-by-step logic:**

1. Fetch the category by ID **with all children** (`GetByIdWithChildrenAsync`). If not found, throw `NotFoundException("Category")`.
2. **Slug update (if `request.Slug` is provided):**
   - Normalize to lowercase.
   - Call `SlugExistAsync(slug, category.Id)` (self-excluded). If taken, throw `ConflictException("Category slug")`.
3. **Parent update logic (mutually exclusive branches):**
   - If `request.ClearParent == true`: Set `category.ParentId = null` immediately (will be confirmed later).
   - Else if `request.ParentId` is set **and differs from the current parent**: Call `ValidateParentCategoryAsync(category.Id, request.ParentId.Value, ...)`, which checks existence, active compatibility, circular reference, and depth.
4. **Active/inactive state transition (if `request.IsActive` is provided):**
   - **Activating (`true`):** If the parent is not changing (or no new parent specified), and the category currently has a parent, verify the parent's `IsActive` is `true`. Throw `InvalidOperationException` if parent is inactive.
   - **Deactivating (`false`):** Inspect the **loaded** `category.Children`. If any child has `IsActive == true`, throw `InvalidOperationException`.
5. Apply all non-null fields to the entity using the null-coalescing pattern:
   ```csharp
   category.Name = request.Name ?? category.Name;
   category.Slug = slug ?? category.Slug;
   // etc.
   ```
6. Apply the resolved parent:
   ```csharp
   category.ParentId = request.ClearParent == true ? null : (request.ParentId ?? category.ParentId);
   ```
7. Call `_unitOfWork.Category.Update(category)` and `SaveChangesAsync()`.

**Business rules enforced:**
- Category must exist.
- New slug (if supplied) must be unique (excluding self).
- `ClearParent` and `ParentId` are mutually exclusive (enforced at the FluentValidation level before the service is reached).
- Cannot activate a category whose current parent is inactive (and the parent is not simultaneously being changed).
- Cannot deactivate a category that has active children.
- Moving a category to a new parent triggers full parent validation (circular reference, depth, active state).
- A category cannot be set as its own parent.

**Exceptions thrown:**

| Exception | Condition | HTTP Status |
|---|---|---|
| `NotFoundException` | Category not found | 404 |
| `ConflictException` | New slug already taken | 409 |
| `NotFoundException` | New `ParentId` not found | 404 |
| `InvalidOperationException` | Circular parent reference | 400 |
| `InvalidOperationException` | Depth exceeded | 400 |
| `InvalidOperationException` | Activating under inactive parent | 400 |
| `InvalidOperationException` | Deactivating with active children | 400 |

**Side effects:**
- One `UPDATE` on the `categories` table.
- `UpdatedAt` is automatically set by `ApplicationDbContext.SaveChangesAsync`.

> ⚠️ **Needs clarification — Active state check gap:** When `request.IsActive = true` and a new `ParentId` is also provided simultaneously, the code path enters `ValidateParentCategoryAsync` (which checks if the *new* parent is active) and then **skips** the re-activation check at line 93 (`if (request.ParentId == null || request.ParentId == category.ParentId) && category.ParentId.HasValue`). This is correct behavior (the new parent's activity is checked in `ValidateParentCategoryAsync`), but the logic is subtle and should be documented with a code comment.

**Atomicity:** Single `SaveChangesAsync` call — the update is atomic at the database level.

---

### `DeleteCategoryAsync`

```csharp
Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
```

**Purpose:** Permanently (hard) deletes a category, provided it has no children. Products are not yet checked (see TODO).

**Step-by-step logic:**

1. Fetch the category by ID **with all children** (`GetByIdWithChildrenAsync`).
2. If not found, throw `NotFoundException("Category")`.
3. If `category.Children.Any()`, throw `InvalidOperationException` with a descriptive message including the child count.
4. Call `_unitOfWork.Category.Remove(category)` and `SaveChangesAsync()`.

**Business rules enforced:**
- Category must exist.
- Category must have zero children (active or inactive).

**Exceptions thrown:**

| Exception | Condition | HTTP Status |
|---|---|---|
| `NotFoundException` | Category not found | 404 |
| `InvalidOperationException` | Category has one or more children | 409 (mapped) |

> ⚠️ **Known gap (noted in code comment):** Product linkage is not yet implemented. The interface's TODO comment states: "Add Has Products check in DeleteCategoryAsync". Until that check is added, a category that has products assigned to it can be deleted, leaving those products with a dangling foreign key (or `null` category, depending on the product schema). This is a **data integrity risk** that must be addressed before the Product feature is released.

**Side effects:**
- One `DELETE` from the `categories` table.
- The database-level `Restrict` FK constraint is the final guard against deleting a parent.

**Atomicity:** Single `SaveChangesAsync` call. Hard delete — no soft delete mechanism exists.

---

### `IsSlugAvailableAsync`

```csharp
Task<bool> IsSlugAvailableAsync(
    CheckSlugAvailabilityRequest request,
    CancellationToken cancellationToken = default)
```

**Purpose:** Provides a real-time slug availability check, typically consumed by the admin UI as the user types a slug. Returns `true` if the slug is free to use, `false` if taken.

**Step-by-step logic:**

1. Normalize the slug to lowercase.
2. Call `SlugExistAsync(slug, request.ExcludeCategoryId)`.
3. Return the logical negation (`true` = available, `false` = taken).

**Business rules enforced:**
- The same slug uniqueness rule as create/update.
- The `ExcludeCategoryId` parameter allows the edit scenario: a category can "claim" its own existing slug as still available.

**Side effects:** None. Read-only operation.

> ⚠️ **Known gap (noted in interface comment):** "TODO: Return slug recommend in CheckSlugAvailability." The endpoint currently returns only a boolean. The planned enhancement would also return one or more suggested alternative slugs when the requested one is taken (e.g., `electronics-2`, `electronics-store`). This feature has not been implemented.

---

### `GetBreadcrumbsBySlugAsync`

```csharp
Task<List<BreadcrumbDto>> GetBreadcrumbsBySlugAsync(
    GetBreadcrumbsRequest request,
    CancellationToken cancellationToken = default)
```

**Purpose:** Returns the ordered list of ancestor categories from root to the specified category, inclusive. Used for breadcrumb navigation UI elements (e.g., `Home > Electronics > Smartphones`).

**Step-by-step logic:**

1. Load all categories via `GetAllAsync()` — `O(n)`.
2. Build a dictionary `categoryById: Dictionary<Guid, CategoryDto>` — `O(n)`.
3. Find the target category by slug (case-insensitive) — `O(n)` scan.
4. If not found, throw `NotFoundException("Category")`.
5. Walk up the ancestor chain via `current.ParentId` lookups in the dictionary — `O(d)` where `d` is depth (max 7).
6. At each step, create a `BreadcrumbDto(Id, Name, Slug)` and add to the list.
7. Reverse the list (ancestors are collected leaf-to-root, result is root-to-leaf) — `O(d)`.
8. Return the list.

**Complexity:** `O(n)` overall, dominated by the full-table load. Ancestor traversal is `O(d)` ≤ `O(7)` = constant.

**Business rules enforced:**
- The category must exist (checked by slug match). **No active filter** — this method is called internally after `CreateCategoryAsync` (where the category may have `IsActive = true` but no restriction prevents building breadcrumbs for inactive categories when called directly via the endpoint).

> ⚠️ **Needs clarification — No active filter on direct endpoint call:** The `GetBreadcrumbsBySlugAsync` public endpoint (`GET /api/categories/slug/breadcrumbs`) allows anonymous access and does **not** filter on `IsActive`. This means breadcrumbs can be retrieved for inactive categories. This may be intentional (for admin previewing inactive routes) or an oversight. It should be clarified whether inactive categories should return 404 on the public endpoint.

**Side effects:** None. Read-only.

---

### `GetCategoryByIdAsync`

```csharp
Task<CategoryResponse> GetCategoryByIdAsync(
    Guid categoryId,
    CancellationToken cancellationToken = default)
```

**Purpose:** Returns full category details for a public storefront page, accessed by UUID. Includes breadcrumbs and direct active sub-categories.

**Step-by-step logic:**

1. Call `GetByIdWithActiveChildrenAsync(categoryId)`. Returns only if `IsActive == true`.
2. If `null`, throw `NotFoundException("Category")`.
3. Call `GetBreadcrumbsBySlugAsync` internally — triggers another full `GetAllAsync` database call.
4. Map children to `List<SubCategoryDto>`.
5. Return `CategoryResponse`.

**Side effects:** None. Two database round-trips (`GetByIdWithActiveChildrenAsync` + `GetAllAsync` inside breadcrumbs).

> ⚠️ **Performance note:** Every `GetCategoryByIdAsync` call triggers at minimum **two database queries** — one for the category+children and one full-table scan for breadcrumbs. For high-traffic storefront pages, this method is a good candidate for response caching or a dedicated breadcrumb index table.

---

### `GetCategoryBySlugAsync`

```csharp
Task<CategoryResponse> GetCategoryBySlugAsync(
    string slug,
    CancellationToken cancellationToken = default)
```

**Purpose:** Identical to `GetCategoryByIdAsync` but looks up by slug. Supports URL-routed pages like `/categories/smartphones`.

**Step-by-step logic:** Identical to `GetCategoryByIdAsync`, substituting the lookup call with `GetBySlugWithActiveChildrenAsync(slug.ToLowerInvariant())`.

**Business rules enforced:** Only active categories are returned. Slug is normalized to lowercase before lookup.

**Side effects:** None. Two database round-trips.

---

### `GetCategoryTreeAsync`

```csharp
Task<IReadOnlyList<CategoryTreeNodeDto>> GetCategoryTreeAsync(
    CancellationToken cancellationToken = default)
```

**Purpose:** Returns the full public category tree — active categories only — as a deeply nested structure. Consumed by the storefront navigation menu.

**Step-by-step logic:**

1. Load flat list of active `CategoryTreeNodeDto` records via `GetFlatActiveTreeNodesAsync()`.
2. Build a dictionary `categoriesById: Dictionary<Guid, CategoryTreeNodeDto>`.
3. Iterate the flat list:
   - If `node.ParentId == null` → add to root list.
   - Else → find the parent in the dictionary and add this node to `parent.Children`.
4. Return the root list (nodes with children nested recursively).

**Note:** The `Children` list on `CategoryTreeNodeDto` is initialized as an empty `new List<CategoryTreeNodeDto>()` by the repository. The tree-building step **mutates** these records by appending to `Children`. This is a mutable record, which is idiomatic here but differs from typical immutable record usage in C#.

**Complexity:** `O(n)` time, `O(n)` space.

**Side effects:** None. One database round-trip.

---

### `GetCategoryAdminTreeAsync`

```csharp
Task<IReadOnlyList<CategoryAdminTreeNodeDto>> GetCategoryAdminTreeAsync(
    CancellationToken cancellationToken = default)
```

**Purpose:** Returns the full admin category tree — all categories, active and inactive — with richer data (`IsActive`, `ImageUrl`). Consumed by the admin panel for category management.

**Step-by-step logic:** Identical to `GetCategoryTreeAsync` but uses `GetFlatAdminTreeNodesAsync()` and `CategoryAdminTreeNodeDto`.

**Side effects:** None. One database round-trip.

---

### Private Helper: `ValidateParentCategoryAsync`

```csharp
private async Task ValidateParentCategoryAsync(
    Guid? currentCategoryId,
    Guid newParentId,
    bool currentCategoryStatus,
    CancellationToken cancellationToken = default)
```

**Purpose:** Central validation gate for all parent-assignment operations (create and update). Enforces four invariants in order.

**Invariants enforced:**

| # | Rule | Error |
|---|---|---|
| 1 | A category cannot be its own parent (`currentCategoryId == newParentId`) | `InvalidOperationException` |
| 2 | The new parent must exist | `NotFoundException("Parent category")` |
| 3 | An active category cannot have an inactive parent (`currentCategoryStatus == true && parent.IsActive == false`) | `InvalidOperationException` |
| 4 | Moving a category cannot create a circular reference (`newParentId` is a descendant of `currentCategoryId`) | `InvalidOperationException` |
| 5 | `parentDepth + 1 + subtreeHeight <= 7` (max depth constraint) | `InvalidOperationException` |

**On create:** `currentCategoryId` is `null`. Invariants 1 and 4 are skipped (no existing category to be circular with). Invariant 5's `subtreeHeight` is `0` (new category has no children yet).

**On update (move):** All 5 invariants apply. The method loads all categories via `GetAllAsync()` for in-memory graph operations.

**Performance:** Triggers one full `GetAllAsync()` database call. When called from `UpdateCategoryAsync` (which also loads all categories potentially via `GetBreadcrumbsBySlugAsync` later), this results in **two full-table scans per update**. This is a potential optimization target.

---

### Private Helper: `IsDescendant`

```csharp
private static bool IsDescendant(
    Guid ancestorId,
    Guid targetId,
    IReadOnlyDictionary<Guid, CategoryDto> categoryById)
```

**Purpose:** Determines whether `targetId` is a node in the subtree rooted at `ancestorId`. Used to detect circular references before assigning a new parent.

**Algorithm:** Iterative upward traversal from `targetId` via `ParentId` links. A `HashSet<Guid>` of visited nodes prevents infinite loops in the event of an unexpected cycle in existing data. Returns `true` if `ancestorId` is reached during traversal.

**Complexity:** `O(d)` where `d` is the depth of the tree, bounded at 7.

---

### Private Helper: `GetDepth`

```csharp
private static int GetDepth(
    Guid categoryId,
    IReadOnlyDictionary<Guid, CategoryDto> categoryById)
```

**Purpose:** Counts the number of levels from the root to `categoryId`, inclusive (root = depth 1).

**Algorithm:** Iterative upward traversal, incrementing a counter at each step. A `HashSet<Guid>` prevents infinite loops.

**Complexity:** `O(d)` ≤ `O(7)`.

---

### Private Helper: `GetSubtreeHeight`

```csharp
private static int GetSubtreeHeight(
    Guid categoryId,
    IReadOnlyList<CategoryDto> allCategories)
```

**Purpose:** Calculates the maximum number of levels below `categoryId` (height of the subtree). A leaf node returns 0.

**Algorithm:** Breadth-First Search (BFS) downward from `categoryId`. A lookup `childrenByParent` (a `Lookup<Guid, CategoryDto>`) is built first for `O(1)` child access. The BFS tracks the current level with each enqueued node. A `HashSet<Guid>` prevents revisiting.

**Complexity:** `O(s)` where `s` is the size of the subtree.

---

## 6. Controller / API Layer

**Base route:** `api/categories`
**Tag (Swagger):** `Categories`

### Endpoint Reference

| # | Method | Path | Auth | Service Method | Request | Response |
|---|---|---|---|---|---|---|
| 1 | `POST` | `/api/categories` | `Admin` role | `CreateCategoryAsync` | `CreateCategoryRequest` (body) | `201 CategoryResponse` |
| 2 | `PATCH` | `/api/categories/{categoryId:guid}` | `Admin` role | `UpdateCategoryAsync` | `UpdateCategoryRequest` (body) | `204 No Content` |
| 3 | `DELETE` | `/api/categories/{categoryId:guid}` | `Admin` role | `DeleteCategoryAsync` | — | `204 No Content` |
| 4 | `GET` | `/api/categories/slug/available` | Anonymous | `IsSlugAvailableAsync` | `CheckSlugAvailabilityRequest` (body) | `200 bool` |
| 5 | `GET` | `/api/categories/tree` | Anonymous | `GetCategoryTreeAsync` | — | `200 IReadOnlyList<CategoryTreeNodeDto>` |
| 6 | `GET` | `/api/categories/admin/tree` | `Admin` role | `GetCategoryAdminTreeAsync` | — | `200 IReadOnlyList<CategoryAdminTreeNodeDto>` |
| 7 | `GET` | `/api/categories/{categoryId:guid}` | Anonymous | `GetCategoryByIdAsync` | `categoryId` (route) | `200 CategoryResponse` |
| 8 | `GET` | `/api/categories/slug/{slug}` | Anonymous | `GetCategoryBySlugAsync` | `slug` (route) | `200 CategoryResponse` |
| 9 | `GET` | `/api/categories/slug/breadcrumbs` | Anonymous | `GetBreadcrumbsBySlugAsync` | `GetBreadcrumbsRequest` (query) | `200 List<BreadcrumbDto>` |

> ⚠️ **Needs clarification — Endpoint #4 (CheckSlugAvailability uses `[FromBody]` on a GET):** `GET /api/categories/slug/available` sends its request model via `[FromBody]`. HTTP GET requests with a body are technically allowed by the HTTP spec but are not supported by all clients, proxies, or caching infrastructure. A more conventional design would use `[FromQuery]` (e.g., `?slug=smartphones&excludeCategoryId=...`). This should be reviewed.

### Named Routes

- `GetCategoryById` (`Name = "GetCategoryById"` on endpoint #7) — used by `CreateAsync` to generate the `Location` header in the `201 Created` response:
  ```csharp
  return CreatedAtRoute("GetCategoryById", new { categoryId = category.Id }, category);
  ```

### Authorization Summary

| Scope | Required Role |
|---|---|
| Create category | `Admin` |
| Update category | `Admin` |
| Delete category | `Admin` |
| View admin tree | `Admin` |
| View public tree | Anonymous |
| View category by id/slug | Anonymous |
| Check slug availability | Anonymous |
| Get breadcrumbs | Anonymous |

---

## 7. Business Behavior & Rules

### Rule 1 — Global Slug Uniqueness

Every category must have a slug that is unique across the **entire** category table, regardless of its position in the hierarchy.

**Business rationale:** Slugs are used as URL path segments (`/categories/{slug}`). Unique slugs guarantee unambiguous URL routing and prevent SEO conflicts between pages.

**Enforcement:** `UNIQUE INDEX` at the database level + `SlugExistAsync` check at the service level.

---

### Rule 2 — Slug Format

A slug must match the regular expression `^[a-z0-9]+(-[a-z0-9]+)*$`.

**Business rationale:** URL slugs must be lowercase, human-readable, and URL-safe. Hyphens are the standard word separator for SEO-friendly URLs. Uppercase letters and special characters would require URL encoding, harming readability and SEO.

**Enforcement:** FluentValidation on both `CreateCategoryRequestValidator` and `UpdateCategoryRequestValidator`.

---

### Rule 3 — Maximum Hierarchy Depth of 7

No category chain (from root to leaf) may exceed 7 levels.

**Business rationale:** Deep hierarchies create poor user experience (navigation becomes unwieldy), performance challenges (deeper tree = more ancestor lookups), and operational complexity. 7 levels is a generous limit for any realistic e-commerce product taxonomy.

**Enforcement:** Service layer `ValidateParentCategoryAsync` calculates `parentDepth + 1 + subtreeHeight` and throws if `> 7`.

---

### Rule 4 — No Circular Parent References

A category cannot be assigned as a parent of any of its own ancestors.

**Business rationale:** A circular reference in the tree would cause infinite loops in any traversal algorithm (breadcrumbs, tree rendering, depth calculation) and represents a logical impossibility in a hierarchy.

**Enforcement:** `IsDescendant` private helper in the service layer, checked whenever a parent is assigned or changed.

---

### Rule 5 — Cannot Activate Under Inactive Parent

An active category cannot have an inactive parent.

**Business rationale:** If a parent category is inactive (hidden from the storefront), all of its descendants should logically also be hidden. Allowing an active child under an inactive parent would create an inconsistent state where the child is theoretically "active" but unreachable through the navigation tree.

**Enforcement:** Service layer checks in `CreateCategoryAsync`, `UpdateCategoryAsync`, and `ValidateParentCategoryAsync`.

---

### Rule 6 — Cannot Deactivate With Active Children

A category cannot be set to `IsActive = false` if it has any direct children that are currently active.

**Business rationale:** Deactivating a parent while children remain active would make those children orphaned on the storefront — they would appear in search or direct URL access but would not appear in navigation. The intended workflow is to deactivate children first, then the parent.

**Enforcement:** Service layer `UpdateCategoryAsync` inspects `category.Children` (pre-loaded in full) for any `IsActive == true` child.

---

### Rule 7 — Cannot Delete a Category With Children

A category with any sub-categories (active or inactive) cannot be deleted.

**Business rationale:** Deleting a parent without first addressing children would either orphan the children (if the FK is `SET NULL`) or cascade-delete them (if `CASCADE`). Both outcomes can result in significant unintended data loss. The `Restrict` delete behavior is chosen instead, requiring the admin to explicitly handle children first.

**Enforcement:** Service layer `DeleteCategoryAsync` checks `category.Children.Any()`. Database `Restrict` FK provides a secondary enforcement layer.

---

### Rule 8 — `ClearParent` and `ParentId` Are Mutually Exclusive

An update request may not simultaneously supply a `ParentId` and set `ClearParent = true`.

**Business rationale:** The two fields express contradictory intent. `ClearParent = true` means "make this a root category"; `ParentId` means "set this specific parent." Allowing both would create ambiguity about which instruction takes precedence.

**Enforcement:** `UpdateCategoryRequestValidator` (FluentValidation) — HTTP 400 before the service is reached.

---

### Rule 9 — Hard Delete Only

There is no soft delete (no `IsDeleted` flag). Deletion is permanent.

**Business rationale:** See [Section 9 — Design Decisions](#9-decisions--trade-offs).

---

### Rule 10 — A Category Cannot Be Its Own Parent

Submitting the category's own ID as the `ParentId` is explicitly rejected.

**Business rationale:** This would be a degenerate case of a circular reference (depth 0 cycle) and is caught as a special case before the full circularity check for clarity of error messaging.

**Enforcement:** First check in `ValidateParentCategoryAsync`.

---

## 8. All Possible Cases / Scenarios

### 8.1 Happy Paths

| Scenario | Expected Behavior |
|---|---|
| **Create root category** | `ParentId = null`, valid slug → 201 with `CategoryResponse`, `Breadcrumbs` contains only self, `SubCategories` is empty |
| **Create child category** | Valid `ParentId`, valid slug, parent is active → 201 with `CategoryResponse`, `Breadcrumbs` = [root, ..., self] |
| **Update category name** | `PATCH` with only `Name` set → 204, all other fields unchanged |
| **Move category to new parent** | `PATCH` with new `ParentId` → validates depth, circularity, active state → 204 |
| **Clear parent (promote to root)** | `PATCH` with `ClearParent: true` → 204, `ParentId` becomes null |
| **Deactivate leaf category** | `PATCH` with `IsActive: false`, no active children → 204 |
| **Delete leaf category** | `DELETE` with no children and no products → 204 |
| **Get category by id** | Active category → 200 with full `CategoryResponse` including breadcrumbs and active sub-categories |
| **Get category by slug** | Active category → 200, same shape as by-id |
| **Get public tree** | Returns nested tree of active categories only, empty list if none exist |
| **Get admin tree** | Returns nested tree of all categories (active + inactive) |
| **Get breadcrumbs** | Returns ordered list from root to target category |
| **Check slug available** | Slug not in use → `true`; in use → `false` |

---

### 8.2 Validation Failures

| Scenario | Validation Level | HTTP Status | Error Detail |
|---|---|---|---|
| `Name` empty on create | FluentValidation | 400 | "Name must not be empty" |
| `Name` > 100 chars | FluentValidation | 400 | "Name must be ≤ 100 characters" |
| `Slug` contains uppercase | FluentValidation | 400 | Regex mismatch message |
| `Slug` contains spaces | FluentValidation | 400 | Regex mismatch message |
| `Slug` empty on create | FluentValidation | 400 | "Slug must not be empty" |
| `Slug` > 100 chars | FluentValidation | 400 | Length message |
| `ImageUrl` not a valid URL | FluentValidation | 400 | "ImageUrl must be a valid absolute URL" |
| `ImageUrl` > 2048 chars | FluentValidation | 400 | Length message |
| `Description` > 1000 chars | FluentValidation | 400 | Length message (⚠️ DB allows only 255) |
| `ParentId = Guid.Empty` | FluentValidation | 400 | "ParentId cannot be an empty GUID" |
| `ClearParent = true` + `ParentId` set | FluentValidation | 400 | "Cannot set ParentId and ClearParent at the same time" |
| Duplicate slug | Service layer | 409 | ConflictException |

---

### 8.3 Hierarchy Edge Cases

| Scenario | Expected Behavior |
|---|---|
| **Self-parent:** `PATCH categoryId` with `ParentId = categoryId` | `InvalidOperationException`: "A category cannot be its own parent." |
| **Direct circular reference:** A→B, then `PATCH B` with `ParentId = A` (where A is a child of B) | `IsDescendant` detects A is in B's subtree → `InvalidOperationException`: "circular reference" |
| **Indirect circular:** A→B→C, then `PATCH A` with `ParentId = C` | `IsDescendant` traverses C→B→A, detects A → `InvalidOperationException` |
| **Delete parent with children** | `category.Children.Any() == true` → `InvalidOperationException` with child count |
| **Move subtree that would exceed depth 7** | `parentDepth + 1 + subtreeHeight > 7` → `InvalidOperationException` |
| **Create at depth 7** | Allowed if `parentDepth == 6` and the new node has no subtree (`parentDepth + 1 + 0 = 7 ≤ 7`) |
| **Create at depth 8** | `parentDepth + 1 = 8 > 7` → `InvalidOperationException` |

---

### 8.4 Deletion Edge Cases

| Scenario | Expected Behavior |
|---|---|
| **Delete non-existent category** | `NotFoundException` → 404 |
| **Delete category with active children** | `InvalidOperationException` → 409 (children must be deleted first) |
| **Delete category with inactive children** | Same as above — **all** children block deletion, not just active ones |
| **Delete category with products** ⚠️ | **Currently not checked.** Products linked to this category will have a dangling reference. This is an open bug. |
| **Delete root category with no children** | Succeeds → 204 |
| **Delete via direct SQL bypass** | Database `Restrict` FK will raise a constraint violation, preventing parent deletion |

---

### 8.5 Concurrency

| Scenario | Expected Behavior |
|---|---|
| **Two concurrent creates with the same slug** | First succeeds (201). Second: passes `SlugExistAsync` check (TOCTOU window), then hits the DB unique constraint on `INSERT`. Results in an unhandled DB exception → HTTP 500. **This is a known bug.** |
| **Two concurrent updates to the same category** | Last-write-wins. EF Core does not use optimistic concurrency (`[Timestamp]`/`rowversion`) by default. The second update overwrites the first. |
| **Update and delete concurrently** | Race condition. If delete wins and commits first, the update's `SaveChangesAsync` may either succeed (if the entity is still in the change tracker) or throw an EF concurrency exception. Behavior is undefined. |

---

### 8.6 Empty / Edge States

| Scenario | Expected Behavior |
|---|---|
| **Empty category table — `GetCategoryTreeAsync`** | Returns empty list `[]` |
| **Empty category table — `GetBreadcrumbsBySlugAsync`** | `GetAllAsync` returns empty list, `FirstOrDefault` returns `null` → `NotFoundException` |
| **Root category (no parent)** | `ParentId = null`, `Breadcrumbs = [self]`, full functionality preserved |
| **Category with no children** | `SubCategories = []` in `CategoryResponse` |
| **Category with no description, icon, or image** | Nullable fields returned as `null` in JSON |
| **Category at maximum depth (7)** | Cannot add children — any child creation would push depth to 8 → `InvalidOperationException` |

---

### 8.7 Permission / Authorization Failures

| Scenario | Expected Behavior |
|---|---|
| **Unauthenticated `POST /api/categories`** | ASP.NET Authorization middleware returns 401 before the controller is reached |
| **Authenticated as `Customer`, `POST /api/categories`** | Authorization middleware returns 403 (role mismatch) |
| **Unauthenticated `GET /api/categories/tree`** | 200 — endpoint is `[AllowAnonymous]` |
| **Unauthenticated `GET /api/categories/admin/tree`** | 401 |

---

### 8.8 Pagination / Filtering / Sorting

> ⚠️ **Not yet implemented.** `CategoryFilterParams` (with `PaginationParams`, `CategorySortBy`, `CategorySearchBy`, `IsActive`, `ParentId`, `SearchTerm` fields) and the supporting enums (`CategorySortBy`, `CategorySearchBy`, `SortDirection`) exist in the codebase but are **not wired up** to any service method, repository method, or controller endpoint. There is no paginated category list endpoint currently. The infrastructure for this feature is in place (DTOs and enums defined) but the implementation is pending.

---

### 8.9 Localization / Multi-Language

> ⚠️ **Not implemented.** The `Category` entity has a single `Name`, `Description`, and `Slug` field with no multi-language support. If internationalization is planned, a localization table (e.g., `CategoryTranslation`) or a JSON column approach would be required. This is not present in the current design.

---

## 9. Decisions & Trade-offs

### 9.1 Adjacency List vs. Nested Set / Closure Table

**Decision:** Adjacency list (`parent_id` FK on the same table).

**Alternatives considered:**
- **Nested Set Model:** Stores `lft` and `rgt` values. Makes subtree reads very fast (`WHERE lft BETWEEN x AND y`), but makes writes (insert/move) expensive because they require recalculating ranges.
- **Closure Table:** Stores every ancestor-descendant pair in a separate table. Excellent for all read patterns but doubles write complexity and adds a second table to maintain.
- **Path Enumeration (Materialized Path):** Stores the full path string (e.g., `/1/4/12/`). Fast for prefix queries but requires string manipulation and is fragile to moves.

**Why adjacency list?** For an e-commerce category tree with bounded depth (≤ 7), infrequent writes (admin-only), and read patterns that are well-served by loading all categories into memory (since the total count is bounded), the adjacency list is the simplest model with adequate performance. All tree operations are performed in-memory after a single flat query, avoiding recursive SQL.

---

### 9.2 Hard Delete vs. Soft Delete

**Decision:** Hard delete (physical deletion from the database).

**Rationale:**
- Categories are not user-generated content — they are admin-managed taxonomy.
- The risk of accidental deletion is mitigated by the "must have no children" rule.
- Soft-deleted categories would need to be filtered out of every query, adding complexity.
- No audit/recovery requirement has been specified.

**Trade-off:** If the product entity has a `CategoryId` foreign key (without `SET NULL`), deleting a category could cause constraint violations at the product level. The current code does not yet enforce this (noted as TODO).

---

### 9.3 In-Memory Tree Assembly vs. Recursive SQL (CTE)

**Decision:** Load flat list, assemble tree in C#.

**Rationale:**
- Recursive CTEs are non-trivial, database-specific (PostgreSQL syntax differs from SQL Server), and harder to unit-test.
- For bounded-size category sets (realistic maximum: hundreds to low thousands), loading all rows and assembling in-memory is fast, simple, and easily testable.
- Dictionary-based lookups make the assembly `O(n)`.

**Trade-off:** Does not scale to tens of thousands of categories. At that scale, a CTE or materialized subtree approach would be necessary.

---

### 9.4 Global Slug Uniqueness vs. Per-Parent Uniqueness

**Decision:** Global slug uniqueness enforced by a unique index on `categories.slug`.

**Rationale:** Slugs appear in flat URL paths (`/categories/smartphones`), not hierarchical paths. A global unique slug means the URL always unambiguously identifies a single category. Per-parent uniqueness would allow duplicate slugs under different parents, requiring hierarchical URL paths (e.g., `/electronics/smartphones` vs. `/toys/smartphones`) to disambiguate, which significantly complicates routing.

---

### 9.5 No Caching on Category Reads

**Decision:** No response caching or memory caching is applied to category reads despite `ICacheService` being available in the system.

**Trade-off:** Every request for the category tree or breadcrumbs results in a database query. For a product catalogue that changes infrequently (admin-only writes), caching category data in-memory (or via HTTP response caching) would dramatically reduce database load. This is the most significant performance optimization opportunity in the current implementation.

---

### 9.6 `CategoryFilterParams` Defined but Not Implemented

**Decision:** The DTO and enums for paginated, filtered, sorted category listing are defined but not yet wired to any endpoint.

**Rationale:** This appears to be a planned feature that has been scaffolded (DTOs pre-defined) but not yet implemented. The existence of these types is not a bug but signals future work.

---

## 10. Examples

### 10.1 Create Category — Happy Path

**Request:**
```http
POST /api/categories
Authorization: Bearer <admin-token>
Content-Type: application/json
```
```json
{
  "name": "Smartphones",
  "slug": "smartphones",
  "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Mobile phones and accessories",
  "icon": "smartphone",
  "imageUrl": "https://cdn.example.com/categories/smartphones.jpg",
  "isActive": true
}
```

**Response — 201 Created**
```
Location: /api/categories/d290f1ee-6c54-4b01-90e6-d701748f0851
```
```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Smartphones",
  "slug": "smartphones",
  "description": "Mobile phones and accessories",
  "imageUrl": "https://cdn.example.com/categories/smartphones.jpg",
  "icon": "smartphone",
  "breadcrumbs": [
    { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "Electronics", "slug": "electronics" },
    { "id": "d290f1ee-6c54-4b01-90e6-d701748f0851", "name": "Smartphones", "slug": "smartphones" }
  ],
  "subCategories": []
}
```

---

### 10.2 Create Category — Duplicate Slug

**Request:**
```http
POST /api/categories
Authorization: Bearer <admin-token>
Content-Type: application/json
```
```json
{
  "name": "Mobile Devices",
  "slug": "smartphones",
  "isActive": true
}
```

**Response — 409 Conflict**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Category slug already exists."
}
```

---

### 10.3 Update Category — Partial Update

**Request:**
```http
PATCH /api/categories/d290f1ee-6c54-4b01-90e6-d701748f0851
Authorization: Bearer <admin-token>
Content-Type: application/json
```
```json
{
  "name": "Android Smartphones",
  "isActive": true
}
```

**Response — 204 No Content**
_(No body)_

---

### 10.4 Update Category — Clear Parent (Promote to Root)

**Request:**
```http
PATCH /api/categories/d290f1ee-6c54-4b01-90e6-d701748f0851
Authorization: Bearer <admin-token>
Content-Type: application/json
```
```json
{
  "clearParent": true
}
```

**Response — 204 No Content**

---

### 10.5 Delete Category — Has Children Error

**Request:**
```http
DELETE /api/categories/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer <admin-token>
```

**Response — 409 Conflict**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Cannot delete 'Electronics' because it has 3 subcategories. Please delete or move the subcategories first."
}
```

---

### 10.6 Get Public Category Tree

**Request:**
```http
GET /api/categories/tree
```

**Response — 200 OK**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "parentId": null,
    "name": "Electronics",
    "slug": "electronics",
    "icon": "zap",
    "children": [
      {
        "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
        "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Smartphones",
        "slug": "smartphones",
        "icon": "smartphone",
        "children": []
      }
    ]
  },
  {
    "id": "a1b2c3d4-0000-0000-0000-000000000001",
    "parentId": null,
    "name": "Home & Kitchen",
    "slug": "home-and-kitchen",
    "icon": "home",
    "children": []
  }
]
```

---

### 10.7 Get Category by Slug

**Request:**
```http
GET /api/categories/slug/smartphones
```

**Response — 200 OK**
```json
{
  "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Smartphones",
  "slug": "smartphones",
  "description": "Mobile phones and accessories",
  "imageUrl": "https://cdn.example.com/categories/smartphones.jpg",
  "icon": "smartphone",
  "breadcrumbs": [
    { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "Electronics", "slug": "electronics" },
    { "id": "d290f1ee-6c54-4b01-90e6-d701748f0851", "name": "Smartphones", "slug": "smartphones" }
  ],
  "subCategories": [
    {
      "id": "b2c3d4e5-0000-0000-0000-000000000002",
      "name": "Android Phones",
      "slug": "android-phones",
      "icon": null,
      "imageUrl": null
    }
  ]
}
```

---

### 10.8 Check Slug Availability

**Request:**
```http
GET /api/categories/slug/available
Content-Type: application/json
```
```json
{
  "slug": "new-arrivals",
  "excludeCategoryId": null
}
```

**Response — 200 OK**
```json
true
```

---

### 10.9 Validation Failure — Invalid Slug Format

**Request:**
```http
POST /api/categories
Authorization: Bearer <admin-token>
Content-Type: application/json
```
```json
{
  "name": "Smart TVs",
  "slug": "Smart TVs",
  "isActive": true
}
```

**Response — 400 Bad Request**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Slug": [
      "Slug must be lowercase letters, numbers, and hyphens only (e.g. 'home-and-kitchen')."
    ]
  }
}
```

---

### 10.10 Circular Reference Error

**Request:**
```http
PATCH /api/categories/{parentId}
Authorization: Bearer <admin-token>
Content-Type: application/json
```
```json
{
  "parentId": "{childId}"
}
```
_(Where `childId` is a descendant of `parentId`)_

**Response — 400 Bad Request**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Cannot set parent category to one of its own descendants (circular reference)."
}
```

---

*End of Category Service Technical Documentation*
