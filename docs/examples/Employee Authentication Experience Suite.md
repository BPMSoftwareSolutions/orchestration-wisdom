# Employee Authentication Experience Suite

**Eight movements. One attendance truth.**

The **Employee Authentication Experience Suite** defines the UX-first orchestration for employee login, authorization, and readiness within the Time & Attendance platform.
It is not a login flow. It is a **composed experience**, intentionally designed as a sequence of **movements**, each made up of **multiple beats** that guide the employee from intent to certainty.

This suite ensures that:

* Authentication is **trusted**, not mysterious
* Progress is **visible**, not implied
* Policy enforcement is **felt as clarity**, not friction
* Every movement ends with a **closed UX loop**

---

## Why an “Experience Suite”?

The term **Experience Suite** is deliberate.

* **Experience** signals that UX is the primary design axis, not backend validation.
* **Suite** signals modular composition: each movement can evolve independently, while preserving a shared rhythm and mental model.

This framing enables:

* Scalable feature growth without cognitive overload
* Clean service boundaries for engineering
* Predictable user confidence at every step

---

## Musical Composition Model

The suite follows a strict composition rule:

* **Movement** = a major UX question is resolved
* **Beat** = a human-perceivable outcome within that movement

Every beat must:

* Reduce uncertainty
* Increase confidence
* Or signal progress

Every movement must end in **UX closure**, where the employee knows:

* What just happened
* What state they are in
* What action is available next

---

## The Eight Movements (At a Glance)

| # | Movement                         | Core Question Resolved                   |
| - | -------------------------------- | ---------------------------------------- |
| 1 | Input Validation                 | “Did I start this correctly?”            |
| 2 | Credential Verification          | “Am I who I say I am?”                   |
| 3 | Authenticate Presence            | “Is my presence trusted?”                |
| 4 | Role Determination & Permissions | “What am I allowed to do?”               |
| 5 | Admin Data Load                  | “Is the system ready for admin actions?” |
| 6 | Employee Data Load               | “Is my view locked to me?”               |
| 7 | Dashboard Initialization         | “Is the system live and stable?”         |
| 8 | Time Entry Display               | “What is today’s attendance truth?”      |

---

## Movement 1 — Input Validation

**Frictionless entry with fast feedback**

**Purpose**
Capture employee intent and eliminate obvious errors immediately, without invoking backend systems.

**Beats**

1. **Initiation** — User signals intent; UI confirms readiness
2. **Validation** — Inputs checked locally for completeness
3. **Closure** — User knows whether they can proceed or must correct

**UX Outcome**
The employee never wonders if the system “noticed” them.

```mermaid
sequenceDiagram
  autonumber
  actor U as 👤 User
  participant LW as 🔐 LoginWindow
  participant V as ✅ Input Validator
  participant T as 📊 Telemetry

  Note over U,T: Movement 1 - Input Validation (frictionless, guided)

  Note over U,LW: Beat 1 - Initiation (intent captured + UX ready)
  U->>LW: Click Login
  LW-->>U: UX: Validating input...
  LW->>T: Track auth.attempted

  Note over LW,V: Beat 2 - Validation (fast fail, no DB)
  LW->>V: Validate(username, password)
  alt Missing fields
    V-->>LW: Invalid (missing)
    LW-->>U: ❌ Please enter both username and password
    LW->>T: Track auth.validation_failed
  else Present
    V-->>LW: Valid
    LW-->>U: ✅ Input OK, checking credentials...
    LW->>T: Track auth.validation_passed
  end

  Note over U,T: Beat 3 - Closure (user knows next move)
  alt Missing fields
    LW-->>U: UX: Cursor focused on missing field
  else Present
    LW-->>U: UX: Continue to credential verification
  end
```

---

## Movement 2 — Credential Verification

**Proof without ambiguity**

**Purpose**
Verify credentials against a single authoritative source while keeping failure states opaque and safe.

**Beats**

1. **Submission** — Credentials committed with visible progress
2. **Verification** — One authoritative lookup (no retries, no guessing)
3. **Closure** — Clear success or failure, with no information leakage

**UX Outcome**
The employee knows whether access is granted — and why.

```mermaid
sequenceDiagram
  autonumber
  actor U as 👤 User
  participant LW as 🔐 LoginWindow
  participant DBH as 🗄️ DatabaseHelper
  participant DB as 💾 SQL Database
  participant T as 📊 Telemetry

  Note over U,T: Movement 2 - Credential Verification (proof, not mystery)

  Note over U,LW: Beat 1 - Submission (credentials committed)
  U->>LW: Submit credentials
  LW-->>U: UX: Checking credentials...
  LW->>T: Track credentials.submitted

  Note over LW,DB: Beat 2 - Verification (single authoritative lookup)
  LW->>DBH: AuthenticateUser(username, password)
  DBH->>DB: Query Users for match
  alt Match found
    DB-->>DBH: ✓ UserAccount
    DBH-->>LW: ✓ UserAccount
  else No match
    DB-->>DBH: ❌ No rows
    DBH-->>LW: ❌ NULL
  end

  Note over U,T: Beat 3 - Closure (no leaks, no ambiguity)
  alt Match found
    LW-->>U: ✅ Credentials verified
    LW->>T: Track credentials.verified
  else No match
    LW-->>U: ❌ Invalid username or password
    LW->>T: Track credentials.rejected
  end
```

