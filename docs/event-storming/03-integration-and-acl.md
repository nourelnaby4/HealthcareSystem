# 03 — Cross-Context Integration & 3rd-Party ACL

How the modular monolith’s modules integrate with each other and with the external insurance provider.

---

## Cross-Context Event Flows

The modular monolith is integrated through **integration events** on an in-process bus, dispatched reliably via the **Outbox/Inbox** pattern.

```
Administration                 Clinical                    Laboratory
  PatientAdmitted ───────────► EncounterStarted ──────────► LabTestRequestReceived
                                  │                              │
                                  │ OrderLabTest                 │
                                  ▼                              ▼
                             LabTestOrdered ─────────────► (Specimen → Result)
                                  ▲                              │
                                  │                              ▼
                             DiagnosisRecorded ◄──── LabResultPublished
                                  │
                                  ▼
                           PrescriptionRequested ─────► Pharmacy
                                  │                        │
                                  │                        ▼
                                  │                  MedicationDispensed
                                  │                        │
                                  ▼                        ▼
                             EncounterDischarged ──► ClaimSubmitted (Insurance)
                                                             │
                                                             ▼
                                                 InsuranceProviderGateway (3rd party)
                                                             │
                                             ClaimApproved/Rejected ◄──── (webhook/poll)
                                                             │
                                                             ▼
                                                   InvoiceIssued → PaymentReceived
```

---

## Integration Event Catalog (cross-module contracts)

These are the **published contracts** between modules — version them (`V2`) when they change.

| Event | Publisher | Subscribers |
|-------|-----------|-------------|
| `PatientAdmitted` | Administration | Clinical |
| `LabTestOrdered` | Clinical | Laboratory |
| `LabResultPublished` | Laboratory | Clinical |
| `DiagnosisRecorded` | Clinical | Pharmacy (suggest Rx) |
| `PrescriptionRejected` | Pharmacy | Clinical |
| `MedicationDispensed` | Pharmacy | Insurance (charge) |
| `EncounterDischarged` | Clinical | Insurance (start claim) |
| `ClaimApproved` / `ClaimRejected` | Insurance | Clinical (notifications) |

---

## Third-Party Insurance Integration (ACL)

The `Insurance` module owns the boundary to the external provider. It never leaks provider concepts into other modules.

- 🟥 **External System:** `InsuranceProviderGateway` — exposed via **HL7 FHIR** or **X12 (837 claim / 835 remittance)**, or a provider-specific REST API.
- **Anti-Corruption Layer** inside `Insurance/Infrastructure`:
  - **Outbound:** translates internal `ClaimSubmitted` → provider payload; persists to Outbox, an adapter flushes it.
  - **Inbound:** maps provider responses (remittance / webhook / polled status) → internal `EligibilityConfirmed|Denied`, `ClaimAdjudicated`, `ClaimApproved|Rejected` via the Inbox (idempotent).
- **Reliability:** Outbox guarantees the claim is sent at-least-once; Inbox + idempotency keys handle duplicates. Retries & circuit-breaker around the gateway.

```
[Claim aggregate] ──ClaimSubmitted──► Outbox ──► InsuranceProviderGateway (3rd party)
                                                        │  (835 / webhook / poll)
                                                        ▼
   [Claim aggregate] ◄──Inbox (idempotent)◄── map response ◄── provider
```

---

⬅️ [02 — Domain Storm](./02-domain-storm.md) · ➡️ [04 — Context Mapping](./04-context-mapping.md) · 🏠 [Index](./README.md)
