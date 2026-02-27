
# ADR 0004 - eAppraisal Database Selection & Deployment

- **Status:** Accepted
- **Date:** 2026-02-27
- **Owner:** Architecture Working Group
- **Related:** ADR0001 (Kubernetes Deployment Variant), ADR0002 (Architectural Style - SBA), ADR0003 (Azure DB Global Deployment Model)

---

## 1. Context
The eAppraisal system is a relational OLTP workload with users across multiple geographies. It requires:
- **Strong consistency** on transactional writes (comments, approvals, CTC),
- **Global read performance** for dashboards/reports,
- **High availability** within the primary region and **disaster recovery** to a second region,
- **Security & compliance**: PAN masking, 7year audit, SIEM, TLS, secrets in vault, and evidenceable controls.

Azure provides several managed database models that can satisfy these needs, notably **Azure SQL Database** (with zoneredundancy and failover groups), **Azure SQL Managed Instance** (MI) failover groups, and **Azure Database for PostgreSQL/MySQL Flexible Server** (zoneredundant HA + crossregion read replicas). Hyperscale on Azure SQL adds read scaleout for analytics; Azure Front Door can reduce user RTT globally. 

---

## 2. Decision
Adopt **Azure SQL Database** with a **singlewrite, multiread** pattern:
1) Enable **ZoneRedundancy** in the primary region for inregion HA. 
2) Configure **Failover Groups** to a secondary region to obtain **readwrite** and **readonly** listeners for geoDR and read offload. 
3) Front the application with **Azure Front Door** (Anycast + splitTCP) to minimize client RTT globally; route readonly traffic to the **readonly listener** where appropriate. 
4) If read load grows, use **Azure SQL Hyperscale read scaleout** to offload analytics/reporting. 

---

## 3. Rationale
- **Latency & UX:** Front Door brings users to the closest edge POP and uses Microsoft's backbone, lowering perceived latency; read workloads can be served from the readonly listener or Hyperscale read replicas. 
- **HA/DR:** Zoneredundant databases handle AZ failures; Failover Groups offer regional redundancy, stable endpoints, and a welldocumented DR envelope (RTO/RPO). 
- **Operational simplicity:** PaaS features (automated backups, HA/DR primitives) align with the WellArchitected SQL guidance and simplify audits. 

---

## 4. Options Considered
1) **Azure SQL Database (Zoneredundant + Failover Groups)** - *Chosen*. Strong PaaS posture, read offload, clear DR. 
2) **Azure SQL Managed Instance (Failover Groups)** - choose if near full SQL Server compatibility is mandatory; slightly higher ops/networking complexity. 
3) **Azure Database for PostgreSQL/MySQL Flexible** - use ZoneRedundant HA and crossregion read replicas (async) for OSS stacks; accept replica lag. 
4) **Cosmos DB (multiwrite)** - only if global multimaster writes and tunable consistency become hard requirements; implies NoSQL model and conflict strategy. 

---

## 5. Consequences
**Positive**
- Predictable global performance; read offload; clear HA/DR; auditfriendly managed capabilities. 

**Negative**
- Remote users pay write RTT to the primary region. For OSS engines, crossregion replicas are async (stale reads possible). 

---

## 6. Risks & Mitigations
- **Failover/client resilience:** use Failover Group listeners; implement retry with backoff and idempotency for write paths. 
- **Read spikes/reporting:** leverage Hyperscale read scaleout or readonly listener; throttle/report windows. 
- **Residency constraints:** place secondaries/replicas in compliant regions; consult nonpaired region guidance when needed. 

---

## 7. Implementation Plan (HighLevel)
1) Provision Azure SQL Database in primary region with **ZoneRedundancy**. 
2) Create a **Failover Group** to a secondary region; verify readonly and readwrite listeners. 
3) Place **Azure Front Door** in front; enforce origin restrictions and WAF; test global probes. 
4) (Optional) Move to **Hyperscale** and enable **read scaleout** for reporting. 
5) Run DR drills (planned geofailover); document RTO/RPO, retry behavior, and application continuity. 

---

## 8. Acceptance Criteria
- p95 < 2s during appraisal windows measured from APAC/EMEA/AMER test agents with Front Door enabled. 
- Successful planned failover using **Failover Group** listeners with no connectionstring changes. 
- Evidence of **zoneredundancy**, **backups/PITR**, and **read offload** (readonly listener or Hyperscale). 

---

## 9. References
- Azure SQL Database **ZoneRedundancy** - Microsoft Learn. 
- Azure SQL Database **Active GeoReplication / Failover Groups** - Microsoft Learn. 
- Azure SQL Database **Hyperscale Read Scaleout** - Microsoft Learn. 
- **Azure Front Door** traffic acceleration (Anycast, splitTCP) - Microsoft Learn. 
- **WellArchitected SQL Database** service guide - Microsoft Learn. 
- **PostgreSQL Flexible** HA/replicas - Azure Docs. 
- **MySQL Flexible** read replicas - Microsoft Learn. 
- **Cosmos DB** consistency & multiwrite - Microsoft Learn. 
