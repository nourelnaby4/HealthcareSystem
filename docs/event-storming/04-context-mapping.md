# 04 — Upstream & Downstream (Context Mapping)

Event Storming finds the bounded contexts; **Context Mapping** (strategic DDD) describes **how** they relate.

---

## The core idea

When context **A** depends on something context **B** provides:

- **B = Upstream (U)** — the *supplier*: it owns the model/service others consume.
- **A = Downstream (D)** — the *customer*: it depends on the upstream.

The **pattern** describes the quality of that dependency:

| Symbol | Pattern | Meaning |
|--------|---------|---------|
| `U / D` | **Customer-Supplier** | Upstream serves downstream and *prioritizes* its needs. |
| `U / C` | **Conformist** | Downstream *blindly conforms* to upstream's model (no influence). |
| `U / ACL` | **Anticorruption Layer** | Downstream builds a translator so upstream concepts never leak in. |
| `OHS` | **Open Host Service** | Upstream exposes **one** standard API/protocol to *many* consumers. |
| `PL` | **Published Language** | A documented, shared interchange model (often paired with OHS). |
| `P ↔ P` | **Partnership** | Two contexts cooperate; mutual dependency. |
| `SK` | **Shared Kernel** | Two contexts share a small piece of model/code explicitly. |

---

## Context Map (Healthcare)

```
                                 ┌──────────────────────┐
                                 │  InsuranceProvider   │   3rd-party
                                 │   Gateway (FHIR/X12) │
                                 │        (OHS+PL)      │
                                 └──────────┬───────────┘
                                            │ U / ACL
                                            ▼
┌───────────────┐  U/D   ┌───────────────┐            ┌──────────────────┐
│ Administration │──────▶│    Clinical   │  P ↔ P     │     Insurance    │
│  (identity,    │       │  (encounters) │◀──────────▶│  & Billing       │
│   patients)    │       └───┬───────┬───┘            │  (claims, $)     │
└──────┬────────┘           │U/D    │U/D              └────────▲─────────┘
       │ OHS                ▼       ▼                          │ U/D
       │            ┌──────────┐ ┌──────────┐                   │
       │            │Laboratory│ │ Pharmacy │───────────────────┘
       │            │ (results)│ │ (dispense│  chargeable items
       ▼            └──────────┘ └──────────┘
  (all modules) — auth/permission claims (Shared Kernel + OHS)
```

---

## Relationship Matrix

| Upstream (U) | Downstream (D) | Pattern | Why |
|--------------|----------------|---------|-----|
| **Administration** | Clinical | `U / D` (Customer-Supplier) | Clinical needs patient/identity that Administration owns. |
| **Administration** | *all modules* | `OHS + SK` | Publishes auth/permission claims everyone consumes. |
| **Laboratory** | Clinical | `U / D` (Customer-Supplier) | Lab owns the test catalog + results Clinical needs. |
| **Pharmacy** | Clinical | `U / D` (Customer-Supplier) | Pharmacy owns medication catalog & dispensing. |
| **Clinical** | Insurance | `U / D` (Customer-Supplier) | Clinical emits chargeable events (`EncounterDischarged`). |
| **Pharmacy** | Insurance | `U / D` (Customer-Supplier) | Pharmacy emits `MedicationDispensed` (chargeable). |
| **Insurance** | Clinical | `U / D` (Customer-Supplier) | Insurance reports claim/invoice/payment status back. |
| **InsuranceProvider** *(3rd party)* | **Insurance** | **`OHS + PL` (upstream) → `ACL` (downstream)** | Provider dictates format; Insurance translates via ACL. |

---

## Worked Examples

### Example 1 — Internal Customer-Supplier: `Clinical (U) → Insurance (D)`
Clinical is the **upstream** of *chargeable events*: when an encounter ends it publishes `EncounterDischarged`. Insurance is **downstream** and consumes it to start a claim.

