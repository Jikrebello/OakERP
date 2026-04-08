# Progress

## Started

- Audited dev certificate state, launch profiles, and Firefox Developer Edition profile settings.

## Completed

- Cleaned stale ASP.NET Core localhost development certificates with `dotnet dev-certs https --clean`.
- Created and trusted one fresh localhost development certificate with `dotnet dev-certs https --trust`.
- Verified trust with `dotnet dev-certs https --check --trust`.
- Verified the API serves successfully on `https://localhost:7057/swagger/index.html`.
- Verified the HTTP fallback still serves successfully on `http://localhost:5169/swagger/index.html`.
- Added a local Firefox Developer Edition profile override via `user.js` to enable `security.enterprise_roots.enabled`.
- Added a concise local HTTPS setup note under `OakERP.Docs` and linked it from the dev docs index.

## Validation

- `dotnet dev-certs https --clean`
- `dotnet dev-certs https --trust`
- `dotnet dev-certs https --check --trust`
- launched `OakERP.API` and verified `https://localhost:7057/swagger/index.html`
- launched `OakERP.API --launch-profile http` and verified `http://localhost:5169/swagger/index.html`

## Notes

- Firefox Developer Edition was running during the change, so the new `security.enterprise_roots.enabled` setting will apply after Firefox is restarted.
- This task changed local machine certificate state and local Firefox profile state in addition to repo docs.
