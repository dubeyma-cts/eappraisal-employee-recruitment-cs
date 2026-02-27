
# ADR 0006 - Backup & Recovery Strategy (eAppraisal)

- **Status:** Accepted
- **Date:** 2026-02-27
- **Owner:** Architecture Working Group
- **Related:** ADR0001 (Kubernetes Deployment), ADR0002 (SBA Architectural Style), ADR0003 (Azure DB Global Deployment), ADR0004 (DB Selection), ADR0005 (Site Reliability)

---

## 1. Context
The eAppraisal platform is a **relational OLTP** system on **Azure SQL Database** with global users and strict compliance requirements (7year audit, masking, SIEM). Azure SQL provides **automated platform backups** (weekly full, 12-24h differentials, 5-10 min log) enabling **PointinTime Restore (PITR)** within a shortterm retention window (7-35 days, tierdependent) and **LongTerm Retention (LTR)** up to 10 years. We must combine these capabilities with **independent copies** and **regular restore drills** to mitigate logical corruption/ransomware and satisfy recovery objectives. 

---

## 2. Decision
Adopt a **layered backup & recovery strategy** for Azure SQL Database:

1) **Enable/confirm PITR retention** to **28-35 days** for production databases; adjust through automated backup settings as needed. 
2) **Enable LTR** (weekly fulls) with **policybased retention** (e.g., 12 monthly, 7 yearly) to satisfy regulatory needs up to 10 years. 
3) **Daily encrypted logical export** of the primary database to **Azure Blob Storage** (separate storage account / RG) to maintain **independent, offplatform copies**; monitor/alert on job failures. 
4) **Georedundant backup storage** for automated backups (GRS/ZRS, as appropriate) to improve durability and regional recoverability; manage via backup storage redundancy settings. 
5) **Quarterly restore drills**: perform **PITR** restores (within retention) and **LTR** restores (long range) into an isolated subscription; document RTO/RPO evidence. 
6) **Runbook variants**: (a) accidental delete  PITR; (b) older logical corruption  LTR or daily export; (c) regional disruption  georestore/failover group and, if needed, LTR. 

---

## 3. Rationale
- **Platformnative resilience**: Azure SQL automated backups + PITR reduce admin overhead and enable rapid recovery from recent errors without custom jobs. 
- **Compliance & longwindow recovery**: LTR provides retention up to 10 years to satisfy audits and to recover from latent corruption beyond PITR windows. 
- **Defenseindepth**: Independent **Blob exports** protect against scenarios where replication and automated backups reflect the same corruption; exports also support forensics and lowerblast recovery. 
- **Recovery confidence**: Regular **restore tests** validate backup health and ensure operators are proficient; recommended in Azure SQL backup bestpractice articles. 

---

## 4. Options Considered
1) **Rely only on PITR** - simplest; insufficient for longrange recovery and some compliance cases; risk if corruption goes undetected beyond PITR. 
2) **PITR + LTR (no exports)** - strong baseline, but lacks an **independent copy** under separate controls; fewer options for selective restore and ransomware forensics. 
3) **PITR + LTR + Daily Export** - *Chosen*. Balances platform simplicity, compliance, and independence; covers a wider set of failure/corruption modes. 

---

## 5. Consequences
**Positive**
- Meets compliance via **LTR**, supports fast recovery via **PITR**, and adds **independent exports** for corruption/ransomware scenarios. 
- Clear, testable runbooks with measurable **RTO/RPO**; operational confidence through quarterly drills. 

**Negative**
- Additional storage & automation cost for daily exports.
- Operational effort to schedule and review periodic restore tests.

---

## 6. Risks & Mitigations
- **Export job failures / silent backup issues**  Azure Monitor alerts on export pipeline and backup health; weekly audits of job status dashboards. 
- **Retention misconfiguration**  Enforce policy via IaC and change management; periodically verify PITR/LTR via portal/CLI. 
- **Regional outage**  Ensure backup storage redundancy (GRS/ZRS) and document **georestore** path with estimated recovery times; combine with Failover Group DR. 

---

## 7. Implementation Plan (HighLevel)
1) **PITR & Redundancy**: Set PITR retention to 28-35 days; configure **backup storage redundancy** (GRS/ZRS) on the logical server. 
2) **LTR Policies**: Define weekly full  LTR with monthly/yearly retention; enable on all production DBs and validate via backup history. 
3) **Daily Export**: Implement automated daily export to **separate Blob Storage** (different RG & access boundary); encrypt at rest; alert on failures. 
4) **Runbooks**: Publish playbooks for accidental delete (PITR), latent corruption (LTR/export), and regional disruptions (georestore/Failover Group). 
5) **Testing**: Schedule **quarterly restore drills** (PITR + LTR) into an isolated subscription; record **RTO/RPO** and fix gaps. 

---

## 8. Acceptance Criteria
- PITR restores within retention window succeed in ** N minutes**; LTR restore validated at least **quarterly**. 
- Export jobs succeed ** 99.5%** monthly, with alerts and incident followup for failures. 
- Evidence pack includes **policy screenshots/CLI output**, **restore logs**, and **RTO/RPO** measurements.

---

## 9. References
- Azure SQL Database automated backups, PITR cadence & architecture. 
- Change automated backup settings (PITR retention & backup storage redundancy). 
- Practical guidance on backup/restore practices & monitoring. 
- LongTerm Retention (up to 10 years) concepts and policies. 
- Independent exports for offplatform copies and alerting. 
- SQL reliability (transient faults, AZ/regional resilience) context. 