```
Clinical   ──EncounterDischarged (integration event)──►  Insurance
(U)                                                         (D)
```
- **What’s shared:** a *published* event contract, not internal classes.
- **Risk:** if Clinical changes the event, Insurance breaks → version the event (`EncounterDischargedV2`).

### Example 2 — Internal Customer-Supplier (reversed): `Laboratory (U) → Clinical (D)`
Direction *flips* depending on who supplies the value. Clinical *orders* tests, but Laboratory **owns** the test catalog and **publishes** `LabResultPublished`. So Laboratory is the upstream *supplier of results*.

```
Laboratory ──LabResultPublished──► Clinical
   (U)                               (D)
```
- Clinical must **conform** to Laboratory’s result schema → if it just copies fields verbatim it becomes a **Conformist (`U / C`)**; better to translate into Clinical’s own `Diagnosis`/result model.

### Example 3 — THE 3rd-PARTY INSURANCE ACL (most important)
The external provider is **upstream** and exposes an **Open Host Service + Published Language** (HL7 **FHIR** resources or **X12** 837/835 EDI). Our `Insurance` module is **downstream** and **must not** let FHIR/X12 types leak into the domain → it uses an **Anti-Corruption Layer**.

```
InsuranceProvider (U, OHS+PL)             Insurance module (D, ACL)
┌───────────────────────┐   837/FHIR   ┌─────────────────────────────┐
│ FHIR Claim resource   │ ────────────► │ ProviderAdapter (translator)│
│ CoverageEligibility…  │              │  maps  FHIR  ⇄  domain      │
│ X12 835 remittance    │ ◄─────────── │                             │
└───────────────────────┘   response   └──────────────┬──────────────┘
                                                          │ domain events
                                                          ▼
                                          EligibilityConfirmed / ClaimApproved
```

- **Upstream (provider):** defines the contract — we have **zero influence** over FHIR/X12, so we treat the *published language* as fixed.
- **Downstream (Insurance):** keeps a **pure domain model** (`Claim`, `Coverage`, `Invoice`). The **ACL** sits in `Insurance/Infrastructure/InsuranceProviderGateway/` and translates in both directions.
- **Why ACL, not Conformist:** a Conformist would copy FHIR concepts (e.g., `ExplanationOfBenefit`) straight in, coupling the domain to the provider. The ACL isolates change — switch providers by swapping the adapter, domain untouched.
- **Reliability layer:** Outbox guarantees `ClaimSubmitted` reaches the gateway; Inbox + idempotency keys dedupe provider responses/webhooks.

### Example 4 — Shared Kernel + OHS: `Administration (OHS/SK) → all`
Every module must know *who* is acting and *whether* they may. Administration owns identity and exposes it as **auth claims** (a published, stable contract) — an **Open Host Service** backed by a **Shared Kernel** (`Shared/Kernel`). Modules never reach into Administration’s DB; they read signed claims.

```
Administration ──auth claims (JWT)──► Clinical / Lab / Pharmacy / Insurance
   (OHS + SK)
```
- Keep the **shared kernel tiny** (identity only). Anything larger becomes a coordination bottleneck.

---

## How this maps back to the storm

| Storm artifact | Context-mapping decision |
|----------------|--------------------------|
| Cross-context 🔶 integration event | Decides U vs D direction of data flow |
| 🟥 External system | Always **upstream** to our boundary → forces an **ACL** |
| 🟣 Policy in another context | The **downstream** reacting to an **upstream** event |
| 📦 Bounded Context | Each becomes one row/column in the matrix |

> Guideline: **any 3rd-party dependency → downstream with ACL.** Any *internal* mutual dependency → **Partnership** or break it via an integration event so one side is clearly upstream.

---

⬅️ [03 — Integration & ACL](./03-integration-and-acl.md) · ➡️ [05 — Module Mapping](./05-module-mapping.md) · 🏠 [Index](./README.md)
