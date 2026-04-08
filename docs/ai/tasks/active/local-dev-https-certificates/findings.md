# Findings

- The machine had multiple localhost ASP.NET development certificates.
- Firefox Developer Edition was not explicitly configured to trust the Windows certificate store.
- `OakERP.API` HTTPS profile uses `https://localhost:7057` and HTTP fallback uses `http://localhost:5169`.
- The API code itself was not the certificate problem; this was local machine and browser trust state.
