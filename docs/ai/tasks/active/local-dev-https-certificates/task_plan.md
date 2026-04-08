# Local Dev HTTPS Certificates

## Scope

- Repair the local ASP.NET Core developer certificate setup for OakERP API development.
- Keep both HTTPS and HTTP launch profiles available.
- Align Firefox Developer Edition with Windows certificate trust.
- Add a short operational doc for future local setup.

## Constraints

- Do not weaken API HTTPS behavior or remove the HTTP fallback profile.
- Treat this as local development only, not production TLS.
- Keep repo documentation concise and operational.

## Ordered Steps

1. Audit current `dotnet dev-certs` and Firefox trust state.
2. Clean stale ASP.NET developer certificates and create one fresh trusted localhost certificate.
3. Verify the API serves successfully over `https://localhost:7057`.
4. Configure Firefox Developer Edition to trust Windows certificates on restart.
5. Add a short local HTTPS setup note under `OakERP.Docs` and link it from the dev docs index.
6. Verify HTTPS and HTTP endpoints remain usable locally.

## Validation

- `dotnet dev-certs https --check --trust`
- launch `OakERP.API`
- verify `https://localhost:7057/swagger/index.html`
- verify `http://localhost:5169/swagger/index.html`
