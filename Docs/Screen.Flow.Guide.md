# Screen.Flow.Guide.md

Operationalizing Approved Screens (Authoritative Guide)

## Purpose

This guide defines the **mandatory process** for transforming an **approved screen image** (PNG, mockup, or design) into a fully operational UI backed by APIs, domain logic, and persistence.

A screen image is treated as an **authoritative UX contract**.
AI tools MUST implement functionality **to match the screen**, not reinterpret or redesign it.

---

## Core Rules (Non-Negotiable)

1. Screens define **WHAT exists**; systems define **HOW it works**
2. AI MUST NOT redesign, reorder, or extend UX
3. Every user action MUST have an explicit outcome
4. Missing states or screens MUST be reported, not invented
5. Backend state governs navigation, not UI assumptions

---

## Step 1 — Screen Decomposition

Given a screen image, the AI MUST extract the following **without assumptions**.

### 1.1 Screen Metadata

* Screen title
* Screen ID (derived, stable)
* Actor / role (Customer, Driver, Admin, Store)
* Primary purpose
* Screen type:

  * Standalone screen
  * Modal / overlay
  * Variant-based screen

### 1.2 Visual Component Inventory

For every visible UI element, identify:

* Static text
* Buttons
* Icons
* Badges
* Input fields
* Lists / cards
* Tabs / selectors
* Status indicators

Each element MUST be classified as:

* Display-only
* User action
* State indicator
* Variant selector

No behavior may be inferred at this stage.

---

## Step 2 — Screen Variants (Same Screen, Different State)

If a screen shows **multiple selectable modes** (tabs, role buttons, toggles):

### Rules

1. This is **ONE screen**, not multiple screens
2. All variants share the same Screen ID
3. Variant changes MUST NOT trigger navigation
4. Variant changes MUST NOT alter backend state unless explicitly defined
5. AI MUST NOT invent additional variants

### Variant Identification (MANDATORY)

Variant:

* Name:
* Trigger (button/tab):
* Visible differences:
* Available actions:
* Backend interaction (if any):

Variants visible in the approved design are **approved states**.

---

## Step 3 — Interaction Mapping

For every **user-action element**, define:

* User action (tap, click, toggle)
* Required input (if any)
* Intended outcome
* Whether a backend call is required

If behavior is unclear:

* Mark **Requires clarification**
* DO NOT invent behavior

---

## Step 4 — Transition Analysis (MANDATORY)

Transitions are **first-class** and MUST be explicit.

### 4.1 Inbound Transitions (Entering the Screen)

Inbound Transition:

* Source screen or system event:
* Trigger:
* Required domain state(s):
* Required context (IDs / DTOs):
* Authorized role:

If an inbound transition cannot be validated against backend state, it MUST be flagged.

### 4.2 Outbound Transitions (Leaving the Screen)

Outbound Transition:

* Action:
* Backend call:
* Domain state change (From → To):
* Destination screen:
* On failure:

No outbound transition may be implied.

---

## Step 5 — Missing Follow-Up States (Critical Rule)

If a user action **requires a follow-up UI state** that is **not present in the approved design**:

### Default Behavior

* AI MUST NOT invent the UI
* AI MUST report the gap

Example report:

Action "Delete" requires a confirmation dialog.
No approved UI state exists. Please provide design or approve standard system overlay.

---

## Step 6 — Standard System UI States (Limited Exception)

The AI MAY use the following **pre-approved system UI states** without explicit designs:

* Loading overlay / spinner
* Generic confirmation dialog (Confirm / Cancel)
* Generic error toast or modal
* Generic success toast

### Constraints

* Visually minimal
* No new actions introduced
* No navigation flow changes
* No independent persistent state

Anything outside this list MUST be reported as missing.

---

## Step 7 — Screen State Identification

For each screen, enumerate all required UI states:

* Initial
* Loading
* Empty
* Success
* Error
* Disabled / blocked
* Partial data

Each UI state MUST map to:

* A backend condition
* A visible UI change

---

## Step 8 — Backend Capability Check (MANDATORY)

Before creating or modifying backend artifacts, the AI MUST consult:

* Docs/Architecture.Guide.md
* Docs/Architecture.Existing.md
* Docs/Database.Guide.md

The AI MUST determine:

* Do required APIs already exist?
* Do required DTOs already exist?
* Do required entities already exist?
* Do required state transitions already exist?

### Reuse Rule

Reuse before creating. Creation requires justification.

---

## Step 9 — API & DTO Derivation

For each backend interaction, define:

* API endpoint
* Request DTO
* Response DTO
* Authorization role
* Validation rules

Rules:

* DTOs MUST NOT expose domain entities
* Authorization enforced server-side
* UI performs orchestration only

---

## Step 10 — Domain & State Alignment

For each backend operation:

* Domain entity involved
* Current state
* Target state
* Validation rules
* Rejection conditions

Invalid or skipped transitions are prohibited.

---

## Step 11 — UI → Backend Flow Validation

The AI MUST verify:

* UI contains no business logic
* Backend enforces all rules
* Errors are surfaced clearly
* Authorization cannot be bypassed
* Navigation reflects backend state, not assumptions

---

## Step 12 — Output Artifacts

Only after Steps 1–11 may implementation begin.

Required outputs:

* Screen spec (if not already present)
* Variant definitions (if applicable)
* API contracts (new or reused)
* DTO definitions (new or reused)
* State transition mapping
* UI implementation matching approved design

---

## Explicit Prohibitions

The AI MUST NOT:

* Redesign screens
* Invent navigation paths
* Add UI elements not shown
* Invent variants or dialogs
* Bypass backend validation
* Create duplicate backend artifacts without checking existing ones

---

## Key Principles

* Screens are contracts
* Variants are states, not screens
* Transitions define behavior
* Missing UX is reported, not invented
* Backend state governs navigation

---

## Screen Authorization Rules (MANDATORY)

Every screen MUST explicitly declare its authorization rules.

For each screen, the AI MUST identify:

- Authorized roles that may VIEW the screen
- Authorized roles that may PERFORM ACTIONS on the screen
- Roles that are explicitly FORBIDDEN

Authorization MUST be enforced server-side.
UI checks are advisory only.

How it’s used in practice (example)

Inside a screen spec derived from the guide:

Authorization:
- View: Customer
- Actions: Customer
- Forbidden: Driver, Admin


Or for read-only cases:

Authorization:
- View: Admin
- Actions: None (read-only)
- Forbidden: Customer, Driver

## Final Rule

If it’s not visible in the design, defined in the guides, or explicitly approved, it does not exist.