---

## Movement 3 — Authenticate Presence

**Establish the trust boundary**

**Purpose**
Confirm not just identity, but *presence legitimacy* using biometric or fallback methods.

**Beats**

1. **Initiation** — Verification begins with clear UX feedback
2. **Verification** — Biometric or PIN validation occurs
3. **Confirmation** — Success or failure is clearly communicated

**UX Outcome**
The employee feels *verified*, not merely logged in.

```mermaid
sequenceDiagram
  autonumber
  actor E as 👤 Employee
  participant UI as 🧭 Time App UI
  participant A as 🔐 Auth Engine
  participant T as 📊 Telemetry

  Note over E,T: Movement 3 - Authenticate Presence (trust boundary)

  Note over E,UI: Beat 1 - Initiation (employee intent + UX readiness)
  E->>UI: Tap "Verify / Clock In"
  UI-->>E: UX: Verifying identity...
  UI->>T: Track auth.started

  Note over UI,A: Beat 2 - Verification (proof step, method selected)
  alt Biometric enabled
    UI->>A: VerifyBiometric (face, fingerprint)
  else PIN fallback
    UI->>A: VerifyPIN (masked entry)
  end
  A-->>UI: Result (success or failure + reasonCode)

  Note over E,T: Beat 3 - Confirmation (UX closure + next action is clear)
  alt Success
    UI-->>E: ✅ Verified (show who + confidence indicator)
    UI->>T: Track auth.success (method, latencyMs)
  else Failure
    UI-->>E: ❌ Could not verify (reason + retry or fallback)
    UI->>T: Track auth.failed (reasonCode, method)
  end
```

---

## Movement 4 — Role Determination & Permissions

**Visible access boundaries**

**Purpose**
Determine role and configure permissions *before* the user can act.

**Beats**

1. **Role Detection** — System determines user lane
2. **Permission Configuration** — UI is shaped to role
3. **Closure** — User understands scope of access

**UX Outcome**
No surprise buttons. No hidden restrictions.

```mermaid
sequenceDiagram
  autonumber
  actor U as 👤 User
  participant MW as 📊 MainWindow
  participant UA as 👤 UserAccount
  participant ACL as 🛡️ Access Config
  participant T as 📊 Telemetry

  Note over U,T: Movement 4 - Role Determination and Permissions (visible access boundary)

  Note over MW,UA: Beat 1 - Read role (determine lane)
  MW->>UA: Read Role
  UA-->>MW: Role (Admin or Employee)
  MW->>T: Track role.detected

  Note over MW,ACL: Beat 2 - Configure UI permissions (before user acts)
  alt Admin
    MW->>ACL: Enable admin controls
    ACL-->>MW: ✓ Admin UX config
  else Employee
    MW->>ACL: Lock to self only
    ACL-->>MW: ✓ Employee UX config
  end
  MW->>T: Track ui.permissions_configured

  Note over U,T: Beat 3 - Closure (user understands access level)
  alt Admin
    MW-->>U: UX: Logged in as Admin (full controls)
  else Employee
    MW-->>U: UX: Logged in as Employee (self only)
  end
```

---

## Movement 5 — Admin Data Load

**Confident system readiness**

**Purpose**
Prepare admin-level data and controls with visible loading and deterministic completion.

**Beats**

1. **Loading State** — Expectations are set
2. **Dataset Retrieval** — Authoritative data is fetched
3. **Closure** — Admin controls are enabled and ready

**UX Outcome**
Admins trust the system before acting.

```mermaid
sequenceDiagram
  autonumber
  actor A as 🧑‍💼 Admin
  participant MW as 📊 Dashboard
  participant DBH as 🗄️ Helper
  participant DB as 💾 SQL Database
  participant UI as 🎨 UI
  participant T as 📊 Telemetry

  Note over A,T: Movement 5 - Admin Data Load (fast, confident setup)

  Note over MW,UI: Beat 1 - Loading state (no silent waiting)
  MW-->>UI: UX: Loading employees...
  MW->>T: Track admin.load_started

  Note over MW,DB: Beat 2 - Fetch dataset (authoritative list)
  MW->>DBH: GetActiveEmployees()
  DBH->>DB: Select active employees
  DB-->>DBH: ✓ List<Employee>
  DBH-->>MW: ✓ employees

  Note over A,T: Beat 3 - Closure (controls enabled, admin ready)
  MW->>UI: Bind EmployeeComboBox = employees
  MW->>UI: Enable ComboBox and BadgeTextBox
  UI-->>A: ✅ Employee list loaded, full controls enabled
  MW->>T: Track admin.load_complete
```

---

