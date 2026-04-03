# Findings

- `AuthService` was the only service in `OakERP.Auth` directly depending on `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`.
- The current auth logic uses only five Identity operations:
  - find user by email
  - create user with password
  - add user to role
  - check password sign-in
  - get user roles
- The JWT mapping seam already exists inside `AuthService` as a single `ApplicationUser -> JwtTokenInput` mapping helper, so this slice does not need to touch JWT claims or token generation behavior.
- `ApplicationUser` remains in `OakERP.Domain` and still inherits `IdentityUser`; that is intentionally out of scope for this slice.
- The slice can stay inside `OakERP.Auth` plus unit-test updates without requiring API or controller contract changes.
