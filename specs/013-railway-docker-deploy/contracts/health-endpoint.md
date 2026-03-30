# Contract: Health Check Endpoint

**Feature**: `013-railway-docker-deploy`
**Date**: 2026-03-30

---

## Endpoint

```
GET /api/health
```

## Authentication

**None** — this endpoint is anonymous. It must not require a JWT bearer token, as Railway's health check has no mechanism to pass authorization headers.

## Request

No request body or query parameters.

## Response

### 200 OK — Service is healthy

```json
{
  "status": "healthy"
}
```

### 503 Service Unavailable — (future) unhealthy

If health checks are extended to probe database connectivity, return `503` when the database is unreachable.

```json
{
  "status": "unhealthy",
  "reason": "Database connection failed"
}
```

*Note*: The MVP implementation returns `200` unconditionally. Database-probing health checks are out of scope for this feature.

## Headers

| Header | Value |
|--------|-------|
| `Content-Type` | `application/json` |
| `Cache-Control` | `no-cache, no-store` |

## Usage

- **Railway deploy health check**: configured via `healthcheckPath = "/api/health"` in `railway.toml`
- **Uptime monitoring**: can be polled by external monitoring tools
- **Load balancer readiness probe**: compatible with standard readiness/liveness probe pattern

## Implementation Notes

- Implemented in `HealthController` extending `backendControllerBase` (per constitution Naming Gate)
- Decorated with `[AllowAnonymous]` to bypass JWT authentication
- Route: `[Route("api/[controller]")]` → resolves to `/api/health`
- Action: `[HttpGet]` → `GET /api/health`
