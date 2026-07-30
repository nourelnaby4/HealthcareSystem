# 01 — Event Storming Workflow

How to run Event Storming for this project: the notation, the phases, and a ready-to-use facilitator runbook.

---

## Notation Legend

Event Storming is modeled on colored sticky notes. Each color has a fixed meaning.

| Sticky | Color | Meaning | Convention |
|--------|-------|---------|------------|
| 🔶 | Orange | **Domain Event** | Past tense: *“something happened”* (`PatientAdmitted`) |
| 🔵 | Blue | **Command** | Imperative intent: *“do something”* (`AdmitPatient`) |
| 🟣 | Lilac | **Policy** | Reactive rule: *“Whenever `X`, then `Y`”* |
| 🟡 | Yellow | **Aggregate / Entity** | Consistency boundary (`Encounter`, `Claim`) |
| 🟩 | Green | **Read Model / Projection** | Query side (`EncounterSummaryView`) |
| 🟥 | Red/Pink | **External System / Actor** | `InsuranceProviderGateway`, `Clinician` |
| 📝 | Pink | **Hotspot / Question** | Open issue, risk, uncertainty |
| 📦 | Big yellow box | **Bounded Context / Module** | A deployed module of the monolith |

---

## Workflow Phases

This is the **step-by-step process** the team follows. Each phase has a goal, inputs, and a concrete output that feeds the next.

### Phase 0 — Preparation
- Invite the right people: domain experts (clinician, lab tech, pharmacist, billing officer), engineers.
- Gather materials: an infinite canvas (Miro / Mural / physical wall) and the legend above.
- Pick a **trigger question**: *"What happens from a patient arriving to the bill being settled?"*
- **Output:** canvas ready, roles assigned.

### Phase 1 — Big Picture (chaotic exploration)
- Storm 🔶 **domain events** only, left → right = time.
- Spread out, no ordering/critique — quantity over quality.
- Look for missing links; mark duplicate/contradictory events.
- **Output:** an orange timeline of everything that *happens* in the domain.

### Phase 2 — Hotspots & Pacing
- Add 📝 **hotspots** wherever there’s confusion, a business rule in dispute, or missing knowledge.
- Group events into **swimlanes** by sub-process (e.g., *Registration → Encounter → Diagnostics → Treatment → Billing*).
- **Output:** ordered timeline, open-questions backlog, identified sub-processes.

### Phase 3 — Process Modeling (add detail)
For each event, enrich with the other stickies:
- 🔵 Command that **causes** the event (place it to the left).
- 🟥 Actor / External system that issues the command (above the command).
- 🟣 Policy that **reacts** to the event (below it, pointing to the next command).
- 🟩 Read model the actor needs to decide (next to the actor).
- **Output:** a complete flow of *intent → action → event → reaction*.

### Phase 4 — Design-Level Storming
- Cluster events/commands under 🟡 **Aggregates** (transactional consistency boundaries).
- Draw **dependency arrows**; flag every cross-cluster dependency as a candidate 🔶 **integration event**.
- Group aggregates into 📦 **Bounded Contexts** (= modules in the modular monolith).
- **Output:** aggregates, bounded contexts, integration contracts.

### Phase 5 — Mapping to the Modular Monolith
- Each 📦 Bounded Context ⇒ one **module** folder.
- Every cross-context arrow ⇒ an **integration event** on the in-process bus (Outbox/Inbox).
- Every 🟥 external system ⇒ an **Anti-Corruption Layer (ACL) + adapter**.
- Every 🟩 read model ⇒ a projection in `Application/Projections`.
- **Output:** module list, integration-event catalog, adapter list → ready to scaffold code.

---

## Facilitator Runbook (per workshop)

1. **Open (10 min):** goal, legend, trigger question.
2. **Storm events (45 min):** Phase 1 — orange stickies only.
3. **Review & pace (20 min):** Phase 2 — swimlanes + hotspots.
4. **Enrich (60 min):** Phase 3 — commands, actors, policies, read models.
5. **Design (45 min):** Phase 4 — aggregates & contexts.
6. **Map to modules (30 min):** Phase 5 — integration events & ACLs.
7. **Retro (10 min):** capture hotspots, assign owners, schedule follow-ups.

> Rule of thumb: a domain event that *no one reacts to* is a smell. A command that *triggers nothing* is a smell.

---

## Facilitator Checklist

- [ ] Legend visible to all participants
- [ ] Trigger question agreed
- [ ] Phase 1 timeline captured (orange events)
- [ ] Swimlanes + hotspots (Phase 2)
- [ ] Commands / actors / policies / read models (Phase 3)
- [ ] Aggregates & bounded contexts (Phase 4)
- [ ] Integration-event catalog finalized
- [ ] Module + ACL list produced (Phase 5)
- [ ] Hotspots assigned to owners with due dates
- [ ] Storm exported & saved under `docs/event-storming/`

---

⬅️ [Back to Event Storming index](./README.md)
