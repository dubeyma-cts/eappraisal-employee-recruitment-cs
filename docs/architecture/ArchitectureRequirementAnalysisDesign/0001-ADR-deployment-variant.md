
# ADR 0001 — Deployment Variant for e‑Appraisal

- **Status:** Accepted
- **Date:** 2026-02-27
- **Owner:** Architecture Working Group

---

## 1. Context
The e‑Appraisal system must satisfy confirmed Functional and Non‑Functional Requirements, including **≥ 1000 concurrent users**, **p95 < 2 seconds**, **role‑based access with PAN masking**, **15‑minute idle timeout**, **3‑strike lockout with manual IT‑admin unlock**, **7‑year audit retention**, and **SIEM alerting**. The logical design is captured in the **C4 L1/L2** views and detailed system context, with independent containers for **Web**, **API**, **Reporting & Export**, **Notification**, and **Audit & Events**.

We evaluated three deployment variants that keep these C4 semantics intact:
1) **All‑in‑One VM**, 2) **Kubernetes**, 3) **App Service** (managed PaaS). The alternatives and diagrams are documented in the deployment variants and portable deployment views.

---

## 2. Decision
Adopt **Kubernetes** as the primary deployment platform for e‑Appraisal (production and pre‑prod).

---

## 3. Options Considered
- **All‑in‑One VM** — Single VM hosts Web, API, Export, Notification, and Audit forwarder processes.
- **Kubernetes (Chosen)** — Separate Deployments for Web, API, Export, Notification, and Audit forwarder, fronted by Ingress/Services; managed DB and object storage.
- **App Service (PaaS)** — Web/API on managed app services; Export/Notification as container apps; managed DB and object storage.

---

## 4. Rationale
- **Performance & Scale:** Kubernetes provides **Horizontal Pod Autoscaling (HPA)** and optional **node‑pool separation**, so **Export** spikes cannot degrade **API/Web** latency; this best supports **≥1000 concurrent** users and **p95 < 2s** targets.
- **Security & Compliance:** K8s supports **NetworkPolicies**, **Pod Security** baselines, and secret stores, reinforcing application guardrails (masking, lockout, idle timeout) and enabling uniform **SIEM** forwarding via sidecars/agents; aligns with ISO 27001 Annex A controls and auditability patterns in our docs.
- **Operational Resilience:** Rolling updates, self‑healing, **multi‑AZ** nodes, and well‑known DR patterns (cross‑region DB replica; object storage replication; DNS failover) fit the appraisal windows and uptime needs.
- **Architectural Runway:** Clean 1:1 mapping from C4 containers to Deployments/Services; easier to add new services (e.g., HRIS adapters, dashboards) without re‑platforming.

---

## 5. Trade‑offs (Summary)
A comparative matrix of **VM vs Kubernetes vs App Service** shows K8s as the best balance of **scale**, **security**, **audit/SIEM**, **DR**, and **future growth**, while App Service offers faster time‑to‑value with less control, and VM is best only for short pilots.

---

## 6. Consequences
- **Positive:** Meets scale and latency SLOs; isolates Export/Notification; strengthens security posture; simplifies audit/SIEM patterns; keeps C4 semantics intact.
- **Negative:** Requires K8s skills (HPA tuning, netpol, observability); higher platform complexity than VM/App Service.

---

## 7. Risks & Mitigations
- **Risk:** Misconfigured autoscaling or resource limits may still impact p95.
  - **Mitigation:** Load tests per release; calibrate HPA thresholds; separate node pool for Export/Notif.
- **Risk:** NetworkPolicies too permissive or restrictive.
  - **Mitigation:** Deny‑all baseline; allow‑list flows (web→api, api→db/obj/smtp/idp).
- **Risk:** Operational overhead and skill gap for K8s.
  - **Mitigation:** Managed K8s service, GitOps, baseline hardening, runbooks, and training.

---

## 8. Rollback Plan
If Kubernetes cannot meet SLOs or staffing constraints in time:
- **Fallback 1:** **App Service** (Web/API as app services; Export/Notification as container apps) while keeping the same C4 boundaries; preserve masking, SIEM, audit, and DR patterns.
- **Fallback 2:** **All‑in‑One VM** for a limited pilot only, with strict export windows and hard CPU quotas; not recommended for full‑scale rollout.

---

## 9. Implementation Plan (High‑Level)
1) Provision **managed K8s** (multi‑AZ) with cluster baseline (ingress, cert/TLS, logging).
2) Create **Namespaces**, **Services**, **Deployments** for Web, API, Export, Notification, Audit; set **requests/limits**; enable **HPA**.
3) Apply **NetworkPolicies** (deny‑all baseline, allow only required flows); configure **secrets** from KMS/Vault.
4) Wire **SIEM** forwarders; validate **7‑year** audit retention in backend.
5) Run **load tests** to prove **≥1000 conc.** and **p95 < 2s**; tune HPA and node pools for Export.
6) Finalize **DR** (cross‑region DB replica, storage replication, DNS failover); execute drills.

---

## 10. Related Artifacts
- **C4 L1/L2 (System + Container)** — architectural baseline.
- **System Context (Detailed)** — scope, actors, externals, trust boundaries.
- **Deployment Variants** — VM vs K8s vs App Service.
- **Portable Deployment Diagrams** — environment and runtime placement with ASCII fallbacks.
- **Trade‑offs** — pros/cons and mitigations per variant.
- **Decision Rationale (Why Kubernetes)** — consolidated reasoning and next steps.
