# CI/CD and Deployment Notes

## Purpose

This document records the current CI/CD deployment workflow and the rewrite impact. The existing deployment workflow is protected infrastructure. Do not delete or modify it unless the rewrite requires a deliberate pipeline update.

The currently running local IIS app is disposable and has no real users/data. Deployment still matters because service/app pool/routes must match the rewritten app, but data preservation is not required.

Current workflow:

- `.github/workflows/deploy.yml`

## Current Workflow Summary

The workflow is named `Build, Test, and Deploy`.

Trigger:

- Push to `main`.

Runner:

- `self-hosted`.

Jobs:

| Job | Purpose |
| --- | --- |
| `build` | Builds backend services in Release |
| `test` | Runs backend xUnit tests |
| `publish` | Publishes backend services and uploads artifacts |
| `configure` | Writes `.env` to `C:\inetpub\HabitTracker\.env` from GitHub secrets |
| `deploy` | Stops IIS app pools, robocopies published output, restarts app pools |

Current backend services in pipeline:

- AuthService
- HabitService
- HabitCompletionService
- ApiGateway

Current deployed IIS app pools:

- `HabitTracker-AuthService`
- `HabitTracker-HabitService`
- `HabitTracker-HabitCompletionService`
- `HabitTracker-ApiGateway`

Current deployment paths:

- `C:\inetpub\HabitTracker\AuthService`
- `C:\inetpub\HabitTracker\HabitService`
- `C:\inetpub\HabitTracker\HabitCompletionService`
- `C:\inetpub\HabitTracker\ApiGateway`

Secrets used:

- `HABITTRACKER_DB_PASSWORD`
- `HABITTRACKER_JWT_SECRET`

## Important Protection Rule

Do not modify `.github/workflows/deploy.yml` during planning.

Modify it only when:

- backend service list changes,
- frontend deployment target is ready,
- app pool names change,
- environment variable requirements change,
- tests/build commands change,
- or deployment paths change.

When any service structure changes, remind the owner before implementation because local IIS may need manual updates.

IIS-impacting changes include:

- adding or removing a backend service,
- merging `HabitCompletionService` into `HabitService`,
- changing service ports,
- changing IIS app pool names,
- changing deployed folder paths,
- changing ApiGateway route destinations,
- adding frontend static deployment,
- or changing required environment variables.

Before a CI/CD update step starts, explicitly call out:

1. which services are changing,
2. which IIS app pools/sites may need changes,
3. which deployed folders may need changes,
4. which gateway routes may need changes,
5. and whether `.github/workflows/deploy.yml` must be edited.

## Current Gaps

### Frontend Is Not Built Or Deployed

The workflow currently builds, tests, publishes, and deploys backend services only.

It does not:

- run `npm install`,
- build shell or MFEs,
- upload frontend artifacts,
- deploy static frontend files to IIS,
- deploy MFE static bundles,
- or verify frontend routes.

This is fine for the current backend-focused pipeline, but the frontend rewrite will need pipeline support before production deployment.

### HabitCompletionService Is In The Pipeline

The backend rewrite plan recommends merging completions into HabitService.

If that decision is implemented, the pipeline must eventually remove:

- HabitCompletionService build step,
- HabitCompletionService test step,
- HabitCompletionService publish step,
- HabitCompletionService deploy step,
- HabitCompletionService app pool stop/start commands.

Do not remove those until the service is actually removed from the solution and IIS.

Because the local app is disposable, removing the old IIS app pool/site/folder is acceptable once the rewritten service layout is ready.

### No Database Migration Step

The workflow does not apply EF migrations.

That may be intentional and safer for local IIS deployment. For the rewrite, decide one of:

1. Apply migrations manually during controlled deployment.
2. Add a separate manual workflow for migrations.
3. Add an automated migration step after tests and before app restart.

Recommendation:

- Do not auto-run migrations in the first rewrite.
- Use manual migration until schema stabilizes.
- Reset/delete the local database manually when applying the new schema.

### No Frontend Tests

The workflow does not run frontend tests.

When the new frontend exists, add:

- TypeScript check.
- Unit/component tests.
- Frontend build.
- Later, smoke tests against deployed IIS/gateway.

## Expected Pipeline Adjustments After Backend Rewrite

Only after backend rewrite is implemented:

1. Remove or disable HabitCompletionService steps if service is merged.
2. Update solution/test paths if test projects are renamed.
3. Ensure AuthService and HabitService build with new migrations.
4. Ensure ApiGateway routes match new service layout.
5. Keep `.env` generation unless environment configuration changes.
6. Keep IIS app pool deployment style unless hosting changes.

## Expected Pipeline Adjustments After Frontend Rewrite

Only after frontend rewrite is implemented:

1. Add Node setup or rely on self-hosted runner Node installation.
2. Install frontend dependencies.
3. Build shell and MFEs.
4. Package static frontend artifacts.
5. Deploy static files to an IIS frontend site.
6. Preserve `src/frontend/web.config` SPA rewrite behavior or replace it deliberately.

Possible frontend IIS layout:

```text
C:\inetpub\HabitTracker\Frontend\
  index.html
  assets\
  mfe-auth\
  mfe-habits\
  mfe-admin\
  mfe-competition\
  web.config
```

The final layout depends on the MFE build strategy.

## Recommended CI/CD Policy For Rewrite

During rewrite:

- Keep the current workflow intact.
- Add docs and implementation first.
- Use local/manual verification before changing deployment.
- Update workflow in a dedicated CI/CD slice.
- Keep CI/CD changes small and reviewable.
- Remind the owner before the CI/CD slice begins that local IIS may need manual updates.

Suggested CI/CD slice:

1. Update backend service list.
2. Run workflow on a test branch or controlled push.
3. Add frontend build/deploy after frontend structure is stable.
4. Add smoke tests last.

## Do Not Do Yet

Do not yet:

- delete deploy workflow,
- rename app pools,
- remove HabitCompletionService deployment steps,
- add frontend deployment steps,
- add automatic database migrations,
- change secrets,
- or change IIS paths.

Those are implementation decisions for later slices.

Disposable local app note:

- It is acceptable later to remove old local IIS resources that belong only to the current prototype.
- Still pause before changing IIS/CI-CD so the app pool names, folders, and gateway routes are updated intentionally.

## Reminder For Later

When we reach the CI/CD update step, pause and ask:

- Has `HabitCompletionService` actually been removed or merged?
- Does local IIS still have a `HabitTracker-HabitCompletionService` app pool?
- Does ApiGateway still route completions to the old service?
- Where should frontend static files be deployed?
- Should database migrations remain manual?

Do not update the workflow until those answers are known.
