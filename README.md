# Ephemeral Cyber Deception & Automated IR Environment (Work In Progress)

> **Status:** Phase 1 - Honey-API Development (TDD Architecture Complete, Implementation In Progress)

## Objective
Architecting a self-hosted, ephemeral cyber deception lab to capture real-world threat data, perform detection engineering mapped to the MITRE ATT&CK framework, and automate Incident Response (IR) workflows.

## The Problem
Static, simulated data doesn't provide the "noise" and unpredictability of real-world attacks. Traditional honeypots lack the custom application-layer telemetry needed for modern AppSec analysis.

## The Solution (Proposed Architecture)
This project builds a complete, ephemeral "SOC in a box":
1. **The Bait (Honey-API):** A custom, intentionally vulnerable REST API (.NET/Node.js) designed to capture rich HTTP payloads and attacker behavior.
2. **The Brain (SIEM):** Wazuh, configured with custom decoders and rules to map telemetry to the MITRE ATT&CK framework.
3. **The Muscle (SOAR):** Shuffle, orchestrating automated OSINT enrichment (IP/ASN reputation) and active response.
4. **The Foundation (IaC):** Docker and Terraform for rapid, ephemeral deployment to minimize cloud costs and attack surface.

## Current Roadmap
- [x] Phase 0: Repository setup and architecture design.
- [x] Phase 1a: Honey-API TDD Architecture (29 tests specified, RED state complete).
- [ ] Phase 1b: Honey-API Implementation (Services, Endpoints, Middleware - GREEN state).
- [ ] Phase 1c: Code Refactoring and Optimization (REFACTOR state).
- [ ] Phase 2: SIEM integration (Wazuh) and initial log ingestion.
- [ ] Phase 3: Detection Engineering (Custom Decoders & Rules).
- [ ] Phase 4: SOAR integration (Shuffle) and webhook automation.

---
*Built as a blueprint for modern DevSecOps teams, this project showcases the integration of custom telemetry, ephemeral infrastructure, and autonomous incident response.*
