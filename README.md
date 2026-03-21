# studious-umbrella

## Project Structure

### Frontend
/frontend
- Contains the client application (UI)
- All frontend-related development happens here
- Technologies: -

---

### Gateway
/gateway
- Entry point for all external requests
- Handles routing, aggregation, and communication with services
- Exposes public API for the frontend

---

### Services
/services
- Contains all backend microservices
- Each service is independent and has its own:
  - solution (.sln)
  - projects
  - infrastructure

Example:

/services/auth-service

/services/voice-service
