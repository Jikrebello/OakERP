## Current State

- `IJwtGenerator` currently takes `ApplicationUser` directly.
- `JwtGenerator` only uses three fields from `ApplicationUser` to build token claims:
  - `Id`
  - `Email`
  - `TenantId`
- The only production caller is `AuthService`, which calls `jwtGenerator.Generate(user)` in register and login flows.
- Unit tests currently mock `IJwtGenerator.Generate(It.IsAny<ApplicationUser>())`.

## Dependency Interpretation

- The JWT generator does not need the full domain identity entity; it only needs the three values already used in claims.
- The correct mapping point is inside `AuthService`, where `ApplicationUser` is already available as part of the existing Identity workflow.
- This slice can remove the direct `IJwtGenerator -> ApplicationUser` contract dependency without changing API contracts, controllers, persistence, or token behavior.
- The implemented mapping point is a single private helper inside `AuthService`, so the domain entity stays at the auth orchestration edge and does not leak into the JWT generator contract.

## Risks

- The biggest risk is accidentally changing claim names, claim values, expiration behavior, or signing behavior while changing the generator input type.
- Unit tests need to be updated carefully so they still validate auth behavior rather than overfitting to the old `ApplicationUser` parameter type.
- This slice must not broaden into changing `ApplicationUser`, repository contracts, or Identity persistence types.
