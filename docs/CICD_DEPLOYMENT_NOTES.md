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
- ApiGateway

Current deployed IIS app pools:

- `HabitTracker-AuthService`
- `HabitService`
- `HabitTracker-ApiGateway`
- `HabitTracker-UI`

Current deployment paths:

- `C:\inetpub\HabitTracker\AuthService`
- `C:\inetpub\HabitTracker\HabitService`
- `C:\inetpub\HabitTracker\ApiGateway`

Secrets used:

- `HABITTRACKER_API_URL`
- `HABITTRACKER_DB_NAME`
- `HABITTRACKER_DB_PASSWORD`
- `HABITTRACKER_JWT_SECRET`

Database target rule:

- Local development defaults to `HabitTracker_Dev` when `ASPNETCORE_ENVIRONMENT=Development`.
- Deployed/non-development environments must provide `HABITTRACKER_DB_NAME`.
- Production deployment should set `HABITTRACKER_DB_NAME` to the production database, for example `HabitTracker_Prod`.
- AuthService and HabitService both use the same database name variable so their schemas stay in the same SQL Server database.

Frontend target rule:

- Existing IIS site: `HabitTracker-UI`.
- Existing binding shown in IIS: `http://D13BG704:8080`.
- Existing deployed physical path used by CI/CD: `C:\inetpub\HabitTracker\Frontend`.
- Set `HABITTRACKER_API_URL` to the deployed ApiGateway origin, for example `http://D13BG704:5000`.
- ApiGateway CORS allows `http://D13BG704:8080` and localhost dev origins.

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

### Frontend Is Built And Deployed

The workflow builds and deploys the React frontend to the existing `HabitTracker-UI` IIS site.

It now:

- runs `npm ci`,
- builds the React shell,
- builds `src/frontend`,
- uploads frontend static artifacts,
- deploys `publish\Frontend` to `C:\inetpub\HabitTracker\Frontend`,
- and preserves the frontend `web.config` SPA fallback.

Manual smoke testing is still required after deployment.

### HabitCompletionService Was Removed From The Pipeline

Completions are merged into HabitService.

Current route status:

- ApiGateway now routes `/api/completions/{**catch-all}` to HabitService on `http://localhost:5110`.
- ApiGateway now routes `/api/competition/{**catch-all}` to HabitService on `http://localhost:5110`.
- ApiGateway now routes `/api/admin/{**catch-all}` to AuthService on `http://localhost:5039`.
- The old `/api/habit-completions/{**catch-all}` gateway route has been removed from ApiGateway config.
- CI/CD no longer builds, tests, publishes, or deploys HabitCompletionService.
- Local IIS may still have the old `HabitTracker-HabitCompletionService` app pool/site/folder. It can be removed manually from IIS because ApiGateway no longer routes to it.

Removed from pipeline:

- HabitCompletionService build step,
- HabitCompletionService test step,
- HabitCompletionService publish step,
- HabitCompletionService deploy step,
- HabitCompletionService app pool stop/start commands.

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

1. Keep Node available on the self-hosted runner.
2. Keep `HABITTRACKER_API_URL` aligned with the ApiGateway IIS binding.
3. Deploy static files to the existing IIS frontend site.
4. Preserve `src/frontend/web.config` SPA rewrite behavior.

Possible frontend IIS layout:

```text
C:\inetpub\HabitTracker\Frontend\
  index.html
  assets\
  web.config
```

The rewrite currently deploys one React shell build. If MFE remotes are reintroduced later, place their static outputs under this same IIS frontend root unless the IIS site layout is deliberately changed.

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
- add automatic database migrations,
- change secrets,
- or change IIS paths without first confirming the live IIS physical path.

HabitCompletionService removal and frontend static deployment have already been applied to the workflow because the rewrite merged completions into HabitService and the frontend has been rebuilt.

Disposable local app note:

- It is acceptable later to remove old local IIS resources that belong only to the current prototype.
- Still pause before changing IIS/CI-CD so the app pool names, folders, and gateway routes are updated intentionally.

## Reminder For Later

When we reach the CI/CD update step, pause and ask:

- Has `HabitCompletionService` stayed removed from IIS?
- Does ApiGateway still route completions to HabitService?
- Should frontend static files still deploy to `C:\inetpub\HabitTracker\Frontend`?
- Should database migrations remain manual?

Do not change the workflow again until those answers are known.