## Movement 6 — Employee Data Load

**Self-only enforcement**

**Purpose**
Load employee-specific data and lock the UI to prevent cross-access.

**Beats**

1. **Loading State** — User knows their data is being fetched
2. **Record Retrieval** — Self record is resolved or rejected
3. **Closure** — UI is locked or a clear error is shown

**UX Outcome**
Employees feel protected — and constrained correctly.

```mermaid
sequenceDiagram
  autonumber
  actor E as 👤 Employee
  participant MW as 📊 Dashboard
  participant DBH as 🗄️ Helper
  participant DB as 💾 SQL Database
  participant UI as 🎨 UI
  participant T as 📊 Telemetry

  Note over E,T: Movement 6 - Employee Data Load (self-only, enforced)

  Note over MW,UI: Beat 1 - Loading state (clarity first)
  MW-->>UI: UX: Loading your profile...
  MW->>T: Track employee.load_started

  Note over MW,DB: Beat 2 - Fetch self record (by EmployeeId)
  MW->>DBH: GetEmployeeById(EmployeeId)
  DBH->>DB: Select employee by id
  alt Found
    DB-->>DBH: ✓ Employee
    DBH-->>MW: ✓ Employee
  else Not found
    DB-->>DBH: ❌ NULL
    DBH-->>MW: ❌ NULL
  end

  Note over E,T: Beat 3 - Closure (lock lane, or block)
  alt Found
    MW->>UI: Populate ComboBox (single)
    MW->>UI: Disable ComboBox and BadgeTextBox
    UI-->>E: ✅ Ready (self-only access)
    MW->>T: Track employee.load_complete
  else Not found
    UI-->>E: ❌ Account misconfigured, contact admin
    MW->>T: Track employee.load_failed
  end
```

---

## Movement 7 — Dashboard Initialization

**Rhythm and readiness**

**Purpose**
Initialize live time signals and conditional refresh logic.

**Beats**

1. **Clock Start** — Time trust is established
2. **Auto-Refresh Enablement** — System stays current when needed
3. **Closure** — Dashboard is visibly “alive”

**UX Outcome**
The system feels active, not static.

```mermaid
sequenceDiagram
  autonumber
  actor U as 👤 User
  participant MW as 📊 Dashboard
  participant Clock as ⏱️ ClockTimer
  participant Refresh as 🔄 RefreshTimer
  participant UI as 🎨 UI
  participant T as 📊 Telemetry

  Note over U,T: Movement 7 - Dashboard Initialization (rhythm + readiness)

  Note over MW,Clock: Beat 1 - Start live clock (time trust)
  MW->>Clock: Start (1s tick)
  Clock-->>UI: Update time display
  MW->>T: Track dashboard.clock_started

  Note over MW,Refresh: Beat 2 - Enable auto-refresh (only when needed)
  MW->>Refresh: Start (60s tick, conditional)
  Refresh-->>MW: Trigger refresh if open punch exists
  MW->>T: Track dashboard.refresh_enabled

  Note over U,T: Beat 3 - Closure (dashboard is truly ready)
  UI-->>U: ✅ Dashboard ready (time live, data will update)
  MW->>T: Track dashboard.ready
```

---

## Movement 8 — Time Entry Display

**Today’s attendance truth**

**Purpose**
Present authoritative time entries and totals for the current day.

**Beats**

1. **Load State** — User expectations are set
2. **Data Retrieval** — Entries and totals are calculated
3. **Closure** — User sees the truth and knows next actions

**UX Outcome**
No guessing. No disputes. Just clarity.

```mermaid
sequenceDiagram
  autonumber
  actor U as 👤 User
  participant MW as 📊 Dashboard
  participant DBH as 🗄️ Helper
  participant DB as 💾 SQL Database
  participant UI as 🎨 UI
  participant T as 📊 Telemetry

  Note over U,T: Movement 8 - Time Entry Display (clarity on todays truth)

  Note over MW,UI: Beat 1 - Load state (set expectation)
  MW-->>UI: UX: Loading todays entries...
  MW->>T: Track entries.load_started

  Note over MW,DB: Beat 2 - Retrieve entries + totals (authoritative)
  MW->>DBH: GetTodayEntries(EmployeeId)
  DBH->>DB: Select todays entries
  DB-->>DBH: ✓ entries
  DBH-->>MW: ✓ entries
  MW->>DBH: GetTotalHoursToday(EmployeeId)
  DBH-->>MW: ✓ totalHours

  Note over U,T: Beat 3 - Closure (user sees truth + next move)
  MW->>UI: Bind grid = entries
  MW->>UI: Render totalHours
  UI-->>U: ✅ Entries displayed with accurate totals
  MW->>T: Track entries.rendered
```

---

## Closing Thought

This suite does not optimize for speed alone.
It optimizes for **confidence per second**.

When authentication feels like a song:

* Users trust outcomes
* Engineers build cleaner systems
* Policy enforcement disappears into clarity

This is not a login flow.
It is a **composed experience**.
