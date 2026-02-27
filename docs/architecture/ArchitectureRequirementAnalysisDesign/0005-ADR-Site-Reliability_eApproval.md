
# ADR 0005 - Site Reliability Strategy (eAppraisal)

- **Status:** Accepted
- **Date:** 2026-02-27
- **Owner:** Architecture Working Group
- **Related:** ADR0001 (Kubernetes Deployment), ADR0002 (Architectural Style - SBA), ADR0003 (Azure DB Global Deployment), ADR0004 (DB Selection & Deployment)

---

## 1. Context
The eAppraisal system serves global users (APAC/EMEA/AMER) and must maintain reliable service through appraisal peaks. The stack runs on **AKS** (managed Kubernetes), fronts traffic via **Azure Front Door (AFD)**, and uses **Azure SQL Database** with zoneredundancy and failover groups for DR. AKS reliability is a **shared responsibility**: Microsoft manages the control plane, while we must design node pools, zones, and workload policies for resilience.   Azure SQL offers builtin redundancy and guidance for handling transient faults, AZ outages, and regional DR with backups and geofeatures.   For global ingress, AFD lowers RTT with Anycast/splitTCP but recent 2025 incidents showed a need for **alternate ingress** options to mitigate configurationblast risks. 

---

## 2. Decision
Adopt a **tiered sitereliability strategy**:

1) **AKS (primary compute) with zoneredundant design and SRE controls**: multiAZ node pools, multireplica deployments, PDBs, probes, autoscaling, and topology spread constraints per best practices. 
2) **Global ingress**: Use **Azure Front Door** as primary entry, **plus a regional failover ingress** (via Azure Traffic Manager  regional Application Gateway) to mitigate global controlplane risks. 
3) **Database reliability**: **Azure SQL Database (zoneredundant)** for inregion HA; **Failover Group** to secondary region for DR and readonly offload; continue SRE hardening against transient faults. 

---

## 3. Rationale
- **AKS**: Microsoft Learn emphasizes that workload reliability (node pools, zones, PDBs, probes, autoscaling) is our responsibility; adopting the documented patterns reduces failure impact and supports rolling upgrades. 
- **Ingress**: AFD's Anycast edge improves global performance, but the 2025 AFD incidents highlighted configuration as a nongeographic fault domain; we therefore add a **secondary ingress path** to reduce singlecontrolplane risk. 
- **Database**: Azure SQL Database has builtin redundancy and guidance for zone and regional resilience; combining **zoneredundancy** with **Failover Groups** provides clear RTO/RPO while handling transient errors gracefully. 

---

## 4. Options Considered
1) **AFD as sole global ingress** - simpler, but single controlplane risk (lessons from Oct 2025). 
2) **AKS singleAZ node pools** - cheaper, but lower resilience to zonal outages; not aligned with AKS reliability guidance. 
3) **No PDBs / singlereplica apps** - simplest ops, but higher blast radius during upgrades and node drains; contrary to best practices. 

---

## 5. Consequences
**Positive**
- Sustains traffic during zonal failures and planned upgrades (AKS zones + PDBs + multireplica + autoscaling). 
- Reduces dependency on a single global ingress control plane (AFD + regional ingress fallback). 
- Clear database HA/DR posture with builtin redundancy and failover groups. 

**Negative**
- Higher cost/complexity (extra ingress path, multiAZ capacity, readiness/health probes overhead).
- Requires automation and runbooks for deterministic failover.

---

## 6. Risks & Mitigations
- **Global edge outage / misconfiguration**: Maintain **alternate ingress**; automate healthbased failover using TM probes; exercise gameday drills. 
- **Zonal capacity shortfall**: Enable cluster autoscaler with multiAZ node pools; prescale during appraisal peaks. 
- **Upgrade disruptions**: Enforce PDBs, surge settings, and topology spread; canary or blue/green rollouts. 
- **DB transient faults**: Implement retry with backoff in app; monitor SQL error taxonomy; validate failover with planned drills. 

---

## 7. Implementation Plan (HighLevel)
1) **AKS**: Create multiAZ node pools (system + user), enable HPA & Cluster Autoscaler, set readiness/liveness/startup probes, apply PDBs and topology spread constraints; enforce minimum replicas (3) for API/WEB. 
2) **Ingress**: Configure **AFD** primary; stand up **Traffic Manager** with regional Application Gateway as alternate path; prove failover via synthetic monitors. 
3) **Database**: Ensure **zoneredundant** Azure SQL deployment and configure **Failover Group**; test planned/unplanned failovers; validate retry behavior. 

---

## 8. Acceptance Criteria
- During rolling upgrades and zonal failure simulations, **p95 latency** and **error rate** remain within SLOs (no singlereplica exposure). 
- Synthetic tests confirm **AFDregional ingress** failover in < N minutes under fault injection. 
- Planned DB failover meets agreed **RTO/RPO** with successful app retries; no configuration changes required. 

---

## 9. References
- **AKS reliability (shared responsibility, zones, best practices)** - Microsoft Learn & bestpractices. 
- **AFD lessons learned (Oct 2025 incidents)** - Microsoft Community postincident report; thirdparty analyses. 
- **Azure SQL Database reliability** - Microsoft Learn reliability guide (transient faults, AZ/regional events, backups). 
