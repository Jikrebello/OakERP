# Local HTTPS Dev Certificate

OakERP uses the normal ASP.NET Core localhost development certificate for local HTTPS.

## What this is

- The certificate is for **local development only**
- it is expected to be **self-signed**
- browsers must trust it locally or `https://localhost` will show a warning

## Normal OakERP local URLs

- API HTTPS: `https://localhost:7057`
- API HTTP fallback: `http://localhost:5169`

Use HTTPS by default. Keep HTTP only as a fallback while repairing local certificate trust.

## Reset the ASP.NET Core dev certificate

From the repo root or any terminal:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
dotnet dev-certs https --check --trust
```

Expected result:
- one trusted `CN=localhost` development certificate is available

## Firefox Developer Edition

Firefox Developer Edition may use its own certificate handling instead of Windows trust automatically.

Preferred fix:
- enable Firefox to trust certificates from the Windows root store

If Firefox still warns after the `dotnet dev-certs` reset:
1. close Firefox Developer Edition
2. reopen it after local trust is fixed
3. revisit `https://localhost:7057/swagger`

If needed, set this preference in Firefox:

- `security.enterprise_roots.enabled = true`

That tells Firefox to trust certificates already trusted by Windows.

## If HTTPS still fails

Check these in order:

1. `dotnet dev-certs https --check --trust`
2. confirm the API is launching on the HTTPS profile
3. confirm no old localhost certificate warnings remain in the browser
4. use `http://localhost:5169` temporarily while fixing browser trust

## What not to do

- do not disable HTTPS permanently for local development
- do not change production certificate behavior because of a localhost trust issue
- do not commit machine-specific certificate files into the repo
