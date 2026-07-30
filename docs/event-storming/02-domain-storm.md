# 02 — Seeded Domain Storm

Pre-filled starting points per bounded context. Use these as the initial canvas; refine with domain experts during the workshop.

> **Format per context:** 🟡 Aggregates, then a table mapping 🔶 Domain Event → 🔵 Command → 🟣 Policy/Reaction, then 🟩 Read models.

---

## 🛡️ Administration & Access Control

**Aggregates:** `User`, `Role`, `Patient`, `Facility`, `AuditEntry`

| 🔶 Domain Event | 🔵 Command | 🟣 Policy / Reaction |
|----------------|------------|----------------------|
| `UserRegistered` | `RegisterUser` | — |
| `RoleAssigned` | `AssignRole` | — |
| `UserSuspended` | `SuspendUser` | *Whenever `UserSuspended` → revoke active sessions* |
| `PatientRegistered` | `RegisterPatient` | — |
| `PatientAdmitted` | `AdmitPatient` | *Whenever `PatientAdmitted` → notify Clinical to open encounter* |
| `SessionRevoked` | `RevokeSession` | — |

**Read models:** `UserDirectoryView`, `PatientDirectoryView`, `AuditLogView`

---

## 🏥 Clinical

**Aggregates:** `Encounter`, `ClinicalNote`, `VitalSigns`, `Diagnosis`

| 🔶 Domain Event | 🔵 Command | 🟣 Policy / Reaction |
|----------------|------------|----------------------|
| `EncounterScheduled` | `ScheduleEncounter` | — |
| `EncounterStarted` | `StartEncounter` | — |
| `VitalSignsRecorded` | `RecordVitalSigns` | — |
| `ClinicalNoteAdded` | `AddClinicalNote` | — |
| `DiagnosisRecorded` | `RecordDiagnosis` | *Whenever `DiagnosisRecorded` w/ actionable code → suggest prescription* |
| `LabTestOrdered` | `OrderLabTest` | *Whenever `LabTestOrdered` → publish to Laboratory* |
| `EncounterDischarged` | `DischargeEncounter` | *Whenever `EncounterDischarged` → publish to Insurance (start claim)* |

**Read models:** `EncounterTimelineView`, `ActiveEncountersView`, `PatientProblemListView`

---

## 🧪 Laboratory

**Aggregates:** `LabTestRequest`, `Specimen`, `LabResult`

| 🔶 Domain Event | 🔵 Command | 🟣 Policy / Reaction |
|----------------|------------|----------------------|
| `LabTestRequestReceived` | *(integration: from Clinical `LabTestOrdered`)* | *Whenever `LabTestRequestReceived` → accept & open request* |
| `SpecimenCollected` | `CollectSpecimen` | — |
| `SpecimenRejected` | `RejectSpecimen` | *Whenever `SpecimenRejected` → notify Clinical* |
| `LabTestInProgress` | `StartAnalysis` | — |
| `LabResultValidated` | `ValidateResult` | — |
| `LabResultPublished` | `PublishResult` | *Whenever `LabResultPublished` → publish to Clinical* |

**Read models:** `TestRequestQueueView`, `PendingValidationView`

---

## 💊 Pharmacy

**Aggregates:** `Prescription`, `Dispensation`, `MedicationBatch` (stock)

| 🔶 Domain Event | 🔵 Command | 🟣 Policy / Reaction |
|----------------|------------|----------------------|
| `PrescriptionRequested` | *(from Clinical `DiagnosisRecorded` policy)* | — |
| `PrescriptionVerified` | `VerifyPrescription` | — |
| `PrescriptionRejected` | `RejectPrescription` | *Whenever `PrescriptionRejected` → notify Clinical* |
| `MedicationDispensed` | `DispenseMedication` | *Whenever `MedicationDispensed` → publish to Billing (chargeable item)* |
| `StockReceived` | `ReceiveStock` | — |
| `StockLow` | *(system)* | *Whenever `StockLow` → create reorder / alert* |

**Read models:** `OpenPrescriptionsView`, `MedicationStockView`

---

## 💰 Insurance & Billing

**Aggregates:** `Coverage` (policy/eligibility), `Claim`, `Invoice`, `Payment`

| 🔶 Domain Event | 🔵 Command | 🟣 Policy / Reaction |
|----------------|------------|----------------------|
| `EligibilityRequested` | `CheckEligibility` | *Whenever `EligibilityRequested` → call Insurance Provider Gateway (ACL)* |
| `EligibilityConfirmed` / `EligibilityDenied` | *(from gateway via ACL)* | — |
| `ClaimSubmitted` | `SubmitClaim` | *Whenever `ClaimSubmitted` → send to gateway (X12 837 / FHIR)* |
| `ClaimAdjudicated` | *(from gateway via ACL)* | — |
| `ClaimApproved` / `ClaimRejected` | `AdjudicateClaim` | *Whenever `ClaimApproved` → finalize invoice* |
| `InvoiceIssued` | `IssueInvoice` | — |
| `PaymentReceived` | `RecordPayment` | *Whenever `PaymentReceived` → allocate to invoice* |
| `RefundIssued` | `IssueRefund` | — |

**Read models:** `ClaimStatusView`, `OutstandingInvoicesView`, `PatientStatementView`

---

⬅️ [Back to Event Storming index](./README.md) · ➡️ [03 — Integration & ACL](./03-integration-and-acl.md)
