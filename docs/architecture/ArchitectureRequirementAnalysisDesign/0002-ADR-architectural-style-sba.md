
# ADR 0002 — Architectural Style Selection (Service‑Based Architecture)

- **Status:** Accepted
- **Date:** 2026-02-27
- **Owner:** Architecture Working Group
- **Related:** ADR‑0001 (Deployment Variant — Kubernetes)

---

## 1. Context
The e‑Appraisal system must satisfy the confirmed requirements:
- **Scale & Performance:** ≥ 1000 concurrent users with **p95 < 2s** during appraisal peaks.
- **Security & Privacy:** PAN masking everywhere, **15‑minute idle timeout**, **3‑strike lockout** with manual IT‑Admin unlock, **no raw PAN** in logs/emails.
- **Audit & Compliance:** **7‑year** append‑only audit trail, centralized logging, SIEM alerts, alignment with ISO 27001/SOC 2/GDPR/DPDP.
- **Functional:** Appraisal workflow (comments, approvals), CTC step isolation and auto‑calc, feedback, policy‑aware exports, notifications.
- **Operate on Kubernetes** (ADR‑0001), with clear C4 containers: **Web**, **API**, **Reporting & Export**, **Notification**, **Audit Forwarder**.

We evaluated four base architectural styles that preserve the C4 semantics:
1) Pure Monolith
2) Modular Monolith
3) **Service‑Based Architecture (SBA)** — few, coarse‑grained services (maps to C4 containers)
4) Microservices — fine‑grained, many small services per sub‑capability

---

## 2. Decision
Adopt **Service‑Based Architecture (SBA)** with 5–6 coarse‑grained services that map 1:1 to the C4 container view:
- **Web (Frontend)**
- **API / Application** (workflow, comments, CTC, masking/RBAC)
- **Reporting & Export** (policy‑aware exports, masking, signed URLs)
- **Notification** (outbox → SMTP, retry/DLQ)
- **Audit Forwarder** (structured audit → SIEM; 7‑year retention backed by platform)

---

## 3. Rationale
- **Meets Scale & p95:** SBA allows **independent scaling** of API/Web vs **Export/Notification**, protecting p95 under appraisal spikes while avoiding the operational overhead of dozens of microservices.
- **Security & Compliance Fit:** Fewer services → **fewer surfaces** to harden and audit. Easier to enforce masking, lockout/idle, and consistent structured audit across all flows.
- **Workflow Integrity:** Appraisal workflow and CTC logic remain **cohesive** within the API service; no distributed transactions/sagas required.
- **C4 Alignment:** SBA matches the existing **C4 L2** model exactly; each container becomes its own runtime service.
- **Delivery Velocity:** Fewer repos/pipelines than microservices; faster iterations with clear boundaries and low blast radius.
- **Future Flexibility:** SBA keeps an easy **evolution path**—split hotspots into microservices later if data shows sustained pressure.

---

## 4. Options Considered (and why not chosen)
- **Pure Monolith:** Simple, but single scale unit; export CPU/IO can starve API/Web → p95 risks at ≥1000 conc.; larger blast radius for changes.
- **Modular Monolith:** Better boundaries than monolith but still one deployable; **no strong runtime isolation** for Export/Notification; limited scale runway.
- **Microservices:** Maximum isolation and autonomy, but **high operational/organizational complexity** (service contracts, distributed tracing, versioning, domain governance) not justified for current scope.

---

## 5. Consequences
**Positive**
- Predictable p95 under load via **HPA** and **node‑pool separation** for Export/Notification.
- Stronger security/compliance posture with consistent masking, audit, and SIEM integration.
- Straightforward CI/CD and rollout (few deployables; rolling updates on K8s).

**Negative**
- API service could become a **fat service** if not curated.
- Single primary relational DB could become a bottleneck if usage patterns change dramatically.

---

## 6. Risks & Mitigations
- **Export spikes impact API:**
  - *Mitigate:* Separate node pool for Export/Notification, HPA on queue depth/CPU, throttling/back‑pressure.
- **API growing too large:**
  - *Mitigate:* Enforce clear module boundaries; periodic refactor; observability to detect hotspots; split only when justified.
- **DB contention or failover impact:**
  - *Mitigate:* **Multi‑AZ** managed DB, short transactions, retry/back‑off & idempotency keys, read replicas if needed; quarterly **PITR restore** drills.

---

## 7. Architectural Implications (Implementation Notes)
- **K8s Deployments:** One Deployment per service; **Ingress** → Service:web/api → Deployments.
- **Security:** NetworkPolicies (deny‑all + allow‑lists), secrets via KMS/Key‑Vault/Secrets Manager, TLS at ingress, HSTS at edge.
- **Masking & RBAC:** Enforced in API and Export; **no raw PAN** anywhere (logs/emails/files).
- **Audit & SIEM:** Central **Audit Forwarder**; structured, append‑only events; 7‑year retention.
- **Resilience:** Multi‑AZ DB endpoint; retries with jitter; idempotent writes; graceful timeouts; planned failover tests.

---

## 8. Metrics & Acceptance
- **Performance:** p95 < 2s at ≥1000 concurrent users during peak appraisal windows.
- **Security/Privacy:** All masking tests pass; idle timeout = 15 minutes; lockout policy & manual unlock flows verified.
- **Auditability:** 100% of critical actions produce structured events; SIEM alert rules validated (failed login bursts, unlock spikes, export spikes, abnormal CTC edits).
- **Resilience/DR:** Successful monthly planned failover in non‑prod; quarterly **PITR restore** drill.

---

## 9. Migration & Evolution Path
- Start with SBA on Kubernetes; instrument everything (p95, queue depth, error rates, cost).
- If a capability becomes a sustained hotspot (e.g., Export or CTC calc), **split** that capability into its own microservice with a clear contract and database (when justified by data).
- Incremental adoption of **event‑driven** overlays (Export Requested, Notification Triggered, Audit Event stream) where async improves UX or resilience.

---

## 10. Decision Review
This ADR will be **reviewed after first peak appraisal cycle** or sooner if:
- p95 breaches recur despite HPA/node‑pool tuning.
- Security or audit findings indicate pattern changes are required.
- Organization/team structure significantly expands.

